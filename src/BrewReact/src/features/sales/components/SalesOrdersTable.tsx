import { useSalesOrders } from '../hooks/useSalesOrders'

interface SalesOrdersTableProps {
  onNewOrder: () => void
}

export function SalesOrdersTable({ onNewOrder }: SalesOrdersTableProps) {
  const { data, page, totalRecords, pageSize, isLoading, error, goToNextPage, goToPrevPage } = useSalesOrders()

  const hasPrev = page > 1
  const hasNext = page * pageSize < totalRecords

  return (
    <div className="sales-page">
      <div className="sales-header">
        <h2>Sales Orders</h2>
        <button type="button" onClick={onNewOrder}>
          New Order
        </button>
      </div>

      {isLoading && <p>Loading...</p>}
      {error && <p className="order-form__error">{error}</p>}
      {!isLoading && !error && data.length === 0 && (
        <p>No sales orders found.</p>
      )}
      {!isLoading && !error && data.length > 0 && (
        <>
          <table className="sales-table" aria-label="Sales Orders">
            <thead>
              <tr>
                <th>Order Number</th>
                <th>Customer</th>
                <th>Order Date</th>
                <th>Delivery Date</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {data.map((order) => (
                <tr key={order.id}>
                  <td>{order.orderNumber}</td>
                  <td>{order.customerName}</td>
                  <td>{order.orderDate}</td>
                  <td>{order.deliveryDate ?? '—'}</td>
                  <td>{order.status}</td>
                </tr>
              ))}
            </tbody>
          </table>
          <div className="sales-pagination">
            <button type="button" onClick={goToPrevPage} disabled={!hasPrev} aria-label="Previous">
              Previous
            </button>
            <span>Page {page}</span>
            <button type="button" onClick={goToNextPage} disabled={!hasNext} aria-label="Next">
              Next
            </button>
          </div>
        </>
      )}
    </div>
  )
}

export default SalesOrdersTable
