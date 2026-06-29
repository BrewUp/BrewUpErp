import { useState, useCallback } from 'react'
import { SalesByCustomerTab } from './SalesByCustomerTab'
import { SalesByProductTab } from './SalesByProductTab'
import { ConnectionStatusBadge } from './ConnectionStatusBadge'
import { useDashboardHub } from '../hooks/useDashboardHub'

type TabId = 'customers' | 'products'

function DashboardPage() {
  const [activeTab, setActiveTab] = useState<TabId>('customers')
  const [customerRefreshToken, setCustomerRefreshToken] = useState(0)
  const [productRefreshToken, setProductRefreshToken] = useState(0)

  const onCustomerUpdated = useCallback(() => setCustomerRefreshToken((t) => t + 1), [])
  const onProductUpdated = useCallback(() => setProductRefreshToken((t) => t + 1), [])

  const hubState = useDashboardHub({ onCustomerUpdated, onProductUpdated })

  return (
    <div className="dashboard-page">
      <div className="dashboard-header">
        <h1>Dashboard</h1>
        <ConnectionStatusBadge hubState={hubState} />
      </div>
      <div role="tablist" aria-label="Dashboard tabs">
        <button
          role="tab"
          aria-selected={activeTab === 'customers'}
          onClick={() => setActiveTab('customers')}
        >
          Sales by Customer
        </button>
        <button
          role="tab"
          aria-selected={activeTab === 'products'}
          onClick={() => setActiveTab('products')}
        >
          Sales by Product
        </button>
      </div>
      <div role="tabpanel">
        {activeTab === 'customers' && <SalesByCustomerTab refreshToken={customerRefreshToken} />}
        {activeTab === 'products' && <SalesByProductTab refreshToken={productRefreshToken} />}
      </div>
    </div>
  )
}

export default DashboardPage
