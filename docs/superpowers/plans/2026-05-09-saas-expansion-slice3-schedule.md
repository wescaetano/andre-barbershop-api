# BarberAgenda — Slice 3: Working Hours, Blocks & Appointment Update

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Barbers configure their weekly working hours and block specific times. `Appointment` gains `BarberId` + `ServiceId`. `GetAvailableSlots` fully respects working hours, blocks, and service duration. Clients see only valid slots; barbers manage their own schedule.

**Prerequisite:** Slices 1 and 2 must be complete.

**Architecture:** New `WorkingHours` + `ScheduleBlock` entities → `WorkingHoursController` + `ScheduleBlockController` → update `Appointment` domain + `CreateAppointmentUseCase` → full rewrite of `GetAvailableSlotsUseCase` → frontend barber working-hours and blocks pages → update `Book.tsx` step 3 calendar.

**Tech Stack:** .NET 8, EF Core / MySQL, React + Vite + TypeScript, TanStack Query

**Spec:** `docs/superpowers/specs/2026-05-09-saas-backoffice-client-redesign.md` — Slice 3

---

## File Map

**Create (backend):**
- `src/BarberShop.Domain/WorkingHours.cs`
- `src/BarberShop.Domain/ScheduleBlock.cs`
- `src/BarberShop.Infra/Mappings/WorkingHoursMap.cs`
- `src/BarberShop.Infra/Mappings/ScheduleBlockMap.cs`
- `src/BarberShop.Communication/Models/WorkingHours/UpsertWorkingHoursModel.cs`
- `src/BarberShop.Communication/Models/ScheduleBlock/CreateScheduleBlockModel.cs`
- `src/BarberShop.Application/UseCases/WorkingHours/Upsert/IUpsertWorkingHoursUseCase.cs`
- `src/BarberShop.Application/UseCases/WorkingHours/Upsert/UpsertWorkingHoursUseCase.cs`
- `src/BarberShop.Application/UseCases/WorkingHours/GetByBarber/IGetWorkingHoursByBarberUseCase.cs`
- `src/BarberShop.Application/UseCases/WorkingHours/GetByBarber/GetWorkingHoursByBarberUseCase.cs`
- `src/BarberShop.Application/UseCases/ScheduleBlock/Create/ICreateScheduleBlockUseCase.cs`
- `src/BarberShop.Application/UseCases/ScheduleBlock/Create/CreateScheduleBlockUseCase.cs`
- `src/BarberShop.Application/UseCases/ScheduleBlock/Delete/IDeleteScheduleBlockUseCase.cs`
- `src/BarberShop.Application/UseCases/ScheduleBlock/Delete/DeleteScheduleBlockUseCase.cs`
- `src/BarberShop.Application/UseCases/ScheduleBlock/GetByBarber/IGetScheduleBlocksByBarberUseCase.cs`
- `src/BarberShop.Application/UseCases/ScheduleBlock/GetByBarber/GetScheduleBlocksByBarberUseCase.cs`
- `src/BarberShop.Api/Controllers/WorkingHoursController.cs`
- `src/BarberShop.Api/Controllers/ScheduleBlockController.cs`

**Modify (backend):**
- `src/BarberShop.Domain/Appointment.cs` — add `BarberId`, `ServiceId`
- `src/BarberShop.Infra/Mappings/AppointmentMap.cs` — add FK mappings
- `src/BarberShop.Infra/DataAccess/BarberShopContext.cs` — add new DbSets
- `src/BarberShop.Communication/Models/Appointment/CreateAppointmentModel.cs` — add fields
- `src/BarberShop.Communication/Models/Appointment/GetAvailableSlotsModel.cs` — add `BarberId`
- `src/BarberShop.Application/UseCases/Appointment/Create/CreateAppointmentUseCase.cs` — use barberId/serviceId
- `src/BarberShop.Application/UseCases/Appointment/GetAvailableSlots/GetAvailableSlotsUseCase.cs` — full rewrite
- `src/BarberShop.IOC/Injection.cs` — register new use cases

**Create (frontend):**
- `frontend/src/types/workingHours.ts`
- `frontend/src/api/workingHours.ts`
- `frontend/src/pages/barber/WorkingHours.tsx`
- `frontend/src/pages/barber/Blocks.tsx`

**Modify (frontend):**
- `frontend/src/router/index.tsx` — add barber working-hours + blocks routes

---

### Task 16: WorkingHours + ScheduleBlock entities + migration

**Files:**
- Create: `src/BarberShop.Domain/WorkingHours.cs`
- Create: `src/BarberShop.Domain/ScheduleBlock.cs`
- Create: `src/BarberShop.Infra/Mappings/WorkingHoursMap.cs`
- Create: `src/BarberShop.Infra/Mappings/ScheduleBlockMap.cs`
- Modify: `src/BarberShop.Infra/DataAccess/BarberShopContext.cs`

- [ ] **Step 1: WorkingHours entity**

```csharp
// src/BarberShop.Domain/WorkingHours.cs
namespace BarberShop.Domain
{
    public class WorkingHours : BaseEntity
    {
        public long BarberId { get; set; }
        public int DayOfWeek { get; set; }   // 0 = Sunday … 6 = Saturday
        public TimeOnly OpenTime { get; set; }
        public TimeOnly CloseTime { get; set; }
        public bool IsOpen { get; set; }

        public Barber Barber { get; set; } = null!;
    }
}
```

- [ ] **Step 2: ScheduleBlock entity**

```csharp
// src/BarberShop.Domain/ScheduleBlock.cs
namespace BarberShop.Domain
{
    public class ScheduleBlock : BaseEntity
    {
        public long BarberId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Reason { get; set; }

        public Barber Barber { get; set; } = null!;
    }
}
```

- [ ] **Step 3: WorkingHoursMap**

```csharp
// src/BarberShop.Infra/Mappings/WorkingHoursMap.cs
using BarberShop.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberShop.Infra.Mappings
{
    public class WorkingHoursMap : IEntityTypeConfiguration<WorkingHours>
    {
        public void Configure(EntityTypeBuilder<WorkingHours> builder)
        {
            builder.ToTable("WorkingHours");
            builder.HasKey(w => w.Id);
            builder.HasOne(w => w.Barber)
                .WithMany()
                .HasForeignKey(w => w.BarberId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(w => new { w.BarberId, w.DayOfWeek }).IsUnique();
        }
    }
}
```

- [ ] **Step 4: ScheduleBlockMap**

```csharp
// src/BarberShop.Infra/Mappings/ScheduleBlockMap.cs
using BarberShop.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberShop.Infra.Mappings
{
    public class ScheduleBlockMap : IEntityTypeConfiguration<ScheduleBlock>
    {
        public void Configure(EntityTypeBuilder<ScheduleBlock> builder)
        {
            builder.ToTable("ScheduleBlocks");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Reason).HasMaxLength(255);
            builder.HasOne(b => b.Barber)
                .WithMany()
                .HasForeignKey(b => b.BarberId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
```

- [ ] **Step 5: Add DbSets**

In `BarberShopContext.cs`, add:

```csharp
public DbSet<WorkingHours> WorkingHours { get; set; } = null!;
public DbSet<ScheduleBlock> ScheduleBlocks { get; set; } = null!;
```

- [ ] **Step 6: Create migration**

```bash
dotnet ef migrations add AddWorkingHoursAndScheduleBlock --project src/BarberShop.Infra --startup-project src/BarberShop.Api
dotnet ef database update --project src/BarberShop.Infra --startup-project src/BarberShop.Api
```

- [ ] **Step 7: Commit**

```bash
git add src/BarberShop.Domain/WorkingHours.cs src/BarberShop.Domain/ScheduleBlock.cs src/BarberShop.Infra/Mappings/WorkingHoursMap.cs src/BarberShop.Infra/Mappings/ScheduleBlockMap.cs src/BarberShop.Infra/DataAccess/BarberShopContext.cs src/BarberShop.Infra/Migrations/
git commit -m "feat(backend): add WorkingHours and ScheduleBlock entities with migration"
```

---

### Task 17: Update Appointment entity + migration

**Files:**
- Modify: `src/BarberShop.Domain/Appointment.cs`
- Modify: `src/BarberShop.Infra/Mappings/AppointmentMap.cs`
- Modify: `src/BarberShop.Communication/Models/Appointment/CreateAppointmentModel.cs`

- [ ] **Step 1: Update Appointment domain**

```csharp
// src/BarberShop.Domain/Appointment.cs
using BarberShop.Communication.Enums.Appointment;

namespace BarberShop.Domain
{
    public class Appointment : BaseEntity
    {
        public long UserId { get; set; }
        public long? BarberId { get; set; }   // nullable for backward compat
        public long? ServiceId { get; set; }  // nullable for backward compat
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public EAppointmentStatus Status { get; set; } = EAppointmentStatus.WaitingPayment;

        public User User { get; set; } = null!;
        public Barber? Barber { get; set; }
        public Service? Service { get; set; }
        public Payment? Payment { get; set; }
    }
}
```

- [ ] **Step 2: Update AppointmentMap**

```csharp
// src/BarberShop.Infra/Mappings/AppointmentMap.cs
using BarberShop.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberShop.Infra.Mappings
{
    public class AppointmentMap : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.ToTable("Appointments");
            builder.HasKey(a => a.Id);

            builder.HasOne(a => a.User)
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Barber)
                .WithMany()
                .HasForeignKey(a => a.BarberId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.HasOne(a => a.Service)
                .WithMany()
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.HasOne(a => a.Payment)
                .WithOne(p => p.Appointment)
                .HasForeignKey<Payment>(p => p.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
```

- [ ] **Step 3: Update CreateAppointmentModel**

```csharp
// src/BarberShop.Communication/Models/Appointment/CreateAppointmentModel.cs
namespace BarberShop.Communication.Models.Appointment
{
    public class CreateAppointmentModel
    {
        public long UserId { get; set; }
        public long BarberId { get; set; }
        public long ServiceId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
    }
}
```

- [ ] **Step 4: Create and apply migration**

```bash
dotnet ef migrations add AddBarberIdAndServiceIdToAppointment --project src/BarberShop.Infra --startup-project src/BarberShop.Api
dotnet ef database update --project src/BarberShop.Infra --startup-project src/BarberShop.Api
```

- [ ] **Step 5: Commit**

```bash
git add src/BarberShop.Domain/Appointment.cs src/BarberShop.Infra/Mappings/AppointmentMap.cs src/BarberShop.Communication/Models/Appointment/CreateAppointmentModel.cs src/BarberShop.Infra/Migrations/
git commit -m "feat(backend): add BarberId and ServiceId to Appointment entity"
```

---

### Task 18: Update CreateAppointmentUseCase + rewrite GetAvailableSlotsUseCase

**Files:**
- Modify: `src/BarberShop.Application/UseCases/Appointment/Create/CreateAppointmentUseCase.cs`
- Modify: `src/BarberShop.Communication/Models/Appointment/GetAvailableSlotsModel.cs`
- Modify: `src/BarberShop.Application/UseCases/Appointment/GetAvailableSlots/GetAvailableSlotsUseCase.cs`
- Modify: `src/BarberShop.Api/Controllers/AppointmentController.cs`

- [ ] **Step 1: Update CreateAppointmentUseCase**

```csharp
// src/BarberShop.Application/UseCases/Appointment/Create/CreateAppointmentUseCase.cs
using BarberShop.Communication.Enums.Appointment;
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Appointment;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Appointment.Create
{
    public class CreateAppointmentUseCase : ICreateAppointmentUseCase
    {
        private readonly IBaseRepository<Domain.Appointment> _appointmentRepository;
        private readonly IBaseRepository<Domain.Service> _serviceRepository;

        public CreateAppointmentUseCase(
            IBaseRepository<Domain.Appointment> appointmentRepository,
            IBaseRepository<Domain.Service> serviceRepository)
        {
            _appointmentRepository = appointmentRepository;
            _serviceRepository = serviceRepository;
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(CreateAppointmentModel model)
        {
            var service = await _serviceRepository.Get(model.ServiceId);
            if (service == null)
                return FactoryResponse<dynamic>.NotFound("Serviço não encontrado.");

            var startTime = model.Date.ToDateTime(model.StartTime);
            var endTime = startTime.AddMinutes(service.DurationMinutes);

            var slotOccupied = await _appointmentRepository.Get(
                a => a.BarberId == model.BarberId
                  && a.StartTime < endTime
                  && a.EndTime > startTime
                  && a.Status != EAppointmentStatus.Cancelled);

            if (slotOccupied != null)
                return FactoryResponse<dynamic>.Conflict("Este horário já está ocupado.");

            var appointment = new Domain.Appointment
            {
                UserId = model.UserId,
                BarberId = model.BarberId,
                ServiceId = model.ServiceId,
                StartTime = startTime,
                EndTime = endTime,
                Status = EAppointmentStatus.WaitingPayment
            };
            appointment.AddCreationDate();

            try
            {
                await _appointmentRepository.Create(appointment);
                return FactoryResponse<dynamic>.SuccessfulCreation(new
                {
                    appointment.Id,
                    appointment.StartTime,
                    appointment.EndTime,
                    Status = appointment.Status.ToString()
                });
            }
            catch (Exception e)
            {
                return FactoryResponse<dynamic>.BadRequestErroInterno(e.Message);
            }
        }
    }
}
```

- [ ] **Step 2: Update GetAvailableSlotsModel to include BarberId**

```csharp
// src/BarberShop.Communication/Models/Appointment/GetAvailableSlotsModel.cs
namespace BarberShop.Communication.Models.Appointment
{
    public class GetAvailableSlotsModel
    {
        public DateOnly Date { get; set; }
        public long BarberId { get; set; }
        public long ServiceId { get; set; }
    }
}
```

- [ ] **Step 3: Full rewrite of GetAvailableSlotsUseCase**

```csharp
// src/BarberShop.Application/UseCases/Appointment/GetAvailableSlots/GetAvailableSlotsUseCase.cs
using BarberShop.Communication.Enums.Appointment;
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.Appointment;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.Appointment.GetAvailableSlots
{
    public class GetAvailableSlotsUseCase : IGetAvailableSlotsUseCase
    {
        private readonly IBaseRepository<Domain.Appointment> _appointmentRepo;
        private readonly IBaseRepository<Domain.Service> _serviceRepo;
        private readonly IBaseRepository<Domain.WorkingHours> _workingHoursRepo;
        private readonly IBaseRepository<Domain.ScheduleBlock> _blockRepo;

        public GetAvailableSlotsUseCase(
            IBaseRepository<Domain.Appointment> appointmentRepo,
            IBaseRepository<Domain.Service> serviceRepo,
            IBaseRepository<Domain.WorkingHours> workingHoursRepo,
            IBaseRepository<Domain.ScheduleBlock> blockRepo)
        {
            _appointmentRepo = appointmentRepo;
            _serviceRepo = serviceRepo;
            _workingHoursRepo = workingHoursRepo;
            _blockRepo = blockRepo;
        }

        public async Task<ResponseModel<dynamic>> ExecuteAsync(GetAvailableSlotsModel model)
        {
            var service = await _serviceRepo.Get(model.ServiceId);
            if (service == null)
                return FactoryResponse<dynamic>.NotFound("Serviço não encontrado.");

            var dayOfWeek = (int)model.Date.DayOfWeek;
            var workingHours = await _workingHoursRepo.Get(
                wh => wh.BarberId == model.BarberId && wh.DayOfWeek == dayOfWeek);

            if (workingHours == null || !workingHours.IsOpen)
                return FactoryResponse<dynamic>.Success(new List<string>());

            var allSlots = GenerateSlots(workingHours.OpenTime, workingHours.CloseTime, service.DurationMinutes);

            var dayStart = model.Date.ToDateTime(TimeOnly.MinValue);
            var dayEnd = model.Date.ToDateTime(TimeOnly.MaxValue);

            var appointments = await _appointmentRepo.GetAll(
                a => a.BarberId == model.BarberId
                  && a.StartTime >= dayStart
                  && a.StartTime < dayEnd
                  && a.Status != EAppointmentStatus.Cancelled);

            var blocks = await _blockRepo.GetAll(
                b => b.BarberId == model.BarberId
                  && b.EndTime > dayStart
                  && b.StartTime < dayEnd);

            var now = DateTime.UtcNow;
            var isToday = model.Date == DateOnly.FromDateTime(now);

            var available = allSlots.Where(slot =>
            {
                var slotStart = model.Date.ToDateTime(slot);
                var slotEnd = slotStart.AddMinutes(service.DurationMinutes);

                if (isToday && slotStart <= now) return false;

                bool hasAppointment = appointments.Any(a =>
                    a.StartTime < slotEnd && a.EndTime > slotStart);

                bool hasBlock = blocks.Any(b =>
                    b.StartTime < slotEnd && b.EndTime > slotStart);

                return !hasAppointment && !hasBlock;
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

- [ ] **Step 4: Update AppointmentController.GetAvailableSlots**

```csharp
[AllowAnonymous]
[HttpGet("available-slots")]
public async Task<IActionResult> GetAvailableSlots(
    [FromQuery] DateOnly date,
    [FromQuery] long barberId,
    [FromQuery] long serviceId)
{
    var result = await _getAvailableSlotsUseCase.ExecuteAsync(
        new GetAvailableSlotsModel { Date = date, BarberId = barberId, ServiceId = serviceId });
    return Result(result);
}
```

- [ ] **Step 5: Build**

```bash
dotnet build src/BarberShop.Api/BarberShop.Api.csproj
```

Expected: 0 errors.

- [ ] **Step 6: Commit**

```bash
git add src/BarberShop.Application/UseCases/Appointment/ src/BarberShop.Communication/Models/Appointment/ src/BarberShop.Api/Controllers/AppointmentController.cs
git commit -m "feat(backend): update CreateAppointment and fully rewrite GetAvailableSlots with working hours and blocks"
```

---

### Task 19: WorkingHours use cases + controller

**Files:**
- Create all listed WorkingHours use case files
- Create: `src/BarberShop.Api/Controllers/WorkingHoursController.cs`
- Create: `src/BarberShop.Communication/Models/WorkingHours/UpsertWorkingHoursModel.cs`

- [ ] **Step 1: Model**

```csharp
// UpsertWorkingHoursModel.cs
namespace BarberShop.Communication.Models.WorkingHours
{
    public class WorkingHoursDayModel
    {
        public int DayOfWeek { get; set; }
        public bool IsOpen { get; set; }
        public TimeOnly OpenTime { get; set; }
        public TimeOnly CloseTime { get; set; }
    }

    public class UpsertWorkingHoursModel
    {
        public long BarberId { get; set; }
        public List<WorkingHoursDayModel> Days { get; set; } = new();
    }
}
```

- [ ] **Step 2: GetByBarber use case**

```csharp
// IGetWorkingHoursByBarberUseCase.cs
using BarberShop.Communication.Models;
namespace BarberShop.Application.UseCases.WorkingHours.GetByBarber
{
    public interface IGetWorkingHoursByBarberUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(long barberId);
    }
}
```

```csharp
// GetWorkingHoursByBarberUseCase.cs
using BarberShop.Communication.Models;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.WorkingHours.GetByBarber
{
    public class GetWorkingHoursByBarberUseCase : IGetWorkingHoursByBarberUseCase
    {
        private readonly IBaseRepository<Domain.WorkingHours> _repo;
        public GetWorkingHoursByBarberUseCase(IBaseRepository<Domain.WorkingHours> repo) => _repo = repo;

        public async Task<ResponseModel<dynamic>> ExecuteAsync(long barberId)
        {
            var wh = await _repo.GetAll(w => w.BarberId == barberId);
            var result = wh.OrderBy(w => w.DayOfWeek).Select(w => new
            {
                w.Id, w.DayOfWeek, w.IsOpen,
                OpenTime = w.OpenTime.ToString("HH:mm"),
                CloseTime = w.CloseTime.ToString("HH:mm")
            }).ToList();
            return FactoryResponse<dynamic>.Success(result);
        }
    }
}
```

- [ ] **Step 3: Upsert use case**

```csharp
// IUpsertWorkingHoursUseCase.cs
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.WorkingHours;
namespace BarberShop.Application.UseCases.WorkingHours.Upsert
{
    public interface IUpsertWorkingHoursUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(UpsertWorkingHoursModel model);
    }
}
```

```csharp
// UpsertWorkingHoursUseCase.cs
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.WorkingHours;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.WorkingHours.Upsert
{
    public class UpsertWorkingHoursUseCase : IUpsertWorkingHoursUseCase
    {
        private readonly IBaseRepository<Domain.WorkingHours> _repo;
        public UpsertWorkingHoursUseCase(IBaseRepository<Domain.WorkingHours> repo) => _repo = repo;

        public async Task<ResponseModel<dynamic>> ExecuteAsync(UpsertWorkingHoursModel model)
        {
            var existing = await _repo.GetAll(w => w.BarberId == model.BarberId);
            var existingByDay = existing.ToDictionary(w => w.DayOfWeek);

            var toCreate = new List<Domain.WorkingHours>();
            var toUpdate = new List<Domain.WorkingHours>();

            foreach (var day in model.Days)
            {
                if (existingByDay.TryGetValue(day.DayOfWeek, out var record))
                {
                    record.IsOpen = day.IsOpen;
                    record.OpenTime = day.OpenTime;
                    record.CloseTime = day.CloseTime;
                    record.AddUpdateDate();
                    toUpdate.Add(record);
                }
                else
                {
                    var newRecord = new Domain.WorkingHours
                    {
                        BarberId = model.BarberId,
                        DayOfWeek = day.DayOfWeek,
                        IsOpen = day.IsOpen,
                        OpenTime = day.OpenTime,
                        CloseTime = day.CloseTime
                    };
                    newRecord.AddCreationDate();
                    toCreate.Add(newRecord);
                }
            }

            if (toUpdate.Any()) await _repo.UpdateRange(toUpdate);
            if (toCreate.Any()) await _repo.Create(toCreate);

            return FactoryResponse<dynamic>.Success(null);
        }
    }
}
```

- [ ] **Step 4: Create WorkingHoursController**

```csharp
// src/BarberShop.Api/Controllers/WorkingHoursController.cs
using BarberShop.Api.Authorization;
using BarberShop.Application.UseCases.WorkingHours.GetByBarber;
using BarberShop.Application.UseCases.WorkingHours.Upsert;
using BarberShop.Communication.Models.WorkingHours;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Controllers
{
    /// <summary>Horários de funcionamento por barbeiro</summary>
    [APIAuthorization("Barber-V", "Barber-E", "Users-V", "Users-E")]
    public class WorkingHoursController : BaseController
    {
        private readonly IGetWorkingHoursByBarberUseCase _get;
        private readonly IUpsertWorkingHoursUseCase _upsert;

        public WorkingHoursController(IGetWorkingHoursByBarberUseCase get, IUpsertWorkingHoursUseCase upsert)
        {
            _get = get;
            _upsert = upsert;
        }

        /// <summary>Retorna horários de funcionamento de um barbeiro (público)</summary>
        [AllowAnonymous]
        [HttpGet("{barberId:long}")]
        public async Task<IActionResult> GetByBarber([FromRoute] long barberId)
            => Result(await _get.ExecuteAsync(barberId));

        /// <summary>Salva horários de funcionamento (cria ou atualiza)</summary>
        [HttpPut]
        public async Task<IActionResult> Upsert([FromBody] UpsertWorkingHoursModel model)
            => Result(await _upsert.ExecuteAsync(model));
    }
}
```

- [ ] **Step 5: Register in IOC**

Add usings to `Injection.cs`:
```csharp
using BarberShop.Application.UseCases.WorkingHours.GetByBarber;
using BarberShop.Application.UseCases.WorkingHours.Upsert;
```

Add registrations:
```csharp
// WorkingHours use cases
services.AddScoped<IGetWorkingHoursByBarberUseCase, GetWorkingHoursByBarberUseCase>();
services.AddScoped<IUpsertWorkingHoursUseCase, UpsertWorkingHoursUseCase>();
```

- [ ] **Step 6: Build and commit**

```bash
dotnet build src/BarberShop.Api/BarberShop.Api.csproj
git add src/BarberShop.Communication/Models/WorkingHours/ src/BarberShop.Application/UseCases/WorkingHours/ src/BarberShop.Api/Controllers/WorkingHoursController.cs src/BarberShop.IOC/Injection.cs
git commit -m "feat(backend): add WorkingHours use cases and controller"
```

---

### Task 20: ScheduleBlock use cases + controller

**Files:**
- Create all listed ScheduleBlock use case files
- Create: `src/BarberShop.Api/Controllers/ScheduleBlockController.cs`
- Create: `src/BarberShop.Communication/Models/ScheduleBlock/CreateScheduleBlockModel.cs`

- [ ] **Step 1: Model**

```csharp
// CreateScheduleBlockModel.cs
namespace BarberShop.Communication.Models.ScheduleBlock
{
    public class CreateScheduleBlockModel
    {
        public long BarberId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Reason { get; set; }
    }
}
```

- [ ] **Step 2: GetByBarber use case**

```csharp
// IGetScheduleBlocksByBarberUseCase.cs
using BarberShop.Communication.Models;
namespace BarberShop.Application.UseCases.ScheduleBlock.GetByBarber
{
    public interface IGetScheduleBlocksByBarberUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(long barberId, DateTime from, DateTime to);
    }
}
```

```csharp
// GetScheduleBlocksByBarberUseCase.cs
using BarberShop.Communication.Models;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.ScheduleBlock.GetByBarber
{
    public class GetScheduleBlocksByBarberUseCase : IGetScheduleBlocksByBarberUseCase
    {
        private readonly IBaseRepository<Domain.ScheduleBlock> _repo;
        public GetScheduleBlocksByBarberUseCase(IBaseRepository<Domain.ScheduleBlock> repo) => _repo = repo;

        public async Task<ResponseModel<dynamic>> ExecuteAsync(long barberId, DateTime from, DateTime to)
        {
            var blocks = await _repo.GetAll(
                b => b.BarberId == barberId && b.EndTime > from && b.StartTime < to);

            var result = blocks.OrderBy(b => b.StartTime).Select(b => new
            {
                b.Id, b.BarberId, b.StartTime, b.EndTime, b.Reason
            }).ToList();

            return FactoryResponse<dynamic>.Success(result);
        }
    }
}
```

- [ ] **Step 3: Create + Delete use cases**

```csharp
// ICreateScheduleBlockUseCase.cs
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.ScheduleBlock;
namespace BarberShop.Application.UseCases.ScheduleBlock.Create
{
    public interface ICreateScheduleBlockUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(CreateScheduleBlockModel model);
    }
}
```

```csharp
// CreateScheduleBlockUseCase.cs
using BarberShop.Communication.Models;
using BarberShop.Communication.Models.ScheduleBlock;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.ScheduleBlock.Create
{
    public class CreateScheduleBlockUseCase : ICreateScheduleBlockUseCase
    {
        private readonly IBaseRepository<Domain.ScheduleBlock> _repo;
        public CreateScheduleBlockUseCase(IBaseRepository<Domain.ScheduleBlock> repo) => _repo = repo;

        public async Task<ResponseModel<dynamic>> ExecuteAsync(CreateScheduleBlockModel model)
        {
            if (model.EndTime <= model.StartTime)
                return FactoryResponse<dynamic>.InvalidModel("Horário de término deve ser após o início.");

            var block = new Domain.ScheduleBlock
            {
                BarberId = model.BarberId,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                Reason = model.Reason
            };
            block.AddCreationDate();
            await _repo.Create(block);
            return FactoryResponse<dynamic>.SuccessfulCreation(new { block.Id, block.StartTime, block.EndTime });
        }
    }
}
```

```csharp
// IDeleteScheduleBlockUseCase.cs
using BarberShop.Communication.Models;
namespace BarberShop.Application.UseCases.ScheduleBlock.Delete
{
    public interface IDeleteScheduleBlockUseCase
    {
        Task<ResponseModel<dynamic>> ExecuteAsync(long id);
    }
}
```

```csharp
// DeleteScheduleBlockUseCase.cs
using BarberShop.Communication.Models;
using BarberShop.Infra.Interfaces;

namespace BarberShop.Application.UseCases.ScheduleBlock.Delete
{
    public class DeleteScheduleBlockUseCase : IDeleteScheduleBlockUseCase
    {
        private readonly IBaseRepository<Domain.ScheduleBlock> _repo;
        public DeleteScheduleBlockUseCase(IBaseRepository<Domain.ScheduleBlock> repo) => _repo = repo;

        public async Task<ResponseModel<dynamic>> ExecuteAsync(long id)
        {
            var block = await _repo.Get(id);
            if (block == null)
                return FactoryResponse<dynamic>.NotFound("Bloqueio não encontrado.");

            await _repo.Remove(block);
            return FactoryResponse<dynamic>.Success(null);
        }
    }
}
```

- [ ] **Step 4: Create ScheduleBlockController**

```csharp
// src/BarberShop.Api/Controllers/ScheduleBlockController.cs
using BarberShop.Api.Authorization;
using BarberShop.Application.UseCases.ScheduleBlock.Create;
using BarberShop.Application.UseCases.ScheduleBlock.Delete;
using BarberShop.Application.UseCases.ScheduleBlock.GetByBarber;
using BarberShop.Communication.Models.ScheduleBlock;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Api.Controllers
{
    /// <summary>Bloqueios de agenda por barbeiro</summary>
    [APIAuthorization("Barber-V", "Barber-E", "Barber-C", "Users-V", "Users-E")]
    public class ScheduleBlockController : BaseController
    {
        private readonly IGetScheduleBlocksByBarberUseCase _get;
        private readonly ICreateScheduleBlockUseCase _create;
        private readonly IDeleteScheduleBlockUseCase _delete;

        public ScheduleBlockController(
            IGetScheduleBlocksByBarberUseCase get,
            ICreateScheduleBlockUseCase create,
            IDeleteScheduleBlockUseCase delete)
        {
            _get = get;
            _create = create;
            _delete = delete;
        }

        /// <summary>Lista bloqueios de um barbeiro em um intervalo</summary>
        [HttpGet("{barberId:long}")]
        public async Task<IActionResult> GetByBarber(
            [FromRoute] long barberId,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
            => Result(await _get.ExecuteAsync(barberId, from, to));

        /// <summary>Cria um bloqueio de horário</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateScheduleBlockModel model)
            => Result(await _create.ExecuteAsync(model));

        /// <summary>Remove um bloqueio</summary>
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete([FromRoute] long id)
            => Result(await _delete.ExecuteAsync(id));
    }
}
```

- [ ] **Step 5: Register in IOC**

Add usings:
```csharp
using BarberShop.Application.UseCases.ScheduleBlock.Create;
using BarberShop.Application.UseCases.ScheduleBlock.Delete;
using BarberShop.Application.UseCases.ScheduleBlock.GetByBarber;
```

Add registrations:
```csharp
// ScheduleBlock use cases
services.AddScoped<IGetScheduleBlocksByBarberUseCase, GetScheduleBlocksByBarberUseCase>();
services.AddScoped<ICreateScheduleBlockUseCase, CreateScheduleBlockUseCase>();
services.AddScoped<IDeleteScheduleBlockUseCase, DeleteScheduleBlockUseCase>();
```

- [ ] **Step 6: Build and commit**

```bash
dotnet build src/BarberShop.Api/BarberShop.Api.csproj
git add src/BarberShop.Communication/Models/ScheduleBlock/ src/BarberShop.Application/UseCases/ScheduleBlock/ src/BarberShop.Api/Controllers/ScheduleBlockController.cs src/BarberShop.IOC/Injection.cs
git commit -m "feat(backend): add ScheduleBlock use cases and controller"
```

---

### Task 21: Frontend — WorkingHours page (barber panel)

**Files:**
- Create: `frontend/src/types/workingHours.ts`
- Create: `frontend/src/api/workingHours.ts`
- Create: `frontend/src/pages/barber/WorkingHours.tsx`
- Modify: `frontend/src/router/index.tsx`

- [ ] **Step 1: Types and API**

```ts
// frontend/src/types/workingHours.ts
export interface WorkingHoursDay {
  id?: number
  dayOfWeek: number  // 0 = Sunday
  isOpen: boolean
  openTime: string   // "HH:mm"
  closeTime: string  // "HH:mm"
}
```

```ts
// frontend/src/api/workingHours.ts
import { apiClient } from './client'
import type { ApiResponse } from '../types/auth'
import type { WorkingHoursDay } from '../types/workingHours'

export const workingHoursApi = {
  getByBarber: async (barberId: number): Promise<WorkingHoursDay[]> => {
    const { data } = await apiClient.get<ApiResponse<WorkingHoursDay[]>>(`/workinghours/${barberId}`)
    return data.data ?? []
  },

  upsert: async (barberId: number, days: WorkingHoursDay[]) => {
    const { data } = await apiClient.put<ApiResponse<null>>('/workinghours', { barberId, days })
    if (!data.success) throw new Error(data.responseLabel)
    return data
  },
}
```

- [ ] **Step 2: Create WorkingHours page**

```tsx
// frontend/src/pages/barber/WorkingHours.tsx
import { useEffect, useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { workingHoursApi } from '../../api/workingHours'
import { Button } from '../../components/ui/Button'
import { Spinner } from '../../components/ui/Spinner'
import { useToast } from '../../components/ui/Toast'
import { useAuthStore } from '../../store/authStore'
import type { WorkingHoursDay } from '../../types/workingHours'

const DAY_NAMES = ['Domingo', 'Segunda', 'Terça', 'Quarta', 'Quinta', 'Sexta', 'Sábado']

const DEFAULT_DAYS: WorkingHoursDay[] = Array.from({ length: 7 }, (_, i) => ({
  dayOfWeek: i,
  isOpen: i >= 1 && i <= 6,
  openTime: '09:00',
  closeTime: '18:00',
}))

export default function WorkingHoursPage() {
  const toast = useToast()
  const qc = useQueryClient()
  const userId = useAuthStore((s) => s.userId)
  const [barberId, setBarberId] = useState<number | null>(null)
  const [days, setDays] = useState<WorkingHoursDay[]>(DEFAULT_DAYS)

  // The barber's own ID is fetched via a barber endpoint.
  // For now we use userId as a proxy until a /barber/me endpoint exists.
  // TODO: replace with GET /barber/me in a future task.
  useEffect(() => {
    if (userId) setBarberId(userId)
  }, [userId])

  const { isLoading } = useQuery({
    queryKey: ['working-hours', barberId],
    queryFn: () => workingHoursApi.getByBarber(barberId!),
    enabled: !!barberId,
    onSuccess: (data) => {
      if (data.length > 0) {
        const merged = DEFAULT_DAYS.map(def => {
          const found = data.find(d => d.dayOfWeek === def.dayOfWeek)
          return found ?? def
        })
        setDays(merged)
      }
    },
  })

  const { mutate: save, isPending: saving } = useMutation({
    mutationFn: () => workingHoursApi.upsert(barberId!, days),
    onSuccess: () => {
      toast('Horários salvos!', 'success')
      qc.invalidateQueries({ queryKey: ['working-hours', barberId] })
    },
    onError: () => toast('Erro ao salvar horários.', 'error'),
  })

  function updateDay(index: number, patch: Partial<WorkingHoursDay>) {
    setDays(prev => prev.map((d, i) => i === index ? { ...d, ...patch } : d))
  }

  return (
    <div className="flex flex-col gap-6">
      <div>
        <p className="text-xs font-display uppercase tracking-[0.2em] text-accent mb-1">BarberAgenda</p>
        <h1 className="font-display font-extrabold text-4xl uppercase leading-none">Meus Horários</h1>
        <p className="text-text-secondary text-sm font-body mt-2">Configure os dias e horários em que você atende.</p>
      </div>

      {isLoading ? (
        <div className="flex justify-center py-10"><Spinner /></div>
      ) : (
        <div className="flex flex-col gap-2">
          {days.map((day, i) => (
            <div key={day.dayOfWeek} className="bg-bg-surface border border-border rounded-sm px-4 py-3 flex items-center gap-4">
              <div className="w-24 flex-shrink-0">
                <p className="text-sm font-body font-medium text-text-primary">{DAY_NAMES[day.dayOfWeek]}</p>
              </div>
              <button
                onClick={() => updateDay(i, { isOpen: !day.isOpen })}
                className={`flex-shrink-0 w-10 h-5 rounded-full transition-colors relative ${day.isOpen ? 'bg-accent' : 'bg-border'}`}
              >
                <span className={`absolute top-0.5 w-4 h-4 rounded-full bg-white transition-transform ${day.isOpen ? 'left-5' : 'left-0.5'}`} />
              </button>
              {day.isOpen ? (
                <div className="flex items-center gap-2 flex-1">
                  <input
                    type="time"
                    value={day.openTime}
                    onChange={e => updateDay(i, { openTime: e.target.value })}
                    className="bg-bg-elevated border border-border rounded-sm px-2 py-1 text-sm font-body text-text-primary"
                  />
                  <span className="text-text-secondary text-sm">→</span>
                  <input
                    type="time"
                    value={day.closeTime}
                    onChange={e => updateDay(i, { closeTime: e.target.value })}
                    className="bg-bg-elevated border border-border rounded-sm px-2 py-1 text-sm font-body text-text-primary"
                  />
                </div>
              ) : (
                <p className="text-sm font-body text-text-secondary flex-1">Fechado</p>
              )}
            </div>
          ))}
          <div className="pt-2">
            <Button loading={saving} onClick={() => save()}>Salvar Horários</Button>
          </div>
        </div>
      )}
    </div>
  )
}
```

- [ ] **Step 3: Add route**

In `router/index.tsx`, add import:
```ts
import BarberWorkingHours from '../pages/barber/WorkingHours'
```

Add to barber children:
```ts
{ path: 'working-hours', element: <BarberWorkingHours /> },
```

- [ ] **Step 4: Commit**

```bash
git add frontend/src/types/workingHours.ts frontend/src/api/workingHours.ts frontend/src/pages/barber/WorkingHours.tsx frontend/src/router/index.tsx
git commit -m "feat(frontend): add barber WorkingHours configuration page"
```

---

### Task 22: Frontend — Schedule Blocks page (barber panel)

**Files:**
- Create: `frontend/src/pages/barber/Blocks.tsx`
- Modify: `frontend/src/router/index.tsx`

- [ ] **Step 1: Create Blocks page**

```tsx
// frontend/src/pages/barber/Blocks.tsx
import { useEffect, useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Plus, Trash2 } from 'lucide-react'
import { apiClient } from '../../api/client'
import { Button } from '../../components/ui/Button'
import { Modal } from '../../components/ui/Modal'
import { Spinner } from '../../components/ui/Spinner'
import { useToast } from '../../components/ui/Toast'
import { useAuthStore } from '../../store/authStore'
import type { ApiResponse } from '../../types/auth'

interface ScheduleBlock {
  id: number
  barberId: number
  startTime: string
  endTime: string
  reason?: string
}

interface BlockForm { date: string; startTime: string; endTime: string; reason: string }
const emptyForm: BlockForm = { date: '', startTime: '', endTime: '', reason: '' }

export default function Blocks() {
  const toast = useToast()
  const qc = useQueryClient()
  const userId = useAuthStore((s) => s.userId)
  const [barberId, setBarberId] = useState<number | null>(null)
  const [modalOpen, setModalOpen] = useState(false)
  const [form, setForm] = useState<BlockForm>(emptyForm)

  useEffect(() => { if (userId) setBarberId(userId) }, [userId])

  const now = new Date()
  const from = now.toISOString()
  const to = new Date(now.getFullYear(), now.getMonth() + 3, 1).toISOString()

  const { data: blocks = [], isLoading } = useQuery({
    queryKey: ['blocks', barberId],
    queryFn: async () => {
      const { data } = await apiClient.get<ApiResponse<ScheduleBlock[]>>(
        `/scheduleblock/${barberId}`, { params: { from, to } })
      return data.data ?? []
    },
    enabled: !!barberId,
  })

  const { mutate: create, isPending: creating } = useMutation({
    mutationFn: () => {
      const start = new Date(`${form.date}T${form.startTime}`)
      const end = new Date(`${form.date}T${form.endTime}`)
      return apiClient.post('/scheduleblock', {
        barberId,
        startTime: start.toISOString(),
        endTime: end.toISOString(),
        reason: form.reason || undefined,
      })
    },
    onSuccess: () => {
      toast('Bloqueio criado!', 'success')
      qc.invalidateQueries({ queryKey: ['blocks', barberId] })
      setModalOpen(false)
      setForm(emptyForm)
    },
    onError: () => toast('Erro ao criar bloqueio.', 'error'),
  })

  const { mutate: remove } = useMutation({
    mutationFn: (id: number) => apiClient.delete(`/scheduleblock/${id}`),
    onSuccess: () => {
      toast('Bloqueio removido!', 'success')
      qc.invalidateQueries({ queryKey: ['blocks', barberId] })
    },
    onError: () => toast('Erro ao remover bloqueio.', 'error'),
  })

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-xs font-display uppercase tracking-[0.2em] text-accent mb-1">BarberAgenda</p>
          <h1 className="font-display font-extrabold text-4xl uppercase leading-none">Bloqueios</h1>
        </div>
        <Button onClick={() => { setForm(emptyForm); setModalOpen(true) }}>
          <Plus size={16} className="mr-1" />Bloquear Horário
        </Button>
      </div>

      {isLoading ? (
        <div className="flex justify-center py-10"><Spinner /></div>
      ) : (
        <div className="bg-bg-surface border border-border rounded-sm overflow-hidden">
          {blocks.length === 0 && (
            <p className="text-text-secondary text-sm font-body p-5">Nenhum bloqueio agendado.</p>
          )}
          {blocks.map((b, i) => {
            const start = new Date(b.startTime)
            const end = new Date(b.endTime)
            return (
              <div key={b.id} className={`flex items-center justify-between px-4 py-3 gap-4 ${i !== blocks.length - 1 ? 'border-b border-border' : ''}`}>
                <div className="flex-1">
                  <p className="font-body font-medium text-text-primary">
                    {start.toLocaleDateString('pt-BR')} · {start.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })} → {end.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}
                  </p>
                  {b.reason && <p className="text-xs text-text-secondary font-body">{b.reason}</p>}
                </div>
                <button onClick={() => remove(b.id)} className="text-text-secondary hover:text-red-400">
                  <Trash2 size={16} />
                </button>
              </div>
            )
          })}
        </div>
      )}

      <Modal open={modalOpen} onClose={() => setModalOpen(false)} title="Bloquear Horário">
        <div className="flex flex-col gap-4">
          <div className="flex flex-col gap-1">
            <label className="text-xs font-body text-text-secondary uppercase tracking-wider">Data</label>
            <input
              type="date"
              value={form.date}
              onChange={e => setForm(f => ({ ...f, date: e.target.value }))}
              className="bg-bg-elevated border border-border rounded-sm px-3 py-2 text-sm font-body text-text-primary"
            />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="flex flex-col gap-1">
              <label className="text-xs font-body text-text-secondary uppercase tracking-wider">Início</label>
              <input
                type="time"
                value={form.startTime}
                onChange={e => setForm(f => ({ ...f, startTime: e.target.value }))}
                className="bg-bg-elevated border border-border rounded-sm px-3 py-2 text-sm font-body text-text-primary"
              />
            </div>
            <div className="flex flex-col gap-1">
              <label className="text-xs font-body text-text-secondary uppercase tracking-wider">Fim</label>
              <input
                type="time"
                value={form.endTime}
                onChange={e => setForm(f => ({ ...f, endTime: e.target.value }))}
                className="bg-bg-elevated border border-border rounded-sm px-3 py-2 text-sm font-body text-text-primary"
              />
            </div>
          </div>
          <div className="flex flex-col gap-1">
            <label className="text-xs font-body text-text-secondary uppercase tracking-wider">Motivo (opcional)</label>
            <input
              value={form.reason}
              onChange={e => setForm(f => ({ ...f, reason: e.target.value }))}
              placeholder="Ex: Folga, reunião..."
              className="bg-bg-elevated border border-border rounded-sm px-3 py-2 text-sm font-body text-text-primary"
            />
          </div>
          <Button fullWidth loading={creating} disabled={!form.date || !form.startTime || !form.endTime} onClick={() => create()}>
            Salvar Bloqueio
          </Button>
        </div>
      </Modal>
    </div>
  )
}
```

- [ ] **Step 2: Add route**

In `router/index.tsx`, add import:
```ts
import BarberBlocks from '../pages/barber/Blocks'
```

Add to barber children:
```ts
{ path: 'blocks', element: <BarberBlocks /> },
```

- [ ] **Step 3: Commit**

```bash
git add frontend/src/pages/barber/Blocks.tsx frontend/src/router/index.tsx
git commit -m "feat(frontend): add barber Schedule Blocks page"
```

---

**Slice 3 complete.** Working hours and blocks are fully wired. `GetAvailableSlots` now respects each barber's schedule. Clients only see real availability.

Continue with `2026-05-09-saas-expansion-slice4-agenda.md`.
