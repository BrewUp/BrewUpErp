import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import ChatMessage from '@features/chat/components/ChatMessage'
import type { ChatMessage as ChatMessageType } from '@features/chat/types/chat.types'

function makeMsg(overrides: Partial<ChatMessageType>): ChatMessageType {
  return {
    id: 'msg-1',
    role: 'user',
    content: 'Default content',
    timestamp: new Date('2026-05-28T10:00:00Z'),
    ...overrides,
  }
}

describe('ChatMessage', () => {
  it('renders a user message with the user role class', () => {
    const msg = makeMsg({ role: 'user', content: 'Hello there' })
    const { container } = render(<ChatMessage message={msg} />)
    expect(container.firstChild).toHaveClass('chat-message--user')
  })

  it('renders an assistant message with the assistant role class', () => {
    const msg = makeMsg({ role: 'assistant', content: 'Hi! How can I help?' })
    const { container } = render(<ChatMessage message={msg} />)
    expect(container.firstChild).toHaveClass('chat-message--assistant')
  })

  it('renders an error message with the error role class', () => {
    const msg = makeMsg({ role: 'error', content: 'Something went wrong' })
    const { container } = render(<ChatMessage message={msg} />)
    expect(container.firstChild).toHaveClass('chat-message--error')
  })

  it('displays the message content for user role', () => {
    const msg = makeMsg({ role: 'user', content: 'User content text' })
    render(<ChatMessage message={msg} />)
    expect(screen.getByText('User content text')).toBeInTheDocument()
  })

  it('displays the message content for assistant role', () => {
    const msg = makeMsg({ role: 'assistant', content: 'Assistant answer text' })
    render(<ChatMessage message={msg} />)
    expect(screen.getByText('Assistant answer text')).toBeInTheDocument()
  })

  it('displays the message content for error role', () => {
    const msg = makeMsg({ role: 'error', content: 'Error message text' })
    render(<ChatMessage message={msg} />)
    expect(screen.getByText('Error message text')).toBeInTheDocument()
  })
})
