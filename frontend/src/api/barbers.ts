import { apiClient } from './client'
import type { ApiResponse } from '../types/auth'
import type { Barber, CreateBarberRequest, UpdateBarberRequest, ChangeBarberStatusRequest } from '../types/barber'

export const barbersApi = {
  getActive: async (): Promise<Barber[]> => {
    const { data } = await apiClient.get<ApiResponse<Barber[]>>('/barber')
    return data.data ?? []
  },

  getAll: async (): Promise<Barber[]> => {
    const { data } = await apiClient.get<ApiResponse<Barber[]>>('/barber/all')
    return data.data ?? []
  },

  create: async (payload: CreateBarberRequest) => {
    const { data } = await apiClient.post<ApiResponse<Barber>>('/barber', payload)
    if (!data.success) throw new Error(data.responseLabel)
    return data
  },

  update: async (payload: UpdateBarberRequest) => {
    const { data } = await apiClient.put<ApiResponse<Barber>>('/barber', payload)
    if (!data.success) throw new Error(data.responseLabel)
    return data
  },

  changeStatus: async (payload: ChangeBarberStatusRequest) => {
    const { data } = await apiClient.patch<ApiResponse<null>>('/barber/status', payload)
    if (!data.success) throw new Error(data.responseLabel)
    return data
  },
}
