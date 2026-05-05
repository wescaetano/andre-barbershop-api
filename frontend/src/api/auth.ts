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

  register: async (name: string, email: string) => {
    const { data } = await apiClient.post<ApiResponse>('/auth/register', { name, email })
    return data
  },
}
