import { describe, it, expect } from 'vitest'
import type { ChatState, ChatAction, ChatMessage } from '@features/chat/types/chat.types'

// The reducer under test — will be exported from useChat.ts (T023)
// Import path matches the implementation target
import { chatReducer, initialState } from '@features/chat/hooks/useChat'

function makeMessage(overrides: Partial<ChatMessage> = {}): ChatMessage {
  return {
    id: 'test-id',
    role: 'user',
    content: 'test message',
    timestamp: new Date(),
    ...overrides,
  }
}

describe('chatReducer', () => {
  describe('USER_SENT', () => {
    it('appends a user message with correct role and content', () => {
      const action: ChatAction = { type: 'USER_SENT', payload: { message: 'Hello AI' } }
      const next: ChatState = chatReducer(initialState, action)

      expect(next.messages).toHaveLength(1)
      expect(next.messages[0].role).toBe('user')
      expect(next.messages[0].content).toBe('Hello AI')
    })

    it('sets isLoading to true', () => {
      const action: ChatAction = { type: 'USER_SENT', payload: { message: 'Hi' } }
      const next = chatReducer(initialState, action)
      expect(next.isLoading).toBe(true)
    })

    it('clears any previous error', () => {
      const state: ChatState = { ...initialState, error: 'previous error' }
      const action: ChatAction = { type: 'USER_SENT', payload: { message: 'retry' } }
      const next = chatReducer(state, action)
      expect(next.error).toBeNull()
    })

    it('generates a unique id for the message', () => {
      const action: ChatAction = { type: 'USER_SENT', payload: { message: 'A' } }
      const s1 = chatReducer(initialState, action)
      const s2 = chatReducer(initialState, action)
      expect(s1.messages[0].id).toBeTruthy()
      expect(s1.messages[0].id).not.toBe(s2.messages[0].id)
    })
  })

  describe('ASSISTANT_REPLIED', () => {
    it('appends an assistant message with correct role and content', () => {
      const state: ChatState = {
        ...initialState,
        messages: [makeMessage()],
        isLoading: true,
      }
      const action: ChatAction = {
        type: 'ASSISTANT_REPLIED',
        payload: { answer: 'I am the AI', conversationId: 'conv-123' },
      }
      const next = chatReducer(state, action)

      expect(next.messages).toHaveLength(2)
      expect(next.messages[1].role).toBe('assistant')
      expect(next.messages[1].content).toBe('I am the AI')
    })

    it('updates conversationId from payload', () => {
      const state: ChatState = { ...initialState, isLoading: true }
      const action: ChatAction = {
        type: 'ASSISTANT_REPLIED',
        payload: { answer: 'Reply', conversationId: 'conv-abc' },
      }
      const next = chatReducer(state, action)
      expect(next.conversationId).toBe('conv-abc')
    })

    it('retains previous conversationId when payload conversationId is null', () => {
      const state: ChatState = { ...initialState, conversationId: 'prev-id', isLoading: true }
      const action: ChatAction = {
        type: 'ASSISTANT_REPLIED',
        payload: { answer: 'Reply', conversationId: null },
      }
      const next = chatReducer(state, action)
      expect(next.conversationId).toBe('prev-id')
    })

    it('sets isLoading to false', () => {
      const state: ChatState = { ...initialState, isLoading: true }
      const action: ChatAction = {
        type: 'ASSISTANT_REPLIED',
        payload: { answer: 'Done', conversationId: null },
      }
      const next = chatReducer(state, action)
      expect(next.isLoading).toBe(false)
    })
  })

  describe('API_ERROR', () => {
    it('appends an error message with role error', () => {
      const state: ChatState = { ...initialState, isLoading: true }
      const action: ChatAction = { type: 'API_ERROR', payload: { error: 'Timeout' } }
      const next = chatReducer(state, action)

      expect(next.messages).toHaveLength(1)
      expect(next.messages[0].role).toBe('error')
      expect(next.messages[0].content).toBe('Timeout')
    })

    it('sets isLoading to false', () => {
      const state: ChatState = { ...initialState, isLoading: true }
      const action: ChatAction = { type: 'API_ERROR', payload: { error: 'err' } }
      const next = chatReducer(state, action)
      expect(next.isLoading).toBe(false)
    })

    it('stores the error in state.error', () => {
      const state: ChatState = { ...initialState, isLoading: true }
      const action: ChatAction = { type: 'API_ERROR', payload: { error: 'Network error' } }
      const next = chatReducer(state, action)
      expect(next.error).toBe('Network error')
    })
  })

  describe('RESET', () => {
    it('returns to initial state', () => {
      const state: ChatState = {
        messages: [makeMessage()],
        isLoading: false,
        error: 'some error',
        conversationId: 'conv-x',
      }
      const action: ChatAction = { type: 'RESET' }
      const next = chatReducer(state, action)
      expect(next).toEqual(initialState)
    })
  })
})
