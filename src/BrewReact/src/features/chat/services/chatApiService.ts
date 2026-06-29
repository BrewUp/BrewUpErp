import type { AxiosError } from 'axios'
import chatApiClient from '@shared/api/chatApiClient'
import type { ChatRequest, ChatResponse, BeerCatalogItem } from '../types/chat.types'

interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  timestamp?: string
}

function mapError(error: unknown): string {
  const axiosError = error as AxiosError<ProblemDetails>
  if (axiosError.response?.data) {
    const { detail, title } = axiosError.response.data
    return detail ?? title ?? 'Errore di comunicazione'
  }
  return 'Errore di comunicazione'
}

export async function sendMessage(req: ChatRequest): Promise<ChatResponse> {
  try {
    const { data } = await chatApiClient.post<ChatResponse>('/chat/', req)
    return data
  } catch (error) {
    throw new Error(mapError(error), { cause: error })
  }
}

export async function getBeerCatalog(): Promise<BeerCatalogItem[]> {
  try {
    const { data } = await chatApiClient.get<BeerCatalogItem[]>('/v1/masterdata/beers')
    return data
  } catch (error) {
    throw new Error(mapError(error), { cause: error })
  }
}
