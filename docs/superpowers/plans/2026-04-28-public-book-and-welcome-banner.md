# Public Book Page + Welcome Banner — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Allow unauthenticated users to browse available appointment slots, block only on the confirm step with a login modal, and show a persistent welcome banner with role badge on the client home page.

**Architecture:** Four frontend changes (router, Book page, Home page, Login page) plus one backend change to allow anonymous access to the available-slots endpoint. Each task is self-contained and commits independently.

**Tech Stack:** React 19, React Router v7, Zustand, TanStack Query, Vitest + Testing Library (frontend) | ASP.NET Core 8, custom `APIAuthorizationFilter` (backend)

---

## File Map

| File | Change |
|---|---|
| `src/BarberShop.Api/Authorization/AuthorizationAttribute.cs` | Skip filter when action has `[AllowAnonymous]` |
| `src/BarberShop.Api/Controllers/AppointmentController.cs` | Add `[AllowAnonymous]` to `GetAvailableSlots` |
| `frontend/src/router/index.tsx` | Move `/book` outside `PrivateRoute` |
| `frontend/src/components/layout/BottomNav.tsx` | Update link from `/app/book` → `/book` |
| `frontend/src/pages/client/Home.tsx` | Update CTA navigate + add welcome banner |
| `frontend/src/pages/client/Book.tsx` | Add `isAuthenticated` check + auth modal on step 2 |
| `frontend/src/pages/Login.tsx` | Read `location.state.returnTo` and redirect after login |
| `frontend/src/pages/client/__tests__/Book.test.tsx` | New — auth gate tests |
| `frontend/src/pages/client/__tests__/Home.test.tsx` | New — welcome banner tests |

---

## Task 1: Backend — Liberar GetAvailableSlots sem autenticação

**Files:**
- Modify: `src/BarberShop.Api/Authorization/AuthorizationAttribute.cs`
- Modify: `src/BarberShop.Api/Controllers/AppointmentController.cs`

- [ ] **Step 1: Adicionar verificação de `[AllowAnonymous]` no filtro**

Em `AuthorizationAttribute.cs`, no início do método `OnActionExecuting`, antes de qualquer outra lógica, adicionar:

```csharp
public async void OnActionExecuting(ActionExecutingContext filterContext)
{
    // Skip authorization for actions decorated with [AllowAnonymous]
    var endpoint = filterContext.HttpContext.GetEndpoint();
    if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
        return;

    var usuarioId = (string?)filterContext.HttpContext.Items["id"];
    // ... rest of existing code unchanged
```

O arquivo completo de `OnActionExecuting` após a mudança:

```csharp
public async void OnActionExecuting(ActionExecutingContext filterContext)
{
    var endpoint = filterContext.HttpContext.GetEndpoint();
    if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
        return;

    var usuarioId = (string?)filterContext.HttpContext.Items["id"];

    var controller = filterContext.Controller as BaseController;

    controller?.SetUsuarioId((long)Convert.ToDouble(usuarioId));

    foreach (var p in Policies)
    {
        try
        {
            if ((await _authService.AuthorizeAsync(filterContext.HttpContext.User, p)).Succeeded)
                return;
        }
        catch
        {
            break;
        }
    }
    filterContext.Result = new JsonResult(FactoryResponse<dynamic>.Forbiden("Usuário sem autorização de acesso!")) { StatusCode = StatusCodes.Status403Forbidden };
    return;
}
```

- [ ] **Step 2: Adicionar `[AllowAnonymous]` no endpoint `GetAvailableSlots`**

Em `AppointmentController.cs`, adicionar `[AllowAnonymous]` acima de `[HttpGet("available-slots")]`:

```csharp
/// <summary>Retorna os horários disponíveis em uma data (09:00-17:30, intervalos de 30min)</summary>
[AllowAnonymous]
[HttpGet("available-slots")]
public async Task<IActionResult> GetAvailableSlots([FromQuery] DateOnly date)
{
    var result = await _getAvailableSlotsUseCase.ExecuteAsync(new GetAvailableSlotsModel { Date = date });
    return Result(result);
}
```

Adicionar o using necessário no topo do arquivo:
```csharp
using Microsoft.AspNetCore.Authorization;
```

- [ ] **Step 3: Build e verificar**

```bash
dotnet build src/BarberShop.Api/BarberShop.Api.csproj
```

Esperado: `0 Erro(s)`

- [ ] **Step 4: Commit**

```bash
git add src/BarberShop.Api/Authorization/AuthorizationAttribute.cs
git add src/BarberShop.Api/Controllers/AppointmentController.cs
git commit -m "feat: allow anonymous access to available-slots endpoint"
```

---

## Task 2: Frontend — Mover `/book` para rota pública

**Files:**
- Modify: `frontend/src/router/index.tsx`
- Modify: `frontend/src/components/layout/BottomNav.tsx`
- Modify: `frontend/src/pages/client/Home.tsx`

- [ ] **Step 1: Atualizar o router**

Substituir o conteúdo de `frontend/src/router/index.tsx`:

```tsx
import { createBrowserRouter, Navigate } from 'react-router-dom'
import { PrivateRoute } from './PrivateRoute'
import { AdminRoute } from './AdminRoute'
import { ClientLayout } from '../components/layout/ClientLayout'
import { AdminLayout } from '../components/layout/AdminLayout'
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
  { path: '/book', element: <Book /> },
  { path: '/', element: <Navigate to="/login" replace /> },
  {
    path: '/app',
    element: <PrivateRoute />,
    children: [
      {
        element: <ClientLayout />,
        children: [
          { index: true, element: <ClientHome /> },
          { path: 'appointments', element: <ClientAppointments /> },
          { path: 'profile', element: <Profile /> },
        ],
      },
    ],
  },
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
])
```

- [ ] **Step 2: Atualizar link no BottomNav**

Em `frontend/src/components/layout/BottomNav.tsx`, alterar a linha do link "Agendar":

```tsx
const links = [
  { to: '/app', label: 'Início', Icon: Home, end: true },
  { to: '/book', label: 'Agendar', Icon: CalendarPlus, end: false },
  { to: '/app/appointments', label: 'Meus', Icon: Calendar, end: false },
  { to: '/app/profile', label: 'Perfil', Icon: User, end: false },
]
```

- [ ] **Step 3: Atualizar navegação no botão CTA da Home**

Em `frontend/src/pages/client/Home.tsx`, alterar as duas ocorrências de `navigate('/app/book')` para `navigate('/book')`:

```tsx
<Button fullWidth size="lg" onClick={() => navigate('/book')}>
  <CalendarPlus size={18} />
  Agendar Agora
</Button>
```

```tsx
<Button size="sm" onClick={() => navigate('/book')}>Fazer primeiro agendamento</Button>
```

- [ ] **Step 4: Commit**

```bash
git add frontend/src/router/index.tsx
git add frontend/src/components/layout/BottomNav.tsx
git add frontend/src/pages/client/Home.tsx
git commit -m "feat: make /book route public and update nav links"
```

---

## Task 3: Frontend — Modal de autenticação no passo Confirmar

**Files:**
- Create: `frontend/src/pages/client/__tests__/Book.test.tsx`
- Modify: `frontend/src/pages/client/Book.tsx`

- [ ] **Step 1: Criar o arquivo de teste**

Criar `frontend/src/pages/client/__tests__/Book.test.tsx`:

```tsx
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import Book from '../Book'
import { useAuthStore } from '../../../store/authStore'

vi.mock('../../../store/authStore', () => ({
  useAuthStore: vi.fn(),
}))

const mockNavigate = vi.fn()
vi.mock('react-router-dom', async (importOriginal) => {
  const actual = await importOriginal<typeof import('react-router-dom')>()
  return { ...actual, useNavigate: () => mockNavigate }
})

vi.mock('../../../api/appointments', () => ({
  appointmentsApi: {
    getAvailableSlots: vi.fn().mockResolvedValue(['09:00', '09:30', '10:00']),
    create: vi.fn(),
  },
}))

vi.mock('../../../components/ui/Toast', () => ({
  useToast: () => vi.fn(),
}))

vi.mock('../../../hooks/useApiError', () => ({
  useApiError: () => ({ getMessage: (m: string) => m }),
}))

function renderBook() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <Book />
      </MemoryRouter>
    </QueryClientProvider>
  )
}

describe('Book — auth gate', () => {
  beforeEach(() => {
    vi.mocked(useAuthStore).mockImplementation((selector: any) =>
      selector({ isAuthenticated: false, userId: null })
    )
    mockNavigate.mockClear()
  })

  it('shows auth modal on confirm step when unauthenticated', async () => {
    renderBook()

    // Step 0: click first available (non-disabled) day
    const dayButtons = screen
      .getAllByRole('button')
      .filter((b) => /^\d+$/.test(b.textContent ?? '') && !b.hasAttribute('disabled'))
    await userEvent.click(dayButtons[0])
    await userEvent.click(screen.getByText('Próximo'))

    // Step 1: wait for slots and select one
    const slot = await screen.findByRole('button', { name: '09:00' })
    await userEvent.click(slot)
    await userEvent.click(screen.getByText('Próximo'))

    // Step 2: auth modal should be visible
    expect(
      screen.getByText('Para finalizar o agendamento, crie uma conta ou entre na sua.')
    ).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Entrar/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Criar conta/i })).toBeInTheDocument()
  })

  it('navigates to /login when "Entrar" is clicked', async () => {
    renderBook()

    const dayButtons = screen
      .getAllByRole('button')
      .filter((b) => /^\d+$/.test(b.textContent ?? '') && !b.hasAttribute('disabled'))
    await userEvent.click(dayButtons[0])
    await userEvent.click(screen.getByText('Próximo'))
    const slot = await screen.findByRole('button', { name: '09:00' })
    await userEvent.click(slot)
    await userEvent.click(screen.getByText('Próximo'))

    await userEvent.click(screen.getByRole('button', { name: /Entrar/i }))
    expect(mockNavigate).toHaveBeenCalledWith('/login', { state: { returnTo: '/book' } })
  })
})
```

- [ ] **Step 2: Rodar o teste e confirmar que falha**

```bash
cd frontend && npx vitest run src/pages/client/__tests__/Book.test.tsx
```

Esperado: FAIL — `isAuthenticated` não existe no store lido pelo componente ainda.

- [ ] **Step 3: Adicionar lógica no `Book.tsx`**

Em `frontend/src/pages/client/Book.tsx`, adicionar `isAuthenticated` ao store read (linha após `userId`):

```tsx
const userId = useAuthStore((s) => s.userId)
const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
```

Adicionar import do `Modal` no topo (após os imports existentes):

```tsx
import { Modal } from '../../components/ui/Modal'
```

Adicionar o modal de autenticação dentro do `<div className="min-h-screen ...">`, logo após a `<div>` do step indicator e antes do `<div className="flex-1 px-5 ...">`:

```tsx
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
```

O bloco fica entre a `</div>` que fecha o step indicator e a `<div className="flex-1 px-5 pt-6 pb-24">`:

```tsx
      {/* Step indicator */}
      <div className="px-5 pt-4 flex items-center gap-2">
        {/* ... existing step indicator ... */}
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
          <Button fullWidth size="lg" onClick={() => navigate('/login')}>
            Entrar
          </Button>
          <Button fullWidth size="lg" variant="ghost" onClick={() => navigate('/login?mode=register')}>
            Criar conta
          </Button>
        </div>
      </Modal>

      <div className="flex-1 px-5 pt-6 pb-24">
        {/* ... existing step content ... */}
```

- [ ] **Step 4: Rodar o teste e confirmar que passa**

```bash
cd frontend && npx vitest run src/pages/client/__tests__/Book.test.tsx
```

Esperado: PASS — 2 testes passando.

- [ ] **Step 5: Commit**

```bash
git add frontend/src/pages/client/__tests__/Book.test.tsx
git add frontend/src/pages/client/Book.tsx
git commit -m "feat: add auth gate modal on book confirm step for unauthenticated users"
```

---

## Task 4: Frontend — Redirect de volta ao /book após login

**Files:**
- Modify: `frontend/src/pages/Login.tsx`

- [ ] **Step 1: Ler `location.state.returnTo` e usá-lo após login**

Em `frontend/src/pages/Login.tsx`, adicionar `useLocation` ao import do react-router-dom:

```tsx
import { useNavigate, useLocation } from 'react-router-dom'
```

Logo após os hooks existentes no corpo do componente, adicionar:

```tsx
const location = useLocation()
const returnTo = (location.state as { returnTo?: string } | null)?.returnTo
```

No `onSubmit`, substituir a navegação atual:

```tsx
// antes:
navigate(data.modulesAssembled.moduleProfileUser.some((m) => m.name === 'Users') ? '/admin' : '/app', { replace: true })

// depois:
const isAdmin = data.modulesAssembled.moduleProfileUser.some((m) => m.name === 'Users')
navigate(returnTo ?? (isAdmin ? '/admin' : '/app'), { replace: true })
```

- [ ] **Step 2: Commit**

```bash
git add frontend/src/pages/Login.tsx
git commit -m "feat: redirect back to returnTo path after login"
```

---

## Task 5: Frontend — Banner de boas-vindas na Home

**Files:**
- Create: `frontend/src/pages/client/__tests__/Home.test.tsx`
- Modify: `frontend/src/pages/client/Home.tsx`

- [ ] **Step 1: Criar arquivo de teste**

Criar `frontend/src/pages/client/__tests__/Home.test.tsx`:

```tsx
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import ClientHome from '../Home'
import { useAuthStore } from '../../../store/authStore'

vi.mock('../../../store/authStore', () => ({
  useAuthStore: vi.fn(),
}))

vi.mock('react-router-dom', async (importOriginal) => {
  const actual = await importOriginal<typeof import('react-router-dom')>()
  return { ...actual, useNavigate: () => vi.fn() }
})

vi.mock('../../../api/appointments', () => ({
  appointmentsApi: {
    getByUser: vi.fn().mockResolvedValue([]),
  },
}))

function renderHome() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <ClientHome />
      </MemoryRouter>
    </QueryClientProvider>
  )
}

describe('ClientHome — welcome banner', () => {
  beforeEach(() => {
    vi.mocked(useAuthStore).mockImplementation((selector: any) =>
      selector({ userId: 1, userName: 'João Silva', role: 'client', isAuthenticated: true })
    )
  })

  it('renders welcome message', () => {
    renderHome()
    expect(screen.getByText('Seja bem-vindo ao BarberAgenda!')).toBeInTheDocument()
  })

  it('shows "Cliente" badge when role is client', () => {
    renderHome()
    expect(screen.getByText('Cliente')).toBeInTheDocument()
  })

  it('shows "Barbeiro" badge when role is admin', () => {
    vi.mocked(useAuthStore).mockImplementation((selector: any) =>
      selector({ userId: 1, userName: 'Carlos', role: 'admin', isAuthenticated: true })
    )
    renderHome()
    expect(screen.getByText('Barbeiro')).toBeInTheDocument()
  })
})
```

- [ ] **Step 2: Rodar o teste e confirmar que falha**

```bash
cd frontend && npx vitest run src/pages/client/__tests__/Home.test.tsx
```

Esperado: FAIL — banner não existe ainda.

- [ ] **Step 3: Adicionar banner na `Home.tsx`**

Em `frontend/src/pages/client/Home.tsx`, adicionar o import do `Badge`:

```tsx
import { Badge } from '../../components/ui/Badge'
```

Adicionar seletor de `role` no store (junto aos outros seletores existentes):

```tsx
const userId = useAuthStore((s) => s.userId)
const userName = useAuthStore((s) => s.userName)
const role = useAuthStore((s) => s.role)
```

Adicionar o banner entre o header (`.clip-diagonal`) e o `<div className="px-5 pt-6 ...">`:

```tsx
      {/* Role banner */}
      <div className="px-5 py-3 bg-bg-surface border-b border-border flex items-center justify-between">
        <span className="text-sm font-body text-text-secondary">
          Seja bem-vindo ao BarberAgenda!
        </span>
        <Badge
          className={
            role === 'admin'
              ? 'bg-accent/10 text-accent border border-accent/30'
              : 'bg-blue-500/10 text-blue-400 border border-blue-500/30'
          }
        >
          {role === 'admin' ? 'Barbeiro' : 'Cliente'}
        </Badge>
      </div>
```

- [ ] **Step 4: Rodar o teste e confirmar que passa**

```bash
cd frontend && npx vitest run src/pages/client/__tests__/Home.test.tsx
```

Esperado: PASS — 3 testes passando.

- [ ] **Step 5: Rodar todos os testes do frontend**

```bash
cd frontend && npx vitest run
```

Esperado: todos os testes passando, sem regressões.

- [ ] **Step 6: Commit**

```bash
git add frontend/src/pages/client/__tests__/Home.test.tsx
git add frontend/src/pages/client/Home.tsx
git commit -m "feat: add welcome banner with role badge to client home"
```
