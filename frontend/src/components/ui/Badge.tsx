import type { AppointmentStatus } from '../../types/appointment'

const statusConfig: Record<AppointmentStatus, { label: string; className: string }> = {
  0: { label: 'Aguardando', className: 'bg-yellow-500/10 text-yellow-400 border border-yellow-500/30' },
  1: { label: 'Confirmado', className: 'bg-green-500/10 text-green-400 border border-green-500/30' },
  2: { label: 'Cancelado', className: 'bg-red-500/10 text-red-400 border border-red-500/30' },
}

export function AppointmentBadge({ status }: { status: AppointmentStatus }) {
  const { label, className } = statusConfig[status]
  return (
    <span className={`text-xs font-body font-medium px-2 py-0.5 rounded-sm ${className}`}>
      {label}
    </span>
  )
}

interface BadgeProps {
  children: React.ReactNode
  className?: string
}

export function Badge({ children, className = '' }: BadgeProps) {
  return (
    <span className={`text-xs font-body font-medium px-2 py-0.5 rounded-sm ${className}`}>
      {children}
    </span>
  )
}
