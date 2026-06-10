# SCR-2026-001 — Tabelle e Archivi di base (macro-area Cube `subm0100`)

**Data**: 2026-06-10
**Proponente**: Chief Architect / Product Owner
**Stato**: ✅ Approvato (auto-approvato in fase di scaffolding pre-team)
**Aree condivise impattate**: `migrations`, schema DB (`KbmDbContext`), `KBM.Application.Security.PermissionCodes`, RBAC seed.

## Contesto

In linea con la mappa di parità Business Cube ([`business-cube-parity.md`](../../product/business-cube-parity.md)),
introduciamo la macro-area **Tabelle e Archivi** con archivi di base normalizzati,
riusati da tutti i moduli. Primo incremento: **Condizioni di pagamento**.

## Modifiche richieste

1. **Nuova entità** `PaymentTerm` (tenant-scoped, `AuditableTenantEntity`).
2. **Nuovo `DbSet<PaymentTerm>`** + configurazione fluent in `KbmDbContext`
   (indice unico `(CompanyId, Code)` con filtro soft-delete, come le altre anagrafiche).
3. **Nuova migration** `AddBaseTables` (additiva, nessuna alterazione di tabelle esistenti).
4. **Nuovi permessi** `base.paymentterms.{read,create,edit,delete}` in `PermissionCodes`
   e relativo seed nei ruoli di sistema (Admin/Manager/Operatore/ReadOnly).

## Impatto e mitigazioni

- **Additivo**: nessuna FK aggiunta alle anagrafiche esistenti in questo incremento;
  i campi liberi (es. `Customer.PaymentMethod`) restano invariati. La migrazione a FK
  sarà un SCR successivo dedicato.
- **Migration auto-applicata** allo startup API (`db.Database.MigrateAsync()`): nessuna
  azione manuale sul DB dev `kbmdbdev`.
- **RBAC**: i nuovi permessi vengono seedati in modo incrementale
  (`SyncSystemRolePermissionsAsync`), compatibile con DB già popolati.

## Pattern stabilito per le tabelle successive

Codici IVA, Unità di misura, Zone, Causali seguiranno lo stesso schema:
entità → DbSet+config → service → controller → permessi `base.*` → vista client +
voce nel gruppo di navigazione **Tabelle**.
