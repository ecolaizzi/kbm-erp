using KBM.Application.Customers;
using KBM.Application.Security;
using KBM.Application.Suppliers;
using KBM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KBM.Infrastructure.Persistence.Suppliers;

public sealed class SupplierService(KbmDbContext db, ICurrentUserContext currentUser) : ISupplierService
{
    public async Task<IReadOnlyList<SupplierListItem>> ListAsync(string? search = null, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var query = db.Suppliers.Where(s => !s.IsDeleted && s.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(s => s.BusinessName.Contains(term) || s.Code.Contains(term) ||
                (s.VatNumber != null && s.VatNumber.Contains(term)));
        }
        return await query.OrderBy(s => s.BusinessName)
            .Select(s => new SupplierListItem(s.Id, s.Code, s.BusinessName, s.VatNumber, s.City, s.Status))
            .ToListAsync(ct);
    }

    public async Task<SupplierDetail?> GetAsync(long id, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var s = await db.Suppliers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        return s is null ? null : Map(s);
    }

    public async Task<SupplierDetail?> UpdateAsync(long id, UpdateSupplierRequest request, CancellationToken ct = default)
    {
        var actorId = currentUser.UserId ?? 0;
        var companyId = CompanyId();
        var s = await db.Suppliers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        if (s is null) return null;

        Validate(request.VatNumber, request.FiscalCode);
        ApplyCore(s, request.BusinessName, request.VatNumber, request.FiscalCode, request.SdiCode, request.PecEmail,
            request.Email, request.Phone, request.Address, request.City, request.Province, request.PostalCode,
            request.Country, request.Iban, request.PaymentTerms, request.Notes);
        s.Status = request.Status;
        s.UpdatedAt = DateTime.UtcNow;
        s.UpdatedBy = actorId;
        await db.SaveChangesAsync(ct);
        return Map(s);
    }

    public async Task<bool> SetEnabledAsync(long id, bool enabled, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var s = await db.Suppliers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        if (s is null) return false;
        s.Status = enabled ? "Active" : "Inactive";
        s.UpdatedAt = DateTime.UtcNow;
        s.UpdatedBy = currentUser.UserId;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<SupplierAggregate?> GetFullAsync(long id, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var s = await db.Suppliers
            .Include(x => x.Addresses).Include(x => x.Contacts).Include(x => x.Banks)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        return s is null ? null : MapAggregate(s);
    }

    public async Task<SupplierAggregate> CreateFullAsync(CreateSupplierAggregateRequest request, CancellationToken ct = default)
    {
        var actorId = currentUser.UserId ?? 0;
        var companyId = CompanyId();
        var core = request.Core;
        Validate(core.VatNumber, core.FiscalCode);

        var code = core.Code.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException("Il codice fornitore e obbligatorio.");
        if (await db.Suppliers.AnyAsync(x => x.Code == code && x.CompanyId == companyId && !x.IsDeleted, ct))
            throw new InvalidOperationException("Codice fornitore gia in uso.");

        var s = new Supplier { CompanyId = companyId, Code = code, Status = "Active", CreatedAt = DateTime.UtcNow, CreatedBy = actorId };
        ApplyCore(s, core.BusinessName, core.VatNumber, core.FiscalCode, core.SdiCode, core.PecEmail, core.Email,
            core.Phone, core.Address, core.City, core.Province, core.PostalCode, core.Country, core.Iban,
            core.PaymentTerms, core.Notes);
        ApplyAccounting(s, request.Accounting);

        foreach (var a in request.Addresses) s.Addresses.Add(NewAddress(a, companyId, actorId));
        foreach (var k in request.Contacts) s.Contacts.Add(NewContact(k, companyId, actorId));
        foreach (var b in request.Banks) s.Banks.Add(NewBank(b, companyId, actorId));

        db.Suppliers.Add(s);
        await db.SaveChangesAsync(ct);
        return MapAggregate(s);
    }

    public async Task<SupplierAggregate?> SaveFullAsync(long id, SaveSupplierAggregateRequest request, CancellationToken ct = default)
    {
        var actorId = currentUser.UserId ?? 0;
        var companyId = CompanyId();
        var s = await db.Suppliers
            .Include(x => x.Addresses).Include(x => x.Contacts).Include(x => x.Banks)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        if (s is null) return null;

        var core = request.Core;
        Validate(core.VatNumber, core.FiscalCode);
        ApplyCore(s, core.BusinessName, core.VatNumber, core.FiscalCode, core.SdiCode, core.PecEmail, core.Email,
            core.Phone, core.Address, core.City, core.Province, core.PostalCode, core.Country, core.Iban,
            core.PaymentTerms, core.Notes);
        s.Status = core.Status;
        s.UpdatedAt = DateTime.UtcNow;
        s.UpdatedBy = actorId;
        ApplyAccounting(s, request.Accounting);

        SyncAddresses(s, request.Addresses, companyId, actorId);
        SyncContacts(s, request.Contacts, companyId, actorId);
        SyncBanks(s, request.Banks, companyId, actorId);

        await db.SaveChangesAsync(ct);
        return MapAggregate(s);
    }

    private long CompanyId() => currentUser.CompanyId ?? throw new InvalidOperationException("Contesto azienda mancante.");

    private static void ApplyCore(Supplier s, string businessName, string? vat, string? cf, string? sdi, string? pec,
        string? email, string? phone, string? address, string? city, string? province, string? postal, string? country,
        string? iban, string? paymentTerms, string? notes)
    {
        s.BusinessName = businessName.Trim();
        s.VatNumber = Clean(vat);
        s.FiscalCode = Clean(cf)?.ToUpperInvariant();
        s.SdiCode = Clean(sdi)?.ToUpperInvariant();
        s.PecEmail = Clean(pec);
        s.Email = Clean(email);
        s.Phone = Clean(phone);
        s.Address = Clean(address);
        s.City = Clean(city);
        s.Province = Clean(province)?.ToUpperInvariant();
        s.PostalCode = Clean(postal);
        s.Country = string.IsNullOrWhiteSpace(country) ? "IT" : country.Trim().ToUpperInvariant();
        s.Iban = Clean(iban)?.Replace(" ", "").ToUpperInvariant();
        s.PaymentTerms = Clean(paymentTerms);
        s.Notes = Clean(notes);
    }

    private static void ApplyAccounting(Supplier s, SupplierAccounting a)
    {
        s.PaymentMethod = Clean(a.PaymentMethod);
        s.PriceListCode = Clean(a.PriceListCode);
        s.Zone = Clean(a.Zone);
        s.CreditLimit = a.CreditLimit;
        s.DiscountPercent = a.DiscountPercent;
        s.SplitPayment = a.SplitPayment;
        s.WithholdingTax = a.WithholdingTax;
        s.VatExemptionCode = Clean(a.VatExemptionCode);
        s.AccountCode = Clean(a.AccountCode);
    }

    private void SyncAddresses(Supplier s, IReadOnlyList<SupplierAddressDto> incoming, long companyId, long actorId)
    {
        var keep = incoming.Where(i => i.Id > 0).Select(i => i.Id).ToHashSet();
        foreach (var ex in s.Addresses.Where(e => !keep.Contains(e.Id)).ToList()) db.SupplierAddresses.Remove(ex);
        foreach (var dto in incoming)
        {
            var ex = dto.Id > 0 ? s.Addresses.FirstOrDefault(e => e.Id == dto.Id) : null;
            if (ex is null) { s.Addresses.Add(NewAddress(dto, companyId, actorId)); continue; }
            ex.AddressType = dto.AddressType; ex.Description = dto.Description?.Trim() ?? "";
            ex.Address = Clean(dto.Address); ex.City = Clean(dto.City);
            ex.Province = Clean(dto.Province)?.ToUpperInvariant(); ex.PostalCode = Clean(dto.PostalCode);
            ex.Country = string.IsNullOrWhiteSpace(dto.Country) ? "IT" : dto.Country.Trim().ToUpperInvariant();
            ex.Phone = Clean(dto.Phone); ex.Email = Clean(dto.Email); ex.IsDefault = dto.IsDefault;
            ex.UpdatedAt = DateTime.UtcNow; ex.UpdatedBy = actorId;
        }
    }

    private void SyncContacts(Supplier s, IReadOnlyList<SupplierContactDto> incoming, long companyId, long actorId)
    {
        var keep = incoming.Where(i => i.Id > 0).Select(i => i.Id).ToHashSet();
        foreach (var ex in s.Contacts.Where(e => !keep.Contains(e.Id)).ToList()) db.SupplierContacts.Remove(ex);
        foreach (var dto in incoming)
        {
            var ex = dto.Id > 0 ? s.Contacts.FirstOrDefault(e => e.Id == dto.Id) : null;
            if (ex is null) { s.Contacts.Add(NewContact(dto, companyId, actorId)); continue; }
            ex.Name = dto.Name?.Trim() ?? ""; ex.Role = Clean(dto.Role); ex.Email = Clean(dto.Email);
            ex.Phone = Clean(dto.Phone); ex.Mobile = Clean(dto.Mobile); ex.Notes = Clean(dto.Notes);
            ex.IsPrimary = dto.IsPrimary; ex.UpdatedAt = DateTime.UtcNow; ex.UpdatedBy = actorId;
        }
    }

    private void SyncBanks(Supplier s, IReadOnlyList<SupplierBankDto> incoming, long companyId, long actorId)
    {
        var keep = incoming.Where(i => i.Id > 0).Select(i => i.Id).ToHashSet();
        foreach (var ex in s.Banks.Where(e => !keep.Contains(e.Id)).ToList()) db.SupplierBanks.Remove(ex);
        foreach (var dto in incoming)
        {
            var ex = dto.Id > 0 ? s.Banks.FirstOrDefault(e => e.Id == dto.Id) : null;
            if (ex is null) { s.Banks.Add(NewBank(dto, companyId, actorId)); continue; }
            ex.BankName = dto.BankName?.Trim() ?? ""; ex.Iban = Clean(dto.Iban)?.Replace(" ", "").ToUpperInvariant();
            ex.Swift = Clean(dto.Swift)?.ToUpperInvariant(); ex.Abi = Clean(dto.Abi); ex.Cab = Clean(dto.Cab);
            ex.IsDefault = dto.IsDefault; ex.UpdatedAt = DateTime.UtcNow; ex.UpdatedBy = actorId;
        }
    }

    private static SupplierAddress NewAddress(SupplierAddressDto d, long companyId, long actorId) => new()
    {
        CompanyId = companyId, AddressType = string.IsNullOrWhiteSpace(d.AddressType) ? "Shipping" : d.AddressType,
        Description = d.Description?.Trim() ?? "", Address = Clean(d.Address), City = Clean(d.City),
        Province = Clean(d.Province)?.ToUpperInvariant(), PostalCode = Clean(d.PostalCode),
        Country = string.IsNullOrWhiteSpace(d.Country) ? "IT" : d.Country.Trim().ToUpperInvariant(),
        Phone = Clean(d.Phone), Email = Clean(d.Email), IsDefault = d.IsDefault, CreatedAt = DateTime.UtcNow, CreatedBy = actorId
    };

    private static SupplierContact NewContact(SupplierContactDto d, long companyId, long actorId) => new()
    {
        CompanyId = companyId, Name = d.Name?.Trim() ?? "", Role = Clean(d.Role), Email = Clean(d.Email),
        Phone = Clean(d.Phone), Mobile = Clean(d.Mobile), Notes = Clean(d.Notes), IsPrimary = d.IsPrimary,
        CreatedAt = DateTime.UtcNow, CreatedBy = actorId
    };

    private static SupplierBank NewBank(SupplierBankDto d, long companyId, long actorId) => new()
    {
        CompanyId = companyId, BankName = d.BankName?.Trim() ?? "",
        Iban = Clean(d.Iban)?.Replace(" ", "").ToUpperInvariant(), Swift = Clean(d.Swift)?.ToUpperInvariant(),
        Abi = Clean(d.Abi), Cab = Clean(d.Cab), IsDefault = d.IsDefault, CreatedAt = DateTime.UtcNow, CreatedBy = actorId
    };

    private static void Validate(string? vat, string? fiscalCode)
    {
        if (!ItalianFiscalValidation.IsValidVatNumber(vat))
            throw new InvalidOperationException("Partita IVA non valida (attese 11 cifre con check digit corretto).");
        if (!ItalianFiscalValidation.IsValidFiscalCode(fiscalCode))
            throw new InvalidOperationException("Codice Fiscale non valido (16 caratteri alfanumerici o 11 cifre).");
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static SupplierDetail Map(Supplier s) => new(
        s.Id, s.Code, s.BusinessName, s.VatNumber, s.FiscalCode, s.SdiCode, s.PecEmail, s.Email, s.Phone,
        s.Address, s.City, s.Province, s.PostalCode, s.Country, s.Iban, s.PaymentTerms, s.Notes, s.Status);

    private static SupplierAggregate MapAggregate(Supplier s) => new(
        Map(s),
        new SupplierAccounting(s.PaymentMethod, s.PriceListCode, s.Zone, s.CreditLimit, s.DiscountPercent,
            s.SplitPayment, s.WithholdingTax, s.VatExemptionCode, s.AccountCode),
        s.Addresses.OrderByDescending(a => a.IsDefault).ThenBy(a => a.Id)
            .Select(a => new SupplierAddressDto(a.Id, a.AddressType, a.Description, a.Address, a.City, a.Province,
                a.PostalCode, a.Country, a.Phone, a.Email, a.IsDefault)).ToList(),
        s.Contacts.OrderByDescending(k => k.IsPrimary).ThenBy(k => k.Id)
            .Select(k => new SupplierContactDto(k.Id, k.Name, k.Role, k.Email, k.Phone, k.Mobile, k.Notes, k.IsPrimary)).ToList(),
        s.Banks.OrderByDescending(b => b.IsDefault).ThenBy(b => b.Id)
            .Select(b => new SupplierBankDto(b.Id, b.BankName, b.Iban, b.Swift, b.Abi, b.Cab, b.IsDefault)).ToList());
}
