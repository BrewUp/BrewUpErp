import { useState, useEffect, useRef } from 'react'
import { HubConnectionBuilder } from '@microsoft/signalr'
import type { HubConnectionState } from '../types/dashboard.types'

interface UseDashboardHubOptions {
  onCustomerUpdated: () => void
  onProductUpdated: () => void
}

export function useDashboardHub({ onCustomerUpdated, onProductUpdated }: UseDashboardHubOptions): HubConnectionState {
  const [hubState, setHubState] = useState<HubConnectionState>('idle')
  const onCustomerRef = useRef(onCustomerUpdated)
  const onProductRef = useRef(onProductUpdated)

  useEffect(() => {
    onCustomerRef.current = onCustomerUpdated
  }, [onCustomerUpdated])

  useEffect(() => {
    onProductRef.current = onProductUpdated
  }, [onProductUpdated])

  useEffect(() => {
    let mounted = true
    const hubUrl = `${import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:6094'}/hubs/dashboards`

    const connection = new HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect()
      .build()

    connection.on('customersDashboardUpdated', () => {
      onCustomerRef.current()
    })

    connection.on('productsDashboardUpdated', () => {
      onProductRef.current()
    })

    connection.onreconnecting(() => {
      if (mounted) setHubState('reconnecting')
    })

    connection.onreconnected(() => {
      if (mounted) setHubState('connected')
    })

    connection.onclose(() => {
      if (mounted) setHubState('offline')
    })

    connection
      .start()
      .then(() => {
        if (mounted) setHubState('connected')
      })
      .catch(() => {
        if (mounted) setHubState('offline')
      })

    return () => {
      mounted = false
      connection.stop()
    }
  }, [])

  return hubState
}
