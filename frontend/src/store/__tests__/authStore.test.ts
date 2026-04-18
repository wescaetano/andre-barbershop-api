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
