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
