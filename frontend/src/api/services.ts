import { apiClient } from './client'
import type { ApiResponse } from '../types/auth'
import type { Service } from '../types/service'

export const servicesApi = {
  getActive: async (): Promise<Service[]> => {
    const { data } = await apiClient.get<ApiResponse<Service[]>>('/service')
    return data.data ?? []
  },

  getAll: async (): Promise<Service[]> => {
    const { data } = await apiClient.get<ApiResponse<Service[]>>('/service/all')
    return data.data ?? []
  },

  create: async (payload: { name: string; durationMinutes: number; price: number }) => {
    const { data } = await apiClient.post<ApiResponse<Service>>('/service', payload)
    if (!data.success) throw new Error(data.responseLabel)
    return data
  },

  update: async (payload: { id: number; name: string; durationMinutes: number; price: number }) => {
    const { data } = await apiClient.put<ApiResponse<Service>>('/service', payload)
    if (!data.success) throw new Error(data.responseLabel)
    return data
  },

  changeStatus: async (id: number, isActive: boolean) => {
    const { data } = await apiClient.patch<ApiResponse<null>>('/service/status', { id, isActive })
    if (!data.success) throw new Error(data.responseLabel)
    return data
  },
}
