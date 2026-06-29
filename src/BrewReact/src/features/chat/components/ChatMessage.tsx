import type { ChatMessage as ChatMessageType } from '../types/chat.types'

interface Props {
  message: ChatMessageType
}

function ChatMessage({ message }: Props) {
  return (
    <div className={`chat-message chat-message--${message.role}`}>
      <p>{message.content}</p>
    </div>
  )
}

export default ChatMessage
