using backend.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace backend.Application.Interfaces
{
    public interface ITicketService
    {
        Task<IEnumerable<TicketSummaryDto>> GetTicketsAsync(int userId, string role);
        Task<TicketDetailDto?> GetTicketByIdAsync(int ticketId, int userId, string role);
        Task<(TicketDetailDto? ticket, string? error)> CreateTicketAsync(CreateTicketDto dto, int userId);
        Task<(bool success, string? error)> UpdateTicketAsync(int ticketId, UpdateTicketDto dto, int userId);
        Task<(bool success, string? error)> DeleteTicketAsync(int ticketId, int userId, string role);
        Task<(bool success, string? error)> AssignTicketAsync(int ticketId, AssignTicketDto dto, int managerId);
        Task<(bool success, string? error)> UpdateStatusAsync(int ticketId, UpdateTicketStatusDto dto, int agentId);
        Task<(bool success, string? error)> EscalateTicketAsync(int ticketId, EscalateTicketDto dto, int userId, string role);
        Task<(TicketCommentDto? comment, string? error)> AddCommentAsync(int ticketId, AddCommentDto dto, int userId, string role);
        Task<IEnumerable<TicketCommentDto>> GetCommentsAsync(int ticketId, int userId, string role);
        Task<(TicketAttachmentDto? attachment, string? error)> UploadAttachmentAsync(int ticketId, IFormFile? file, int userId, string role);
        Task<(Stream? stream, string contentType, string fileName, string? error)> GetAttachmentStreamAsync(
            int ticketId, int attachmentId, int userId, string role, bool inline);
        Task<(IEnumerable<AgentAvailabilityDto> agents, string? error)> GetAgentsAvailabilityAsync(int ticketId, int managerId);
    }
}
