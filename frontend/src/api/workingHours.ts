import { apiClient } from './client'
import type { ApiResponse } from '../types/auth'
import type { WorkingHoursDay } from '../types/workingHours'

export const workingHoursApi = {
  getByBarber: async (barberId: number): Promise<WorkingHoursDay[]> => {
    const { data } = await apiClient.get<ApiResponse<WorkingHoursDay[]>>(`/workinghours/${barberId}`)
    return data.data ?? []
  },

  upsert: async (barberId: number, days: WorkingHoursDay[]) => {
    const { data } = await apiClient.put<ApiResponse<null>>('/workinghours', { barberId, days })
    if (!data.success) throw new Error(data.responseLabel)
    return data
  },
}
