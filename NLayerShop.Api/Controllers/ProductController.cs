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
        [ProducesResponseType(typeof(PagedResponse<ProductDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
        public Task<PagedResponse<ProductDetailDto>> Get([FromQuery] ProductQueryRequest req, CancellationToken ct)
        {
            return _svc.GetPagedAsync(req, ct);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
        public Task<ProductDetailDto> GetById(int id, CancellationToken ct)
        {
            return _svc.GetByIdAsync(id, ct);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
        public async Task<ActionResult<ProductDetailDto>> Create([FromBody] ProductCreateDto dto, CancellationToken ct)
        {
            var created = await _svc.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict, "application/problem+json")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto dto, CancellationToken ct)
        {
            await _svc.UpdateAsync(id, dto, ct);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _svc.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
