import { useNavigate } from 'react-router-dom'
import { useAuthStore } from '../store/authStore'
import { authApi } from '../api/auth'

export function useAuth() {
  const navigate = useNavigate()
  const { setAuth, logout: storeLogout, isAuthenticated, role, userId } = useAuthStore()

  const login = async (email: string, password: string) => {
    const res = await authApi.login(email, password)
    if (!res.success || !res.data) throw new Error(res.responseLabel)
    setAuth(res.data)
    return res.data
  }

  const logout = () => {
    storeLogout()
    navigate('/login', { replace: true })
  }

  return { login, logout, isAuthenticated, role, userId }
}
