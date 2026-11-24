using Microsoft.AspNetCore.Mvc;
using NLayerShop.Business.Interfaces;
using NLayerShop.Contracts.Common;
using NLayerShop.Contracts.DTOs.Orders;

namespace NLayerShop.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _svc;
        public OrdersController(IOrderService svc)
        {
            _svc = svc;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<OrderListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
        public Task<PagedResponse<OrderListItemDto>> Get([FromQuery] OrderQueryRequest req, CancellationToken ct)
        {
            return _svc.GetPagedAsync(req, ct);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(OrderDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
        public Task<OrderDetailDto> GetById(int id, CancellationToken ct)
        {
            return _svc.GetByIdAsync(id, ct);
        }

        [HttpPost]
        [ProducesResponseType(typeof(OrderDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict, "application/problem+json")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
        public async Task<ActionResult<OrderDetailDto>> Create([FromBody] OrderCreateDto dto, CancellationToken ct)
        {
            var created = await _svc.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // Siparisler silinmez; yasam dongusu bu iki aksiyonla yonetilir (Pending -> Cancelled | Completed).
        [HttpPost("{id:int}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict, "application/problem+json")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
        public async Task<IActionResult> Cancel(int id, CancellationToken ct)
        {
            await _svc.CancelAsync(id, ct);
            return NoContent();
        }

        [HttpPost("{id:int}/complete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict, "application/problem+json")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
        public async Task<IActionResult> Complete(int id, CancellationToken ct)
        {
            await _svc.CompleteAsync(id, ct);
            return NoContent();
        }
    }
}
