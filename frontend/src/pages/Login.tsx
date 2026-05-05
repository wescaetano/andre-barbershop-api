import { useState } from 'react'
import { useNavigate, useLocation, useSearchParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { Scissors, CheckCircle } from 'lucide-react'
import { useAuth } from '../hooks/useAuth'
import { useToast } from '../components/ui/Toast'
import { useApiError } from '../hooks/useApiError'
import { Input } from '../components/ui/Input'
import { Button } from '../components/ui/Button'
import { authApi } from '../api/auth'

const loginSchema = z.object({
  email: z.string().email('E-mail inválido'),
  password: z.string().min(1, 'Senha obrigatória'),
})

const registerSchema = z.object({
  name: z.string().min(1, 'Nome obrigatório'),
  email: z.string().email('E-mail inválido'),
})

type LoginData = z.infer<typeof loginSchema>
type RegisterData = z.infer<typeof registerSchema>

export default function Login() {
  const { login, isAuthenticated, role } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [searchParams] = useSearchParams()
  const mode = searchParams.get('mode')
  const returnTo = (location.state as { returnTo?: string } | null)?.returnTo
  const toast = useToast()
  const { getMessage } = useApiError()
  const [loading, setLoading] = useState(false)
  const [registered, setRegistered] = useState(false)
  const [forgotOpen, setForgotOpen] = useState(false)
  const [forgotEmail, setForgotEmail] = useState('')
  const [forgotLoading, setForgotLoading] = useState(false)

  const loginForm = useForm<LoginData>({ resolver: zodResolver(loginSchema) })
  const registerForm = useForm<RegisterData>({ resolver: zodResolver(registerSchema) })

  if (isAuthenticated) {
    navigate(role === 'admin' ? '/admin' : '/app', { replace: true })
    return null
  }

  const onLogin = async ({ email, password }: LoginData) => {
    setLoading(true)
    try {
      const data = await login(email, password)
      const isAdmin = data.modulesAssembled.moduleProfileUser.some((m) => m.name === 'Users')
      navigate(returnTo ?? (isAdmin ? '/admin' : '/app'), { replace: true })
    } catch (err: unknown) {
      const label = err instanceof Error ? err.message : ''
      toast(getMessage(label, 'Erro ao fazer login.'), 'error')
    } finally {
      setLoading(false)
    }
  }

  const onRegister = async ({ name, email }: RegisterData) => {
    setLoading(true)
    try {
      const res = await authApi.register(name, email)
      if (!res.success) throw new Error(res.responseLabel)
      setRegistered(true)
    } catch (err: unknown) {
      const label = err instanceof Error ? err.message : ''
      toast(getMessage(label, 'Erro ao criar conta.'), 'error')
    } finally {
      setLoading(false)
    }
  }

  const handleForgot = async () => {
    if (!forgotEmail) return
    setForgotLoading(true)
    try {
      await authApi.sendResetPassword(forgotEmail)
      toast('E-mail de recuperação enviado!', 'success')
      setForgotOpen(false)
      setForgotEmail('')
    } catch {
      toast('Erro ao enviar e-mail.', 'error')
    } finally {
      setForgotLoading(false)
    }
  }

  const goToLogin = () => navigate('/login', { state: { returnTo }, replace: true })
  const goToRegister = () => navigate('/login?mode=register', { state: { returnTo }, replace: true })

  return (
    <div className="min-h-screen bg-bg-base flex flex-col items-center justify-center p-6">
      {/* Logo */}
      <div className="mb-10 flex flex-col items-center gap-3">
        <div className="w-14 h-14 bg-accent rounded-sm flex items-center justify-center">
          <Scissors size={28} className="text-white" />
        </div>
        <h1 className="font-display font-extrabold text-3xl tracking-widest uppercase text-text-primary">
          Barber<span className="text-accent">Agenda</span>
        </h1>
        <p className="text-text-secondary text-sm font-body">Barbearia que respeita o seu tempo.</p>
      </div>

      {/* Card */}
      <div className="w-full max-w-sm bg-bg-surface border border-border rounded-sm p-6 flex flex-col gap-5">

        {/* ── Registro: sucesso ── */}
        {mode === 'register' && registered && (
          <div className="flex flex-col items-center gap-4 py-4 text-center">
            <CheckCircle size={40} className="text-accent" strokeWidth={1.5} />
            <div>
              <h2 className="font-display font-bold text-xl uppercase tracking-wide">Conta criada!</h2>
              <p className="text-text-secondary text-sm font-body mt-1">
                Verifique seu e-mail para definir sua senha.
              </p>
            </div>
            <Button fullWidth size="lg" onClick={goToLogin}>
              Ir para o login
            </Button>
          </div>
        )}

        {/* ── Formulário de registro ── */}
        {mode === 'register' && !registered && (
          <>
            <div className="border-l-2 border-accent pl-3">
              <h2 className="font-display font-bold text-xl uppercase tracking-wide">Criar conta</h2>
              <p className="text-text-secondary text-xs font-body mt-0.5">Preencha seus dados para começar</p>
            </div>

            <form onSubmit={registerForm.handleSubmit(onRegister)} className="flex flex-col gap-4" noValidate>
              <Input
                id="name"
                label="Nome"
                type="text"
                placeholder="Seu nome completo"
                error={registerForm.formState.errors.name?.message}
                {...registerForm.register('name')}
              />
              <Input
                id="reg-email"
                label="E-mail"
                type="email"
                placeholder="seuemail@exemplo.com"
                error={registerForm.formState.errors.email?.message}
                {...registerForm.register('email')}
              />
              <Button type="submit" fullWidth size="lg" loading={loading}>
                Criar conta
              </Button>
            </form>

            <button
              onClick={goToLogin}
              className="text-xs text-text-secondary hover:text-accent transition-colors font-body text-center"
            >
              Já tenho conta — <span className="text-accent">Entrar</span>
            </button>
          </>
        )}

        {/* ── Formulário de login ── */}
        {mode !== 'register' && (
          <>
            <div className="border-l-2 border-accent pl-3">
              <h2 className="font-display font-bold text-xl uppercase tracking-wide">Acesse sua conta</h2>
              <p className="text-text-secondary text-xs font-body mt-0.5">Entre com suas credenciais</p>
            </div>

            <form onSubmit={loginForm.handleSubmit(onLogin)} className="flex flex-col gap-4" noValidate>
              <Input
                id="email"
                label="E-mail"
                type="email"
                placeholder="seuemail@exemplo.com"
                error={loginForm.formState.errors.email?.message}
                {...loginForm.register('email')}
              />
              <Input
                id="password"
                label="Senha"
                type="password"
                placeholder="••••••••"
                error={loginForm.formState.errors.password?.message}
                {...loginForm.register('password')}
              />
              <Button type="submit" fullWidth size="lg" loading={loading}>
                Entrar
              </Button>
            </form>

            <div className="flex flex-col gap-2 items-center">
              <button
                onClick={() => setForgotOpen(true)}
                className="text-xs text-text-secondary hover:text-accent transition-colors font-body"
              >
                Esqueci minha senha
              </button>
              <button
                onClick={goToRegister}
                className="text-xs text-text-secondary hover:text-accent transition-colors font-body"
              >
                Não tenho conta — <span className="text-accent">Criar conta</span>
              </button>
            </div>
          </>
        )}
      </div>

      {/* Forgot password inline panel */}
      {forgotOpen && (
        <div className="w-full max-w-sm bg-bg-elevated border border-border rounded-sm p-5 mt-3 flex flex-col gap-3">
          <p className="text-sm font-body text-text-primary">Digite seu e-mail para receber o link de recuperação:</p>
          <Input
            placeholder="seuemail@exemplo.com"
            type="email"
            value={forgotEmail}
            onChange={(e) => setForgotEmail(e.target.value)}
          />
          <div className="flex gap-2">
            <Button variant="ghost" size="sm" onClick={() => setForgotOpen(false)} className="flex-1">
              Cancelar
            </Button>
            <Button size="sm" loading={forgotLoading} onClick={handleForgot} className="flex-1">
              Enviar
            </Button>
          </div>
        </div>
      )}

      {/* Decorative bottom accent line */}
      <div className="fixed bottom-0 left-0 right-0 h-1 bg-accent" />
    </div>
  )
}
