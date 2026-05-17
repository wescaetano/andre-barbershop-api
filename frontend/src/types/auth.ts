export interface ModuleProfileUser {
  name: string
  idModule: number
  visualize: boolean
  edit: boolean
  register: boolean
  inactivate: boolean
  exclude: boolean
}

export interface ModulesAssembled {
  infoProfile: {
    name: string
    idProfile: number
  }
  moduleProfileUser: ModuleProfileUser[]
}

export interface LoginResponse {
  userId: number
  userName: string
  userEmail: string
  userProfileImage?: string
  accessToken: string
  refreshToken: string
  modulesAssembled: ModulesAssembled
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

export type UserRole = 'admin' | 'barber' | 'client'
