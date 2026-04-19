import { describe, it, expect, beforeEach } from 'vitest'
import { useAuthStore } from '../authStore'
import { act } from '@testing-library/react'

const baseModules = { infoProfile: { name: '', idProfile: 0 }, moduleProfileUser: [] }

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
      modulesAssembled: baseModules,
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
      modulesAssembled: {
        infoProfile: { name: 'Admin', idProfile: 1 },
        moduleProfileUser: [{ name: 'Users', idModule: 1, visualize: true, edit: true, register: true, inactivate: true, exclude: true }],
      },
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
      modulesAssembled: {
        infoProfile: { name: 'Cliente', idProfile: 2 },
        moduleProfileUser: [{ name: 'Appointments', idModule: 2, visualize: true, edit: false, register: true, inactivate: false, exclude: false }],
      },
      dtExpiration: '',
      dtCreation: '',
    }))
    expect(useAuthStore.getState().role).toBe('client')
  })

  it('should clear state on logout', () => {
    act(() => useAuthStore.getState().setAuth({
      userId: 1, userName: 'X', userEmail: 'x@x.com',
      accessToken: 'tok', refreshToken: 'ref',
      modulesAssembled: baseModules, dtExpiration: '', dtCreation: '',
    }))
    act(() => useAuthStore.getState().logout())
    expect(useAuthStore.getState().isAuthenticated).toBe(false)
    expect(useAuthStore.getState().accessToken).toBeNull()
  })
})
