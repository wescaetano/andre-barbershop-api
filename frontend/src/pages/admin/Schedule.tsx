import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import { appointmentsApi } from '../../api/appointments'
import { barbersApi } from '../../api/barbers'
import { AppointmentBadge } from '../../components/ui/Badge'
import { Spinner } from '../../components/ui/Spinner'
import type { Appointment } from '../../types/appointment'
import type { Barber } from '../../types/barber'

type ViewMode = 'day' | 'week'

function getWeekStart(date: Date): Date {
  const d = new Date(date)
  d.setDate(d.getDate() - d.getDay())
  d.setHours(0, 0, 0, 0)
  return d
}

function addDays(date: Date, n: number): Date {
  const d = new Date(date)
  d.setDate(d.getDate() + n)
  return d
}

const HOURS = Array.from({ length: 12 }, (_, i) => i + 8) // 08:00 to 19:00
const DAY_LABELS = ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb']

interface BarberAppointments {
  barber: Barber
  appointments: Appointment[]
}

export default function AdminSchedule() {
  const [mode, setMode] = useState<ViewMode>('week')
  const [currentDate, setCurrentDate] = useState(new Date())
  const [selectedBarberId, setSelectedBarberId] = useState<number | 'all'>('all')

  const { data: allBarbers = [], isLoading: loadingBarbers } = useQuery({
    queryKey: ['barbers'],
    queryFn: barbersApi.getActive,
  })

  const weekStart = getWeekStart(currentDate)
  const weekEnd = addDays(weekStart, 7)
  const dayStart = new Date(currentDate)
  dayStart.setHours(0, 0, 0, 0)
  const dayEnd = addDays(dayStart, 1)

  const from = (mode === 'week' ? weekStart : dayStart).toISOString()
  const to = (mode === 'week' ? weekEnd : dayEnd).toISOString()

  const activeBarbers =
    selectedBarberId === 'all'
      ? allBarbers
      : allBarbers.filter((b) => b.id === selectedBarberId)

  const { data: barberAppointments = [], isLoading: loadingAppts } = useQuery({
    queryKey: ['admin-schedule', activeBarbers.map((b) => b.id), from, to],
    queryFn: async (): Promise<BarberAppointments[]> => {
      const results = await Promise.all(
        activeBarbers.map(async (barber) => {
          const appointments = await appointmentsApi.getByBarber(barber.id, from, to)
          return { barber, appointments }
        }),
      )
      return results
    },
    enabled: activeBarbers.length > 0,
  })

  const isLoading = loadingBarbers || loadingAppts

  const days = Array.from({ length: 7 }, (_, i) => addDays(weekStart, i))

  function getAppointmentsForBarberDayHour(
    barberData: BarberAppointments,
    day: Date,
    hour: number,
  ) {
    return barberData.appointments.filter((a) => {
      const d = new Date(a.startTime)
      return (
        d.getDate() === day.getDate() &&
        d.getMonth() === day.getMonth() &&
        d.getFullYear() === day.getFullYear() &&
        d.getHours() === hour
      )
    })
  }

  function getAppointmentsForBarberHour(barberData: BarberAppointments, hour: number) {
    const day = currentDate
    return getAppointmentsForBarberDayHour(barberData, day, hour)
  }

  function navigate(direction: 1 | -1) {
    if (mode === 'day') setCurrentDate((d) => addDays(d, direction))
    else setCurrentDate((d) => addDays(d, direction * 7))
  }

  function getHeaderLabel() {
    if (mode === 'day') {
      return currentDate.toLocaleDateString('pt-BR', {
        weekday: 'long',
        day: 'numeric',
        month: 'long',
        year: 'numeric',
      })
    }
    const end = addDays(weekStart, 6)
    return `${weekStart.toLocaleDateString('pt-BR', { day: 'numeric', month: 'short' })} – ${end.toLocaleDateString('pt-BR', { day: 'numeric', month: 'short', year: 'numeric' })}`
  }

  return (
    <div className="flex flex-col gap-4">
      <div className="flex items-center justify-between flex-wrap gap-2">
        <div>
          <p className="text-xs font-display uppercase tracking-[0.2em] text-accent mb-1">André BarberShop</p>
          <h1 className="font-display font-extrabold text-4xl uppercase leading-none">Agenda</h1>
        </div>
        <div className="flex items-center gap-2 flex-wrap">
          <select
            value={selectedBarberId}
            onChange={(e) =>
              setSelectedBarberId(e.target.value === 'all' ? 'all' : Number(e.target.value))
            }
            className="bg-bg-elevated border border-border rounded-sm px-3 py-1.5 text-xs font-body text-text-primary focus:outline-none focus:border-accent"
          >
            <option value="all">Todos os barbeiros</option>
            {allBarbers.map((b) => (
              <option key={b.id} value={b.id}>
                {b.displayName}
              </option>
            ))}
          </select>
          <div className="flex border border-border rounded-sm overflow-hidden">
            <button
              onClick={() => setMode('day')}
              className={`px-3 py-1.5 text-xs font-body transition-colors ${mode === 'day' ? 'bg-accent text-white' : 'text-text-secondary hover:text-text-primary'}`}
            >
              Dia
            </button>
            <button
              onClick={() => setMode('week')}
              className={`px-3 py-1.5 text-xs font-body transition-colors ${mode === 'week' ? 'bg-accent text-white' : 'text-text-secondary hover:text-text-primary'}`}
            >
              Semana
            </button>
          </div>
          <button onClick={() => navigate(-1)} className="text-text-secondary hover:text-text-primary p-1">
            <ChevronLeft size={18} />
          </button>
          <span className="text-xs font-body text-text-secondary capitalize">{getHeaderLabel()}</span>
          <button onClick={() => navigate(1)} className="text-text-secondary hover:text-text-primary p-1">
            <ChevronRight size={18} />
          </button>
        </div>
      </div>

      {isLoading ? (
        <div className="flex justify-center py-10">
          <Spinner />
        </div>
      ) : activeBarbers.length === 0 ? (
        <p className="text-text-secondary font-body">Nenhum barbeiro ativo encontrado.</p>
      ) : mode === 'day' ? (
        /* Day mode: columns = barbers */
        <div className="overflow-auto border border-border rounded-sm">
          <table className="w-full border-collapse text-xs font-body">
            <thead>
              <tr className="bg-bg-surface">
                <th className="w-14 border-b border-r border-border px-2 py-2 text-text-secondary font-normal">
                  Hora
                </th>
                {barberAppointments.map(({ barber }) => (
                  <th
                    key={barber.id}
                    className="border-b border-r border-border px-2 py-2 text-center min-w-[120px]"
                  >
                    <p className="font-bold text-sm text-text-primary">{barber.displayName}</p>
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {HOURS.map((hour) => (
                <tr key={hour} className="border-b border-border">
                  <td className="border-r border-border px-2 py-1 text-text-secondary text-right whitespace-nowrap">
                    {String(hour).padStart(2, '0')}:00
                  </td>
                  {barberAppointments.map((barberData) => {
                    const appts = getAppointmentsForBarberHour(barberData, hour)
                    return (
                      <td
                        key={barberData.barber.id}
                        className="border-r border-border px-1 py-1 align-top min-h-[36px]"
                      >
                        {appts.map((a) => {
                          const start = new Date(a.startTime)
                          const end = new Date(a.endTime)
                          return (
                            <div
                              key={a.id}
                              className="bg-accent/10 border border-accent/30 rounded-sm px-1.5 py-1 mb-0.5"
                            >
                              <p className="text-text-primary font-medium truncate">
                                {start.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}
                                –
                                {end.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}
                              </p>
                              <AppointmentBadge status={a.status} />
                            </div>
                          )
                        })}
                      </td>
                    )
                  })}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        /* Week mode: columns = days, show barber name + time per appointment */
        <div className="overflow-auto border border-border rounded-sm">
          <table className="w-full border-collapse text-xs font-body">
            <thead>
              <tr className="bg-bg-surface">
                <th className="w-14 border-b border-r border-border px-2 py-2 text-text-secondary font-normal">
                  Hora
                </th>
                {days.map((day) => (
                  <th
                    key={day.toISOString()}
                    className="border-b border-r border-border px-2 py-2 text-center min-w-[110px]"
                  >
                    <p className="text-text-secondary font-normal">{DAY_LABELS[day.getDay()]}</p>
                    <p
                      className={`font-bold text-sm ${day.toDateString() === new Date().toDateString() ? 'text-accent' : 'text-text-primary'}`}
                    >
                      {day.getDate()}
                    </p>
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {HOURS.map((hour) => (
                <tr key={hour} className="border-b border-border">
                  <td className="border-r border-border px-2 py-1 text-text-secondary text-right whitespace-nowrap">
                    {String(hour).padStart(2, '0')}:00
                  </td>
                  {days.map((day) => {
                    const cellAppts: { barber: Barber; appointment: Appointment }[] = []
                    for (const barberData of barberAppointments) {
                      const appts = getAppointmentsForBarberDayHour(barberData, day, hour)
                      for (const a of appts) {
                        cellAppts.push({ barber: barberData.barber, appointment: a })
                      }
                    }
                    return (
                      <td
                        key={day.toISOString()}
                        className="border-r border-border px-1 py-1 align-top min-h-[36px]"
                      >
                        {cellAppts.map(({ barber, appointment: a }) => {
                          const start = new Date(a.startTime)
                          const end = new Date(a.endTime)
                          return (
                            <div
                              key={`${barber.id}-${a.id}`}
                              className="bg-accent/10 border border-accent/30 rounded-sm px-1.5 py-1 mb-0.5"
                            >
                              <p className="text-text-primary font-medium truncate">
                                {start.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}
                                –
                                {end.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}
                              </p>
                              <p className="text-text-secondary truncate mb-0.5">{barber.displayName}</p>
                              <AppointmentBadge status={a.status} />
                            </div>
                          )
                        })}
                      </td>
                    )
                  })}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
