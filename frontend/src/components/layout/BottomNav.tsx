import { NavLink } from 'react-router-dom'
import { Home, CalendarPlus, Calendar, User } from 'lucide-react'

const links = [
  { to: '/app', label: 'Início', Icon: Home, end: true },
  { to: '/book', label: 'Agendar', Icon: CalendarPlus, end: false },
  { to: '/app/appointments', label: 'Meus', Icon: Calendar, end: false },
  { to: '/app/profile', label: 'Perfil', Icon: User, end: false },
]

export function BottomNav() {
  return (
    <nav className="fixed bottom-0 left-0 right-0 bg-bg-surface border-t border-border z-40">
      <div className="flex">
        {links.map(({ to, label, Icon, end }) => (
          <NavLink
            key={to}
            to={to}
            end={end}
            className={({ isActive }) =>
              `flex-1 flex flex-col items-center gap-0.5 py-3 text-xs font-body transition-colors ${
                isActive ? 'text-accent' : 'text-text-secondary'
              }`
            }
          >
            <Icon size={20} strokeWidth={1.5} />
            <span>{label}</span>
          </NavLink>
        ))}
      </div>
    </nav>
  )
}
