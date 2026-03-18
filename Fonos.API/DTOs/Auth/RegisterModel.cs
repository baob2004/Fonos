using System.ComponentModel.DataAnnotations;

namespace Fonos.API.DTOs.Auth
{
    public record RegisterModel(
            [Required] string FullName,
            [Required] string Username,
            [Required][EmailAddress] string Email,
            [Required][MinLength(6)] string Password
    );
}
