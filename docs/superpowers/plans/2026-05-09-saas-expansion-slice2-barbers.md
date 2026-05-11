# BarberAgenda — Slice 2: Barbers Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a `Barber` role and entity; expose barber CRUD; add admin Barbers page; add `BarberRoute` + barber panel layout; add barber selection as step 2 of the client booking flow.

**Prerequisite:** Slice 1 (Services) must be complete.

**Architecture:** Seed new `Barber` module + `Barbeiro` profile → `Barber` entity + `CreateBarberUseCase` (creates User + Barber atomically) → `BarberController` → frontend `BarberRoute` + barber panel + `Book.tsx` step 2.

**Tech Stack:** .NET 8, EF Core / MySQL, React + Vite + TypeScript, TanStack Query, Tailwind CSS

**Spec:** `docs/superpowers/specs/2026-05-09-saas-backoffice-client-redesign.md` — Slice 2

---

## File Map

**Create (backend):**
- `src/BarberShop.Domain/Barber.cs`
- `src/BarberShop.Infra/Mappings/BarberMap.cs`
- `src/BarberShop.Communication/Models/Barber/CreateBarberModel.cs`
- `src/BarberShop.Communication/Models/Barber/UpdateBarberModel.cs`
- `src/BarberShop.Communication/Models/Barber/ChangeBarberStatusModel.cs`
- `src/BarberShop.Application/UseCases/Barber/Create/ICreateBarberUseCase.cs`
- `src/BarberShop.Application/UseCases/Barber/Create/CreateBarberUseCase.cs`
- `src/BarberShop.Application/UseCases/Barber/Update/IUpdateBarberUseCase.cs`
- `src/BarberShop.Application/UseCases/Barber/Update/UpdateBarberUseCase.cs`
- `src/BarberShop.Application/UseCases/Barber/GetAll/IGetBarbersUseCase.cs`
- `src/BarberShop.Application/UseCases/Barber/GetAll/GetBarbersUseCase.cs`
- `src/BarberShop.Application/UseCases/Barber/ChangeStatus/IChangeBarberStatusUseCase.cs`
- `src/BarberShop.Application/UseCases/Barber/ChangeStatus/ChangeBarberStatusUseCase.cs`
- `src/BarberShop.Api/Controllers/BarberController.cs`

**Modify (backend):**
- `src/BarberShop.Infra/DataAccess/BarberShopContext.cs` — add `DbSet<Barber>`
- `src/BarberShop.Infra/DataAccess/DatabaseSeeder.cs` — add Barber module + Barbeiro profile
- `src/BarberShop.IOC/Injection.cs` — register barber use cases

**Create (frontend):**
- `frontend/src/types/barber.ts`
- `frontend/src/api/barbers.ts`
- `frontend/src/router/BarberRoute.tsx`
- `frontend/src/components/layout/BarberLayout.tsx`
- `frontend/src/components/layout/BarberSidebar.tsx`
- `frontend/src/pages/barber/Dashboard.tsx`
- `frontend/src/pages/admin/Barbers.tsx`

**Modify (frontend):**
- `frontend/src/types/auth.ts` — add `'barber'` to `UserRole`
- `frontend/src/store/authStore.ts` — detect barber role
- `frontend/src/router/index.tsx` — add `/barber/*` routes, update `RootRedirect`
- `frontend/src/pages/client/Book.tsx` — add barber selection step

---

### Task 9: Barber domain entity + seeder + migration

**Files:**
- Create: `src/BarberShop.Domain/Barber.cs`
- Create: `src/BarberShop.Infra/Mappings/BarberMap.cs`
- Modify: `src/BarberShop.Infra/DataAccess/BarberShopContext.cs`
- Modify: `src/BarberShop.Infra/DataAccess/DatabaseSeeder.cs`

- [ ] **Step 1: Create Barber entity**

```csharp
// src/BarberShop.Domain/Barber.cs
namespace BarberShop.Domain
{
    public class Barber : BaseEntity
    {
        public long UserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public User User { get; set; } = null!;
    }
}
```

- [ ] **Step 2: Create BarberMap**

```csharp
// src/BarberShop.Infra/Mappings/BarberMap.cs
using BarberShop.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberShop.Infra.Mappings
{
    public class BarberMap : IEntityTypeConfiguration<Barber>
    {
        public void Configure(EntityTypeBuilder<Barber> builder)
        {
            builder.ToTable("Barbers");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.DisplayName).IsRequired().HasMaxLength(100);
            builder.HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(b => b.UserId).IsUnique();
        }
    }
}
```

- [ ] **Step 3: Add DbSet**

In `BarberShopContext.cs`, add after `DbSet<Service>`:

```csharp
public DbSet<Barber> Barbers { get; set; } = null!;
```

- [ ] **Step 4: Seed Barber module and Barbeiro profile**

In `DatabaseSeeder.cs`, update `SeedModules` to add Module Id 6:

```csharp
new Module { Id = 6, Name = "Barber", Visualize = true, Edit = true, Register = true, Inactivate = true, Exclude = true },
```

In `SeedProfiles`, add Profile Id 3:

```csharp
new Profile { Id = 3, Name = "Barbeiro", Status = EProfileStatus.Ativo },
```

In `SeedProfileModules`, add Barbeiro profile modules:

```csharp
// Barbeiro — acesso à própria agenda e ao módulo Barber
new ProfileModule { ProfileId = 3, ModuleId = 6, Visualize = true, Edit = true, Register = true, Inactivate = true, Exclude = true },
new ProfileModule { ProfileId = 3, ModuleId = 4, Visualize = true, Edit = false, Register = true, Inactivate = true, Exclude = false },
```

- [ ] **Step 5: Add DefaultBarberProfileId to appsettings**

In `src/BarberShop.Api/appsettings.json`, add:

```json
"DefaultBarberProfileId": 3
```

- [ ] **Step 6: Create and apply migration**

```bash
dotnet ef migrations add AddBarberEntityAndSeedBarberProfile --project src/BarberShop.Infra --startup-project src/BarberShop.Api
dotnet ef database update --project src/BarberShop.Infra --startup-project src/BarberShop.Api
```

- [ ] **Step 7: Commit**

```bash
git add src/BarberShop.Domain/Barber.cs src/BarberShop.Infra/Mappings/BarberMap.cs src/BarberShop.Infra/DataAccess/ src/BarberShop.Infra/Migrations/ src/BarberShop.Api/appsettings.json
git commit -m "feat(backend): add Barber entity, seed Barbeiro profile and Barber module"
```

---

### Task 10: Barber communication models + use cases

**Files:**
- Create: `src/BarberShop.Communication/Models/Barber/CreateBarberModel.cs`
- Create: `src/BarberShop.Communication/Models/Barber/UpdateBarberModel.cs`
- Create: `src/BarberShop.Communication/Models/Barber/ChangeBarberStatusModel.cs`
- Create: all use case files listed in File Map

- [ ] **Step 1: Create models**

```csharp
// CreateBarberModel.cs
namespace BarberShop.Communication.Models.Barber
{
    public class CreateBarberModel
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }
}
```

```csharp
// UpdateBarberModel.cs
namespace BarberShop.Communication.Models.Barber
{
    public class UpdateBarberModel
    {
        public long Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }
}
```

```csharp
// ChangeBarberStatusModel.cs
namespace BarberShop.Communication.Models.Barber
{
    public class ChangeBarberStatusModel
    {
        public long Id { get; set; }
        public bool IsActive { get; set; }
    }
}
```

- [ ] **Step 2: GetAll use case**

```csharp
// IGetBarbersUseCase.cs
using BarberShop.Communication.Models;
namespace BarberShop.Application.UseCases.Barber.GetAll
{
    public interface IGetBarbersUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(bool activeOnly);
    }
}
```

```csharp
// GetBarbersUseCase.cs
using BarberShop.Communication.Models;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Barber.GetAll
{
    public class GetBarbersUseCase : IGetBarbersUseCase
    {
        private readonly IBaseRepository<Domain.Barber> _repo;
        public GetBarbersUseCase(IBaseRepository<Domain.Barber> repo) => _repo = repo;

        public async Task<ResponseModel<dynamic>> ExecuteAsync(bool activeOnly)
        {
            var barbers = activeOnly
                ? await _repo.GetAll(b => b.IsActive)
                : await _repo.Get();

            var result = barbers.Select(b => new
            {
                b.Id, b.UserId, b.DisplayName, b.IsActive
            }).ToList();

            return FactoryResponse<dynamic>.Success(result);
        }
    }
}
```

- [ ] **Step 3: Create use case**

Creating a barber creates a `User` record + a `Barber` record + assigns the `Barbeiro` profile, then sends the reset-password email so the barber can set their own password.

```csharp
// ICreateBarberUseCase.cs
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Barber;
namespace BarberShop.Application.UseCases.Barber.Create
{
    public interface ICreateBarberUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(CreateBarberModel model);
    }
}
```

```csharp
// CreateBarberUseCase.cs
using BarberShop.Application.UseCases.Auth.SendEmailResetPassword;
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Barber;
using BarberShop.Domain.AccessControl;
using BarberShop.Infra.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BarberShop.Application.UseCases.Barber.Create
{
    public class CreateBarberUseCase : ICreateBarberUseCase
    {
        private readonly IBaseRepository<Domain.User> _userRepo;
        private readonly IBaseRepository<Domain.Barber> _barberRepo;
        private readonly ISendEmailResetPasswordUseCase _sendEmail;
        private readonly long _barberProfileId;

        public CreateBarberUseCase(
            IBaseRepository<Domain.User> userRepo,
            IBaseRepository<Domain.Barber> barberRepo,
            ISendEmailResetPasswordUseCase sendEmail,
            IConfiguration configuration)
        {
            _userRepo = userRepo;
            _barberRepo = barberRepo;
            _sendEmail = sendEmail;
            _barberProfileId = configuration.GetValue<long>("DefaultBarberProfileId");
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(CreateBarberModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return FactoryResponse<dynamic>.InvalidModel("Nome é obrigatório.");
            if (string.IsNullOrWhiteSpace(model.Email))
                return FactoryResponse<dynamic>.InvalidModel("E-mail é obrigatório.");
            if (string.IsNullOrWhiteSpace(model.DisplayName))
                return FactoryResponse<dynamic>.InvalidModel("Nome de exibição é obrigatório.");

            var existing = await _userRepo.Get(u => u.Email.ToLower() == model.Email.ToLower());
            if (existing != null)
                return FactoryResponse<dynamic>.Conflict("Já existe uma conta com este e-mail.");

            var user = new Domain.User { Name = model.Name, Email = model.Email };
            user.AddCreationDate();
            user.ProfilesUsers.Add(new ProfileUser { ProfileId = _barberProfileId });

            try
            {
                await _userRepo.Create(user);

                var barber = new Domain.Barber
                {
                    UserId = user.Id,
                    DisplayName = model.DisplayName.Trim(),
                    IsActive = true
                };
                barber.AddCreationDate();
                await _barberRepo.Create(barber);

                await _sendEmail.ExecuteAsync(model.Email);
                return FactoryResponse<dynamic>.SuccessfulCreation(new { barber.Id, barber.DisplayName, user.Email });
            }
            catch (Exception e)
            {
                return FactoryResponse<dynamic>.BadRequestErroInterno(e.Message);
            }
        }
    }
}
```

- [ ] **Step 4: Update and ChangeStatus use cases**

```csharp
// IUpdateBarberUseCase.cs
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Barber;
namespace BarberShop.Application.UseCases.Barber.Update
{
    public interface IUpdateBarberUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(UpdateBarberModel model);
    }
}
```

```csharp
// UpdateBarberUseCase.cs
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Barber;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Barber.Update
{
    public class UpdateBarberUseCase : IUpdateBarberUseCase
    {
        private readonly IBaseRepository<Domain.Barber> _repo;
        public UpdateBarberUseCase(IBaseRepository<Domain.Barber> repo) => _repo = repo;

        public async Task<ResponseModel<dynamic>> ExecuteAsync(UpdateBarberModel model)
        {
            var barber = await _repo.Get(model.Id);
            if (barber == null)
                return FactoryResponse<dynamic>.NotFound("Barbeiro não encontrado.");
            if (string.IsNullOrWhiteSpace(model.DisplayName))
                return FactoryResponse<dynamic>.InvalidModel("Nome de exibição é obrigatório.");

            barber.DisplayName = model.DisplayName.Trim();
            barber.AddUpdateDate();
            await _repo.Update(barber);
            return FactoryResponse<dynamic>.Success(new { barber.Id, barber.DisplayName });
        }
    }
}
```

```csharp
// IChangeBarberStatusUseCase.cs
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Barber;
namespace BarberShop.Application.UseCases.Barber.ChangeStatus
{
    public interface IChangeBarberStatusUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(ChangeBarberStatusModel model);
    }
}
```

```csharp
// ChangeBarberStatusUseCase.cs
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Barber;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Barber.ChangeStatus
{
    public class ChangeBarberStatusUseCase : IChangeBarberStatusUseCase
    {
        private readonly IBaseRepository<Domain.Barber> _repo;
        public ChangeBarberStatusUseCase(IBaseRepository<Domain.Barber> repo) => _repo = repo;

        public async Task<ResponseModel<dynamic>> ExecuteAsync(ChangeBarberStatusModel model)
        {
            var barber = await _repo.Get(model.Id);
            if (barber == null)
                return FactoryResponse<dynamic>.NotFound("Barbeiro não encontrado.");

            barber.IsActive = model.IsActive;
            barber.AddUpdateDate();
            await _repo.Update(barber);
            return FactoryResponse<dynamic>.Success(null);
        }
    }
}
```

- [ ] **Step 5: Commit**

```bash
git add src/BarberShop.Communication/Models/Barber/ src/BarberShop.Application/UseCases/Barber/
git commit -m "feat(backend): add Barber use cases (GetAll, Create, Update, ChangeStatus)"
```

---

### Task 11: BarberController + IOC registration

**Files:**
- Create: `src/BarberShop.Api/Controllers/BarberController.cs`
- Modify: `src/BarberShop.IOC/Injection.cs`

- [ ] **Step 1: Create controller**

```csharp
// src/BarberShop.Api/Controllers/BarberController.cs
using BarberShop.Api.Authorization;
using BarberShop.Application.UseCases.Barber.ChangeStatus;
using BarberShop.Application.UseCases.Barber.Create;
using BarberShop.Application.UseCases.Barber.GetAll;
using BarberShop.Application.UseCases.Barber.Update;
using BarberShop.Communication.Models.Barber;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Controllers
{
    /// <summary>Gerenciamento de barbeiros</summary>
    [APIAuthorization("Users-C", "Users-E", "Users-V", "Users-I", "Barber-C", "Barber-E", "Barber-V")]
    public class BarberController : BaseController
    {
        private readonly IGetBarbersUseCase _getBarbers;
        private readonly ICreateBarberUseCase _create;
        private readonly IUpdateBarberUseCase _update;
        private readonly IChangeBarberStatusUseCase _changeStatus;

        public BarberController(
            IGetBarbersUseCase getBarbers,
            ICreateBarberUseCase create,
            IUpdateBarberUseCase update,
            IChangeBarberStatusUseCase changeStatus)
        {
            _getBarbers = getBarbers;
            _create = create;
            _update = update;
            _changeStatus = changeStatus;
        }

        /// <summary>Lista barbeiros ativos (público)</summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetActive()
            => Result(await _getBarbers.ExecuteAsync(activeOnly: true));

        /// <summary>Lista todos os barbeiros (admin)</summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
            => Result(await _getBarbers.ExecuteAsync(activeOnly: false));

        /// <summary>Cria um novo barbeiro (cria usuário + perfil barbeiro + envia e-mail de senha)</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBarberModel model)
            => Result(await _create.ExecuteAsync(model));

        /// <summary>Atualiza dados do barbeiro</summary>
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateBarberModel model)
            => Result(await _update.ExecuteAsync(model));

        /// <summary>Ativa ou inativa um barbeiro</summary>
        [HttpPatch("status")]
        public async Task<IActionResult> ChangeStatus([FromBody] ChangeBarberStatusModel model)
            => Result(await _changeStatus.ExecuteAsync(model));
    }
}
```

- [ ] **Step 2: Register in IOC**

In `Injection.cs`, add usings:

```csharp
using BarberShop.Application.UseCases.Barber.ChangeStatus;
using BarberShop.Application.UseCases.Barber.Create;
using BarberShop.Application.UseCases.Barber.GetAll;
using BarberShop.Application.UseCases.Barber.Update;
```

Add registration block:

```csharp
// Barber use cases
services.AddScoped<IGetBarbersUseCase, GetBarbersUseCase>();
services.AddScoped<ICreateBarberUseCase, CreateBarberUseCase>();
services.AddScoped<IUpdateBarberUseCase, UpdateBarberUseCase>();
services.AddScoped<IChangeBarberStatusUseCase, ChangeBarberStatusUseCase>();
```

- [ ] **Step 3: Build**

```bash
dotnet build src/BarberShop.Api/BarberShop.Api.csproj
```

Expected: 0 errors.

- [ ] **Step 4: Commit**

```bash
git add src/BarberShop.Api/Controllers/BarberController.cs src/BarberShop.IOC/Injection.cs
git commit -m "feat(backend): add BarberController and register barber use cases in IOC"
```

---

### Task 12: Frontend auth — add barber role

**Files:**
- Modify: `frontend/src/types/auth.ts`
- Modify: `frontend/src/store/authStore.ts`

- [ ] **Step 1: Update UserRole type**

In `frontend/src/types/auth.ts`, change:

```ts
export type UserRole = 'admin' | 'client' | 'barber'
```

- [ ] **Step 2: Update role detection in authStore**

In `frontend/src/store/authStore.ts`, update the `setAuth` action's role detection:

```ts
role: (() => {
  const modules = data.modulesAssembled.moduleProfileUser.map(m => m.name)
  if (modules.includes('Users')) return 'admin'
  if (modules.includes('Barber')) return 'barber'
  return 'client'
})(),
```

- [ ] **Step 3: Commit**

```bash
git add frontend/src/types/auth.ts frontend/src/store/authStore.ts
git commit -m "feat(frontend): add barber role detection to auth store"
```

---

### Task 13: BarberRoute guard + Barber panel layout

**Files:**
- Create: `frontend/src/router/BarberRoute.tsx`
- Create: `frontend/src/components/layout/BarberSidebar.tsx`
- Create: `frontend/src/components/layout/BarberLayout.tsx`
- Create: `frontend/src/pages/barber/Dashboard.tsx`
- Modify: `frontend/src/router/index.tsx`

- [ ] **Step 1: Create BarberRoute guard**

```tsx
// frontend/src/router/BarberRoute.tsx
import { Navigate, Outlet } from 'react-router-dom'
import { useAuthStore } from '../store/authStore'

export function BarberRoute() {
  const role = useAuthStore((s) => s.role)
  if (role !== 'barber') return <Navigate to="/login" replace />
  return <Outlet />
}
```

- [ ] **Step 2: Create BarberSidebar**

```tsx
// frontend/src/components/layout/BarberSidebar.tsx
import { useState } from 'react'
import { NavLink, useNavigate } from 'react-router-dom'
import { LayoutDashboard, Calendar, Clock, Lock, LogOut, Scissors, ChevronLeft, ChevronRight } from 'lucide-react'
import { useAuth } from '../../hooks/useAuth'

const links = [
  { to: '/barber', label: 'Dashboard', Icon: LayoutDashboard, end: true },
  { to: '/barber/schedule', label: 'Agenda', Icon: Calendar, end: false },
  { to: '/barber/working-hours', label: 'Horários', Icon: Clock, end: false },
  { to: '/barber/blocks', label: 'Bloqueios', Icon: Lock, end: false },
]

export function BarberSidebar() {
  const { logout } = useAuth()
  const navigate = useNavigate()
  const [collapsed, setCollapsed] = useState(false)

  return (
    <aside className={`${collapsed ? 'w-16' : 'w-56'} bg-bg-surface border-r border-border flex flex-col min-h-screen transition-all duration-200 flex-shrink-0`}>
      <div className="px-3 py-4 border-b border-border flex items-center justify-between gap-2">
        <button onClick={() => navigate('/barber')} className="flex items-center gap-2 min-w-0 overflow-hidden">
          <Scissors size={18} className="text-accent flex-shrink-0" />
          {!collapsed && (
            <span className="font-display font-bold text-base tracking-widest uppercase truncate">
              Barber<span className="text-accent">Agenda</span>
            </span>
          )}
        </button>
        <button onClick={() => setCollapsed(c => !c)} className="text-text-secondary hover:text-text-primary flex-shrink-0">
          {collapsed ? <ChevronRight size={14} /> : <ChevronLeft size={14} />}
        </button>
      </div>
      <nav className="flex-1 p-2 flex flex-col gap-1">
        {links.map(({ to, label, Icon, end }) => (
          <NavLink
            key={to}
            to={to}
            end={end}
            title={collapsed ? label : undefined}
            className={({ isActive }) =>
              `flex items-center gap-3 px-3 py-2.5 rounded-sm text-sm font-body transition-colors
              ${collapsed ? 'justify-center' : ''}
              ${isActive ? 'bg-accent/10 text-accent border-l-2 border-accent' : 'text-text-secondary hover:text-text-primary hover:bg-bg-elevated'}`
            }
          >
            <Icon size={16} strokeWidth={1.5} />
            {!collapsed && label}
          </NavLink>
        ))}
      </nav>
      <div className="p-2 border-t border-border">
        <button
          onClick={logout}
          title={collapsed ? 'Sair' : undefined}
          className={`flex items-center gap-3 px-3 py-2.5 w-full text-sm font-body text-text-secondary hover:text-red-400 transition-colors ${collapsed ? 'justify-center' : ''}`}
        >
          <LogOut size={16} strokeWidth={1.5} />
          {!collapsed && 'Sair'}
        </button>
      </div>
    </aside>
  )
}
```

- [ ] **Step 3: Create BarberLayout**

```tsx
// frontend/src/components/layout/BarberLayout.tsx
import { Outlet } from 'react-router-dom'
import { BarberSidebar } from './BarberSidebar'

export function BarberLayout() {
  return (
    <div className="flex min-h-screen bg-bg-base">
      <BarberSidebar />
      <main className="flex-1 overflow-y-auto p-6">
        <Outlet />
      </main>
    </div>
  )
}
```

- [ ] **Step 4: Create Barber Dashboard**

```tsx
// frontend/src/pages/barber/Dashboard.tsx
import { useQuery } from '@tanstack/react-query'
import { Calendar, Clock } from 'lucide-react'
import { appointmentsApi } from '../../api/appointments'
import { AppointmentBadge, Badge } from '../../components/ui/Badge'
import { Spinner } from '../../components/ui/Spinner'
import { useAuthStore } from '../../store/authStore'

function isToday(isoString: string) {
  const d = new Date(isoString)
  const today = new Date()
  return d.getDate() === today.getDate() && d.getMonth() === today.getMonth() && d.getFullYear() === today.getFullYear()
}

function isThisWeek(isoString: string) {
  const d = new Date(isoString)
  const now = new Date()
  const weekStart = new Date(now)
  weekStart.setDate(now.getDate() - now.getDay())
  weekStart.setHours(0, 0, 0, 0)
  const weekEnd = new Date(weekStart)
  weekEnd.setDate(weekStart.getDate() + 7)
  return d >= weekStart && d < weekEnd
}

function StatCard({ label, value, Icon }: { label: string; value: number; Icon: React.ElementType }) {
  return (
    <div className="bg-bg-surface border border-border border-l-[3px] border-l-accent rounded-sm p-5 flex items-center gap-4">
      <div className="w-10 h-10 bg-accent/10 rounded-sm flex items-center justify-center">
        <Icon size={18} className="text-accent" />
      </div>
      <div>
        <p className="text-text-secondary text-xs font-body uppercase tracking-wider">{label}</p>
        <p className="font-display font-bold text-3xl">{value}</p>
      </div>
    </div>
  )
}

export default function BarberDashboard() {
  const userId = useAuthStore((s) => s.userId)
  const userName = useAuthStore((s) => s.userName)

  const { data: appointments = [], isLoading } = useQuery({
    queryKey: ['barber-appointments'],
    queryFn: () => appointmentsApi.getByUser(userId!),
    enabled: !!userId,
  })

  const todayCount = appointments.filter((a) => isToday(a.startTime) && a.status !== 2).length
  const weekCount = appointments.filter((a) => isThisWeek(a.startTime) && a.status !== 2).length
  const recent = [...appointments]
    .sort((a, b) => new Date(b.creationDate).getTime() - new Date(a.creationDate).getTime())
    .slice(0, 10)

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <p className="text-xs font-display uppercase tracking-[0.2em] text-accent mb-1">BarberAgenda</p>
          <h1 className="font-display font-extrabold text-4xl uppercase leading-none">
            Olá, {userName?.split(' ')[0] ?? 'Barbeiro'}
          </h1>
          <p className="text-text-secondary text-sm font-body mt-2">Sua agenda de hoje</p>
        </div>
        <Badge className="bg-accent/10 text-accent border border-accent/30">Barbeiro</Badge>
      </div>

      {isLoading ? (
        <div className="flex justify-center py-10"><Spinner /></div>
      ) : (
        <>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <StatCard label="Agendamentos hoje" value={todayCount} Icon={Calendar} />
            <StatCard label="Esta semana" value={weekCount} Icon={Clock} />
          </div>
          <section>
            <h2 className="font-display font-bold text-sm uppercase tracking-widest text-text-secondary mb-3">Recentes</h2>
            <div className="bg-bg-surface border border-border rounded-sm overflow-hidden">
              {recent.length === 0 && (
                <p className="text-text-secondary text-sm font-body p-5">Nenhum agendamento.</p>
              )}
              {recent.map((appt, i) => {
                const d = new Date(appt.startTime)
                return (
                  <div key={appt.id} className={`flex items-center justify-between px-4 py-3 ${i !== recent.length - 1 ? 'border-b border-border' : ''}`}>
                    <div>
                      <p className="text-sm font-body font-medium">
                        {d.toLocaleDateString('pt-BR')} — {d.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}
                      </p>
                      <p className="text-xs text-text-secondary font-body">ID #{appt.id}</p>
                    </div>
                    <AppointmentBadge status={appt.status} />
                  </div>
                )
              })}
            </div>
          </section>
        </>
      )}
    </div>
  )
}
```

- [ ] **Step 5: Add barber routes to router**

In `frontend/src/router/index.tsx`, add imports:

```ts
import { BarberRoute } from './BarberRoute'
import { BarberLayout } from '../components/layout/BarberLayout'
import BarberDashboard from '../pages/barber/Dashboard'
```

Update `RootRedirect`:

```tsx
function RootRedirect() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const role = useAuthStore((s) => s.role)
  if (!isAuthenticated) return <Navigate to="/book" replace />
  if (role === 'admin') return <Navigate to="/admin" replace />
  if (role === 'barber') return <Navigate to="/barber" replace />
  return <Navigate to="/app" replace />
}
```

Add barber route group (before the admin block):

```ts
{
  path: '/barber',
  element: <BarberRoute />,
  children: [
    {
      element: <BarberLayout />,
      children: [
        { index: true, element: <BarberDashboard /> },
      ],
    },
  ],
},
```

- [ ] **Step 6: Commit**

```bash
git add frontend/src/router/BarberRoute.tsx frontend/src/components/layout/BarberLayout.tsx frontend/src/components/layout/BarberSidebar.tsx frontend/src/pages/barber/Dashboard.tsx frontend/src/router/index.tsx
git commit -m "feat(frontend): add BarberRoute guard, BarberLayout, and barber dashboard"
```

---

### Task 14: Admin Barbers page

**Files:**
- Create: `frontend/src/types/barber.ts`
- Create: `frontend/src/api/barbers.ts`
- Create: `frontend/src/pages/admin/Barbers.tsx`
- Modify: `frontend/src/router/index.tsx`
- Modify: `frontend/src/components/layout/Sidebar.tsx`

- [ ] **Step 1: Barber types and API**

```ts
// frontend/src/types/barber.ts
export interface Barber {
  id: number
  userId: number
  displayName: string
  isActive: boolean
}
```

```ts
// frontend/src/api/barbers.ts
import { apiClient } from './client'
import type { ApiResponse } from '../types/auth'
import type { Barber } from '../types/barber'

export const barbersApi = {
  getActive: async (): Promise<Barber[]> => {
    const { data } = await apiClient.get<ApiResponse<Barber[]>>('/barber')
    return data.data ?? []
  },

  getAll: async (): Promise<Barber[]> => {
    const { data } = await apiClient.get<ApiResponse<Barber[]>>('/barber/all')
    return data.data ?? []
  },

  create: async (payload: { name: string; email: string; displayName: string }) => {
    const { data } = await apiClient.post<ApiResponse<Barber>>('/barber', payload)
    if (!data.success) throw new Error(data.responseLabel)
    return data
  },

  update: async (payload: { id: number; displayName: string }) => {
    const { data } = await apiClient.put<ApiResponse<Barber>>('/barber', payload)
    if (!data.success) throw new Error(data.responseLabel)
    return data
  },

  changeStatus: async (id: number, isActive: boolean) => {
    const { data } = await apiClient.patch<ApiResponse<null>>('/barber/status', { id, isActive })
    if (!data.success) throw new Error(data.responseLabel)
    return data
  },
}
```

- [ ] **Step 2: Create Barbers page**

```tsx
// frontend/src/pages/admin/Barbers.tsx
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

interface BarberForm { name: string; email: string; displayName: string }
const emptyForm: BarberForm = { name: '', email: '', displayName: '' }

export default function Barbers() {
  const toast = useToast()
  const qc = useQueryClient()
  const [modalOpen, setModalOpen] = useState(false)
  const [editing, setEditing] = useState<Barber | null>(null)
  const [form, setForm] = useState<BarberForm>(emptyForm)

  const { data: barbers = [], isLoading } = useQuery({
    queryKey: ['barbers-all'],
    queryFn: barbersApi.getAll,
  })

  const { mutate: save, isPending: saving } = useMutation({
    mutationFn: () =>
      editing
        ? barbersApi.update({ id: editing.id, displayName: form.displayName })
        : barbersApi.create({ name: form.name, email: form.email, displayName: form.displayName }),
    onSuccess: () => {
      toast(editing ? 'Barbeiro atualizado!' : 'Barbeiro criado! E-mail enviado para definir senha.', 'success')
      qc.invalidateQueries({ queryKey: ['barbers-all'] })
      qc.invalidateQueries({ queryKey: ['barbers'] })
      setModalOpen(false)
    },
    onError: () => toast('Erro ao salvar barbeiro.', 'error'),
  })

  const { mutate: toggleStatus } = useMutation({
    mutationFn: (b: Barber) => barbersApi.changeStatus(b.id, !b.isActive),
    onSuccess: () => {
      toast('Status atualizado!', 'success')
      qc.invalidateQueries({ queryKey: ['barbers-all'] })
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
    setForm({ name: '', email: '', displayName: b.displayName })
    setModalOpen(true)
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-xs font-display uppercase tracking-[0.2em] text-accent mb-1">BarberAgenda</p>
          <h1 className="font-display font-extrabold text-4xl uppercase leading-none">Barbeiros</h1>
        </div>
        <Button onClick={openCreate}><Plus size={16} className="mr-1" />Novo Barbeiro</Button>
      </div>

      {isLoading ? (
        <div className="flex justify-center py-10"><Spinner /></div>
      ) : (
        <div className="bg-bg-surface border border-border rounded-sm overflow-hidden">
          {barbers.length === 0 && (
            <p className="text-text-secondary text-sm font-body p-5">Nenhum barbeiro cadastrado.</p>
          )}
          {barbers.map((b, i) => (
            <div key={b.id} className={`flex items-center justify-between px-4 py-3 gap-4 ${i !== barbers.length - 1 ? 'border-b border-border' : ''}`}>
              <div className="flex-1">
                <p className={`font-body font-medium ${!b.isActive ? 'text-text-secondary line-through' : 'text-text-primary'}`}>
                  {b.displayName}
                </p>
                <p className="text-xs text-text-secondary font-body">ID #{b.id}</p>
              </div>
              <div className="flex items-center gap-2">
                <button onClick={() => openEdit(b)} className="text-text-secondary hover:text-text-primary"><Edit2 size={16} /></button>
                <button onClick={() => toggleStatus(b)} className={b.isActive ? 'text-accent' : 'text-text-secondary'}>
                  {b.isActive ? <ToggleRight size={20} /> : <ToggleLeft size={20} />}
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      <Modal open={modalOpen} onClose={() => setModalOpen(false)} title={editing ? 'Editar Barbeiro' : 'Novo Barbeiro'}>
        <div className="flex flex-col gap-4">
          {!editing && (
            <>
              <Input label="Nome completo" value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} />
              <Input label="E-mail" type="email" value={form.email} onChange={e => setForm(f => ({ ...f, email: e.target.value }))} />
            </>
          )}
          <Input label="Nome de exibição" value={form.displayName} onChange={e => setForm(f => ({ ...f, displayName: e.target.value }))} placeholder="Ex: João" />
          {!editing && (
            <p className="text-xs text-text-secondary font-body">Um e-mail será enviado para o barbeiro definir sua senha.</p>
          )}
          <Button fullWidth loading={saving} onClick={() => save()}>Salvar</Button>
        </div>
      </Modal>
    </div>
  )
}
```

- [ ] **Step 3: Add to router and sidebar**

In `router/index.tsx`, add:
```ts
import AdminBarbers from '../pages/admin/Barbers'
```
Add to admin children:
```ts
{ path: 'barbers', element: <AdminBarbers /> },
```

In `Sidebar.tsx`, add to `links`:
```ts
import { LayoutDashboard, Calendar, Users, LogOut, Scissors, User2, ChevronLeft, ChevronRight } from 'lucide-react'
// ...
{ to: '/admin/barbers', label: 'Barbeiros', Icon: User2, end: false },
```

- [ ] **Step 4: Commit**

```bash
git add frontend/src/types/barber.ts frontend/src/api/barbers.ts frontend/src/pages/admin/Barbers.tsx frontend/src/router/index.tsx frontend/src/components/layout/Sidebar.tsx
git commit -m "feat(frontend): add Admin Barbers CRUD page"
```

---

### Task 15: Book wizard — add Barber selection step (Step 1)

**Files:**
- Modify: `frontend/src/pages/client/Book.tsx`
- Modify: `frontend/src/api/appointments.ts`

After Slice 1, `Book.tsx` has steps: Serviço → Data → Horário → Confirmar. Now insert Barbeiro as step 1: Serviço → **Barbeiro** → Data → Horário → Confirmar (5 steps total). The `available-slots` query also needs `barberId`.

- [ ] **Step 1: Update appointments API**

In `frontend/src/api/appointments.ts`, update `getAvailableSlots` to accept `barberId`:

```ts
getAvailableSlots: async (date: string, serviceId: number, barberId: number): Promise<string[]> => {
  const { data } = await apiClient.get<ApiResponse<string[]>>('/appointment/available-slots', {
    params: { date, serviceId, barberId },
  })
  return data.data ?? []
},
```

Update `CreateAppointmentRequest` in `frontend/src/types/appointment.ts`:

```ts
export interface CreateAppointmentRequest {
  userId: number
  date: string
  startTime: string
  serviceId: number
  barberId: number
}
```

- [ ] **Step 2: Update Book.tsx — add barber step**

Replace `STEPS` and add barber state and step. The diff from Slice 1's Book.tsx:

1. Add import: `import { barbersApi } from '../../api/barbers'`
2. Add import: `import type { Barber } from '../../types/barber'`
3. Change `STEPS`:
```ts
const STEPS = ['Serviço', 'Barbeiro', 'Data', 'Horário', 'Confirmar']
```

4. Add state: `const [selectedBarber, setSelectedBarber] = useState<Barber | null>(null)`

5. Add barbers query after services query:
```ts
const { data: barbers = [], isLoading: loadingBarbers } = useQuery({
  queryKey: ['barbers'],
  queryFn: barbersApi.getActive,
})
```

6. Update slots query to include `barberId`:
```ts
const { data: slots = [], isFetching: loadingSlots, isError: slotsError } = useQuery({
  queryKey: ['slots', dateString, selectedService?.id, selectedBarber?.id],
  queryFn: () => appointmentsApi.getAvailableSlots(dateString, selectedService!.id, selectedBarber!.id),
  enabled: !!dateString && !!selectedService && !!selectedBarber && step >= 3,
})
```

7. Update `createAppointment` mutationFn to include `barberId`:
```ts
appointmentsApi.create({
  userId: userId!,
  date: dateString,
  startTime: selectedSlot!,
  serviceId: selectedService!.id,
  barberId: selectedBarber!.id,
})
```

8. Update auth gate modal `open` condition: `step === 4 && !isAuthenticated`

9. Add step 1 (barber selection) between service (step 0) and date (step 2):
```tsx
{/* Step 1 — Barber */}
{step === 1 && (
  <div className="flex flex-col gap-5">
    <div className="flex items-center gap-2 mb-1">
      <User2 size={16} className="text-accent" />
      <h2 className="font-display font-bold text-sm uppercase tracking-widest text-text-secondary">Escolha o barbeiro</h2>
    </div>
    {loadingBarbers ? (
      <div className="flex justify-center py-10"><Spinner /></div>
    ) : (
      <div className="flex flex-col gap-3">
        {barbers.map(b => (
          <button
            key={b.id}
            onClick={() => setSelectedBarber(b)}
            className={`w-full text-left p-4 rounded-sm border transition-colors
              ${selectedBarber?.id === b.id ? 'border-accent bg-accent/5' : 'border-border hover:border-accent/50'}`}
          >
            <p className="font-body font-medium text-text-primary">{b.displayName}</p>
          </button>
        ))}
      </div>
    )}
    <Button fullWidth size="lg" disabled={!selectedBarber} onClick={() => setStep(2)}>
      Próximo
    </Button>
  </div>
)}
```

10. Shift existing step conditions: `step === 1` → `step === 2` (Date), `step === 2` → `step === 3` (Time), `step === 3` → `step === 4` (Confirm).

11. Add barber to confirmation summary card:
```tsx
<div className="flex justify-between items-center">
  <span className="text-xs font-body text-text-secondary uppercase tracking-wider">Barbeiro</span>
  <span className="font-body font-medium text-text-primary">{selectedBarber?.displayName}</span>
</div>
<div className="h-px bg-border" />
```

12. Add `User2` to lucide import.

- [ ] **Step 3: Verify in browser**

Navigate to `/book`. Confirm 5-step flow: Serviço → Barbeiro → Data → Horário → Confirmar. Summary shows service, barber, date, time, and price.

- [ ] **Step 4: Commit**

```bash
git add frontend/src/pages/client/Book.tsx frontend/src/api/appointments.ts frontend/src/types/appointment.ts
git commit -m "feat(frontend): add barber selection as step 2 in Book wizard"
```

---

**Slice 2 complete.** Three roles (admin, barber, client) are live. Admin can create and manage barbers. Clients pick a barber during booking. Barbers can log in and see their dashboard.

Continue with `2026-05-09-saas-expansion-slice3-schedule.md`.
