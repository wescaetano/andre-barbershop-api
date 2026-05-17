import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { Search, Plus, Eye, Power, Trash2, ChevronLeft, ChevronRight } from 'lucide-react'
import { usersApi } from '../../api/users'
import { Button } from '../../components/ui/Button'
import { Input } from '../../components/ui/Input'
import { Badge } from '../../components/ui/Badge'
import { Modal } from '../../components/ui/Modal'
import { Spinner } from '../../components/ui/Spinner'
import { useToast } from '../../components/ui/Toast'
import { useApiError } from '../../hooks/useApiError'
import type { User } from '../../types/user'

const createSchema = z.object({
  name: z.string().min(1, 'Nome obrigatório'),
  email: z.string().email('E-mail inválido'),
  password: z.string().min(6, 'Senha deve ter no mínimo 6 caracteres'),
  accessProfile: z.coerce.number().min(1, 'Perfil obrigatório'),
})
type CreateForm = z.infer<typeof createSchema>

export default function AdminUsers() {
  const navigate = useNavigate()
  const toast = useToast()
  const { getMessage } = useApiError()
  const qc = useQueryClient()
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [draftSearch, setDraftSearch] = useState('')
  const [createOpen, setCreateOpen] = useState(false)
  const [deleteTarget, setDeleteTarget] = useState<User | null>(null)

  const { data, isLoading } = useQuery({
    queryKey: ['users', page, search],
    queryFn: () => usersApi.list({ pageNumber: page, pageSize: 20, name: search || undefined }),
  })

  const { register, handleSubmit, reset, formState: { errors } } = useForm<CreateForm>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(createSchema) as any,
  })

  const { mutate: createUser, isPending: creating } = useMutation({
    mutationFn: (d: CreateForm) => usersApi.create({ name: d.name, email: d.email, password: d.password, accessProfile: d.accessProfile }),
    onSuccess: () => {
      toast('Usuário criado com sucesso!', 'success')
      setCreateOpen(false)
      reset()
      qc.invalidateQueries({ queryKey: ['users'] })
    },
    onError: (err: Error) => toast(getMessage(err.message, 'Erro ao criar usuário.'), 'error'),
  })

  const { mutate: toggleStatus, isPending: togglingStatus } = useMutation({
    mutationFn: (user: User) => usersApi.changeStatus(user.id, user.status === 1 ? 0 : 1),
    onSuccess: () => {
      toast('Status atualizado.', 'success')
      qc.invalidateQueries({ queryKey: ['users'] })
    },
    onError: (err: Error) => toast(getMessage(err.message, 'Erro ao alterar status.'), 'error'),
  })

  const { mutate: deleteUser, isPending: deleting } = useMutation({
    mutationFn: () => usersApi.delete(deleteTarget!.id),
    onSuccess: () => {
      toast('Usuário removido.', 'info')
      setDeleteTarget(null)
      qc.invalidateQueries({ queryKey: ['users'] })
    },
    onError: (err: Error) => toast(getMessage(err.message, 'Erro ao remover.'), 'error'),
  })

  const users = data?.items ?? []
  const totalPages = data?.totalPages ?? 1

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="font-display font-extrabold text-4xl uppercase">Usuários</h1>
          <p className="text-text-secondary text-sm font-body mt-1">{data?.totalCount ?? 0} registros</p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <Plus size={16} />
          Novo usuário
        </Button>
      </div>

      <form className="flex gap-2" onSubmit={(e) => { e.preventDefault(); setSearch(draftSearch); setPage(1) }}>
        <Input placeholder="Buscar por nome..." value={draftSearch} onChange={(e) => setDraftSearch(e.target.value)} className="flex-1" />
        <Button type="submit" variant="ghost"><Search size={16} /></Button>
      </form>

      {isLoading ? (
        <div className="flex justify-center py-10"><Spinner /></div>
      ) : (
        <div className="bg-bg-surface border border-border rounded-sm overflow-hidden">
          {users.length === 0 && (
            <p className="text-text-secondary text-sm font-body p-6">Nenhum usuário encontrado.</p>
          )}
          {users.map((user, i) => (
            <div key={user.id} className={`flex items-center justify-between px-4 py-3 gap-3 ${i !== users.length - 1 ? 'border-b border-border' : ''}`}>
              <div className="flex-1 min-w-0">
                <p className="text-sm font-body font-medium truncate">{user.name}</p>
                <p className="text-xs text-text-secondary font-body truncate">{user.email}</p>
              </div>
              <Badge className={user.status === 1 ? 'bg-green-500/10 text-green-400 border border-green-500/30' : 'bg-red-500/10 text-red-400 border border-red-500/30'}>
                {user.status === 1 ? 'Ativo' : 'Inativo'}
              </Badge>
              <div className="flex gap-1">
                <button onClick={() => navigate(`/admin/users/${user.id}`)} className="p-1.5 text-text-secondary hover:text-text-primary transition-colors" title="Ver detalhes">
                  <Eye size={15} />
                </button>
                <button onClick={() => toggleStatus(user)} disabled={togglingStatus} className="p-1.5 text-text-secondary hover:text-accent transition-colors" title="Alternar status">
                  <Power size={15} />
                </button>
                <button onClick={() => setDeleteTarget(user)} className="p-1.5 text-text-secondary hover:text-red-400 transition-colors" title="Excluir">
                  <Trash2 size={15} />
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {totalPages > 1 && (
        <div className="flex items-center justify-center gap-3">
          <button disabled={page === 1} onClick={() => setPage(p => p - 1)} className="text-text-secondary hover:text-text-primary disabled:opacity-30">
            <ChevronLeft size={18} />
          </button>
          <span className="text-sm font-body text-text-secondary">Página {page} de {totalPages}</span>
          <button disabled={page === totalPages} onClick={() => setPage(p => p + 1)} className="text-text-secondary hover:text-text-primary disabled:opacity-30">
            <ChevronRight size={18} />
          </button>
        </div>
      )}

      <Modal open={createOpen} onClose={() => { setCreateOpen(false); reset() }} title="Novo usuário">
        <form onSubmit={handleSubmit((d) => createUser(d as CreateForm))} className="flex flex-col gap-4">
          <Input id="name" label="Nome" placeholder="Nome completo" error={errors.name?.message} {...register('name')} />
          <Input id="email" label="E-mail" type="email" placeholder="email@exemplo.com" error={errors.email?.message} {...register('email')} />
          <Input id="password" label="Senha" type="password" placeholder="Mínimo 6 caracteres" error={errors.password?.message} {...register('password')} />
          <Input id="profile" label="ID do perfil de acesso" type="number" placeholder="Ex: 2" error={errors.accessProfile?.message} {...register('accessProfile')} />
          <Button type="submit" fullWidth loading={creating}>Criar usuário</Button>
        </form>
      </Modal>

      <Modal open={!!deleteTarget} onClose={() => setDeleteTarget(null)} title="Excluir usuário">
        <div className="flex flex-col gap-4">
          <p className="text-sm font-body text-text-secondary">
            Tem certeza que deseja excluir <strong className="text-text-primary">{deleteTarget?.name}</strong>? Esta ação não pode ser desfeita.
          </p>
          <div className="flex gap-2">
            <Button variant="ghost" size="sm" onClick={() => setDeleteTarget(null)} className="flex-1">Cancelar</Button>
            <Button variant="danger" size="sm" loading={deleting} onClick={() => deleteUser()} className="flex-1">Excluir</Button>
          </div>
        </div>
      </Modal>
    </div>
  )
}
