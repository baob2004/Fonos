using System.ComponentModel.DataAnnotations;

namespace Fonos.API.DTOs.Auth
{
    public record TokenRequestModel(
            [Required][EmailAddress] string Email,
            [Required] string Password
    );
}
