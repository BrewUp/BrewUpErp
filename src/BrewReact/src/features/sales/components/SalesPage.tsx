import { useState } from 'react'
import { SalesOrdersTable } from './SalesOrdersTable'
import { CreateOrderForm } from './CreateOrderForm'

type SalesView = 'list' | 'create'

function SalesPage() {
  const [view, setView] = useState<SalesView>('list')

  return (
    <div className="sales-page">
      {view === 'list' ? (
        <SalesOrdersTable onNewOrder={() => setView('create')} />
      ) : (
        <CreateOrderForm
          onSuccess={() => setView('list')}
          onCancel={() => setView('list')}
        />
      )}
    </div>
  )
}

export default SalesPage
