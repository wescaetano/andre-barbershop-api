export interface Barber {
  id: number
  userId: number
  displayName: string
  isActive: boolean
}

export interface CreateBarberRequest {
  name: string
  email: string
  displayName: string
}

export interface UpdateBarberRequest {
  id: number
  displayName: string
}

export interface ChangeBarberStatusRequest {
  id: number
  isActive: boolean
}
