using KBM.Api.Authorization;
using KBM.Application.Auth;
using KBM.Application.Items;
using KBM.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KBM.Api.Controllers;

[ApiController]
[Route("api/items")]
[Authorize]
public class ItemsController(IItemService items) : ControllerBase
{
    // ===================== Categorie =====================
    [HttpGet("categories")]
    [RequiresPermission(PermissionCodes.ItemsRead)]
    public Task<IReadOnlyList<ItemCategoryListItem>> ListCategories(CancellationToken ct) =>
        items.ListCategoriesAsync(ct);

    [HttpGet("categories/{id:long}")]
    [RequiresPermission(PermissionCodes.ItemsRead)]
    public async Task<ActionResult<ItemCategoryDetail>> GetCategory(long id, CancellationToken ct)
    {
        var c = await items.GetCategoryAsync(id, ct);
        return c is null ? NotFound() : Ok(c);
    }

    [HttpPost("categories")]
    [RequiresPermission(PermissionCodes.ItemsCreate)]
    public async Task<ActionResult<ItemCategoryDetail>> CreateCategory([FromBody] CreateItemCategoryRequest request, CancellationToken ct)
    {
        try
        {
            var c = await items.CreateCategoryAsync(request, ct);
            return CreatedAtAction(nameof(GetCategory), new { id = c.Id }, c);
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpPut("categories/{id:long}")]
    [RequiresPermission(PermissionCodes.ItemsEdit)]
    public async Task<ActionResult<ItemCategoryDetail>> UpdateCategory(long id, [FromBody] UpdateItemCategoryRequest request, CancellationToken ct)
    {
        try
        {
            var c = await items.UpdateCategoryAsync(id, request, ct);
            return c is null ? NotFound() : Ok(c);
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpPost("categories/{id:long}/disable")]
    [RequiresPermission(PermissionCodes.ItemsEdit)]
    public async Task<IActionResult> DisableCategory(long id, CancellationToken ct) =>
        await items.SetCategoryEnabledAsync(id, false, ct) ? NoContent() : NotFound();

    [HttpPost("categories/{id:long}/enable")]
    [RequiresPermission(PermissionCodes.ItemsEdit)]
    public async Task<IActionResult> EnableCategory(long id, CancellationToken ct) =>
        await items.SetCategoryEnabledAsync(id, true, ct) ? NoContent() : NotFound();

    // ===================== Articoli =====================
    [HttpGet]
    [RequiresPermission(PermissionCodes.ItemsRead)]
    public Task<IReadOnlyList<ItemListItem>> List([FromQuery] string? search, CancellationToken ct) =>
        items.ListAsync(search, ct);

    [HttpGet("{id:long}")]
    [RequiresPermission(PermissionCodes.ItemsRead)]
    public async Task<ActionResult<ItemDetail>> Get(long id, CancellationToken ct)
    {
        var i = await items.GetAsync(id, ct);
        return i is null ? NotFound() : Ok(i);
    }

    [HttpPost]
    [RequiresPermission(PermissionCodes.ItemsCreate)]
    public async Task<ActionResult<ItemDetail>> Create([FromBody] CreateItemRequest request, CancellationToken ct)
    {
        try
        {
            var i = await items.CreateAsync(request, ct);
            return CreatedAtAction(nameof(Get), new { id = i.Id }, i);
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpPut("{id:long}")]
    [RequiresPermission(PermissionCodes.ItemsEdit)]
    public async Task<ActionResult<ItemDetail>> Update(long id, [FromBody] UpdateItemRequest request, CancellationToken ct)
    {
        try
        {
            var i = await items.UpdateAsync(id, request, ct);
            return i is null ? NotFound() : Ok(i);
        }
        catch (InvalidOperationException ex) { return Conflict(new ApiError("CONFLICT", ex.Message)); }
    }

    [HttpPost("{id:long}/disable")]
    [RequiresPermission(PermissionCodes.ItemsEdit)]
    public async Task<IActionResult> Disable(long id, CancellationToken ct) =>
        await items.SetEnabledAsync(id, false, ct) ? NoContent() : NotFound();

    [HttpPost("{id:long}/enable")]
    [RequiresPermission(PermissionCodes.ItemsEdit)]
    public async Task<IActionResult> Enable(long id, CancellationToken ct) =>
        await items.SetEnabledAsync(id, true, ct) ? NoContent() : NotFound();
}
