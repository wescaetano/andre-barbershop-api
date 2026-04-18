import { describe, it, expect, vi, beforeEach } from 'vitest'

describe('api client', () => {
  it('should have baseURL from env', async () => {
    const { apiClient } = await import('../client')
    expect(apiClient.defaults.baseURL).toBe(import.meta.env.VITE_API_URL || 'http://localhost:5000')
  })
})
