using backend.Application.DTOs;

namespace backend.Application.Interfaces
{
    public interface ILookupRepository
    {
        Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
        Task<IEnumerable<PriorityDto>> GetPrioritiesAsync();
        Task<IEnumerable<TicketStatusDto>> GetStatusesAsync();
    }
}
