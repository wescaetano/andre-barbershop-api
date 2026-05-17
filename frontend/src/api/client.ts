import axios, { AxiosError } from 'axios'
import { useAuthStore } from '../store/authStore'

const BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000'

export const apiClient = axios.create({
  baseURL: BASE_URL,
  headers: { 'Content-Type': 'application/json' },
})

// Dedicated client for refresh calls (avoids circular interceptor triggering)
const refreshClient = axios.create({ baseURL: BASE_URL })

// Keep the Authorization default in sync with Zustand store changes.
// This fires synchronously on: login (setAuth), logout, token refresh, AND
// persist hydration (when Zustand reads from localStorage and calls set()).
useAuthStore.subscribe((state) => {
  if (state.accessToken) {
    apiClient.defaults.headers.common['Authorization'] = `Bearer ${state.accessToken}`
  } else {
    delete apiClient.defaults.headers.common['Authorization']
  }
})

// Also initialize from current state (covers synchronous-storage case where
// persist hydrates the store before any subscriber is registered)
const { accessToken: _initial } = useAuthStore.getState()
if (_initial) {
  apiClient.defaults.headers.common['Authorization'] = `Bearer ${_initial}`
}

// Silent refresh on 401
let isRefreshing = false
let refreshQueue: Array<{ resolve: (token: string) => void; reject: (err: unknown) => void }> = []

function drainQueue(token: string) {
  refreshQueue.forEach(({ resolve }) => resolve(token))
  refreshQueue = []
}

function rejectQueue(err: unknown) {
  refreshQueue.forEach(({ reject }) => reject(err))
  refreshQueue = []
}

apiClient.interceptors.response.use(
  (res) => res,
  async (error: AxiosError) => {
    const original = error.config as typeof error.config & { _retry?: boolean }
    if (error.response?.status !== 401 || original?._retry) {
      return Promise.reject(error)
    }
    original._retry = true

    if (isRefreshing) {
      return new Promise<string>((resolve, reject) => {
        refreshQueue.push({ resolve, reject })
      }).then((token) => {
        original!.headers!['Authorization'] = `Bearer ${token}`
        return apiClient(original!)
      })
    }

    isRefreshing = true
    try {
      const { refreshToken, setAuth } = useAuthStore.getState()
      if (!refreshToken) throw new Error('no refresh token')

      const { data } = await refreshClient.post('/auth/refresh-token', null, {
        params: { token: refreshToken },
      })
      const newData = data.data
      setAuth(newData) // triggers subscriber → updates defaults.headers.common

      isRefreshing = false
      drainQueue(newData.accessToken)
      original!.headers!['Authorization'] = `Bearer ${newData.accessToken}`
      return apiClient(original!)
    } catch (err) {
      isRefreshing = false
      rejectQueue(err)
      useAuthStore.getState().logout()
      window.location.href = '/login'
      return Promise.reject(error)
    }
  },
)
