# KBM — Database Naming Conventions

**Versione**: 1.0
**Data**: 2026-06-09
**Owner**: Database Architect Agent
**Target DB**: SQL Server 2019+ (preferred 2022)
**ORM**: EF Core 8 Code First

---

## 1. Principi generali

- **Lingua**: inglese per tutti gli oggetti di schema (tabelle, colonne, indici, constraint). I valori dei dati (es. anagrafica clienti) restano in italiano: la lingua del codice non è la lingua del dominio.
- **Case style**: `PascalCase` ovunque (no snake_case, no kebab-case).
- **Singolare**: nomi tabella al singolare (`User`, non `Users`; `SalesOrder`, non `SalesOrders`).
- **No abbreviazioni** non standard. Solo abbreviazioni universalmente note: `Id`, `Url`, `Vat`, `Iban`, `Bic`, `Pec`, `Sdi`, `Cf`, `Iva`.
- **No prefissi hungarian** (`tbl`, `vw`, `sp` → vietati).
- **Schema SQL**: solo `dbo` in MVP. Multi-schema (`audit`, `sales`, ...) rimandato post-MVP.
- **Reserved words**: evitare collisioni con keyword T-SQL (es. `User` è riservata → si usa comunque ma sempre quotata `[User]` nelle script). EF Core gestisce il quoting automaticamente.

---

## 2. Tabelle

| Regola | Esempio | Anti-pattern |
|---|---|---|
| PascalCase singular | `Customer`, `SalesOrder`, `InventoryMovement` | `customers`, `sales_orders` |
| Tabelle di join: `<A><B>` o nome semantico | `UserRole`, `RolePermission` | `User_Role`, `tbl_user_role` |
| Tabelle di dettaglio: suffisso `Line` | `SalesOrderLine`, `PurchaseOrderLine` | `SalesOrderDetail`, `OrderItems` |
| Tabelle di sistema / lookup globali: prefisso `Sys` opzionale | `Country`, `Currency`, `Province` (no prefisso, sono entità) | — |
| Tabelle di configurazione tenant-aware: suffisso `Setting` | `CompanySetting` | `Configs` |

---

## 3. Colonne

| Categoria | Convenzione | Esempio |
|---|---|---|
| Primary key | `Id` (sempre, mai `{Table}Id` come PK) | `User.Id`, `Customer.Id` |
| Foreign key | `<ReferencedTable>Id` | `Customer.CompanyId`, `SalesOrder.CustomerId` |
| Self-reference | `Parent<Table>Id` o ruolo semantico | `ItemCategory.ParentCategoryId` |
| Boolean | `Is<Adjective>` o `Has<Noun>` | `IsActive`, `IsDeleted`, `HasVat` |
| Date/time | `<Verb>At` (istante) / `<Noun>Date` (data logica) | `CreatedAt`, `OrderDate`, `DueDate` |
| Importi monetari | `<Concept>Amount` | `NetAmount`, `VatAmount`, `TotalAmount` |
| Quantità | `<Concept>Quantity` o `Qty<Concept>` | `OrderedQuantity`, `DeliveredQuantity` |
| Codici utente | `Code` (univoco per CompanyId) | `Customer.Code = "CLI-0001"` |
| Descrizioni / nomi | `Name`, `Description`, `Notes` | — |
| Status / enum | `Status` (stringa breve o int FK a lookup) | `SalesOrder.Status = 'Confirmed'` |
| Row version | `RowVersion` (tipo `ROWVERSION`/`TIMESTAMP`) | — |

**Tipi standard**:
- Chiavi: `BIGINT IDENTITY(1,1)` (no `INT`: cresce nel tempo, no `UNIQUEIDENTIFIER` salvo casi specifici di sync esterno).
- Codici utente: `NVARCHAR(32)`.
- Nomi/ragione sociale: `NVARCHAR(200)`.
- Descrizioni libere: `NVARCHAR(500)` o `NVARCHAR(MAX)` per note lunghe.
- P.IVA: `NVARCHAR(20)` (gestisce VAT EU/extra-EU).
- Codice Fiscale: `NVARCHAR(16)`.
- IBAN: `NVARCHAR(34)`.
- Email: `NVARCHAR(254)` (RFC 5321).
- Importi: `DECIMAL(18,4)` per importi unitari/prezzi, `DECIMAL(18,2)` per totali documento, `DECIMAL(18,6)` per cambi valuta.
- Quantità: `DECIMAL(18,4)`.
- Percentuali (sconti, IVA): `DECIMAL(5,2)`.
- Date/orari: `DATETIME2(3)` (precisione ms, no `DATETIME` legacy).
- Date pure (senza orario): `DATE`.
- Json: `NVARCHAR(MAX)` con `CHECK (ISJSON(<col>) = 1)`.

---

## 4. Constraint & Index

| Oggetto | Pattern | Esempio |
|---|---|---|
| Primary key | `PK_<Table>` | `PK_Customer` |
| Foreign key | `FK_<Table>_<Referenced>[_<Role>]` | `FK_SalesOrder_Customer`, `FK_User_User_CreatedBy` |
| Unique | `UQ_<Table>_<Columns>` | `UQ_Customer_CompanyId_Code` |
| Check | `CK_<Table>_<Rule>` | `CK_Customer_VatOrCfPresent` |
| Default | `DF_<Table>_<Column>` | `DF_Customer_IsActive` |
| Index non-cluster | `IX_<Table>_<Col1>[_<Col2>...]` | `IX_SalesOrder_CompanyId_Status_OrderDate` |
| Index filtered | `IX_<Table>_<Cols>_<Predicate>` | `IX_Customer_CompanyId_Code_Active` (WHERE IsDeleted = 0) |
| Index include | nome base; le colonne `INCLUDE` non entrano nel nome |  |
| Index covering | suffisso `_Cov` opzionale | `IX_SalesOrder_CompanyId_CustomerId_Cov` |

**Ordine colonne negli indici composti**: il discriminante `CompanyId` è sempre il **primo** campo per ogni indice multi-tenant (massimizza il filtro tenant prima di ogni altra valutazione).

---

## 5. Migrations EF Core

- Formato: `YYYYMMDDHHmmss_<DescriptiveName>` (timestamp UTC).
- Naming **imperativo, conciso, semantico**: `20260615091500_AddCustomerVatLookupColumns`, non `Update1`.
- Una migration per logical change set; mai mischiare modifiche scorrelate.
- Dettagli completi nel documento `migration-strategy.md`.

---

## 6. Stored procedures, viste, funzioni (uso minimale)

| Tipo | Prefisso | Esempio |
|---|---|---|
| View | `vw_` (eccezione al no-prefix per disambiguare) | `vw_InventoryStockSnapshot` |
| Stored procedure | `usp_` (user stored procedure) | `usp_RebuildInventoryStock` |
| Function scalar | `fn_` | `fn_ValidateItalianVat` |
| Function table-valued | `tvf_` | `tvf_GetCustomerExposure` |

Stored procedure e function: **uso sconsigliato in MVP** (logica nel layer applicativo). Ammessi solo per maintenance/reporting batch.

---

## 7. Anti-pattern proibiti

- ❌ Nomi italiani (`Clienti`, `OrdineVendita`).
- ❌ Plurali (`Customers`).
- ❌ Prefissi hungarian (`tblCustomer`, `vwOrders`).
- ❌ FK senza nome esplicito (lasciare auto-naming EF non garantisce stabilità across migrations).
- ❌ `INT` per PK (limite 2.1 mld → si arriva, e refactor in produzione è doloroso).
- ❌ `FLOAT`/`REAL` per importi monetari (imprecisione binaria).
- ❌ `CHAR` di lunghezza fissa (sprecato per dati variabili).
- ❌ Colonne nullable senza ragione esplicita documentata.
- ❌ `SELECT *` in viste/SP (vincola schema evolution).
