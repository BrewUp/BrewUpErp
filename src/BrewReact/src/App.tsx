import { useState } from 'react'
import { ThemeProvider, ThemeToggle } from '@shared/theme'
import { AppSidebar } from '@shared/sidebar'
import { useSidebarCollapsed } from '@shared/sidebar'
import { MODULES } from '@shared/modules'
import type { ModuleId } from '@shared/modules'

function App() {
  const [activeModule, setActiveModule] = useState<ModuleId>('chat')
  const [collapsed, toggleCollapse] = useSidebarCollapsed()

  const ActiveComponent =
    MODULES.find((m) => m.id === activeModule)?.component ?? MODULES[0].component

  return (
    <ThemeProvider>
      <div id="app">
        <header className="app-header">
          <span className="app-header__brand">🍺 BrewUp</span>
          <ThemeToggle />
        </header>
        <div className="app-layout">
          <AppSidebar
            modules={MODULES}
            activeModule={activeModule}
            collapsed={collapsed}
            onModuleSelect={setActiveModule}
            onToggleCollapse={toggleCollapse}
          />
          <main className="app-layout__main">
            <ActiveComponent />
          </main>
        </div>
      </div>
    </ThemeProvider>
  )
}

export default App
