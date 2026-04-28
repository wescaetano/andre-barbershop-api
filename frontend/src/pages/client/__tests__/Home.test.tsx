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
