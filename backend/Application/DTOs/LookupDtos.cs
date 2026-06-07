namespace backend.Application.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class PriorityDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class TicketStatusDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
