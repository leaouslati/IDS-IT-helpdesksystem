using backend.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace backend.Application.Interfaces
{
    public interface IProfileService
    {
        Task<ProfileDto?> GetProfileAsync(int userId);
        Task<(bool success, Dictionary<string, string>? errors)> UpdateProfileAsync(int userId, UpdateProfileDto dto);
        Task<(bool success, string? error)> ChangePasswordAsync(int userId, ChangePasswordDto dto);
        Task<(string? profilePictureUrl, string? error)> UploadPictureAsync(int userId, IFormFile? file);
        Task<(bool success, string? error)> DeletePictureAsync(int userId);
        Task<(Stream? stream, string contentType, string? error)> GetPictureStreamAsync(int userId);
    }
}
