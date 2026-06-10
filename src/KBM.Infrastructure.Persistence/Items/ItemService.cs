using KBM.Application.Items;
using KBM.Application.Security;
using KBM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KBM.Infrastructure.Persistence.Items;

public sealed class ItemService(KbmDbContext db, ICurrentUserContext currentUser) : IItemService
{
    // ===================== Categorie =====================
    public async Task<IReadOnlyList<ItemCategoryListItem>> ListCategoriesAsync(CancellationToken ct = default)
    {
        var companyId = CompanyId();
        return await db.ItemCategories.Where(c => !c.IsDeleted && c.CompanyId == companyId)
            .OrderBy(c => c.Name)
            .Select(c => new ItemCategoryListItem(c.Id, c.Code, c.Name, c.Status))
            .ToListAsync(ct);
    }

    public async Task<ItemCategoryDetail?> GetCategoryAsync(long id, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var c = await db.ItemCategories.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        return c is null ? null : new ItemCategoryDetail(c.Id, c.Code, c.Name, c.Description, c.Status);
    }

    public async Task<ItemCategoryDetail> CreateCategoryAsync(CreateItemCategoryRequest request, CancellationToken ct = default)
    {
        var actorId = currentUser.UserId ?? 0;
        var companyId = CompanyId();
        var code = request.Code.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code)) throw new InvalidOperationException("Il codice categoria e obbligatorio.");
        if (await db.ItemCategories.AnyAsync(c => c.Code == code && c.CompanyId == companyId && !c.IsDeleted, ct))
            throw new InvalidOperationException("Codice categoria gia in uso.");

        var cat = new ItemCategory
        {
            CompanyId = companyId, Code = code, Name = request.Name.Trim(), Description = Clean(request.Description),
            Status = "Active", CreatedAt = DateTime.UtcNow, CreatedBy = actorId
        };
        db.ItemCategories.Add(cat);
        await db.SaveChangesAsync(ct);
        return new ItemCategoryDetail(cat.Id, cat.Code, cat.Name, cat.Description, cat.Status);
    }

    public async Task<ItemCategoryDetail?> UpdateCategoryAsync(long id, UpdateItemCategoryRequest request, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var cat = await db.ItemCategories.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        if (cat is null) return null;
        cat.Name = request.Name.Trim();
        cat.Description = Clean(request.Description);
        cat.Status = request.Status;
        cat.UpdatedAt = DateTime.UtcNow;
        cat.UpdatedBy = currentUser.UserId;
        await db.SaveChangesAsync(ct);
        return new ItemCategoryDetail(cat.Id, cat.Code, cat.Name, cat.Description, cat.Status);
    }

    public async Task<bool> SetCategoryEnabledAsync(long id, bool enabled, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var cat = await db.ItemCategories.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        if (cat is null) return false;
        cat.Status = enabled ? "Active" : "Inactive";
        cat.UpdatedAt = DateTime.UtcNow;
        cat.UpdatedBy = currentUser.UserId;
        await db.SaveChangesAsync(ct);
        return true;
    }

    // ===================== Articoli =====================
    public async Task<IReadOnlyList<ItemListItem>> ListAsync(string? search = null, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var query = db.Items.Where(i => !i.IsDeleted && i.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(i => i.Description.Contains(term) || i.Code.Contains(term) ||
                (i.Barcode != null && i.Barcode.Contains(term)));
        }
        return await query.OrderBy(i => i.Description)
            .Select(i => new ItemListItem(i.Id, i.Code, i.Description,
                i.Category != null ? i.Category.Name : null, i.UnitOfMeasure, i.BasePrice, i.VatRate, i.Status))
            .ToListAsync(ct);
    }

    public async Task<ItemDetail?> GetAsync(long id, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var i = await db.Items.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        return i is null ? null : Map(i);
    }

    public async Task<ItemDetail> CreateAsync(CreateItemRequest request, CancellationToken ct = default)
    {
        var actorId = currentUser.UserId ?? 0;
        var companyId = CompanyId();
        var code = request.Code.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code)) throw new InvalidOperationException("Il codice articolo e obbligatorio.");
        if (await db.Items.AnyAsync(i => i.Code == code && i.CompanyId == companyId && !i.IsDeleted, ct))
            throw new InvalidOperationException("Codice articolo gia in uso.");
        await ValidateCategoryAsync(request.CategoryId, companyId, ct);

        var item = new Item
        {
            CompanyId = companyId, Code = code, Status = "Active", CreatedAt = DateTime.UtcNow, CreatedBy = actorId
        };
        ApplyCore(item, request.Description, request.CategoryId, request.UnitOfMeasure, request.Barcode,
            request.SupplierItemCode, request.BasePrice, request.VatRate, request.RevenueAccount, request.CostAccount, request.Notes);
        db.Items.Add(item);
        await db.SaveChangesAsync(ct);
        return Map(item);
    }

    public async Task<ItemDetail?> UpdateAsync(long id, UpdateItemRequest request, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var item = await db.Items.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        if (item is null) return null;
        await ValidateCategoryAsync(request.CategoryId, companyId, ct);

        ApplyCore(item, request.Description, request.CategoryId, request.UnitOfMeasure, request.Barcode,
            request.SupplierItemCode, request.BasePrice, request.VatRate, request.RevenueAccount, request.CostAccount, request.Notes);
        item.Status = request.Status;
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = currentUser.UserId;
        await db.SaveChangesAsync(ct);
        return Map(item);
    }

    public async Task<bool> SetEnabledAsync(long id, bool enabled, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var item = await db.Items.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        if (item is null) return false;
        item.Status = enabled ? "Active" : "Inactive";
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = currentUser.UserId;
        await db.SaveChangesAsync(ct);
        return true;
    }

    private long CompanyId() => currentUser.CompanyId ?? throw new InvalidOperationException("Contesto azienda mancante.");

    private async Task ValidateCategoryAsync(long? categoryId, long companyId, CancellationToken ct)
    {
        if (categoryId is null or 0) return;
        var ok = await db.ItemCategories.AnyAsync(c => c.Id == categoryId && c.CompanyId == companyId && !c.IsDeleted, ct);
        if (!ok) throw new InvalidOperationException("Categoria non valida.");
    }

    private static void ApplyCore(Item i, string description, long? categoryId, string um, string? barcode,
        string? supplierCode, decimal basePrice, decimal vatRate, string? revenue, string? cost, string? notes)
    {
        i.Description = description.Trim();
        i.CategoryId = categoryId is 0 ? null : categoryId;
        i.UnitOfMeasure = string.IsNullOrWhiteSpace(um) ? "NR" : um.Trim().ToUpperInvariant();
        i.Barcode = Clean(barcode);
        i.SupplierItemCode = Clean(supplierCode);
        i.BasePrice = basePrice < 0 ? 0 : basePrice;
        i.VatRate = vatRate < 0 ? 0 : vatRate;
        i.RevenueAccount = Clean(revenue);
        i.CostAccount = Clean(cost);
        i.Notes = Clean(notes);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static ItemDetail Map(Item i) => new(
        i.Id, i.Code, i.Description, i.CategoryId, i.UnitOfMeasure, i.Barcode, i.SupplierItemCode,
        i.BasePrice, i.VatRate, i.RevenueAccount, i.CostAccount, i.Notes, i.Status);
}
