# BarberAgenda — Slice 4: Agenda View Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Barbers and admin can visualize appointments in a day/week calendar view. Add a `GET /appointment/barber/:id` endpoint. Add barber schedule page. Add admin schedule page. Update admin sidebar with all new sections.

**Prerequisite:** Slices 1, 2, and 3 must be complete.

**Architecture:** New `GetBarberAppointmentsUseCase` → endpoint on `AppointmentController` → frontend `barber/Schedule.tsx` (day/week calendar) → frontend `admin/Schedule.tsx` (all barbers). Update admin Sidebar + routes.

**Tech Stack:** .NET 8, React + Vite + TypeScript, TanStack Query, Tailwind CSS

**Spec:** `docs/superpowers/specs/2026-05-09-saas-backoffice-client-redesign.md` — Slice 4

---

## File Map

**Create (backend):**
- `src/BarberShop.Application/UseCases/Appointment/GetByBarber/IGetBarberAppointmentsUseCase.cs`
- `src/BarberShop.Application/UseCases/Appointment/GetByBarber/GetBarberAppointmentsUseCase.cs`

**Modify (backend):**
- `src/BarberShop.Api/Controllers/AppointmentController.cs` — add GetByBarber endpoint
- `src/BarberShop.IOC/Injection.cs` — register use case

**Create (frontend):**
- `frontend/src/pages/barber/Schedule.tsx`
- `frontend/src/pages/admin/Schedule.tsx`

**Modify (frontend):**
- `frontend/src/api/appointments.ts` — add `getByBarber` method
- `frontend/src/router/index.tsx` — add barber/schedule + admin/schedule routes
- `frontend/src/components/layout/Sidebar.tsx` — add Barbers, Services, Schedule links

---

### Task 23: GetByBarber appointment endpoint

**Files:**
- Create: `src/BarberShop.Application/UseCases/Appointment/GetByBarber/IGetBarberAppointmentsUseCase.cs`
- Create: `src/BarberShop.Application/UseCases/Appointment/GetByBarber/GetBarberAppointmentsUseCase.cs`
- Modify: `src/BarberShop.Api/Controllers/AppointmentController.cs`
- Modify: `src/BarberShop.IOC/Injection.cs`

- [ ] **Step 1: Use case**

```csharp
// IGetBarberAppointmentsUseCase.cs
using BarberShop.Communication.Models;
namespace BarberShop.Application.UseCases.Appointment.GetByBarber
{
    public interface IGetBarberAppointmentsUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(long barberId, DateTime from, DateTime to);
    }
}
```

```csharp
// GetBarberAppointmentsUseCase.cs
using BarberShop.Communication.Enums.Appointment;
using BarberShop.Communication.Models;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Appointment.GetByBarber
{
    public class GetBarberAppointmentsUseCase : IGetBarberAppointmentsUseCase
    {
        private readonly IBaseRepository<Domain.Appointment> _repo;
        public GetBarberAppointmentsUseCase(IBaseRepository<Domain.Appointment> repo) => _repo = repo;

        public async Task<ResponseModel<dynamic>> ExecuteAsync(long barberId, DateTime from, DateTime to)
        {
            var appointments = await _repo.GetAll(
                a => a.BarberId == barberId
                  && a.StartTime >= from
                  && a.StartTime < to);

            var result = appointments
                .OrderBy(a => a.StartTime)
                .Select(a => new
                {
                    a.Id,
                    a.UserId,
                    a.BarberId,
                    a.ServiceId,
                    a.StartTime,
                    a.EndTime,
                    Status = a.Status.ToString(),
                    a.CreationDate
                }).ToList();

            return FactoryResponse<dynamic>.Success(result);
        }
    }
}
```

- [ ] **Step 2: Add endpoint to AppointmentController**

Add to the constructor injection in `AppointmentController.cs`:

```csharp
private readonly IGetBarberAppointmentsUseCase _getBarberAppointmentsUseCase;
```

Add to constructor parameters:
```csharp
IGetBarberAppointmentsUseCase getBarberAppointmentsUseCase
```

Add assignment:
```csharp
_getBarberAppointmentsUseCase = getBarberAppointmentsUseCase;
```

Add using at top:
```csharp
using BarberShop.Application.UseCases.Appointment.GetByBarber;
```

Add action method:
```csharp
/// <summary>Lista agendamentos de um barbeiro em um intervalo de datas</summary>
[HttpGet("barber/{barberId:long}")]
public async Task<IActionResult> GetByBarber(
    [FromRoute] long barberId,
    [FromQuery] DateTime from,
    [FromQuery] DateTime to)
{
    var result = await _getBarberAppointmentsUseCase.ExecuteAsync(barberId, from, to);
    return Result(result);
}
```

- [ ] **Step 3: Register in IOC**

Add using:
```csharp
using BarberShop.Application.UseCases.Appointment.GetByBarber;
```

Add registration:
```csharp
services.AddScoped<IGetBarberAppointmentsUseCase, GetBarberAppointmentsUseCase>();
```

- [ ] **Step 4: Build and commit**

```bash
dotnet build src/BarberShop.Api/BarberShop.Api.csproj
git add src/BarberShop.Application/UseCases/Appointment/GetByBarber/ src/BarberShop.Api/Controllers/AppointmentController.cs src/BarberShop.IOC/Injection.cs
git commit -m "feat(backend): add GetByBarber appointment endpoint"
```

---

### Task 24: Frontend — update appointments API

**Files:**
- Modify: `frontend/src/api/appointments.ts`

- [ ] **Step 1: Add getByBarber**

In `frontend/src/api/appointments.ts`, add method:

```ts
getByBarber: async (barberId: number, from: string, to: string): Promise<Appointment[]> => {
  const { data } = await apiClient.get<ApiResponse<Appointment[]>>(
    `/appointment/barber/${barberId}`,
    { params: { from, to } }
  )
  return data.data ?? []
},
```

- [ ] **Step 2: Commit**

```bash
git add frontend/src/api/appointments.ts
git commit -m "feat(frontend): add getByBarber to appointmentsApi"
```

---

### Task 25: Barber Schedule page (day/week view)

**Files:**
- Create: `frontend/src/pages/barber/Schedule.tsx`
- Modify: `frontend/src/router/index.tsx`

- [ ] **Step 1: Create Schedule page**

```tsx
// frontend/src/pages/barber/Schedule.tsx
import { useEffect, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import { appointmentsApi } from '../../api/appointments'
import { AppointmentBadge } from '../../components/ui/Badge'
import { Spinner } from '../../components/ui/Spinner'
import { useAuthStore } from '../../store/authStore'

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

const HOURS = Array.from({ length: 18 }, (_, i) => i + 6) // 06:00 to 23:00

export default function BarberSchedule() {
  const userId = useAuthStore((s) => s.userId)
  const [barberId, setBarberId] = useState<number | null>(null)
  const [mode, setMode] = useState<ViewMode>('week')
  const [currentDate, setCurrentDate] = useState(new Date())

  useEffect(() => { if (userId) setBarberId(userId) }, [userId])

  const weekStart = getWeekStart(currentDate)
  const weekEnd = addDays(weekStart, 7)
  const dayStart = new Date(currentDate)
  dayStart.setHours(0, 0, 0, 0)
  const dayEnd = addDays(dayStart, 1)

  const from = (mode === 'week' ? weekStart : dayStart).toISOString()
  const to = (mode === 'week' ? weekEnd : dayEnd).toISOString()

  const { data: appointments = [], isLoading } = useQuery({
    queryKey: ['barber-schedule', barberId, from, to],
    queryFn: () => appointmentsApi.getByBarber(barberId!, from, to),
    enabled: !!barberId,
  })

  const days = mode === 'week'
    ? Array.from({ length: 7 }, (_, i) => addDays(weekStart, i))
    : [new Date(currentDate)]

  function getAppointmentsForDayAndHour(day: Date, hour: number) {
    return appointments.filter(a => {
      const d = new Date(a.startTime)
      return d.getDate() === day.getDate()
        && d.getMonth() === day.getMonth()
        && d.getHours() === hour
    })
  }

  function navigate(direction: 1 | -1) {
    if (mode === 'day') setCurrentDate(d => addDays(d, direction))
    else setCurrentDate(d => addDays(d, direction * 7))
  }

  const DAY_LABELS = ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb']

  return (
    <div className="flex flex-col gap-4 h-full">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-xs font-display uppercase tracking-[0.2em] text-accent mb-1">BarberAgenda</p>
          <h1 className="font-display font-extrabold text-4xl uppercase leading-none">Minha Agenda</h1>
        </div>
        <div className="flex items-center gap-2">
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
          <button onClick={() => navigate(1)} className="text-text-secondary hover:text-text-primary p-1">
            <ChevronRight size={18} />
          </button>
        </div>
      </div>

      {isLoading ? (
        <div className="flex justify-center py-10"><Spinner /></div>
      ) : (
        <div className="overflow-auto flex-1 border border-border rounded-sm">
          <table className="w-full border-collapse text-xs font-body">
            <thead>
              <tr className="bg-bg-surface">
                <th className="w-14 border-b border-r border-border px-2 py-2 text-text-secondary font-normal">Hora</th>
                {days.map(day => (
                  <th key={day.toISOString()} className="border-b border-r border-border px-2 py-2 text-center min-w-[100px]">
                    <p className="text-text-secondary font-normal">{DAY_LABELS[day.getDay()]}</p>
                    <p className={`font-bold text-sm ${
                      day.toDateString() === new Date().toDateString() ? 'text-accent' : 'text-text-primary'
                    }`}>
                      {day.getDate()}
                    </p>
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {HOURS.map(hour => (
                <tr key={hour} className="border-b border-border">
                  <td className="border-r border-border px-2 py-1 text-text-secondary text-right whitespace-nowrap">
                    {String(hour).padStart(2, '0')}:00
                  </td>
                  {days.map(day => {
                    const appts = getAppointmentsForDayAndHour(day, hour)
                    return (
                      <td key={day.toISOString()} className="border-r border-border px-1 py-1 align-top min-h-[40px]">
                        {appts.map(a => {
                          const start = new Date(a.startTime)
                          const end = new Date(a.endTime)
                          const statusNum = a.status === 'WaitingPayment' ? 0 : a.status === 'Paid' ? 1 : 2
                          return (
                            <div key={a.id} className="bg-accent/10 border border-accent/30 rounded-sm px-1.5 py-1 mb-0.5">
                              <p className="text-text-primary font-medium truncate">
                                {start.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}–{end.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}
                              </p>
                              <AppointmentBadge status={statusNum as 0 | 1 | 2} />
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
```

- [ ] **Step 2: Add route**

In `router/index.tsx`, add import:
```ts
import BarberSchedule from '../pages/barber/Schedule'
```

Add to barber children:
```ts
{ path: 'schedule', element: <BarberSchedule /> },
```

- [ ] **Step 3: Commit**

```bash
git add frontend/src/pages/barber/Schedule.tsx frontend/src/router/index.tsx
git commit -m "feat(frontend): add barber Schedule day/week view"
```

---

### Task 26: Admin Schedule page

**Files:**
- Create: `frontend/src/pages/admin/Schedule.tsx`
- Modify: `frontend/src/router/index.tsx`

- [ ] **Step 1: Create admin Schedule page**

```tsx
// frontend/src/pages/admin/Schedule.tsx
import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import { appointmentsApi } from '../../api/appointments'
import { barbersApi } from '../../api/barbers'
import { AppointmentBadge } from '../../components/ui/Badge'
import { Spinner } from '../../components/ui/Spinner'
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

const HOURS = Array.from({ length: 14 }, (_, i) => i + 8) // 08:00 to 21:00
const DAY_LABELS = ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb']

export default function AdminSchedule() {
  const [mode, setMode] = useState<ViewMode>('day')
  const [currentDate, setCurrentDate] = useState(new Date())
  const [selectedBarberId, setSelectedBarberId] = useState<number | 'all'>('all')

  const { data: barbers = [] } = useQuery({
    queryKey: ['barbers'],
    queryFn: barbersApi.getActive,
  })

  const weekStart = getWeekStart(currentDate)
  const dayStart = new Date(currentDate)
  dayStart.setHours(0, 0, 0, 0)

  const from = (mode === 'week' ? weekStart : dayStart).toISOString()
  const to = (mode === 'week' ? addDays(weekStart, 7) : addDays(dayStart, 1)).toISOString()

  const activeBarbers = selectedBarberId === 'all'
    ? barbers
    : barbers.filter(b => b.id === selectedBarberId)

  const queries = useQuery({
    queryKey: ['admin-schedule', activeBarbers.map(b => b.id), from, to],
    queryFn: async () => {
      const results = await Promise.all(
        activeBarbers.map(b => appointmentsApi.getByBarber(b.id, from, to))
      )
      return activeBarbers.map((b, i) => ({ barber: b, appointments: results[i] }))
    },
    enabled: activeBarbers.length > 0,
  })

  const barberData = queries.data ?? []

  function navigate(direction: 1 | -1) {
    if (mode === 'day') setCurrentDate(d => addDays(d, direction))
    else setCurrentDate(d => addDays(d, direction * 7))
  }

  const days = mode === 'week'
    ? Array.from({ length: 7 }, (_, i) => addDays(weekStart, i))
    : [new Date(currentDate)]

  function getApptsForBarberDayHour(barber: Barber, day: Date, hour: number) {
    const entry = barberData.find(e => e.barber.id === barber.id)
    if (!entry) return []
    return entry.appointments.filter(a => {
      const d = new Date(a.startTime)
      return d.getDate() === day.getDate()
        && d.getMonth() === day.getMonth()
        && d.getHours() === hour
    })
  }

  return (
    <div className="flex flex-col gap-4">
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div>
          <p className="text-xs font-display uppercase tracking-[0.2em] text-accent mb-1">BarberAgenda</p>
          <h1 className="font-display font-extrabold text-4xl uppercase leading-none">Agenda Geral</h1>
        </div>
        <div className="flex items-center gap-2 flex-wrap">
          <select
            value={String(selectedBarberId)}
            onChange={e => setSelectedBarberId(e.target.value === 'all' ? 'all' : Number(e.target.value))}
            className="bg-bg-elevated border border-border rounded-sm px-3 py-1.5 text-sm font-body text-text-primary"
          >
            <option value="all">Todos os barbeiros</option>
            {barbers.map(b => (
              <option key={b.id} value={b.id}>{b.displayName}</option>
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
          <button onClick={() => navigate(1)} className="text-text-secondary hover:text-text-primary p-1">
            <ChevronRight size={18} />
          </button>
        </div>
      </div>

      {queries.isLoading ? (
        <div className="flex justify-center py-10"><Spinner /></div>
      ) : (
        <div className="overflow-auto border border-border rounded-sm">
          <table className="w-full border-collapse text-xs font-body">
            <thead>
              <tr className="bg-bg-surface">
                <th className="w-14 border-b border-r border-border px-2 py-2 text-text-secondary font-normal">Hora</th>
                {mode === 'week'
                  ? days.map(day => (
                      <th key={day.toISOString()} colSpan={activeBarbers.length} className="border-b border-r border-border px-2 py-2 text-center">
                        <p className="text-text-secondary font-normal">{DAY_LABELS[day.getDay()]}</p>
                        <p className={`font-bold text-sm ${day.toDateString() === new Date().toDateString() ? 'text-accent' : 'text-text-primary'}`}>
                          {day.getDate()}
                        </p>
                      </th>
                    ))
                  : activeBarbers.map(b => (
                      <th key={b.id} className="border-b border-r border-border px-2 py-2 text-center min-w-[120px]">
                        <p className="font-bold text-text-primary">{b.displayName}</p>
                      </th>
                    ))
                }
              </tr>
            </thead>
            <tbody>
              {HOURS.map(hour => (
                <tr key={hour} className="border-b border-border">
                  <td className="border-r border-border px-2 py-1 text-text-secondary text-right whitespace-nowrap">
                    {String(hour).padStart(2, '0')}:00
                  </td>
                  {mode === 'day'
                    ? activeBarbers.map(b => {
                        const appts = getApptsForBarberDayHour(b, days[0], hour)
                        return (
                          <td key={b.id} className="border-r border-border px-1 py-1 align-top">
                            {appts.map(a => {
                              const start = new Date(a.startTime)
                              const end = new Date(a.endTime)
                              const statusNum = a.status === 'WaitingPayment' ? 0 : a.status === 'Paid' ? 1 : 2
                              return (
                                <div key={a.id} className="bg-accent/10 border border-accent/30 rounded-sm px-1.5 py-1 mb-0.5">
                                  <p className="text-text-primary font-medium truncate">
                                    {start.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}–{end.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}
                                  </p>
                                  <AppointmentBadge status={statusNum as 0 | 1 | 2} />
                                </div>
                              )
                            })}
                          </td>
                        )
                      })
                    : days.map(day =>
                        activeBarbers.map(b => {
                          const appts = getApptsForBarberDayHour(b, day, hour)
                          return (
                            <td key={`${day.toISOString()}-${b.id}`} className="border-r border-border px-1 py-1 align-top">
                              {appts.map(a => {
                                const start = new Date(a.startTime)
                                const statusNum = a.status === 'WaitingPayment' ? 0 : a.status === 'Paid' ? 1 : 2
                                return (
                                  <div key={a.id} className="bg-accent/10 border border-accent/30 rounded-sm px-1 py-0.5 mb-0.5">
                                    <p className="text-text-primary truncate text-[10px]">
                                      {b.displayName} {start.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}
                                    </p>
                                    <AppointmentBadge status={statusNum as 0 | 1 | 2} />
                                  </div>
                                )
                              })}
                            </td>
                          )
                        })
                      )
                  }
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
```

- [ ] **Step 2: Add route and update sidebar**

In `router/index.tsx`, add import:
```ts
import AdminSchedule from '../pages/admin/Schedule'
```

Add to admin children:
```ts
{ path: 'schedule', element: <AdminSchedule /> },
```

In `Sidebar.tsx`, update the `links` array to include all new sections (replace existing links):

```ts
import { LayoutDashboard, Calendar, Users, LogOut, Scissors, User2, ChevronLeft, ChevronRight, Clock } from 'lucide-react'

const links = [
  { to: '/admin', label: 'Dashboard', Icon: LayoutDashboard, end: true },
  { to: '/admin/schedule', label: 'Agenda', Icon: Calendar, end: false },
  { to: '/admin/appointments', label: 'Agendamentos', Icon: Clock, end: false },
  { to: '/admin/barbers', label: 'Barbeiros', Icon: User2, end: false },
  { to: '/admin/services', label: 'Serviços', Icon: Scissors, end: false },
  { to: '/admin/users', label: 'Usuários', Icon: Users, end: false },
]
```

- [ ] **Step 3: Commit**

```bash
git add frontend/src/pages/admin/Schedule.tsx frontend/src/router/index.tsx frontend/src/components/layout/Sidebar.tsx
git commit -m "feat(frontend): add Admin Schedule page and update sidebar nav"
```

---

### Task 27: Update Admin Dashboard stats

The admin dashboard currently queries `appointmentsApi.getByUser(userId)` which returns the admin's own appointments. After this slice, update it to pull appointments from all barbers.

**Files:**
- Modify: `frontend/src/pages/admin/Dashboard.tsx`

- [ ] **Step 1: Update dashboard to aggregate across barbers**

In `frontend/src/pages/admin/Dashboard.tsx`, replace the single appointments query with a multi-barber aggregate:

```tsx
// Replace the single useQuery with:
const { data: barbers = [] } = useQuery({
  queryKey: ['barbers'],
  queryFn: barbersApi.getActive,
})

const now = new Date()
const weekStart = new Date(now)
weekStart.setDate(now.getDate() - now.getDay())
weekStart.setHours(0, 0, 0, 0)
const weekEnd = new Date(weekStart)
weekEnd.setDate(weekStart.getDate() + 7)

const { data: allAppointments = [], isLoading } = useQuery({
  queryKey: ['admin-all-appointments', barbers.map(b => b.id)],
  queryFn: async () => {
    const results = await Promise.all(
      barbers.map(b => appointmentsApi.getByBarber(
        b.id,
        new Date(now.getFullYear(), now.getMonth(), now.getDate()).toISOString(),
        weekEnd.toISOString()
      ))
    )
    return results.flat()
  },
  enabled: barbers.length > 0,
})
```

Add import at top:
```ts
import { barbersApi } from '../../api/barbers'
```

- [ ] **Step 2: Commit**

```bash
git add frontend/src/pages/admin/Dashboard.tsx
git commit -m "feat(frontend): update Admin Dashboard to aggregate appointments across all barbers"
```

---

### Task 28: Self-review checklist — verify full system

- [ ] **Step 1: Run frontend type check**

```bash
cd frontend && npx tsc --noEmit
```

Expected: 0 errors.

- [ ] **Step 2: Build backend**

```bash
dotnet build src/BarberShop.Api/BarberShop.Api.csproj
```

Expected: 0 errors.

- [ ] **Step 3: Start both servers and do manual E2E verification**

Start backend:
```bash
dotnet run --project src/BarberShop.Api
```

Start frontend (new terminal):
```bash
cd frontend && npm run dev
```

Verify the following flows manually:

**Admin flow:**
1. Login as admin → redirected to `/admin`
2. Navigate to `/admin/services` → create a service (e.g., "Corte", 30min, R$35)
3. Navigate to `/admin/barbers` → create a barber
4. Navigate to `/admin/schedule` → shows calendar with barber column

**Barber flow:**
1. Login as barber (after setting password via reset-password email) → redirected to `/barber`
2. Navigate to `/barber/working-hours` → configure Mon-Sat 09:00-18:00 → save
3. Navigate to `/barber/blocks` → create a block for tomorrow morning
4. Navigate to `/barber/schedule` → shows own agenda

**Client flow:**
1. Go to `/book` → step 0 shows service cards
2. Select service → step 1 shows barbers
3. Select barber → step 2 shows calendar (days where barber is closed are disabled)
4. Select date → step 3 shows available slots (blocked time is not shown)
5. Select slot → step 4 shows confirmation summary
6. Login if needed → confirm → appointment created

- [ ] **Step 4: Final commit**

```bash
git add -A
git commit -m "chore: verify all 4 slices integrated and working"
```

---

**All 4 slices complete.**

The system now supports:
- Services with configurable duration and price
- Multiple barbers with individual working hours and schedule blocks
- Booking flow: service → barber → date → time → confirm
- Barber panel: dashboard, agenda, working hours, blocks
- Admin panel: dashboard, schedule (all barbers), barbers CRUD, services CRUD
