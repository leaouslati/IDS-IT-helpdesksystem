namespace backend.Domain.Entities
{
    public class TicketAttachment
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;
        // Original filename shown to the user
        public string FileName { get; set; } = string.Empty;
        // Virtual path stored in DB (e.g. /uploads/tickets/{ticketId}/{guid}.ext)
        public string FilePath { get; set; } = string.Empty;
        // GUID-based filename saved on disk (empty for legacy records stored in wwwroot)
        public string StoredFileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileType { get; set; } = string.Empty;
        public int UploadedByUserId { get; set; }
        public User UploadedByUser { get; set; } = null!;
        public int? CommentId { get; set; }
        public TicketComment? Comment { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
