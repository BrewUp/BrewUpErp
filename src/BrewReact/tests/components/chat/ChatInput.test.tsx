import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import ChatInput from '@features/chat/components/ChatInput'

describe('ChatInput', () => {
  it('renders an empty text input by default', () => {
    render(<ChatInput onSend={vi.fn()} isLoading={false} />)
    const input = screen.getByRole('textbox')
    expect(input).toHaveValue('')
  })

  it('has the Send button disabled when input is empty', () => {
    render(<ChatInput onSend={vi.fn()} isLoading={false} />)
    expect(screen.getByRole('button', { name: /invia/i })).toBeDisabled()
  })

  it('enables the Send button when the user types something', async () => {
    const user = userEvent.setup()
    render(<ChatInput onSend={vi.fn()} isLoading={false} />)

    await user.type(screen.getByRole('textbox'), 'Hello')
    expect(screen.getByRole('button', { name: /invia/i })).toBeEnabled()
  })

  it('calls onSend with trimmed text and clears the input when Send is clicked', async () => {
    const user = userEvent.setup()
    const onSend = vi.fn()
    render(<ChatInput onSend={onSend} isLoading={false} />)

    await user.type(screen.getByRole('textbox'), '  Hello AI  ')
    await user.click(screen.getByRole('button', { name: /invia/i }))

    expect(onSend).toHaveBeenCalledWith('Hello AI')
    expect(screen.getByRole('textbox')).toHaveValue('')
  })

  it('calls onSend when Enter is pressed', async () => {
    const user = userEvent.setup()
    const onSend = vi.fn()
    render(<ChatInput onSend={onSend} isLoading={false} />)

    await user.type(screen.getByRole('textbox'), 'Quick send{Enter}')
    expect(onSend).toHaveBeenCalledWith('Quick send')
  })

  it('disables input and button when isLoading is true', () => {
    render(<ChatInput onSend={vi.fn()} isLoading={true} />)
    expect(screen.getByRole('textbox')).toBeDisabled()
    expect(screen.getByRole('button', { name: /invia/i })).toBeDisabled()
  })

  it('does not call onSend when input contains only whitespace', async () => {
    const user = userEvent.setup()
    const onSend = vi.fn()
    render(<ChatInput onSend={onSend} isLoading={false} />)

    await user.type(screen.getByRole('textbox'), '   ')
    // Button should still be disabled for whitespace-only
    expect(screen.getByRole('button', { name: /invia/i })).toBeDisabled()
  })
})
