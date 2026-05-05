import { useQuery } from '@tanstack/react-query'
import { Calendar, Clock } from 'lucide-react'
import { appointmentsApi } from '../../api/appointments'
import { AppointmentBadge, Badge } from '../../components/ui/Badge'
import { Spinner } from '../../components/ui/Spinner'
import { useAuthStore } from '../../store/authStore'

function isToday(isoString: string) {
  const d = new Date(isoString)
  const today = new Date()
  return d.getDate() === today.getDate() && d.getMonth() === today.getMonth() && d.getFullYear() === today.getFullYear()
}

function isThisWeek(isoString: string) {
  const d = new Date(isoString)
  const now = new Date()
  const weekStart = new Date(now)
  weekStart.setDate(now.getDate() - now.getDay())
  weekStart.setHours(0, 0, 0, 0)
  const weekEnd = new Date(weekStart)
  weekEnd.setDate(weekStart.getDate() + 7)
  return d >= weekStart && d < weekEnd
}

function StatCard({ label, value, Icon }: { label: string; value: number; Icon: React.ElementType }) {
  return (
    <div className="bg-bg-surface border border-border border-l-[3px] border-l-accent rounded-sm p-5 flex items-center gap-4">
      <div className="w-10 h-10 bg-accent/10 rounded-sm flex items-center justify-center">
        <Icon size={18} className="text-accent" />
      </div>
      <div>
        <p className="text-text-secondary text-xs font-body uppercase tracking-wider">{label}</p>
        <p className="font-display font-bold text-3xl">{value}</p>
      </div>
    </div>
  )
}

export default function AdminDashboard() {
  const userId = useAuthStore((s) => s.userId)
  const userName = useAuthStore((s) => s.userName)
  const role = useAuthStore((s) => s.role)

  const { data: appointments = [], isLoading } = useQuery({
    queryKey: ['admin-appointments'],
    queryFn: () => appointmentsApi.getByUser(userId!),
    enabled: !!userId,
  })

  const todayCount = appointments.filter((a) => isToday(a.startTime) && a.status !== 2).length
  const weekCount = appointments.filter((a) => isThisWeek(a.startTime) && a.status !== 2).length
  const recent = [...appointments]
    .sort((a, b) => new Date(b.creationDate).getTime() - new Date(a.creationDate).getTime())
    .slice(0, 10)

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <p className="text-xs font-display uppercase tracking-[0.2em] text-accent mb-1">BarberAgenda</p>
          <h1 className="font-display font-extrabold text-4xl uppercase leading-none">
            Olá, {userName?.split(' ')[0] ?? 'Barbeiro'}
          </h1>
          <p className="text-text-secondary text-sm font-body mt-2">Seja bem-vindo ao BarberAgenda!</p>
        </div>
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

      {isLoading ? (
        <div className="flex justify-center py-10"><Spinner /></div>
      ) : (
        <>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <StatCard label="Agendamentos hoje" value={todayCount} Icon={Calendar} />
            <StatCard label="Esta semana" value={weekCount} Icon={Clock} />
          </div>

          <section>
            <h2 className="font-display font-bold text-sm uppercase tracking-widest text-text-secondary mb-3">
              Recentes
            </h2>
            <div className="bg-bg-surface border border-border rounded-sm overflow-hidden">
              {recent.length === 0 && (
                <p className="text-text-secondary text-sm font-body p-5">Nenhum agendamento.</p>
              )}
              {recent.map((appt, i) => {
                const d = new Date(appt.startTime)
                return (
                  <div key={appt.id} className={`flex items-center justify-between px-4 py-3 ${i !== recent.length - 1 ? 'border-b border-border' : ''}`}>
                    <div>
                      <p className="text-sm font-body font-medium">
                        {d.toLocaleDateString('pt-BR')} — {d.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}
                      </p>
                      <p className="text-xs text-text-secondary font-body">ID #{appt.id}</p>
                    </div>
                    <AppointmentBadge status={appt.status} />
                  </div>
                )
              })}
            </div>
          </section>
        </>
      )}
    </div>
  )
}
