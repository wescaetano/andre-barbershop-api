import axios, { AxiosError } from 'axios'

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000',
  headers: { 'Content-Type': 'application/json' },
})

// Attach token to every request
apiClient.interceptors.request.use((config) => {
  const raw = localStorage.getItem('auth-storage')
  if (raw) {
    try {
      const parsed = JSON.parse(raw)
      const token = parsed?.state?.accessToken
      if (token) config.headers.Authorization = `Bearer ${token}`
    } catch {}
  }
  return config
})

// Silent refresh on 401
let isRefreshing = false
let refreshQueue: Array<(token: string) => void> = []

apiClient.interceptors.response.use(
  (res) => res,
  async (error: AxiosError) => {
    const original = error.config as typeof error.config & { _retry?: boolean }
    if (error.response?.status !== 401 || original?._retry) {
      return Promise.reject(error)
    }
    original._retry = true

    if (isRefreshing) {
      return new Promise((resolve) => {
        refreshQueue.push((token) => {
          original!.headers!.Authorization = `Bearer ${token}`
          resolve(apiClient(original!))
        })
      })
    }

    isRefreshing = true
    try {
      const raw = localStorage.getItem('auth-storage')
      const refreshToken = raw ? JSON.parse(raw)?.state?.refreshToken : null
      if (!refreshToken) throw new Error('no refresh token')

      const { data } = await axios.post(
        `${import.meta.env.VITE_API_URL || 'http://localhost:5000'}/auth/refresh-token`,
        null,
        { params: { token: refreshToken } },
      )
      const newAccessToken: string = data.data.accessToken
      const newRefreshToken: string = data.data.refreshToken

      // Update persisted store
      const stored = JSON.parse(localStorage.getItem('auth-storage') || '{}')
      stored.state.accessToken = newAccessToken
      stored.state.refreshToken = newRefreshToken
      localStorage.setItem('auth-storage', JSON.stringify(stored))

      refreshQueue.forEach((cb) => cb(newAccessToken))
      refreshQueue = []
      original!.headers!.Authorization = `Bearer ${newAccessToken}`
      return apiClient(original!)
    } catch {
      // Refresh failed — force logout
      localStorage.removeItem('auth-storage')
      window.location.href = '/login'
      return Promise.reject(error)
    } finally {
      isRefreshing = false
    }
  },
)
