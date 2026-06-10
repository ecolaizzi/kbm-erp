namespace KBM.Application.Items;

public interface IItemService
{
    // Categorie
    Task<IReadOnlyList<ItemCategoryListItem>> ListCategoriesAsync(CancellationToken ct = default);
    Task<ItemCategoryDetail?> GetCategoryAsync(long id, CancellationToken ct = default);
    Task<ItemCategoryDetail> CreateCategoryAsync(CreateItemCategoryRequest request, CancellationToken ct = default);
    Task<ItemCategoryDetail?> UpdateCategoryAsync(long id, UpdateItemCategoryRequest request, CancellationToken ct = default);
    Task<bool> SetCategoryEnabledAsync(long id, bool enabled, CancellationToken ct = default);

    // Articoli
    Task<IReadOnlyList<ItemListItem>> ListAsync(string? search = null, CancellationToken ct = default);
    Task<ItemDetail?> GetAsync(long id, CancellationToken ct = default);
    Task<ItemDetail> CreateAsync(CreateItemRequest request, CancellationToken ct = default);
    Task<ItemDetail?> UpdateAsync(long id, UpdateItemRequest request, CancellationToken ct = default);
    Task<bool> SetEnabledAsync(long id, bool enabled, CancellationToken ct = default);
}
