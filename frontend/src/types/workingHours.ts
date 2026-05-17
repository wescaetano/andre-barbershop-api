export interface WorkingHoursDay {
  id?: number
  dayOfWeek: number  // 0 = Sunday
  isOpen: boolean
  openTime: string   // "HH:mm"
  closeTime: string  // "HH:mm"
}
