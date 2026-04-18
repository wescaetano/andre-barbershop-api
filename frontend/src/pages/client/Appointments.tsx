import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { CalendarX, Clock } from 'lucide-react'
import { useAuthStore } from '../../store/authStore'
import { appointmentsApi } from '../../api/appointments'
import { AppointmentBadge } from '../../components/ui/Badge'
import { Button } from '../../components/ui/Button'
import { Modal } from '../../components/ui/Modal'
import { Spinner } from '../../components/ui/Spinner'
import { useToast } from '../../components/ui/Toast'
import { useApiError } from '../../hooks/useApiError'
import type { Appointment } from '../../types/appointment'

function formatDateTime(iso: string) {
  const d = new Date(iso)
  return {
    date: d.toLocaleDateString('pt-BR', { weekday: 'short', day: 'numeric', month: 'short' }),
    time: d.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' }),
  }
}

export default function ClientAppointments() {
  const userId = useAuthStore((s) => s.userId)
  const toast = useToast()
  const { getMessage } = useApiError()
  const qc = useQueryClient()
  const [cancelTarget, setCancelTarget] = useState<Appointment | null>(null)

  const { data: appointments = [], isLoading } = useQuery({
    queryKey: ['appointments', userId],
    queryFn: () => appointmentsApi.getByUser(userId!),
    enabled: !!userId,
  })

  const { mutate: cancel, isPending: cancelling } = useMutation({
    mutationFn: () => appointmentsApi.cancel({ appointmentId: cancelTarget!.id, userId: userId! }),
    onSuccess: () => {
      toast('Agendamento cancelado.', 'info')
      setCancelTarget(null)
      qc.invalidateQueries({ queryKey: ['appointments', userId] })
    },
    onError: (err: Error) => {
      toast(getMessage(err.message, 'Erro ao cancelar.'), 'error')
    },
  })

  const sorted = [...appointments].sort(
    (a, b) => new Date(b.startTime).getTime() - new Date(a.startTime).getTime(),
  )

  return (
    <div className="min-h-screen bg-bg-base">
      <div className="bg-bg-surface border-b border-border px-5 pt-10 pb-5">
        <h1 className="font-display font-extrabold text-3xl uppercase">Meus Agendamentos</h1>
      </div>

      <div className="px-5 pt-5 flex flex-col gap-3">
        {isLoading && <div className="flex justify-center py-10"><Spinner /></div>}

        {!isLoading && appointments.length === 0 && (
          <div className="border border-border border-dashed rounded-sm p-10 flex flex-col items-center gap-3 text-center mt-4">
            <CalendarX size={36} className="text-text-secondary" strokeWidth={1} />
            <p className="text-text-secondary text-sm font-body">Nenhum agendamento encontrado.</p>
          </div>
        )}

        {sorted.map((appt) => {
          const { date, time } = formatDateTime(appt.startTime)
          const canCancel = appt.status === 0
          return (
            <div
              key={appt.id}
              className={`bg-bg-surface border border-border border-l-[3px] ${
                appt.status === 2 ? 'border-l-border opacity-60' : 'border-l-accent'
              } rounded-sm p-4 flex flex-col gap-3`}
            >
              <div className="flex items-start justify-between">
                <div className="flex flex-col gap-1">
                  <span className="text-xs font-body text-text-secondary uppercase tracking-wider capitalize">{date}</span>
                  <div className="flex items-center gap-2">
                    <Clock size={13} className="text-accent" />
                    <span className="font-display font-bold text-2xl">{time}</span>
                  </div>
                </div>
                <AppointmentBadge status={appt.status} />
              </div>
              {canCancel && (
                <Button variant="danger" size="sm" onClick={() => setCancelTarget(appt)}>
                  Cancelar agendamento
                </Button>
              )}
            </div>
          )
        })}
      </div>

      <Modal open={!!cancelTarget} onClose={() => setCancelTarget(null)} title="Cancelar agendamento">
        <div className="flex flex-col gap-4">
          <p className="text-sm font-body text-text-secondary">
            Tem certeza que deseja cancelar este agendamento? Esta ação não pode ser desfeita.
          </p>
          <div className="flex gap-2">
            <Button variant="ghost" size="sm" onClick={() => setCancelTarget(null)} className="flex-1">
              Voltar
            </Button>
            <Button variant="danger" size="sm" loading={cancelling} onClick={() => cancel()} className="flex-1">
              Sim, cancelar
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  )
}
