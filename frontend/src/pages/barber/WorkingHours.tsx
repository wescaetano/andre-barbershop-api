import { useState, useEffect } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { barbersApi } from '../../api/barbers'
import { workingHoursApi } from '../../api/workingHours'
import { useAuthStore } from '../../store/authStore'
import { Button } from '../../components/ui/Button'
import { Spinner } from '../../components/ui/Spinner'
import { useToast } from '../../components/ui/Toast'
import type { WorkingHoursDay } from '../../types/workingHours'

const DAY_NAMES = ['Domingo', 'Segunda', 'Terça', 'Quarta', 'Quinta', 'Sexta', 'Sábado']

const DEFAULT_DAYS: WorkingHoursDay[] = Array.from({ length: 7 }, (_, i) => ({
  dayOfWeek: i,
  isOpen: i >= 1 && i <= 6,
  openTime: '09:00',
  closeTime: '18:00',
}))

export default function BarberWorkingHours() {
  const toast = useToast()
  const qc = useQueryClient()
  const userId = useAuthStore((s) => s.userId)
  const [days, setDays] = useState<WorkingHoursDay[]>(DEFAULT_DAYS)

  const { data: allBarbers = [], isLoading: loadingBarbers } = useQuery({
    queryKey: ['barbers', 'active'],
    queryFn: barbersApi.getActive,
  })

  const barberId = allBarbers.find((b) => b.userId === userId)?.id ?? null

  const { data: fetchedHours, isLoading: loadingHours } = useQuery({
    queryKey: ['working-hours', barberId],
    queryFn: () => workingHoursApi.getByBarber(barberId!),
    enabled: barberId !== null,
  })

  useEffect(() => {
    if (!fetchedHours) return
    if (fetchedHours.length === 0) return
    setDays(
      DEFAULT_DAYS.map((def) => {
        const saved = fetchedHours.find((h) => h.dayOfWeek === def.dayOfWeek)
        return saved ? { ...def, ...saved } : def
      }),
    )
  }, [fetchedHours])

  const { mutate: save, isPending: saving } = useMutation({
    mutationFn: () => workingHoursApi.upsert(barberId!, days),
    onSuccess: () => {
      toast('Horários salvos com sucesso!', 'success')
      qc.invalidateQueries({ queryKey: ['working-hours', barberId] })
    },
    onError: () => toast('Erro ao salvar horários.', 'error'),
  })

  function toggleDay(index: number) {
    setDays((prev) =>
      prev.map((d, i) => (i === index ? { ...d, isOpen: !d.isOpen } : d)),
    )
  }

  function updateTime(index: number, field: 'openTime' | 'closeTime', value: string) {
    setDays((prev) =>
      prev.map((d, i) => (i === index ? { ...d, [field]: value } : d)),
    )
  }

  const isLoading = loadingBarbers || (barberId !== null && loadingHours)

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
          <h1 className="font-display font-extrabold text-4xl uppercase leading-none">Horários</h1>
        </div>
        <p className="text-text-secondary font-body">Perfil de barbeiro não encontrado.</p>
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-xs font-display uppercase tracking-[0.2em] text-accent mb-1">BarberAgenda</p>
          <h1 className="font-display font-extrabold text-4xl uppercase leading-none">Horários</h1>
        </div>
        <Button onClick={() => save()} loading={saving} disabled={barberId === null}>
          Salvar
        </Button>
      </div>

      <div className="bg-bg-surface border border-border rounded-sm overflow-hidden">
        {days.map((day, i) => (
          <div
            key={day.dayOfWeek}
            className={`flex items-center gap-4 px-4 py-3 ${
              i !== days.length - 1 ? 'border-b border-border' : ''
            }`}
          >
            {/* Toggle */}
            <button
              aria-label={day.isOpen ? `Fechar ${DAY_NAMES[day.dayOfWeek]}` : `Abrir ${DAY_NAMES[day.dayOfWeek]}`}
              onClick={() => toggleDay(i)}
              className={`flex-shrink-0 w-10 h-5 rounded-full transition-colors relative ${
                day.isOpen ? 'bg-accent' : 'bg-bg-elevated'
              }`}
            >
              <span
                className={`absolute top-0.5 left-0.5 w-4 h-4 rounded-full bg-white shadow transition-transform ${
                  day.isOpen ? 'translate-x-5' : 'translate-x-0'
                }`}
              />
            </button>

            {/* Day name */}
            <span
              className={`w-20 font-body text-sm font-medium flex-shrink-0 ${
                day.isOpen ? 'text-text-primary' : 'text-text-secondary'
              }`}
            >
              {DAY_NAMES[day.dayOfWeek]}
            </span>

            {/* Time inputs */}
            {day.isOpen ? (
              <div className="flex items-center gap-2 flex-wrap">
                <input
                  type="time"
                  value={day.openTime}
                  aria-label={`Abertura ${DAY_NAMES[day.dayOfWeek]}`}
                  onChange={(e) => updateTime(i, 'openTime', e.target.value)}
                  className="bg-bg-elevated border border-border rounded-sm px-2 py-1 text-sm font-body text-text-primary focus:outline-none focus:border-accent"
                />
                <span className="text-text-secondary text-sm font-body">até</span>
                <input
                  type="time"
                  value={day.closeTime}
                  aria-label={`Fechamento ${DAY_NAMES[day.dayOfWeek]}`}
                  onChange={(e) => updateTime(i, 'closeTime', e.target.value)}
                  className="bg-bg-elevated border border-border rounded-sm px-2 py-1 text-sm font-body text-text-primary focus:outline-none focus:border-accent"
                />
              </div>
            ) : (
              <span className="text-text-secondary text-sm font-body italic">Fechado</span>
            )}
          </div>
        ))}
      </div>
    </div>
  )
}
