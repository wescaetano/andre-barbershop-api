import { LogOut, Mail, User as UserIcon } from 'lucide-react'
import { useAuthStore } from '../../store/authStore'
import { useAuth } from '../../hooks/useAuth'
import { Button } from '../../components/ui/Button'

export default function Profile() {
  const { userName, userEmail } = useAuthStore()
  const { logout } = useAuth()

  return (
    <div className="min-h-screen bg-bg-base">
      <div className="bg-bg-surface border-b border-border px-5 pt-10 pb-5">
        <h1 className="font-display font-extrabold text-3xl uppercase">Perfil</h1>
      </div>

      <div className="px-5 pt-6 flex flex-col gap-4">
        <div className="bg-bg-surface border border-border border-l-[3px] border-l-accent rounded-sm p-5 flex flex-col gap-3">
          <div className="flex items-center gap-3">
            <div className="w-12 h-12 bg-accent/10 border border-accent/20 rounded-sm flex items-center justify-center">
              <UserIcon size={22} className="text-accent" />
            </div>
            <div>
              <p className="font-display font-bold text-xl uppercase">{userName}</p>
              <div className="flex items-center gap-1.5">
                <Mail size={11} className="text-text-secondary" />
                <p className="text-text-secondary text-xs font-body">{userEmail}</p>
              </div>
            </div>
          </div>
        </div>

        <Button variant="danger" fullWidth onClick={logout}>
          <LogOut size={16} />
          Sair da conta
        </Button>
      </div>
    </div>
  )
}
