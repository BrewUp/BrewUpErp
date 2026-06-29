import type { ModuleConfig, ModuleId } from '@shared/modules'

interface AppSidebarProps {
  modules: ModuleConfig[]
  activeModule: ModuleId
  collapsed: boolean
  onModuleSelect: (id: ModuleId) => void
  onToggleCollapse: () => void
}

export function AppSidebar({
  modules,
  activeModule,
  collapsed,
  onModuleSelect,
  onToggleCollapse,
}: AppSidebarProps) {
  function handleKeyDown(e: React.KeyboardEvent<HTMLUListElement>) {
    const items = Array.from(
      e.currentTarget.querySelectorAll<HTMLButtonElement>('button.sidebar-item__button'),
    )
    const idx = items.indexOf(document.activeElement as HTMLButtonElement)
    if (idx === -1) return

    if (e.key === 'ArrowDown') {
      e.preventDefault()
      items[(idx + 1) % items.length].focus()
    } else if (e.key === 'ArrowUp') {
      e.preventDefault()
      items[(idx - 1 + items.length) % items.length].focus()
    }
  }

  return (
    <nav
      aria-label="Main navigation"
      className={`sidebar${collapsed ? ' sidebar--collapsed' : ''}`}
    >
      <ul role="list" className="sidebar__list" onKeyDown={handleKeyDown}>
        {modules.map((m) => {
          const isActive = activeModule === m.id
          return (
            <li key={m.id} className="sidebar-item">
              <button
                type="button"
                className={`sidebar-item__button${isActive ? ' sidebar-item__button--active' : ''}`}
                aria-current={isActive ? 'page' : undefined}
                aria-label={m.label}
                title={collapsed ? m.label : undefined}
                onClick={() => {
                  if (!isActive) onModuleSelect(m.id)
                }}
              >
                <span className="sidebar-item__icon">
                  <m.icon />
                </span>
                <span
                  className={`sidebar-item__label${collapsed ? ' sidebar-item__label--hidden' : ''}`}
                  aria-hidden={collapsed ? true : undefined}
                >
                  {m.label}
                </span>
              </button>
            </li>
          )
        })}
      </ul>

      <div className="sidebar__footer">
        <button
          type="button"
          className="sidebar-toggle"
          aria-label={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
          onClick={onToggleCollapse}
        >
          <svg
            aria-hidden="true"
            width="16"
            height="16"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
            style={{ transform: collapsed ? 'rotate(180deg)' : 'none', transition: 'transform 250ms' }}
          >
            <polyline points="15 18 9 12 15 6" />
          </svg>
        </button>
      </div>
    </nav>
  )
}
