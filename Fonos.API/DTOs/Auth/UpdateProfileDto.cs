namespace Fonos.API.DTOs.Auth
{
    public class UpdateProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }
}
