import { useRef, useEffect } from 'react'
import type { ChatMessage as ChatMessageType } from '../types/chat.types'
import ChatMessage from './ChatMessage'

interface Props {
  messages: ChatMessageType[]
  isLoading: boolean
}

function ChatThread({ messages, isLoading }: Props) {
  const bottomRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages.length, isLoading])

  return (
    <div className="chat-thread">
      {messages.length === 0 && !isLoading && (
        <p className="chat-thread__empty">Inizia la conversazione…</p>
      )}
      {messages.map((msg) => (
        <ChatMessage key={msg.id} message={msg} />
      ))}
      {isLoading && (
        <div
          className="chat-thread__spinner"
          aria-label="Caricamento risposta"
          role="status"
        />
      )}
      <div ref={bottomRef} aria-hidden="true" />
    </div>
  )
}

export default ChatThread
