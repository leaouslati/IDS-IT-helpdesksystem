namespace backend.Application.DTOs
{
    public class ProfileDto
    {
        public int      Id                { get; set; }
        public string   FirstName         { get; set; } = string.Empty;
        public string   LastName          { get; set; } = string.Empty;
        public string   Email             { get; set; } = string.Empty;
        public string   Role              { get; set; } = string.Empty;
        public int?     DepartmentId      { get; set; }
        public string?  Department        { get; set; }
        public string?  ProfilePictureUrl { get; set; }
        public DateTime CreatedAt         { get; set; }
    }

    public class UpdateProfileDto
    {
        public string FirstName    { get; set; } = string.Empty;
        public string LastName     { get; set; } = string.Empty;
        public string Email        { get; set; } = string.Empty;
        public int?   DepartmentId { get; set; }
    }

    public class ChangePasswordDto
    {
        public string CurrentPassword    { get; set; } = string.Empty;
        public string NewPassword        { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
