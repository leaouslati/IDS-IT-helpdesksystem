using backend.Domain.Entities;

namespace backend.Application.Interfaces
{
    public interface ITicketRepository
    {
        // ── Ticket queries ──────────────────────────────────────────────────
        Task<List<Ticket>> GetTicketsWithBasicIncludesAsync();
        Task<Ticket?> GetTicketWithFullDetailsAsync(int ticketId);
        Task<Ticket?> GetTicketWithStatusAsync(int ticketId);
        Task<Ticket?> FindTicketAsync(int ticketId);

        // ── Ticket mutations ────────────────────────────────────────────────
        void AddTicket(Ticket ticket);
        void RemoveTicket(Ticket ticket);
        void RemoveComments(IEnumerable<TicketComment> comments);
        void RemoveAttachments(IEnumerable<TicketAttachment> attachments);

        // ── User lookups ────────────────────────────────────────────────────
        Task<int?> GetUserDepartmentIdAsync(int userId);
        Task<User?> FindUserAsync(int userId);
        Task<User?> GetUserWithRoleAsync(int userId);

        // ── Reference data ──────────────────────────────────────────────────
        Task<bool> CategoryExistsAsync(int id);
        Task<bool> PriorityExistsAsync(int id);
        Task<TicketStatus?> GetStatusByNameAsync(string name);
        Task<TicketStatus> GetOpenStatusAsync();

        // ── Department ──────────────────────────────────────────────────────
        Task<Department?> FindDepartmentAsync(int id);
        Task<int?> GetDepartmentManagerIdAsync(int deptId);
        Task<int?> GetManagerUserIdForDepartmentAsync(int deptId);

        // ── Agent queries ───────────────────────────────────────────────────
        Task<int> CountActiveTicketsByAgentAsync(int agentId);
        Task<int> GetAgentRoleIdAsync();
        Task<List<User>> GetAgentsInDepartmentAsync(int deptId, int agentRoleId);
        Task<List<(int AgentId, string StatusName, DateTime? UpdatedAt)>> GetTicketDataForAgentsAsync(List<int> agentIds);

        // ── Comments ────────────────────────────────────────────────────────
        // ThenInclude on User.Role is required to return UserRole in TicketCommentDto
        Task<List<TicketComment>> GetCommentsWithUserAndRoleAsync(int ticketId);
        Task<List<TicketComment>> GetCommentsForDeleteAsync(int ticketId);
        void AddComment(TicketComment comment);

        // ── Attachments ─────────────────────────────────────────────────────
        Task<List<TicketAttachment>> GetAttachmentsWithUploaderAsync(int ticketId);
        Task<List<TicketAttachment>> GetAttachmentsForDeleteAsync(int ticketId);
        Task<TicketAttachment?> FindAttachmentAsync(int attachmentId);
        void AddAttachment(TicketAttachment attachment);

        // ── ActivityLog ─────────────────────────────────────────────────────
        Task<List<ActivityLog>> GetActivityLogsForTicketAsync(int ticketId);
        void AddActivityLog(ActivityLog log);

        // ── Participant lookup for notifications ────────────────────────────
        // Returns unique user IDs of: ticket creator, assigned agent, and all commenters,
        // excluding the given userId (the person triggering the event).
        Task<List<int>> GetTicketParticipantIdsAsync(int ticketId, int excludeUserId);

        // ── Hours ───────────────────────────────────────────────────────────
        Task<decimal> GetTotalHoursForTicketAsync(int ticketId);

        // ── Persistence ─────────────────────────────────────────────────────
        Task SaveChangesAsync();
    }
}
