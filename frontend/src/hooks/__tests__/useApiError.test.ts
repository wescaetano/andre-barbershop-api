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
