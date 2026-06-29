import { useState, useEffect } from 'react'
import { getBeers } from '@features/sales/services/salesApiService'
import type { Beer } from '../types/sales.types'

export interface UseBeersResult {
  beers: Beer[]
  isLoading: boolean
  error: string | null
}

export function useBeers(): UseBeersResult {
  const [beers, setBeers] = useState<Beer[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let mounted = true
    setIsLoading(true)
    setError(null)

    getBeers()
      .then((result) => {
        if (!mounted) return
        setBeers(result.results)
      })
      .catch((err: Error) => {
        if (!mounted) return
        setError(err.message)
        setBeers([])
      })
      .finally(() => {
        if (mounted) setIsLoading(false)
      })

    return () => { mounted = false }
  }, [])

  return { beers, isLoading, error }
}
