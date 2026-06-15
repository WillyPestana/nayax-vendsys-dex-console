import { createContext, useContext } from 'react'

export type AuthState = {
  username: string
  authorizationHeader: string
}

export type AuthContextValue = {
  isAuthenticated: boolean
  username: string | null
  authorizationHeader: string | null
  login: (username: string, password: string) => Promise<void>
  logout: () => void
}

export const AuthContext = createContext<AuthContextValue | null>(null)

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used inside AuthProvider.')
  }

  return context
}
