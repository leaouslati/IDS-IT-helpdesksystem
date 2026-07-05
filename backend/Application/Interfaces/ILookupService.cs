using backend.Application.DTOs;

namespace backend.Application.Interfaces
{
    public interface ILookupService
    {
        Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
        Task<IEnumerable<PriorityDto>> GetPrioritiesAsync();
        Task<IEnumerable<TicketStatusDto>> GetStatusesAsync();
        Task<IEnumerable<DepartmentLookupDto>> GetDepartmentsAsync();
    }
}
