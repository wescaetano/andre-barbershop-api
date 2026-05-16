import { createBrowserRouter, Navigate } from 'react-router-dom'
import { PrivateRoute } from './PrivateRoute'
import { AdminRoute } from './AdminRoute'
import { BarberRoute } from './BarberRoute'
import { ClientLayout } from '../components/layout/ClientLayout'
import { AdminLayout } from '../components/layout/AdminLayout'
import { BarberLayout } from '../components/layout/BarberLayout'
import Login from '../pages/Login'
import ClientHome from '../pages/client/Home'
import Book from '../pages/client/Book'
import ClientAppointments from '../pages/client/Appointments'
import Profile from '../pages/client/Profile'
import AdminDashboard from '../pages/admin/Dashboard'
import AdminSchedule from '../pages/admin/Schedule'
import AdminAppointments from '../pages/admin/Appointments'
import AdminUsers from '../pages/admin/Users'
import AdminServices from '../pages/admin/Services'
import AdminBarbers from '../pages/admin/Barbers'
import UserDetail from '../pages/admin/UserDetail'
import BarberDashboard from '../pages/barber/Dashboard'
import BarberAgenda from '../pages/barber/Agenda'
import BarberSchedule from '../pages/barber/Schedule'
import BarberWorkingHours from '../pages/barber/WorkingHours'
import BarberBlocks from '../pages/barber/Blocks'
import { useAuthStore } from '../store/authStore'

function RootRedirect() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const role = useAuthStore((s) => s.role)
  if (!isAuthenticated) return <Navigate to="/book" replace />
  if (role === 'admin') return <Navigate to="/admin" replace />
  if (role === 'barber') return <Navigate to="/barber" replace />
  return <Navigate to="/app" replace />
}

export const router = createBrowserRouter([
  { path: '/login', element: <Login /> },
  { path: '/book', element: <Book /> },
  { path: '/', element: <RootRedirect /> },
  {
    path: '/app',
    element: <PrivateRoute />,
    children: [
      {
        element: <ClientLayout />,
        children: [
          { index: true, element: <ClientHome /> },
          { path: 'appointments', element: <ClientAppointments /> },
          { path: 'profile', element: <Profile /> },
        ],
      },
    ],
  },
  {
    path: '/admin',
    element: <AdminRoute />,
    children: [
      {
        element: <AdminLayout />,
        children: [
          { index: true, element: <AdminDashboard /> },
          { path: 'schedule', element: <AdminSchedule /> },
          { path: 'appointments', element: <AdminAppointments /> },
          { path: 'users', element: <AdminUsers /> },
          { path: 'users/:id', element: <UserDetail /> },
          { path: 'services', element: <AdminServices /> },
          { path: 'barbers', element: <AdminBarbers /> },
        ],
      },
    ],
  },
  {
    path: '/barber',
    element: <BarberRoute />,
    children: [
      {
        element: <BarberLayout />,
        children: [
          { index: true, element: <BarberDashboard /> },
          { path: 'agenda', element: <BarberAgenda /> },
          { path: 'schedule', element: <BarberSchedule /> },
          { path: 'working-hours', element: <BarberWorkingHours /> },
          { path: 'blocks', element: <BarberBlocks /> },
        ],
      },
    ],
  },
])
