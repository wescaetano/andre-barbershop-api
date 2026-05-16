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
