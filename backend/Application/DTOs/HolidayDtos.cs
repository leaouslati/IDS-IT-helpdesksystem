namespace backend.Application.DTOs
{
    public class HolidayDto
    {
        public int      Id          { get; set; }
        public string   Name        { get; set; } = string.Empty;
        public DateTime Date        { get; set; }
        public bool     IsRecurring { get; set; }
    }

    public class CreateHolidayDto
    {
        public string   Name        { get; set; } = string.Empty;
        public DateTime Date        { get; set; }
        public bool     IsRecurring { get; set; } = false;
    }

    public class UpdateHolidayDto
    {
        public string   Name        { get; set; } = string.Empty;
        public DateTime Date        { get; set; }
        public bool     IsRecurring { get; set; } = false;
    }
}
