namespace backend.Application.Common
{
    /// <summary>Shared file upload allow-list, used by both ticket attachments and profile pictures.</summary>
    public static class FileValidationRules
    {
        // ── Allowed extensions (server-side allow-list) ───────────────────────
        public static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            // Images
            ".png", ".jpg", ".jpeg", ".gif", ".webp",
            // Documents
            ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".pptx", ".ppt", ".txt", ".csv",
            // Logs
            ".log"
        };

        // ── Explicitly blocked extensions ─────────────────────────────────────
        public static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".exe", ".php", ".sh", ".bat", ".cmd", ".js", ".vbs", ".ps1",
            ".dll", ".msi", ".jar", ".py", ".asp", ".aspx", ".jsp"
        };

        /// <summary>
        /// Returns true when the file's magic bytes reveal it is a forbidden executable type,
        /// regardless of what extension the client declared.
        /// </summary>
        public static bool HasForbiddenMagicBytes(byte[] header, int bytesRead)
        {
            if (bytesRead < 2) return false;

            // MZ signature — Windows executables: EXE, DLL, COM, etc.
            if (header[0] == 0x4D && header[1] == 0x5A)
                return true;

            if (bytesRead < 4) return false;

            // ELF signature — Linux / Unix executables
            if (header[0] == 0x7F && header[1] == 0x45 && header[2] == 0x4C && header[3] == 0x46)
                return true;

            // Mach-O thin binary — macOS executables (big-endian)
            if (header[0] == 0xFE && header[1] == 0xED && header[2] == 0xFA && header[3] == 0xCE)
                return true;

            // Mach-O thin binary — macOS executables (little-endian)
            if (header[0] == 0xCE && header[1] == 0xFA && header[2] == 0xED && header[3] == 0xFE)
                return true;

            return false;
        }
    }
}
