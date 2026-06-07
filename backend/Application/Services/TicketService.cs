using backend.Application.DTOs;
using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Services
{
    public class TicketService : ITicketService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif",
            ".pdf", ".doc", ".docx", ".xls", ".xlsx",
            ".zip", ".txt", ".log"
        };
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        public TicketService(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env     = env;
        }

        // ── LIST ─────────────────────────────────────────────────────────────

        public async Task<IEnumerable<TicketSummaryDto>> GetTicketsAsync(int userId, string role)
        {
            IQueryable<Ticket> query = _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.TicketStatus)
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser);

            if (role == "Manager")
            {
                var deptId = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.DepartmentId)
                    .FirstOrDefaultAsync();

                if (!deptId.HasValue) return Enumerable.Empty<TicketSummaryDto>();
                query = query.Where(t => t.DepartmentId == deptId.Value);
            }
            else if (role == "Agent")
            {
                query = query.Where(t => t.AssignedToUserId == userId);
            }
            else if (role == "Employee")
            {
                query = query.Where(t => t.CreatedByUserId == userId);
            }
            // Admin: no additional filter

            return await query
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TicketSummaryDto
                {
                    Id = t.Id,
                    ReferenceNumber = t.ReferenceNumber,
                    Title = t.Title,
                    Category = t.Category.Name,
                    Priority = t.Priority.Name,
                    Status = t.TicketStatus.Name,
                    CreatedBy = t.CreatedByUser.FirstName + " " + t.CreatedByUser.LastName,
                    AssignedTo = t.AssignedToUser != null
                        ? t.AssignedToUser.FirstName + " " + t.AssignedToUser.LastName
                        : null,
                    IsEscalated = t.IsEscalated,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .ToListAsync();
        }

        // ── GET BY ID ────────────────────────────────────────────────────────

        public async Task<TicketDetailDto?> GetTicketByIdAsync(int ticketId, int userId, string role)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.TicketStatus)
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Department)
                .Include(t => t.EscalatedByUser)
                .Include(t => t.Comments).ThenInclude(c => c.User)
                .Include(t => t.Attachments).ThenInclude(a => a.UploadedByUser)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null) return null;

            // Role-based access: return null (→ 404) to avoid leaking existence
            if (role == "Employee" && ticket.CreatedByUserId != userId) return null;
            if (role == "Agent"    && ticket.AssignedToUserId != userId) return null;
            if (role == "Manager")
            {
                var deptId = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.DepartmentId)
                    .FirstOrDefaultAsync();
                if (deptId != ticket.DepartmentId) return null;
            }

            bool isOpen    = ticket.TicketStatus.Name == "Open";
            bool canEdit   = isOpen && role == "Employee" && ticket.CreatedByUserId == userId;
            bool canDelete = isOpen && (
                (role == "Employee" && ticket.CreatedByUserId == userId) ||
                 role == "Manager"
            );

            return new TicketDetailDto
            {
                Id              = ticket.Id,
                ReferenceNumber = ticket.ReferenceNumber,
                Title           = ticket.Title,
                Description     = ticket.Description,
                CategoryId      = ticket.CategoryId,
                Category        = ticket.Category.Name,
                PriorityId      = ticket.PriorityId,
                Priority        = ticket.Priority.Name,
                TicketStatusId  = ticket.TicketStatusId,
                Status          = ticket.TicketStatus.Name,
                CreatedByUserId = ticket.CreatedByUserId,
                CreatedBy       = ticket.CreatedByUser.FirstName + " " + ticket.CreatedByUser.LastName,
                AssignedToUserId = ticket.AssignedToUserId,
                AssignedTo      = ticket.AssignedToUser != null
                    ? ticket.AssignedToUser.FirstName + " " + ticket.AssignedToUser.LastName
                    : null,
                DepartmentId    = ticket.DepartmentId,
                Department      = ticket.Department?.Name,
                IsEscalated     = ticket.IsEscalated,
                EscalatedBy     = ticket.EscalatedByUser != null
                    ? ticket.EscalatedByUser.FirstName + " " + ticket.EscalatedByUser.LastName
                    : null,
                EscalatedAt     = ticket.EscalatedAt,
                CreatedAt       = ticket.CreatedAt,
                UpdatedAt       = ticket.UpdatedAt,
                CanEdit         = canEdit,
                CanDelete       = canDelete,
                Comments        = ticket.Comments
                    .OrderBy(c => c.CreatedAt)
                    .Select(c => new TicketCommentDto
                    {
                        Id                  = c.Id,
                        UserId              = c.UserId,
                        UserName            = c.User.FirstName + " " + c.User.LastName,
                        Content             = c.Content,
                        IsEscalationComment = c.IsEscalationComment,
                        CreatedAt           = c.CreatedAt
                    }).ToList(),
                Attachments     = ticket.Attachments
                    .OrderBy(a => a.UploadedAt)
                    .Select(a => new TicketAttachmentDto
                    {
                        Id               = a.Id,
                        FileName         = a.FileName,
                        FilePath         = a.FilePath,
                        FileSize         = a.FileSize,
                        FileType         = a.FileType,
                        UploadedByUserId = a.UploadedByUserId,
                        UploadedBy       = a.UploadedByUser.FirstName + " " + a.UploadedByUser.LastName,
                        CommentId        = a.CommentId,
                        UploadedAt       = a.UploadedAt
                    }).ToList()
            };
        }

        // ── CREATE ───────────────────────────────────────────────────────────

        public async Task<(TicketDetailDto? ticket, string? error)> CreateTicketAsync(CreateTicketDto dto, int userId)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))       return (null, "Title is required.");
            if (string.IsNullOrWhiteSpace(dto.Description)) return (null, "Description is required.");

            var user = await _context.Users.FindAsync(userId);
            if (user?.DepartmentId == null)
                return (null, "You must be assigned to a department to submit a ticket.");

            if (!await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId))
                return (null, "Invalid category.");
            if (!await _context.Priorities.AnyAsync(p => p.Id == dto.PriorityId))
                return (null, "Invalid priority.");

            var openStatus = await _context.TicketStatuses.FirstAsync(s => s.Name == "Open");

            var ticket = new Ticket
            {
                ReferenceNumber  = "TKT-TMP",
                Title            = dto.Title.Trim(),
                Description      = dto.Description.Trim(),
                CategoryId       = dto.CategoryId,
                PriorityId       = dto.PriorityId,
                TicketStatusId   = openStatus.Id,
                CreatedByUserId  = userId,
                AssignedToUserId = null,
                DepartmentId     = user.DepartmentId,
                IsEscalated      = false,
                CreatedAt        = DateTime.UtcNow
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            // Stable reference number derived from the DB-generated ID
            ticket.ReferenceNumber = $"TKT-{ticket.Id:D3}";

            // Notify department manager
            var dept = await _context.Departments.FindAsync(user.DepartmentId.Value);
            if (dept?.ManagerId != null)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId    = dept.ManagerId.Value,
                    Message   = $"New ticket {ticket.ReferenceNumber} submitted in your department: {ticket.Title}",
                    IsRead    = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.ActivityLogs.Add(new ActivityLog
            {
                UserId   = userId,
                Action   = "Ticket Created",
                Details  = $"Ticket {ticket.ReferenceNumber} created: {ticket.Title}",
                LoggedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            var result = await GetTicketByIdAsync(ticket.Id, userId, "Employee");
            return (result, null);
        }

        // ── UPDATE (Employee, Open only) ──────────────────────────────────────

        public async Task<(bool success, string? error)> UpdateTicketAsync(int ticketId, UpdateTicketDto dto, int userId)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))       return (false, "Title is required.");
            if (string.IsNullOrWhiteSpace(dto.Description)) return (false, "Description is required.");

            var ticket = await _context.Tickets
                .Include(t => t.TicketStatus)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null)                            return (false, "Ticket not found.");
            if (ticket.CreatedByUserId != userId)          return (false, "You can only edit your own tickets.");
            if (ticket.TicketStatus.Name != "Open")        return (false, "Tickets can only be edited while status is Open.");

            if (!await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId))
                return (false, "Invalid category.");
            if (!await _context.Priorities.AnyAsync(p => p.Id == dto.PriorityId))
                return (false, "Invalid priority.");

            ticket.Title       = dto.Title.Trim();
            ticket.Description = dto.Description.Trim();
            ticket.CategoryId  = dto.CategoryId;
            ticket.PriorityId  = dto.PriorityId;
            ticket.UpdatedAt   = DateTime.UtcNow;

            _context.ActivityLogs.Add(new ActivityLog
            {
                UserId   = userId,
                Action   = "Ticket Updated",
                Details  = $"Ticket {ticket.ReferenceNumber} updated",
                LoggedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return (true, null);
        }

        // ── DELETE (Employee/Manager, Open only) ──────────────────────────────

        public async Task<(bool success, string? error)> DeleteTicketAsync(int ticketId, int userId, string role)
        {
            var ticket = await _context.Tickets
                .Include(t => t.TicketStatus)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null) return (false, "Ticket not found.");
            if (ticket.TicketStatus.Name != "Open") return (false, "Only Open tickets can be deleted.");

            if (role == "Employee" && ticket.CreatedByUserId != userId)
                return (false, "You can only delete your own tickets.");

            if (role == "Manager")
            {
                var deptId = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.DepartmentId)
                    .FirstOrDefaultAsync();
                if (deptId != ticket.DepartmentId)
                    return (false, "Ticket does not belong to your department.");
            }

            var refNum = ticket.ReferenceNumber;

            var comments    = await _context.TicketComments.Where(c => c.TicketId == ticketId).ToListAsync();
            var attachments = await _context.TicketAttachments.Where(a => a.TicketId == ticketId).ToListAsync();
            _context.TicketComments.RemoveRange(comments);
            _context.TicketAttachments.RemoveRange(attachments);
            _context.Tickets.Remove(ticket);

            _context.ActivityLogs.Add(new ActivityLog
            {
                UserId   = userId,
                Action   = "Ticket Deleted",
                Details  = $"Ticket {refNum} deleted",
                LoggedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return (true, null);
        }

        // ── ASSIGN (Manager) ──────────────────────────────────────────────────

        public async Task<(bool success, string? error)> AssignTicketAsync(int ticketId, AssignTicketDto dto, int managerId)
        {
            var manager = await _context.Users.FindAsync(managerId);
            if (manager?.DepartmentId == null)
                return (false, "Manager is not assigned to a department.");

            var ticket = await _context.Tickets
                .Include(t => t.TicketStatus)
                .FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null) return (false, "Ticket not found.");

            if (ticket.DepartmentId != manager.DepartmentId)
                return (false, "Ticket does not belong to your department.");

            var agent = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == dto.AgentUserId && u.IsActive);

            if (agent == null)                          return (false, "Agent not found.");
            if (agent.Role.Name != "Agent")             return (false, "Selected user is not an agent.");
            if (agent.DepartmentId != manager.DepartmentId)
                return (false, "Agent does not belong to your department.");

            // Count active (non-resolved/closed) tickets already held by this agent
            var openCount = await _context.Tickets.CountAsync(t =>
                t.AssignedToUserId == dto.AgentUserId &&
                t.TicketStatus.Name != "Resolved" &&
                t.TicketStatus.Name != "Closed");

            if (openCount >= 3)
                return (false, $"Agent already has {openCount} active ticket(s). Maximum is 3.");

            ticket.AssignedToUserId = dto.AgentUserId;
            ticket.UpdatedAt        = DateTime.UtcNow;

            _context.Notifications.Add(new Notification
            {
                UserId    = dto.AgentUserId,
                Message   = $"Ticket {ticket.ReferenceNumber} has been assigned to you: {ticket.Title}",
                IsRead    = false,
                CreatedAt = DateTime.UtcNow
            });

            _context.ActivityLogs.Add(new ActivityLog
            {
                UserId   = managerId,
                Action   = "Ticket Assigned",
                Details  = $"Ticket {ticket.ReferenceNumber} assigned to {agent.FirstName} {agent.LastName}",
                LoggedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return (true, null);
        }

        // ── UPDATE STATUS (Agent) ─────────────────────────────────────────────

        public async Task<(bool success, string? error)> UpdateStatusAsync(int ticketId, UpdateTicketStatusDto dto, int agentId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.TicketStatus)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null)                   return (false, "Ticket not found.");
            if (ticket.AssignedToUserId != agentId) return (false, "This ticket is not assigned to you.");

            if (ticket.TicketStatus.Name == "Resolved" || ticket.TicketStatus.Name == "Closed")
                return (false, "Cannot update status of a resolved or closed ticket.");

            if (dto.StatusName == "Open")
                return (false, "Cannot set status back to Open.");

            var newStatus = await _context.TicketStatuses
                .FirstOrDefaultAsync(s => s.Name == dto.StatusName);
            if (newStatus == null) return (false, $"Invalid status '{dto.StatusName}'.");

            ticket.TicketStatusId = newStatus.Id;
            ticket.UpdatedAt      = DateTime.UtcNow;

            if (dto.StatusName is "Resolved" or "Closed")
            {
                _context.Notifications.Add(new Notification
                {
                    UserId    = ticket.CreatedByUserId,
                    Message   = $"Your ticket {ticket.ReferenceNumber} has been {dto.StatusName.ToLower()}.",
                    IsRead    = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.ActivityLogs.Add(new ActivityLog
            {
                UserId   = agentId,
                Action   = "Ticket Status Updated",
                Details  = $"Ticket {ticket.ReferenceNumber} status changed to {dto.StatusName}",
                LoggedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return (true, null);
        }

        // ── ESCALATE (Agent/Manager) ──────────────────────────────────────────

        public async Task<(bool success, string? error)> EscalateTicketAsync(int ticketId, EscalateTicketDto dto, int userId, string role)
        {
            if (string.IsNullOrWhiteSpace(dto.Reason))
                return (false, "Escalation reason is required.");

            var ticket = await _context.Tickets
                .Include(t => t.TicketStatus)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null) return (false, "Ticket not found.");

            if (ticket.TicketStatus.Name is "Resolved" or "Closed")
                return (false, "Cannot escalate a resolved or closed ticket.");

            if (role == "Agent" && ticket.AssignedToUserId != userId)
                return (false, "This ticket is not assigned to you.");

            if (role == "Manager")
            {
                var deptId = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.DepartmentId)
                    .FirstOrDefaultAsync();
                if (deptId != ticket.DepartmentId)
                    return (false, "Ticket does not belong to your department.");
            }

            ticket.IsEscalated      = true;
            ticket.EscalatedByUserId = userId;
            ticket.EscalatedAt      = DateTime.UtcNow;
            ticket.UpdatedAt        = DateTime.UtcNow;

            // Mandatory escalation reason stored as a flagged comment
            _context.TicketComments.Add(new TicketComment
            {
                TicketId            = ticketId,
                UserId              = userId,
                Content             = dto.Reason.Trim(),
                IsEscalationComment = true,
                CreatedAt           = DateTime.UtcNow
            });

            // When an agent escalates, notify the department manager
            if (role == "Agent" && ticket.DepartmentId.HasValue)
            {
                var deptManagerId = await _context.Departments
                    .Where(d => d.Id == ticket.DepartmentId.Value)
                    .Select(d => d.ManagerId)
                    .FirstOrDefaultAsync();

                if (deptManagerId.HasValue)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId    = deptManagerId.Value,
                        Message   = $"Ticket {ticket.ReferenceNumber} has been escalated: {dto.Reason}",
                        IsRead    = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            var escalator = await _context.Users.FindAsync(userId);
            _context.ActivityLogs.Add(new ActivityLog
            {
                UserId   = userId,
                Action   = "Ticket Escalated",
                Details  = $"Ticket {ticket.ReferenceNumber} escalated by {escalator?.FirstName} {escalator?.LastName}: {dto.Reason}",
                LoggedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return (true, null);
        }

        // ── ADD COMMENT ───────────────────────────────────────────────────────

        public async Task<(TicketCommentDto? comment, string? error)> AddCommentAsync(int ticketId, AddCommentDto dto, int userId, string role)
        {
            if (string.IsNullOrWhiteSpace(dto.Content))
                return (null, "Comment content is required.");

            var ticket = await _context.Tickets
                .Include(t => t.TicketStatus)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null) return (null, "Ticket not found.");

            if (ticket.TicketStatus.Name == "Closed")
                return (null, "Cannot add comments to a closed ticket.");

            // Access check mirrors GetTicketById
            if (role == "Employee" && ticket.CreatedByUserId != userId)
                return (null, "You can only comment on your own tickets.");
            if (role == "Agent" && ticket.AssignedToUserId != userId)
                return (null, "You can only comment on tickets assigned to you.");
            if (role == "Manager")
            {
                var deptId = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.DepartmentId)
                    .FirstOrDefaultAsync();
                if (deptId != ticket.DepartmentId)
                    return (null, "Ticket does not belong to your department.");
            }

            var comment = new TicketComment
            {
                TicketId            = ticketId,
                UserId              = userId,
                Content             = dto.Content.Trim(),
                IsEscalationComment = false,
                CreatedAt           = DateTime.UtcNow
            };

            _context.TicketComments.Add(comment);

            // Notify ticket creator when someone else comments
            if (ticket.CreatedByUserId != userId)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId    = ticket.CreatedByUserId,
                    Message   = $"New comment added to your ticket {ticket.ReferenceNumber}.",
                    IsRead    = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.ActivityLogs.Add(new ActivityLog
            {
                UserId   = userId,
                Action   = "Comment Added",
                Details  = $"Comment added to ticket {ticket.ReferenceNumber}",
                LoggedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);
            return (new TicketCommentDto
            {
                Id                  = comment.Id,
                UserId              = comment.UserId,
                UserName            = user != null ? user.FirstName + " " + user.LastName : "Unknown",
                Content             = comment.Content,
                IsEscalationComment = comment.IsEscalationComment,
                CreatedAt           = comment.CreatedAt
            }, null);
        }

        // ── GET COMMENTS ──────────────────────────────────────────────────────

        public async Task<IEnumerable<TicketCommentDto>> GetCommentsAsync(int ticketId, int userId, string role)
        {
            var ticket = await _context.Tickets
                .Include(t => t.TicketStatus)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null) return Enumerable.Empty<TicketCommentDto>();

            if (role == "Employee" && ticket.CreatedByUserId != userId)
                return Enumerable.Empty<TicketCommentDto>();
            if (role == "Agent" && ticket.AssignedToUserId != userId)
                return Enumerable.Empty<TicketCommentDto>();
            if (role == "Manager")
            {
                var deptId = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.DepartmentId)
                    .FirstOrDefaultAsync();
                if (deptId != ticket.DepartmentId)
                    return Enumerable.Empty<TicketCommentDto>();
            }

            return await _context.TicketComments
                .Include(c => c.User)
                .Where(c => c.TicketId == ticketId)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new TicketCommentDto
                {
                    Id                  = c.Id,
                    UserId              = c.UserId,
                    UserName            = c.User.FirstName + " " + c.User.LastName,
                    Content             = c.Content,
                    IsEscalationComment = c.IsEscalationComment,
                    CreatedAt           = c.CreatedAt
                })
                .ToListAsync();
        }

        // ── UPLOAD ATTACHMENT ─────────────────────────────────────────────────

        public async Task<(TicketAttachmentDto? attachment, string? error)> UploadAttachmentAsync(
            int ticketId, IFormFile? file, int userId, string role)
        {
            if (file == null || file.Length == 0)
                return (null, "No file provided.");

            var ext = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(ext))
                return (null, $"File type '{ext}' is not allowed. Allowed: {string.Join(", ", AllowedExtensions)}");

            if (file.Length > MaxFileSizeBytes)
                return (null, "File size exceeds the 10 MB limit.");

            var ticket = await _context.Tickets
                .Include(t => t.TicketStatus)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null) return (null, "Ticket not found.");

            if (ticket.TicketStatus.Name == "Closed")
                return (null, "Cannot add attachments to a closed ticket.");

            // Access check mirrors comment rules
            if (role == "Employee" && ticket.CreatedByUserId != userId)
                return (null, "You can only upload attachments to your own tickets.");
            if (role == "Agent" && ticket.AssignedToUserId != userId)
                return (null, "You can only upload attachments to tickets assigned to you.");
            if (role == "Manager")
            {
                var deptId = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.DepartmentId)
                    .FirstOrDefaultAsync();
                if (deptId != ticket.DepartmentId)
                    return (null, "Ticket does not belong to your department.");
            }

            // Resolve wwwroot — fall back to <cwd>/wwwroot when not explicitly configured
            var webRoot = string.IsNullOrEmpty(_env.WebRootPath)
                ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
                : _env.WebRootPath;

            var folder = Path.Combine(webRoot, "uploads", "attachments", ticketId.ToString());
            Directory.CreateDirectory(folder);

            var storedName   = $"{Guid.NewGuid()}{ext}";
            var physicalPath = Path.Combine(folder, storedName);

            await using (var stream = new FileStream(physicalPath, FileMode.Create))
                await file.CopyToAsync(stream);

            var relativeUrl = $"/uploads/attachments/{ticketId}/{storedName}";

            var record = new TicketAttachment
            {
                TicketId         = ticketId,
                FileName         = file.FileName,
                FilePath         = relativeUrl,
                FileSize         = file.Length,
                FileType         = file.ContentType,
                UploadedByUserId = userId,
                UploadedAt       = DateTime.UtcNow
            };

            _context.TicketAttachments.Add(record);

            _context.ActivityLogs.Add(new ActivityLog
            {
                UserId   = userId,
                Action   = "Attachment Uploaded",
                Details  = $"File '{file.FileName}' uploaded to ticket {ticket.ReferenceNumber}",
                LoggedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            var uploader = await _context.Users.FindAsync(userId);
            return (new TicketAttachmentDto
            {
                Id               = record.Id,
                FileName         = record.FileName,
                FilePath         = record.FilePath,
                FileSize         = record.FileSize,
                FileType         = record.FileType,
                UploadedByUserId = record.UploadedByUserId,
                UploadedBy       = uploader != null ? uploader.FirstName + " " + uploader.LastName : "Unknown",
                CommentId        = record.CommentId,
                UploadedAt       = record.UploadedAt
            }, null);
        }

        // ── AGENTS AVAILABILITY (Manager assign helper) ───────────────────────

        public async Task<(IEnumerable<AgentAvailabilityDto> agents, string? error)> GetAgentsAvailabilityAsync(
            int ticketId, int managerId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null)
                return (Enumerable.Empty<AgentAvailabilityDto>(), "Ticket not found.");

            var managerDeptId = await _context.Users
                .Where(u => u.Id == managerId)
                .Select(u => u.DepartmentId)
                .FirstOrDefaultAsync();

            if (managerDeptId != ticket.DepartmentId)
                return (Enumerable.Empty<AgentAvailabilityDto>(), "Ticket does not belong to your department.");

            var agentRoleId = await _context.Roles
                .Where(r => r.Name == "Agent")
                .Select(r => r.Id)
                .FirstAsync();

            var agents = await _context.Users
                .Where(u => u.RoleId == agentRoleId && u.IsActive && u.DepartmentId == ticket.DepartmentId)
                .ToListAsync();

            var agentIds  = agents.Select(a => a.Id).ToList();
            var weekStart = DateTime.UtcNow.Date.AddDays(-6);

            var agentTickets = await _context.Tickets
                .Where(t => t.AssignedToUserId.HasValue && agentIds.Contains(t.AssignedToUserId.Value))
                .Select(t => new { t.AssignedToUserId, StatusName = t.TicketStatus.Name, t.UpdatedAt })
                .ToListAsync();

            var result = agents.Select(agent =>
            {
                var openCount = agentTickets.Count(t =>
                    t.AssignedToUserId == agent.Id &&
                    t.StatusName != "Resolved" &&
                    t.StatusName != "Closed");

                var resolvedThisWeek = agentTickets.Count(t =>
                    t.AssignedToUserId == agent.Id &&
                    (t.StatusName == "Resolved" || t.StatusName == "Closed") &&
                    t.UpdatedAt.HasValue && t.UpdatedAt.Value >= weekStart);

                return new AgentAvailabilityDto
                {
                    UserId           = agent.Id,
                    AgentName        = agent.FirstName + " " + agent.LastName,
                    OpenTickets      = openCount,
                    ResolvedThisWeek = resolvedThisWeek,
                    IsAvailable      = openCount < 3
                };
            }).ToList();

            return (result, null);
        }
    }
}
