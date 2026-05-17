import { apiClient } from './client'
import type { ApiResponse } from '../types/auth'
import type {
  User,
  CreateUserRequest,
  UpdateUserRequest,
  GetUsersPaginatedRequest,
  PaginatedResponse,
} from '../types/user'

interface RawPaginatedResponse<T> {
  data: T[]
  total: number
  page: number
  limit: number
  pages: number
}

export const usersApi = {
  list: async (params: GetUsersPaginatedRequest = {}): Promise<PaginatedResponse<User>> => {
    const { data } = await apiClient.get<ApiResponse<RawPaginatedResponse<User>>>('/user', { params })
    const raw = data.data
    if (!raw) return { items: [], totalCount: 0, pageNumber: 1, pageSize: 20, totalPages: 0 }
    return {
      items: raw.data,
      totalCount: Number(raw.total),
      pageNumber: Number(raw.page),
      pageSize: Number(raw.limit),
      totalPages: Number(raw.pages),
    }
  },

  getById: async (id: number): Promise<User> => {
    const { data } = await apiClient.get<ApiResponse<User>>(`/user/${id}`)
    if (!data.data) throw new Error(data.responseLabel)
    return data.data
  },

  create: async (body: CreateUserRequest): Promise<void> => {
    const { data } = await apiClient.post<ApiResponse>('/user', body)
    if (!data.success) throw new Error(data.responseLabel)
  },

  update: async (body: UpdateUserRequest): Promise<void> => {
    const { data } = await apiClient.put<ApiResponse>('/user', body)
    if (!data.success) throw new Error(data.responseLabel)
  },

  changeStatus: async (id: number, status: 0 | 1): Promise<void> => {
    const { data } = await apiClient.patch<ApiResponse>('/user/status', { id, status })
    if (!data.success) throw new Error(data.responseLabel)
  },

  delete: async (id: number): Promise<void> => {
    const { data } = await apiClient.delete<ApiResponse>(`/user/${id}`)
    if (!data.success) throw new Error(data.responseLabel)
  },
}
