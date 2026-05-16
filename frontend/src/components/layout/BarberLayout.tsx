import { Outlet } from 'react-router-dom'
import { BarberSidebar } from './BarberSidebar'

export function BarberLayout() {
  return (
    <div className="flex min-h-screen bg-bg-base">
      <BarberSidebar />
      <main className="flex-1 overflow-y-auto p-6">
        <Outlet />
      </main>
    </div>
  )
}
