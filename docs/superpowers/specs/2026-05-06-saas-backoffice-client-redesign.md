# Design: SaaS Backoffice + Client Panel Redesign

**Date:** 2026-05-06  
**Status:** Approved

---

## 1. Overview

Expand the BarberAgenda system from a single-role appointment app into a structured SaaS with three distinct panels: **Admin** (full control), **Barber** (own schedule), and **Client** (booking). The work is organized as four independent feature slices, each delivering working backend + frontend together.

**Scope decisions:**
- Single barbershop (not multi-tenant)
- Multiple barbers, each with their own schedule
- Client chooses: service → barber → date → time
- Appointments are auto-confirmed (no payment required this iteration)
- Payment flow (Mercado Pago) is deferred to a future iteration

---

## 2. Roles

| Role | Login | Access |
|------|-------|--------|
| Admin | Yes | Full system: services, barbers, all schedules, all appointments |
| Barber | Yes | Own schedule, working hours, blocks, own appointments |
| Client | Yes (or guest for browsing) | Booking + own appointments |

Role detection after login uses the existing `modulesAssembled` mechanism. A new `Barber` module is added to the access control system.

---

## 3. Data Model

### New Entities

```
Service
  Id            long (PK)
  Name          string
  DurationMinutes int
  Price         decimal
  IsActive      bool
  CreatedAt     datetime

Barber
  Id            long (PK)
  UserId        long (FK → User)
  DisplayName   string
  IsActive      bool
  CreatedAt     datetime

WorkingHours
  Id            long (PK)
  BarberId      long (FK → Barber)
  DayOfWeek     int (0 = Sunday … 6 = Saturday)
  OpenTime      TimeOnly
  CloseTime     TimeOnly
  IsOpen        bool

ScheduleBlock
  Id            long (PK)
  BarberId      long (FK → Barber)
  StartTime     datetime
  EndTime       datetime
  Reason        string? (optional)
```

### Modified Entities

```
Appointment (adds)
  ServiceId     long (FK → Service)
  BarberId      long (FK → Barber)
```

### Available Slots Logic (updated)

```
Input: date, barberId, serviceId

1. Load WorkingHours for barberId on date.DayOfWeek
   → if IsOpen = false: return empty list
2. Generate slots from OpenTime to CloseTime with step = Service.DurationMinutes
3. Remove slots occupied by existing Appointments for this barber on this date
   (status ≠ Cancelled)
4. Remove slots that overlap any ScheduleBlock for this barber
5. Remove slots in the past (if date = today)
6. Return remaining slots as ["HH:mm", ...]
```

---

## 4. Routing

```
/                  → RootRedirect (by role)
/login             → public
/book              → public (guest browsing)

/app/*             → PrivateRoute (role: client)
  /app             → ClientHome
  /app/appointments → ClientAppointments
  /app/profile      → Profile

/barber/*          → BarberRoute (role: barber) — NEW
  /barber          → BarberDashboard
  /barber/schedule → BarberSchedule (day/week view)
  /barber/working-hours → WorkingHours config
  /barber/blocks   → ScheduleBlocks

/admin/*           → AdminRoute (role: admin)
  /admin           → AdminDashboard
  /admin/appointments → AdminAppointments
  /admin/users     → Users
  /admin/users/:id → UserDetail
  /admin/services  → Services (NEW)
  /admin/barbers   → Barbers (NEW)
  /admin/schedule  → AdminSchedule (all barbers, NEW)
```

**RootRedirect logic:**
```
not authenticated → /book
role = admin      → /admin
role = barber     → /barber
role = client     → /app
```

---

## 5. Feature Slices

### Slice 1 — Services

**Goal:** Admin can create and manage services. Client sees services on step 1 of booking.

**Backend:**
- `Service` entity + EF migration
- `GET /service` — list active services (public)
- `GET /service/all` — list all (admin)
- `POST /service` — create (admin)
- `PUT /service` — update (admin)
- `PATCH /service/status` — activate/inactivate (admin)

**Frontend — Admin (`/admin/services`):**
- Table: Name | Duration | Price | Status
- "Novo Serviço" button → modal with form (name, duration in minutes, price)
- Edit action → same modal pre-filled
- Toggle status with confirmation

**Frontend — Client (Book step 1):**
- Grid of service cards: name, duration, price
- Selected card highlighted with `border-accent`
- `GET /service` called on mount

---

### Slice 2 — Barbers

**Goal:** Admin manages barber users. Client selects a barber in booking step 2.

**Backend:**
- `Barber` entity + EF migration
- New `Barber` module in access control
- `GET /barber` — list active barbers (public)
- `GET /barber/all` — list all (admin)
- `POST /barber` — create barber user (admin): creates User + Barber record atomically
- `PUT /barber/:id` — update (admin)
- `PATCH /barber/status` — activate/inactivate (admin)
- `BarberRoute` guard on frontend (checks `modulesAssembled` for `Barber` key)

**Frontend — Admin (`/admin/barbers`):**
- Table: Name | Email | Status
- "Novo Barbeiro" → modal: name, email, temp password, display name
- Edit + toggle status

**Frontend — Client (Book step 2):**
- Grid of barber cards: avatar (initials fallback), display name
- Selected card highlighted

**Frontend — Routing:**
- Add `BarberRoute` guard (mirrors `AdminRoute` but checks for `Barber` module)
- Update `RootRedirect` to handle barber role
- Scaffold barber panel layout (mirrors `AdminLayout` but simpler sidebar)

---

### Slice 3 — Working Hours & Schedule Blocks

**Goal:** Each barber configures their own working hours and can block specific times. Slot generation respects these rules.

**Backend:**
- `WorkingHours` + `ScheduleBlock` entities + EF migration
- `GET /barber/:id/working-hours` — get working hours (public, needed for calendar)
- `PUT /barber/working-hours` — upsert full week config (barber or admin)
- `GET /barber/:id/blocks?from=&to=` — list blocks in range (barber or admin)
- `POST /barber/blocks` — create block (barber or admin)
- `DELETE /barber/blocks/:id` — remove block (barber or admin)
- Update `GetAvailableSlots` to consume working hours + blocks + service duration

**Frontend — Barber (`/barber/working-hours`):**
- 7-row table (Mon–Sun)
- Each row: day name | toggle On/Off | time inputs (open → close)
- Disabled inputs when toggle is Off
- Save button → `PUT /barber/working-hours`

**Frontend — Barber (`/barber/blocks`):**
- List of future blocks: date range | reason
- "Bloquear horário" button → modal: date, start time, end time, reason (optional)
- Delete button per block with confirmation
- Past blocks not shown

**Frontend — Client (Book step 3):**
- Calendar disables dates where barber has `IsOpen = false` for that day of week
- Slot grid generated from updated endpoint

---

### Slice 4 — Agenda View

**Goal:** Barbers and admin can visualize appointments in a day/week calendar.

**Backend:**
- `GET /appointment/barber/:id?from=&to=` — appointments for a barber in a date range (barber sees own, admin sees any)

**Frontend — Barber (`/barber/schedule`):**
- Toggle: day view / week view
- Day view: time column (rows = 30min intervals) + appointments as cards
- Week view: 7 columns (Mon–Sun) + time rows
- Appointment card: client name, service name, time, status badge
- Clicking a card shows detail modal

**Frontend — Admin (`/admin/schedule`):**
- Same day/week structure but with one column per barber
- Filter by barber (show all or single barber)
- Read-only view (no drag-and-drop in MVP)

---

## 6. API Reference (new endpoints)

| Endpoint | Method | Auth | Purpose |
|----------|--------|------|---------|
| `/service` | GET | No | List active services |
| `/service/all` | GET | Admin | List all services |
| `/service` | POST | Admin | Create service |
| `/service` | PUT | Admin | Update service |
| `/service/status` | PATCH | Admin | Toggle service status |
| `/barber` | GET | No | List active barbers |
| `/barber/all` | GET | Admin | List all barbers |
| `/barber` | POST | Admin | Create barber |
| `/barber/:id` | PUT | Admin | Update barber |
| `/barber/status` | PATCH | Admin | Toggle barber status |
| `/barber/:id/working-hours` | GET | No | Get working hours |
| `/barber/working-hours` | PUT | Barber/Admin | Upsert working hours |
| `/barber/:id/blocks` | GET | Barber/Admin | List blocks |
| `/barber/blocks` | POST | Barber/Admin | Create block |
| `/barber/blocks/:id` | DELETE | Barber/Admin | Delete block |
| `/appointment/available-slots` | GET | No | Slots (now requires barberId + serviceId) |
| `/appointment/barber/:id` | GET | Barber/Admin | Barber's appointments |

---

## 7. Updated Booking Flow (Client)

```
Step 1 — Serviço
  GET /service → grid of service cards
  Select one → enables Step 2

Step 2 — Barbeiro
  GET /barber → grid of barber cards
  Select one → enables Step 3

Step 3 — Data & Horário
  Calendar: days where barber.workingHours[day].IsOpen = false are disabled
  On date select: GET /appointment/available-slots?date=&barberId=&serviceId=
  Slot grid: available slots shown as buttons, occupied/blocked shown as disabled

Step 4 — Confirmação
  Summary: service | barber | date | time | price
  CONFIRMAR → POST /appointment { userId?, barberId, serviceId, date, startTime }
  If not authenticated → auth gate modal (existing behavior)
  Success → /app/appointments + success toast
```

---

## 8. TypeScript Types (additions)

```ts
interface Service {
  id: number
  name: string
  durationMinutes: number
  price: number
  isActive: boolean
}

interface Barber {
  id: number
  userId: number
  displayName: string
  isActive: boolean
}

interface WorkingHours {
  id: number
  barberId: number
  dayOfWeek: number  // 0 = Sunday
  openTime: string   // "HH:mm"
  closeTime: string  // "HH:mm"
  isOpen: boolean
}

interface ScheduleBlock {
  id: number
  barberId: number
  startTime: string  // ISO datetime
  endTime: string    // ISO datetime
  reason?: string
}

// Appointment updated
interface Appointment {
  id: number
  userId: number
  barberId: number    // added
  serviceId: number   // added
  startTime: string
  endTime: string
  status: 0 | 1 | 2
  creationDate: string
  service?: Service   // optional populated relation
  barber?: Barber     // optional populated relation
}
```

---

## 9. Visual Identity

No changes to the existing design system (dark theme, accent red, Barlow Condensed + DM Sans). New screens follow the same patterns established in `AdminDashboard` and `AdminLayout`.

Barber panel uses the same `AdminLayout` structure with a narrower sidebar containing only barber-relevant links.

---

## 10. Out of Scope

- Payment flow (Mercado Pago) — deferred
- "Any available barber" option in booking — future iteration
- Drag-and-drop rescheduling in agenda view
- Push/email notifications on new booking
- Multi-tenant (multiple barbershops)
- Client-facing cancellation policy enforcement
