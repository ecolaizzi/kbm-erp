# KBM — Migration Strategy

**Versione**: 1.0
**Data**: 2026-06-09
**Owner**: Database Architect Agent
**Tool**: EF Core 8 Code First Migrations
**Target**: SQL Server 2019+ (preferred 2022)

---

## 0. Vincolo timeline (aggiornato 2026-06-09)

**Delivery in 2 mesi (8 settimane)** con strategia **incrementale per modulo**: lo schema completo è disegnato in MVP (fondamenta), ma le migration sono rilasciate in ondate. I moduli non production-ready in fase 1-3 hanno tabelle vuote (schema disponibile, popolamento e logica applicativa in roadmap successive).

| Settimana | Migration set | Modulo | Stato |
|---|---|---|---|
| **W1** | `20260615*_CreateCorePlatformSchema` + `_SeedPermissionsAndSystemRoles` + `_SeedSystemLookups` | Core Platform + lookup globali (Country, Currency, Province, UoM) | Production-ready |
| **W2** | `20260622*_AddCoreSecondaryIndexes` + `_SeedDefaultPaymentTerms` | Core completamento (indici secondari, audit ready) | Production-ready |
| **W3** | `20260629*_CreateAnagraphicsSchema_Part1` (Customer, CustomerCategory, Address) | Anagrafiche clienti | Production-ready |
| **W4** | `20260706*_CreateAnagraphicsSchema_Part2` (Supplier, Item, ItemCategory, PriceList, PriceListItem, ItemAlternativeCode, CustomerSpecialPrice) | Anagrafiche complete | Production-ready |
| **W5** | `20260713*_CreateSalesSchema` (DocumentNumberSequence, SalesOrder, SalesOrderLine) | Ciclo Attivo base | Production-ready |
| **W6** | `20260720*_AddSalesIndexesAndConstraints` + seed demo Sales | Ciclo Attivo completamento + demo data | Production-ready |
| **W7** | `20260727*_CreatePurchaseSchema` (PurchaseOrder, PurchaseOrderLine) + `_CreateInventorySchema` (Warehouse, InventoryMovement, InventoryStock) | Acquisti + Inventory **schema-only** (no demo data, logica applicativa post-MVP) | Schema-ready |
| **W8** | `20260803*_HardeningAndPerformancePass` (Query Store, fill factor tuning, missing index review) | Stabilizzazione | Production-ready |

**Priorità implementazione applicativa**: Core → Anagrafiche → Sales. Purchase + Inventory: schema disponibile, **CRUD applicativo posticipato** se time-box stringe.

**Indici critici subito** (W1-W6): `CompanyId`, FK navigate, `UQ_*(CompanyId, Code)`. **Indici di tuning** (Query Store evidence): consolidati in W8.

**Seed data**:
- W1: Permission catalog completo + system roles (Admin, Manager, User, ReadOnly) + lookup geografici/monetari (Country, Currency, Province IT, UoM standard).
- W2: PaymentTerm di sistema (`Rimessa Diretta`, `RiBa 30gg`, `30/60/90`, `Bonifico 30gg fine mese`).
- W3-W6: demo data per moduli production-ready (3-5 clienti, 5-10 articoli, 1 listino, 1-2 ordini) — solo in ambienti dev/demo, MAI in prod.
- W7+: nessun demo data per Purchase/Inventory.

---

## 1. Approccio generale

Si adotta **EF Core Code First Migrations** come unica source of truth dello schema. Lo schema fisico è derivato dai modelli C# nel progetto `KBM.Domain` (entità) e configurato in `KBM.Infrastructure.Persistence` (DbContext, IEntityTypeConfiguration<T>).

**Principi**:
1. **Code-first, niente DB-first**: lo schema non si modifica mai direttamente in SQL Server; ogni cambio passa da una migration EF.
2. **Reversibile**: ogni migration deve avere `Up` e `Down` implementati e testati.
3. **Idempotente in deployment**: lo script SQL generato (`dotnet ef migrations script --idempotent`) deve poter essere ri-applicato senza errori.
4. **Una migration = un logical change set**: niente mega-migration mensili; niente mix di feature scorrelate.
5. **Mai modificare una migration già rilasciata in produzione**: si crea sempre una nuova migration correttiva.

---

## 2. Layout progetti

```
src/
  KBM.Domain/                       ← entità POCO, value object, enum
  KBM.Infrastructure.Persistence/
    KbmDbContext.cs                 ← unico DbContext applicativo
    Configurations/                 ← una classe IEntityTypeConfiguration<T> per entità
    Migrations/                     ← cartella generata da EF (commitata)
    SeedData/                       ← classi statiche con dati seed
  KBM.Api/                          ← startup, usa Persistence
tools/
  KBM.DbTool/                       ← console app per script generation, seed, smoke test
```

Il DbContext di design-time è esposto in `KBM.Infrastructure.Persistence` con `IDesignTimeDbContextFactory<KbmDbContext>` che legge la connection string da `appsettings.Design.json` (mai dal `KBM.Api`, per evitare dipendenze circolari).

---

## 3. Naming convenzione migration

Formato: `YYYYMMDDHHmmss_<DescriptiveName>` (timestamp UTC, generato automaticamente da EF).

Regole sul `DescriptiveName`:
- PascalCase, imperativo, conciso.
- Inizia con un verbo: `Add`, `Remove`, `Rename`, `Alter`, `Drop`, `Create`, `Seed`, `Backfill`.
- Indica l'entità e il dettaglio: `AddCustomerVatLookupColumns`, non `Update3`.

Esempi:
- `20260615091500_CreateCorePlatformSchema`
- `20260616120000_AddCustomerVatLookupColumns`
- `20260620083000_SeedSystemRolesAndPermissions`
- `20260701150000_AddIxSalesOrderCompanyStatusOrderDate`

Comando:
```bash
dotnet ef migrations add AddCustomerVatLookupColumns \
  --project src/KBM.Infrastructure.Persistence \
  --startup-project src/KBM.Api \
  --output-dir Migrations
```

---

## 4. Workflow di sviluppo

1. **Modifica entità / configurazione** nel codice (`KBM.Domain` o `Configurations/`).
2. **Genera migration**: `dotnet ef migrations add <Name>`.
3. **Ispeziona il file generato** (`Up`/`Down`). Verifica:
   - nessuna `DropColumn` non voluta (rinominati sono di default drop+add → forzare `RenameColumn`),
   - default values corretti per colonne `NOT NULL` aggiunte a tabelle popolate,
   - data migration custom (`migrationBuilder.Sql(...)`) per back-fill quando necessario.
4. **Test locale**: `dotnet ef database update` su DB di sviluppo isolato.
5. **Test reverse**: `dotnet ef database update <PreviousMigration>` per validare `Down`.
6. **Commit**: la cartella `Migrations/` è committata; il `Snapshot.cs` è committato.
7. **CI**: pipeline esegue `dotnet ef migrations script --idempotent` e fa applicare lo script su DB ephemeral (container SQL Server) + smoke test EF.

---

## 5. Deployment

| Ambiente | Modalità | Owner |
|---|---|---|
| Dev (sandbox developer) | `dotnet ef database update` automatico all'avvio app (solo in `Development`) | Developer |
| Test / Staging | Script idempotente applicato da pipeline CD | DevOps |
| Production | Script idempotente generato in CI, **review manuale + approvazione DBA**, applicato in finestra di manutenzione | DBA + Release Manager |

**Mai** eseguire `dotnet ef database update` in produzione: il binding application-DB non è auditabile e non gestisce permessi DDL separati.

Generazione script per produzione:
```bash
dotnet ef migrations script <FromMigration> <ToMigration> \
  --idempotent \
  --output deploy/sql/<YYYYMMDD>_<release>.sql
```

Lo script è committato nel repository di release (`deploy/sql/`) e firmato dal DBA.

---

## 6. Seed data strategy

Due tipologie distinte:

### 6.1 Seed di sistema (immutabile)
Dati che fanno parte della definizione dello schema applicativo: **permissions, system roles, system settings template, lookup geografici (paesi, valute)**.

- Implementati in `OnModelCreating` con `entity.HasData(...)` di EF Core.
- Versionati: ogni modifica genera una migration `Seed*` o `Update*Seed`.
- Identità con valori **negativi o ben sopra il range applicativo** (es. `Id = -1, -2, ...` o `Id ≥ 1_000_000_000`) per non collidere con i record utente.
- Esempi:
  - `Permission` rows (sales.order.create, sales.order.read, ...)
  - `Role` di sistema (Admin, Manager, User, ReadOnly)
  - `Country`, `Currency`, `Province` italiane
  - `PaymentTerm` standard (Rimessa diretta, RiBa 30gg, 30/60/90)

### 6.2 Seed di onboarding (mutabile)
Dati creati per ogni nuovo Tenant/Company: **company default settings, default warehouse, default user admin**.

- **Non** vivono nelle migration EF.
- Eseguiti via servizio applicativo `ITenantProvisioningService` invocato al `CreateCompany`.
- Idempotenti (re-run safe).
- Coperti da test di integrazione.

---

## 7. Casi particolari

### 7.1 Colonna NOT NULL su tabella popolata
1. Migration A: aggiungi colonna `NULL`, default applicativo.
2. Backfill: `migrationBuilder.Sql("UPDATE ... SET ... WHERE ... IS NULL")` nella stessa migration o in una migration dedicata.
3. Migration B: `ALTER COLUMN ... NOT NULL`.

### 7.2 Rinominare colonna/tabella
Usa esplicitamente `migrationBuilder.RenameColumn` / `RenameTable`. EF non lo deduce: senza override genera drop+create che perde i dati.

### 7.3 Index online (production hot tables)
Per tabelle large in produzione (`AuditLog`, `InventoryMovement`):
```csharp
migrationBuilder.Sql(
    "CREATE NONCLUSTERED INDEX IX_AuditLog_CompanyId_Timestamp " +
    "ON dbo.AuditLog (CompanyId, [Timestamp] DESC) " +
    "WITH (ONLINE = ON, MAXDOP = 4);");
```

### 7.4 Breaking change su contratto API
Schema change che rompono il contratto API esistente devono seguire **expand → migrate → contract**:
1. Expand: aggiungi nuova colonna/tabella, app scrive su entrambe.
2. Migrate: backfill batch dei record vecchi, deploy app che legge dalla nuova.
3. Contract: rimuovi colonna/tabella vecchia (migration successiva, dopo almeno 1 release stabile).

---

## 8. Rollback policy

- `Down()` deve essere implementato per **tutte** le migration MVP.
- Rollback in produzione è **emergenza**: prima opzione è sempre **forward fix** (nuova migration correttiva).
- Backup full pre-deploy obbligatorio in prod; restore point garantito a +1h dall'applicazione dello script.

---

## 9. Versioning della migration history

EF mantiene la tabella `__EFMigrationsHistory` (schema `dbo`). **Non modificare manualmente** se non in scenari di disaster recovery, con approvazione DBA e Architect.
