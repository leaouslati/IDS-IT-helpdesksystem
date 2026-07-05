namespace backend.Application.DTOs
{
    public class SystemInfoDto
    {
        public int      AdminCount       { get; set; }
        public int      ManagerCount     { get; set; }
        public int      AgentCount       { get; set; }
        public int      EmployeeCount    { get; set; }
        public int      TotalUsers       { get; set; }
        public int      TotalTickets     { get; set; }
        public long     StorageUsedBytes { get; set; }
        public string   AppVersion       { get; set; } = string.Empty;
        public string   DatabaseName     { get; set; } = string.Empty;
        public DateTime ServerTimeUtc    { get; set; }
    }
}
