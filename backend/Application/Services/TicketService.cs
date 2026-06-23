using AutoMapper;
using backend.Application.DTOs;
using backend.Application.Interfaces;
using backend.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace backend.Application.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository      _repo;
        private readonly IFileStorageService    _fileStorage;
        private readonly INotificationService   _notifications;
        private readonly IMapper                _mapper;
        private readonly int                    _maxFileSizeBytes;

        // ── Allowed extensions (server-side allow-list) ───────────────────────
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            // Images
            ".png", ".jpg", ".jpeg", ".gif", ".webp",
            // Documents
            ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".pptx", ".ppt", ".txt", ".csv",
            // Logs
            ".log"
        };

        // ── Explicitly blocked extensions ─────────────────────────────────────
        private static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".exe", ".php", ".sh", ".bat", ".cmd", ".js", ".vbs", ".ps1",
            ".dll", ".msi", ".jar", ".py", ".asp", ".aspx", ".jsp"
        };

        public TicketService(
            ITicketRepository repo,
            IFileStorageService fileStorage,
            INotificationService notifications,
            IMapper mapper,
            IConfiguration config)
        {
            _repo          = repo;
            _fileStorage   = fileStorage;
            _notifications = notifications;
            _mapper        = mapper;
            // Read from config; default 10 MB
            var maxMb         = config.GetValue<int>("FileUpload:MaxSizeMB", 10);
            _maxFileSizeBytes = maxMb * 1024 * 1024;
        }

        // ── LIST ─────────────────────────────────────────────────────────────

        public async Task<IEnumerable<TicketSummaryDto>> GetTicketsAsync(int userId, string role)
        {
            var tickets = await _repo.GetTicketsWithBasicIncludesAsync();

            IEnumerable<Ticket> filtered = role switch
            {
                "Manager"  => await FilterForManagerAsync(tickets, userId),
                "Agent"    => tickets.Where(t => t.AssignedToUserId == userId),
                "Employee" => tickets.Where(t => t.CreatedByUserId == userId),
                _          => tickets // Admin: no filter
            };

            return filtered
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => _mapper.Map<TicketSummaryDto>(t));
        }

        private async Task<IEnumerable<Ticket>> FilterForManagerAsync(List<Ticket> tickets, int userId)
        {
            var deptId = await _repo.GetUserDepartmentIdAsync(userId);
            return deptId.HasValue
                ? tickets.Where(t => t.DepartmentId == deptId.Value)
                : Enumerable.Empty<Ticket>();
        }

        // ── GET BY ID ────────────────────────────────────────────────────────

        public async Task<TicketDetailDto?> GetTicketByIdAsync(int ticketId, int userId, string role)
        {
            var ticket = await _repo.GetTicketWithFullDetailsAsync(ticketId);
            if (ticket == null) return null;

            // Role-based access — return null (→ 404) to avoid leaking ticket existence
            if (role == "Employee" && ticket.CreatedByUserId != userId) return null;
            if (role == "Agent"    && ticket.AssignedToUserId != userId) return null;
            if (role == "Manager")
            {
                var deptId = await _repo.GetUserDepartmentIdAsync(userId);
                if (deptId != ticket.DepartmentId) return null;
            }

            var comments    = await _repo.GetCommentsWithUserAndRoleAsync(ticketId);
            var attachments = await _repo.GetAttachmentsWithUploaderAsync(ticketId);
            var totalHours  = await _repo.GetTotalHoursForTicketAsync(ticketId);

            List<ActivityLog> activityLogs = role is "Manager" or "Admin"
                ? await _repo.GetActivityLogsForTicketAsync(ticketId)
                : new List<ActivityLog>();

            bool isOpen          = ticket.TicketStatus.Name == "Open";
            bool isActive        = ticket.TicketStatus.Name != "Resolved";
            bool canEdit         = isOpen && role == "Employee" && ticket.CreatedByUserId == userId;
            bool canDelete       = isOpen && (
                (role == "Employee" && ticket.CreatedByUserId == userId) ||
                 role == "Manager" || role == "Admin");
            bool canAssign       = (role == "Manager" || role == "Admin") && isActive;
            bool canUpdateStatus = role == "Agent" && ticket.AssignedToUserId == userId && isActive;
            bool canEscalate     = (role == "Agent" || role == "Manager") && isActive
                                   && ticket.AssignedToUserId != null && !ticket.IsEscalated;
            bool canLogHours     = role == "Agent" && ticket.AssignedToUserId == userId && isActive;

            string? escalationReason = ticket.IsEscalated
                ? comments
                    .Where(c => c.IsEscalationComment)
                    .OrderBy(c => c.CreatedAt)
                    .Select(c => c.Content)
                    .FirstOrDefault()
                : null;

            if (escalationReason != null)
                escalationReason = System.Text.RegularExpressions.Regex
                    .Replace(escalationReason, "<.*?>", string.Empty)
                    .Replace("&nbsp;", " ")
                    .Trim();

            bool canSeeEscalationHistory = role is "Admin" or "Manager";

            return new TicketDetailDto
            {
                Id               = ticket.Id,
                ReferenceNumber  = ticket.ReferenceNumber,
                Title            = ticket.Title,
                Description      = ticket.Description,
                CategoryId       = ticket.CategoryId,
                Category         = ticket.Category.Name,
                PriorityId       = ticket.PriorityId,
                Priority         = ticket.Priority.Name,
                TicketStatusId   = ticket.TicketStatusId,
                Status           = ticket.TicketStatus.Name,
                CreatedByUserId  = ticket.CreatedByUserId,
                CreatedBy        = ticket.CreatedByUser.FirstName + " " + ticket.CreatedByUser.LastName,
                AssignedToUserId = ticket.AssignedToUserId,
                AssignedTo       = ticket.AssignedToUser != null
                    ? ticket.AssignedToUser.FirstName + " " + ticket.AssignedToUser.LastName
                    : null,
                DepartmentId     = ticket.DepartmentId,
                Department       = ticket.Department?.Name,
                IsEscalated      = ticket.IsEscalated,
                EscalatedByUserId = canSeeEscalationHistory ? ticket.EscalatedByUserId : null,
                EscalatedBy      = canSeeEscalationHistory && ticket.EscalatedByUser != null
                    ? ticket.EscalatedByUser.FirstName + " " + ticket.EscalatedByUser.LastName
                    : null,
                EscalatedAt      = canSeeEscalationHistory ? ticket.EscalatedAt : null,
                CreatedAt        = ticket.CreatedAt,
                UpdatedAt        = ticket.UpdatedAt,
                CanEdit          = canEdit,
                CanDelete        = canDelete,
                CanAssign        = canAssign,
                CanUpdateStatus  = canUpdateStatus,
                CanEscalate      = canEscalate,
                CanLogHours      = canLogHours,
                EscalationReason = canSeeEscalationHistory ? escalationReason : null,
                TotalHoursWorked = totalHours,
                Comments         = _mapper.Map<List<TicketCommentDto>>(comments),
                Attachments      = _mapper.Map<List<TicketAttachmentDto>>(attachments),
                ActivityLog      = _mapper.Map<List<TicketActivityLogDto>>(activityLogs)
            };
        }

        // ── CREATE ───────────────────────────────────────────────────────────

        public async Task<(TicketDetailDto? ticket, string? error)> CreateTicketAsync(CreateTicketDto dto, int userId)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))       return (null, "Title is required.");
            if (string.IsNullOrWhiteSpace(dto.Description)) return (null, "Description is required.");

            var user = await _repo.FindUserAsync(userId);
            if (user?.DepartmentId == null)
                return (null, "You must be assigned to a department to submit a ticket.");

            if (!await _repo.CategoryExistsAsync(dto.CategoryId))
                return (null, "Invalid category.");
            if (!await _repo.PriorityExistsAsync(dto.PriorityId))
                return (null, "Invalid priority.");

            var openStatus = await _repo.GetOpenStatusAsync();

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

            _repo.AddTicket(ticket);
            await _repo.SaveChangesAsync();

            ticket.ReferenceNumber = $"TKT-{ticket.Id:D3}";

            _repo.AddActivityLog(new ActivityLog
            {
                UserId   = userId,
                TicketId = ticket.Id,
                Action   = "Ticket Created",
                Details  = $"Ticket {ticket.ReferenceNumber} created: {ticket.Title}",
                LoggedAt = DateTime.UtcNow
            });

            await _repo.SaveChangesAsync();

            // Notify department manager (bell + email)
            var managerId = await _repo.GetDepartmentManagerIdAsync(user.DepartmentId.Value);
            if (managerId.HasValue)
            {
                await _notifications.NotifyAsync(
                    type:           NotificationType.TicketCreated,
                    ticketId:       ticket.Id,
                    ticketRef:      ticket.ReferenceNumber,
                    ticketTitle:    ticket.Title,
                    recipientUserIds: new[] { managerId.Value },
                    message:        $"New ticket {ticket.ReferenceNumber} submitted in your department: \"{ticket.Title}\". Please assign it to an agent.",
                    sendEmail:      true);
            }

            var result = await GetTicketByIdAsync(ticket.Id, userId, "Employee");
            return (result, null);
        }

        // ── UPDATE (Employee, Open only) ──────────────────────────────────────

        public async Task<(bool success, string? error)> UpdateTicketAsync(int ticketId, UpdateTicketDto dto, int userId)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))       return (false, "Title is required.");
            if (string.IsNullOrWhiteSpace(dto.Description)) return (false, "Description is required.");

            var ticket = await _repo.GetTicketWithStatusAsync(ticketId);
            if (ticket == null)                           return (false, "Ticket not found.");
            if (ticket.CreatedByUserId != userId)         return (false, "You can only edit your own tickets.");
            if (ticket.TicketStatus.Name != "Open")       return (false, "Tickets can only be edited while status is Open.");

            if (!await _repo.CategoryExistsAsync(dto.CategoryId))
                return (false, "Invalid category.");
            if (!await _repo.PriorityExistsAsync(dto.PriorityId))
                return (false, "Invalid priority.");

            var changes = new List<string>();
            if (ticket.Title != dto.Title.Trim()) changes.Add("title");
            if (ticket.Description != dto.Description.Trim()) changes.Add("description");
            if (ticket.CategoryId != dto.CategoryId) changes.Add("category");
            if (ticket.PriorityId != dto.PriorityId) changes.Add("priority");

            ticket.Title       = dto.Title.Trim();
            ticket.Description = dto.Description.Trim();
            ticket.CategoryId  = dto.CategoryId;
            ticket.PriorityId  = dto.PriorityId;
            ticket.UpdatedAt   = DateTime.UtcNow;

            _repo.AddActivityLog(new ActivityLog
            {
                UserId   = userId,
                TicketId = ticketId,
                Action   = "Ticket Updated",
                Details  = changes.Count > 0
                    ? $"Updated {string.Join(", ", changes)} on ticket {ticket.ReferenceNumber}"
                    : $"Ticket {ticket.ReferenceNumber} updated (no field changes detected)",
                LoggedAt = DateTime.UtcNow
            });

            await _repo.SaveChangesAsync();
            return (true, null);
        }

        // ── DELETE ────────────────────────────────────────────────────────────

        public async Task<(bool success, string? error)> DeleteTicketAsync(int ticketId, int userId, string role)
        {
            var ticket = await _repo.GetTicketWithStatusAsync(ticketId);
            if (ticket == null) return (false, "Ticket not found.");
            if (ticket.TicketStatus.Name != "Open") return (false, "Only Open tickets can be deleted.");

            if (role == "Employee" && ticket.CreatedByUserId != userId)
                return (false, "You can only delete your own tickets.");

            if (role == "Manager")
            {
                var deptId = await _repo.GetUserDepartmentIdAsync(userId);
                if (deptId != ticket.DepartmentId)
                    return (false, "Ticket does not belong to your department.");
            }

            var refNum = ticket.ReferenceNumber;

            var attachments = await _repo.GetAttachmentsForDeleteAsync(ticketId);
            // Delete physical files before removing DB records
            foreach (var a in attachments)
                _fileStorage.DeleteFile(a.StoredFileName, a.TicketId);

            var comments = await _repo.GetCommentsForDeleteAsync(ticketId);
            _repo.RemoveComments(comments);
            _repo.RemoveAttachments(attachments);
            _repo.RemoveTicket(ticket);

            _repo.AddActivityLog(new ActivityLog
            {
                UserId   = userId,
                Action   = "Ticket Deleted",
                Details  = $"Ticket {refNum} deleted by {role}",
                LoggedAt = DateTime.UtcNow
            });

            await _repo.SaveChangesAsync();
            return (true, null);
        }

        // ── ASSIGN (Manager) ──────────────────────────────────────────────────

        public async Task<(bool success, string? error)> AssignTicketAsync(int ticketId, AssignTicketDto dto, int managerId)
        {
            var manager = await _repo.FindUserAsync(managerId);
            if (manager?.DepartmentId == null)
                return (false, "Manager is not assigned to a department.");

            var ticket = await _repo.GetTicketWithStatusAsync(ticketId);
            if (ticket == null) return (false, "Ticket not found.");

            if (ticket.DepartmentId != manager.DepartmentId)
                return (false, "Ticket does not belong to your department.");

            var agent = await _repo.GetUserWithRoleAsync(dto.AgentUserId);
            if (agent == null)                              return (false, "Agent not found.");
            if (agent.Role.Name != "Agent")                 return (false, "Selected user is not an agent.");
            if (agent.DepartmentId != manager.DepartmentId) return (false, "Agent does not belong to your department.");

            var openCount = await _repo.CountActiveTicketsByAgentAsync(dto.AgentUserId);
            if (openCount >= 3)
                return (false, $"Agent already has {openCount} active ticket(s). Maximum is 3.");

            string? previousAgent = null;
            if (ticket.AssignedToUserId.HasValue)
            {
                var prev = await _repo.FindUserAsync(ticket.AssignedToUserId.Value);
                if (prev != null) previousAgent = prev.FirstName + " " + prev.LastName;
            }

            ticket.AssignedToUserId = dto.AgentUserId;
            ticket.UpdatedAt        = DateTime.UtcNow;

            _repo.AddActivityLog(new ActivityLog
            {
                UserId    = managerId,
                TicketId  = ticketId,
                Action    = "Agent Assigned",
                Details   = $"Ticket {ticket.ReferenceNumber} assigned to {agent.FirstName} {agent.LastName}",
                FromValue = previousAgent,
                ToValue   = agent.FirstName + " " + agent.LastName,
                LoggedAt  = DateTime.UtcNow
            });

            await _repo.SaveChangesAsync();

            // Notify the assigned agent (bell + email)
            await _notifications.NotifyAsync(
                type:           NotificationType.TicketAssigned,
                ticketId:       ticketId,
                ticketRef:      ticket.ReferenceNumber,
                ticketTitle:    ticket.Title,
                recipientUserIds: new[] { dto.AgentUserId },
                message:        $"Ticket {ticket.ReferenceNumber} \"{ticket.Title}\" has been assigned to you.",
                sendEmail:      true);

            return (true, null);
        }

        // ── UPDATE STATUS (Agent) ─────────────────────────────────────────────

        public async Task<(bool success, string? error)> UpdateStatusAsync(int ticketId, UpdateTicketStatusDto dto, int agentId)
        {
            var ticket = await _repo.GetTicketWithStatusAsync(ticketId);
            if (ticket == null)                    return (false, "Ticket not found.");
            if (ticket.AssignedToUserId != agentId) return (false, "This ticket is not assigned to you.");

            if (ticket.TicketStatus.Name == "Resolved")
                return (false, "Cannot update status of a resolved ticket.");
            if (dto.StatusName == "Open")
                return (false, "Cannot set status back to Open.");
            if (dto.StatusName == "Pending")
                return (false, "Pending is not a valid status.");

            var newStatus = await _repo.GetStatusByNameAsync(dto.StatusName);
            if (newStatus == null) return (false, $"Invalid status '{dto.StatusName}'.");

            var previousStatus = ticket.TicketStatus.Name;

            ticket.TicketStatusId = newStatus.Id;
            ticket.UpdatedAt      = DateTime.UtcNow;

            _repo.AddActivityLog(new ActivityLog
            {
                UserId    = agentId,
                TicketId  = ticketId,
                Action    = "Status Changed",
                Details   = $"Status of ticket {ticket.ReferenceNumber} changed",
                FromValue = previousStatus,
                ToValue   = dto.StatusName,
                LoggedAt  = DateTime.UtcNow
            });

            await _repo.SaveChangesAsync();

            // Notify ticket creator when resolved (bell + email — prompts follow-up with client)
            if (dto.StatusName == "Resolved")
            {
                await _notifications.NotifyAsync(
                    type:           NotificationType.TicketClosed,
                    ticketId:       ticketId,
                    ticketRef:      ticket.ReferenceNumber,
                    ticketTitle:    ticket.Title,
                    recipientUserIds: new[] { ticket.CreatedByUserId },
                    message:        $"Your ticket {ticket.ReferenceNumber} \"{ticket.Title}\" has been resolved. Please follow up with the client to confirm.",
                    sendEmail:      true);
            }

            return (true, null);
        }

        // ── ESCALATE (Agent/Manager) ──────────────────────────────────────────

        public async Task<(bool success, string? error)> EscalateTicketAsync(int ticketId, EscalateTicketDto dto, int userId, string role)
        {
            if (string.IsNullOrWhiteSpace(dto.Reason))
                return (false, "Escalation reason is required.");

            var ticket = await _repo.GetTicketWithStatusAsync(ticketId);
            if (ticket == null) return (false, "Ticket not found.");

            if (ticket.TicketStatus.Name == "Resolved")
                return (false, "Cannot escalate a resolved ticket.");
            if (ticket.AssignedToUserId == null)
                return (false, "Cannot escalate an unassigned ticket. Assign it to an agent first.");
            if (role == "Agent" && ticket.AssignedToUserId != userId)
                return (false, "This ticket is not assigned to you.");

            if (role == "Manager")
            {
                var deptId = await _repo.GetUserDepartmentIdAsync(userId);
                if (deptId != ticket.DepartmentId)
                    return (false, "Ticket does not belong to your department.");
            }

            ticket.IsEscalated       = true;
            ticket.EscalatedByUserId = userId;
            ticket.EscalatedAt       = DateTime.UtcNow;
            ticket.UpdatedAt         = DateTime.UtcNow;
            // Clear assignee so the ticket leaves the agent's queue and the manager must reassign
            ticket.AssignedToUserId  = null;

            _repo.AddComment(new TicketComment
            {
                TicketId            = ticketId,
                UserId              = userId,
                Content             = dto.Reason.Trim(),
                IsEscalationComment = true,
                CreatedAt           = DateTime.UtcNow
            });

            var escalator = await _repo.FindUserAsync(userId);
            _repo.AddActivityLog(new ActivityLog
            {
                UserId   = userId,
                TicketId = ticketId,
                Action   = "Ticket Escalated",
                Details  = $"Ticket {ticket.ReferenceNumber} escalated by {escalator?.FirstName} {escalator?.LastName}: {dto.Reason}",
                LoggedAt = DateTime.UtcNow
            });

            await _repo.SaveChangesAsync();

            // Notify department manager(s) to reassign (bell + email)
            if (ticket.DepartmentId.HasValue)
            {
                var deptManagerId = await _repo.GetDepartmentManagerIdAsync(ticket.DepartmentId.Value);
                if (deptManagerId.HasValue)
                {
                    await _notifications.NotifyAsync(
                        type:           NotificationType.TicketEscalated,
                        ticketId:       ticketId,
                        ticketRef:      ticket.ReferenceNumber,
                        ticketTitle:    ticket.Title,
                        recipientUserIds: new[] { deptManagerId.Value },
                        message:        $"Ticket {ticket.ReferenceNumber} \"{ticket.Title}\" has been escalated and needs reassignment.",
                        sendEmail:      true);
                }
            }

            return (true, null);
        }

        // ── ADD COMMENT ───────────────────────────────────────────────────────

        public async Task<(TicketCommentDto? comment, string? error)> AddCommentAsync(
            int ticketId, AddCommentDto dto, int userId, string role)
        {
            if (string.IsNullOrWhiteSpace(dto.Content))
                return (null, "Comment content is required.");

            var ticket = await _repo.GetTicketWithStatusAsync(ticketId);
            if (ticket == null) return (null, "Ticket not found.");

            if (ticket.TicketStatus.Name == "Resolved")
                return (null, "Cannot add comments to a resolved ticket.");

            if (role == "Employee" && ticket.CreatedByUserId != userId)
                return (null, "You can only comment on your own tickets.");
            if (role == "Agent" && ticket.AssignedToUserId != userId)
                return (null, "You can only comment on tickets assigned to you.");
            if (role == "Manager")
            {
                var deptId = await _repo.GetUserDepartmentIdAsync(userId);
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

            _repo.AddComment(comment);

            _repo.AddActivityLog(new ActivityLog
            {
                UserId   = userId,
                TicketId = ticketId,
                Action   = "Comment Added",
                Details  = $"Comment added to ticket {ticket.ReferenceNumber}",
                LoggedAt = DateTime.UtcNow
            });

            await _repo.SaveChangesAsync();

            // Notify all ticket participants except the commenter (bell only)
            var participants = await _repo.GetTicketParticipantIdsAsync(ticketId, userId);
            if (participants.Count > 0)
            {
                await _notifications.NotifyAsync(
                    type:           NotificationType.CommentAdded,
                    ticketId:       ticketId,
                    ticketRef:      ticket.ReferenceNumber,
                    ticketTitle:    ticket.Title,
                    recipientUserIds: participants,
                    message:        $"A new comment was added to ticket {ticket.ReferenceNumber} \"{ticket.Title}\".",
                    sendEmail:      false);
            }

            var user = await _repo.GetUserWithRoleAsync(userId);
            return (new TicketCommentDto
            {
                Id                  = comment.Id,
                UserId              = comment.UserId,
                UserName            = user != null ? user.FirstName + " " + user.LastName : "Unknown",
                UserRole            = user?.Role?.Name ?? string.Empty,
                Content             = comment.Content,
                IsEscalationComment = comment.IsEscalationComment,
                IsAttachmentOnly    = false,
                IsInternal          = false,
                CreatedAt           = comment.CreatedAt
            }, null);
        }

        // ── GET COMMENTS ──────────────────────────────────────────────────────

        public async Task<IEnumerable<TicketCommentDto>> GetCommentsAsync(int ticketId, int userId, string role)
        {
            var ticket = await _repo.GetTicketWithStatusAsync(ticketId);
            if (ticket == null) return Enumerable.Empty<TicketCommentDto>();

            if (role == "Employee" && ticket.CreatedByUserId != userId)
                return Enumerable.Empty<TicketCommentDto>();
            if (role == "Agent" && ticket.AssignedToUserId != userId)
                return Enumerable.Empty<TicketCommentDto>();
            if (role == "Manager")
            {
                var deptId = await _repo.GetUserDepartmentIdAsync(userId);
                if (deptId != ticket.DepartmentId)
                    return Enumerable.Empty<TicketCommentDto>();
            }

            var comments = await _repo.GetCommentsWithUserAndRoleAsync(ticketId);
            return _mapper.Map<List<TicketCommentDto>>(comments);
        }

        // ── UPLOAD ATTACHMENT ─────────────────────────────────────────────────
        // Design: POST /api/ticket/{id}/attachments (file only, no comment required).
        // A system-style timeline comment is auto-created so the upload appears in the
        // chronological feed alongside regular comments. CommentId links the attachment to it.
        // For a "comment + file" workflow, the frontend makes two calls: POST /comment then POST /attachments.

        public async Task<(TicketAttachmentDto? attachment, string? error)> UploadAttachmentAsync(
            int ticketId, IFormFile? file, int userId, string role)
        {
            if (file == null || file.Length == 0)
                return (null, "No file provided.");

            // ── Extension validation ──────────────────────────────────────────
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (BlockedExtensions.Contains(ext))
                return (null, $"File type '{ext}' is explicitly blocked for security reasons.");

            if (!AllowedExtensions.Contains(ext))
                return (null, $"File type '{ext}' is not allowed. Allowed: {string.Join(", ", AllowedExtensions.OrderBy(e => e))}");

            // ── Size validation ───────────────────────────────────────────────
            if (file.Length > _maxFileSizeBytes)
                return (null, $"File size exceeds the {_maxFileSizeBytes / (1024 * 1024)} MB limit.");

            // ── Magic-byte validation (catch executables disguised as other types) ──
            await using var peekStream = file.OpenReadStream();
            var header = new byte[8];
            var read   = await peekStream.ReadAsync(header.AsMemory(0, header.Length));
            if (HasForbiddenMagicBytes(header, read))
                return (null, "File content does not match the declared file type. Executable content is not allowed.");

            // ── Ticket access validation ──────────────────────────────────────
            var ticket = await _repo.GetTicketWithStatusAsync(ticketId);
            if (ticket == null) return (null, "Ticket not found.");

            if (ticket.TicketStatus.Name == "Resolved")
                return (null, "Cannot add attachments to a resolved ticket.");

            if (role == "Employee" && ticket.CreatedByUserId != userId)
                return (null, "You can only upload attachments to your own tickets.");
            if (role == "Agent" && ticket.AssignedToUserId != userId)
                return (null, "You can only upload attachments to tickets assigned to you.");
            if (role == "Manager")
            {
                var deptId = await _repo.GetUserDepartmentIdAsync(userId);
                if (deptId != ticket.DepartmentId)
                    return (null, "Ticket does not belong to your department.");
            }

            // ── Save file via storage service ─────────────────────────────────
            peekStream.Seek(0, SeekOrigin.Begin);
            var (storedFileName, virtualPath) = await _fileStorage.SaveFileAsync(
                peekStream, ticketId, file.FileName, ext);

            // ── Create auto-generated timeline comment ────────────────────────
            var uploader = await _repo.FindUserAsync(userId);
            var uploaderName = uploader != null
                ? uploader.FirstName + " " + uploader.LastName
                : "Unknown";

            var timelineComment = new TicketComment
            {
                TicketId         = ticketId,
                UserId           = userId,
                Content          = $"{uploaderName} attached {file.FileName}",
                IsEscalationComment = false,
                IsAttachmentOnly = true,
                CreatedAt        = DateTime.UtcNow
            };
            _repo.AddComment(timelineComment);

            // Flush to get the comment PK before linking the attachment
            await _repo.SaveChangesAsync();

            // ── Persist attachment record ─────────────────────────────────────
            var record = new TicketAttachment
            {
                TicketId         = ticketId,
                FileName         = file.FileName,
                FilePath         = virtualPath,
                StoredFileName   = storedFileName,
                FileSize         = file.Length,
                FileType         = file.ContentType,
                UploadedByUserId = userId,
                CommentId        = timelineComment.Id,
                UploadedAt       = DateTime.UtcNow
            };

            _repo.AddAttachment(record);

            _repo.AddActivityLog(new ActivityLog
            {
                UserId   = userId,
                TicketId = ticketId,
                Action   = "Attachment Uploaded",
                Details  = $"File '{file.FileName}' uploaded to ticket {ticket.ReferenceNumber}",
                LoggedAt = DateTime.UtcNow
            });

            await _repo.SaveChangesAsync();

            // Notify all other participants (bell only)
            var participants = await _repo.GetTicketParticipantIdsAsync(ticketId, userId);
            if (participants.Count > 0)
            {
                await _notifications.NotifyAsync(
                    type:           NotificationType.AttachmentAdded,
                    ticketId:       ticketId,
                    ticketRef:      ticket.ReferenceNumber,
                    ticketTitle:    ticket.Title,
                    recipientUserIds: participants,
                    message:        $"{uploaderName} attached '{file.FileName}' to ticket {ticket.ReferenceNumber} \"{ticket.Title}\".",
                    sendEmail:      false);
            }

            return (new TicketAttachmentDto
            {
                Id               = record.Id,
                FileName         = record.FileName,
                FilePath         = record.FilePath,
                FileSize         = record.FileSize,
                FileType         = record.FileType,
                UploadedByUserId = record.UploadedByUserId,
                UploadedBy       = uploaderName,
                CommentId        = record.CommentId,
                UploadedAt       = record.UploadedAt
            }, null);
        }

        // ── GET ATTACHMENT STREAM (download / preview) ────────────────────────

        public async Task<(Stream? stream, string contentType, string fileName, string? error)> GetAttachmentStreamAsync(
            int ticketId, int attachmentId, int userId, string role, bool inline)
        {
            // Verify ticket access (same rules as GetTicketByIdAsync)
            var ticket = await _repo.GetTicketWithStatusAsync(ticketId);
            if (ticket == null) return (null, string.Empty, string.Empty, "Ticket not found.");

            if (role == "Employee" && ticket.CreatedByUserId != userId)
                return (null, string.Empty, string.Empty, "Access denied.");
            if (role == "Agent" && ticket.AssignedToUserId != userId)
                return (null, string.Empty, string.Empty, "Access denied.");
            if (role == "Manager")
            {
                var deptId = await _repo.GetUserDepartmentIdAsync(userId);
                if (deptId != ticket.DepartmentId)
                    return (null, string.Empty, string.Empty, "Access denied.");
            }

            var attachment = await _repo.FindAttachmentAsync(attachmentId);
            if (attachment == null || attachment.TicketId != ticketId)
                return (null, string.Empty, string.Empty, "Attachment not found.");

            var (stream, contentType) = await _fileStorage.GetFileStreamAsync(
                attachment.StoredFileName, ticketId, attachment.FilePath);

            if (stream == null)
                return (null, string.Empty, string.Empty, "File not found on disk.");

            // For preview: return as-is with Content-Type (browser renders inline)
            // For download: caller sets Content-Disposition: attachment
            return (stream, contentType, attachment.FileName, null);
        }

        // ── AGENTS AVAILABILITY (Manager assign helper) ───────────────────────

        public async Task<(IEnumerable<AgentAvailabilityDto> agents, string? error)> GetAgentsAvailabilityAsync(
            int ticketId, int managerId)
        {
            var ticket = await _repo.FindTicketAsync(ticketId);
            if (ticket == null)
                return (Enumerable.Empty<AgentAvailabilityDto>(), "Ticket not found.");

            var managerDeptId = await _repo.GetUserDepartmentIdAsync(managerId);
            if (managerDeptId != ticket.DepartmentId)
                return (Enumerable.Empty<AgentAvailabilityDto>(), "Ticket does not belong to your department.");

            var agentRoleId = await _repo.GetAgentRoleIdAsync();
            var agents      = await _repo.GetAgentsInDepartmentAsync(ticket.DepartmentId!.Value, agentRoleId);
            var agentIds    = agents.Select(a => a.Id).ToList();
            var weekStart   = DateTime.UtcNow.Date.AddDays(-6);

            var agentTickets = await _repo.GetTicketDataForAgentsAsync(agentIds);

            var result = agents.Select(agent =>
            {
                var openCount = agentTickets.Count(t =>
                    t.AgentId == agent.Id &&
                    t.StatusName != "Resolved");

                var resolvedThisWeek = agentTickets.Count(t =>
                    t.AgentId == agent.Id &&
                    t.StatusName == "Resolved" &&
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

        // ── HELPERS ───────────────────────────────────────────────────────────

        /// <summary>
        /// Returns true when the file's magic bytes reveal it is a forbidden executable type,
        /// regardless of what extension the client declared.
        /// </summary>
        private static bool HasForbiddenMagicBytes(byte[] header, int bytesRead)
        {
            if (bytesRead < 2) return false;

            // MZ signature — Windows executables: EXE, DLL, COM, etc.
            if (header[0] == 0x4D && header[1] == 0x5A)
                return true;

            if (bytesRead < 4) return false;

            // ELF signature — Linux / Unix executables
            if (header[0] == 0x7F && header[1] == 0x45 && header[2] == 0x4C && header[3] == 0x46)
                return true;

            // Mach-O thin binary — macOS executables (big-endian)
            if (header[0] == 0xFE && header[1] == 0xED && header[2] == 0xFA && header[3] == 0xCE)
                return true;

            // Mach-O thin binary — macOS executables (little-endian)
            if (header[0] == 0xCE && header[1] == 0xFA && header[2] == 0xED && header[3] == 0xFE)
                return true;

            return false;
        }
    }
}
