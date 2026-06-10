using KBM.Domain.Common;

namespace KBM.Domain.Entities;

/// <summary>
/// Condizione di pagamento (tabella di base, stile Business Cube `subm0100`).
/// Definisce come un importo viene ripartito in rate con relative scadenze.
/// </summary>
public class PaymentTerm : AuditableTenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>Numero di rate in cui suddividere l'importo (min 1).</summary>
    public int InstallmentsCount { get; set; } = 1;

    /// <summary>Giorni dalla data documento alla prima scadenza.</summary>
    public int FirstDueDays { get; set; }

    /// <summary>Giorni tra una rata e la successiva.</summary>
    public int IntervalDays { get; set; } = 30;

    /// <summary>Se true, ogni scadenza viene spostata a fine mese.</summary>
    public bool EndOfMonth { get; set; }

    /// <summary>Metodo di pagamento (es. Bonifico, RIBA, Contanti) — testo libero per ora.</summary>
    public string? PaymentMethod { get; set; }

    public string? Notes { get; set; }
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Logica gestionale riutilizzabile: genera lo scadenziere ripartendo
    /// <paramref name="amount"/> sulle rate a partire da <paramref name="documentDate"/>.
    /// L'eventuale arrotondamento viene aggiunto all'ultima rata.
    /// </summary>
    public IReadOnlyList<Installment> BuildSchedule(DateTime documentDate, decimal amount)
    {
        var count = InstallmentsCount < 1 ? 1 : InstallmentsCount;
        var perInstallment = Math.Round(amount / count, 2, MidpointRounding.AwayFromZero);
        var result = new List<Installment>(count);
        decimal allocated = 0m;

        for (var i = 0; i < count; i++)
        {
            var due = documentDate.AddDays(FirstDueDays + (IntervalDays * i));
            if (EndOfMonth)
                due = new DateTime(due.Year, due.Month, DateTime.DaysInMonth(due.Year, due.Month));

            var value = i == count - 1 ? amount - allocated : perInstallment;
            allocated += value;
            result.Add(new Installment(i + 1, due, value));
        }

        return result;
    }
}

/// <summary>Singola rata di uno scadenziere.</summary>
public readonly record struct Installment(int Number, DateTime DueDate, decimal Amount);
