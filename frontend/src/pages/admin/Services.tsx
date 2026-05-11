import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Plus, Edit2, ToggleLeft, ToggleRight } from 'lucide-react'
import { servicesApi } from '../../api/services'
import { Button } from '../../components/ui/Button'
import { Input } from '../../components/ui/Input'
import { Modal } from '../../components/ui/Modal'
import { Spinner } from '../../components/ui/Spinner'
import { useToast } from '../../components/ui/Toast'
import type { Service } from '../../types/service'

interface ServiceForm {
  name: string
  durationMinutes: string
  price: string
}

const emptyForm: ServiceForm = { name: '', durationMinutes: '30', price: '' }

export default function Services() {
  const toast = useToast()
  const qc = useQueryClient()
  const [modalOpen, setModalOpen] = useState(false)
  const [editing, setEditing] = useState<Service | null>(null)
  const [form, setForm] = useState<ServiceForm>(emptyForm)

  const { data: services = [], isLoading } = useQuery({
    queryKey: ['services-all'],
    queryFn: servicesApi.getAll,
  })

  const { mutate: save, isPending: saving } = useMutation({
    mutationFn: () =>
      editing
        ? servicesApi.update({
            id: editing.id,
            name: form.name,
            durationMinutes: Number(form.durationMinutes),
            price: Number(form.price),
          })
        : servicesApi.create({
            name: form.name,
            durationMinutes: Number(form.durationMinutes),
            price: Number(form.price),
          }),
    onSuccess: () => {
      toast(editing ? 'Serviço atualizado!' : 'Serviço criado!', 'success')
      qc.invalidateQueries({ queryKey: ['services-all'] })
      qc.invalidateQueries({ queryKey: ['services'] })
      setModalOpen(false)
    },
    onError: () => toast('Erro ao salvar serviço.', 'error'),
  })

  const { mutate: toggleStatus } = useMutation({
    mutationFn: (s: Service) => servicesApi.changeStatus(s.id, !s.isActive),
    onSuccess: () => {
      toast('Status atualizado!', 'success')
      qc.invalidateQueries({ queryKey: ['services-all'] })
    },
    onError: () => toast('Erro ao alterar status.', 'error'),
  })

  function openCreate() {
    setEditing(null)
    setForm(emptyForm)
    setModalOpen(true)
  }

  function openEdit(s: Service) {
    setEditing(s)
    setForm({
      name: s.name,
      durationMinutes: String(s.durationMinutes),
      price: String(s.price),
    })
    setModalOpen(true)
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-xs font-display uppercase tracking-[0.2em] text-accent mb-1">BarberAgenda</p>
          <h1 className="font-display font-extrabold text-4xl uppercase leading-none">Serviços</h1>
        </div>
        <Button onClick={openCreate}>
          <Plus size={16} className="mr-1" />
          Novo Serviço
        </Button>
      </div>

      {isLoading ? (
        <div className="flex justify-center py-10">
          <Spinner />
        </div>
      ) : (
        <div className="bg-bg-surface border border-border rounded-sm overflow-hidden">
          {services.length === 0 && (
            <p className="text-text-secondary text-sm font-body p-5">Nenhum serviço cadastrado.</p>
          )}
          {services.map((s, i) => (
            <div
              key={s.id}
              className={`flex items-center justify-between px-4 py-3 gap-4 ${
                i !== services.length - 1 ? 'border-b border-border' : ''
              }`}
            >
              <div className="flex-1">
                <p
                  className={`font-body font-medium ${
                    !s.isActive ? 'text-text-secondary line-through' : 'text-text-primary'
                  }`}
                >
                  {s.name}
                </p>
                <p className="text-xs text-text-secondary font-body">
                  {s.durationMinutes} min · R$ {Number(s.price).toFixed(2)}
                </p>
              </div>
              <div className="flex items-center gap-2">
                <button
                  aria-label={`Editar ${s.name}`}
                  onClick={() => openEdit(s)}
                  className="text-text-secondary hover:text-text-primary"
                >
                  <Edit2 size={16} />
                </button>
                <button
                  aria-label={s.isActive ? 'Desativar' : 'Ativar'}
                  onClick={() => toggleStatus(s)}
                  className={s.isActive ? 'text-accent' : 'text-text-secondary'}
                >
                  {s.isActive ? <ToggleRight size={20} /> : <ToggleLeft size={20} />}
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      <Modal open={modalOpen} onClose={() => { setModalOpen(false); setEditing(null); setForm(emptyForm) }} title={editing ? 'Editar Serviço' : 'Novo Serviço'}>
        <div className="flex flex-col gap-4">
          <Input
            label="Nome"
            value={form.name}
            onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
            placeholder="Ex: Corte + Barba"
          />
          <Input
            label="Duração (minutos)"
            type="number"
            value={form.durationMinutes}
            onChange={(e) => setForm((f) => ({ ...f, durationMinutes: e.target.value }))}
          />
          <Input
            label="Preço (R$)"
            type="number"
            step="0.01"
            value={form.price}
            onChange={(e) => setForm((f) => ({ ...f, price: e.target.value }))}
          />
          <Button fullWidth loading={saving} onClick={() => save()}>
            Salvar
          </Button>
        </div>
      </Modal>
    </div>
  )
}
