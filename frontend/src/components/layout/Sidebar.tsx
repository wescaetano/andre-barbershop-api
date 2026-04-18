import { NavLink } from 'react-router-dom'
import { LayoutDashboard, Calendar, Users, LogOut, Scissors } from 'lucide-react'
import { useAuth } from '../../hooks/useAuth'

const links = [
  { to: '/admin', label: 'Dashboard', Icon: LayoutDashboard, end: true },
  { to: '/admin/appointments', label: 'Agendamentos', Icon: Calendar, end: false },
  { to: '/admin/users', label: 'Usuários', Icon: Users, end: false },
]

export function Sidebar() {
  const { logout } = useAuth()
  return (
    <aside className="w-56 bg-bg-surface border-r border-border flex flex-col min-h-screen">
      <div className="p-5 border-b border-border">
        <div className="flex items-center gap-2">
          <Scissors size={18} className="text-accent" />
          <span className="font-display font-bold text-lg tracking-widest uppercase">
            Barber<span className="text-accent">Agenda</span>
          </span>
        </div>
      </div>
      <nav className="flex-1 p-3 flex flex-col gap-1">
        {links.map(({ to, label, Icon, end }) => (
          <NavLink
            key={to}
            to={to}
            end={end}
            className={({ isActive }) =>
              `flex items-center gap-3 px-3 py-2.5 rounded-sm text-sm font-body transition-colors ${
                isActive
                  ? 'bg-accent/10 text-accent border-l-2 border-accent'
                  : 'text-text-secondary hover:text-text-primary hover:bg-bg-elevated'
              }`
            }
          >
            <Icon size={16} strokeWidth={1.5} />
            {label}
          </NavLink>
        ))}
      </nav>
      <div className="p-3 border-t border-border">
        <button
          onClick={logout}
          className="flex items-center gap-3 px-3 py-2.5 w-full text-sm font-body text-text-secondary hover:text-red-400 transition-colors"
        >
          <LogOut size={16} strokeWidth={1.5} />
          Sair
        </button>
      </div>
    </aside>
  )
}
