import { useReducer, useCallback } from 'react'
import { sendMessage as apiSendMessage } from '../services/chatApiService'
import type { ChatState, ChatAction, ChatMessage } from '../types/chat.types'

export const initialState: ChatState = {
  messages: [],
  isLoading: false,
  error: null,
  conversationId: null,
}

function generateId(): string {
  return `msg-${Date.now()}-${Math.random().toString(36).slice(2, 9)}`
}

export function chatReducer(state: ChatState, action: ChatAction): ChatState {
  switch (action.type) {
    case 'USER_SENT': {
      const message: ChatMessage = {
        id: generateId(),
        role: 'user',
        content: action.payload.message,
        timestamp: new Date(),
      }
      return {
        ...state,
        messages: [...state.messages, message],
        isLoading: true,
        error: null,
      }
    }
    case 'ASSISTANT_REPLIED': {
      const message: ChatMessage = {
        id: generateId(),
        role: 'assistant',
        content: action.payload.answer,
        timestamp: new Date(),
      }
      return {
        ...state,
        messages: [...state.messages, message],
        isLoading: false,
        conversationId: action.payload.conversationId ?? state.conversationId,
      }
    }
    case 'API_ERROR': {
      const message: ChatMessage = {
        id: generateId(),
        role: 'error',
        content: action.payload.error,
        timestamp: new Date(),
      }
      return {
        ...state,
        messages: [...state.messages, message],
        isLoading: false,
        error: action.payload.error,
      }
    }
    case 'RESET':
      return initialState
    default:
      return state
  }
}

export interface UseChatReturn {
  state: ChatState
  sendMessage: (text: string) => Promise<void>
  reset: () => void
}

export function useChat(): UseChatReturn {
  const [state, dispatch] = useReducer(chatReducer, initialState)

  const sendMessage = useCallback(
    async (text: string) => {
      dispatch({ type: 'USER_SENT', payload: { message: text } })
      try {
        const response = await apiSendMessage({
          message: text,
          conversationId: state.conversationId,
        })
        dispatch({
          type: 'ASSISTANT_REPLIED',
          payload: {
            answer: response.answer ?? '',
            conversationId: response.conversationId ?? null,
          },
        })
      } catch (error) {
        const msg = error instanceof Error ? error.message : 'Errore di comunicazione'
        dispatch({ type: 'API_ERROR', payload: { error: msg } })
      }
    },
    [state.conversationId],
  )

  const reset = useCallback(() => {
    dispatch({ type: 'RESET' })
  }, [])

  return { state, sendMessage, reset }
}
