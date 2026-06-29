import { useState, useEffect, useCallback } from 'react'
import { getSalesOrders } from '@features/sales/services/salesApiService'
import type { SalesOrder } from '../types/sales.types'

export interface UseSalesOrdersResult {
  data: SalesOrder[]
  page: number
  totalRecords: number
  pageSize: number
  isLoading: boolean
  error: string | null
  goToNextPage: () => void
  goToPrevPage: () => void
}

export function useSalesOrders(refreshToken: number = 0): UseSalesOrdersResult {
  const [page, setPage] = useState(1)
  const [data, setData] = useState<SalesOrder[]>([])
  const [totalRecords, setTotalRecords] = useState(0)
  const [pageSize, setPageSize] = useState(10)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let mounted = true
    setIsLoading(true)
    setError(null)

    getSalesOrders(page)
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
