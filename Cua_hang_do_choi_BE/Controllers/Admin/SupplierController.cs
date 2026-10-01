
using Application.IService;
using Application.Model.Supplier;
using Microsoft.AspNetCore.Mvc;

namespace Cua_hang_do_choi_BE.Controllers.Admin;

[ApiController]
[Route("api/admin/suppliers")]
public class SupplierController : ControllerBase
{
    private readonly ISupplierService _service;

    public SupplierController(ISupplierService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);

        if (result == null)
            return NotFound(new { message = "Không tìm thấy nhà cung cấp." });

        return Ok(result);
    }

    [HttpGet("deleted")]
    public async Task<IActionResult> GetDeleted(
        CancellationToken cancellationToken)
    {
        var result = await _service.GetDeletedAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var createdBy = Request.Headers["X-User"].FirstOrDefault() ?? "system";

            var result = await _service.CreateAsync(
                request,
                createdBy,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updatedBy = Request.Headers["X-User"].FirstOrDefault() ?? "system";

            var result = await _service.UpdateAsync(
                id,
                request,
                updatedBy,
                cancellationToken);

            if (result == null)
                return NotFound(new { message = "Không tìm thấy nhà cung cấp." });

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deletedBy = Request.Headers["X-User"].FirstOrDefault() ?? "system";

        var success = await _service.DeleteAsync(
            id,
            deletedBy,
            cancellationToken);

        if (!success)
            return NotFound(new { message = "Không tìm thấy nhà cung cấp." });

        return NoContent();
    }

    [HttpPut("{id:guid}/restore")]
    public async Task<IActionResult> Restore(
        Guid id,
        CancellationToken cancellationToken)
    {
        var success = await _service.RestoreAsync(id, cancellationToken);

        if (!success)
            return NotFound(new { message = "Không tìm thấy nhà cung cấp đã xóa." });

        return NoContent();
    }
}
