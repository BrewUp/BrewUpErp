import type { AxiosError } from 'axios'
import dashboardApiClient from '@shared/api/dashboardApiClient'
import type { PagedResult, SalesByCustomer, SalesByProduct } from '../types/dashboard.types'

interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
}

function mapError(error: unknown): string {
  const axiosError = error as AxiosError<ProblemDetails>
  if (axiosError.response?.data) {
    const { detail, title } = axiosError.response.data
    return detail ?? title ?? 'Errore di comunicazione'
  }
  return 'Errore di comunicazione'
}

export async function getSalesByCustomer(pageNumber: number): Promise<PagedResult<SalesByCustomer>> {
  try {
    const { data } = await dashboardApiClient.get<PagedResult<SalesByCustomer>>('/v1/dashboards/customers', {
      params: { pageNumber },
    })
    return data
  } catch (error) {
    throw new Error(mapError(error), { cause: error })
  }
}

export async function getSalesByProduct(pageNumber: number): Promise<PagedResult<SalesByProduct>> {
  try {
    const { data } = await dashboardApiClient.get<PagedResult<SalesByProduct>>('/v1/dashboards/products', {
      params: { pageNumber },
    })
    return data
  } catch (error) {
    throw new Error(mapError(error), { cause: error })
  }
}
