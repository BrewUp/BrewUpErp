import type { ComponentType } from 'react'
import ChatPage from '@features/chat/components/ChatPage'
import DashboardPage from '@features/dashboard/components/DashboardPage'
import SalesPage from '@features/sales/components/SalesPage'
import WarehousePage from '@features/warehouse/components/WarehousePage'
import {
  ChatIcon,
  DashboardIcon,
  SalesIcon,
  WarehouseIcon,
} from '@shared/sidebar/icons'

export type ModuleId = 'chat' | 'dashboard' | 'sales' | 'warehouse'

export interface ModuleConfig {
  id: ModuleId
  label: string
  icon: ComponentType
  component: ComponentType
}

export const MODULES: ModuleConfig[] = [
  { id: 'chat', label: 'Chat', icon: ChatIcon, component: ChatPage },
  { id: 'dashboard', label: 'Dashboard', icon: DashboardIcon, component: DashboardPage },
  { id: 'sales', label: 'Sales', icon: SalesIcon, component: SalesPage },
  { id: 'warehouse', label: 'Warehouse', icon: WarehouseIcon, component: WarehousePage },
]
