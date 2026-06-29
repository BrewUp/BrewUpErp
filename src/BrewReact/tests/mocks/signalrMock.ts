import { vi } from 'vitest'

export function createMockHubConnection() {
  const handlers: Record<string, (...args: unknown[]) => void> = {}
  const reconnectingCallbacks: Array<(error?: Error) => void> = []
  const reconnectedCallbacks: Array<(connectionId?: string) => void> = []
  const closeCallbacks: Array<(error?: Error) => void> = []

  const conn = {
    start: vi.fn().mockResolvedValue(undefined),
    stop: vi.fn().mockResolvedValue(undefined),
    on: vi.fn((method: string, handler: (...args: unknown[]) => void) => {
      handlers[method] = handler
    }),
    off: vi.fn(),
    onreconnecting: vi.fn((cb: (error?: Error) => void) => {
      reconnectingCallbacks.push(cb)
    }),
    onreconnected: vi.fn((cb: (connectionId?: string) => void) => {
      reconnectedCallbacks.push(cb)
    }),
    onclose: vi.fn((cb: (error?: Error) => void) => {
      closeCallbacks.push(cb)
    }),
    state: 'Disconnected' as const,

    /** Helper: simulate a server push message */
    simulateMessage: (method: string, ...args: unknown[]) => {
      handlers[method]?.(...args)
    },
    /** Helper: simulate reconnecting event */
    simulateReconnecting: (error?: Error) => {
      reconnectingCallbacks.forEach((cb) => cb(error))
    },
    /** Helper: simulate reconnected event */
    simulateReconnected: (connectionId?: string) => {
      reconnectedCallbacks.forEach((cb) => cb(connectionId))
    },
    /** Helper: simulate close event */
    simulateClose: (error?: Error) => {
      closeCallbacks.forEach((cb) => cb(error))
    },
  }

  return conn
}

export type MockHubConnection = ReturnType<typeof createMockHubConnection>
