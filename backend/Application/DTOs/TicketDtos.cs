namespace backend.Application.DTOs
{
    public class AssignTicketDto
    {
        public int AgentUserId { get; set; }
    }

    public class UpdateTicketStatusDto
    {
        public string StatusName { get; set; } = string.Empty;
    }

    public class CreateTicketDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int PriorityId { get; set; }
    }

    public class UpdateTicketDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int PriorityId { get; set; }
    }

    public class EscalateTicketDto
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class AddCommentDto
    {
        public string Content { get; set; } = string.Empty;
    }

    public class TicketSummaryDto
    {
        public int Id { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string? AssignedTo { get; set; }
        public bool IsEscalated { get; set; }
        public int CommentCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class TicketDetailDto
    {
        public int Id { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string Category { get; set; } = string.Empty;
        public int PriorityId { get; set; }
        public string Priority { get; set; } = string.Empty;
        public int TicketStatusId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int CreatedByUserId { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public int? AssignedToUserId { get; set; }
        public string? AssignedTo { get; set; }
        public int? DepartmentId { get; set; }
        public string? Department { get; set; }
        public bool IsEscalated { get; set; }
        public string? EscalatedBy { get; set; }
        public DateTime? EscalatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanAssign { get; set; }
        public bool CanUpdateStatus { get; set; }
        public bool CanEscalate { get; set; }
        public bool CanLogHours { get; set; }
        public string? EscalationReason { get; set; }
        public decimal TotalHoursWorked { get; set; }
        public List<TicketCommentDto> Comments { get; set; } = new();
        public List<TicketAttachmentDto> Attachments { get; set; } = new();
        public List<TicketActivityLogDto> ActivityLog { get; set; } = new();
    }

    public class TicketActivityLogDto
    {
        public string Action { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? FromValue { get; set; }
        public string? ToValue { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserRole { get; set; }
        public DateTime LoggedAt { get; set; }
    }

    public class TicketCommentDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsEscalationComment { get; set; }
        public bool IsInternal { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TicketAttachmentDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileType { get; set; } = string.Empty;
        public int UploadedByUserId { get; set; }
        public string UploadedBy { get; set; } = string.Empty;
        public int? CommentId { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
