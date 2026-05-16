import { useState } from 'react'
import { NavLink, useNavigate } from 'react-router-dom'
import { LayoutDashboard, Calendar, Users, LogOut, Scissors, ChevronLeft, ChevronRight, User2 } from 'lucide-react'
import { useAuth } from '../../hooks/useAuth'

const links = [
  { to: '/admin', label: 'Dashboard', Icon: LayoutDashboard, end: true },
  { to: '/admin/appointments', label: 'Agendamentos', Icon: Calendar, end: false },
  { to: '/admin/users', label: 'Usuários', Icon: Users, end: false },
  { to: '/admin/services', label: 'Serviços', Icon: Scissors, end: false },
  { to: '/admin/barbers', label: 'Barbeiros', Icon: User2, end: false },
]

export function Sidebar() {
  const { logout } = useAuth()
  const navigate = useNavigate()
  const [collapsed, setCollapsed] = useState(false)

  return (
    <aside className={`${collapsed ? 'w-16' : 'w-56'} bg-bg-surface border-r border-border flex flex-col min-h-screen transition-all duration-200 flex-shrink-0`}>
      <div className="px-3 py-4 border-b border-border flex items-center justify-between gap-2">
        <button
          onClick={() => navigate('/admin')}
          className="flex items-center gap-2 min-w-0 overflow-hidden"
        >
          <Scissors size={18} className="text-accent flex-shrink-0" />
          {!collapsed && (
            <span className="font-display font-bold text-base tracking-widest uppercase truncate">
              Barber<span className="text-accent">Agenda</span>
            </span>
          )}
        </button>
        <button
          onClick={() => setCollapsed((c) => !c)}
          className="text-text-secondary hover:text-text-primary flex-shrink-0"
        >
          {collapsed ? <ChevronRight size={14} /> : <ChevronLeft size={14} />}
        </button>
      </div>

      <nav className="flex-1 p-2 flex flex-col gap-1">
        {links.map(({ to, label, Icon, end }) => (
          <NavLink
            key={to}
            to={to}
            end={end}
            title={collapsed ? label : undefined}
            className={({ isActive }) =>
              `flex items-center gap-3 px-3 py-2.5 rounded-sm text-sm font-body transition-colors
              ${collapsed ? 'justify-center' : ''}
              ${isActive
                ? 'bg-accent/10 text-accent border-l-2 border-accent'
                : 'text-text-secondary hover:text-text-primary hover:bg-bg-elevated'
              }`
            }
          >
            <Icon size={16} strokeWidth={1.5} />
            {!collapsed && label}
          </NavLink>
        ))}
      </nav>

      <div className="p-2 border-t border-border">
        <button
          onClick={logout}
          title={collapsed ? 'Sair' : undefined}
          className={`flex items-center gap-3 px-3 py-2.5 w-full text-sm font-body text-text-secondary hover:text-red-400 transition-colors ${collapsed ? 'justify-center' : ''}`}
        >
          <LogOut size={16} strokeWidth={1.5} />
          {!collapsed && 'Sair'}
        </button>
      </div>
    </aside>
  )
}
