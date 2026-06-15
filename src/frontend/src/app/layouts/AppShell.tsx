import { Database, LogOut, Upload } from 'lucide-react'
import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../providers/AuthContext'
import { Button } from '../../shared/ui/Button/Button'

export function AppShell() {
  const { username, logout } = useAuth()

  return (
    <div className="app-shell">
      <aside className="sidebar" aria-label="Primary navigation">
        <div className="brand-block">
          <div className="brand-mark">NX</div>
          <div>
            <strong>Nayax VendSys</strong>
            <span>DEX Console</span>
          </div>
        </div>

        <nav className="nav-stack">
          <NavLink to="/upload" className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}>
            <Upload size={18} aria-hidden="true" />
            Upload DEX
          </NavLink>
          <NavLink to="/meters" className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}>
            <Database size={18} aria-hidden="true" />
            DEX Meters
          </NavLink>
        </nav>

        <div className="sidebar-footer">
          <div className="signed-in">
            <span>Signed in</span>
            <strong>{username}</strong>
          </div>
          <Button type="button" variant="ghost" icon={LogOut} onClick={logout}>
            Logout
          </Button>
        </div>
      </aside>

      <main className="main-surface">
        <Outlet />
      </main>
    </div>
  )
}
