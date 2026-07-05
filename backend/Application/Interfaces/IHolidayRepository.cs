using backend.Domain.Entities;

namespace backend.Application.Interfaces
{
    public interface IHolidayRepository
    {
        Task<List<Holiday>> GetAllAsync();
        Task<Holiday?> FindAsync(int id);
        void Add(Holiday holiday);
        void Remove(Holiday holiday);
        Task SaveChangesAsync();
    }
}
