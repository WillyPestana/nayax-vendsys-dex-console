import { Navigate, Outlet, Route, Routes } from 'react-router-dom'
import { AppShell } from './layouts/AppShell'
import { useAuth } from './providers/AuthContext'
import { DexMetersPage } from '../pages/DexMetersPage/DexMetersPage'
import { DexUploadPage } from '../pages/DexUploadPage/DexUploadPage'
import { LoginPage } from '../pages/LoginPage/LoginPage'

export function AppRouter() {
  const { isAuthenticated } = useAuth()

  return (
    <Routes>
      <Route path="/login" element={isAuthenticated ? <Navigate to="/upload" replace /> : <LoginPage />} />
      <Route element={<ProtectedRoute />}>
        <Route element={<AppShell />}>
          <Route path="/upload" element={<DexUploadPage />} />
          <Route path="/meters" element={<DexMetersPage />} />
        </Route>
      </Route>
      <Route path="*" element={<Navigate to={isAuthenticated ? '/upload' : '/login'} replace />} />
    </Routes>
  )
}

function ProtectedRoute() {
  const { isAuthenticated } = useAuth()
  return isAuthenticated ? <Outlet /> : <Navigate to="/login" replace />
}
