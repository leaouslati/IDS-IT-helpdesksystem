namespace backend.Domain.Entities
{
    public class TicketAttachment
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileType { get; set; } = string.Empty;
        public int UploadedByUserId { get; set; }
        public User UploadedByUser { get; set; } = null!;
        public int? CommentId { get; set; }
        public TicketComment? Comment { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
