import { useState, useEffect, useCallback } from 'react'
import { getSalesByProduct } from '@features/dashboard/services/dashboardApiService'
import type { SalesByProduct } from '../types/dashboard.types'

export interface UseSalesByProductResult {
  data: SalesByProduct[]
  page: number
  totalRecords: number
  pageSize: number
  isLoading: boolean
  error: string | null
  goToNextPage: () => void
  goToPrevPage: () => void
}

export function useSalesByProduct(refreshToken: number): UseSalesByProductResult {
  const [page, setPage] = useState(1)
  const [data, setData] = useState<SalesByProduct[]>([])
  const [totalRecords, setTotalRecords] = useState(0)
  const [pageSize, setPageSize] = useState(10)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let mounted = true
    setIsLoading(true)
    setError(null)

    getSalesByProduct(page)
      .then((result) => {
        if (!mounted) return
        setData(result.results)
        setTotalRecords(result.totalRecords)
        setPageSize(result.pageSize)
      })
      .catch((err: Error) => {
        if (!mounted) return
        setError(err.message)
        setData([])
      })
      .finally(() => {
        if (mounted) setIsLoading(false)
      })

    return () => { mounted = false }
  }, [page, refreshToken])

  const goToNextPage = useCallback(() => {
    setPage((p) => p + 1)
  }, [])

  const goToPrevPage = useCallback(() => {
    setPage((p) => Math.max(1, p - 1))
  }, [])

  return { data, page, totalRecords, pageSize, isLoading, error, goToNextPage, goToPrevPage }
}
