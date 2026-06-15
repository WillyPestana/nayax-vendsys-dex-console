import { useCallback, useMemo, useState, type PropsWithChildren } from 'react'
import { useNavigate } from 'react-router-dom'
import { authenticate } from '../../shared/api/authApi'
import { AuthContext, type AuthContextValue, type AuthState } from './AuthContext'

const StorageKey = 'nayax-vendsys-auth'

export function AuthProvider({ children }: PropsWithChildren) {
  const navigate = useNavigate()
  const [auth, setAuth] = useState<AuthState | null>(() => readStoredAuth())

  const login = useCallback(async (username: string, password: string) => {
    const response = await authenticate({ username, password })
    const nextAuth = {
      username: response.username,
      authorizationHeader: response.authorizationHeader,
    }

    sessionStorage.setItem(StorageKey, JSON.stringify(nextAuth))
    setAuth(nextAuth)
  }, [])

  const logout = useCallback(() => {
    sessionStorage.removeItem(StorageKey)
    setAuth(null)
    navigate('/login', { replace: true })
  }, [navigate])

  const value = useMemo<AuthContextValue>(
    () => ({
      isAuthenticated: Boolean(auth),
      username: auth?.username ?? null,
      authorizationHeader: auth?.authorizationHeader ?? null,
      login,
      logout,
    }),
    [auth, login, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

function readStoredAuth(): AuthState | null {
  const raw = sessionStorage.getItem(StorageKey)
  if (!raw) {
    return null
  }

  try {
    const parsed = JSON.parse(raw) as Partial<AuthState>
    if (parsed.username && parsed.authorizationHeader) {
      return {
        username: parsed.username,
        authorizationHeader: parsed.authorizationHeader,
      }
    }
  } catch {
    sessionStorage.removeItem(StorageKey)
  }

  return null
}
