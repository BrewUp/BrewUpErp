import { describe, it, expect } from 'vitest'
import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { http, HttpResponse } from 'msw'
import { server } from '../../../src/mocks/server'
import ChatPage from '@features/chat/components/ChatPage'
import type { ChatResponse } from '@features/chat/types/chat.types'

const BASE_URL = 'http://localhost:6094'

describe('ChatPage integration', () => {
  it('renders with the Chat tab active by default', () => {
    render(<ChatPage />)
    const chatTab = screen.getByRole('tab', { name: /chat/i })
    expect(chatTab).toHaveAttribute('aria-selected', 'true')
  })

  it('sends a message and appends both user and assistant messages', async () => {
    const user = userEvent.setup()
    render(<ChatPage />)

    await user.type(screen.getByRole('textbox'), 'What beers do you have?')
    await user.click(screen.getByRole('button', { name: /invia/i }))

    // User message appears
    await waitFor(() => {
      expect(screen.getByText('What beers do you have?')).toBeInTheDocument()
    })

    // Assistant reply appears (default handler echoes the message)
    await waitFor(() => {
      expect(screen.getByText(/echo.*what beers/i)).toBeInTheDocument()
    })
  })

  it('includes conversationId from first response in the follow-up request', async () => {
    const user = userEvent.setup()
    let capturedConversationId: string | null | undefined = undefined

    // Override handler for second call to capture request body
    let callCount = 0
    server.use(
      http.post(`${BASE_URL}/chat/`, async ({ request }) => {
        callCount++
        const body = await request.json() as { message: string; conversationId?: string | null }
        if (callCount === 2) {
          capturedConversationId = body.conversationId
        }
        return HttpResponse.json<ChatResponse>({
          answer: `Reply #${callCount}`,
          conversationId: 'session-conv-id',
        })
      }),
    )

    render(<ChatPage />)

    // First message
    await user.type(screen.getByRole('textbox'), 'First question')
    await user.click(screen.getByRole('button', { name: /invia/i }))
    await waitFor(() => screen.getByText('Reply #1'))

    // Second message — should send conversationId from first response
    await user.type(screen.getByRole('textbox'), 'Follow-up')
    await user.click(screen.getByRole('button', { name: /invia/i }))
    await waitFor(() => screen.getByText('Reply #2'))

    expect(capturedConversationId).toBe('session-conv-id')
  })

  it('switches to Beer Catalog tab and renders the table', async () => {
    const user = userEvent.setup()
    render(<ChatPage />)

    const catalogTab = screen.getByRole('tab', { name: /catalogo birre/i })
    await user.click(catalogTab)

    await waitFor(() => {
      expect(screen.getByText('Pale Ale')).toBeInTheDocument()
    })
  })

  it('appends an error message and keeps input enabled when API fails', async () => {
    const user = userEvent.setup()

    server.use(
      http.post(`${BASE_URL}/chat/`, () => {
        return HttpResponse.json(
          { detail: 'AI is down' },
          { status: 500 },
        )
      }),
    )

    render(<ChatPage />)

    await user.type(screen.getByRole('textbox'), 'Trigger error')
    await user.click(screen.getByRole('button', { name: /invia/i }))

    await waitFor(() => {
      expect(screen.getByText('AI is down')).toBeInTheDocument()
    })

    // Input must be re-enabled after error
    expect(screen.getByRole('textbox')).not.toBeDisabled()
  })

  it('shows loading state while waiting for API response', async () => {
    const user = userEvent.setup()
    let resolveRequest!: (value: Response) => void

    server.use(
      http.post(`${BASE_URL}/chat/`, () => {
        return new Promise<Response>((resolve) => {
          resolveRequest = resolve as (value: Response) => void
        })
      }),
    )

    render(<ChatPage />)
    await user.type(screen.getByRole('textbox'), 'Slow request')
    await user.click(screen.getByRole('button', { name: /invia/i }))

    // Loading indicator should appear while request is pending
    await waitFor(() => {
      expect(screen.getByLabelText('Caricamento risposta')).toBeInTheDocument()
    })

    // Clean up: resolve the hanging request
    resolveRequest(HttpResponse.json({ answer: 'done', conversationId: null }) as unknown as Response)
  })
})
