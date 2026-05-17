import { useEffect, useState, type ReactNode } from 'react'
import { RouterProvider } from 'react-router-dom'
import { router } from './router'
import { useAuthStore } from './store/authStore'

function HydrationGate({ children }: { children: ReactNode }) {
  const [ready, setReady] = useState(false)

  useEffect(() => {
    // If the store is already hydrated (sync storage path), mark ready immediately.
    if (useAuthStore.persist.hasHydrated()) {
      setReady(true)
      return
    }
    // Otherwise wait for the async hydration callback.
    return useAuthStore.persist.onFinishHydration(() => setReady(true))
  }, [])

  if (!ready) return null
  return <>{children}</>
}

export default function App() {
  return (
    <HydrationGate>
      <RouterProvider router={router} />
    </HydrationGate>
  )
}
