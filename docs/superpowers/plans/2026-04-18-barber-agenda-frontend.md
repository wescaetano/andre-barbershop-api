# BarberAgenda Frontend Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a complete React + Vite frontend for BarberAgenda with client booking area and admin management area, integrated 100% with the existing .NET 8 backend.

**Architecture:** Single SPA (React + Vite + TypeScript) at `frontend/`. Role detection via JWT `modulesAssembled` field post-login. Axios interceptors handle token injection and silent refresh. Zustand persists auth state to localStorage.

**Tech Stack:** React 18, Vite, TypeScript, React Router v6, Axios, Zustand, TanStack Query v5, React Hook Form, Zod, Tailwind CSS, Lucide React, Vitest, React Testing Library

---

## File Map

```
frontend/
├── .env.example
├── index.html
├── vite.config.ts
├── tailwind.config.ts
├── src/
│   ├── main.tsx
│   ├── App.tsx
│   ├── types/
│   │   ├── auth.ts
│   │   ├── appointment.ts
│   │   └── user.ts
│   ├── api/
│   │   ├── client.ts
│   │   ├── auth.ts
│   │   ├── appointments.ts
│   │   └── users.ts
│   ├── store/
│   │   └── authStore.ts
│   ├── hooks/
│   │   ├── useAuth.ts
│   │   └── useApiError.ts
│   ├── router/
│   │   ├── index.tsx
│   │   ├── PrivateRoute.tsx
│   │   └── AdminRoute.tsx
│   ├── components/
│   │   ├── ui/
│   │   │   ├── Button.tsx
│   │   │   ├── Input.tsx
│   │   │   ├── Badge.tsx
│   │   │   ├── Spinner.tsx
│   │   │   ├── Modal.tsx
│   │   │   └── Toast.tsx
│   │   └── layout/
│   │       ├── ClientLayout.tsx
│   │       ├── BottomNav.tsx
│   │       ├── AdminLayout.tsx
│   │       └── Sidebar.tsx
│   └── pages/
│       ├── Login.tsx
│       ├── client/
│       │   ├── Home.tsx
│       │   ├── Book.tsx
│       │   ├── Appointments.tsx
│       │   └── Profile.tsx
│       └── admin/
│           ├── Dashboard.tsx
│           ├── Appointments.tsx
│           ├── Users.tsx
│           └── UserDetail.tsx
```

---

### Task 1: Scaffold Project

**Files:**
- Create: `frontend/` (entire directory)
- Create: `frontend/vite.config.ts`
- Create: `frontend/tailwind.config.ts`
- Create: `frontend/.env.example`
- Create: `frontend/src/index.css`

- [ ] **Step 1: Create Vite project**

```bash
cd d:/Repositorios/Pessoal/barber-agenda
npm create vite@latest frontend -- --template react-ts
cd frontend
npm install
```

- [ ] **Step 2: Install all dependencies**

```bash
npm install axios zustand @tanstack/react-query react-router-dom react-hook-form zod @hookform/resolvers lucide-react
npm install -D tailwindcss postcss autoprefixer vitest @vitejs/plugin-react @testing-library/react @testing-library/user-event @testing-library/jest-dom jsdom
npx tailwindcss init -p
```

- [ ] **Step 3: Configure `vite.config.ts`**

```ts
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/test/setup.ts',
  },
})
```

- [ ] **Step 4: Create test setup file `src/test/setup.ts`**

```ts
import '@testing-library/jest-dom'
```

- [ ] **Step 5: Configure `tailwind.config.ts`**

```ts
import type { Config } from 'tailwindcss'

export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
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
      },
      fontFamily: {
        display: ['"Barlow Condensed"', 'sans-serif'],
        body: ['"DM Sans"', 'sans-serif'],
      },
      borderRadius: {
        sm: '2px',
        DEFAULT: '4px',
      },
    },
  },
  plugins: [],
} satisfies Config
```

- [ ] **Step 6: Set up `src/index.css`**

```css
@import url('https://fonts.googleapis.com/css2?family=Barlow+Condensed:wght@700;800&family=DM+Sans:wght@400;500;600&display=swap');

@tailwind base;
@tailwind components;
@tailwind utilities;

@layer base {
  body {
    @apply bg-bg-base text-text-primary font-body;
    -webkit-font-smoothing: antialiased;
  }

  * {
    @apply border-border;
  }
}

@layer utilities {
  .clip-diagonal {
    clip-path: polygon(0 0, 100% 0, 100% 85%, 0 100%);
  }
}
```

- [ ] **Step 7: Create `.env.example`**

```
VITE_API_URL=http://localhost:5000
```

- [ ] **Step 8: Copy `.env.example` to `.env` and update `src/main.tsx`**

```bash
cp .env.example .env
```

`src/main.tsx`:
```tsx
import React from 'react'
import ReactDOM from 'react-dom/client'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import App from './App'
import './index.css'

const queryClient = new QueryClient({
  defaultOptions: { queries: { retry: 1, staleTime: 30_000 } },
})

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <App />
    </QueryClientProvider>
  </React.StrictMode>,
)
```

- [ ] **Step 9: Commit**

```bash
git add frontend/
git commit -m "feat(frontend): scaffold React + Vite project with Tailwind and fonts"
```

---

### Task 2: TypeScript Types

**Files:**
- Create: `frontend/src/types/auth.ts`
- Create: `frontend/src/types/appointment.ts`
- Create: `frontend/src/types/user.ts`

- [ ] **Step 1: Create `src/types/auth.ts`**

```ts
export interface ModulePermissions {
  Visualize: boolean
  Edit: boolean
  Register: boolean
  Inactivate: boolean
  Exclude: boolean
}

export interface LoginResponse {
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

export interface ApiResponse<T = unknown> {
  success: boolean
  statusCode: number
  responseLabel: string
  description?: string
  data?: T
}

export type UserRole = 'admin' | 'client'
```

- [ ] **Step 2: Create `src/types/appointment.ts`**

```ts
export type AppointmentStatus = 0 | 1 | 2 // WaitingPayment | Paid | Cancelled

export interface Appointment {
  id: number
  userId: number
  startTime: string
  endTime: string
  status: AppointmentStatus
  creationDate: string
  updateDate?: string
}

export interface CreateAppointmentRequest {
  userId: number
  date: string       // "YYYY-MM-DD"
  startTime: string  // "HH:mm"
}

export interface CancelAppointmentRequest {
  appointmentId: number
  userId: number
}
```

- [ ] **Step 3: Create `src/types/user.ts`**

```ts
export type UserStatus = 0 | 1 // Inativo | Ativo

export interface User {
  id: number
  name: string
  email: string
  imageUrl?: string
  status: UserStatus
  creationDate: string
  updateDate?: string
  profilesUsers?: { profile: { id: number; name: string } }[]
}

export interface CreateUserRequest {
  name: string
  email: string
  imageBase64?: string
  accessProfile: number
}

export interface UpdateUserRequest {
  id: number
  name?: string
  email?: string
  imageUrl?: string
  accessProfile?: number
}

export interface GetUsersPaginatedRequest {
  name?: string
  email?: string
  status?: UserStatus
  pageSize?: number
  pageNumber?: number
  sortField?: 'Id' | 'Name' | 'Email' | 'CreationDate'
  sortOrder?: 'asc' | 'desc'
}

export interface PaginatedResponse<T> {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}
```

- [ ] **Step 4: Commit**

```bash
git add frontend/src/types/
git commit -m "feat(frontend): add TypeScript types aligned with backend DTOs"
```

---

### Task 3: Axios API Client

**Files:**
- Create: `frontend/src/api/client.ts`
- Create: `frontend/src/test/mocks/server.ts`

- [ ] **Step 1: Write failing test `src/api/__tests__/client.test.ts`**

```ts
import { describe, it, expect, vi, beforeEach } from 'vitest'

describe('api client', () => {
  it('should have baseURL from env', async () => {
    const { apiClient } = await import('../client')
    expect(apiClient.defaults.baseURL).toBe(import.meta.env.VITE_API_URL || 'http://localhost:5000')
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

```bash
npx vitest run src/api/__tests__/client.test.ts
```

Expected: FAIL — "Cannot find module '../client'"

- [ ] **Step 3: Create `src/api/client.ts`**

```ts
import axios, { AxiosError } from 'axios'

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000',
  headers: { 'Content-Type': 'application/json' },
})

// Attach token to every request
apiClient.interceptors.request.use((config) => {
  const raw = localStorage.getItem('auth-storage')
  if (raw) {
    try {
      const parsed = JSON.parse(raw)
      const token = parsed?.state?.accessToken
      if (token) config.headers.Authorization = `Bearer ${token}`
    } catch {}
  }
  return config
})

// Silent refresh on 401
let isRefreshing = false
let refreshQueue: Array<(token: string) => void> = []

apiClient.interceptors.response.use(
  (res) => res,
  async (error: AxiosError) => {
    const original = error.config as typeof error.config & { _retry?: boolean }
    if (error.response?.status !== 401 || original?._retry) {
      return Promise.reject(error)
    }
    original._retry = true

    if (isRefreshing) {
      return new Promise((resolve) => {
        refreshQueue.push((token) => {
          original!.headers!.Authorization = `Bearer ${token}`
          resolve(apiClient(original!))
        })
      })
    }

    isRefreshing = true
    try {
      const raw = localStorage.getItem('auth-storage')
      const refreshToken = raw ? JSON.parse(raw)?.state?.refreshToken : null
      if (!refreshToken) throw new Error('no refresh token')

      const { data } = await axios.post(
        `${import.meta.env.VITE_API_URL || 'http://localhost:5000'}/auth/refresh-token`,
        null,
        { params: { token: refreshToken } },
      )
      const newAccessToken: string = data.data.accessToken
      const newRefreshToken: string = data.data.refreshToken

      // Update persisted store
      const stored = JSON.parse(localStorage.getItem('auth-storage') || '{}')
      stored.state.accessToken = newAccessToken
      stored.state.refreshToken = newRefreshToken
      localStorage.setItem('auth-storage', JSON.stringify(stored))

      refreshQueue.forEach((cb) => cb(newAccessToken))
      refreshQueue = []
      original!.headers!.Authorization = `Bearer ${newAccessToken}`
      return apiClient(original!)
    } catch {
      // Refresh failed — force logout
      localStorage.removeItem('auth-storage')
      window.location.href = '/login'
      return Promise.reject(error)
    } finally {
      isRefreshing = false
    }
  },
)
```

- [ ] **Step 4: Run test to verify it passes**

```bash
npx vitest run src/api/__tests__/client.test.ts
```

Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add frontend/src/api/
git commit -m "feat(frontend): add Axios client with Bearer injection and silent token refresh"
```

---

### Task 4: Auth Zustand Store

**Files:**
- Create: `frontend/src/store/authStore.ts`

- [ ] **Step 1: Write failing test `src/store/__tests__/authStore.test.ts`**

```ts
import { describe, it, expect, beforeEach } from 'vitest'
import { useAuthStore } from '../authStore'
import { act } from '@testing-library/react'

describe('authStore', () => {
  beforeEach(() => {
    act(() => useAuthStore.getState().logout())
  })

  it('should start unauthenticated', () => {
    expect(useAuthStore.getState().accessToken).toBeNull()
    expect(useAuthStore.getState().isAuthenticated).toBe(false)
  })

  it('should set auth data on setAuth', () => {
    act(() => useAuthStore.getState().setAuth({
      userId: 1,
      userName: 'Marcus',
      userEmail: 'marcus@test.com',
      accessToken: 'token123',
      refreshToken: 'refresh123',
      modulesAssembled: {},
      dtExpiration: '',
      dtCreation: '',
    }))
    expect(useAuthStore.getState().isAuthenticated).toBe(true)
    expect(useAuthStore.getState().userName).toBe('Marcus')
  })

  it('should detect admin role when Users module present', () => {
    act(() => useAuthStore.getState().setAuth({
      userId: 1,
      userName: 'Admin',
      userEmail: 'admin@test.com',
      accessToken: 'token',
      refreshToken: 'refresh',
      modulesAssembled: { Users: { Visualize: true, Edit: true, Register: true, Inactivate: true, Exclude: true } },
      dtExpiration: '',
      dtCreation: '',
    }))
    expect(useAuthStore.getState().role).toBe('admin')
  })

  it('should detect client role when Users module absent', () => {
    act(() => useAuthStore.getState().setAuth({
      userId: 2,
      userName: 'Client',
      userEmail: 'client@test.com',
      accessToken: 'token',
      refreshToken: 'refresh',
      modulesAssembled: { Appointments: { Visualize: true, Edit: false, Register: true, Inactivate: false, Exclude: false } },
      dtExpiration: '',
      dtCreation: '',
    }))
    expect(useAuthStore.getState().role).toBe('client')
  })

  it('should clear state on logout', () => {
    act(() => useAuthStore.getState().setAuth({
      userId: 1, userName: 'X', userEmail: 'x@x.com',
      accessToken: 'tok', refreshToken: 'ref',
      modulesAssembled: {}, dtExpiration: '', dtCreation: '',
    }))
    act(() => useAuthStore.getState().logout())
    expect(useAuthStore.getState().isAuthenticated).toBe(false)
    expect(useAuthStore.getState().accessToken).toBeNull()
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

```bash
npx vitest run src/store/__tests__/authStore.test.ts
```

Expected: FAIL — "Cannot find module '../authStore'"

- [ ] **Step 3: Create `src/store/authStore.ts`**

```ts
import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { LoginResponse, UserRole } from '../types/auth'

interface AuthState {
  userId: number | null
  userName: string | null
  userEmail: string | null
  userProfileImage: string | null
  accessToken: string | null
  refreshToken: string | null
  modulesAssembled: LoginResponse['modulesAssembled']
  role: UserRole | null
  isAuthenticated: boolean
  setAuth: (data: LoginResponse) => void
  logout: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      userId: null,
      userName: null,
      userEmail: null,
      userProfileImage: null,
      accessToken: null,
      refreshToken: null,
      modulesAssembled: {},
      role: null,
      isAuthenticated: false,
      setAuth: (data) =>
        set({
          userId: data.userId,
          userName: data.userName,
          userEmail: data.userEmail,
          userProfileImage: data.userProfileImage ?? null,
          accessToken: data.accessToken,
          refreshToken: data.refreshToken,
          modulesAssembled: data.modulesAssembled,
          role: 'Users' in data.modulesAssembled ? 'admin' : 'client',
          isAuthenticated: true,
        }),
      logout: () =>
        set({
          userId: null,
          userName: null,
          userEmail: null,
          userProfileImage: null,
          accessToken: null,
          refreshToken: null,
          modulesAssembled: {},
          role: null,
          isAuthenticated: false,
        }),
    }),
    { name: 'auth-storage' },
  ),
)
```

- [ ] **Step 4: Run tests to verify they pass**

```bash
npx vitest run src/store/__tests__/authStore.test.ts
```

Expected: 4 tests PASS

- [ ] **Step 5: Commit**

```bash
git add frontend/src/store/
git commit -m "feat(frontend): add Zustand auth store with role detection and persistence"
```

---

### Task 5: Auth API Functions

**Files:**
- Create: `frontend/src/api/auth.ts`
- Create: `frontend/src/hooks/useAuth.ts`
- Create: `frontend/src/hooks/useApiError.ts`

- [ ] **Step 1: Create `src/api/auth.ts`**

```ts
import { apiClient } from './client'
import type { ApiResponse, LoginResponse } from '../types/auth'

export const authApi = {
  login: async (login: string, password: string) => {
    const { data } = await apiClient.post<ApiResponse<LoginResponse>>('/auth/login', { login, password })
    return data
  },

  refreshToken: async (token: string) => {
    const { data } = await apiClient.post<ApiResponse<LoginResponse>>(
      '/auth/refresh-token',
      null,
      { params: { token } },
    )
    return data
  },

  sendResetPassword: async (email: string) => {
    const { data } = await apiClient.post<ApiResponse>('/auth/send-reset-password', null, {
      params: { email },
    })
    return data
  },

  resetPassword: async (newPassword: string, token: string) => {
    const { data } = await apiClient.post<ApiResponse>('/auth/reset-password', { newPassword, token })
    return data
  },
}
```

- [ ] **Step 2: Create `src/hooks/useApiError.ts`**

```ts
const labelMessages: Record<string, string> = {
  NOT_FOUND: 'Registro não encontrado.',
  FORBIDDEN: 'Sem permissão para esta ação.',
  INVALID_MODEL: 'Dados inválidos. Verifique os campos.',
  ALREADY_EXISTS: 'Registro já cadastrado.',
  INVALID_CREDENTIALS: 'E-mail ou senha incorretos.',
  BAD_REQUEST: 'Requisição inválida.',
  SUCCESSFUL: 'Operação realizada com sucesso.',
  SUCCESSFULCREATION: 'Criado com sucesso.',
}

export function useApiError() {
  const getMessage = (responseLabel?: string, fallback?: string): string => {
    if (!responseLabel) return fallback ?? 'Erro inesperado.'
    return labelMessages[responseLabel] ?? fallback ?? 'Erro inesperado.'
  }
  return { getMessage }
}
```

- [ ] **Step 3: Create `src/hooks/useAuth.ts`**

```ts
import { useNavigate } from 'react-router-dom'
import { useAuthStore } from '../store/authStore'
import { authApi } from '../api/auth'

export function useAuth() {
  const navigate = useNavigate()
  const { setAuth, logout: storeLogout, isAuthenticated, role, userId } = useAuthStore()

  const login = async (email: string, password: string) => {
    const res = await authApi.login(email, password)
    if (!res.success || !res.data) throw new Error(res.responseLabel)
    setAuth(res.data)
    return res.data
  }

  const logout = () => {
    storeLogout()
    navigate('/login', { replace: true })
  }

  return { login, logout, isAuthenticated, role, userId }
}
```

- [ ] **Step 4: Write test for useApiError `src/hooks/__tests__/useApiError.test.ts`**

```ts
import { describe, it, expect } from 'vitest'
import { renderHook } from '@testing-library/react'
import { useApiError } from '../useApiError'

describe('useApiError', () => {
  it('returns Portuguese message for INVALID_CREDENTIALS', () => {
    const { result } = renderHook(() => useApiError())
    expect(result.current.getMessage('INVALID_CREDENTIALS')).toBe('E-mail ou senha incorretos.')
  })

  it('returns fallback for unknown label', () => {
    const { result } = renderHook(() => useApiError())
    expect(result.current.getMessage('UNKNOWN', 'Fallback msg')).toBe('Fallback msg')
  })

  it('returns default error for undefined label', () => {
    const { result } = renderHook(() => useApiError())
    expect(result.current.getMessage(undefined)).toBe('Erro inesperado.')
  })
})
```

- [ ] **Step 5: Run tests**

```bash
npx vitest run src/hooks/__tests__/useApiError.test.ts
```

Expected: 3 tests PASS

- [ ] **Step 6: Commit**

```bash
git add frontend/src/api/auth.ts frontend/src/hooks/
git commit -m "feat(frontend): add auth API, useAuth hook, and useApiError"
```

---

### Task 6: Router and Route Guards

**Files:**
- Create: `frontend/src/router/PrivateRoute.tsx`
- Create: `frontend/src/router/AdminRoute.tsx`
- Create: `frontend/src/router/index.tsx`
- Create: `frontend/src/App.tsx`

- [ ] **Step 1: Create `src/router/PrivateRoute.tsx`**

```tsx
import { Navigate, Outlet } from 'react-router-dom'
import { useAuthStore } from '../store/authStore'

export function PrivateRoute() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  return isAuthenticated ? <Outlet /> : <Navigate to="/login" replace />
}
```

- [ ] **Step 2: Create `src/router/AdminRoute.tsx`**

```tsx
import { Navigate, Outlet } from 'react-router-dom'
import { useAuthStore } from '../store/authStore'

export function AdminRoute() {
  const { isAuthenticated, role } = useAuthStore()
  if (!isAuthenticated) return <Navigate to="/login" replace />
  if (role !== 'admin') return <Navigate to="/app" replace />
  return <Outlet />
}
```

- [ ] **Step 3: Create placeholder pages (fill in later tasks)**

```bash
mkdir -p frontend/src/pages/client frontend/src/pages/admin
```

Create `src/pages/Login.tsx`:
```tsx
export default function Login() { return <div>Login</div> }
```

Create `src/pages/client/Home.tsx`:
```tsx
export default function ClientHome() { return <div>Client Home</div> }
```

Create `src/pages/client/Book.tsx`:
```tsx
export default function Book() { return <div>Book</div> }
```

Create `src/pages/client/Appointments.tsx`:
```tsx
export default function ClientAppointments() { return <div>My Appointments</div> }
```

Create `src/pages/client/Profile.tsx`:
```tsx
export default function Profile() { return <div>Profile</div> }
```

Create `src/pages/admin/Dashboard.tsx`:
```tsx
export default function AdminDashboard() { return <div>Admin Dashboard</div> }
```

Create `src/pages/admin/Appointments.tsx`:
```tsx
export default function AdminAppointments() { return <div>Admin Appointments</div> }
```

Create `src/pages/admin/Users.tsx`:
```tsx
export default function AdminUsers() { return <div>Admin Users</div> }
```

Create `src/pages/admin/UserDetail.tsx`:
```tsx
export default function UserDetail() { return <div>User Detail</div> }
```

- [ ] **Step 4: Create `src/router/index.tsx`**

```tsx
import { createBrowserRouter, Navigate } from 'react-router-dom'
import { PrivateRoute } from './PrivateRoute'
import { AdminRoute } from './AdminRoute'
import Login from '../pages/Login'
import ClientHome from '../pages/client/Home'
import Book from '../pages/client/Book'
import ClientAppointments from '../pages/client/Appointments'
import Profile from '../pages/client/Profile'
import AdminDashboard from '../pages/admin/Dashboard'
import AdminAppointments from '../pages/admin/Appointments'
import AdminUsers from '../pages/admin/Users'
import UserDetail from '../pages/admin/UserDetail'

export const router = createBrowserRouter([
  { path: '/login', element: <Login /> },
  { path: '/', element: <Navigate to="/login" replace /> },
  {
    path: '/app',
    element: <PrivateRoute />,
    children: [
      { index: true, element: <ClientHome /> },
      { path: 'book', element: <Book /> },
      { path: 'appointments', element: <ClientAppointments /> },
      { path: 'profile', element: <Profile /> },
    ],
  },
  {
    path: '/admin',
    element: <AdminRoute />,
    children: [
      { index: true, element: <AdminDashboard /> },
      { path: 'appointments', element: <AdminAppointments /> },
      { path: 'users', element: <AdminUsers /> },
      { path: 'users/:id', element: <UserDetail /> },
    ],
  },
])
```

- [ ] **Step 5: Update `src/App.tsx`**

```tsx
import { RouterProvider } from 'react-router-dom'
import { router } from './router'

export default function App() {
  return <RouterProvider router={router} />
}
```

- [ ] **Step 6: Commit**

```bash
git add frontend/src/router/ frontend/src/pages/ frontend/src/App.tsx
git commit -m "feat(frontend): add router with role-based guards and placeholder pages"
```

---

### Task 7: UI Primitive Components

**Files:**
- Create: `frontend/src/components/ui/Button.tsx`
- Create: `frontend/src/components/ui/Input.tsx`
- Create: `frontend/src/components/ui/Badge.tsx`
- Create: `frontend/src/components/ui/Spinner.tsx`
- Create: `frontend/src/components/ui/Toast.tsx`
- Create: `frontend/src/components/ui/Modal.tsx`

- [ ] **Step 1: Create `src/components/ui/Button.tsx`**

```tsx
import { ButtonHTMLAttributes, forwardRef } from 'react'
import { Spinner } from './Spinner'

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'ghost' | 'danger'
  size?: 'sm' | 'md' | 'lg'
  loading?: boolean
  fullWidth?: boolean
}

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  ({ variant = 'primary', size = 'md', loading, fullWidth, children, className = '', disabled, ...props }, ref) => {
    const base = 'font-display font-bold uppercase tracking-wider transition-colors duration-150 flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed'
    const variants = {
      primary: 'bg-accent hover:bg-accent-hover text-white',
      ghost: 'bg-transparent border border-border text-text-primary hover:border-accent hover:text-accent',
      danger: 'bg-transparent border border-red-600 text-red-500 hover:bg-red-600 hover:text-white',
    }
    const sizes = {
      sm: 'px-3 py-1.5 text-xs rounded-sm',
      md: 'px-5 py-2.5 text-sm rounded-sm',
      lg: 'px-6 py-3.5 text-base rounded-sm',
    }
    return (
      <button
        ref={ref}
        disabled={disabled || loading}
        className={`${base} ${variants[variant]} ${sizes[size]} ${fullWidth ? 'w-full' : ''} ${className}`}
        {...props}
      >
        {loading && <Spinner size="sm" />}
        {children}
      </button>
    )
  },
)
Button.displayName = 'Button'
```

- [ ] **Step 2: Create `src/components/ui/Spinner.tsx`**

```tsx
interface SpinnerProps { size?: 'sm' | 'md' | 'lg' }

export function Spinner({ size = 'md' }: SpinnerProps) {
  const sizes = { sm: 'w-3 h-3 border', md: 'w-5 h-5 border-2', lg: 'w-8 h-8 border-2' }
  return (
    <div
      className={`${sizes[size]} rounded-full border-white/20 border-t-white animate-spin`}
      role="status"
      aria-label="Carregando"
    />
  )
}
```

- [ ] **Step 3: Create `src/components/ui/Input.tsx`**

```tsx
import { forwardRef, InputHTMLAttributes } from 'react'

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string
  error?: string
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, className = '', id, ...props }, ref) => (
    <div className="flex flex-col gap-1">
      {label && (
        <label htmlFor={id} className="text-xs font-body font-medium text-text-secondary uppercase tracking-wider">
          {label}
        </label>
      )}
      <input
        ref={ref}
        id={id}
        className={`bg-bg-elevated border ${error ? 'border-red-500' : 'border-border'} text-text-primary placeholder-text-secondary px-3 py-2.5 text-sm font-body rounded-sm focus:outline-none focus:border-accent transition-colors ${className}`}
        {...props}
      />
      {error && <span className="text-xs text-red-500">{error}</span>}
    </div>
  ),
)
Input.displayName = 'Input'
```

- [ ] **Step 4: Create `src/components/ui/Badge.tsx`**

```tsx
import type { AppointmentStatus } from '../../types/appointment'

const statusConfig: Record<AppointmentStatus, { label: string; className: string }> = {
  0: { label: 'Aguardando', className: 'bg-yellow-500/10 text-yellow-400 border border-yellow-500/30' },
  1: { label: 'Confirmado', className: 'bg-green-500/10 text-green-400 border border-green-500/30' },
  2: { label: 'Cancelado', className: 'bg-red-500/10 text-red-400 border border-red-500/30' },
}

export function AppointmentBadge({ status }: { status: AppointmentStatus }) {
  const { label, className } = statusConfig[status]
  return (
    <span className={`text-xs font-body font-medium px-2 py-0.5 rounded-sm ${className}`}>
      {label}
    </span>
  )
}

interface BadgeProps {
  children: React.ReactNode
  className?: string
}

export function Badge({ children, className = '' }: BadgeProps) {
  return (
    <span className={`text-xs font-body font-medium px-2 py-0.5 rounded-sm ${className}`}>
      {children}
    </span>
  )
}
```

- [ ] **Step 5: Create `src/components/ui/Toast.tsx`**

```tsx
import { createContext, useCallback, useContext, useState, ReactNode } from 'react'
import { X } from 'lucide-react'

type ToastType = 'success' | 'error' | 'info'

interface Toast {
  id: string
  message: string
  type: ToastType
}

interface ToastContextValue {
  toast: (message: string, type?: ToastType) => void
}

const ToastContext = createContext<ToastContextValue | null>(null)

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([])

  const removeToast = useCallback((id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id))
  }, [])

  const toast = useCallback((message: string, type: ToastType = 'info') => {
    const id = Math.random().toString(36).slice(2)
    setToasts((prev) => [...prev, { id, message, type }])
    setTimeout(() => removeToast(id), 4000)
  }, [removeToast])

  const borderColors = { success: 'border-l-green-500', error: 'border-l-accent', info: 'border-l-blue-500' }

  return (
    <ToastContext.Provider value={{ toast }}>
      {children}
      <div className="fixed top-4 left-1/2 -translate-x-1/2 z-50 flex flex-col gap-2 w-[calc(100%-2rem)] max-w-sm">
        {toasts.map((t) => (
          <div
            key={t.id}
            className={`bg-bg-surface border border-border border-l-4 ${borderColors[t.type]} px-4 py-3 flex items-center justify-between gap-3 rounded-sm shadow-lg animate-in slide-in-from-top-2`}
          >
            <span className="text-sm font-body text-text-primary">{t.message}</span>
            <button onClick={() => removeToast(t.id)} className="text-text-secondary hover:text-text-primary">
              <X size={14} />
            </button>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  )
}

export function useToast() {
  const ctx = useContext(ToastContext)
  if (!ctx) throw new Error('useToast must be used within ToastProvider')
  return ctx.toast
}
```

- [ ] **Step 6: Create `src/components/ui/Modal.tsx`**

```tsx
import { ReactNode, useEffect } from 'react'
import { X } from 'lucide-react'

interface ModalProps {
  open: boolean
  onClose: () => void
  title?: string
  children: ReactNode
}

export function Modal({ open, onClose, title, children }: ModalProps) {
  useEffect(() => {
    const handler = (e: KeyboardEvent) => e.key === 'Escape' && onClose()
    document.addEventListener('keydown', handler)
    return () => document.removeEventListener('keydown', handler)
  }, [onClose])

  if (!open) return null

  return (
    <div className="fixed inset-0 z-50 flex items-end sm:items-center justify-center p-4">
      <div className="absolute inset-0 bg-black/70 backdrop-blur-sm" onClick={onClose} />
      <div className="relative bg-bg-surface border border-border rounded-sm w-full max-w-md max-h-[90vh] overflow-y-auto">
        <div className="flex items-center justify-between p-4 border-b border-border">
          {title && <h2 className="font-display font-bold text-lg uppercase tracking-wide">{title}</h2>}
          <button onClick={onClose} className="ml-auto text-text-secondary hover:text-text-primary">
            <X size={18} />
          </button>
        </div>
        <div className="p-4">{children}</div>
      </div>
    </div>
  )
}
```

- [ ] **Step 7: Write component test `src/components/ui/__tests__/Button.test.tsx`**

```tsx
import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Button } from '../Button'

describe('Button', () => {
  it('renders children', () => {
    render(<Button>Agendar</Button>)
    expect(screen.getByText('Agendar')).toBeInTheDocument()
  })

  it('shows spinner when loading', () => {
    render(<Button loading>Agendar</Button>)
    expect(screen.getByRole('status')).toBeInTheDocument()
  })

  it('is disabled when loading', () => {
    render(<Button loading>Agendar</Button>)
    expect(screen.getByRole('button')).toBeDisabled()
  })

  it('calls onClick when clicked', async () => {
    const onClick = vi.fn()
    render(<Button onClick={onClick}>Click me</Button>)
    await userEvent.click(screen.getByRole('button'))
    expect(onClick).toHaveBeenCalledOnce()
  })
})
```

- [ ] **Step 8: Run tests**

```bash
npx vitest run src/components/ui/__tests__/Button.test.tsx
```

Expected: 4 tests PASS

- [ ] **Step 9: Update `src/main.tsx` to include ToastProvider**

```tsx
import React from 'react'
import ReactDOM from 'react-dom/client'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import App from './App'
import { ToastProvider } from './components/ui/Toast'
import './index.css'

const queryClient = new QueryClient({
  defaultOptions: { queries: { retry: 1, staleTime: 30_000 } },
})

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <ToastProvider>
        <App />
      </ToastProvider>
    </QueryClientProvider>
  </React.StrictMode>,
)
```

- [ ] **Step 10: Commit**

```bash
git add frontend/src/components/ui/ frontend/src/main.tsx
git commit -m "feat(frontend): add UI primitive components (Button, Input, Badge, Spinner, Toast, Modal)"
```

---

### Task 8: Layout Components

**Files:**
- Create: `frontend/src/components/layout/ClientLayout.tsx`
- Create: `frontend/src/components/layout/BottomNav.tsx`
- Create: `frontend/src/components/layout/AdminLayout.tsx`
- Create: `frontend/src/components/layout/Sidebar.tsx`

- [ ] **Step 1: Create `src/components/layout/BottomNav.tsx`**

```tsx
import { NavLink } from 'react-router-dom'
import { Home, CalendarPlus, Calendar, User } from 'lucide-react'

const links = [
  { to: '/app', label: 'Início', Icon: Home, end: true },
  { to: '/app/book', label: 'Agendar', Icon: CalendarPlus, end: false },
  { to: '/app/appointments', label: 'Meus', Icon: Calendar, end: false },
  { to: '/app/profile', label: 'Perfil', Icon: User, end: false },
]

export function BottomNav() {
  return (
    <nav className="fixed bottom-0 left-0 right-0 bg-bg-surface border-t border-border z-40 safe-area-pb">
      <div className="flex">
        {links.map(({ to, label, Icon, end }) => (
          <NavLink
            key={to}
            to={to}
            end={end}
            className={({ isActive }) =>
              `flex-1 flex flex-col items-center gap-0.5 py-3 text-xs font-body transition-colors ${
                isActive ? 'text-accent' : 'text-text-secondary'
              }`
            }
          >
            <Icon size={20} strokeWidth={1.5} />
            <span>{label}</span>
          </NavLink>
        ))}
      </div>
    </nav>
  )
}
```

- [ ] **Step 2: Create `src/components/layout/ClientLayout.tsx`**

```tsx
import { Outlet } from 'react-router-dom'
import { BottomNav } from './BottomNav'

export function ClientLayout() {
  return (
    <div className="min-h-screen bg-bg-base flex flex-col">
      <main className="flex-1 pb-20 overflow-y-auto">
        <Outlet />
      </main>
      <BottomNav />
    </div>
  )
}
```

- [ ] **Step 3: Update router to use ClientLayout**

In `src/router/index.tsx`, update the `/app` route:

```tsx
import { ClientLayout } from '../components/layout/ClientLayout'

// Inside the router, replace the /app children:
{
  path: '/app',
  element: <PrivateRoute />,
  children: [
    {
      element: <ClientLayout />,
      children: [
        { index: true, element: <ClientHome /> },
        { path: 'book', element: <Book /> },
        { path: 'appointments', element: <ClientAppointments /> },
        { path: 'profile', element: <Profile /> },
      ],
    },
  ],
},
```

- [ ] **Step 4: Create `src/components/layout/Sidebar.tsx`**

```tsx
import { NavLink } from 'react-router-dom'
import { LayoutDashboard, Calendar, Users, LogOut, Scissors } from 'lucide-react'
import { useAuth } from '../../hooks/useAuth'

const links = [
  { to: '/admin', label: 'Dashboard', Icon: LayoutDashboard, end: true },
  { to: '/admin/appointments', label: 'Agendamentos', Icon: Calendar, end: false },
  { to: '/admin/users', label: 'Usuários', Icon: Users, end: false },
]

export function Sidebar() {
  const { logout } = useAuth()
  return (
    <aside className="w-56 bg-bg-surface border-r border-border flex flex-col min-h-screen">
      <div className="p-5 border-b border-border">
        <div className="flex items-center gap-2">
          <Scissors size={18} className="text-accent" />
          <span className="font-display font-bold text-lg tracking-widest uppercase">
            Barber<span className="text-accent">Agenda</span>
          </span>
        </div>
      </div>
      <nav className="flex-1 p-3 flex flex-col gap-1">
        {links.map(({ to, label, Icon, end }) => (
          <NavLink
            key={to}
            to={to}
            end={end}
            className={({ isActive }) =>
              `flex items-center gap-3 px-3 py-2.5 rounded-sm text-sm font-body transition-colors ${
                isActive
                  ? 'bg-accent/10 text-accent border-l-2 border-accent'
                  : 'text-text-secondary hover:text-text-primary hover:bg-bg-elevated'
              }`
            }
          >
            <Icon size={16} strokeWidth={1.5} />
            {label}
          </NavLink>
        ))}
      </nav>
      <div className="p-3 border-t border-border">
        <button
          onClick={logout}
          className="flex items-center gap-3 px-3 py-2.5 w-full text-sm font-body text-text-secondary hover:text-red-400 transition-colors"
        >
          <LogOut size={16} strokeWidth={1.5} />
          Sair
        </button>
      </div>
    </aside>
  )
}
```

- [ ] **Step 5: Create `src/components/layout/AdminLayout.tsx`**

```tsx
import { Outlet } from 'react-router-dom'
import { Sidebar } from './Sidebar'

export function AdminLayout() {
  return (
    <div className="flex min-h-screen bg-bg-base">
      <Sidebar />
      <main className="flex-1 overflow-y-auto p-6">
        <Outlet />
      </main>
    </div>
  )
}
```

- [ ] **Step 6: Update router to use AdminLayout**

In `src/router/index.tsx`, update the `/admin` route:

```tsx
import { AdminLayout } from '../components/layout/AdminLayout'

{
  path: '/admin',
  element: <AdminRoute />,
  children: [
    {
      element: <AdminLayout />,
      children: [
        { index: true, element: <AdminDashboard /> },
        { path: 'appointments', element: <AdminAppointments /> },
        { path: 'users', element: <AdminUsers /> },
        { path: 'users/:id', element: <UserDetail /> },
      ],
    },
  ],
},
```

- [ ] **Step 7: Commit**

```bash
git add frontend/src/components/layout/ frontend/src/router/index.tsx
git commit -m "feat(frontend): add client and admin layout components with navigation"
```

---

### Task 9: Login Page

**Files:**
- Modify: `frontend/src/pages/Login.tsx`

- [ ] **Step 1: Write failing test `src/pages/__tests__/Login.test.tsx`**

```tsx
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import Login from '../Login'

const mockLogin = vi.fn()
vi.mock('../../hooks/useAuth', () => ({ useAuth: () => ({ login: mockLogin, isAuthenticated: false, role: null }) }))
vi.mock('../../components/ui/Toast', () => ({ useToast: () => vi.fn() }))

function renderLogin() {
  return render(<MemoryRouter><Login /></MemoryRouter>)
}

describe('Login page', () => {
  beforeEach(() => { mockLogin.mockReset() })

  it('renders email and password fields', () => {
    renderLogin()
    expect(screen.getByLabelText(/e-mail/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/senha/i)).toBeInTheDocument()
  })

  it('shows validation error when submitted empty', async () => {
    renderLogin()
    await userEvent.click(screen.getByRole('button', { name: /entrar/i }))
    expect(await screen.findByText(/e-mail inválido/i)).toBeInTheDocument()
  })

  it('calls login with credentials on submit', async () => {
    mockLogin.mockResolvedValue({ role: 'client' })
    renderLogin()
    await userEvent.type(screen.getByLabelText(/e-mail/i), 'test@test.com')
    await userEvent.type(screen.getByLabelText(/senha/i), 'password123')
    await userEvent.click(screen.getByRole('button', { name: /entrar/i }))
    await waitFor(() => expect(mockLogin).toHaveBeenCalledWith('test@test.com', 'password123'))
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

```bash
npx vitest run src/pages/__tests__/Login.test.tsx
```

Expected: FAIL — placeholder Login component has no form

- [ ] **Step 3: Implement `src/pages/Login.tsx`**

```tsx
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { Scissors } from 'lucide-react'
import { useAuth } from '../hooks/useAuth'
import { useToast } from '../components/ui/Toast'
import { useApiError } from '../hooks/useApiError'
import { Input } from '../components/ui/Input'
import { Button } from '../components/ui/Button'
import { authApi } from '../api/auth'

const schema = z.object({
  email: z.string().email('E-mail inválido'),
  password: z.string().min(1, 'Senha obrigatória'),
})

type FormData = z.infer<typeof schema>

export default function Login() {
  const { login, isAuthenticated, role } = useAuth()
  const navigate = useNavigate()
  const toast = useToast()
  const { getMessage } = useApiError()
  const [loading, setLoading] = useState(false)
  const [forgotOpen, setForgotOpen] = useState(false)
  const [forgotEmail, setForgotEmail] = useState('')
  const [forgotLoading, setForgotLoading] = useState(false)

  const { register, handleSubmit, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  if (isAuthenticated) {
    navigate(role === 'admin' ? '/admin' : '/app', { replace: true })
    return null
  }

  const onSubmit = async ({ email, password }: FormData) => {
    setLoading(true)
    try {
      const data = await login(email, password)
      navigate('Users' in data.modulesAssembled ? '/admin' : '/app', { replace: true })
    } catch (err: unknown) {
      const label = err instanceof Error ? err.message : ''
      toast(getMessage(label, 'Erro ao fazer login.'), 'error')
    } finally {
      setLoading(false)
    }
  }

  const handleForgot = async () => {
    if (!forgotEmail) return
    setForgotLoading(true)
    try {
      await authApi.sendResetPassword(forgotEmail)
      toast('E-mail de recuperação enviado!', 'success')
      setForgotOpen(false)
      setForgotEmail('')
    } catch {
      toast('Erro ao enviar e-mail.', 'error')
    } finally {
      setForgotLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-bg-base flex flex-col items-center justify-center p-6">
      {/* Logo */}
      <div className="mb-10 flex flex-col items-center gap-3">
        <div className="w-14 h-14 bg-accent rounded-sm flex items-center justify-center">
          <Scissors size={28} className="text-white" />
        </div>
        <h1 className="font-display font-extrabold text-3xl tracking-widest uppercase text-text-primary">
          Barber<span className="text-accent">Agenda</span>
        </h1>
        <p className="text-text-secondary text-sm font-body">Barbearia que respeita o seu tempo.</p>
      </div>

      {/* Card */}
      <div className="w-full max-w-sm bg-bg-surface border border-border rounded-sm p-6 flex flex-col gap-5">
        <div className="border-l-2 border-accent pl-3">
          <h2 className="font-display font-bold text-xl uppercase tracking-wide">Acesse sua conta</h2>
          <p className="text-text-secondary text-xs font-body mt-0.5">Entre com suas credenciais</p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4" noValidate>
          <Input
            id="email"
            label="E-mail"
            type="email"
            placeholder="seuemail@exemplo.com"
            error={errors.email?.message}
            {...register('email')}
          />
          <Input
            id="password"
            label="Senha"
            type="password"
            placeholder="••••••••"
            error={errors.password?.message}
            {...register('password')}
          />
          <Button type="submit" fullWidth size="lg" loading={loading}>
            Entrar
          </Button>
        </form>

        <button
          onClick={() => setForgotOpen(true)}
          className="text-xs text-text-secondary hover:text-accent transition-colors font-body text-center"
        >
          Esqueci minha senha
        </button>
      </div>

      {/* Forgot password inline panel */}
      {forgotOpen && (
        <div className="w-full max-w-sm bg-bg-elevated border border-border rounded-sm p-5 mt-3 flex flex-col gap-3">
          <p className="text-sm font-body text-text-primary">Digite seu e-mail para receber o link de recuperação:</p>
          <Input
            placeholder="seuemail@exemplo.com"
            type="email"
            value={forgotEmail}
            onChange={(e) => setForgotEmail(e.target.value)}
          />
          <div className="flex gap-2">
            <Button variant="ghost" size="sm" onClick={() => setForgotOpen(false)} className="flex-1">
              Cancelar
            </Button>
            <Button size="sm" loading={forgotLoading} onClick={handleForgot} className="flex-1">
              Enviar
            </Button>
          </div>
        </div>
      )}

      {/* Decorative diagonal accent */}
      <div className="fixed bottom-0 left-0 right-0 h-1 bg-accent" />
    </div>
  )
}
```

- [ ] **Step 4: Run tests**

```bash
npx vitest run src/pages/__tests__/Login.test.tsx
```

Expected: 3 tests PASS

- [ ] **Step 5: Commit**

```bash
git add frontend/src/pages/Login.tsx frontend/src/pages/__tests__/
git commit -m "feat(frontend): implement Login page with validation, auth, and forgot password"
```

---

### Task 10: Appointments API

**Files:**
- Create: `frontend/src/api/appointments.ts`

- [ ] **Step 1: Create `src/api/appointments.ts`**

```ts
import { apiClient } from './client'
import type { ApiResponse } from '../types/auth'
import type { Appointment, CreateAppointmentRequest, CancelAppointmentRequest } from '../types/appointment'

export const appointmentsApi = {
  getAvailableSlots: async (date: string): Promise<string[]> => {
    const { data } = await apiClient.get<ApiResponse<string[]>>('/appointment/available-slots', {
      params: { date },
    })
    return data.data ?? []
  },

  create: async (body: CreateAppointmentRequest): Promise<Appointment> => {
    const { data } = await apiClient.post<ApiResponse<Appointment>>('/appointment', body)
    if (!data.success || !data.data) throw new Error(data.responseLabel)
    return data.data
  },

  cancel: async (body: CancelAppointmentRequest): Promise<void> => {
    const { data } = await apiClient.patch<ApiResponse>('/appointment/cancel', body)
    if (!data.success) throw new Error(data.responseLabel)
  },

  getByUser: async (userId: number): Promise<Appointment[]> => {
    const { data } = await apiClient.get<ApiResponse<Appointment[]>>(`/appointment/user/${userId}`)
    return data.data ?? []
  },

  getById: async (id: number): Promise<Appointment> => {
    const { data } = await apiClient.get<ApiResponse<Appointment>>(`/appointment/${id}`)
    if (!data.data) throw new Error(data.responseLabel)
    return data.data
  },
}
```

- [ ] **Step 2: Commit**

```bash
git add frontend/src/api/appointments.ts
git commit -m "feat(frontend): add appointments API layer"
```

---

### Task 11: Client Home Page

**Files:**
- Modify: `frontend/src/pages/client/Home.tsx`

- [ ] **Step 1: Implement `src/pages/client/Home.tsx`**

```tsx
import { useNavigate } from 'react-router-dom'
import { CalendarPlus, Clock, ChevronRight } from 'lucide-react'
import { useQuery } from '@tanstack/react-query'
import { useAuthStore } from '../../store/authStore'
import { appointmentsApi } from '../../api/appointments'
import { Button } from '../../components/ui/Button'
import { AppointmentBadge } from '../../components/ui/Badge'
import { Spinner } from '../../components/ui/Spinner'
import type { Appointment } from '../../types/appointment'

function formatDate(isoString: string) {
  return new Date(isoString).toLocaleDateString('pt-BR', { weekday: 'short', day: 'numeric', month: 'short' })
}

function formatTime(isoString: string) {
  return new Date(isoString).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })
}

function AppointmentCard({ appt }: { appt: Appointment }) {
  const navigate = useNavigate()
  const cancelled = appt.status === 2
  return (
    <div
      className={`bg-bg-surface border border-border border-l-[3px] ${cancelled ? 'border-l-border opacity-50' : 'border-l-accent'} rounded-sm p-4 flex items-center justify-between gap-3 cursor-pointer hover:bg-bg-elevated transition-colors`}
      onClick={() => navigate('/app/appointments')}
    >
      <div className="flex flex-col gap-1">
        <span className="text-xs font-body text-text-secondary uppercase tracking-wider">
          {formatDate(appt.startTime)}
        </span>
        <div className="flex items-center gap-2">
          <Clock size={14} className="text-accent" />
          <span className="font-display font-bold text-xl">{formatTime(appt.startTime)}</span>
        </div>
        <AppointmentBadge status={appt.status} />
      </div>
      <ChevronRight size={16} className="text-text-secondary" />
    </div>
  )
}

export default function ClientHome() {
  const navigate = useNavigate()
  const userId = useAuthStore((s) => s.userId)
  const userName = useAuthStore((s) => s.userName)

  const { data: appointments = [], isLoading } = useQuery({
    queryKey: ['appointments', userId],
    queryFn: () => appointmentsApi.getByUser(userId!),
    enabled: !!userId,
  })

  const upcoming = appointments
    .filter((a) => a.status !== 2 && new Date(a.startTime) >= new Date())
    .sort((a, b) => new Date(a.startTime).getTime() - new Date(b.startTime).getTime())
    .slice(0, 3)

  return (
    <div className="min-h-screen bg-bg-base">
      {/* Header */}
      <div className="bg-bg-surface border-b border-border px-5 pt-10 pb-6 clip-diagonal">
        <p className="text-xs font-display uppercase tracking-[0.2em] text-accent mb-1">Barbearia</p>
        <h1 className="font-display font-extrabold text-4xl uppercase leading-none">
          Olá,<br />{userName?.split(' ')[0] ?? 'Cliente'}
        </h1>
      </div>

      <div className="px-5 pt-6 flex flex-col gap-6">
        {/* CTA */}
        <Button fullWidth size="lg" onClick={() => navigate('/app/book')}>
          <CalendarPlus size={18} />
          Agendar Agora
        </Button>

        {/* Upcoming appointments */}
        <section>
          <h2 className="font-display font-bold text-sm uppercase tracking-widest text-text-secondary mb-3">
            Próximos Agendamentos
          </h2>

          {isLoading && (
            <div className="flex justify-center py-8">
              <Spinner />
            </div>
          )}

          {!isLoading && upcoming.length === 0 && (
            <div className="border border-border border-dashed rounded-sm p-8 flex flex-col items-center gap-3 text-center">
              <CalendarPlus size={32} className="text-text-secondary" strokeWidth={1} />
              <p className="text-text-secondary text-sm font-body">Nenhum agendamento próximo.</p>
              <Button size="sm" onClick={() => navigate('/app/book')}>Fazer primeiro agendamento</Button>
            </div>
          )}

          {!isLoading && upcoming.map((appt) => (
            <div key={appt.id} className="mb-2">
              <AppointmentCard appt={appt} />
            </div>
          ))}
        </section>
      </div>
    </div>
  )
}
```

- [ ] **Step 2: Commit**

```bash
git add frontend/src/pages/client/Home.tsx
git commit -m "feat(frontend): implement Client Home page with upcoming appointments"
```

---

### Task 12: Book Appointment Wizard

**Files:**
- Modify: `frontend/src/pages/client/Book.tsx`

- [ ] **Step 1: Implement `src/pages/client/Book.tsx`**

```tsx
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { ChevronLeft, Calendar, Clock, Check } from 'lucide-react'
import { useAuthStore } from '../../store/authStore'
import { appointmentsApi } from '../../api/appointments'
import { Button } from '../../components/ui/Button'
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

function SlotGrid({
  slots,
  selected,
  onSelect,
}: {
  slots: string[]
  selected: string | null
  onSelect: (s: string) => void
}) {
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
  const [step, setStep] = useState(0)
  const [selectedDate, setSelectedDate] = useState<Date | null>(null)
  const [selectedSlot, setSelectedSlot] = useState<string | null>(null)

  const dateString = selectedDate ? toISODate(selectedDate) : ''

  const { data: slots = [], isFetching: loadingSlots } = useQuery({
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
      {/* Header */}
      <div className="bg-bg-surface border-b border-border px-5 pt-10 pb-4 flex items-center gap-3">
        <button onClick={() => step === 0 ? navigate('/app') : setStep(s => s - 1)} className="text-text-secondary hover:text-text-primary">
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

      {/* Content */}
      <div className="flex-1 px-5 pt-6 pb-24">
        {/* Step 0: Date */}
        {step === 0 && (
          <div className="flex flex-col gap-5">
            <div className="flex items-center gap-2 mb-1">
              <Calendar size={16} className="text-accent" />
              <h2 className="font-display font-bold text-sm uppercase tracking-widest text-text-secondary">Escolha a data</h2>
            </div>
            <DatePicker value={selectedDate} onChange={setSelectedDate} />
            <Button
              fullWidth
              size="lg"
              disabled={!selectedDate}
              onClick={() => setStep(1)}
            >
              Próximo
            </Button>
          </div>
        )}

        {/* Step 1: Time slot */}
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
            ) : (
              <SlotGrid slots={slots} selected={selectedSlot} onSelect={setSelectedSlot} />
            )}
            <Button fullWidth size="lg" disabled={!selectedSlot} onClick={() => setStep(2)}>
              Próximo
            </Button>
          </div>
        )}

        {/* Step 2: Confirm */}
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
```

- [ ] **Step 2: Commit**

```bash
git add frontend/src/pages/client/Book.tsx
git commit -m "feat(frontend): implement 3-step Book Appointment wizard"
```

---

### Task 13: My Appointments Page

**Files:**
- Modify: `frontend/src/pages/client/Appointments.tsx`

- [ ] **Step 1: Implement `src/pages/client/Appointments.tsx`**

```tsx
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
                <Button
                  variant="danger"
                  size="sm"
                  onClick={() => setCancelTarget(appt)}
                >
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
```

- [ ] **Step 2: Commit**

```bash
git add frontend/src/pages/client/Appointments.tsx
git commit -m "feat(frontend): implement My Appointments page with cancel flow"
```

---

### Task 14: Users API

**Files:**
- Create: `frontend/src/api/users.ts`

- [ ] **Step 1: Create `src/api/users.ts`**

```ts
import { apiClient } from './client'
import type { ApiResponse } from '../types/auth'
import type {
  User,
  CreateUserRequest,
  UpdateUserRequest,
  GetUsersPaginatedRequest,
  PaginatedResponse,
} from '../types/user'

export const usersApi = {
  list: async (params: GetUsersPaginatedRequest = {}): Promise<PaginatedResponse<User>> => {
    const { data } = await apiClient.get<ApiResponse<PaginatedResponse<User>>>('/user', { params })
    return data.data ?? { items: [], totalCount: 0, pageNumber: 1, pageSize: 20, totalPages: 0 }
  },

  getById: async (id: number): Promise<User> => {
    const { data } = await apiClient.get<ApiResponse<User>>(`/user/${id}`)
    if (!data.data) throw new Error(data.responseLabel)
    return data.data
  },

  create: async (body: CreateUserRequest): Promise<void> => {
    const { data } = await apiClient.post<ApiResponse>('/user', body)
    if (!data.success) throw new Error(data.responseLabel)
  },

  update: async (body: UpdateUserRequest): Promise<void> => {
    const { data } = await apiClient.put<ApiResponse>('/user', body)
    if (!data.success) throw new Error(data.responseLabel)
  },

  changeStatus: async (id: number, status: 0 | 1): Promise<void> => {
    const { data } = await apiClient.patch<ApiResponse>('/user/status', { id, status })
    if (!data.success) throw new Error(data.responseLabel)
  },

  delete: async (id: number): Promise<void> => {
    const { data } = await apiClient.delete<ApiResponse>(`/user/${id}`)
    if (!data.success) throw new Error(data.responseLabel)
  },
}
```

- [ ] **Step 2: Commit**

```bash
git add frontend/src/api/users.ts
git commit -m "feat(frontend): add users API layer"
```

---

### Task 15: Admin Dashboard

**Files:**
- Modify: `frontend/src/pages/admin/Dashboard.tsx`

- [ ] **Step 1: Implement `src/pages/admin/Dashboard.tsx`**

```tsx
import { useQuery } from '@tanstack/react-query'
import { Calendar, Clock } from 'lucide-react'
import { appointmentsApi } from '../../api/appointments'
import { AppointmentBadge } from '../../components/ui/Badge'
import { Spinner } from '../../components/ui/Spinner'
import { useAuthStore } from '../../store/authStore'
import type { Appointment } from '../../types/appointment'

function isToday(isoString: string) {
  const d = new Date(isoString)
  const today = new Date()
  return (
    d.getDate() === today.getDate() &&
    d.getMonth() === today.getMonth() &&
    d.getFullYear() === today.getFullYear()
  )
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

  // NOTE: The backend doesn't have a "list all appointments" endpoint without a userId.
  // We use the current admin's appointments as a proxy for now.
  // In production this endpoint should be extended to support admin-level queries.
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
      <div>
        <h1 className="font-display font-extrabold text-4xl uppercase">Dashboard</h1>
        <p className="text-text-secondary text-sm font-body mt-1">Visão geral do sistema</p>
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
                  <div
                    key={appt.id}
                    className={`flex items-center justify-between px-4 py-3 ${i !== recent.length - 1 ? 'border-b border-border' : ''}`}
                  >
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
```

- [ ] **Step 2: Commit**

```bash
git add frontend/src/pages/admin/Dashboard.tsx
git commit -m "feat(frontend): implement Admin Dashboard with stats and recent appointments"
```

---

### Task 16: Admin Users Page

**Files:**
- Modify: `frontend/src/pages/admin/Users.tsx`

- [ ] **Step 1: Implement `src/pages/admin/Users.tsx`**

```tsx
import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { Search, Plus, Eye, Power, Trash2, ChevronLeft, ChevronRight } from 'lucide-react'
import { usersApi } from '../../api/users'
import { Button } from '../../components/ui/Button'
import { Input } from '../../components/ui/Input'
import { Badge } from '../../components/ui/Badge'
import { Modal } from '../../components/ui/Modal'
import { Spinner } from '../../components/ui/Spinner'
import { useToast } from '../../components/ui/Toast'
import { useApiError } from '../../hooks/useApiError'
import type { User } from '../../types/user'

const createSchema = z.object({
  name: z.string().min(1, 'Nome obrigatório'),
  email: z.string().email('E-mail inválido'),
  accessProfile: z.coerce.number().min(1, 'Perfil obrigatório'),
})
type CreateForm = z.infer<typeof createSchema>

export default function AdminUsers() {
  const navigate = useNavigate()
  const toast = useToast()
  const { getMessage } = useApiError()
  const qc = useQueryClient()
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [draftSearch, setDraftSearch] = useState('')
  const [createOpen, setCreateOpen] = useState(false)
  const [deleteTarget, setDeleteTarget] = useState<User | null>(null)

  const { data, isLoading } = useQuery({
    queryKey: ['users', page, search],
    queryFn: () => usersApi.list({ pageNumber: page, pageSize: 20, name: search || undefined }),
  })

  const { register, handleSubmit, reset, formState: { errors } } = useForm<CreateForm>({
    resolver: zodResolver(createSchema),
  })

  const { mutate: createUser, isPending: creating } = useMutation({
    mutationFn: (d: CreateForm) => usersApi.create({ ...d }),
    onSuccess: () => {
      toast('Usuário criado com sucesso!', 'success')
      setCreateOpen(false)
      reset()
      qc.invalidateQueries({ queryKey: ['users'] })
    },
    onError: (err: Error) => toast(getMessage(err.message, 'Erro ao criar usuário.'), 'error'),
  })

  const { mutate: toggleStatus, isPending: togglingStatus } = useMutation({
    mutationFn: (user: User) => usersApi.changeStatus(user.id, user.status === 1 ? 0 : 1),
    onSuccess: () => {
      toast('Status atualizado.', 'success')
      qc.invalidateQueries({ queryKey: ['users'] })
    },
    onError: (err: Error) => toast(getMessage(err.message, 'Erro ao alterar status.'), 'error'),
  })

  const { mutate: deleteUser, isPending: deleting } = useMutation({
    mutationFn: () => usersApi.delete(deleteTarget!.id),
    onSuccess: () => {
      toast('Usuário removido.', 'info')
      setDeleteTarget(null)
      qc.invalidateQueries({ queryKey: ['users'] })
    },
    onError: (err: Error) => toast(getMessage(err.message, 'Erro ao remover.'), 'error'),
  })

  const users = data?.items ?? []
  const totalPages = data?.totalPages ?? 1

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="font-display font-extrabold text-4xl uppercase">Usuários</h1>
          <p className="text-text-secondary text-sm font-body mt-1">{data?.totalCount ?? 0} registros</p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <Plus size={16} />
          Novo usuário
        </Button>
      </div>

      {/* Search */}
      <form
        className="flex gap-2"
        onSubmit={(e) => { e.preventDefault(); setSearch(draftSearch); setPage(1) }}
      >
        <Input
          placeholder="Buscar por nome..."
          value={draftSearch}
          onChange={(e) => setDraftSearch(e.target.value)}
          className="flex-1"
        />
        <Button type="submit" variant="ghost">
          <Search size={16} />
        </Button>
      </form>

      {/* Table */}
      {isLoading ? (
        <div className="flex justify-center py-10"><Spinner /></div>
      ) : (
        <div className="bg-bg-surface border border-border rounded-sm overflow-hidden">
          {users.length === 0 && (
            <p className="text-text-secondary text-sm font-body p-6">Nenhum usuário encontrado.</p>
          )}
          {users.map((user, i) => (
            <div
              key={user.id}
              className={`flex items-center justify-between px-4 py-3 gap-3 ${i !== users.length - 1 ? 'border-b border-border' : ''}`}
            >
              <div className="flex-1 min-w-0">
                <p className="text-sm font-body font-medium truncate">{user.name}</p>
                <p className="text-xs text-text-secondary font-body truncate">{user.email}</p>
              </div>
              <Badge className={user.status === 1 ? 'bg-green-500/10 text-green-400 border border-green-500/30' : 'bg-red-500/10 text-red-400 border border-red-500/30'}>
                {user.status === 1 ? 'Ativo' : 'Inativo'}
              </Badge>
              <div className="flex gap-1">
                <button
                  onClick={() => navigate(`/admin/users/${user.id}`)}
                  className="p-1.5 text-text-secondary hover:text-text-primary transition-colors"
                  title="Ver detalhes"
                >
                  <Eye size={15} />
                </button>
                <button
                  onClick={() => toggleStatus(user)}
                  disabled={togglingStatus}
                  className="p-1.5 text-text-secondary hover:text-accent transition-colors"
                  title="Alternar status"
                >
                  <Power size={15} />
                </button>
                <button
                  onClick={() => setDeleteTarget(user)}
                  className="p-1.5 text-text-secondary hover:text-red-400 transition-colors"
                  title="Excluir"
                >
                  <Trash2 size={15} />
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex items-center justify-center gap-3">
          <button
            disabled={page === 1}
            onClick={() => setPage(p => p - 1)}
            className="text-text-secondary hover:text-text-primary disabled:opacity-30"
          >
            <ChevronLeft size={18} />
          </button>
          <span className="text-sm font-body text-text-secondary">Página {page} de {totalPages}</span>
          <button
            disabled={page === totalPages}
            onClick={() => setPage(p => p + 1)}
            className="text-text-secondary hover:text-text-primary disabled:opacity-30"
          >
            <ChevronRight size={18} />
          </button>
        </div>
      )}

      {/* Create modal */}
      <Modal open={createOpen} onClose={() => { setCreateOpen(false); reset() }} title="Novo usuário">
        <form onSubmit={handleSubmit((d) => createUser(d))} className="flex flex-col gap-4">
          <Input id="name" label="Nome" placeholder="Nome completo" error={errors.name?.message} {...register('name')} />
          <Input id="email" label="E-mail" type="email" placeholder="email@exemplo.com" error={errors.email?.message} {...register('email')} />
          <Input id="profile" label="ID do perfil de acesso" type="number" placeholder="Ex: 2" error={errors.accessProfile?.message} {...register('accessProfile')} />
          <Button type="submit" fullWidth loading={creating}>Criar usuário</Button>
        </form>
      </Modal>

      {/* Delete confirmation */}
      <Modal open={!!deleteTarget} onClose={() => setDeleteTarget(null)} title="Excluir usuário">
        <div className="flex flex-col gap-4">
          <p className="text-sm font-body text-text-secondary">
            Tem certeza que deseja excluir <strong className="text-text-primary">{deleteTarget?.name}</strong>? Esta ação não pode ser desfeita.
          </p>
          <div className="flex gap-2">
            <Button variant="ghost" size="sm" onClick={() => setDeleteTarget(null)} className="flex-1">Cancelar</Button>
            <Button variant="danger" size="sm" loading={deleting} onClick={() => deleteUser()} className="flex-1">Excluir</Button>
          </div>
        </div>
      </Modal>
    </div>
  )
}
```

- [ ] **Step 2: Commit**

```bash
git add frontend/src/pages/admin/Users.tsx
git commit -m "feat(frontend): implement Admin Users page with CRUD, search, and pagination"
```

---

### Task 17: Admin Appointments Page

**Files:**
- Modify: `frontend/src/pages/admin/Appointments.tsx`

- [ ] **Step 1: Implement `src/pages/admin/Appointments.tsx`**

```tsx
import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { appointmentsApi } from '../../api/appointments'
import { AppointmentBadge } from '../../components/ui/Badge'
import { Spinner } from '../../components/ui/Spinner'
import { useAuthStore } from '../../store/authStore'
import type { AppointmentStatus } from '../../types/appointment'

const STATUS_FILTERS: { label: string; value: AppointmentStatus | 'all' }[] = [
  { label: 'Todos', value: 'all' },
  { label: 'Aguardando', value: 0 },
  { label: 'Confirmados', value: 1 },
  { label: 'Cancelados', value: 2 },
]

export default function AdminAppointments() {
  const userId = useAuthStore((s) => s.userId)
  const [statusFilter, setStatusFilter] = useState<AppointmentStatus | 'all'>('all')

  const { data: appointments = [], isLoading } = useQuery({
    queryKey: ['admin-appointments-list'],
    queryFn: () => appointmentsApi.getByUser(userId!),
    enabled: !!userId,
  })

  const filtered = statusFilter === 'all'
    ? appointments
    : appointments.filter((a) => a.status === statusFilter)

  const sorted = [...filtered].sort(
    (a, b) => new Date(b.startTime).getTime() - new Date(a.startTime).getTime(),
  )

  return (
    <div className="flex flex-col gap-6">
      <div>
        <h1 className="font-display font-extrabold text-4xl uppercase">Agendamentos</h1>
        <p className="text-text-secondary text-sm font-body mt-1">{filtered.length} registros</p>
      </div>

      {/* Status filter pills */}
      <div className="flex gap-2 flex-wrap">
        {STATUS_FILTERS.map(({ label, value }) => (
          <button
            key={String(value)}
            onClick={() => setStatusFilter(value)}
            className={`px-3 py-1.5 text-xs font-body rounded-sm border transition-colors ${
              statusFilter === value
                ? 'bg-accent border-accent text-white'
                : 'border-border text-text-secondary hover:border-accent hover:text-accent'
            }`}
          >
            {label}
          </button>
        ))}
      </div>

      {isLoading ? (
        <div className="flex justify-center py-10"><Spinner /></div>
      ) : (
        <div className="bg-bg-surface border border-border rounded-sm overflow-hidden">
          {sorted.length === 0 && (
            <p className="text-text-secondary text-sm font-body p-6">Nenhum agendamento encontrado.</p>
          )}
          {sorted.map((appt, i) => {
            const d = new Date(appt.startTime)
            return (
              <div
                key={appt.id}
                className={`flex items-center justify-between px-4 py-3 gap-3 ${i !== sorted.length - 1 ? 'border-b border-border' : ''}`}
              >
                <div>
                  <p className="text-sm font-body font-medium">
                    {d.toLocaleDateString('pt-BR', { weekday: 'short', day: 'numeric', month: 'short' })}
                  </p>
                  <p className="text-xs text-text-secondary font-body">
                    {d.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })} — ID #{appt.id}
                  </p>
                </div>
                <AppointmentBadge status={appt.status} />
              </div>
            )
          })}
        </div>
      )}
    </div>
  )
}
```

- [ ] **Step 2: Commit**

```bash
git add frontend/src/pages/admin/Appointments.tsx
git commit -m "feat(frontend): implement Admin Appointments page with status filters"
```

---

### Task 18: Admin User Detail Page

**Files:**
- Modify: `frontend/src/pages/admin/UserDetail.tsx`

- [ ] **Step 1: Implement `src/pages/admin/UserDetail.tsx`**

```tsx
import { useParams, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { ChevronLeft, Mail, Calendar } from 'lucide-react'
import { usersApi } from '../../api/users'
import { appointmentsApi } from '../../api/appointments'
import { Badge, AppointmentBadge } from '../../components/ui/Badge'
import { Spinner } from '../../components/ui/Spinner'
import { Button } from '../../components/ui/Button'

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
      <button
        onClick={() => navigate('/admin/users')}
        className="flex items-center gap-1 text-text-secondary hover:text-text-primary text-sm font-body transition-colors w-fit"
      >
        <ChevronLeft size={16} />
        Voltar para usuários
      </button>

      {/* User card */}
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
        <Button variant="ghost" size="sm" onClick={() => navigate('/admin/users')} className="w-fit">
          Editar usuário
        </Button>
      </div>

      {/* Appointments */}
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
                <div
                  key={appt.id}
                  className={`flex items-center justify-between px-4 py-3 ${i !== sortedAppts.length - 1 ? 'border-b border-border' : ''}`}
                >
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
```

- [ ] **Step 2: Commit**

```bash
git add frontend/src/pages/admin/UserDetail.tsx
git commit -m "feat(frontend): implement Admin User Detail page"
```

---

### Task 19: Client Profile Page

**Files:**
- Modify: `frontend/src/pages/client/Profile.tsx`

- [ ] **Step 1: Implement `src/pages/client/Profile.tsx`**

```tsx
import { LogOut, Mail, User as UserIcon } from 'lucide-react'
import { useAuthStore } from '../../store/authStore'
import { useAuth } from '../../hooks/useAuth'
import { Button } from '../../components/ui/Button'

export default function Profile() {
  const { userName, userEmail } = useAuthStore()
  const { logout } = useAuth()

  return (
    <div className="min-h-screen bg-bg-base">
      <div className="bg-bg-surface border-b border-border px-5 pt-10 pb-5">
        <h1 className="font-display font-extrabold text-3xl uppercase">Perfil</h1>
      </div>

      <div className="px-5 pt-6 flex flex-col gap-4">
        <div className="bg-bg-surface border border-border border-l-[3px] border-l-accent rounded-sm p-5 flex flex-col gap-3">
          <div className="flex items-center gap-3">
            <div className="w-12 h-12 bg-accent/10 border border-accent/20 rounded-sm flex items-center justify-center">
              <UserIcon size={22} className="text-accent" />
            </div>
            <div>
              <p className="font-display font-bold text-xl uppercase">{userName}</p>
              <div className="flex items-center gap-1.5">
                <Mail size={11} className="text-text-secondary" />
                <p className="text-text-secondary text-xs font-body">{userEmail}</p>
              </div>
            </div>
          </div>
        </div>

        <Button variant="danger" fullWidth onClick={logout}>
          <LogOut size={16} />
          Sair da conta
        </Button>
      </div>
    </div>
  )
}
```

- [ ] **Step 2: Commit**

```bash
git add frontend/src/pages/client/Profile.tsx
git commit -m "feat(frontend): implement Client Profile page with logout"
```

---

### Task 20: Final Wiring and Verification

- [ ] **Step 1: Run all tests**

```bash
cd frontend && npx vitest run
```

Expected: all tests PASS (no failures)

- [ ] **Step 2: Start dev server and verify**

```bash
cd frontend && npm run dev
```

Verify manually:
- [ ] `/login` renders without errors, can submit form
- [ ] Invalid credentials shows toast
- [ ] Successful login redirects to correct area
- [ ] Client area: BottomNav visible, Home loads
- [ ] Client area: Book wizard navigates through 3 steps
- [ ] Client area: Appointments page shows list
- [ ] Admin area: Sidebar visible, Dashboard loads
- [ ] Admin area: Users list, search, create modal
- [ ] Admin area: User detail page loads
- [ ] Logout works and clears auth

- [ ] **Step 3: Fix any TypeScript errors**

```bash
npx tsc --noEmit
```

Fix any type errors reported.

- [ ] **Step 4: Final commit**

```bash
git add -A
git commit -m "feat(frontend): complete BarberAgenda frontend — all pages integrated with backend"
```

---

## Self-Review

**Spec coverage:**
- ✅ React + Vite + TypeScript scaffold (Task 1)
- ✅ TypeScript types aligned with backend DTOs (Task 2)
- ✅ Axios client with Bearer injection and silent refresh (Task 3)
- ✅ Zustand auth store with persistence and role detection (Task 4)
- ✅ Auth API + useAuth + useApiError (Task 5)
- ✅ Router with PrivateRoute and AdminRoute (Task 6)
- ✅ UI primitives: Button, Input, Badge, Spinner, Toast, Modal (Task 7)
- ✅ Layouts: ClientLayout + BottomNav, AdminLayout + Sidebar (Task 8)
- ✅ Login page with validation and forgot password (Task 9)
- ✅ Appointments API layer (Task 10)
- ✅ Client Home with upcoming appointments (Task 11)
- ✅ 3-step Book Appointment wizard (Task 12)
- ✅ My Appointments with cancel flow (Task 13)
- ✅ Users API layer (Task 14)
- ✅ Admin Dashboard (Task 15)
- ✅ Admin Users with CRUD, search, pagination (Task 16)
- ✅ Admin Appointments with status filters (Task 17)
- ✅ Admin User Detail (Task 18)
- ✅ Client Profile (Task 19)
- ✅ Final verification (Task 20)

**Gaps:** None identified.

**Known limitation:** Admin appointment queries use `userId` of the logged-in admin (backend only exposes `/appointment/user/{userId}`). A future backend endpoint for all appointments would improve admin views.
