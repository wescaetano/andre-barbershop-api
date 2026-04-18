import { useParams, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { ChevronLeft, Mail, Calendar } from 'lucide-react'
import { usersApi } from '../../api/users'
import { appointmentsApi } from '../../api/appointments'
import { Badge, AppointmentBadge } from '../../components/ui/Badge'
import { Spinner } from '../../components/ui/Spinner'

export default function UserDetail() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const userId = Number(id)

  const { data: user, isLoading: loadingUser } = useQuery({
    queryKey: ['user', userId],
    queryFn: () => usersApi.getById(userId),
    enabled: !!userId,
  })

  const { data: appointments = [], isLoading: loadingAppts } = useQuery({
    queryKey: ['appointments', userId],
    queryFn: () => appointmentsApi.getByUser(userId),
    enabled: !!userId,
  })

  if (loadingUser) return <div className="flex justify-center py-20"><Spinner /></div>
  if (!user) return <p className="text-text-secondary font-body">Usuário não encontrado.</p>

  const sortedAppts = [...appointments].sort(
    (a, b) => new Date(b.startTime).getTime() - new Date(a.startTime).getTime(),
  )

  return (
    <div className="flex flex-col gap-6 max-w-2xl">
      <button onClick={() => navigate('/admin/users')} className="flex items-center gap-1 text-text-secondary hover:text-text-primary text-sm font-body transition-colors w-fit">
        <ChevronLeft size={16} />
        Voltar para usuários
      </button>

      <div className="bg-bg-surface border border-border border-l-[3px] border-l-accent rounded-sm p-6 flex flex-col gap-4">
        <div className="flex items-start justify-between">
          <div>
            <h1 className="font-display font-extrabold text-3xl uppercase">{user.name}</h1>
            <div className="flex items-center gap-1.5 mt-1">
              <Mail size={12} className="text-text-secondary" />
              <span className="text-text-secondary text-sm font-body">{user.email}</span>
            </div>
          </div>
          <Badge className={user.status === 1 ? 'bg-green-500/10 text-green-400 border border-green-500/30' : 'bg-red-500/10 text-red-400 border border-red-500/30'}>
            {user.status === 1 ? 'Ativo' : 'Inativo'}
          </Badge>
        </div>
        <div className="flex items-center gap-1.5 text-xs text-text-secondary font-body">
          <Calendar size={12} />
          Cadastrado em {new Date(user.creationDate).toLocaleDateString('pt-BR')}
        </div>
        {user.profilesUsers && user.profilesUsers.length > 0 && (
          <div>
            <span className="text-xs font-body text-text-secondary uppercase tracking-wider">Perfis: </span>
            {user.profilesUsers.map((pu) => (
              <Badge key={pu.profile.id} className="border border-border text-text-secondary ml-1">{pu.profile.name}</Badge>
            ))}
          </div>
        )}
      </div>

      <section>
        <h2 className="font-display font-bold text-sm uppercase tracking-widest text-text-secondary mb-3">
          Histórico de Agendamentos
        </h2>
        {loadingAppts ? (
          <div className="flex justify-center py-6"><Spinner /></div>
        ) : (
          <div className="bg-bg-surface border border-border rounded-sm overflow-hidden">
            {sortedAppts.length === 0 && (
              <p className="text-text-secondary text-sm font-body p-5">Nenhum agendamento.</p>
            )}
            {sortedAppts.map((appt, i) => {
              const d = new Date(appt.startTime)
              return (
                <div key={appt.id} className={`flex items-center justify-between px-4 py-3 ${i !== sortedAppts.length - 1 ? 'border-b border-border' : ''}`}>
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
        )}
      </section>
    </div>
  )
}
