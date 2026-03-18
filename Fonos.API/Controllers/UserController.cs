using Fonos.API.DTOs.Payments;
using Fonos.API.Services.Payments;
using Fonos.API.Services.Users;
using Microsoft.AspNetCore.Mvc;

namespace Fonos.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IPaymentService _paymentService;
        public UserController(IUserService userService, IPaymentService paymentService)
        {
            _userService = userService;
            _paymentService = paymentService;
        }

        [HttpGet("{userId}/payments")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetUserPayments(string userId)
        {
            var payments = await _paymentService.GetUserPaymentsAsync(userId);
            return Ok(payments);
        }
    }
}
