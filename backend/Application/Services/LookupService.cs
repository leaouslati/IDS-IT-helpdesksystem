using backend.Application.DTOs;
using backend.Application.Interfaces;

namespace backend.Application.Services
{
    public class LookupService : ILookupService
    {
        private readonly ILookupRepository _repo;

        public LookupService(ILookupRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<CategoryDto>> GetCategoriesAsync() =>
            _repo.GetCategoriesAsync();

        public Task<IEnumerable<PriorityDto>> GetPrioritiesAsync() =>
            _repo.GetPrioritiesAsync();

        public Task<IEnumerable<TicketStatusDto>> GetStatusesAsync() =>
            _repo.GetStatusesAsync();
    }
}
