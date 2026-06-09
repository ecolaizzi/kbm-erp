# KBM — Schema Design Document

**Versione**: 1.0
**Data**: 2026-06-09
**Owner**: Database Architect Agent
**Target**: SQL Server 2019+ (preferred 2022)
**ORM**: EF Core 8 Code First
**Scope**: MVP (Fase 1 Core + Fase 2 Anagrafiche) + schema preview Transactions/Inventory

---

## 1. Convenzioni rapide

- Schema: `dbo` (unico in MVP).
- Tutti gli identificatori in inglese, PascalCase singular. Dettagli completi in `naming-conventions.md`.
- Base Entity Pattern applicato a tutte le entità di dominio (vedi §2).
- Multi-tenancy via `CompanyId` su ogni tabella operativa.
- Soft delete via `IsDeleted` (escluso da query EF tramite Global Query Filter).
- Concorrenza optimistic via colonna `RowVersion` (`ROWVERSION` SQL Server).
- Tutti gli importi monetari `DECIMAL`. Tutte le date applicative `DATETIME2(3)` o `DATE`.

---

## 2. Base Entity Pattern

Tutte le entità di dominio modificabili ereditano da `AuditableTenantEntity`:

| Colonna | Tipo SQL | Nullable | Default | Note |
|---|---|---|---|---|
| `Id` | `BIGINT IDENTITY(1,1)` | NO | — | PK clustered |
| `CompanyId` | `BIGINT` | NO | — | FK → `Company.Id` |
| `CreatedAt` | `DATETIME2(3)` | NO | `SYSUTCDATETIME()` | UTC, impostato da app |
| `CreatedBy` | `BIGINT` | NO | — | FK → `User.Id` |
| `UpdatedAt` | `DATETIME2(3)` | YES | — | Aggiornato da app a ogni update |
| `UpdatedBy` | `BIGINT` | YES | — | FK → `User.Id` |
| `IsDeleted` | `BIT` | NO | `0` | Soft delete |
| `RowVersion` | `ROWVERSION` | NO | (system) | Concorrenza optimistic |

**Eccezioni** (entità che NON seguono il pattern completo):
- `AuditLog`: append-only — no `UpdatedAt`, `UpdatedBy`, `IsDeleted`, `RowVersion`.
- `InventoryMovement`: append-only — no `UpdatedAt`, `UpdatedBy`, `IsDeleted`, `RowVersion`.
- `RefreshToken`: append-only con `RevokedAt` per revoche — no `UpdatedAt`, `UpdatedBy`, `IsDeleted`, `RowVersion`.
- `Permission`: seed-only globale — no `CompanyId`, no audit, no soft delete.
- `Country`, `Currency`, `Province`, `UnitOfMeasure`: lookup globali — no `CompanyId`, no audit, no soft delete.
- `Company` stessa: `CompanyId` non si applica (è la root). Mantiene il resto del pattern.
- `User` globale: `CompanyId` non si applica (user vive cross-company); accesso ai tenant via `UserCompany`.

---

## 3. Core Platform

### 3.1 Company

| Colonna | Tipo | Nullable | Note |
|---|---|---|---|
| `Id` | `BIGINT IDENTITY` | NO | PK |
| `Code` | `NVARCHAR(32)` | NO | UQ globale |
| `BusinessName` | `NVARCHAR(200)` | NO | |
| `LegalName` | `NVARCHAR(200)` | YES | |
| `VatNumber` | `NVARCHAR(20)` | YES | P.IVA italiana o VAT EU |
| `FiscalCode` | `NVARCHAR(16)` | YES | CF |
| `Sdi` | `NVARCHAR(7)` | YES | Codice destinatario SDI |
| `Pec` | `NVARCHAR(254)` | YES | |
| `CountryId` | `BIGINT` | NO | FK → `Country.Id` |
| `DefaultCurrencyId` | `BIGINT` | NO | FK → `Currency.Id` |
| `Status` | `NVARCHAR(16)` | NO | CHECK IN (`Active`,`Suspended`,`Closed`) |
| `LogoFileId` | `BIGINT` | YES | FK → `FileBlob.Id` (post-MVP) |
| ...campi audit, IsDeleted, RowVersion (vedi §2) | | | |

**Constraint**:
- `PK_Company` (Id)
- `UQ_Company_Code` (Code) WHERE IsDeleted = 0
- `FK_Company_Country` (CountryId) → Country(Id)
- `FK_Company_Currency` (DefaultCurrencyId) → Currency(Id)
- `CK_Company_Status`

**Indexes**: solo PK + UQ_Code.

### 3.2 User

| Colonna | Tipo | Nullable | Note |
|---|---|---|---|
| `Id` | `BIGINT IDENTITY` | NO | PK |
| `Username` | `NVARCHAR(64)` | NO | UQ globale |
| `Email` | `NVARCHAR(254)` | NO | UQ globale |
| `PasswordHash` | `NVARCHAR(255)` | NO | BCrypt/Argon2 |
| `FirstName` | `NVARCHAR(100)` | NO | |
| `LastName` | `NVARCHAR(100)` | NO | |
| `Status` | `NVARCHAR(16)` | NO | CHECK IN (`Active`,`Locked`,`Disabled`,`PendingActivation`) |
| `MfaEnabled` | `BIT` | NO | DEFAULT 0 |
| `MfaSecret` | `NVARCHAR(255)` | YES | Cipher text (AES-GCM) |
| `LastLoginAt` | `DATETIME2(3)` | YES | |
| `FailedLoginAttempts` | `INT` | NO | DEFAULT 0 |
| `LockedUntil` | `DATETIME2(3)` | YES | |
| `PasswordChangedAt` | `DATETIME2(3)` | NO | DEFAULT `SYSUTCDATETIME()` |
| `PreferredLanguage` | `NVARCHAR(8)` | NO | DEFAULT `'it-IT'` |
| ...campi audit, IsDeleted, RowVersion | | | |

**Constraint**:
- `UQ_User_Username` filtered IsDeleted = 0
- `UQ_User_Email` filtered IsDeleted = 0
- `CK_User_Status`

> `User` **non** ha `CompanyId`: vive globalmente. L'appartenenza ai tenant è in `UserCompany`.

### 3.3 UserCompany

| Colonna | Tipo | Nullable | Note |
|---|---|---|---|
| `Id` | `BIGINT IDENTITY` | NO | PK |
| `UserId` | `BIGINT` | NO | FK → User.Id |
| `CompanyId` | `BIGINT` | NO | FK → Company.Id |
| `IsDefault` | `BIT` | NO | DEFAULT 0 |
| `CreatedAt`, `CreatedBy` | (vedi §2) | | |

**Constraint**:
- `UQ_UserCompany_User_Company` (UserId, CompanyId)
- Trigger applicativo: garantisce al più una riga `IsDefault = 1` per utente.

### 3.4 Role

| Colonna | Tipo | Nullable | Note |
|---|---|---|---|
| `Id` | `BIGINT IDENTITY` | NO | PK |
| `CompanyId` | `BIGINT` | YES | NULL = ruolo di sistema globale |
| `Code` | `NVARCHAR(64)` | NO | |
| `Name` | `NVARCHAR(100)` | NO | |
| `Description` | `NVARCHAR(500)` | YES | |
| `IsSystem` | `BIT` | NO | DEFAULT 0; `IsSystem = 1` ⇒ CompanyId NULL |
| ...campi audit, IsDeleted, RowVersion | | | |

**Constraint**:
- `UQ_Role_Company_Code` (CompanyId, Code) filtered IsDeleted = 0
  - Per ruoli sistema (CompanyId NULL) il filter consente univocità globale.
- `CK_Role_SystemHasNullCompany`: `(IsSystem = 1 AND CompanyId IS NULL) OR (IsSystem = 0 AND CompanyId IS NOT NULL)`

### 3.5 Permission

| Colonna | Tipo | Nullable | Note |
|---|---|---|---|
| `Id` | `BIGINT IDENTITY` | NO | PK |
| `Code` | `NVARCHAR(128)` | NO | UQ — formato `module.entity.action` |
| `Module` | `NVARCHAR(64)` | NO | |
| `Description` | `NVARCHAR(255)` | NO | |

Solo seed, immutabile dopo deploy migration.

### 3.6 UserRole, RolePermission

`UserRole`:
- `Id`, `UserId` FK, `RoleId` FK, `CompanyId` FK (scope dell'assegnazione), `CreatedAt`, `CreatedBy`.
- `UQ_UserRole(UserId, RoleId, CompanyId)`.

`RolePermission`:
- `Id`, `RoleId` FK, `PermissionId` FK, `CreatedAt`, `CreatedBy`.
- `UQ_RolePermission(RoleId, PermissionId)`.

### 3.7 RefreshToken

| Colonna | Tipo | Nullable | Note |
|---|---|---|---|
| `Id` | `BIGINT IDENTITY` | NO | PK |
| `UserId` | `BIGINT` | NO | FK → User.Id |
| `CompanyId` | `BIGINT` | NO | FK → Company.Id (contesto attivo del token) |
| `TokenHash` | `NVARCHAR(128)` | NO | SHA-256 hex del token |
| `IssuedAt` | `DATETIME2(3)` | NO | |
| `ExpiresAt` | `DATETIME2(3)` | NO | |
| `RevokedAt` | `DATETIME2(3)` | YES | |
| `RevocationReason` | `NVARCHAR(64)` | YES | |
| `IpAddress` | `NVARCHAR(45)` | YES | IPv4/IPv6 |
| `UserAgent` | `NVARCHAR(500)` | YES | |

`UQ_RefreshToken_TokenHash`. Cleanup job giornaliero rimuove righe con `ExpiresAt < now - 30gg`.

### 3.8 AuditLog (IMMUTABLE)

| Colonna | Tipo | Nullable | Note |
|---|---|---|---|
| `Id` | `BIGINT IDENTITY` | NO | PK |
| `Timestamp` | `DATETIME2(3)` | NO | DEFAULT `SYSUTCDATETIME()` |
| `UserId` | `BIGINT` | YES | NULL per eventi di sistema |
| `CompanyId` | `BIGINT` | YES | NULL per eventi globali (es. login fallito) |
| `Action` | `NVARCHAR(50)` | NO | `Create|Update|Delete|Login|LoginFailed|Logout|Permission*|...` |
| `EntityType` | `NVARCHAR(100)` | YES | Nome entità (FQN tipo .NET o nome tabella) |
| `EntityId` | `BIGINT` | YES | |
| `OldValue` | `NVARCHAR(MAX)` | YES | JSON (`CHECK ISJSON(OldValue)=1`) |
| `NewValue` | `NVARCHAR(MAX)` | YES | JSON (`CHECK ISJSON(NewValue)=1`) |
| `IpAddress` | `NVARCHAR(45)` | YES | |
| `UserAgent` | `NVARCHAR(500)` | YES | |
| `CorrelationId` | `NVARCHAR(64)` | YES | Trace ID per correlare con log applicativi |

**Immutabilità**:
- Nessuna colonna `UpdatedAt/UpdatedBy/RowVersion/IsDeleted`.
- Grant esplicito: solo `INSERT` per service account applicativo; `SELECT` per ruoli `AuditReader`; **niente UPDATE/DELETE** (a livello permessi DB).
- DELETE/PURGE solo via job di retention con account dedicato (es. `AuditPurger`, attivo dopo 5 anni).

### 3.9 CompanySetting

Key-value per parametri runtime per-tenant (formato numerazione documenti, soglie, integrazioni esterne...).

| Colonna | Tipo | Note |
|---|---|---|
| `Id` BIGINT PK | | |
| `CompanyId` BIGINT FK | | |
| `Key` NVARCHAR(128) | | |
| `Value` NVARCHAR(MAX) | | |
| `ValueType` NVARCHAR(16) | CHECK IN (`String`,`Int`,`Decimal`,`Bool`,`Json`) | |
| `UpdatedAt`, `UpdatedBy`, `RowVersion` | | |

`UQ_CompanySetting(CompanyId, Key)`.

---

## 4. Anagrafiche

### 4.1 Lookup geografici/monetari (globali)

**Country**: `Id`, `IsoCode2 NVARCHAR(2) UQ`, `IsoCode3 NVARCHAR(3)`, `Name NVARCHAR(100)`, `IsEuMember BIT`, `PhonePrefix NVARCHAR(8)`.
**Currency**: `Id`, `IsoCode NVARCHAR(3) UQ`, `Name NVARCHAR(64)`, `Symbol NVARCHAR(8)`, `DecimalDigits TINYINT`.
**Province** (Italia + grain configurabile): `Id`, `CountryId FK`, `Code NVARCHAR(8)`, `Name NVARCHAR(100)`. `UQ_Province(CountryId, Code)`.
**UnitOfMeasure**: `Id`, `Code NVARCHAR(8) UQ`, `Name NVARCHAR(64)`, `UnitType NVARCHAR(16)` (`Quantity|Weight|Volume|Length|Time`).
**UnitOfMeasureConversion**: `Id`, `FromUnitId FK`, `ToUnitId FK`, `Factor DECIMAL(18,8)`. `UQ(FromUnitId, ToUnitId)`.

### 4.2 Address (riusabile)

| Colonna | Tipo | Note |
|---|---|---|
| `Id` BIGINT PK | | |
| `CompanyId` BIGINT FK | | tenant scope |
| `Street` NVARCHAR(200) | NO | |
| `StreetExtra` NVARCHAR(200) | YES | scala, interno, c/o |
| `City` NVARCHAR(100) | NO | |
| `ZipCode` NVARCHAR(16) | NO | |
| `ProvinceId` BIGINT FK | YES | |
| `CountryId` BIGINT FK | NO | |
| `Latitude` DECIMAL(9,6) | YES | |
| `Longitude` DECIMAL(9,6) | YES | |
| ...audit, IsDeleted, RowVersion | | |

### 4.3 Customer

Vedi ERD `02-anagraphics.md` per il set completo. Campi salienti aggiuntivi:

| Colonna | Tipo | Nullable | Note |
|---|---|---|---|
| `CustomerCategoryId` | BIGINT FK | YES | |
| `CreditLimit` | DECIMAL(18,2) | YES | esposizione max consentita |
| `DefaultDiscountPercent` | DECIMAL(5,2) | NO | DEFAULT 0 |
| `InvoiceDeliveryMethod` | NVARCHAR(16) | NO | CHECK IN (`Sdi`,`Pec`,`Email`,`Paper`) |
| `IsPriceTaxIncluded` | BIT | NO | DEFAULT 0 — listini IVA esposta inclusa/esclusa |

**Constraint chiave**:
- `UQ_Customer_Company_Code` filtered IsDeleted = 0.
- `CK_Customer_VatOrCfPresent`: per `Type IN ('Company','Individual')` AND `Status <> 'Prospect'` AND `CountryId = IT` ⇒ almeno uno tra `VatNumber`/`FiscalCode` valorizzato.
  - Implementato come CHECK semplice + validazione applicativa per la parte condizionale paese.
- Validazione algoritmica P.IVA IT, CF, SDI: lato applicativo (FluentValidation), non in CHECK constraint.

### 4.4 CustomerAddress / CustomerContact

`CustomerAddress`:
- `Id`, `CompanyId`, `CustomerId` FK, `AddressType` (`Legal|Shipping|Billing|Other`), `IsDefault` BIT, `AddressId` FK → `Address.Id`.
- `UQ` su (CustomerId, AddressType, IsDefault) WHERE IsDefault = 1 (uno solo default per tipo).

`CustomerContact`:
- `Id`, `CompanyId`, `CustomerId` FK, `ContactType` (`Phone|Email|Mobile|Fax|Other`), `ContactName` NVARCHAR(100), `Value` NVARCHAR(254), `Role` NVARCHAR(100), `IsDefault` BIT.

### 4.5 Supplier (+ SupplierAddress, SupplierContact, SupplierBankAccount)

Stessa struttura di Customer (ma niente `AgentUserId`, `CreditLimit`). Aggiunte:
- `VatRegime` NVARCHAR(16): `Ordinary|Forfettario|MinimiPa|Other` (impatta calcolo IVA in fattura passiva).
- `WithholdingTaxApplicable` BIT (ritenuta d'acconto applicabile).

`SupplierBankAccount`: `Id`, `CompanyId`, `SupplierId` FK, `Iban` NVARCHAR(34), `Bic` NVARCHAR(11), `BankName` NVARCHAR(200), `IsDefault` BIT, ...audit.

### 4.6 Item

Vedi ERD per campi. Note:

- `IsStockManaged` discrimina articoli a giacenza (`Goods`) da servizi (`Service`) per cui non si genera `InventoryMovement`.
- `ListPrice` è il prezzo di listino base; i prezzi differenziati vivono in `PriceListItem`.
- `StandardCost` per valorizzazione di default; costo medio reale calcolato in `InventoryStock.AverageCost`.

**ItemAlternativeCode**: gestisce barcode (EAN-13/UPC), codice cliente, codice fornitore (`SupplierId` FK opzionale).
`UQ_ItemAlternativeCode_Company_CodeType_Code` filtered IsDeleted = 0 — per evitare due articoli con stesso barcode nel tenant.

### 4.7 ItemCategory

Albero gerarchico con `ParentCategoryId` self-reference.
- `Id`, `CompanyId`, `Code`, `Name`, `ParentCategoryId` FK (nullable per root), `DisplayOrder` INT.
- Profondità massima consigliata 5 livelli (enforced applicativamente per limitare query ricorsive).

### 4.8 PriceList / PriceListItem

`PriceList`:
- `Id`, `CompanyId`, `Code`, `Name`, `PriceListType` (`Sales|Purchase`), `CurrencyId` FK, `CustomerCategoryId` FK (nullable), `ValidFrom`, `ValidTo`, `Status`.

`PriceListItem`:
- `Id`, `CompanyId`, `PriceListId` FK, `ItemId` FK, `UnitPrice` DECIMAL(18,4), `DiscountPercent` DECIMAL(5,2), `MinQuantity` DECIMAL(18,4) DEFAULT 1, `ValidFrom` DATE, `ValidTo` DATE NULL, `RowVersion`.
- `UQ(PriceListId, ItemId, ValidFrom)`.

### 4.9 CustomerSpecialPrice (US-136)

Tabella dedicata per prezzi speciali cliente↔articolo senza listino dedicato.

| Colonna | Tipo | Note |
|---|---|---|
| `Id` BIGINT PK | | |
| `CompanyId` BIGINT FK | | |
| `CustomerId` BIGINT FK | | |
| `ItemId` BIGINT FK | | |
| `UnitPrice` DECIMAL(18,4) | NO | |
| `DiscountPercent` DECIMAL(5,2) | NO | DEFAULT 0 |
| `ValidFrom` DATE | NO | |
| `ValidTo` DATE | YES | |
| ...audit, IsDeleted, RowVersion | | |

`UQ(CustomerId, ItemId, ValidFrom)`. Logica di precedenza: special price > price list > list price.

### 4.10 PaymentTerm

| Colonna | Tipo | Note |
|---|---|---|
| `Id` BIGINT PK | | |
| `CompanyId` BIGINT FK | NULLABLE — NULL = system default | |
| `Code` NVARCHAR(32) | NO | |
| `Name` NVARCHAR(100) | NO | |
| `PaymentMethod` NVARCHAR(32) | NO | `BankTransfer|RiBa|Cash|Check|SDD|Other` |
| `InstallmentsJson` NVARCHAR(MAX) | NO | JSON `[{"days":30,"percent":50},{"days":60,"percent":50}]`. CHECK ISJSON |
| `EndOfMonth` BIT | NO | DEFAULT 0 |
| `ExtraDays` INT | NO | DEFAULT 0 |
| ...audit, IsDeleted, RowVersion | | |

`UQ_PaymentTerm_Company_Code` filtered IsDeleted = 0 (CompanyId NULL ammesso per system).

---

## 5. Transactions (preview, scope MVP minimale)

Vedi ERD `03-transactions.md`. Tabelle: `SalesOrder`, `SalesOrderLine`, `PurchaseOrder`, `PurchaseOrderLine`.

**Pattern comuni**:
- `Number` NVARCHAR(32), univoco per CompanyId + tipo documento.
- Snapshot `Description`, `UnitPrice`, `VatPercent`, `UnitOfMeasureId` su ogni riga.
- Quantità tracciate: `OrderedQuantity`, `DeliveredQuantity` (sales) / `ReceivedQuantity` (purchase), `InvoicedQuantity`.
- Importi calcolati su riga: `NetAmount = (UnitPrice * OrderedQuantity) * (1 - LineDiscount/100)`; `VatAmount`; `TotalAmount`. Totali testata aggregati da app a ogni write.
- `Status` testata e riga con CHECK constraint dei valori validi.

**Numerazione**:
- Servizio `INumberingService` riserva numeri da una tabella `DocumentNumberSequence`:
  - `Id`, `CompanyId`, `DocumentType`, `Year`, `Prefix`, `LastNumber`, `NumberFormat NVARCHAR(64)`, `RowVersion`.
  - Riservazione atomica via `UPDATE ... OUTPUT inserted.LastNumber WITH (UPDLOCK, ROWLOCK)`.

---

## 6. Inventory (preview)

Vedi ERD `04-inventory.md`. Tabelle: `Warehouse`, `InventoryMovement`, `InventoryStock`.

- `InventoryMovement` append-only.
- `InventoryStock` aggregato per `(CompanyId, WarehouseId, ItemId)` con `QuantityOnHand`, `QuantityReserved`, `QuantityAvailable` (computed persistente), `AverageCost`.
- Movimenti generati transazionalmente con upsert stock + insert movement.

---

## 7. Concurrency control

- `RowVersion` (`ROWVERSION` SQL Server) su tutte le entità modificabili. EF Core mappa con `IsRowVersion()` su byte[]; concurrency token validato a ogni `SaveChanges`.
- Resolution strategy: **last-write-wins con eccezione**. UI richiede ricarica record e merge esplicito dell'utente in caso di `DbUpdateConcurrencyException`.
- Per operazioni "hot" cross-record (numerazione documenti, aggiornamento stock) si usa lock pessimistico mirato (`UPDLOCK, ROWLOCK` su singolo record + transazione corta).

---

## 8. Multi-tenancy enforcement

1. **DB layer**: ogni FK include `CompanyId` quando la relazione è intra-tenant (es. `SalesOrder.CustomerId` → `Customer.Id` con check applicativo `SalesOrder.CompanyId = Customer.CompanyId`).
2. **EF Core layer**: `KbmDbContext.OnModelCreating` aggiunge `HasQueryFilter(e => e.CompanyId == _tenantContext.CurrentCompanyId && !e.IsDeleted)` per ogni `ITenantEntity`.
3. **Application layer**: middleware risolve `CurrentCompanyId` da JWT claim. `INFRASTRUCTURE bypass` (job scheduler cross-tenant, audit reader) richiede flag esplicito `IgnoreQueryFilters()` e log motivazione.
4. **Test**: suite di sicurezza valida che ogni endpoint con bypass è coperto da test che dimostrano isolamento.

---

## 9. Audit hook

`AuditInterceptor` (EF Core `SaveChangesInterceptor`) intercetta `ChangeTracker`:
- Per ogni entità `Modified` produce `OldValue`/`NewValue` JSON con diff dei soli campi modificati.
- Per `Added`/`Deleted` (soft) registra l'evento.
- Insert in `AuditLog` nella **stessa transazione** dell'entità modificata.
- Eventi di autenticazione (`Login`, `LoginFailed`, `Logout`) inseriti dal middleware auth.

---

## 10. Triggers (uso minimale)

In MVP **nessun trigger** SQL Server. Tutta la logica (audit, validation, computed) sta nel layer applicativo per testabilità e portabilità. Eccezione possibile post-MVP: trigger di immutabilità su `AuditLog` (`INSTEAD OF UPDATE/DELETE` che solleva error) se permessi a livello DB non sono sufficienti.

---

## 11. JSON columns

Uso intenzionale e limitato:
- `AuditLog.OldValue`, `AuditLog.NewValue` — diff dati.
- `CompanySetting.Value` (quando `ValueType='Json'`).
- `PaymentTerm.InstallmentsJson` — definizione rate variabile.

Tutte con `CHECK (ISJSON(<col>) = 1)`. Ricerca dentro JSON via `JSON_VALUE(...)`; indici computed persistiti **solo** se identificato pattern di query specifico (post-MVP).

---

## 12. Temporal Tables (valutazione)

SQL Server 2016+ Temporal Tables (system-versioned) sono **non adottate in MVP**:
- ✅ Pro: history automatica, no codice applicativo.
- ❌ Contro: duplica scopo dell'`AuditLog` (che cattura anche `User`, `IpAddress`, contesto applicativo); aggiunge complessità schema (history tables, period columns); rende migration più rischiose; lo storico include dati sensibili di soft-delete che il GDPR può richiedere di purgare puntualmente.

`AuditLog` resta source of truth. Temporal Tables valutate post-MVP solo per tabelle dove è richiesto query "time-travel" performante (es. report storico prezzi).

---

## 13. Full-Text Search

Non incluso in MVP. Pianificato post-MVP per `Item.Name/Description`, `Customer.BusinessName`. Catalogo dedicato `FTS_KbmCatalog`.

---

## 14. Constraint summary (sintesi)

| Tipo | Pattern | Esempio |
|---|---|---|
| PK | `Id BIGINT IDENTITY PK CLUSTERED` | tutte le tabelle |
| FK intra-tenant | `<Ref>Id` FK + app check uguale CompanyId | `SalesOrder.CustomerId → Customer.Id` |
| FK su lookup globale | semplice FK | `Customer.CountryId → Country.Id` |
| UQ codice | `UQ(CompanyId, Code)` filtered `IsDeleted=0` | `Customer`, `Item`, `Supplier` |
| CK status | `CK_<Table>_Status` IN (...) | tutti gli `Status` |
| CK ISJSON | `CK_<Table>_<Col>Json` | colonne JSON |
| Soft delete filter | `WHERE IsDeleted = 0` su UQ | tutti gli UQ business |

---

## 15. Riferimenti

- ERD: `erd/01-core-platform.md`, `erd/02-anagraphics.md`, `erd/03-transactions.md`, `erd/04-inventory.md`
- Naming: `naming-conventions.md`
- Migration: `migration-strategy.md`
- Indexing: `indexing-strategy.md`
- SQL draft: `scripts/schema-creation.sql`
- Workflow origine: `vfs://org/kbm/deliverables/research/workflows/`
- User stories: `vfs://org/kbm/deliverables/product/backlog/`
