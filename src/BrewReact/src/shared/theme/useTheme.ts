import { useThemeValue, useThemeDispatch } from './ThemeProvider'

export function useTheme() {
  return {
    theme: useThemeValue(),
    toggleTheme: useThemeDispatch(),
  }
}
