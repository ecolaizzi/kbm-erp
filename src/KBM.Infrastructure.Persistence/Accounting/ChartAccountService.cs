using KBM.Application.Accounting;
using KBM.Application.Security;
using KBM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KBM.Infrastructure.Persistence.Accounting;

public sealed class ChartAccountService(KbmDbContext db, ICurrentUserContext currentUser) : IChartAccountService
{
    public async Task<IReadOnlyList<ChartAccountListItem>> ListAsync(string? search = null, bool postableOnly = false, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var query = db.AccountMasters.Where(a => !a.IsDeleted && a.CompanyId == companyId);
        if (postableOnly) query = query.Where(a => a.AllowsPosting);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(a => a.FullCode.Contains(term) || a.Name.Contains(term));
        }
        return await query.OrderBy(a => a.FullCode)
            .Select(a => new ChartAccountListItem(a.Id, a.ParentId, (int)a.Level, a.Code, a.FullCode, a.Name,
                (int)a.Nature, (int)a.Sign, (int)a.SubKind, a.AllowsPosting, a.Status))
            .ToListAsync(ct);
    }

    public async Task<ChartAccountDetail?> GetAsync(long id, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var a = await db.AccountMasters.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        return a is null ? null : Map(a);
    }

    public async Task<ChartAccountDetail> CreateAsync(CreateChartAccountRequest request, CancellationToken ct = default)
    {
        var actorId = currentUser.UserId ?? 0;
        var companyId = CompanyId();
        var code = request.Code.Trim();
        if (string.IsNullOrWhiteSpace(code)) throw new InvalidOperationException("Il codice e obbligatorio.");

        AccountMaster? parent = null;
        if (request.ParentId is > 0)
        {
            parent = await db.AccountMasters.FirstOrDefaultAsync(x => x.Id == request.ParentId && !x.IsDeleted && x.CompanyId == companyId, ct)
                ?? throw new InvalidOperationException("Voce padre non trovata.");
            if (parent.Level == AccountLevel.Sottoconto)
                throw new InvalidOperationException("Non e possibile creare voci sotto un sottoconto.");
        }

        var level = parent is null ? AccountLevel.Mastro : (AccountLevel)((int)parent.Level + 1);
        var nature = parent?.Nature ?? (AccountNature)(request.Nature ?? (int)AccountNature.Patrimoniale);
        var fullCode = parent is null ? code : $"{parent.FullCode}.{code}";

        if (await db.AccountMasters.AnyAsync(x => x.FullCode == fullCode && x.CompanyId == companyId && !x.IsDeleted, ct))
            throw new InvalidOperationException($"Codice conto '{fullCode}' gia in uso.");

        var entity = new AccountMaster
        {
            CompanyId = companyId,
            ParentId = parent?.Id,
            Level = level,
            Code = code,
            FullCode = fullCode,
            Name = request.Name.Trim(),
            Nature = nature,
            Sign = (AccountSign)(request.Sign == 0 ? (int)(parent?.Sign ?? AccountSign.Dare) : request.Sign),
            SubKind = (AccountSubKind)request.SubKind,
            AllowsPosting = level == AccountLevel.Sottoconto,
            BilCeeDare = Clean(request.BilCeeDare),
            BilCeeAvere = Clean(request.BilCeeAvere),
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = actorId
        };
        db.AccountMasters.Add(entity);
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<ChartAccountDetail?> UpdateAsync(long id, UpdateChartAccountRequest request, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var a = await db.AccountMasters.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        if (a is null) return null;
        a.Name = request.Name.Trim();
        a.Sign = (AccountSign)(request.Sign == 0 ? (int)a.Sign : request.Sign);
        a.SubKind = (AccountSubKind)request.SubKind;
        a.BilCeeDare = Clean(request.BilCeeDare);
        a.BilCeeAvere = Clean(request.BilCeeAvere);
        a.Status = string.IsNullOrWhiteSpace(request.Status) ? a.Status : request.Status;
        a.UpdatedAt = DateTime.UtcNow;
        a.UpdatedBy = currentUser.UserId;
        await db.SaveChangesAsync(ct);
        return Map(a);
    }

    public async Task<bool> SetEnabledAsync(long id, bool enabled, CancellationToken ct = default)
    {
        var companyId = CompanyId();
        var a = await db.AccountMasters.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.CompanyId == companyId, ct);
        if (a is null) return false;
        a.Status = enabled ? "Active" : "Inactive";
        a.UpdatedAt = DateTime.UtcNow;
        a.UpdatedBy = currentUser.UserId;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<int> SeedStandardAsync(CancellationToken ct = default)
    {
        var companyId = CompanyId();
        if (await db.AccountMasters.AnyAsync(a => a.CompanyId == companyId && !a.IsDeleted, ct)) return 0;

        var actorId = currentUser.UserId ?? 0;
        var now = DateTime.UtcNow;
        var added = new List<AccountMaster>();

        AccountMaster Add(AccountMaster? parent, string code, string name, AccountNature nature, AccountSign sign, AccountSubKind kind = AccountSubKind.Standard)
        {
            var level = parent is null ? AccountLevel.Mastro : (AccountLevel)((int)parent.Level + 1);
            var node = new AccountMaster
            {
                CompanyId = companyId,
                Parent = parent,
                Level = level,
                Code = code,
                FullCode = parent is null ? code : $"{parent.FullCode}.{code}",
                Name = name,
                Nature = parent?.Nature ?? nature,
                Sign = sign,
                SubKind = kind,
                AllowsPosting = level == AccountLevel.Sottoconto,
                Status = "Active",
                CreatedAt = now,
                CreatedBy = actorId
            };
            added.Add(node);
            return node;
        }

        // Stato patrimoniale attivo
        var immob = Add(null, "10", "Immobilizzazioni", AccountNature.Patrimoniale, AccountSign.Dare);
        var cImmob = Add(immob, "01", "Immobilizzazioni materiali", AccountNature.Patrimoniale, AccountSign.Dare);
        Add(cImmob, "0001", "Macchinari e attrezzature", AccountNature.Patrimoniale, AccountSign.Dare);

        var clienti = Add(null, "13", "Crediti verso clienti", AccountNature.Patrimoniale, AccountSign.Dare);
        var cClienti = Add(clienti, "01", "Clienti Italia", AccountNature.Patrimoniale, AccountSign.Dare);
        Add(cClienti, "0001", "Clienti diversi", AccountNature.Patrimoniale, AccountSign.Dare, AccountSubKind.Cliente);

        var liquide = Add(null, "15", "Disponibilita liquide", AccountNature.Patrimoniale, AccountSign.Dare);
        var cBanche = Add(liquide, "01", "Banche", AccountNature.Patrimoniale, AccountSign.Dare);
        Add(cBanche, "0001", "Banca c/c", AccountNature.Patrimoniale, AccountSign.Dare, AccountSubKind.Banca);
        var cCassa = Add(liquide, "02", "Cassa", AccountNature.Patrimoniale, AccountSign.Dare);
        Add(cCassa, "0001", "Cassa contanti", AccountNature.Patrimoniale, AccountSign.Dare, AccountSubKind.Cassa);

        // Stato patrimoniale passivo
        var fornitori = Add(null, "23", "Debiti verso fornitori", AccountNature.Patrimoniale, AccountSign.Avere);
        var cFornitori = Add(fornitori, "01", "Fornitori Italia", AccountNature.Patrimoniale, AccountSign.Avere);
        Add(cFornitori, "0001", "Fornitori diversi", AccountNature.Patrimoniale, AccountSign.Avere, AccountSubKind.Fornitore);

        var erarioIva = Add(null, "40", "Erario c/IVA", AccountNature.Patrimoniale, AccountSign.Avere);
        var cIva = Add(erarioIva, "01", "IVA", AccountNature.Patrimoniale, AccountSign.Avere);
        Add(cIva, "0001", "IVA ns/debito", AccountNature.Patrimoniale, AccountSign.Avere, AccountSubKind.Iva);
        Add(cIva, "0002", "IVA ns/credito", AccountNature.Patrimoniale, AccountSign.Dare, AccountSubKind.Iva);

        // Conto economico
        var ricavi = Add(null, "30", "Ricavi delle vendite", AccountNature.Economico, AccountSign.Avere);
        var cRicavi = Add(ricavi, "01", "Ricavi vendite", AccountNature.Economico, AccountSign.Avere);
        Add(cRicavi, "0001", "Ricavi vendita merci", AccountNature.Economico, AccountSign.Avere);

        var costi = Add(null, "32", "Costi per acquisti", AccountNature.Economico, AccountSign.Dare);
        var cCosti = Add(costi, "01", "Acquisti", AccountNature.Economico, AccountSign.Dare);
        Add(cCosti, "0001", "Acquisti merci", AccountNature.Economico, AccountSign.Dare);

        db.AccountMasters.AddRange(added);
        await db.SaveChangesAsync(ct);
        return added.Count;
    }

    private long CompanyId() => currentUser.CompanyId ?? throw new InvalidOperationException("Contesto azienda mancante.");
    private static string? Clean(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

    private static ChartAccountDetail Map(AccountMaster a) => new(
        a.Id, a.ParentId, (int)a.Level, a.Code, a.FullCode, a.Name, (int)a.Nature, (int)a.Sign, (int)a.SubKind,
        a.AllowsPosting, a.BilCeeDare, a.BilCeeAvere, a.Status);
}
