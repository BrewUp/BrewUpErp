import { useState, useEffect } from 'react'
import { getCustomers } from '@features/sales/services/salesApiService'
import type { Customer } from '../types/sales.types'

export interface UseCustomersResult {
  customers: Customer[]
  isLoading: boolean
  error: string | null
}

export function useCustomers(): UseCustomersResult {
  const [customers, setCustomers] = useState<Customer[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let mounted = true
    setIsLoading(true)
    setError(null)

    getCustomers()
      .then((result) => {
        if (!mounted) return
        setCustomers(result.results)
      })
      .catch((err: Error) => {
        if (!mounted) return
        setError(err.message)
        setCustomers([])
      })
      .finally(() => {
        if (mounted) setIsLoading(false)
      })

    return () => { mounted = false }
  }, [])

  return { customers, isLoading, error }
}
