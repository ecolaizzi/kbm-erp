using KBM.Application.Orders;
using KBM.Application.Security;
using KBM.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;

namespace KBM.Infrastructure.Persistence.Orders;

public sealed class SalesOrderService(KbmDbContext db, ICurrentUserContext currentUser) : ISalesOrderService
{
    public async Task<IReadOnlyList<SalesOrderListItem>> ListAsync(string? search = null, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var q = db.SalesOrders.Where(o => !o.IsDeleted && o.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            q = q.Where(o => o.Number.Contains(term) || o.Customer.BusinessName.Contains(term));
        }
        return await q.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.Id)
            .Select(o => new SalesOrderListItem(o.Id, o.Number, o.OrderDate, o.Customer.BusinessName,
                o.Lines.Count, o.TotalAmount, (int)o.Status))
            .ToListAsync(ct);
    }

    public async Task<SalesOrderDetail?> GetAsync(long id, CancellationToken ct = default)
    {
        var o = await LoadAsync(id, ct);
        return o is null ? null : Map(o);
    }

    public async Task<SalesOrderDetail> CreateAsync(CreateSalesOrderRequest request, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var actorId = currentUser.UserId ?? 0;
        var now = DateTime.UtcNow;
        await EnsureCustomerAsync(request.CustomerId, companyId, ct);

        var lines = request.Lines.Where(l => !string.IsNullOrWhiteSpace(l.Description)).ToList();
        if (lines.Count == 0) throw new InvalidOperationException("L'ordine deve contenere almeno una riga.");

        var order = new SalesOrder
        {
            CompanyId = companyId,
            Number = await NextNumberAsync("OV", companyId, request.OrderDate, ct),
            OrderDate = request.OrderDate == default ? now : request.OrderDate,
            CustomerId = request.CustomerId,
            CustomerOrderReference = Clean(request.CustomerOrderReference),
            BillingAddressId = request.BillingAddressId,
            ShippingAddressId = request.ShippingAddressId,
            PriceListId = request.PriceListId,
            PaymentTermId = request.PaymentTermId,
            WarehouseId = request.WarehouseId,
            CurrencyId = request.CurrencyId,
            CarrierId = request.CarrierId,
            PortTypeId = request.PortTypeId,
            DocumentTypeId = request.DocumentTypeId,
            ExchangeRate = request.ExchangeRate <= 0 ? 1m : request.ExchangeRate,
            HeaderDiscountPercent = request.HeaderDiscountPercent,
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
            Notes = Clean(request.Notes),
            Status = OrderDocumentStatus.Draft,
            CreatedAt = now,
            CreatedBy = actorId
        };

        var lineNum = 1;
        var totals = new List<(decimal Net, decimal Vat)>();
        foreach (var l in lines)
        {
            var line = await BuildLineAsync(l, lineNum++, companyId, actorId, now, ct);
            order.Lines.Add(line);
            totals.Add((line.NetAmount, line.VatAmount));
        }
        ApplyTotals(order, totals);

        db.SalesOrders.Add(order);
        await db.SaveChangesAsync(ct);
        return (await GetAsync(order.Id, ct))!;
    }

    public async Task<SalesOrderDetail?> SaveAsync(long id, SaveSalesOrderRequest request, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var actorId = currentUser.UserId ?? 0;
        var now = DateTime.UtcNow;
        var order = await db.SalesOrders.Include(o => o.Lines).Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == id && o.CompanyId == companyId && !o.IsDeleted, ct);
        if (order is null) return null;
        if (order.Status is OrderDocumentStatus.Invoiced or OrderDocumentStatus.Cancelled)
            throw new InvalidOperationException("Ordine non modificabile nello stato corrente.");

        var lines = request.Lines.Where(l => !string.IsNullOrWhiteSpace(l.Description)).ToList();
        if (lines.Count == 0) throw new InvalidOperationException("L'ordine deve contenere almeno una riga.");

        order.OrderDate = request.OrderDate == default ? order.OrderDate : request.OrderDate;
        order.CustomerOrderReference = Clean(request.CustomerOrderReference);
        order.BillingAddressId = request.BillingAddressId;
        order.ShippingAddressId = request.ShippingAddressId;
        order.PriceListId = request.PriceListId;
        order.PaymentTermId = request.PaymentTermId;
        order.WarehouseId = request.WarehouseId;
        order.CurrencyId = request.CurrencyId;
        order.CarrierId = request.CarrierId;
        order.PortTypeId = request.PortTypeId;
        order.DocumentTypeId = request.DocumentTypeId;
        order.ExchangeRate = request.ExchangeRate <= 0 ? 1m : request.ExchangeRate;
        order.HeaderDiscountPercent = request.HeaderDiscountPercent;
        order.ExpectedDeliveryDate = request.ExpectedDeliveryDate;
        order.Notes = Clean(request.Notes);
        order.Status = (OrderDocumentStatus)request.Status;
        order.UpdatedAt = now;
        order.UpdatedBy = actorId;

        var keepIds = lines.Where(l => l.Id > 0).Select(l => l.Id!.Value).ToHashSet();
        foreach (var existing in order.Lines.Where(l => !keepIds.Contains(l.Id)).ToList())
            db.SalesOrderLines.Remove(existing);

        var lineNum = 1;
        var totals = new List<(decimal Net, decimal Vat)>();
        foreach (var l in lines)
        {
            SalesOrderLine line;
            if (l.Id > 0)
            {
                line = order.Lines.First(x => x.Id == l.Id);
                await ApplyLineAsync(line, l, lineNum++, ct);
            }
            else
            {
                line = await BuildLineAsync(l, lineNum++, companyId, actorId, now, ct);
                order.Lines.Add(line);
            }
            totals.Add((line.NetAmount, line.VatAmount));
        }
        ApplyTotals(order, totals);
        await db.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var order = await db.SalesOrders.FirstOrDefaultAsync(o => o.Id == id && o.CompanyId == companyId && !o.IsDeleted, ct);
        if (order is null) return false;
        if (order.Status != OrderDocumentStatus.Draft)
            throw new InvalidOperationException("Solo gli ordini in bozza possono essere eliminati.");
        order.IsDeleted = true;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = currentUser.UserId;
        await db.SaveChangesAsync(ct);
        return true;
    }

    private async Task<SalesOrder?> LoadAsync(long id, CancellationToken ct)
    {
        var companyId = CompanyId();
        return await db.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.Lines).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(o => o.Id == id && o.CompanyId == companyId && !o.IsDeleted, ct);
    }

    private async Task<SalesOrderLine> BuildLineAsync(SaveOrderLineRequest l, int lineNum, long companyId, long actorId, DateTime now, CancellationToken ct)
    {
        var line = new SalesOrderLine { CompanyId = companyId, LineNumber = lineNum, CreatedAt = now, CreatedBy = actorId };
        await ApplyLineAsync(line, l, lineNum, ct);
        return line;
    }

    private async Task ApplyLineAsync(SalesOrderLine line, SaveOrderLineRequest l, int lineNum, CancellationToken ct)
    {
        line.LineNumber = lineNum;
        line.ItemId = l.ItemId;
        line.Description = l.Description.Trim();
        line.OrderedQuantity = l.OrderedQuantity <= 0 ? 1m : l.OrderedQuantity;
        line.UnitOfMeasure = Clean(l.UnitOfMeasure);
        line.VatCodeId = l.VatCodeId;
        line.UnitPrice = l.UnitPrice;
        line.LineDiscountPercent = l.LineDiscountPercent;
        line.VatPercent = l.VatPercent;

        if (l.ItemId is > 0)
        {
            var item = await db.Items.FirstOrDefaultAsync(i => i.Id == l.ItemId && !i.IsDeleted, ct);
            if (item is not null)
            {
                if (string.IsNullOrWhiteSpace(line.UnitOfMeasure)) line.UnitOfMeasure = item.UnitOfMeasure;
                if (line.UnitPrice == 0) line.UnitPrice = item.BasePrice;
                if (line.VatPercent == 0) line.VatPercent = item.VatRate;
            }
        }

        var (net, vat, total) = OrderTotalsHelper.LineTotals(line.OrderedQuantity, line.UnitPrice, line.LineDiscountPercent, line.VatPercent);
        line.NetAmount = net;
        line.VatAmount = vat;
        line.TotalAmount = total;
        line.LineStatus = OrderLineStatus.Open;
    }

    private static void ApplyTotals(SalesOrder order, List<(decimal Net, decimal Vat)> lines)
    {
        var (net, vat, total) = OrderTotalsHelper.DocumentTotals(lines, order.HeaderDiscountPercent);
        order.NetAmount = net;
        order.VatAmount = vat;
        order.TotalAmount = total;
    }

    private static SalesOrderDetail Map(SalesOrder o) => new(
        o.Id, o.Number, o.OrderDate, o.CustomerId, o.Customer.BusinessName,
        o.CustomerOrderReference, o.BillingAddressId, o.ShippingAddressId,
        o.PriceListId, o.PaymentTermId, o.WarehouseId, o.CurrencyId,
        o.CarrierId, o.PortTypeId, o.DocumentTypeId,
        o.ExchangeRate, o.HeaderDiscountPercent, o.NetAmount, o.VatAmount, o.TotalAmount,
        (int)o.Status, o.ExpectedDeliveryDate, o.Notes,
        o.Lines.OrderBy(l => l.LineNumber).Select(l => new OrderLineDto(
            l.Id, l.LineNumber, l.ItemId, l.Item?.Code, l.Description,
            l.OrderedQuantity, l.DeliveredQuantity, l.InvoicedQuantity,
            l.UnitOfMeasure, l.VatCodeId, l.UnitPrice, l.LineDiscountPercent,
            l.VatPercent, l.NetAmount, l.VatAmount, l.TotalAmount, (int)l.LineStatus)).ToList());

    private async Task EnsureCustomerAsync(long customerId, long companyId, CancellationToken ct)
    {
        if (!await db.Customers.AnyAsync(c => c.Id == customerId && c.CompanyId == companyId && !c.IsDeleted, ct))
            throw new InvalidOperationException("Cliente non trovato.");
    }

    private async Task<string> NextNumberAsync(string prefix, long companyId, DateTime date, CancellationToken ct)
    {
        var year = (date == default ? DateTime.UtcNow : date).Year;
        var count = await db.SalesOrders.CountAsync(o => o.CompanyId == companyId && o.OrderDate.Year == year, ct);
        return $"{prefix}/{year}/{count + 1:0000}";
    }

    private long CompanyId() => currentUser.CompanyId ?? throw new InvalidOperationException("Contesto azienda mancante.");
    private static string? Clean(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();
}
