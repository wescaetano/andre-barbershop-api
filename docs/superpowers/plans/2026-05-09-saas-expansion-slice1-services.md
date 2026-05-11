# BarberAgenda — Slice 1: Services Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a `Service` entity (name, duration, price) to the backend; expose CRUD endpoints for admin; add a Services management page to the admin panel; add service selection as step 1 of the client booking flow.

**Architecture:** New `Service` domain entity → EF migration → use cases + `ServiceController` → frontend `servicesApi` → admin `Services.tsx` page + updated `Book.tsx` wizard (4 steps instead of 3).

**Tech Stack:** .NET 8, EF Core / MySQL, React + Vite + TypeScript, TanStack Query, Tailwind CSS

**Spec:** `docs/superpowers/specs/2026-05-09-saas-backoffice-client-redesign.md` — Slice 1

---

## File Map

**Create (backend):**
- `src/BarberShop.Domain/Service.cs`
- `src/BarberShop.Infra/Mappings/ServiceMap.cs`
- `src/BarberShop.Communication/Models/Service/CreateServiceModel.cs`
- `src/BarberShop.Communication/Models/Service/UpdateServiceModel.cs`
- `src/BarberShop.Communication/Models/Service/ChangeServiceStatusModel.cs`
- `src/BarberShop.Application/UseCases/Service/GetAll/IGetServicesUseCase.cs`
- `src/BarberShop.Application/UseCases/Service/GetAll/GetServicesUseCase.cs`
- `src/BarberShop.Application/UseCases/Service/Create/ICreateServiceUseCase.cs`
- `src/BarberShop.Application/UseCases/Service/Create/CreateServiceUseCase.cs`
- `src/BarberShop.Application/UseCases/Service/Update/IUpdateServiceUseCase.cs`
- `src/BarberShop.Application/UseCases/Service/Update/UpdateServiceUseCase.cs`
- `src/BarberShop.Application/UseCases/Service/ChangeStatus/IChangeServiceStatusUseCase.cs`
- `src/BarberShop.Application/UseCases/Service/ChangeStatus/ChangeServiceStatusUseCase.cs`
- `src/BarberShop.Api/Controllers/ServiceController.cs`

**Modify (backend):**
- `src/BarberShop.Infra/DataAccess/BarberShopContext.cs` — add `DbSet<Service>`
- `src/BarberShop.IOC/Injection.cs` — register service use cases

**Create (frontend):**
- `frontend/src/types/service.ts`
- `frontend/src/api/services.ts`
- `frontend/src/pages/admin/Services.tsx`

**Modify (frontend):**
- `frontend/src/pages/client/Book.tsx` — add service selection step
- `frontend/src/api/appointments.ts` — pass `serviceId` to available-slots
- `frontend/src/router/index.tsx` — add `/admin/services` route
- `frontend/src/components/layout/Sidebar.tsx` — add Services nav link

---

### Task 1: Service domain entity + EF mapping

**Files:**
- Create: `src/BarberShop.Domain/Service.cs`
- Create: `src/BarberShop.Infra/Mappings/ServiceMap.cs`
- Modify: `src/BarberShop.Infra/DataAccess/BarberShopContext.cs`

- [ ] **Step 1: Create Service entity**

```csharp
// src/BarberShop.Domain/Service.cs
namespace BarberShop.Domain
{
    public class Service : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
```

- [ ] **Step 2: Create ServiceMap**

```csharp
// src/BarberShop.Infra/Mappings/ServiceMap.cs
using BarberShop.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberShop.Infra.Mappings
{
    public class ServiceMap : IEntityTypeConfiguration<Service>
    {
        public void Configure(EntityTypeBuilder<Service> builder)
        {
            builder.ToTable("Services");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
            builder.Property(s => s.Price).HasColumnType("decimal(10,2)");
        }
    }
}
```

- [ ] **Step 3: Add DbSet to context**

In `src/BarberShop.Infra/DataAccess/BarberShopContext.cs`, add after `DbSet<Payment>`:

```csharp
public DbSet<Service> Services { get; set; } = null!;
```

- [ ] **Step 4: Create and apply migration**

```bash
dotnet ef migrations add AddServiceEntity --project src/BarberShop.Infra --startup-project src/BarberShop.Api
dotnet ef database update --project src/BarberShop.Infra --startup-project src/BarberShop.Api
```

Expected: migration file created, `Services` table created in DB.

- [ ] **Step 5: Commit**

```bash
git add src/BarberShop.Domain/Service.cs src/BarberShop.Infra/Mappings/ServiceMap.cs src/BarberShop.Infra/DataAccess/BarberShopContext.cs src/BarberShop.Infra/Migrations/
git commit -m "feat(backend): add Service entity, mapping, and migration"
```

---

### Task 2: Service communication models

**Files:**
- Create: `src/BarberShop.Communication/Models/Service/CreateServiceModel.cs`
- Create: `src/BarberShop.Communication/Models/Service/UpdateServiceModel.cs`
- Create: `src/BarberShop.Communication/Models/Service/ChangeServiceStatusModel.cs`

- [ ] **Step 1: Create models**

```csharp
// src/BarberShop.Communication/Models/Service/CreateServiceModel.cs
namespace BarberShop.Communication.Models.Service
{
    public class CreateServiceModel
    {
        public string Name { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public decimal Price { get; set; }
    }
}
```

```csharp
// src/BarberShop.Communication/Models/Service/UpdateServiceModel.cs
namespace BarberShop.Communication.Models.Service
{
    public class UpdateServiceModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public decimal Price { get; set; }
    }
}
```

```csharp
// src/BarberShop.Communication/Models/Service/ChangeServiceStatusModel.cs
namespace BarberShop.Communication.Models.Service
{
    public class ChangeServiceStatusModel
    {
        public long Id { get; set; }
        public bool IsActive { get; set; }
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add src/BarberShop.Communication/Models/Service/
git commit -m "feat(backend): add Service communication models"
```

---

### Task 3: Service use cases

**Files:**
- Create: `src/BarberShop.Application/UseCases/Service/GetAll/IGetServicesUseCase.cs`
- Create: `src/BarberShop.Application/UseCases/Service/GetAll/GetServicesUseCase.cs`
- Create: `src/BarberShop.Application/UseCases/Service/Create/ICreateServiceUseCase.cs`
- Create: `src/BarberShop.Application/UseCases/Service/Create/CreateServiceUseCase.cs`
- Create: `src/BarberShop.Application/UseCases/Service/Update/IUpdateServiceUseCase.cs`
- Create: `src/BarberShop.Application/UseCases/Service/Update/UpdateServiceUseCase.cs`
- Create: `src/BarberShop.Application/UseCases/Service/ChangeStatus/IChangeServiceStatusUseCase.cs`
- Create: `src/BarberShop.Application/UseCases/Service/ChangeStatus/ChangeServiceStatusUseCase.cs`

- [ ] **Step 1: GetAll**

```csharp
// IGetServicesUseCase.cs
using BarberShop.Communication.Models;
namespace BarberShop.Application.UseCases.Service.GetAll
{
    public interface IGetServicesUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(bool activeOnly);
    }
}
```

```csharp
// GetServicesUseCase.cs
using BarberShop.Communication.Models;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Service.GetAll
{
    public class GetServicesUseCase : IGetServicesUseCase
    {
        private readonly IBaseRepository<Domain.Service> _repo;
        public GetServicesUseCase(IBaseRepository<Domain.Service> repo) => _repo = repo;

        public async Task<ResponseModel<dynamic>> ExecuteAsync(bool activeOnly)
        {
            var services = activeOnly
                ? await _repo.GetAll(s => s.IsActive)
                : await _repo.Get();

            var result = services.Select(s => new
            {
                s.Id, s.Name, s.DurationMinutes, s.Price, s.IsActive
            }).ToList();

            return FactoryResponse<dynamic>.Success(result);
        }
    }
}
```

- [ ] **Step 2: Create**

```csharp
// ICreateServiceUseCase.cs
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Service;
namespace BarberShop.Application.UseCases.Service.Create
{
    public interface ICreateServiceUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(CreateServiceModel model);
    }
}
```

```csharp
// CreateServiceUseCase.cs
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Service;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Service.Create
{
    public class CreateServiceUseCase : ICreateServiceUseCase
    {
        private readonly IBaseRepository<Domain.Service> _repo;
        public CreateServiceUseCase(IBaseRepository<Domain.Service> repo) => _repo = repo;

        public async Task<ResponseModel<dynamic>> ExecuteAsync(CreateServiceModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return FactoryResponse<dynamic>.InvalidModel("Nome é obrigatório.");
            if (model.DurationMinutes <= 0)
                return FactoryResponse<dynamic>.InvalidModel("Duração deve ser maior que zero.");
            if (model.Price < 0)
                return FactoryResponse<dynamic>.InvalidModel("Preço não pode ser negativo.");

            var service = new Domain.Service
            {
                Name = model.Name.Trim(),
                DurationMinutes = model.DurationMinutes,
                Price = model.Price,
                IsActive = true
            };
            service.AddCreationDate();
            await _repo.Create(service);
            return FactoryResponse<dynamic>.SuccessfulCreation(new { service.Id, service.Name, service.DurationMinutes, service.Price });
        }
    }
}
```

- [ ] **Step 3: Update**

```csharp
// IUpdateServiceUseCase.cs
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Service;
namespace BarberShop.Application.UseCases.Service.Update
{
    public interface IUpdateServiceUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(UpdateServiceModel model);
    }
}
```

```csharp
// UpdateServiceUseCase.cs
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Service;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Service.Update
{
    public class UpdateServiceUseCase : IUpdateServiceUseCase
    {
        private readonly IBaseRepository<Domain.Service> _repo;
        public UpdateServiceUseCase(IBaseRepository<Domain.Service> repo) => _repo = repo;

        public async Task<ResponseModel<dynamic>> ExecuteAsync(UpdateServiceModel model)
        {
            var service = await _repo.Get(model.Id);
            if (service == null)
                return FactoryResponse<dynamic>.NotFound("Serviço não encontrado.");
            if (string.IsNullOrWhiteSpace(model.Name))
                return FactoryResponse<dynamic>.InvalidModel("Nome é obrigatório.");

            service.Name = model.Name.Trim();
            service.DurationMinutes = model.DurationMinutes;
            service.Price = model.Price;
            service.AddUpdateDate();
            await _repo.Update(service);
            return FactoryResponse<dynamic>.Success(new { service.Id, service.Name, service.DurationMinutes, service.Price });
        }
    }
}
```

- [ ] **Step 4: ChangeStatus**

```csharp
// IChangeServiceStatusUseCase.cs
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Service;
namespace BarberShop.Application.UseCases.Service.ChangeStatus
{
    public interface IChangeServiceStatusUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(ChangeServiceStatusModel model);
    }
}
```

```csharp
// ChangeServiceStatusUseCase.cs
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Service;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Service.ChangeStatus
{
    public class ChangeServiceStatusUseCase : IChangeServiceStatusUseCase
    {
        private readonly IBaseRepository<Domain.Service> _repo;
        public ChangeServiceStatusUseCase(IBaseRepository<Domain.Service> repo) => _repo = repo;

        public async Task<ResponseModel<dynamic>> ExecuteAsync(ChangeServiceStatusModel model)
        {
            var service = await _repo.Get(model.Id);
            if (service == null)
                return FactoryResponse<dynamic>.NotFound("Serviço não encontrado.");

            service.IsActive = model.IsActive;
            service.AddUpdateDate();
            await _repo.Update(service);
            return FactoryResponse<dynamic>.Success(null);
        }
    }
}
```

- [ ] **Step 5: Commit**

```bash
git add src/BarberShop.Application/UseCases/Service/
git commit -m "feat(backend): add Service use cases (GetAll, Create, Update, ChangeStatus)"
```

---

### Task 4: ServiceController + IOC registration

**Files:**
- Create: `src/BarberShop.Api/Controllers/ServiceController.cs`
- Modify: `src/BarberShop.IOC/Injection.cs`

- [ ] **Step 1: Create controller**

```csharp
// src/BarberShop.Api/Controllers/ServiceController.cs
using BarberShop.Api.Authorization;
using BarberShop.Application.UseCases.Service.ChangeStatus;
using BarberShop.Application.UseCases.Service.Create;
using BarberShop.Application.UseCases.Service.GetAll;
using BarberShop.Application.UseCases.Service.Update;
using BarberShop.Communication.Models.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Controllers
{
    /// <summary>Gerenciamento de serviços da barbearia</summary>
    [APIAuthorization("Users-C", "Users-E", "Users-V", "Users-I")]
    public class ServiceController : BaseController
    {
        private readonly IGetServicesUseCase _getServices;
        private readonly ICreateServiceUseCase _create;
        private readonly IUpdateServiceUseCase _update;
        private readonly IChangeServiceStatusUseCase _changeStatus;

        public ServiceController(
            IGetServicesUseCase getServices,
            ICreateServiceUseCase create,
            IUpdateServiceUseCase update,
            IChangeServiceStatusUseCase changeStatus)
        {
            _getServices = getServices;
            _create = create;
            _update = update;
            _changeStatus = changeStatus;
        }

        /// <summary>Lista serviços ativos (público)</summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetActive()
            => Result(await _getServices.ExecuteAsync(activeOnly: true));

        /// <summary>Lista todos os serviços incluindo inativos (admin)</summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
            => Result(await _getServices.ExecuteAsync(activeOnly: false));

        /// <summary>Cria um novo serviço</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateServiceModel model)
            => Result(await _create.ExecuteAsync(model));

        /// <summary>Atualiza um serviço</summary>
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateServiceModel model)
            => Result(await _update.ExecuteAsync(model));

        /// <summary>Ativa ou inativa um serviço</summary>
        [HttpPatch("status")]
        public async Task<IActionResult> ChangeStatus([FromBody] ChangeServiceStatusModel model)
            => Result(await _changeStatus.ExecuteAsync(model));
    }
}
```

- [ ] **Step 2: Register in IOC**

In `src/BarberShop.IOC/Injection.cs`, add after the Payment use cases block. First add the using statements at the top:

```csharp
using BarberShop.Application.UseCases.Service.ChangeStatus;
using BarberShop.Application.UseCases.Service.Create;
using BarberShop.Application.UseCases.Service.GetAll;
using BarberShop.Application.UseCases.Service.Update;
```

Then add at the end of `InjectDependencies`, before `return services`:

```csharp
// Service use cases
services.AddScoped<IGetServicesUseCase, GetServicesUseCase>();
services.AddScoped<ICreateServiceUseCase, CreateServiceUseCase>();
services.AddScoped<IUpdateServiceUseCase, UpdateServiceUseCase>();
services.AddScoped<IChangeServiceStatusUseCase, ChangeServiceStatusUseCase>();
```

- [ ] **Step 3: Build and verify no compile errors**

```bash
dotnet build src/BarberShop.Api/BarberShop.Api.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Commit**

```bash
git add src/BarberShop.Api/Controllers/ServiceController.cs src/BarberShop.IOC/Injection.cs
git commit -m "feat(backend): add ServiceController and register use cases in IOC"
```

---

### Task 5: Frontend — Service types and API client

**Files:**
- Create: `frontend/src/types/service.ts`
- Create: `frontend/src/api/services.ts`

- [ ] **Step 1: Create types**

```ts
// frontend/src/types/service.ts
export interface Service {
  id: number
  name: string
  durationMinutes: number
  price: number
  isActive: boolean
}
```

- [ ] **Step 2: Create API module**

```ts
// frontend/src/api/services.ts
import { apiClient } from './client'
import type { ApiResponse } from '../types/auth'
import type { Service } from '../types/service'

export const servicesApi = {
  getActive: async (): Promise<Service[]> => {
    const { data } = await apiClient.get<ApiResponse<Service[]>>('/service')
    return data.data ?? []
  },

  getAll: async (): Promise<Service[]> => {
    const { data } = await apiClient.get<ApiResponse<Service[]>>('/service/all')
    return data.data ?? []
  },

  create: async (payload: { name: string; durationMinutes: number; price: number }) => {
    const { data } = await apiClient.post<ApiResponse<Service>>('/service', payload)
    if (!data.success) throw new Error(data.responseLabel)
    return data
  },

  update: async (payload: { id: number; name: string; durationMinutes: number; price: number }) => {
    const { data } = await apiClient.put<ApiResponse<Service>>('/service', payload)
    if (!data.success) throw new Error(data.responseLabel)
    return data
  },

  changeStatus: async (id: number, isActive: boolean) => {
    const { data } = await apiClient.patch<ApiResponse<null>>('/service/status', { id, isActive })
    if (!data.success) throw new Error(data.responseLabel)
    return data
  },
}
```

- [ ] **Step 3: Commit**

```bash
git add frontend/src/types/service.ts frontend/src/api/services.ts
git commit -m "feat(frontend): add Service type and servicesApi"
```

---

### Task 6: Admin Services page

**Files:**
- Create: `frontend/src/pages/admin/Services.tsx`
- Modify: `frontend/src/router/index.tsx`
- Modify: `frontend/src/components/layout/Sidebar.tsx`

- [ ] **Step 1: Create Services page**

```tsx
// frontend/src/pages/admin/Services.tsx
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

interface ServiceForm { name: string; durationMinutes: string; price: string }
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
        ? servicesApi.update({ id: editing.id, name: form.name, durationMinutes: Number(form.durationMinutes), price: Number(form.price) })
        : servicesApi.create({ name: form.name, durationMinutes: Number(form.durationMinutes), price: Number(form.price) }),
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
    setForm({ name: s.name, durationMinutes: String(s.durationMinutes), price: String(s.price) })
    setModalOpen(true)
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-xs font-display uppercase tracking-[0.2em] text-accent mb-1">BarberAgenda</p>
          <h1 className="font-display font-extrabold text-4xl uppercase leading-none">Serviços</h1>
        </div>
        <Button onClick={openCreate}><Plus size={16} className="mr-1" />Novo Serviço</Button>
      </div>

      {isLoading ? (
        <div className="flex justify-center py-10"><Spinner /></div>
      ) : (
        <div className="bg-bg-surface border border-border rounded-sm overflow-hidden">
          {services.length === 0 && (
            <p className="text-text-secondary text-sm font-body p-5">Nenhum serviço cadastrado.</p>
          )}
          {services.map((s, i) => (
            <div
              key={s.id}
              className={`flex items-center justify-between px-4 py-3 gap-4 ${i !== services.length - 1 ? 'border-b border-border' : ''}`}
            >
              <div className="flex-1">
                <p className={`font-body font-medium ${!s.isActive ? 'text-text-secondary line-through' : 'text-text-primary'}`}>
                  {s.name}
                </p>
                <p className="text-xs text-text-secondary font-body">
                  {s.durationMinutes} min · R$ {Number(s.price).toFixed(2)}
                </p>
              </div>
              <div className="flex items-center gap-2">
                <button onClick={() => openEdit(s)} className="text-text-secondary hover:text-text-primary">
                  <Edit2 size={16} />
                </button>
                <button onClick={() => toggleStatus(s)} className={s.isActive ? 'text-accent' : 'text-text-secondary'}>
                  {s.isActive ? <ToggleRight size={20} /> : <ToggleLeft size={20} />}
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      <Modal open={modalOpen} onClose={() => setModalOpen(false)} title={editing ? 'Editar Serviço' : 'Novo Serviço'}>
        <div className="flex flex-col gap-4">
          <Input
            label="Nome"
            value={form.name}
            onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
            placeholder="Ex: Corte + Barba"
          />
          <Input
            label="Duração (minutos)"
            type="number"
            value={form.durationMinutes}
            onChange={e => setForm(f => ({ ...f, durationMinutes: e.target.value }))}
          />
          <Input
            label="Preço (R$)"
            type="number"
            step="0.01"
            value={form.price}
            onChange={e => setForm(f => ({ ...f, price: e.target.value }))}
          />
          <Button fullWidth loading={saving} onClick={() => save()}>
            Salvar
          </Button>
        </div>
      </Modal>
    </div>
  )
}
```

- [ ] **Step 2: Add route in router**

In `frontend/src/router/index.tsx`, add import at the top:
```ts
import AdminServices from '../pages/admin/Services'
```

Add inside the admin children (after `users/:id`):
```ts
{ path: 'services', element: <AdminServices /> },
```

- [ ] **Step 3: Add nav link in Sidebar**

In `frontend/src/components/layout/Sidebar.tsx`, add `Scissors` to the lucide import (already imported), then add to `links` array:
```ts
{ to: '/admin/services', label: 'Serviços', Icon: Scissors, end: false },
```

- [ ] **Step 4: Start dev server and manually verify**

```bash
cd frontend && npm run dev
```

Navigate to `/admin/services`. Verify: page loads, "Novo Serviço" button opens modal, form submits and creates a service (requires backend running). Toggle status works.

- [ ] **Step 5: Commit**

```bash
git add frontend/src/pages/admin/Services.tsx frontend/src/router/index.tsx frontend/src/components/layout/Sidebar.tsx
git commit -m "feat(frontend): add Admin Services CRUD page"
```

---

### Task 7: Book wizard — add Service selection step (Step 0)

**Files:**
- Modify: `frontend/src/pages/client/Book.tsx`
- Modify: `frontend/src/api/appointments.ts`
- Modify: `frontend/src/types/appointment.ts`

- [ ] **Step 1: Update appointment types**

In `frontend/src/types/appointment.ts`, update `CreateAppointmentRequest`:

```ts
export interface CreateAppointmentRequest {
  userId: number
  date: string       // "YYYY-MM-DD"
  startTime: string  // "HH:mm"
  serviceId: number
}
```

- [ ] **Step 2: Update appointments API**

In `frontend/src/api/appointments.ts`, update `getAvailableSlots` to accept `serviceId`:

```ts
getAvailableSlots: async (date: string, serviceId: number): Promise<string[]> => {
  const { data } = await apiClient.get<ApiResponse<string[]>>('/appointment/available-slots', {
    params: { date, serviceId },
  })
  return data.data ?? []
},
```

- [ ] **Step 3: Rewrite Book.tsx with 4-step wizard**

Replace the entire content of `frontend/src/pages/client/Book.tsx`:

```tsx
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { ChevronLeft, Calendar, Clock, Check, Scissors } from 'lucide-react'
import { useAuthStore } from '../../store/authStore'
import { appointmentsApi } from '../../api/appointments'
import { servicesApi } from '../../api/services'
import { Button } from '../../components/ui/Button'
import { Modal } from '../../components/ui/Modal'
import { Spinner } from '../../components/ui/Spinner'
import { useToast } from '../../components/ui/Toast'
import { useApiError } from '../../hooks/useApiError'
import type { Service } from '../../types/service'

function toISODate(d: Date) {
  return d.toISOString().split('T')[0]
}

function DatePicker({ value, onChange }: { value: Date | null; onChange: (d: Date) => void }) {
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  const weeks: Date[][] = []
  const start = new Date(today)
  start.setDate(today.getDate() - today.getDay())
  for (let w = 0; w < 5; w++) {
    const week: Date[] = []
    for (let d = 0; d < 7; d++) {
      const day = new Date(start)
      day.setDate(start.getDate() + w * 7 + d)
      week.push(day)
    }
    weeks.push(week)
  }
  const dayNames = ['D', 'S', 'T', 'Q', 'Q', 'S', 'S']
  return (
    <div className="flex flex-col gap-2">
      <div className="grid grid-cols-7 gap-0.5">
        {dayNames.map((d, i) => (
          <div key={i} className="text-center text-xs font-body text-text-secondary py-1">{d}</div>
        ))}
        {weeks.flat().map((day, i) => {
          const past = day < today
          const selected = value && toISODate(day) === toISODate(value)
          const isToday = toISODate(day) === toISODate(today)
          return (
            <button
              key={i}
              disabled={past}
              onClick={() => onChange(day)}
              className={`aspect-square flex items-center justify-center text-sm font-body rounded-sm transition-colors
                ${past ? 'text-text-secondary/30 cursor-not-allowed' : ''}
                ${selected ? 'bg-accent text-white font-bold' : ''}
                ${!selected && !past ? 'hover:bg-bg-elevated' : ''}
                ${isToday && !selected ? 'border border-accent text-accent' : ''}
              `}
            >
              {day.getDate()}
            </button>
          )
        })}
      </div>
    </div>
  )
}

function SlotGrid({ slots, selected, onSelect }: { slots: string[]; selected: string | null; onSelect: (s: string) => void }) {
  const allSlots: string[] = []
  for (let h = 9; h < 18; h++) {
    allSlots.push(`${String(h).padStart(2, '0')}:00`)
    allSlots.push(`${String(h).padStart(2, '0')}:30`)
  }
  return (
    <div className="grid grid-cols-4 gap-2">
      {allSlots.map((slot) => {
        const available = slots.includes(slot)
        const isSelected = selected === slot
        return (
          <button
            key={slot}
            disabled={!available}
            onClick={() => onSelect(slot)}
            className={`py-2.5 text-sm font-body rounded-sm transition-colors border
              ${!available ? 'border-border/30 text-text-secondary/30 line-through cursor-not-allowed' : ''}
              ${isSelected ? 'bg-accent border-accent text-white font-medium' : ''}
              ${available && !isSelected ? 'border-border text-text-primary hover:border-accent hover:text-accent' : ''}
            `}
          >
            {slot}
          </button>
        )
      })}
    </div>
  )
}

const STEPS = ['Serviço', 'Data', 'Horário', 'Confirmar']

export default function Book() {
  const navigate = useNavigate()
  const toast = useToast()
  const { getMessage } = useApiError()
  const userId = useAuthStore((s) => s.userId)
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)

  const [step, setStep] = useState(0)
  const [selectedService, setSelectedService] = useState<Service | null>(null)
  const [selectedDate, setSelectedDate] = useState<Date | null>(null)
  const [selectedSlot, setSelectedSlot] = useState<string | null>(null)

  const dateString = selectedDate ? toISODate(selectedDate) : ''

  const { data: services = [], isLoading: loadingServices } = useQuery({
    queryKey: ['services'],
    queryFn: servicesApi.getActive,
  })

  const { data: slots = [], isFetching: loadingSlots, isError: slotsError } = useQuery({
    queryKey: ['slots', dateString, selectedService?.id],
    queryFn: () => appointmentsApi.getAvailableSlots(dateString, selectedService!.id),
    enabled: !!dateString && !!selectedService && step >= 2,
  })

  const { mutate: createAppointment, isPending } = useMutation({
    mutationFn: () =>
      appointmentsApi.create({
        userId: userId!,
        date: dateString,
        startTime: selectedSlot!,
        serviceId: selectedService!.id,
      }),
    onSuccess: () => {
      toast('Agendamento criado com sucesso!', 'success')
      navigate('/app/appointments')
    },
    onError: (err: Error) => {
      toast(getMessage(err.message, 'Erro ao agendar.'), 'error')
    },
  })

  const formattedDate = selectedDate
    ? selectedDate.toLocaleDateString('pt-BR', { weekday: 'long', day: 'numeric', month: 'long' })
    : ''

  return (
    <div className="min-h-screen bg-bg-base flex flex-col">
      <div className="bg-bg-surface border-b border-border px-5 pt-10 pb-4 flex items-center gap-3">
        <button
          onClick={() => step === 0 ? navigate(-1) : setStep(s => s - 1)}
          className="text-text-secondary hover:text-text-primary"
        >
          <ChevronLeft size={22} />
        </button>
        <h1 className="font-display font-bold text-2xl uppercase flex-1">Agendar</h1>
        {!isAuthenticated && (
          <div className="flex items-center gap-2">
            <Button size="sm" variant="ghost" onClick={() => navigate('/login?mode=register', { state: { returnTo: '/book' } })}>
              Criar conta
            </Button>
            <Button size="sm" onClick={() => navigate('/login', { state: { returnTo: '/book' } })}>
              Entrar
            </Button>
          </div>
        )}
      </div>

      {/* Step indicator */}
      <div className="px-5 pt-4 flex items-center gap-2">
        {STEPS.map((label, i) => (
          <div key={i} className="flex items-center gap-2 flex-1">
            <div className={`w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold transition-colors ${
              i < step ? 'bg-accent text-white' : i === step ? 'border-2 border-accent text-accent' : 'border border-border text-text-secondary'
            }`}>
              {i < step ? <Check size={12} /> : i + 1}
            </div>
            <span className={`text-xs font-body ${i === step ? 'text-text-primary' : 'text-text-secondary'}`}>{label}</span>
            {i < STEPS.length - 1 && <div className={`flex-1 h-px ${i < step ? 'bg-accent' : 'bg-border'}`} />}
          </div>
        ))}
      </div>

      <Modal open={step === 3 && !isAuthenticated} onClose={() => setStep(2)} title="Conta necessária">
        <div className="flex flex-col gap-4">
          <p className="text-sm font-body text-text-secondary">
            Para finalizar o agendamento, crie uma conta ou entre na sua.
          </p>
          <Button fullWidth size="lg" onClick={() => navigate('/login', { state: { returnTo: '/book' } })}>
            Entrar
          </Button>
          <Button fullWidth size="lg" variant="ghost" onClick={() => navigate('/login?mode=register', { state: { returnTo: '/book' } })}>
            Criar conta
          </Button>
        </div>
      </Modal>

      <div className="flex-1 px-5 pt-6 pb-24">
        {/* Step 0 — Service */}
        {step === 0 && (
          <div className="flex flex-col gap-5">
            <div className="flex items-center gap-2 mb-1">
              <Scissors size={16} className="text-accent" />
              <h2 className="font-display font-bold text-sm uppercase tracking-widest text-text-secondary">Escolha o serviço</h2>
            </div>
            {loadingServices ? (
              <div className="flex justify-center py-10"><Spinner /></div>
            ) : (
              <div className="flex flex-col gap-3">
                {services.map(s => (
                  <button
                    key={s.id}
                    onClick={() => setSelectedService(s)}
                    className={`w-full text-left p-4 rounded-sm border transition-colors
                      ${selectedService?.id === s.id
                        ? 'border-accent bg-accent/5'
                        : 'border-border hover:border-accent/50'
                      }`}
                  >
                    <p className="font-body font-medium text-text-primary">{s.name}</p>
                    <p className="text-xs text-text-secondary font-body mt-1">
                      {s.durationMinutes} min · R$ {Number(s.price).toFixed(2)}
                    </p>
                  </button>
                ))}
              </div>
            )}
            <Button fullWidth size="lg" disabled={!selectedService} onClick={() => setStep(1)}>
              Próximo
            </Button>
          </div>
        )}

        {/* Step 1 — Date */}
        {step === 1 && (
          <div className="flex flex-col gap-5">
            <div className="flex items-center gap-2 mb-1">
              <Calendar size={16} className="text-accent" />
              <h2 className="font-display font-bold text-sm uppercase tracking-widest text-text-secondary">Escolha a data</h2>
            </div>
            <DatePicker value={selectedDate} onChange={setSelectedDate} />
            <Button fullWidth size="lg" disabled={!selectedDate} onClick={() => setStep(2)}>
              Próximo
            </Button>
          </div>
        )}

        {/* Step 2 — Time */}
        {step === 2 && (
          <div className="flex flex-col gap-5">
            <div>
              <div className="flex items-center gap-2 mb-1">
                <Clock size={16} className="text-accent" />
                <h2 className="font-display font-bold text-sm uppercase tracking-widest text-text-secondary">Escolha o horário</h2>
              </div>
              <p className="text-text-secondary text-xs font-body capitalize">{formattedDate}</p>
            </div>
            {loadingSlots ? (
              <div className="flex justify-center py-10"><Spinner /></div>
            ) : slotsError ? (
              <div className="py-8 flex flex-col items-center gap-2 text-center">
                <p className="text-sm font-body text-text-secondary">Não foi possível carregar os horários.</p>
              </div>
            ) : slots.length === 0 ? (
              <div className="py-8 text-center">
                <p className="text-sm font-body text-text-secondary">Nenhum horário disponível nesta data.</p>
              </div>
            ) : (
              <SlotGrid slots={slots} selected={selectedSlot} onSelect={setSelectedSlot} />
            )}
            <Button fullWidth size="lg" disabled={!selectedSlot} onClick={() => setStep(3)}>
              Próximo
            </Button>
          </div>
        )}

        {/* Step 3 — Confirm */}
        {step === 3 && (
          <div className="flex flex-col gap-5">
            <h2 className="font-display font-bold text-sm uppercase tracking-widest text-text-secondary">Confirmação</h2>
            <div className="bg-bg-surface border border-border border-l-[3px] border-l-accent rounded-sm p-5 flex flex-col gap-3">
              <div className="flex justify-between items-center">
                <span className="text-xs font-body text-text-secondary uppercase tracking-wider">Serviço</span>
                <span className="font-body font-medium text-text-primary">{selectedService?.name}</span>
              </div>
              <div className="h-px bg-border" />
              <div className="flex justify-between items-center">
                <span className="text-xs font-body text-text-secondary uppercase tracking-wider">Data</span>
                <span className="font-body font-medium text-text-primary capitalize">{formattedDate}</span>
              </div>
              <div className="h-px bg-border" />
              <div className="flex justify-between items-center">
                <span className="text-xs font-body text-text-secondary uppercase tracking-wider">Horário</span>
                <span className="font-display font-bold text-2xl text-text-primary">{selectedSlot}</span>
              </div>
              <div className="h-px bg-border" />
              <div className="flex justify-between items-center">
                <span className="text-xs font-body text-text-secondary uppercase tracking-wider">Valor</span>
                <span className="font-body font-medium text-accent">R$ {Number(selectedService?.price).toFixed(2)}</span>
              </div>
            </div>
            <Button fullWidth size="lg" loading={isPending} onClick={() => createAppointment()}>
              Confirmar Agendamento
            </Button>
          </div>
        )}
      </div>
    </div>
  )
}
```

- [ ] **Step 4: Verify in browser**

Navigate to `/book`. Confirm: step 0 shows service cards, selecting one enables "Próximo", step 1 shows calendar, step 2 shows slots, step 3 shows summary with service name and price.

- [ ] **Step 5: Commit**

```bash
git add frontend/src/pages/client/Book.tsx frontend/src/api/appointments.ts frontend/src/types/appointment.ts
git commit -m "feat(frontend): add service selection as step 1 in Book wizard"
```

---

### Task 8: Update backend GetAvailableSlots to accept serviceId

The slots endpoint currently ignores duration. After Slice 3 (WorkingHours), it will fully use service duration. For now, update the signature so it accepts `serviceId` and uses the service's `DurationMinutes` for slot generation.

**Files:**
- Modify: `src/BarberShop.Communication/Models/Appointment/GetAvailableSlotsModel.cs`
- Modify: `src/BarberShop.Application/UseCases/Appointment/GetAvailableSlots/GetAvailableSlotsUseCase.cs`
- Modify: `src/BarberShop.Api/Controllers/AppointmentController.cs`

- [ ] **Step 1: Update model**

```csharp
// src/BarberShop.Communication/Models/Appointment/GetAvailableSlotsModel.cs
namespace BarberShop.Communication.Models.Appointment
{
    public class GetAvailableSlotsModel
    {
        public DateOnly Date { get; set; }
        public long ServiceId { get; set; }
    }
}
```

- [ ] **Step 2: Update use case to use service duration**

```csharp
// GetAvailableSlotsUseCase.cs — replace existing implementation
using BarberShop.Communication.Enums.Appointment;
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Appointment;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Appointment.GetAvailableSlots
{
    public class GetAvailableSlotsUseCase : IGetAvailableSlotsUseCase
    {
        private readonly IBaseRepository<Domain.Appointment> _appointmentRepository;
        private readonly IBaseRepository<Domain.Service> _serviceRepository;
        private static readonly TimeOnly _openTime = new(9, 0);
        private static readonly TimeOnly _closeTime = new(18, 0);

        public GetAvailableSlotsUseCase(
            IBaseRepository<Domain.Appointment> appointmentRepository,
            IBaseRepository<Domain.Service> serviceRepository)
        {
            _appointmentRepository = appointmentRepository;
            _serviceRepository = serviceRepository;
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(GetAvailableSlotsModel model)
        {
            var service = await _serviceRepository.Get(model.ServiceId);
            if (service == null)
                return FactoryResponse<dynamic>.NotFound("Serviço não encontrado.");

            var dayStart = model.Date.ToDateTime(TimeOnly.MinValue);
            var dayEnd = model.Date.ToDateTime(TimeOnly.MaxValue);

            var occupied = await _appointmentRepository.GetAll(
                a => a.StartTime >= dayStart && a.StartTime <= dayEnd
                  && a.Status != EAppointmentStatus.Cancelled);

            var occupiedSlots = occupied.Select(a => TimeOnly.FromDateTime(a.StartTime)).ToHashSet();

            var allSlots = GenerateSlots(_openTime, _closeTime, service.DurationMinutes);
            var now = DateTime.UtcNow;
            var isToday = model.Date == DateOnly.FromDateTime(now);

            var available = allSlots
                .Where(s =>
                {
                    if (isToday && model.Date.ToDateTime(s) <= now) return false;
                    return !occupiedSlots.Contains(s);
                })
                .Select(s => s.ToString("HH:mm"))
                .ToList();

            return FactoryResponse<dynamic>.Success(available);
        }

        private static List<TimeOnly> GenerateSlots(TimeOnly open, TimeOnly close, int durationMinutes)
        {
            var slots = new List<TimeOnly>();
            var current = open;
            while (current.AddMinutes(durationMinutes) <= close)
            {
                slots.Add(current);
                current = current.AddMinutes(durationMinutes);
            }
            return slots;
        }
    }
}
```

- [ ] **Step 3: Update controller action signature**

In `AppointmentController.cs`, update the `GetAvailableSlots` action:

```csharp
/// <summary>Retorna os horários disponíveis em uma data para um serviço</summary>
[AllowAnonymous]
[HttpGet("available-slots")]
public async Task<IActionResult> GetAvailableSlots([FromQuery] DateOnly date, [FromQuery] long serviceId)
{
    var result = await _getAvailableSlotsUseCase.ExecuteAsync(
        new GetAvailableSlotsModel { Date = date, ServiceId = serviceId });
    return Result(result);
}
```

- [ ] **Step 4: Build and verify**

```bash
dotnet build src/BarberShop.Api/BarberShop.Api.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 5: Commit**

```bash
git add src/BarberShop.Communication/Models/Appointment/GetAvailableSlotsModel.cs src/BarberShop.Application/UseCases/Appointment/GetAvailableSlots/GetAvailableSlotsUseCase.cs src/BarberShop.Api/Controllers/AppointmentController.cs
git commit -m "feat(backend): update GetAvailableSlots to use service duration"
```

---

**Slice 1 complete.** The system now supports services end-to-end: admin can manage services, clients pick a service before booking, and slot generation respects service duration.

Continue with `2026-05-09-saas-expansion-slice2-barbers.md`.
