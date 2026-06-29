import { useState } from 'react'

interface Props {
  onSend: (text: string) => void
  isLoading: boolean
}

function ChatInput({ onSend, isLoading }: Props) {
  const [text, setText] = useState('')

  const trimmed = text.trim()
  const canSend = trimmed.length > 0 && !isLoading

  function handleSubmit() {
    if (!canSend) return
    onSend(trimmed)
    setText('')
  }

  function handleKeyDown(e: React.KeyboardEvent<HTMLInputElement>) {
    if (e.key === 'Enter') {
      handleSubmit()
    }
  }

  return (
    <div className="chat-input">
      <input
        type="text"
        value={text}
        onChange={(e) => setText(e.target.value)}
        onKeyDown={handleKeyDown}
        disabled={isLoading}
        placeholder="Scrivi un messaggio…"
        aria-label="Messaggio"
      />
      <button
        type="button"
        onClick={handleSubmit}
        disabled={!canSend}
      >
        Invia
      </button>
    </div>
  )
}

export default ChatInput
