import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { Scissors } from 'lucide-react'
import { useAuth } from '../hooks/useAuth'
import { useToast } from '../components/ui/Toast'
import { useApiError } from '../hooks/useApiError'
import { Input } from '../components/ui/Input'
import { Button } from '../components/ui/Button'
import { authApi } from '../api/auth'

const schema = z.object({
  email: z.string().email('E-mail inválido'),
  password: z.string().min(1, 'Senha obrigatória'),
})

type FormData = z.infer<typeof schema>

export default function Login() {
  const { login, isAuthenticated, role } = useAuth()
  const navigate = useNavigate()
  const toast = useToast()
  const { getMessage } = useApiError()
  const [loading, setLoading] = useState(false)
  const [forgotOpen, setForgotOpen] = useState(false)
  const [forgotEmail, setForgotEmail] = useState('')
  const [forgotLoading, setForgotLoading] = useState(false)

  const { register, handleSubmit, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  if (isAuthenticated) {
    navigate(role === 'admin' ? '/admin' : '/app', { replace: true })
    return null
  }

  const onSubmit = async ({ email, password }: FormData) => {
    setLoading(true)
    try {
      const data = await login(email, password)
      navigate('Users' in data.modulesAssembled ? '/admin' : '/app', { replace: true })
    } catch (err: unknown) {
      const label = err instanceof Error ? err.message : ''
      toast(getMessage(label, 'Erro ao fazer login.'), 'error')
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
        <div className="border-l-2 border-accent pl-3">
          <h2 className="font-display font-bold text-xl uppercase tracking-wide">Acesse sua conta</h2>
          <p className="text-text-secondary text-xs font-body mt-0.5">Entre com suas credenciais</p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4" noValidate>
          <Input
            id="email"
            label="E-mail"
            type="email"
            placeholder="seuemail@exemplo.com"
            error={errors.email?.message}
            {...register('email')}
          />
          <Input
            id="password"
            label="Senha"
            type="password"
            placeholder="••••••••"
            error={errors.password?.message}
            {...register('password')}
          />
          <Button type="submit" fullWidth size="lg" loading={loading}>
            Entrar
          </Button>
        </form>

        <button
          onClick={() => setForgotOpen(true)}
          className="text-xs text-text-secondary hover:text-accent transition-colors font-body text-center"
        >
          Esqueci minha senha
        </button>
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
