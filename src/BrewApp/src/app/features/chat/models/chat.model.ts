export interface BeerCatalogItem {
  beerId?: string;
  name?: string;
  style?: string;
  alcoholByVolume?: number;
  isActive?: boolean;
}

export interface ChatRequest {
  message: string;
  conversationId?: string;
}

export interface ChatResponse {
  answer?: string;
  conversationId?: string;
}

export interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
}
