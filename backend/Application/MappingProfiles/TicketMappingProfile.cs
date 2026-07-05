using AutoMapper;
using backend.Application.DTOs;
using backend.Domain.Entities;

namespace backend.Application.MappingProfiles
{
    public class TicketMappingProfile : Profile
    {
        public TicketMappingProfile()
        {
            // TicketComment → TicketCommentDto
            // ThenInclude on c.User.Role is required to populate UserRole for chat-bubble alignment
            CreateMap<TicketComment, TicketCommentDto>()
                .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.FirstName + " " + s.User.LastName))
                .ForMember(d => d.UserRole, o => o.MapFrom(s => s.User.Role != null ? s.User.Role.Name : string.Empty));

            // ActivityLog → TicketActivityLogDto
            CreateMap<ActivityLog, TicketActivityLogDto>()
                .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.FirstName + " " + s.User.LastName))
                .ForMember(d => d.UserRole, o => o.MapFrom(s => s.User.Role != null ? s.User.Role.Name : null));

            // TicketAttachment → TicketAttachmentDto
            CreateMap<TicketAttachment, TicketAttachmentDto>()
                .ForMember(d => d.UploadedBy, o => o.MapFrom(s =>
                    s.UploadedByUser.FirstName + " " + s.UploadedByUser.LastName));

            // Ticket → TicketSummaryDto (used in list queries — note: CommentCount is set separately)
            CreateMap<Ticket, TicketSummaryDto>()
                .ForMember(d => d.Category, o => o.MapFrom(s => s.Category.Name))
                .ForMember(d => d.Priority, o => o.MapFrom(s => s.Priority.Name))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.TicketStatus.Name))
                .ForMember(d => d.CreatedBy, o => o.MapFrom(s =>
                    s.CreatedByUser.FirstName + " " + s.CreatedByUser.LastName))
                .ForMember(d => d.AssignedTo, o => o.MapFrom(s =>
                    s.AssignedToUser != null
                        ? s.AssignedToUser.FirstName + " " + s.AssignedToUser.LastName
                        : null))
                .ForMember(d => d.CommentCount, o => o.MapFrom(s => s.Comments.Count));

            // TicketHoursLog → HoursLogResponseDto
            CreateMap<TicketHoursLog, HoursLogResponseDto>()
                .ForMember(d => d.AgentName, o => o.MapFrom(s =>
                    s.Agent.FirstName + " " + s.Agent.LastName));
        }
    }
}
