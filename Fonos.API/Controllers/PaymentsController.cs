using Fonos.API.DTOs.Payments;
using Fonos.API.Services.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fonos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<PaymentDto>> Create([FromBody] PaymentCreateDto command)
        {
            var userId = User.FindFirst("uid")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không tìm thấy thông tin định danh trong Token.");
            }

            var secureCommand = command with { UserId = userId };

            var result = await _paymentService.CreatePaymentAsync(secureCommand);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<PaymentDto>> GetById(Guid id)
        {
            return Ok(await _paymentService.GetPaymentAsync(id));
        }

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> Complete(Guid id)
        {
            await _paymentService.CompletePaymentAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            await _paymentService.CancelPaymentAsync(id);
            return NoContent();
        }
    }
}