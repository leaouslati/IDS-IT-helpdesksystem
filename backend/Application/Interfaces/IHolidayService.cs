using backend.Application.DTOs;

namespace backend.Application.Interfaces
{
    public interface IHolidayService
    {
        Task<List<HolidayDto>> GetHolidaysForYearAsync(int year);
        Task<(HolidayDto? holiday, string? error, bool isPastDate)> CreateAsync(CreateHolidayDto dto);
        Task<(bool success, string? error)> UpdateAsync(int id, UpdateHolidayDto dto);
        Task<(bool success, string? error)> DeleteAsync(int id);
    }
}
