import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { ChevronLeft, Calendar, Clock, Check } from 'lucide-react'
import { useAuthStore } from '../../store/authStore'
import { appointmentsApi } from '../../api/appointments'
import { Button } from '../../components/ui/Button'
import { Modal } from '../../components/ui/Modal'
import { Spinner } from '../../components/ui/Spinner'
import { useToast } from '../../components/ui/Toast'
import { useApiError } from '../../hooks/useApiError'

function toISODate(d: Date) {
  return d.toISOString().split('T')[0]
}

function DatePicker({ value, onChange }: { value: Date | null; onChange: (d: Date) => void }) {
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  const weeks: Date[][] = []
  const start = new Date(today)
  start.setDate(today.getDate() - today.getDay())
  for (let w = 0; w < 5; w++) {
    const week: Date[] = []
    for (let d = 0; d < 7; d++) {
      const day = new Date(start)
      day.setDate(start.getDate() + w * 7 + d)
      week.push(day)
    }
    weeks.push(week)
  }
  const dayNames = ['D', 'S', 'T', 'Q', 'Q', 'S', 'S']
  return (
    <div className="flex flex-col gap-2">
      <div className="grid grid-cols-7 gap-0.5">
        {dayNames.map((d, i) => (
          <div key={i} className="text-center text-xs font-body text-text-secondary py-1">{d}</div>
        ))}
        {weeks.flat().map((day, i) => {
          const past = day < today
          const selected = value && toISODate(day) === toISODate(value)
          const isToday = toISODate(day) === toISODate(today)
          return (
            <button
              key={i}
              disabled={past}
              onClick={() => onChange(day)}
              className={`aspect-square flex items-center justify-center text-sm font-body rounded-sm transition-colors
                ${past ? 'text-text-secondary/30 cursor-not-allowed' : ''}
                ${selected ? 'bg-accent text-white font-bold' : ''}
                ${!selected && !past ? 'hover:bg-bg-elevated' : ''}
                ${isToday && !selected ? 'border border-accent text-accent' : ''}
              `}
            >
              {day.getDate()}
            </button>
          )
        })}
      </div>
    </div>
  )
}

function SlotGrid({ slots, selected, onSelect }: { slots: string[]; selected: string | null; onSelect: (s: string) => void }) {
  const allSlots: string[] = []
  for (let h = 9; h < 18; h++) {
    allSlots.push(`${String(h).padStart(2, '0')}:00`)
    allSlots.push(`${String(h).padStart(2, '0')}:30`)
  }
  return (
    <div className="grid grid-cols-4 gap-2">
      {allSlots.map((slot) => {
        const available = slots.includes(slot)
        const isSelected = selected === slot
        return (
          <button
            key={slot}
            disabled={!available}
            onClick={() => onSelect(slot)}
            className={`py-2.5 text-sm font-body rounded-sm transition-colors border
              ${!available ? 'border-border/30 text-text-secondary/30 line-through cursor-not-allowed' : ''}
              ${isSelected ? 'bg-accent border-accent text-white font-medium' : ''}
              ${available && !isSelected ? 'border-border text-text-primary hover:border-accent hover:text-accent' : ''}
            `}
          >
            {slot}
          </button>
        )
      })}
    </div>
  )
}

const STEPS = ['Data', 'Horário', 'Confirmar']

export default function Book() {
  const navigate = useNavigate()
  const toast = useToast()
  const { getMessage } = useApiError()
  const userId = useAuthStore((s) => s.userId)
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const [step, setStep] = useState(0)
  const [selectedDate, setSelectedDate] = useState<Date | null>(null)
  const [selectedSlot, setSelectedSlot] = useState<string | null>(null)

  const dateString = selectedDate ? toISODate(selectedDate) : ''

  const { data: slots = [], isFetching: loadingSlots, isError: slotsError } = useQuery({
    queryKey: ['slots', dateString],
    queryFn: () => appointmentsApi.getAvailableSlots(dateString),
    enabled: !!dateString && step >= 1,
  })

  const { mutate: createAppointment, isPending } = useMutation({
    mutationFn: () =>
      appointmentsApi.create({ userId: userId!, date: dateString, startTime: selectedSlot! }),
    onSuccess: () => {
      toast('Agendamento criado com sucesso!', 'success')
      navigate('/app/appointments')
    },
    onError: (err: Error) => {
      toast(getMessage(err.message, 'Erro ao agendar.'), 'error')
    },
  })

  const formattedDate = selectedDate
    ? selectedDate.toLocaleDateString('pt-BR', { weekday: 'long', day: 'numeric', month: 'long' })
    : ''

  return (
    <div className="min-h-screen bg-bg-base flex flex-col">
      <div className="bg-bg-surface border-b border-border px-5 pt-10 pb-4 flex items-center gap-3">
        <button onClick={() => step === 0 ? navigate(-1) : setStep(s => s - 1)} className="text-text-secondary hover:text-text-primary">
          <ChevronLeft size={22} />
        </button>
        <h1 className="font-display font-bold text-2xl uppercase">Agendar</h1>
      </div>

      {/* Step indicator */}
      <div className="px-5 pt-4 flex items-center gap-2">
        {STEPS.map((label, i) => (
          <div key={i} className="flex items-center gap-2 flex-1">
            <div className={`w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold transition-colors ${
              i < step ? 'bg-accent text-white' : i === step ? 'border-2 border-accent text-accent' : 'border border-border text-text-secondary'
            }`}>
              {i < step ? <Check size={12} /> : i + 1}
            </div>
            <span className={`text-xs font-body ${i === step ? 'text-text-primary' : 'text-text-secondary'}`}>{label}</span>
            {i < STEPS.length - 1 && <div className={`flex-1 h-px ${i < step ? 'bg-accent' : 'bg-border'}`} />}
          </div>
        ))}
      </div>

      <Modal
        open={step === 2 && !isAuthenticated}
        onClose={() => setStep(1)}
        title="Conta necessária"
      >
        <div className="flex flex-col gap-4">
          <p className="text-sm font-body text-text-secondary">
            Para finalizar o agendamento, crie uma conta ou entre na sua.
          </p>
          <Button fullWidth size="lg" onClick={() => navigate('/login', { state: { returnTo: '/book' } })}>
            Entrar
          </Button>
          <Button fullWidth size="lg" variant="ghost" onClick={() => navigate('/login?mode=register', { state: { returnTo: '/book' } })}>
            Criar conta
          </Button>
        </div>
      </Modal>

      <div className="flex-1 px-5 pt-6 pb-24">
        {step === 0 && (
          <div className="flex flex-col gap-5">
            <div className="flex items-center gap-2 mb-1">
              <Calendar size={16} className="text-accent" />
              <h2 className="font-display font-bold text-sm uppercase tracking-widest text-text-secondary">Escolha a data</h2>
            </div>
            <DatePicker value={selectedDate} onChange={setSelectedDate} />
            <Button fullWidth size="lg" disabled={!selectedDate} onClick={() => setStep(1)}>
              Próximo
            </Button>
          </div>
        )}

        {step === 1 && (
          <div className="flex flex-col gap-5">
            <div>
              <div className="flex items-center gap-2 mb-1">
                <Clock size={16} className="text-accent" />
                <h2 className="font-display font-bold text-sm uppercase tracking-widest text-text-secondary">Escolha o horário</h2>
              </div>
              <p className="text-text-secondary text-xs font-body capitalize">{formattedDate}</p>
            </div>
            {loadingSlots ? (
              <div className="flex justify-center py-10"><Spinner /></div>
            ) : slotsError ? (
              <div className="py-8 flex flex-col items-center gap-2 text-center">
                <p className="text-sm font-body text-text-secondary">Não foi possível carregar os horários.</p>
                <p className="text-xs font-body text-text-secondary/60">Tente novamente mais tarde.</p>
              </div>
            ) : slots.length === 0 ? (
              <div className="py-8 text-center">
                <p className="text-sm font-body text-text-secondary">Nenhum horário disponível nesta data.</p>
              </div>
            ) : (
              <SlotGrid slots={slots} selected={selectedSlot} onSelect={setSelectedSlot} />
            )}
            <Button fullWidth size="lg" disabled={!selectedSlot} onClick={() => setStep(2)}>
              Próximo
            </Button>
          </div>
        )}

        {step === 2 && (
          <div className="flex flex-col gap-5">
            <h2 className="font-display font-bold text-sm uppercase tracking-widest text-text-secondary">Confirmação</h2>
            <div className="bg-bg-surface border border-border border-l-[3px] border-l-accent rounded-sm p-5 flex flex-col gap-3">
              <div className="flex justify-between items-center">
                <span className="text-xs font-body text-text-secondary uppercase tracking-wider">Data</span>
                <span className="font-body font-medium text-text-primary capitalize">{formattedDate}</span>
              </div>
              <div className="h-px bg-border" />
              <div className="flex justify-between items-center">
                <span className="text-xs font-body text-text-secondary uppercase tracking-wider">Horário</span>
                <span className="font-display font-bold text-2xl text-text-primary">{selectedSlot}</span>
              </div>
            </div>
            <Button fullWidth size="lg" loading={isPending} onClick={() => createAppointment()}>
              Confirmar Agendamento
            </Button>
          </div>
        )}
      </div>
    </div>
  )
}
