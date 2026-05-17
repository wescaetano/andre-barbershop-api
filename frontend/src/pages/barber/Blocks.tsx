import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Plus, Trash2 } from 'lucide-react'
import { barbersApi } from '../../api/barbers'
import { apiClient } from '../../api/client'
import { useAuthStore } from '../../store/authStore'
import { Button } from '../../components/ui/Button'
import { Spinner } from '../../components/ui/Spinner'
import { Modal } from '../../components/ui/Modal'
import { useToast } from '../../components/ui/Toast'
import type { ApiResponse } from '../../types/auth'

interface ScheduleBlock {
  id: number
  barberId: number
  startTime: string
  endTime: string
  reason?: string
}

interface BlockForm {
  date: string
  startTime: string
  endTime: string
  reason: string
}

const EMPTY_FORM: BlockForm = {
  date: '',
  startTime: '',
  endTime: '',
  reason: '',
}

export default function BarberBlocks() {
  const toast = useToast()
  const qc = useQueryClient()
  const userId = useAuthStore((s) => s.userId)
  const [modalOpen, setModalOpen] = useState(false)
  const [form, setForm] = useState<BlockForm>(EMPTY_FORM)

  const { data: allBarbers = [], isLoading: loadingBarbers } = useQuery({
    queryKey: ['barbers', 'active'],
    queryFn: barbersApi.getActive,
  })

  const barberId = allBarbers.find((b) => b.userId === userId)?.id ?? null

  function toLocalIso(d: Date) {
    const pad = (n: number) => String(n).padStart(2, '0')
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`
  }

  const now = new Date()
  const threeMonthsLater = new Date(now)
  threeMonthsLater.setMonth(threeMonthsLater.getMonth() + 3)

  const { data: blocks = [], isLoading: loadingBlocks } = useQuery({
    queryKey: ['blocks', barberId],
    queryFn: async () => {
      const res = await apiClient.get<ApiResponse<ScheduleBlock[]>>(
        `/scheduleblock/${barberId}`,
        {
          params: {
            from: toLocalIso(now),
            to: toLocalIso(threeMonthsLater),
          },
        },
      )
      return res.data.data ?? []
    },
    enabled: barberId !== null,
  })

  const { mutate: createBlock, isPending: creating } = useMutation({
    mutationFn: async () => {
      const startTime = `${form.date}T${form.startTime}:00`
      const endTime = `${form.date}T${form.endTime}:00`
      await apiClient.post<ApiResponse>('/scheduleblock', {
        barberId,
        startTime,
        endTime,
        ...(form.reason ? { reason: form.reason } : {}),
      })
    },
    onSuccess: () => {
      toast('Bloqueio criado com sucesso!', 'success')
      qc.invalidateQueries({ queryKey: ['blocks', barberId] })
      setModalOpen(false)
      setForm(EMPTY_FORM)
    },
    onError: () => toast('Erro ao criar bloqueio.', 'error'),
  })

  const { mutate: deleteBlock, isPending: deleting } = useMutation({
    mutationFn: async (id: number) => {
      await apiClient.delete<ApiResponse>(`/scheduleblock/${id}`)
    },
    onSuccess: () => {
      toast('Bloqueio removido.', 'success')
      qc.invalidateQueries({ queryKey: ['blocks', barberId] })
    },
    onError: () => toast('Erro ao remover bloqueio.', 'error'),
  })

  function handleCloseModal() {
    setModalOpen(false)
    setForm(EMPTY_FORM)
  }

  function formatBlockLabel(block: ScheduleBlock) {
    const start = new Date(block.startTime)
    const end = new Date(block.endTime)
    const date = start.toLocaleDateString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    })
    const startHour = start.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })
    const endHour = end.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })
    return { date, range: `${startHour} – ${endHour}` }
  }

  const isLoading = loadingBarbers || (barberId !== null && loadingBlocks)

  if (isLoading) {
    return (
      <div className="flex justify-center py-10">
        <Spinner />
      </div>
    )
  }

  if (!loadingBarbers && barberId === null) {
    return (
      <div className="flex flex-col gap-6">
        <div>
          <p className="text-xs font-display uppercase tracking-[0.2em] text-accent mb-1">BarberAgenda</p>
          <h1 className="font-display font-extrabold text-4xl uppercase leading-none">Bloqueios</h1>
        </div>
        <p className="text-text-secondary font-body">Perfil de barbeiro não encontrado.</p>
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <p className="text-xs font-display uppercase tracking-[0.2em] text-accent mb-1">BarberAgenda</p>
          <h1 className="font-display font-extrabold text-4xl uppercase leading-none">Bloqueios</h1>
        </div>
        <Button onClick={() => setModalOpen(true)} disabled={barberId === null}>
          <Plus size={16} />
          Bloquear Horário
        </Button>
      </div>

      {/* Blocks list */}
      {blocks.length === 0 ? (
        <div className="bg-bg-surface border border-border rounded-sm px-4 py-8 text-center text-text-secondary font-body text-sm">
          Nenhum bloqueio agendado para os próximos 3 meses.
        </div>
      ) : (
        <div className="bg-bg-surface border border-border rounded-sm overflow-hidden">
          {blocks.map((block, i) => {
            const { date, range } = formatBlockLabel(block)
            return (
              <div
                key={block.id}
                className={`flex items-center gap-4 px-4 py-3 ${
                  i !== blocks.length - 1 ? 'border-b border-border' : ''
                }`}
              >
                <div className="flex-1 min-w-0">
                  <span className="font-body text-sm font-medium text-text-primary">
                    {date}
                  </span>
                  <span className="font-body text-sm text-text-secondary mx-2">·</span>
                  <span className="font-body text-sm text-text-secondary">{range}</span>
                  {block.reason && (
                    <span className="font-body text-sm text-text-secondary ml-2 italic truncate">
                      — {block.reason}
                    </span>
                  )}
                </div>
                <button
                  aria-label="Remover bloqueio"
                  disabled={deleting}
                  onClick={() => deleteBlock(block.id)}
                  className="text-text-secondary hover:text-red-400 transition-colors disabled:opacity-50 flex-shrink-0"
                >
                  <Trash2 size={16} />
                </button>
              </div>
            )
          })}
        </div>
      )}

      {/* Create modal */}
      <Modal open={modalOpen} onClose={handleCloseModal} title="Bloquear Horário">
        <div className="flex flex-col gap-4">
          <div className="flex flex-col gap-1">
            <label className="text-xs font-display uppercase tracking-wider text-text-secondary">
              Data
            </label>
            <input
              type="date"
              value={form.date}
              onChange={(e) => setForm((f) => ({ ...f, date: e.target.value }))}
              className="bg-bg-elevated border border-border rounded-sm px-3 py-2 text-sm font-body text-text-primary focus:outline-none focus:border-accent"
            />
          </div>

          <div className="flex gap-3">
            <div className="flex flex-col gap-1 flex-1">
              <label className="text-xs font-display uppercase tracking-wider text-text-secondary">
                Início
              </label>
              <input
                type="time"
                value={form.startTime}
                onChange={(e) => setForm((f) => ({ ...f, startTime: e.target.value }))}
                className="bg-bg-elevated border border-border rounded-sm px-3 py-2 text-sm font-body text-text-primary focus:outline-none focus:border-accent"
              />
            </div>
            <div className="flex flex-col gap-1 flex-1">
              <label className="text-xs font-display uppercase tracking-wider text-text-secondary">
                Fim
              </label>
              <input
                type="time"
                value={form.endTime}
                onChange={(e) => setForm((f) => ({ ...f, endTime: e.target.value }))}
                className="bg-bg-elevated border border-border rounded-sm px-3 py-2 text-sm font-body text-text-primary focus:outline-none focus:border-accent"
              />
            </div>
          </div>

          <div className="flex flex-col gap-1">
            <label className="text-xs font-display uppercase tracking-wider text-text-secondary">
              Motivo <span className="normal-case">(opcional)</span>
            </label>
            <input
              type="text"
              placeholder="Ex: Férias, consulta médica..."
              value={form.reason}
              onChange={(e) => setForm((f) => ({ ...f, reason: e.target.value }))}
              className="bg-bg-elevated border border-border rounded-sm px-3 py-2 text-sm font-body text-text-primary placeholder:text-text-secondary focus:outline-none focus:border-accent"
            />
          </div>

          <div className="flex gap-3 pt-1">
            <Button variant="ghost" fullWidth onClick={handleCloseModal} disabled={creating}>
              Cancelar
            </Button>
            <Button
              fullWidth
              loading={creating}
              disabled={!form.date || !form.startTime || !form.endTime}
              onClick={() => createBlock()}
            >
              Confirmar
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  )
}
