import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import ChatThread from '@features/chat/components/ChatThread'
import type { ChatMessage } from '@features/chat/types/chat.types'

function makeMsg(id: string, role: ChatMessage['role'], content: string): ChatMessage {
  return { id, role, content, timestamp: new Date() }
}

describe('ChatThread', () => {
  it('shows the empty-state placeholder when there are no messages', () => {
    render(<ChatThread messages={[]} isLoading={false} />)
    expect(screen.getByText(/inizia la conversazione/i)).toBeInTheDocument()
  })

  it('renders messages in the correct order', () => {
    const messages: ChatMessage[] = [
      makeMsg('1', 'user', 'First message'),
      makeMsg('2', 'assistant', 'Second message'),
      makeMsg('3', 'user', 'Third message'),
    ]
    render(<ChatThread messages={messages} isLoading={false} />)

    const items = screen.getAllByText(/message/i)
    expect(items[0]).toHaveTextContent('First message')
    expect(items[1]).toHaveTextContent('Second message')
    expect(items[2]).toHaveTextContent('Third message')
  })

  it('shows the loading spinner with aria-label when isLoading is true', () => {
    render(<ChatThread messages={[]} isLoading={true} />)
    expect(screen.getByLabelText('Caricamento risposta')).toBeInTheDocument()
  })

  it('does not show the loading spinner when isLoading is false', () => {
    render(<ChatThread messages={[]} isLoading={false} />)
    expect(screen.queryByLabelText('Caricamento risposta')).not.toBeInTheDocument()
  })

  it('does not show the empty placeholder when messages are present', () => {
    const messages: ChatMessage[] = [makeMsg('1', 'user', 'Hello')]
    render(<ChatThread messages={messages} isLoading={false} />)
    expect(screen.queryByText(/inizia la conversazione/i)).not.toBeInTheDocument()
  })
})
