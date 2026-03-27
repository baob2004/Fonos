using Fonos.API.DTOs.Auth;

namespace Fonos.API.Services.Users
{
    public interface IUserService
    {
        Task<string> RegisterAsync(RegisterModel model);
        Task<AuthenticationModel> GetTokenAsync(TokenRequestModel model);
        Task<string> AddRoleAsync(AddRoleModel model);
        Task<UserDto?> GetCurrentUserAsync(string userId);
        Task<string> UpdateProfileAsync(string userId, string fullName, IFormFile? avatarFile);
        Task<string> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    }
}
