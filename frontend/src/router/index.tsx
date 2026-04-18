import { createBrowserRouter, Navigate } from 'react-router-dom'
import { PrivateRoute } from './PrivateRoute'
import { AdminRoute } from './AdminRoute'
import Login from '../pages/Login'
import ClientHome from '../pages/client/Home'
import Book from '../pages/client/Book'
import ClientAppointments from '../pages/client/Appointments'
import Profile from '../pages/client/Profile'
import AdminDashboard from '../pages/admin/Dashboard'
import AdminAppointments from '../pages/admin/Appointments'
import AdminUsers from '../pages/admin/Users'
import UserDetail from '../pages/admin/UserDetail'

export const router = createBrowserRouter([
  { path: '/login', element: <Login /> },
  { path: '/', element: <Navigate to="/login" replace /> },
  {
    path: '/app',
    element: <PrivateRoute />,
    children: [
      { index: true, element: <ClientHome /> },
      { path: 'book', element: <Book /> },
      { path: 'appointments', element: <ClientAppointments /> },
      { path: 'profile', element: <Profile /> },
    ],
  },
  {
    path: '/admin',
    element: <AdminRoute />,
    children: [
      { index: true, element: <AdminDashboard /> },
      { path: 'appointments', element: <AdminAppointments /> },
      { path: 'users', element: <AdminUsers /> },
      { path: 'users/:id', element: <UserDetail /> },
    ],
  },
])
