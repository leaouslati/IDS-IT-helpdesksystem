namespace backend.Application.DTOs
{
    public class AnalyzeTicketRequestDto
    {
        public string Description { get; set; } = string.Empty;
    }

    public class AnalyzeTicketResponseDto
    {
        public string? SuggestedCategory { get; set; }
        public string? SuggestedPriority { get; set; }
        public float CategoryConfidence { get; set; }
        public float PriorityConfidence { get; set; }
    }
}
