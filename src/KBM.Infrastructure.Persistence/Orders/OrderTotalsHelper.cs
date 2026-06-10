namespace KBM.Infrastructure.Persistence.Orders;

internal static class OrderTotalsHelper
{
    public static (decimal Net, decimal Vat, decimal Total) LineTotals(
        decimal quantity, decimal unitPrice, decimal lineDiscountPercent, decimal vatPercent)
    {
        var gross = quantity * unitPrice;
        var net = Math.Round(gross * (1m - (lineDiscountPercent / 100m)), 2, MidpointRounding.AwayFromZero);
        var vat = Math.Round(net * (vatPercent / 100m), 2, MidpointRounding.AwayFromZero);
        return (net, vat, net + vat);
    }

    public static (decimal Net, decimal Vat, decimal Total) DocumentTotals(
        IEnumerable<(decimal Net, decimal Vat)> lines, decimal headerDiscountPercent)
    {
        var net = lines.Sum(l => l.Net);
        var vat = lines.Sum(l => l.Vat);
        if (headerDiscountPercent > 0)
        {
            var factor = 1m - (headerDiscountPercent / 100m);
            net = Math.Round(net * factor, 2, MidpointRounding.AwayFromZero);
            vat = Math.Round(vat * factor, 2, MidpointRounding.AwayFromZero);
        }
        return (net, vat, net + vat);
    }
}
