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
