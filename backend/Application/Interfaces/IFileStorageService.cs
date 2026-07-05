namespace backend.Application.Interfaces
{
    public interface IFileStorageService
    {
        /// <summary>Saves a file stream to disk and returns (storedFileName, virtualPath).</summary>
        Task<(string storedFileName, string virtualPath)> SaveFileAsync(
            Stream stream, int ticketId, string originalFileName, string extension);

        /// <summary>
        /// Opens a read stream for the requested file.
        /// Pass storedFileName + ticketId for new-style storage, or legacyFilePath for old wwwroot files.
        /// </summary>
        Task<(Stream? stream, string contentType)> GetFileStreamAsync(
            string storedFileName, int ticketId, string? legacyFilePath = null);

        /// <summary>Deletes the physical file from disk (no-op if missing).</summary>
        void DeleteFile(string storedFileName, int ticketId);

        /// <summary>Saves a profile picture under profiles/{userId}/ and returns (storedFileName, virtualPath).</summary>
        Task<(string storedFileName, string virtualPath)> SaveProfilePictureAsync(
            Stream stream, int userId, string originalFileName, string extension);

        /// <summary>Opens a read stream for a user's stored profile picture.</summary>
        Task<(Stream? stream, string contentType)> GetProfilePictureStreamAsync(string storedFileName, int userId);

        /// <summary>Deletes the physical profile picture file from disk (no-op if missing).</summary>
        void DeleteProfilePicture(string storedFileName, int userId);

        /// <summary>Total bytes used by all stored uploads (tickets + profiles).</summary>
        long GetTotalStorageUsedBytes();
    }
}
