namespace backend.DTOs
{
    public class AssignTicketDto
    {
        public int AgentUserId { get; set; }
    }

    public class UpdateTicketStatusDto
    {
        public string StatusName { get; set; } = string.Empty;
    }
}
