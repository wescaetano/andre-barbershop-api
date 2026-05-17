export type UserStatus = 0 | 1 // Inativo | Ativo

export interface User {
  id: number
  name: string
  email: string
  imageUrl?: string
  status: UserStatus
  creationDate: string
  updateDate?: string
  profilesUsers?: { profile: { id: number; name: string } }[]
}

export interface CreateUserRequest {
  name: string
  email: string
  password: string
  imageBase64?: string
  accessProfile: number
}

export interface UpdateUserRequest {
  id: number
  name?: string
  email?: string
  imageUrl?: string
  accessProfile?: number
}

export interface GetUsersPaginatedRequest {
  name?: string
  email?: string
  status?: UserStatus
  pageSize?: number
  pageNumber?: number
  sortField?: 'Id' | 'Name' | 'Email' | 'CreationDate'
  sortOrder?: 'asc' | 'desc'
}

export interface PaginatedResponse<T> {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}
