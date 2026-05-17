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
