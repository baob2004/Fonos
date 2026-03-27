using Fonos.API.DTOs.Auth;
using Fonos.API.DTOs.Payments;
using Fonos.API.Models;
using Fonos.API.Services.Payments;
using Fonos.API.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fonos.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IPaymentService _paymentService;
        private readonly UserManager<ApplicationUser> _userManager;
        public UserController(IUserService userService, IPaymentService paymentService, UserManager<ApplicationUser> userManager)
        {
            _userService = userService;
            _paymentService = paymentService;
            _userManager = userManager;
        }

        [HttpGet("{userId}/payments")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetUserPayments(string userId)
        {
            var payments = await _paymentService.GetUserPaymentsAsync(userId);
            return Ok(payments);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult> RegisterAsync(RegisterModel model)
        {

            var result = await _userService.RegisterAsync(model);
            return Ok(result);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTokenAsync(TokenRequestModel model)
        {
            var result = await _userService.GetTokenAsync(model);
            return Ok(result);
        }

        [HttpPost("addrole")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AddRoleAsync(AddRoleModel model)
        {
            var result = await _userService.AddRoleAsync(model);
            return Ok(result);
        }

        [HttpGet("me")]
        [Authorize] 
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var userId = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Message = "Token không hợp lệ hoặc đã hết hạn." });
            }

            var userDto = await _userService.GetCurrentUserAsync(userId);

            if (userDto == null)
            {
                return NotFound(new { Message = "Không tìm thấy thông tin người dùng." });
            }

            return Ok(userDto);
        }

        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto model)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Gọi service xử lý
            var result = await _userService.UpdateProfileAsync(userId, model.FullName, model.AvatarFile);

            if (result.Contains("successfully"))
            {
                // Lấy lại user để trả về AvatarUrl mới cho Frontend cập nhật ngay
                var user = await _userManager.FindByIdAsync(userId);
                return Ok(new { message = result, avatarUrl = user.AvatarUrl });
            }

            return BadRequest(new { message = result });
        }
        public class UpdateProfileDto
        {
            public string FullName { get; set; } = string.Empty;
            public IFormFile? AvatarFile { get; set; }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            var userId = User.FindFirstValue("uid");
            var result = await _userService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);

            if (result.Contains("successfully")) return Ok(new { message = result });
            return BadRequest(new { message = result });
        }
    }
}
