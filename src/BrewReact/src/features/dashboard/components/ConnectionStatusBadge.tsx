import type { HubConnectionState } from '../types/dashboard.types'

interface ConnectionStatusBadgeProps {
  hubState: HubConnectionState
}

export function ConnectionStatusBadge({ hubState }: ConnectionStatusBadgeProps) {
  if (hubState === 'idle') return null

  const label = hubState === 'connected' ? 'Live' : hubState === 'reconnecting' ? 'Reconnecting…' : 'Offline'

  return (
    <span role="status" className={`connection-badge connection-badge--${hubState}`}>
      {label}
    </span>
  )
}
