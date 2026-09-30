using Application.IService;
using Application.Model.Warehouse;
using Microsoft.AspNetCore.Mvc;

namespace Cua_hang_do_choi_BE.Controllers.Admin;

[ApiController]
[Route("api/admin/warehouses")]
public class WarehouseController : ControllerBase
{
	private readonly IWarehouseService _warehouseService;

	public WarehouseController(IWarehouseService warehouseService)
	{
		_warehouseService = warehouseService;
	}

	// GET: api/admin/warehouses
	[HttpGet]
	public async Task<IActionResult> GetAll(
		CancellationToken cancellationToken)
	{
		var result = await _warehouseService.GetAllAsync(
			cancellationToken);

		return Ok(result);
	}

	// GET: api/admin/warehouses/{id}
	[HttpGet("{id:guid}")]
	public async Task<IActionResult> GetById(
		Guid id,
		CancellationToken cancellationToken)
	{
		var result = await _warehouseService.GetByIdAsync(
			id,
			cancellationToken);

		if (result == null)
		{
			return NotFound(new
			{
				message = "Không tìm thấy phiếu nhập kho."
			});
		}

		return Ok(result);
	}

	// POST: api/admin/warehouses
	[HttpPost]
	public async Task<IActionResult> Create(
		[FromBody] CreateWarehouseRequest request,
		CancellationToken cancellationToken)
	{
		try
		{
			var username =
				Request.Headers["X-User"].FirstOrDefault()
				?? "system";

			var result = await _warehouseService.CreateAsync(
				request,
				username,
				cancellationToken);

			return CreatedAtAction(
				nameof(GetById),
				new { id = result.Id },
				result);
		}
		catch (ArgumentException ex)
		{
			return BadRequest(new
			{
				message = ex.Message
			});
		}
	}

	// PUT: api/admin/warehouses/{id}
	[HttpPut("{id:guid}")]
	public async Task<IActionResult> Update(
		Guid id,
		[FromBody] UpdateWarehouseRequest request,
		CancellationToken cancellationToken)
	{
		try
		{
			var username =
				Request.Headers["X-User"].FirstOrDefault()
				?? "system";

			var result = await _warehouseService.UpdateAsync(
				id,
				request,
				username,
				cancellationToken);

			if (result == null)
			{
				return NotFound(new
				{
					message = "Không tìm thấy phiếu nhập kho."
				});
			}

			return Ok(result);
		}
		catch (ArgumentException ex)
		{
			return BadRequest(new
			{
				message = ex.Message
			});
		}
	}

	// DELETE: api/admin/warehouses/{id}
	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> Delete(
		Guid id,
		CancellationToken cancellationToken)
	{
		var username =
			Request.Headers["X-User"].FirstOrDefault()
			?? "system";

		var result = await _warehouseService.DeleteAsync(
			id,
			username,
			cancellationToken);

		if (!result)
		{
			return NotFound(new
			{
				message = "Không tìm thấy phiếu nhập kho."
			});
		}

		return NoContent();
	}

	// GET: api/admin/warehouses/deleted
	[HttpGet("deleted")]
	public async Task<IActionResult> GetDeleted(
		CancellationToken cancellationToken)
	{
		var result = await _warehouseService.GetDeletedAsync(
			cancellationToken);

		return Ok(result);
	}

	// PUT: api/admin/warehouses/{id}/restore
	[HttpPut("{id:guid}/restore")]
	public async Task<IActionResult> Restore(
		Guid id,
		CancellationToken cancellationToken)
	{
		var username =
			Request.Headers["X-User"].FirstOrDefault()
			?? "system";

		var result = await _warehouseService.RestoreAsync(
			id,
			username,
			cancellationToken);

		if (!result)
		{
			return NotFound(new
			{
				message = "Không tìm thấy phiếu nhập kho đã xóa."
			});
		}

		return NoContent();
	}
}