using KBM.Application.Customers;
using KBM.Application.Security;
using KBM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KBM.Infrastructure.Persistence.Customers;

public sealed class CustomerService(KbmDbContext db, ICurrentUserContext currentUser) : ICustomerService
{
    public async Task<IReadOnlyList<CustomerListItem>> ListAsync(string? search = null, CancellationToken ct = default)
    {
        var companyId = currentUser.CompanyId
            ?? throw new InvalidOperationException("Contesto azienda mancante.");

        var query = db.Customers.Where(c => !c.IsDeleted && c.CompanyId == companyId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c =>
                c.BusinessName.Contains(term) ||
                c.Code.Contains(term) ||
                (c.VatNumber != null && c.VatNumber.Contains(term)));
        }

        return await query
            .OrderBy(c => c.BusinessName)
            .Select(c => new CustomerListItem(c.Id, c.Code, c.BusinessName, c.VatNumber, c.City, c.Status))
            .ToListAsync(ct);
    }

    public async Task<CustomerDetail?> GetAsync(long id, CancellationToken ct = default)
    {
        var companyId = currentUser.CompanyId
            ?? throw new InvalidOperationException("Contesto azienda mancante.");

        var customer = await db.Customers
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted && c.CompanyId == companyId, ct);
        return customer is null ? null : Map(customer);
    }

    public async Task<CustomerDetail> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default)
    {
        var actorId = currentUser.UserId ?? 0;
        var companyId = currentUser.CompanyId
            ?? throw new InvalidOperationException("Contesto azienda mancante.");

        Validate(request.VatNumber, request.FiscalCode);

        var code = request.Code.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException("Il codice cliente e obbligatorio.");
        if (await db.Customers.AnyAsync(c => c.Code == code && c.CompanyId == companyId && !c.IsDeleted, ct))
            throw new InvalidOperationException("Codice cliente gia in uso.");

        var customer = new Customer
        {
            CompanyId = companyId,
            Code = code,
            BusinessName = request.BusinessName.Trim(),
            VatNumber = Clean(request.VatNumber),
            FiscalCode = Clean(request.FiscalCode)?.ToUpperInvariant(),
            SdiCode = Clean(request.SdiCode)?.ToUpperInvariant(),
            PecEmail = Clean(request.PecEmail),
            Email = Clean(request.Email),
            Phone = Clean(request.Phone),
            Address = Clean(request.Address),
            City = Clean(request.City),
            Province = Clean(request.Province)?.ToUpperInvariant(),
            PostalCode = Clean(request.PostalCode),
            Country = string.IsNullOrWhiteSpace(request.Country) ? "IT" : request.Country.Trim().ToUpperInvariant(),
            Iban = Clean(request.Iban)?.Replace(" ", "").ToUpperInvariant(),
            PaymentTerms = Clean(request.PaymentTerms),
            Notes = Clean(request.Notes),
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = actorId
        };
        db.Customers.Add(customer);
        await db.SaveChangesAsync(ct);
        return Map(customer);
    }

    public async Task<CustomerDetail?> UpdateAsync(long id, UpdateCustomerRequest request, CancellationToken ct = default)
    {
        var actorId = currentUser.UserId ?? 0;
        var companyId = currentUser.CompanyId
            ?? throw new InvalidOperationException("Contesto azienda mancante.");

        var customer = await db.Customers
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted && c.CompanyId == companyId, ct);
        if (customer is null) return null;

        Validate(request.VatNumber, request.FiscalCode);

        customer.BusinessName = request.BusinessName.Trim();
        customer.VatNumber = Clean(request.VatNumber);
        customer.FiscalCode = Clean(request.FiscalCode)?.ToUpperInvariant();
        customer.SdiCode = Clean(request.SdiCode)?.ToUpperInvariant();
        customer.PecEmail = Clean(request.PecEmail);
        customer.Email = Clean(request.Email);
        customer.Phone = Clean(request.Phone);
        customer.Address = Clean(request.Address);
        customer.City = Clean(request.City);
        customer.Province = Clean(request.Province)?.ToUpperInvariant();
        customer.PostalCode = Clean(request.PostalCode);
        customer.Country = string.IsNullOrWhiteSpace(request.Country) ? "IT" : request.Country.Trim().ToUpperInvariant();
        customer.Iban = Clean(request.Iban)?.Replace(" ", "").ToUpperInvariant();
        customer.PaymentTerms = Clean(request.PaymentTerms);
        customer.Notes = Clean(request.Notes);
        customer.Status = request.Status;
        customer.UpdatedAt = DateTime.UtcNow;
        customer.UpdatedBy = actorId;
        await db.SaveChangesAsync(ct);
        return Map(customer);
    }

    public async Task<bool> SetEnabledAsync(long id, bool enabled, CancellationToken ct = default)
    {
        var companyId = currentUser.CompanyId
            ?? throw new InvalidOperationException("Contesto azienda mancante.");

        var customer = await db.Customers
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted && c.CompanyId == companyId, ct);
        if (customer is null) return false;

        customer.Status = enabled ? "Active" : "Inactive";
        customer.UpdatedAt = DateTime.UtcNow;
        customer.UpdatedBy = currentUser.UserId;
        await db.SaveChangesAsync(ct);
        return true;
    }

    // ===================== Aggregato a schede =====================
    public async Task<CustomerAggregate?> GetFullAsync(long id, CancellationToken ct = default)
    {
        var companyId = currentUser.CompanyId
            ?? throw new InvalidOperationException("Contesto azienda mancante.");

        var c = await db.Customers
            .Include(x => x.Addresses)
            .Include(x => x.Contacts)
            .Include(x => x.Banks)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        return c is null ? null : MapAggregate(c);
    }

    public async Task<CustomerAggregate> CreateFullAsync(CreateCustomerAggregateRequest request, CancellationToken ct = default)
    {
        var actorId = currentUser.UserId ?? 0;
        var companyId = currentUser.CompanyId
            ?? throw new InvalidOperationException("Contesto azienda mancante.");

        var core = request.Core;
        Validate(core.VatNumber, core.FiscalCode);

        var code = core.Code.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException("Il codice cliente e obbligatorio.");
        if (await db.Customers.AnyAsync(x => x.Code == code && x.CompanyId == companyId && !x.IsDeleted, ct))
            throw new InvalidOperationException("Codice cliente gia in uso.");

        var customer = new Customer
        {
            CompanyId = companyId,
            Code = code,
            BusinessName = core.BusinessName.Trim(),
            VatNumber = Clean(core.VatNumber),
            FiscalCode = Clean(core.FiscalCode)?.ToUpperInvariant(),
            SdiCode = Clean(core.SdiCode)?.ToUpperInvariant(),
            PecEmail = Clean(core.PecEmail),
            Email = Clean(core.Email),
            Phone = Clean(core.Phone),
            Address = Clean(core.Address),
            City = Clean(core.City),
            Province = Clean(core.Province)?.ToUpperInvariant(),
            PostalCode = Clean(core.PostalCode),
            Country = string.IsNullOrWhiteSpace(core.Country) ? "IT" : core.Country.Trim().ToUpperInvariant(),
            Iban = Clean(core.Iban)?.Replace(" ", "").ToUpperInvariant(),
            PaymentTerms = Clean(core.PaymentTerms),
            Notes = Clean(core.Notes),
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = actorId
        };
        ApplyAccounting(customer, request.Accounting);

        foreach (var a in request.Addresses) customer.Addresses.Add(NewAddress(a, companyId, actorId));
        foreach (var k in request.Contacts) customer.Contacts.Add(NewContact(k, companyId, actorId));
        foreach (var b in request.Banks) customer.Banks.Add(NewBank(b, companyId, actorId));

        db.Customers.Add(customer);
        await db.SaveChangesAsync(ct);
        return MapAggregate(customer);
    }

    public async Task<CustomerAggregate?> SaveFullAsync(long id, SaveCustomerAggregateRequest request, CancellationToken ct = default)
    {
        var actorId = currentUser.UserId ?? 0;
        var companyId = currentUser.CompanyId
            ?? throw new InvalidOperationException("Contesto azienda mancante.");

        var customer = await db.Customers
            .Include(x => x.Addresses)
            .Include(x => x.Contacts)
            .Include(x => x.Banks)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        if (customer is null) return null;

        var core = request.Core;
        Validate(core.VatNumber, core.FiscalCode);

        customer.BusinessName = core.BusinessName.Trim();
        customer.VatNumber = Clean(core.VatNumber);
        customer.FiscalCode = Clean(core.FiscalCode)?.ToUpperInvariant();
        customer.SdiCode = Clean(core.SdiCode)?.ToUpperInvariant();
        customer.PecEmail = Clean(core.PecEmail);
        customer.Email = Clean(core.Email);
        customer.Phone = Clean(core.Phone);
        customer.Address = Clean(core.Address);
        customer.City = Clean(core.City);
        customer.Province = Clean(core.Province)?.ToUpperInvariant();
        customer.PostalCode = Clean(core.PostalCode);
        customer.Country = string.IsNullOrWhiteSpace(core.Country) ? "IT" : core.Country.Trim().ToUpperInvariant();
        customer.Iban = Clean(core.Iban)?.Replace(" ", "").ToUpperInvariant();
        customer.PaymentTerms = Clean(core.PaymentTerms);
        customer.Notes = Clean(core.Notes);
        customer.Status = core.Status;
        customer.UpdatedAt = DateTime.UtcNow;
        customer.UpdatedBy = actorId;
        ApplyAccounting(customer, request.Accounting);

        SyncAddresses(customer, request.Addresses, companyId, actorId);
        SyncContacts(customer, request.Contacts, companyId, actorId);
        SyncBanks(customer, request.Banks, companyId, actorId);

        await db.SaveChangesAsync(ct);
        return MapAggregate(customer);
    }

    private static void ApplyAccounting(Customer c, CustomerAccounting a)
    {
        c.PaymentMethod = Clean(a.PaymentMethod);
        c.PriceListCode = Clean(a.PriceListCode);
        c.AgentCode = Clean(a.AgentCode);
        c.Zone = Clean(a.Zone);
        c.CreditLimit = a.CreditLimit;
        c.DiscountPercent = a.DiscountPercent;
        c.SplitPayment = a.SplitPayment;
        c.WithholdingTax = a.WithholdingTax;
        c.VatExemptionCode = Clean(a.VatExemptionCode);
        c.AccountCode = Clean(a.AccountCode);
    }

    private void SyncAddresses(Customer c, IReadOnlyList<CustomerAddressDto> incoming, long companyId, long actorId)
    {
        var keepIds = incoming.Where(i => i.Id > 0).Select(i => i.Id).ToHashSet();
        foreach (var ex in c.Addresses.Where(e => !keepIds.Contains(e.Id)).ToList())
            db.CustomerAddresses.Remove(ex);
        foreach (var dto in incoming)
        {
            var ex = dto.Id > 0 ? c.Addresses.FirstOrDefault(e => e.Id == dto.Id) : null;
            if (ex is null) { c.Addresses.Add(NewAddress(dto, companyId, actorId)); continue; }
            ex.AddressType = dto.AddressType; ex.Description = dto.Description?.Trim() ?? "";
            ex.Address = Clean(dto.Address); ex.City = Clean(dto.City);
            ex.Province = Clean(dto.Province)?.ToUpperInvariant(); ex.PostalCode = Clean(dto.PostalCode);
            ex.Country = string.IsNullOrWhiteSpace(dto.Country) ? "IT" : dto.Country.Trim().ToUpperInvariant();
            ex.Phone = Clean(dto.Phone); ex.Email = Clean(dto.Email); ex.IsDefault = dto.IsDefault;
            ex.UpdatedAt = DateTime.UtcNow; ex.UpdatedBy = actorId;
        }
    }

    private void SyncContacts(Customer c, IReadOnlyList<CustomerContactDto> incoming, long companyId, long actorId)
    {
        var keepIds = incoming.Where(i => i.Id > 0).Select(i => i.Id).ToHashSet();
        foreach (var ex in c.Contacts.Where(e => !keepIds.Contains(e.Id)).ToList())
            db.CustomerContacts.Remove(ex);
        foreach (var dto in incoming)
        {
            var ex = dto.Id > 0 ? c.Contacts.FirstOrDefault(e => e.Id == dto.Id) : null;
            if (ex is null) { c.Contacts.Add(NewContact(dto, companyId, actorId)); continue; }
            ex.Name = dto.Name?.Trim() ?? ""; ex.Role = Clean(dto.Role); ex.Email = Clean(dto.Email);
            ex.Phone = Clean(dto.Phone); ex.Mobile = Clean(dto.Mobile); ex.Notes = Clean(dto.Notes);
            ex.IsPrimary = dto.IsPrimary; ex.UpdatedAt = DateTime.UtcNow; ex.UpdatedBy = actorId;
        }
    }

    private void SyncBanks(Customer c, IReadOnlyList<CustomerBankDto> incoming, long companyId, long actorId)
    {
        var keepIds = incoming.Where(i => i.Id > 0).Select(i => i.Id).ToHashSet();
        foreach (var ex in c.Banks.Where(e => !keepIds.Contains(e.Id)).ToList())
            db.CustomerBanks.Remove(ex);
        foreach (var dto in incoming)
        {
            var ex = dto.Id > 0 ? c.Banks.FirstOrDefault(e => e.Id == dto.Id) : null;
            if (ex is null) { c.Banks.Add(NewBank(dto, companyId, actorId)); continue; }
            ex.BankName = dto.BankName?.Trim() ?? ""; ex.Iban = Clean(dto.Iban)?.Replace(" ", "").ToUpperInvariant();
            ex.Swift = Clean(dto.Swift)?.ToUpperInvariant(); ex.Abi = Clean(dto.Abi); ex.Cab = Clean(dto.Cab);
            ex.IsDefault = dto.IsDefault; ex.UpdatedAt = DateTime.UtcNow; ex.UpdatedBy = actorId;
        }
    }

    private static CustomerAddress NewAddress(CustomerAddressDto d, long companyId, long actorId) => new()
    {
        CompanyId = companyId, AddressType = string.IsNullOrWhiteSpace(d.AddressType) ? "Shipping" : d.AddressType,
        Description = d.Description?.Trim() ?? "", Address = Clean(d.Address), City = Clean(d.City),
        Province = Clean(d.Province)?.ToUpperInvariant(), PostalCode = Clean(d.PostalCode),
        Country = string.IsNullOrWhiteSpace(d.Country) ? "IT" : d.Country.Trim().ToUpperInvariant(),
        Phone = Clean(d.Phone), Email = Clean(d.Email), IsDefault = d.IsDefault,
        CreatedAt = DateTime.UtcNow, CreatedBy = actorId
    };

    private static CustomerContact NewContact(CustomerContactDto d, long companyId, long actorId) => new()
    {
        CompanyId = companyId, Name = d.Name?.Trim() ?? "", Role = Clean(d.Role), Email = Clean(d.Email),
        Phone = Clean(d.Phone), Mobile = Clean(d.Mobile), Notes = Clean(d.Notes), IsPrimary = d.IsPrimary,
        CreatedAt = DateTime.UtcNow, CreatedBy = actorId
    };

    private static CustomerBank NewBank(CustomerBankDto d, long companyId, long actorId) => new()
    {
        CompanyId = companyId, BankName = d.BankName?.Trim() ?? "",
        Iban = Clean(d.Iban)?.Replace(" ", "").ToUpperInvariant(), Swift = Clean(d.Swift)?.ToUpperInvariant(),
        Abi = Clean(d.Abi), Cab = Clean(d.Cab), IsDefault = d.IsDefault,
        CreatedAt = DateTime.UtcNow, CreatedBy = actorId
    };

    private static CustomerAggregate MapAggregate(Customer c) => new(
        Map(c),
        new CustomerAccounting(c.PaymentMethod, c.PriceListCode, c.AgentCode, c.Zone, c.CreditLimit,
            c.DiscountPercent, c.SplitPayment, c.WithholdingTax, c.VatExemptionCode, c.AccountCode),
        c.Addresses.OrderByDescending(a => a.IsDefault).ThenBy(a => a.Id)
            .Select(a => new CustomerAddressDto(a.Id, a.AddressType, a.Description, a.Address, a.City, a.Province,
                a.PostalCode, a.Country, a.Phone, a.Email, a.IsDefault)).ToList(),
        c.Contacts.OrderByDescending(k => k.IsPrimary).ThenBy(k => k.Id)
            .Select(k => new CustomerContactDto(k.Id, k.Name, k.Role, k.Email, k.Phone, k.Mobile, k.Notes, k.IsPrimary)).ToList(),
        c.Banks.OrderByDescending(b => b.IsDefault).ThenBy(b => b.Id)
            .Select(b => new CustomerBankDto(b.Id, b.BankName, b.Iban, b.Swift, b.Abi, b.Cab, b.IsDefault)).ToList());

    private static void Validate(string? vat, string? fiscalCode)
    {
        if (!ItalianFiscalValidation.IsValidVatNumber(vat))
            throw new InvalidOperationException("Partita IVA non valida (attese 11 cifre con check digit corretto).");
        if (!ItalianFiscalValidation.IsValidFiscalCode(fiscalCode))
            throw new InvalidOperationException("Codice Fiscale non valido (16 caratteri alfanumerici o 11 cifre).");
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static CustomerDetail Map(Customer c) => new(
        c.Id, c.Code, c.BusinessName, c.VatNumber, c.FiscalCode, c.SdiCode, c.PecEmail,
        c.Email, c.Phone, c.Address, c.City, c.Province, c.PostalCode, c.Country,
        c.Iban, c.PaymentTerms, c.Notes, c.Status);
}
