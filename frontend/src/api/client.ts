import axios, { AxiosError } from 'axios'

const BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000'

export const apiClient = axios.create({
  baseURL: BASE_URL,
  headers: { 'Content-Type': 'application/json' },
})

// Dedicated client for refresh calls (avoids circular interceptor triggering)
const refreshClient = axios.create({ baseURL: BASE_URL })

// Attach token to every request
apiClient.interceptors.request.use((config) => {
  const raw = localStorage.getItem('auth-storage')
  if (raw) {
    try {
      const parsed = JSON.parse(raw)
      const token = parsed?.state?.accessToken
      if (token) config.headers.Authorization = `Bearer ${token}`
    } catch (e) {
      console.warn('auth-storage parse error:', e)
    }
  }
  return config
})

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
        original!.headers!.Authorization = `Bearer ${token}`
        return apiClient(original!)
      })
    }

    isRefreshing = true
    try {
      const raw = localStorage.getItem('auth-storage')
      const parsed = raw ? JSON.parse(raw) : null
      const refreshToken = parsed?.state?.refreshToken
      if (!refreshToken) throw new Error('no refresh token')

      const { data } = await refreshClient.post('/auth/refresh-token', null, {
        params: { token: refreshToken },
      })
      const newAccessToken: string = data.data.accessToken
      const newRefreshToken: string = data.data.refreshToken

      // Update persisted store (reuse already-parsed object)
      if (parsed) {
        parsed.state.accessToken = newAccessToken
        parsed.state.refreshToken = newRefreshToken
        try {
          localStorage.setItem('auth-storage', JSON.stringify(parsed))
        } catch (e) {
          console.warn('Failed to persist refreshed tokens:', e)
        }
      }

      isRefreshing = false
      drainQueue(newAccessToken)
      original!.headers!.Authorization = `Bearer ${newAccessToken}`
      return apiClient(original!)
    } catch (err) {
      isRefreshing = false
      rejectQueue(err)
      localStorage.removeItem('auth-storage')
      window.location.href = '/login'
      return Promise.reject(error)
    }
  },
)
