export interface ModulePermissions {
  Visualize: boolean
  Edit: boolean
  Register: boolean
  Inactivate: boolean
  Exclude: boolean
}

export interface LoginResponse {
  userId: number
  userName: string
  userEmail: string
  userProfileImage?: string
  accessToken: string
  refreshToken: string
  modulesAssembled: Record<string, ModulePermissions>
  dtCreation: string
  dtExpiration: string
}

export interface ApiResponse<T = unknown> {
  success: boolean
  statusCode: number
  responseLabel: string
  description?: string
  data?: T
}

export type UserRole = 'admin' | 'client'
