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
        public Task<PagedResponse<OrderListItemDto>> Get([FromQuery] OrderQueryRequest req, CancellationToken ct)
        {
            return _svc.GetPagedAsync(req, ct);
        }

        [HttpGet("{id:int}")]
        public Task<OrderDetailDto> GetById(int id, CancellationToken ct)
        {
            return _svc.GetByIdAsync(id, ct);
        }

        [HttpPost]
        public async Task<ActionResult<OrderDetailDto>> Create([FromBody] OrderCreateDto dto, CancellationToken ct)
        {
            var created = await _svc.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // Siparisler silinmez; yasam dongusu bu iki aksiyonla yonetilir (Pending -> Cancelled | Completed).
        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id, CancellationToken ct)
        {
            await _svc.CancelAsync(id, ct);
            return NoContent();
        }

        [HttpPost("{id:int}/complete")]
        public async Task<IActionResult> Complete(int id, CancellationToken ct)
        {
            await _svc.CompleteAsync(id, ct);
            return NoContent();
        }
    }
}
