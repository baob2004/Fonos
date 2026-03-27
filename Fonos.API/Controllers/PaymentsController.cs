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

        [HttpPost("create-vnpay-url")]
        [Authorize]
        public async Task<IActionResult> CreateUrl([FromBody] Guid bookId)
        {
            var userId = User.FindFirst("uid")?.Value;
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

            try
            {
                var url = await _paymentService.CreateVnPayUrlAsync(bookId, userId, ipAddress);
                return Ok(new { paymentUrl = url });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("vnpay-return")]
        [Authorize]
        public async Task<IActionResult> VnPayReturn()
        {
            var vnpayData = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());

            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized("Không tìm thấy UserId trong Token.");

            try
            {
                var result = await _paymentService.ProcessVnPayCallbackAsync(vnpayData, userId);

                return Ok(result); 
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}