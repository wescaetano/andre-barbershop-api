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
      modulesAssembled: { infoProfile: { name: '', idProfile: 0 }, moduleProfileUser: [] },
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
          role: data.modulesAssembled.moduleProfileUser.some((m) => m.name === 'Users')
            ? 'admin'
            : data.modulesAssembled.moduleProfileUser.some((m) => m.name === 'Barber')
              ? 'barber'
              : 'client',
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
          modulesAssembled: { infoProfile: { name: '', idProfile: 0 }, moduleProfileUser: [] },
          role: null,
          isAuthenticated: false,
        }),
    }),
    { name: 'auth-storage' },
  ),
)
