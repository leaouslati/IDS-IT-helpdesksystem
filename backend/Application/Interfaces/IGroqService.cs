namespace backend.Application.Interfaces
{
    public class TicketAnalysisResult
    {
        public string? SuggestedCategory { get; set; }
        public string? SuggestedPriority { get; set; }
        public float CategoryConfidence { get; set; }
        public float PriorityConfidence { get; set; }
    }

    public interface IGroqService
    {
        /// <summary>
        /// Asks the Groq LLM to suggest a category and priority for a ticket description.
        /// Never throws — API/parsing failures are logged and return null suggestions with 0 confidence.
        /// </summary>
        Task<TicketAnalysisResult> AnalyzeTicketAsync(string description);
    }
}
