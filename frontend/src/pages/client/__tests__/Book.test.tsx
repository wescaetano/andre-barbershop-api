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

  it('navigates to /login with returnTo state when "Entrar" is clicked', async () => {
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
