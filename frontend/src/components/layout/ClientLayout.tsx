import { Outlet } from 'react-router-dom'
import { BottomNav } from './BottomNav'

export function ClientLayout() {
  return (
    <div className="min-h-screen bg-bg-base flex flex-col">
      <main className="flex-1 pb-20 overflow-y-auto">
        <Outlet />
      </main>
      <BottomNav />
    </div>
  )
}
