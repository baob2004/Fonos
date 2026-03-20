namespace Fonos.API.DTOs.Auth
{
    public record UserDto(string Id, string FullName, string? AvatarUrl, string Email);
}
