using backend.Application.Common;
using backend.Application.DTOs;
using backend.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace backend.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository  _repo;
        private readonly IFileStorageService _fileStorage;
        private readonly int                 _maxFileSizeBytes;

        public ProfileService(
            IProfileRepository repo,
            IFileStorageService fileStorage,
            IConfiguration config)
        {
            _repo             = repo;
            _fileStorage      = fileStorage;
            var maxMb         = config.GetValue<int>("FileUpload:MaxSizeMB", 10);
            _maxFileSizeBytes = maxMb * 1024 * 1024;
        }

        public async Task<ProfileDto?> GetProfileAsync(int userId)
        {
            var user = await _repo.FindUserWithDetailsAsync(userId);
            if (user == null) return null;

            return new ProfileDto
            {
                Id                = user.Id,
                FirstName         = user.FirstName,
                LastName          = user.LastName,
                Email             = user.Email,
                Role              = user.Role.Name,
                DepartmentId      = user.DepartmentId,
                Department        = user.Department?.Name,
                ProfilePictureUrl = user.ProfilePictureUrl,
                CreatedAt         = user.CreatedAt
            };
        }

        public async Task<(bool success, Dictionary<string, string>? errors)> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(dto.FirstName))
                errors["firstName"] = "First name is required.";
            if (string.IsNullOrWhiteSpace(dto.LastName))
                errors["lastName"] = "Last name is required.";

            if (string.IsNullOrWhiteSpace(dto.Email))
                errors["email"] = "Email is required.";
            else if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                errors["email"] = "Enter a valid email address.";

            var user = await _repo.FindUserWithDetailsAsync(userId);
            if (user == null)
            {
                errors["general"] = "User not found.";
                return (false, errors);
            }

            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            if (!errors.ContainsKey("email") &&
                normalizedEmail != user.Email &&
                await _repo.EmailExistsAsync(normalizedEmail, userId))
            {
                errors["email"] = "This email is already in use by another account.";
            }

            if (dto.DepartmentId.HasValue && !await _repo.DepartmentExistsAsync(dto.DepartmentId.Value))
                errors["departmentId"] = "Invalid department selected.";

            if (errors.Count > 0)
                return (false, errors);

            user.FirstName    = dto.FirstName.Trim();
            user.LastName     = dto.LastName.Trim();
            user.Email        = normalizedEmail;
            user.DepartmentId = dto.DepartmentId;

            await _repo.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool success, string? error)> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _repo.FindUserWithDetailsAsync(userId);
            if (user == null) return (false, "User not found.");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                return (false, "Current password is incorrect.");

            if (dto.NewPassword != dto.ConfirmNewPassword)
                return (false, "New password and confirmation do not match.");

            if (!IsPasswordStrongEnough(dto.NewPassword))
                return (false, "New password must be at least 8 characters and include an uppercase letter, a lowercase letter, and a number.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _repo.SaveChangesAsync();
            return (true, null);
        }

        private static bool IsPasswordStrongEnough(string password) =>
            password.Length >= 8 &&
            password.Any(char.IsUpper) &&
            password.Any(char.IsLower) &&
            password.Any(char.IsDigit);

        public async Task<(string? profilePictureUrl, string? error)> UploadPictureAsync(int userId, IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return (null, "No file provided.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (FileValidationRules.BlockedExtensions.Contains(ext))
                return (null, $"File type '{ext}' is explicitly blocked for security reasons.");
            if (!FileValidationRules.AllowedExtensions.Contains(ext))
                return (null, $"File type '{ext}' is not allowed.");
            if (file.Length > _maxFileSizeBytes)
                return (null, $"File size exceeds the {_maxFileSizeBytes / (1024 * 1024)} MB limit.");

            await using var peekStream = file.OpenReadStream();
            var header = new byte[8];
            var read   = await peekStream.ReadAsync(header.AsMemory(0, header.Length));
            if (FileValidationRules.HasForbiddenMagicBytes(header, read))
                return (null, "File content does not match the declared file type. Executable content is not allowed.");

            var user = await _repo.FindUserWithDetailsAsync(userId);
            if (user == null) return (null, "User not found.");

            // Remove the previous picture, if any, before saving the new one
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                var previousStoredName = Path.GetFileName(user.ProfilePictureUrl);
                _fileStorage.DeleteProfilePicture(previousStoredName, userId);
            }

            peekStream.Seek(0, SeekOrigin.Begin);
            var (_, virtualPath) = await _fileStorage.SaveProfilePictureAsync(peekStream, userId, file.FileName, ext);

            user.ProfilePictureUrl = virtualPath;
            await _repo.SaveChangesAsync();

            return (virtualPath, null);
        }

        public async Task<(bool success, string? error)> DeletePictureAsync(int userId)
        {
            var user = await _repo.FindUserWithDetailsAsync(userId);
            if (user == null) return (false, "User not found.");

            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                var storedName = Path.GetFileName(user.ProfilePictureUrl);
                _fileStorage.DeleteProfilePicture(storedName, userId);
            }

            user.ProfilePictureUrl = null;
            await _repo.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(Stream? stream, string contentType, string? error)> GetPictureStreamAsync(int userId)
        {
            var user = await _repo.FindUserWithDetailsAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.ProfilePictureUrl))
                return (null, string.Empty, "No profile picture set.");

            var storedName = Path.GetFileName(user.ProfilePictureUrl);
            var (stream, contentType) = await _fileStorage.GetProfilePictureStreamAsync(storedName, userId);
            if (stream == null) return (null, string.Empty, "Profile picture file not found.");

            return (stream, contentType, null);
        }
    }
}
