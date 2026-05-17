import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Plus, Edit2, ToggleLeft, ToggleRight } from 'lucide-react'
import { barbersApi } from '../../api/barbers'
import { Button } from '../../components/ui/Button'
import { Input } from '../../components/ui/Input'
import { Modal } from '../../components/ui/Modal'
import { Spinner } from '../../components/ui/Spinner'
import { useToast } from '../../components/ui/Toast'
import type { Barber } from '../../types/barber'

interface BarberForm {
  name: string
  email: string
  password: string
  displayName: string
}

const emptyForm: BarberForm = { name: '', email: '', password: '', displayName: '' }

export default function Barbers() {
  const toast = useToast()
  const qc = useQueryClient()
  const [modalOpen, setModalOpen] = useState(false)
  const [editing, setEditing] = useState<Barber | null>(null)
  const [form, setForm] = useState<BarberForm>(emptyForm)

  const { data: barbers = [], isLoading } = useQuery({
    queryKey: ['barbers'],
    queryFn: barbersApi.getAll,
  })

  const { mutate: save, isPending: saving } = useMutation({
    mutationFn: () =>
      editing
        ? barbersApi.update({
            id: editing.id,
            displayName: form.displayName,
          })
        : barbersApi.create({
            name: form.name,
            email: form.email,
            password: form.password,
            displayName: form.displayName,
          }),
    onSuccess: () => {
      toast(editing ? 'Barbeiro atualizado!' : 'Barbeiro criado!', 'success')
      qc.invalidateQueries({ queryKey: ['barbers'] })
      setModalOpen(false)
    },
    onError: () => toast('Erro ao salvar barbeiro.', 'error'),
  })

  const { mutate: toggleStatus } = useMutation({
    mutationFn: (b: Barber) => barbersApi.changeStatus({ id: b.id, isActive: !b.isActive }),
    onSuccess: () => {
      toast('Status atualizado!', 'success')
      qc.invalidateQueries({ queryKey: ['barbers'] })
    },
    onError: () => toast('Erro ao alterar status.', 'error'),
  })

  function openCreate() {
    setEditing(null)
    setForm(emptyForm)
    setModalOpen(true)
  }

  function openEdit(b: Barber) {
    setEditing(b)
    setForm({
      name: '',
      email: '',
      displayName: b.displayName,
    })
    setModalOpen(true)
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-xs font-display uppercase tracking-[0.2em] text-accent mb-1">BarberAgenda</p>
          <h1 className="font-display font-extrabold text-4xl uppercase leading-none">Barbeiros</h1>
        </div>
        <Button onClick={openCreate}>
          <Plus size={16} className="mr-1" />
          Novo Barbeiro
        </Button>
      </div>

      {isLoading ? (
        <div className="flex justify-center py-10">
          <Spinner />
        </div>
      ) : (
        <div className="bg-bg-surface border border-border rounded-sm overflow-hidden">
          {barbers.length === 0 && (
            <p className="text-text-secondary text-sm font-body p-5">Nenhum barbeiro cadastrado.</p>
          )}
          {barbers.map((b, i) => (
            <div
              key={b.id}
              className={`flex items-center justify-between px-4 py-3 gap-4 ${
                i !== barbers.length - 1 ? 'border-b border-border' : ''
              }`}
            >
              <div className="flex-1">
                <p
                  className={`font-body font-medium ${
                    !b.isActive ? 'text-text-secondary line-through' : 'text-text-primary'
                  }`}
                >
                  {b.displayName}
                </p>
                <p className="text-xs text-text-secondary font-body">
                  {b.isActive ? (
                    <span className="text-green-500">Ativo</span>
                  ) : (
                    <span className="text-text-secondary">Inativo</span>
                  )}
                </p>
              </div>
              <div className="flex items-center gap-2">
                <button
                  aria-label={`Editar ${b.displayName}`}
                  onClick={() => openEdit(b)}
                  className="text-text-secondary hover:text-text-primary"
                >
                  <Edit2 size={16} />
                </button>
                <button
                  aria-label={b.isActive ? 'Desativar' : 'Ativar'}
                  onClick={() => toggleStatus(b)}
                  className={b.isActive ? 'text-accent' : 'text-text-secondary'}
                >
                  {b.isActive ? <ToggleRight size={20} /> : <ToggleLeft size={20} />}
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      <Modal open={modalOpen} onClose={() => { setModalOpen(false); setEditing(null); setForm(emptyForm) }} title={editing ? 'Editar Barbeiro' : 'Novo Barbeiro'}>
        <div className="flex flex-col gap-4">
          {!editing && (
            <>
              <Input
                label="Nome"
                value={form.name}
                onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
                placeholder="Ex: João Silva"
              />
              <Input
                label="E-mail"
                type="email"
                value={form.email}
                onChange={(e) => setForm((f) => ({ ...f, email: e.target.value }))}
                placeholder="Ex: joao@barbearia.com"
              />
              <Input
                label="Senha"
                type="password"
                value={form.password}
                onChange={(e) => setForm((f) => ({ ...f, password: e.target.value }))}
                placeholder="Mínimo 6 caracteres"
              />
            </>
          )}
          <Input
            label="Nome de Exibição"
            value={form.displayName}
            onChange={(e) => setForm((f) => ({ ...f, displayName: e.target.value }))}
            placeholder="Ex: João"
          />
          <Button fullWidth loading={saving} onClick={() => save()}>
            Salvar
          </Button>
        </div>
      </Modal>
    </div>
  )
}
