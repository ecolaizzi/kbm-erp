namespace KBM.Application.Customers;

/// <summary>Validazioni fiscali italiane (P.IVA con check digit, formato Codice Fiscale).</summary>
public static class ItalianFiscalValidation
{
    /// <summary>Valida la Partita IVA italiana (11 cifre, algoritmo di Luhn ministeriale).</summary>
    public static bool IsValidVatNumber(string? vat)
    {
        if (string.IsNullOrWhiteSpace(vat)) return true; // opzionale
        vat = vat.Trim();
        if (vat.Length != 11 || !vat.All(char.IsDigit)) return false;

        var sum = 0;
        for (var i = 0; i < 11; i++)
        {
            var digit = vat[i] - '0';
            if (i % 2 == 1)
            {
                digit *= 2;
                if (digit > 9) digit -= 9;
            }
            sum += digit;
        }
        return sum % 10 == 0;
    }

    /// <summary>Valida il formato del Codice Fiscale (16 alfanumerici o 11 cifre per soggetti con sola P.IVA).</summary>
    public static bool IsValidFiscalCode(string? cf)
    {
        if (string.IsNullOrWhiteSpace(cf)) return true; // opzionale
        cf = cf.Trim().ToUpperInvariant();
        if (cf.Length == 11 && cf.All(char.IsDigit)) return true;
        if (cf.Length != 16) return false;
        return cf.All(c => char.IsLetterOrDigit(c));
    }
}
