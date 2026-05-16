export type AppointmentStatus = 0 | 1 | 2 // WaitingPayment | Paid | Cancelled

export interface Appointment {
  id: number
  userId: number
  startTime: string
  endTime: string
  status: AppointmentStatus
  creationDate: string
  updateDate?: string
}

export interface CreateAppointmentRequest {
  userId: number
  date: string       // "YYYY-MM-DD"
  startTime: string  // "HH:mm"
  serviceId: number
  barberId: number
}

export interface CancelAppointmentRequest {
  appointmentId: number
  userId: number
}
