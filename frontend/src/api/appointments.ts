import { apiClient } from './client'
import type { ApiResponse } from '../types/auth'
import type { Appointment, CreateAppointmentRequest, CancelAppointmentRequest } from '../types/appointment'

export const appointmentsApi = {
  getAvailableSlots: async (date: string, serviceId: number, barberId: number): Promise<string[]> => {
    const { data } = await apiClient.get<ApiResponse<string[]>>('/appointment/available-slots', {
      params: { date, serviceId, barberId },
    })
    return data.data ?? []
  },

  create: async (body: CreateAppointmentRequest): Promise<Appointment> => {
    const { data } = await apiClient.post<ApiResponse<Appointment>>('/appointment', body)
    if (!data.success || !data.data) throw new Error(data.responseLabel)
    return data.data
  },

  cancel: async (body: CancelAppointmentRequest): Promise<void> => {
    const { data } = await apiClient.patch<ApiResponse>('/appointment/cancel', body)
    if (!data.success) throw new Error(data.responseLabel)
  },

  getByUser: async (userId: number): Promise<Appointment[]> => {
    const { data } = await apiClient.get<ApiResponse<Appointment[]>>(`/appointment/user/${userId}`)
    return data.data ?? []
  },

  getById: async (id: number): Promise<Appointment> => {
    const { data } = await apiClient.get<ApiResponse<Appointment>>(`/appointment/${id}`)
    if (!data.data) throw new Error(data.responseLabel)
    return data.data
  },
}
