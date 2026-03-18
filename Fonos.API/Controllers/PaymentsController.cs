using Fonos.API.DTOs.Payments;
using Fonos.API.Services.Payments;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<PaymentDto>> Create([FromBody] PaymentCreateDto command)
        {
            var result = await _paymentService.CreatePaymentAsync(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("{id}")]
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