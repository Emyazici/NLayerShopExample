using Microsoft.AspNetCore.Mvc;
using NLayerShop.Business.Interfaces;
using NLayerShop.Contracts.Common;
using NLayerShop.Contracts.DTOs.Categories;

namespace NLayerShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _svc;
    public CategoriesController(ICategoryService svc) => _svc = svc;

    [HttpGet]
    public Task<PagedResponse<CategoryDetailDto>> Get([FromQuery] PagedRequest req, CancellationToken ct)
        => _svc.GetPagedAsync(req, ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDetailDto>> GetById(int id, CancellationToken ct)
    {
        var dto = await _svc.GetByIdAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto, CancellationToken ct)
    {
        var id = await _svc.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateDto dto, CancellationToken ct)
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
