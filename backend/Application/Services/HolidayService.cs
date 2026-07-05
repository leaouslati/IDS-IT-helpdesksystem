using backend.Application.DTOs;
using backend.Application.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Services
{
    public class HolidayService : IHolidayService
    {
        private readonly IHolidayRepository _repo;

        public HolidayService(IHolidayRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<HolidayDto>> GetHolidaysForYearAsync(int year)
        {
            var all = await _repo.GetAllAsync();

            return all
                .Where(h => h.IsRecurring || h.Date.Year == year)
                .Select(h => ToDto(h, year))
                .OrderBy(h => h.Date)
                .ToList();
        }

        public async Task<(HolidayDto? holiday, string? error, bool isPastDate)> CreateAsync(CreateHolidayDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return (null, "Holiday name is required.", false);
            if (dto.Name.Trim().Length > 100)
                return (null, "Holiday name cannot exceed 100 characters.", false);

            var date = dto.Date.Date;
            var isPastDate = date < DateTime.UtcNow.Date;

            var holiday = new Holiday
            {
                Name        = dto.Name.Trim(),
                Date        = date,
                IsRecurring = dto.IsRecurring,
                CreatedAt   = DateTime.UtcNow
            };

            _repo.Add(holiday);
            await _repo.SaveChangesAsync();

            return (ToDto(holiday, holiday.Date.Year), null, isPastDate);
        }

        public async Task<(bool success, string? error)> UpdateAsync(int id, UpdateHolidayDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return (false, "Holiday name is required.");
            if (dto.Name.Trim().Length > 100)
                return (false, "Holiday name cannot exceed 100 characters.");

            var holiday = await _repo.FindAsync(id);
            if (holiday == null) return (false, "Holiday not found.");

            holiday.Name        = dto.Name.Trim();
            holiday.Date        = dto.Date.Date;
            holiday.IsRecurring = dto.IsRecurring;

            await _repo.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool success, string? error)> DeleteAsync(int id)
        {
            var holiday = await _repo.FindAsync(id);
            if (holiday == null) return (false, "Holiday not found.");

            _repo.Remove(holiday);
            await _repo.SaveChangesAsync();
            return (true, null);
        }

        // For a recurring holiday, project its stored month/day onto the requested year
        // (Feb 29 clamps to Feb 28 on non-leap years)
        private static HolidayDto ToDto(Holiday h, int year) => new()
        {
            Id          = h.Id,
            Name        = h.Name,
            Date        = h.IsRecurring
                ? new DateTime(year, h.Date.Month, Math.Min(h.Date.Day, DateTime.DaysInMonth(year, h.Date.Month)))
                : h.Date,
            IsRecurring = h.IsRecurring
        };
    }
}
