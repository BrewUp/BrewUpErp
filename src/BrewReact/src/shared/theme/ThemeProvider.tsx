import { createContext, useCallback, useContext, useEffect, useState } from 'react'

export type Theme = 'light' | 'dark'

export const ThemeValueContext = createContext<Theme | undefined>(undefined)
export const ThemeDispatchContext = createContext<(() => void) | undefined>(undefined)

function getInitialTheme(): Theme {
  const stored = localStorage.getItem('brewup-theme')
  if (stored === 'light' || stored === 'dark') return stored
  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
}

interface ThemeProviderProps {
  children: React.ReactNode
}

export function ThemeProvider({ children }: ThemeProviderProps) {
  const [theme, setTheme] = useState<Theme>(getInitialTheme)

  const toggleTheme = useCallback(() => {
    setTheme((prev) => (prev === 'light' ? 'dark' : 'light'))
  }, [])

  useEffect(() => {
    document.documentElement.dataset.theme = theme
    localStorage.setItem('brewup-theme', theme)
  }, [theme])

  return (
    <ThemeValueContext.Provider value={theme}>
      <ThemeDispatchContext.Provider value={toggleTheme}>
        {children}
      </ThemeDispatchContext.Provider>
    </ThemeValueContext.Provider>
  )
}

export function useThemeValue(): Theme {
  const ctx = useContext(ThemeValueContext)
  if (ctx === undefined) {
    throw new Error('useTheme must be used within a ThemeProvider')
  }
  return ctx
}

export function useThemeDispatch(): () => void {
  const ctx = useContext(ThemeDispatchContext)
  if (ctx === undefined) {
    throw new Error('useTheme must be used within a ThemeProvider')
  }
  return ctx
}
