using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly AppDbContext _context;

        public TicketRepository(AppDbContext context)
        {
            _context = context;
        }

        // ── Ticket queries ──────────────────────────────────────────────────

        public async Task<List<Ticket>> GetTicketsWithBasicIncludesAsync() =>
            await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.TicketStatus)
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Comments)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

        public async Task<Ticket?> GetTicketWithFullDetailsAsync(int ticketId) =>
            await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.TicketStatus)
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Department)
                .Include(t => t.EscalatedByUser)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

        public async Task<Ticket?> GetTicketWithStatusAsync(int ticketId) =>
            await _context.Tickets
                .Include(t => t.TicketStatus)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

        public async Task<Ticket?> FindTicketAsync(int ticketId) =>
            await _context.Tickets.FindAsync(ticketId);

        // ── Ticket mutations ────────────────────────────────────────────────

        public void AddTicket(Ticket ticket) => _context.Tickets.Add(ticket);

        public void RemoveTicket(Ticket ticket) => _context.Tickets.Remove(ticket);

        public void RemoveComments(IEnumerable<TicketComment> comments) =>
            _context.TicketComments.RemoveRange(comments);

        public void RemoveAttachments(IEnumerable<TicketAttachment> attachments) =>
            _context.TicketAttachments.RemoveRange(attachments);

        // ── User lookups ────────────────────────────────────────────────────

        public async Task<int?> GetUserDepartmentIdAsync(int userId) =>
            await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.DepartmentId)
                .FirstOrDefaultAsync();

        public async Task<User?> FindUserAsync(int userId) =>
            await _context.Users.FindAsync(userId);

        public async Task<User?> GetUserWithRoleAsync(int userId) =>
            await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

        // ── Reference data ──────────────────────────────────────────────────

        public async Task<bool> CategoryExistsAsync(int id) =>
            await _context.Categories.AnyAsync(c => c.Id == id);

        public async Task<bool> PriorityExistsAsync(int id) =>
            await _context.Priorities.AnyAsync(p => p.Id == id);

        public async Task<TicketStatus?> GetStatusByNameAsync(string name) =>
            await _context.TicketStatuses.FirstOrDefaultAsync(s => s.Name == name);

        public async Task<TicketStatus> GetOpenStatusAsync() =>
            await _context.TicketStatuses.FirstAsync(s => s.Name == "Open");

        // ── Department ──────────────────────────────────────────────────────

        public async Task<Department?> FindDepartmentAsync(int id) =>
            await _context.Departments.FindAsync(id);

        public async Task<int?> GetDepartmentManagerIdAsync(int deptId) =>
            await _context.Departments
                .Where(d => d.Id == deptId)
                .Select(d => d.ManagerId)
                .FirstOrDefaultAsync();

        public async Task<int?> GetManagerUserIdForDepartmentAsync(int deptId) =>
            await _context.Users
                .Where(u => u.DepartmentId == deptId && u.IsActive && u.Role.Name == "Manager")
                .Select(u => (int?)u.Id)
                .FirstOrDefaultAsync();

        // ── Agent queries ───────────────────────────────────────────────────

        public async Task<int> CountActiveTicketsByAgentAsync(int agentId) =>
            await _context.Tickets.CountAsync(t =>
                t.AssignedToUserId == agentId &&
                t.TicketStatus.Name != "Resolved");

        public async Task<int> GetAgentRoleIdAsync() =>
            await _context.Roles
                .Where(r => r.Name == "Agent")
                .Select(r => r.Id)
                .FirstAsync();

        public async Task<List<User>> GetAgentsInDepartmentAsync(int deptId, int agentRoleId) =>
            await _context.Users
                .Where(u => u.RoleId == agentRoleId && u.IsActive && u.DepartmentId == deptId)
                .ToListAsync();

        public async Task<List<(int AgentId, string StatusName, DateTime? UpdatedAt)>> GetTicketDataForAgentsAsync(
            List<int> agentIds) =>
            (await _context.Tickets
                .Where(t => t.AssignedToUserId.HasValue && agentIds.Contains(t.AssignedToUserId.Value))
                .Select(t => new
                {
                    AgentId = t.AssignedToUserId!.Value,
                    StatusName = t.TicketStatus.Name,
                    t.UpdatedAt
                })
                .ToListAsync())
                .Select(x => (x.AgentId, x.StatusName, x.UpdatedAt))
                .ToList();

        // ── Comments ────────────────────────────────────────────────────────

        // ThenInclude on User.Role required so that UserRole is available for TicketCommentDto
        public async Task<List<TicketComment>> GetCommentsWithUserAndRoleAsync(int ticketId) =>
            await _context.TicketComments
                .Include(c => c.User)
                    .ThenInclude(u => u.Role)
                .Where(c => c.TicketId == ticketId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

        public async Task<List<TicketComment>> GetCommentsForDeleteAsync(int ticketId) =>
            await _context.TicketComments
                .Where(c => c.TicketId == ticketId)
                .ToListAsync();

        public void AddComment(TicketComment comment) => _context.TicketComments.Add(comment);

        // ── Attachments ─────────────────────────────────────────────────────

        // ThenInclude on UploadedByUser required to populate UploadedBy name in TicketAttachmentDto
        public async Task<List<TicketAttachment>> GetAttachmentsWithUploaderAsync(int ticketId) =>
            await _context.TicketAttachments
                .Include(a => a.UploadedByUser)
                .Where(a => a.TicketId == ticketId)
                .OrderBy(a => a.UploadedAt)
                .ToListAsync();

        public async Task<List<TicketAttachment>> GetAttachmentsForDeleteAsync(int ticketId) =>
            await _context.TicketAttachments
                .Where(a => a.TicketId == ticketId)
                .ToListAsync();

        public async Task<TicketAttachment?> FindAttachmentAsync(int attachmentId) =>
            await _context.TicketAttachments.FindAsync(attachmentId);

        public void AddAttachment(TicketAttachment attachment) =>
            _context.TicketAttachments.Add(attachment);

        // ── ActivityLog ─────────────────────────────────────────────────────

        public async Task<List<ActivityLog>> GetActivityLogsForTicketAsync(int ticketId) =>
            await _context.ActivityLogs
                .Include(a => a.User)
                    .ThenInclude(u => u.Role)
                .Where(a => a.TicketId == ticketId)
                .OrderBy(a => a.LoggedAt)
                .ToListAsync();

        public void AddActivityLog(ActivityLog log) => _context.ActivityLogs.Add(log);

        // ── Participant lookup ───────────────────────────────────────────────

        public async Task<List<int>> GetTicketParticipantIdsAsync(int ticketId, int excludeUserId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return new List<int>();

            var participants = new HashSet<int>();
            if (ticket.CreatedByUserId != excludeUserId)
                participants.Add(ticket.CreatedByUserId);
            if (ticket.AssignedToUserId.HasValue && ticket.AssignedToUserId.Value != excludeUserId)
                participants.Add(ticket.AssignedToUserId.Value);

            var commenters = await _context.TicketComments
                .Where(c => c.TicketId == ticketId && c.UserId != excludeUserId)
                .Select(c => c.UserId)
                .Distinct()
                .ToListAsync();

            foreach (var id in commenters) participants.Add(id);
            return participants.ToList();
        }

        // ── Hours ───────────────────────────────────────────────────────────

        public async Task<decimal> GetTotalHoursForTicketAsync(int ticketId) =>
            await _context.TicketHoursLogs
                .Where(h => h.TicketId == ticketId)
                .SumAsync(h => (decimal?)h.HoursWorked) ?? 0m;

        // ── Persistence ─────────────────────────────────────────────────────

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
