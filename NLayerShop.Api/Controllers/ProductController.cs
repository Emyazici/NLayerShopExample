using Microsoft.AspNetCore.Mvc;
using NLayerShop.Business.Interfaces;
using NLayerShop.Contracts.Common;
using NLayerShop.Contracts.DTOs.Products;

namespace NLayerShop.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _svc;
        public ProductsController(IProductService svc)
        {
            _svc = svc;
        }

        [HttpGet]
        public Task<PagedResponse<ProductDetailDto>> Get([FromQuery] PagedRequest req, CancellationToken ct)
        {
            return _svc.GetPagedAsync(req, ct);
        }

        [HttpGet("{id:int}")]
        public Task<ProductDetailDto> GetById(int id, CancellationToken ct)
        {
            return _svc.GetByIdAsync(id, ct);
        }

        [HttpPost]
        public async Task<ActionResult<ProductDetailDto>> Create([FromBody] ProductCreateDto dto, CancellationToken ct)
        {
            var created = await _svc.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto dto, CancellationToken ct)
        {
            await _svc.UpdateAsync(id, dto, ct);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _svc.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
