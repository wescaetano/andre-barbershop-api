import { Navigate, Outlet } from 'react-router-dom'
import { useAuthStore } from '../store/authStore'

export function BarberRoute() {
  const { isAuthenticated, role } = useAuthStore()
  if (!isAuthenticated) return <Navigate to="/login" replace />
  if (role !== 'barber') return <Navigate to="/" replace />
  return <Outlet />
}
