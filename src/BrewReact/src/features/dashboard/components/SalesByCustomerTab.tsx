import { useSalesByCustomer } from '../hooks/useSalesByCustomer'

interface SalesByCustomerTabProps {
  refreshToken: number
}

const fmt = new Intl.NumberFormat('en-US', { maximumFractionDigits: 2, minimumFractionDigits: 2 })

export function SalesByCustomerTab({ refreshToken }: SalesByCustomerTabProps) {
  const { data, page, totalRecords, pageSize, isLoading, error, goToNextPage, goToPrevPage } =
    useSalesByCustomer(refreshToken)

  const hasNextPage = page * pageSize < totalRecords
  const hasPrevPage = page > 1

  if (isLoading) {
    return <div className="dashboard-loading">Loading…</div>
  }

  if (error) {
    return <div className="dashboard-error">{error}</div>
  }

  if (data.length === 0) {
    return <div className="dashboard-empty">No data available.</div>
  }

  return (
    <div>
      <table className="dashboard-table" aria-label="Sales by Customer">
        <thead>
          <tr>
            <th>Customer Name</th>
            <th>Year</th>
            <th>Total Sales</th>
            <th>Currency</th>
          </tr>
        </thead>
        <tbody>
          {data.map((row) => (
            <tr key={row.customerId}>
              <td>{row.customerName}</td>
              <td>{row.year}</td>
              <td>{fmt.format(row.totalSales)}</td>
              <td>{row.currency}</td>
            </tr>
          ))}
        </tbody>
      </table>
      <div className="pagination">
        <button aria-label="Previous page" disabled={!hasPrevPage} onClick={goToPrevPage}>
          Previous
        </button>
        <span>Page {page}</span>
        <button aria-label="Next page" disabled={!hasNextPage} onClick={goToNextPage}>
          Next
        </button>
      </div>
    </div>
  )
}
