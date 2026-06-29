import { describe, it, expect, afterEach } from 'vitest'
import { http, HttpResponse } from 'msw'
import { server } from '../../../src/mocks/server'
import { sendMessage, getBeerCatalog } from '@features/chat/services/chatApiService'
import type { ChatRequest, ChatResponse, BeerCatalogItem } from '@features/chat/types/chat.types'

const BASE_URL = 'http://localhost:6094'

describe('chatApiService', () => {
  describe('sendMessage()', () => {
    it('returns ChatResponse on success', async () => {
      const req: ChatRequest = { message: 'What beers do you have?' }
      const response = await sendMessage(req)

      expect(response.answer).toContain('Echo')
      expect(response.conversationId).toBe('mock-conversation-id')
    })

    it('passes conversationId in request body when provided', async () => {
      let capturedBody: ChatRequest | null = null

      server.use(
        http.post(`${BASE_URL}/chat/`, async ({ request }) => {
          capturedBody = await request.json() as ChatRequest
          return HttpResponse.json<ChatResponse>({
            answer: 'ok',
            conversationId: 'conv-123',
          })
        }),
      )

      await sendMessage({ message: 'Follow-up', conversationId: 'conv-123' })
      expect(capturedBody?.conversationId).toBe('conv-123')
    })

    it('throws a mapped error string on API 500', async () => {
      server.use(
        http.post(`${BASE_URL}/chat/`, () => {
          return HttpResponse.json(
            { title: 'Internal Server Error', detail: 'AI service unavailable' },
            { status: 500 },
          )
        }),
      )

      await expect(sendMessage({ message: 'boom' })).rejects.toThrow('AI service unavailable')
    })

    it('falls back to title when detail is missing', async () => {
      server.use(
        http.post(`${BASE_URL}/chat/`, () => {
          return HttpResponse.json(
            { title: 'Bad Gateway' },
            { status: 500 },
          )
        }),
      )

      await expect(sendMessage({ message: 'boom' })).rejects.toThrow('Bad Gateway')
    })

    it('falls back to generic message when both detail and title are missing', async () => {
      server.use(
        http.post(`${BASE_URL}/chat/`, () => {
          return HttpResponse.json({}, { status: 500 })
        }),
      )

      await expect(sendMessage({ message: 'boom' })).rejects.toThrow('Errore di comunicazione')
    })
  })

  describe('getBeerCatalog()', () => {
    it('returns an array of BeerCatalogItem on success', async () => {
      const result = await getBeerCatalog()

      expect(Array.isArray(result)).toBe(true)
      expect(result.length).toBeGreaterThan(0)

      const first = result[0] as BeerCatalogItem
      expect(first.name).toBe('Pale Ale')
      expect(first.isActive).toBe(true)
    })

    it('includes items with null alcoholByVolume', async () => {
      const result = await getBeerCatalog()
      const stout = result.find((b) => b.name === 'Stout')
      expect(stout?.alcoholByVolume).toBeNull()
    })

    it('throws a mapped error string on API 500', async () => {
      server.use(
        http.get(`${BASE_URL}/chat/beers`, () => {
          return HttpResponse.json(
            { title: 'Service Error', detail: 'Catalog unavailable' },
            { status: 500 },
          )
        }),
      )

      await expect(getBeerCatalog()).rejects.toThrow('Catalog unavailable')
    })
  })

  afterEach(() => {
    // server.resetHandlers() is called by setupTests.ts — no duplication needed
  })
})
