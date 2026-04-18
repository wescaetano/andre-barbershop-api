# BarberAgenda Frontend — Design Spec

**Date:** 2026-04-18  
**Status:** Approved  

---

## 1. Overview

Build a complete React + Vite frontend for the BarberAgenda system — a barbershop appointment platform. The frontend integrates 100% with the existing .NET 8 backend API. Two distinct user areas share a single SPA: **clients** (book and view appointments) and **admins** (manage appointments and users).

**MVP Scope:** Login → book appointment → view appointments. Payment flow (Mercado Pago) is out of scope for this iteration.

---

## 2. Technical Stack

| Concern | Choice | Reason |
|---------|--------|--------|
| Framework | React 18 + Vite + TypeScript | Fast dev server, modern React, type safety |
| Routing | React Router v6 | Role-based guards, nested layouts |
| HTTP | Axios | Interceptors for auth injection and token refresh |
| Global state | Zustand + persist middleware | Lightweight, auth state persisted to localStorage |
| Server state | TanStack Query v5 | Caching, background refetch, loading/error states |
| Forms | React Hook Form + Zod | Validation aligned with backend FluentValidation rules |
| Styling | Tailwind CSS + custom design tokens | Rapid development with full design control |
| Icons | Lucide React | Thin stroke, consistent with urban aesthetic |

---

## 3. Project Structure

```
src/
├── api/
│   ├── client.ts           # Axios instance + request/response interceptors
│   ├── auth.ts             # login, refresh-token, send-reset-password, reset-password
│   ├── appointments.ts     # available-slots, create, cancel, list by user
│   └── users.ts            # CRUD + paginated list (admin)
├── components/
│   ├── ui/                 # Button, Input, Card, Badge, Modal, Toast, Spinner
│   ├── layout/             # ClientLayout, AdminLayout, BottomNav, Sidebar
│   └── auth/               # PrivateRoute, AdminRoute
├── hooks/
│   ├── useAuth.ts          # login, logout, token refresh helpers
│   └── useApiError.ts      # responseLabel → Portuguese message
├── pages/
│   ├── Login.tsx
│   ├── client/
│   │   ├── Home.tsx
│   │   ├── Book.tsx
│   │   ├── Appointments.tsx
│   │   └── Profile.tsx
│   └── admin/
│       ├── Dashboard.tsx
│       ├── Appointments.tsx
│       ├── Users.tsx
│       └── UserDetail.tsx
├── router/
│   └── index.tsx           # Route definitions + role guards
├── store/
│   └── authStore.ts        # Zustand store: userId, tokens, modulesAssembled
└── types/
    ├── auth.ts
    ├── appointment.ts
    └── user.ts
```

---

## 4. Routing & Guards

```
/login                  → public, redirect to /app if authenticated
/app                    → PrivateRoute (requires valid token)
  /app                  → ClientHome
  /app/book             → Book (3-step wizard)
  /app/appointments     → Client appointment list
  /app/profile          → User profile
/admin                  → AdminRoute (requires Users module permission)
  /admin                → AdminDashboard
  /admin/appointments   → Admin appointment list
  /admin/users          → Users list
  /admin/users/:id      → User detail
```

**PrivateRoute:** reads token from Zustand store. If missing → redirect to `/login`.  
**AdminRoute:** additionally checks `modulesAssembled` contains `Users` key. If absent → redirect to `/app`.  
**Role detection:** after login, if `modulesAssembled.Users` exists → admin, else → client.

---

## 5. Authentication Flow

### Login
```
POST /auth/login
Body: { login: string, password: string }
Response: { userId, userName, userEmail, accessToken, refreshToken, modulesAssembled, dtExpiration }
```
- Store all fields in Zustand (persisted to localStorage)
- Decode role from `modulesAssembled`
- Redirect: admin → `/admin`, client → `/app`

### Axios Interceptors (`src/api/client.ts`)
- **Request interceptor:** attach `Authorization: Bearer <accessToken>`
- **Response interceptor (401):** call `POST /auth/refresh-token?token=<refreshToken>` → update store with new tokens → retry original request. If refresh fails → `authStore.logout()` → navigate to `/login`

### Logout
- Clear Zustand store + localStorage
- Navigate to `/login`

### Forgot Password
- `POST /auth/send-reset-password?email=<email>` → show success toast
- Reset link in email → `/reset-password?token=<token>` → `POST /auth/reset-password`

---

## 6. Screen Specifications

### 6.1 Login (`/login`)
- Full-screen, centered card on mobile
- Logo "BARBER**AGENDA**" in Barlow Condensed at top
- Email + password inputs
- "ENTRAR" button (primary, full-width)
- "Esqueci minha senha" link → modal/page for email input
- Error toast on invalid credentials

### 6.2 Client Home (`/app`)
- Greeting: "Olá, {userName}" in display font
- Section: próximos agendamentos (max 3, ordered by date)
  - Cards with left red border accent
  - Shows: date, time, status badge
  - Cancelled appointments visually dimmed
- Prominent "AGENDAR AGORA" CTA button full-width
- If no appointments → empty state with illustration and CTA

### 6.3 Book Appointment (`/app/book`) — 3-step wizard
- **Step 1 — Data:** calendar date picker (disables past dates)
  - Calls `GET /appointment/available-slots?date=` on date change
- **Step 2 — Horário:** grid of available slots (30-min intervals, 09:00–18:00)
  - Occupied slots disabled and visually struck through
- **Step 3 — Confirmação:** summary card → "CONFIRMAR" button
  - Calls `POST /appointment` with `{ userId, date, startTime }`
  - Success → redirect to `/app/appointments` with success toast
- Step indicator at top (1/2/3 with connecting line)

### 6.4 My Appointments (`/app/appointments`)
- List grouped by status (upcoming / past / cancelled)
- Each card: date, time, status badge
- Cancel button on WaitingPayment appointments → modal confirmation → `PATCH /appointment/cancel`
- Pull-to-refresh (TanStack Query refetch)

### 6.5 Admin Dashboard (`/admin`)
- Stats row: agendamentos hoje / esta semana
- Recent appointments list (last 10)

### 6.6 Admin Appointments (`/admin/appointments`)
- Table on desktop, cards on mobile
- Filters: date range, status
- Calls `GET /appointment/user/{userId}` (all users if needed, or adapt)

### 6.7 Admin Users (`/admin/users`)
- Paginated list with search by name/email, filter by status
- Calls `GET /user` with query params
- Actions per row: edit (modal), toggle status, delete (with confirmation)
- "NOVO USUÁRIO" button → modal with `POST /user`

### 6.8 Admin User Detail (`/admin/users/:id`)
- Full user info
- Appointment history

---

## 7. Visual Identity

### Color Tokens (Tailwind config)
```js
colors: {
  bg: {
    base: '#0D0D0D',
    surface: '#1A1A1A',
    elevated: '#242424',
  },
  accent: {
    DEFAULT: '#E63329',
    hover: '#FF4540',
  },
  text: {
    primary: '#F5F5F5',
    secondary: '#888888',
  },
  border: '#2E2E2E',
}
```

### Typography
- **Display/Headlines:** Barlow Condensed (700, 800) — condensed, strong, urban
- **Body/UI:** DM Sans (400, 500, 600) — clean, legible

### Visual Details
- Diagonal `clip-path` cuts on section headers and page banners
- Cards: `border-l-[3px] border-accent` as accent detail
- Buttons: uppercase, font-weight 700, border-radius 2px (nearly square)
- Animations: 150ms ease-out — snappy, not floaty
- No shadows on dark surfaces; use border-based elevation instead
- Status badges: `WaitingPayment` → yellow, `Paid` → green, `Cancelled` → red/dimmed

---

## 8. Error Handling

### API Response Shape
```ts
interface ApiResponse<T> {
  success: boolean
  statusCode: number
  responseLabel: string  // EResponseLabel enum
  description?: string
  data?: T
}
```

### responseLabel → Portuguese Messages
```ts
NOT_FOUND         → "Registro não encontrado"
FORBIDDEN         → "Sem permissão para esta ação"
INVALID_MODEL     → "Dados inválidos. Verifique os campos."
ALREADY_EXISTS    → "Registro já cadastrado"
INVALID_CREDENTIALS → "E-mail ou senha incorretos"
BAD_REQUEST       → "Requisição inválida"
```

### Toast System
- Custom `useToast` hook + `Toast` component (no external library)
- Top-right position on desktop, top-center on mobile
- Auto-dismiss after 4s
- Types: success (green border), error (red border), info (neutral)

---

## 9. TypeScript Types

```ts
// Auth
interface LoginResponse {
  userId: number
  userName: string
  userEmail: string
  userProfileImage?: string
  accessToken: string
  refreshToken: string
  modulesAssembled: Record<string, ModulePermissions>
  dtCreation: string
  dtExpiration: string
}

interface ModulePermissions {
  Visualize: boolean
  Edit: boolean
  Register: boolean
  Inactivate: boolean
  Exclude: boolean
}

// Appointment
interface Appointment {
  id: number
  userId: number
  startTime: string   // ISO datetime
  endTime: string
  status: 0 | 1 | 2  // WaitingPayment | Paid | Cancelled
  creationDate: string
}

// User
interface User {
  id: number
  name: string
  email: string
  imageUrl?: string
  status: 0 | 1       // Inativo | Ativo
  creationDate: string
}
```

---

## 10. Out of Scope (MVP)

- Mercado Pago payment flow (checkout URL redirect, payment status polling)
- Social login (Google/Apple)
- Push notifications
- PWA manifest / offline support
- Image upload for user profiles

---

## 11. Backend API Reference

**Base URL:** `http://localhost:5000` (dev) — configure via `VITE_API_URL` env var

| Endpoint | Method | Auth | Purpose |
|----------|--------|------|---------|
| `/auth/login` | POST | No | Login |
| `/auth/refresh-token` | POST | No | Refresh tokens |
| `/auth/send-reset-password` | POST | No | Send reset email |
| `/auth/reset-password` | POST | No | Reset password |
| `/appointment/available-slots` | GET | Yes | Available time slots |
| `/appointment` | POST | Yes | Create appointment |
| `/appointment/cancel` | PATCH | Yes | Cancel appointment |
| `/appointment/user/{userId}` | GET | Yes | List user appointments |
| `/user` | GET | Yes (admin) | List users paginated |
| `/user` | POST | Yes (admin) | Create user |
| `/user` | PUT | Yes (admin) | Update user |
| `/user/status` | PATCH | Yes (admin) | Toggle user status |
| `/user/{id}` | GET | Yes (admin) | Get user detail |
| `/user/{id}` | DELETE | Yes (admin) | Delete user |
