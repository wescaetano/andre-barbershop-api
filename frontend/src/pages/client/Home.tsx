import { useNavigate } from 'react-router-dom'
import { CalendarPlus, Clock, ChevronRight } from 'lucide-react'
import { useQuery } from '@tanstack/react-query'
import { useAuthStore } from '../../store/authStore'
import { appointmentsApi } from '../../api/appointments'
import { Button } from '../../components/ui/Button'
import { AppointmentBadge, Badge } from '../../components/ui/Badge'
import { Spinner } from '../../components/ui/Spinner'
import type { Appointment } from '../../types/appointment'

function formatDate(isoString: string) {
  return new Date(isoString).toLocaleDateString('pt-BR', { weekday: 'short', day: 'numeric', month: 'short' })
}

function formatTime(isoString: string) {
  return new Date(isoString).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })
}

function AppointmentCard({ appt }: { appt: Appointment }) {
  const navigate = useNavigate()
  const cancelled = appt.status === 2
  return (
    <div
      className={`bg-bg-surface border border-border border-l-[3px] ${cancelled ? 'border-l-border opacity-50' : 'border-l-accent'} rounded-sm p-4 flex items-center justify-between gap-3 cursor-pointer hover:bg-bg-elevated transition-colors`}
      onClick={() => navigate('/app/appointments')}
    >
      <div className="flex flex-col gap-1">
        <span className="text-xs font-body text-text-secondary uppercase tracking-wider">
          {formatDate(appt.startTime)}
        </span>
        <div className="flex items-center gap-2">
          <Clock size={14} className="text-accent" />
          <span className="font-display font-bold text-xl">{formatTime(appt.startTime)}</span>
        </div>
        <AppointmentBadge status={appt.status} />
      </div>
      <ChevronRight size={16} className="text-text-secondary" />
    </div>
  )
}

export default function ClientHome() {
  const navigate = useNavigate()
  const userId = useAuthStore((s) => s.userId)
  const userName = useAuthStore((s) => s.userName)
  const role = useAuthStore((s) => s.role)

  const { data: appointments = [], isLoading } = useQuery({
    queryKey: ['appointments', userId],
    queryFn: () => appointmentsApi.getByUser(userId!),
    enabled: !!userId,
  })

  const upcoming = appointments
    .filter((a) => a.status !== 2 && new Date(a.startTime) >= new Date())
    .sort((a, b) => new Date(a.startTime).getTime() - new Date(b.startTime).getTime())
    .slice(0, 3)

  return (
    <div className="min-h-screen bg-bg-base">
      {/* Header */}
      <div className="bg-bg-surface border-b border-border px-5 pt-10 pb-6 clip-diagonal">
        <p className="text-xs font-display uppercase tracking-[0.2em] text-accent mb-1">Barbearia</p>
        <h1 className="font-display font-extrabold text-4xl uppercase leading-none">
          Olá,<br />{userName?.split(' ')[0] ?? 'Cliente'}
        </h1>
      </div>

      {/* Role banner */}
      <div className="px-5 py-3 bg-bg-surface border-b border-border flex items-center justify-between">
        <span className="text-sm font-body text-text-secondary">
          Seja bem-vindo ao André BarberShop!
        </span>
        <Badge
          className={
            role === 'admin'
              ? 'bg-accent/10 text-accent border border-accent/30'
              : 'bg-blue-500/10 text-blue-400 border border-blue-500/30'
          }
        >
          {role === 'admin' ? 'Barbeiro' : 'Cliente'}
        </Badge>
      </div>

      <div className="px-5 pt-6 flex flex-col gap-6">
        {/* CTA */}
        <Button fullWidth size="lg" onClick={() => navigate('/book')}>
          <CalendarPlus size={18} />
          Agendar Agora
        </Button>

        {/* Upcoming appointments */}
        <section>
          <h2 className="font-display font-bold text-sm uppercase tracking-widest text-text-secondary mb-3">
            Próximos Agendamentos
          </h2>

          {isLoading && (
            <div className="flex justify-center py-8">
              <Spinner />
            </div>
          )}

          {!isLoading && upcoming.length === 0 && (
            <div className="border border-border border-dashed rounded-sm p-8 flex flex-col items-center gap-3 text-center">
              <CalendarPlus size={32} className="text-text-secondary" strokeWidth={1} />
              <p className="text-text-secondary text-sm font-body">Nenhum agendamento próximo.</p>
              <Button size="sm" onClick={() => navigate('/book')}>Fazer primeiro agendamento</Button>
            </div>
          )}

          {!isLoading && upcoming.map((appt) => (
            <div key={appt.id} className="mb-2">
              <AppointmentCard appt={appt} />
            </div>
          ))}
        </section>
      </div>
    </div>
  )
}
