// ── API contract types (mirror openapi.yaml) ───────────────────────────────

export interface BeerCatalogItem {
  beerId?: string
  beerName?: string
  beerStyle?: string
  alcoholByVolume?: number | null
  isActive?: boolean
}

export interface ChatRequest {
  /** Required. The user's message text. Must not be empty. */
  message: string
  /** Optional. Pass the conversationId from a previous ChatResponse to continue a session. */
  conversationId?: string | null
}

export interface ChatResponse {
  answer?: string
  conversationId?: string | null
}

// ── Client-side domain types (not serialised to API) ──────────────────────

export type MessageRole = 'user' | 'assistant' | 'error'

export interface ChatMessage {
  id: string
  role: MessageRole
  content: string
  timestamp: Date
}

export interface ChatState {
  messages: ChatMessage[]
  isLoading: boolean
  error: string | null
  conversationId: string | null
}

// ── Reducer action types ──────────────────────────────────────────────────

export type ChatAction =
  | { type: 'USER_SENT'; payload: { message: string } }
  | { type: 'ASSISTANT_REPLIED'; payload: { answer: string; conversationId: string | null } }
  | { type: 'API_ERROR'; payload: { error: string } }
  | { type: 'RESET' }
