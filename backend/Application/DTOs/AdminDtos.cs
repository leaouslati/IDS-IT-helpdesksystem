namespace backend.Application.DTOs
{
    public class CreateUserDto
    {
        public string FirstName    { get; set; } = string.Empty;
        public string LastName     { get; set; } = string.Empty;
        public string Email        { get; set; } = string.Empty;
        public string Password     { get; set; } = string.Empty;
        public int    RoleId       { get; set; }
        public int?   DepartmentId { get; set; }
    }

    public class UpdateUserRoleDto
    {
        public int RoleId { get; set; }
    }

    public class UserListDto
    {
        public int      Id           { get; set; }
        public string   FirstName    { get; set; } = string.Empty;
        public string   LastName     { get; set; } = string.Empty;
        public string   Email        { get; set; } = string.Empty;
        public string   Role         { get; set; } = string.Empty;
        public int      RoleId       { get; set; }
        public string?  Department   { get; set; }
        public int?     DepartmentId { get; set; }
        public bool     IsActive     { get; set; }
        public DateTime CreatedAt    { get; set; }
    }

    public class RoleLookupDto
    {
        public int    Id   { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class DepartmentLookupDto
    {
        public int    Id   { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
