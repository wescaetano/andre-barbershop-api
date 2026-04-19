import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { appointmentsApi } from '../../api/appointments'
import { AppointmentBadge } from '../../components/ui/Badge'
import { Spinner } from '../../components/ui/Spinner'
import { useAuthStore } from '../../store/authStore'
import type { AppointmentStatus } from '../../types/appointment'

const STATUS_FILTERS: { label: string; value: AppointmentStatus | 'all' }[] = [
  { label: 'Todos', value: 'all' },
  { label: 'Aguardando', value: 0 },
  { label: 'Confirmados', value: 1 },
  { label: 'Cancelados', value: 2 },
]

export default function AdminAppointments() {
  const userId = useAuthStore((s) => s.userId)
  const [statusFilter, setStatusFilter] = useState<AppointmentStatus | 'all'>('all')

  const { data: appointments = [], isLoading } = useQuery({
    queryKey: ['admin-appointments-list'],
    queryFn: () => appointmentsApi.getByUser(userId!),
    enabled: !!userId,
  })

  const filtered = statusFilter === 'all' ? appointments : appointments.filter((a) => a.status === statusFilter)
  const sorted = [...filtered].sort((a, b) => new Date(b.startTime).getTime() - new Date(a.startTime).getTime())

  return (
    <div className="flex flex-col gap-6">
      <div>
        <h1 className="font-display font-extrabold text-4xl uppercase">Agendamentos</h1>
        <p className="text-text-secondary text-sm font-body mt-1">{filtered.length} registros</p>
      </div>

      <div className="flex gap-2 flex-wrap">
        {STATUS_FILTERS.map(({ label, value }) => (
          <button
            key={String(value)}
            onClick={() => setStatusFilter(value)}
            className={`px-3 py-1.5 text-xs font-body rounded-sm border transition-colors ${
              statusFilter === value
                ? 'bg-accent border-accent text-white'
                : 'border-border text-text-secondary hover:border-accent hover:text-accent'
            }`}
          >
            {label}
          </button>
        ))}
      </div>

      {isLoading ? (
        <div className="flex justify-center py-10"><Spinner /></div>
      ) : (
        <div className="bg-bg-surface border border-border rounded-sm overflow-hidden">
          {sorted.length === 0 && (
            <p className="text-text-secondary text-sm font-body p-6">Nenhum agendamento encontrado.</p>
          )}
          {sorted.map((appt, i) => {
            const d = new Date(appt.startTime)
            return (
              <div key={appt.id} className={`flex items-center justify-between px-4 py-3 gap-3 ${i !== sorted.length - 1 ? 'border-b border-border' : ''}`}>
                <div>
                  <p className="text-sm font-body font-medium">
                    {d.toLocaleDateString('pt-BR', { weekday: 'short', day: 'numeric', month: 'short' })}
                  </p>
                  <p className="text-xs text-text-secondary font-body">
                    {d.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })} — ID #{appt.id}
                  </p>
                </div>
                <AppointmentBadge status={appt.status} />
              </div>
            )
          })}
        </div>
      )}
    </div>
  )
}
