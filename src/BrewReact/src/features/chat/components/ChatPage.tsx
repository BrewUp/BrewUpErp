import { useState } from 'react'
import { useChat } from '../hooks/useChat'
import ChatThread from './ChatThread'
import ChatInput from './ChatInput'
import BeerCatalog from './BeerCatalog'

type TabKey = 'chat' | 'catalog'

function ChatPage() {
  const [activeTab, setActiveTab] = useState<TabKey>('chat')
  const { state, sendMessage } = useChat()

  return (
    <div className="chat-page">
      <div role="tablist" aria-label="Chat navigation">
        <button
          role="tab"
          aria-selected={activeTab === 'chat'}
          onClick={() => setActiveTab('chat')}
        >
          Chat
        </button>
        <button
          role="tab"
          aria-selected={activeTab === 'catalog'}
          onClick={() => setActiveTab('catalog')}
        >
          Catalogo Birre
        </button>
      </div>

      <div role="tabpanel">
        {activeTab === 'chat' && (
          <div className="chat-page__panel">
            <ChatThread messages={state.messages} isLoading={state.isLoading} />
            <ChatInput onSend={sendMessage} isLoading={state.isLoading} />
          </div>
        )}
        {activeTab === 'catalog' && <BeerCatalog />}
      </div>
    </div>
  )
}

export default ChatPage
