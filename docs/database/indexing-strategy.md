# KBM — Indexing Strategy

**Versione**: 1.0
**Data**: 2026-06-09
**Owner**: Database Architect Agent
**Target**: SQL Server 2019+

---

## 0. Roadmap pragmatica (delivery 8 settimane)

In coerenza con il vincolo "2 mesi" e la migration roadmap, gli indici sono rilasciati in due ondate:

- **Ondata 1 — indici critici (W1–W6, contestuali alle migration di modulo)**
  - PK clustered su ogni tabella (default EF).
  - `CompanyId` come prima colonna di ogni indice multi-tenant.
  - FK navigate (in particolare: `Customer.CompanyId`, `Item.CompanyId`, `SalesOrder.CustomerId`, `*Line.<Parent>Id`).
  - `UQ(CompanyId, Code)` su tutte le entità con codice utente.
  - Indici composti minimi per le pagine "lista" e per i lookup fiscali (`VatNumber`, `FiscalCode`).

- **Ondata 2 — performance tuning (W8, opzionale se tempo)**
  - Covering index `INCLUDE` sulle query più frequenti emerse da Query Store.
  - Indici filtrati avanzati (es. per `Status` specifici).
  - Rivalutazione `FILLFACTOR` su tabelle write-heavy con dati reali.
  - Missing/Unused index review.

Il catalogo completo qui sotto rappresenta lo **stato target finale**. La colonna "Wave" indica se l'indice è W1–W6 (critico, parte della migration del modulo) o W8 (tuning).

---

## 1. Principi guida

1. **`CompanyId` first**: ogni indice non-clustered su tabella multi-tenant ha `CompanyId` come **prima colonna**. Senza questo, il filtro tenant non riesce a sfruttare l'indice.
2. **Clustered = PK identità**: tutte le tabelle MVP hanno PK clustered su `Id BIGINT IDENTITY`. Insert sequenziali → niente page split, minima frammentazione. Eccezione: `AuditLog` valuterà un clustered index su `(Timestamp, Id)` se partizionato per data.
3. **No over-indexing**: ogni indice non-clustered è giustificato da almeno una query frequente nelle pagine d'uso o da una FK navigata regolarmente. Indici "preventivi" senza pattern d'accesso → vietati.
4. **Index a copertura per liste**: pagine "elenco" (es. lista clienti, lista ordini) usano covering index con `INCLUDE` delle colonne mostrate, per evitare key lookup.
5. **Indici filtrati per soft-delete**: indici di unicità e di lookup escludono `IsDeleted = 1` con `WHERE [IsDeleted] = 0`.
6. **FK non auto-indicizzati**: SQL Server **non** crea indici automatici sulle FK (differenza da MySQL). Vanno creati esplicitamente quando la FK è navigata in query o usata in delete/update cascade.
7. **Rowstore-first**: in MVP solo indici b-tree rowstore. Columnstore valutato per `AuditLog` e `InventoryMovement` post-MVP (analytics).

---

## 2. Catalogo indici per tabella

Notazione: `IX_<Table>(col1, col2) [INCLUDE (...)] [WHERE ...]`. Tutti gli indici non-clustered.

### Core Platform

| Tabella | Indice | Motivazione |
|---|---|---|
| `Company` | `UQ_Company(Code)` filtered `WHERE IsDeleted=0` | Lookup azienda per codice; unicità globale. |
| `User` | `UQ_User(Email)` filtered `WHERE IsDeleted=0` | Login lookup. Globale (non per tenant: utente può essere su più Company via `UserCompany`). |
| `User` | `IX_User(Username)` filtered `WHERE IsDeleted=0` | Login alternativo. |
| `UserCompany` | `UQ_UserCompany(UserId, CompanyId)` | Una sola riga per coppia user-company. |
| `UserCompany` | `IX_UserCompany(CompanyId, UserId)` | Lookup utenti per azienda. |
| `Role` | `UQ_Role(CompanyId, Code)` filtered `WHERE IsDeleted=0` | Unicità code per tenant; `NULL` su CompanyId per ruoli di sistema. |
| `Permission` | `UQ_Permission(Code)` | Permission globali, codice unico. |
| `UserRole` | `UQ_UserRole(UserId, RoleId, CompanyId)` | Una sola assegnazione per terna. |
| `UserRole` | `IX_UserRole(CompanyId, UserId)` | Risoluzione ruoli al login. |
| `RolePermission` | `UQ_RolePermission(RoleId, PermissionId)` | Unicità coppia. |
| `RolePermission` | `IX_RolePermission(PermissionId)` | Reverse lookup (chi ha permesso X). |
| `RefreshToken` | `UQ_RefreshToken(TokenHash)` | Validazione token. |
| `RefreshToken` | `IX_RefreshToken(UserId, ExpiresAt)` | Revoca per utente, cleanup scaduti. |
| `AuditLog` | `IX_AuditLog(CompanyId, Timestamp DESC)` | Pagina audit log per tenant. |
| `AuditLog` | `IX_AuditLog(CompanyId, EntityType, EntityId, Timestamp DESC)` | Storia di un singolo record. |
| `AuditLog` | `IX_AuditLog(CompanyId, UserId, Timestamp DESC)` | Audit per utente. |

### Anagrafiche

| Tabella | Indice | Motivazione |
|---|---|---|
| `Country` | `UQ_Country(IsoCode2)` | Lookup ISO 3166-1 alpha-2. |
| `Currency` | `UQ_Currency(IsoCode)` | Lookup ISO 4217. |
| `Province` | `UQ_Province(CountryId, Code)` | Codice provincia per nazione. |
| `Customer` | `UQ_Customer(CompanyId, Code)` filtered `WHERE IsDeleted=0` | Codice cliente univoco per tenant. |
| `Customer` | `IX_Customer(CompanyId, VatNumber)` filtered `WHERE IsDeleted=0 AND VatNumber IS NOT NULL` | Lookup per P.IVA + duplicate warning. |
| `Customer` | `IX_Customer(CompanyId, FiscalCode)` filtered `WHERE IsDeleted=0 AND FiscalCode IS NOT NULL` | Lookup per CF. |
| `Customer` | `IX_Customer(CompanyId, Status, BusinessName) INCLUDE (Code, VatNumber, AgentUserId)` | Lista clienti con ricerca + status filter. |
| `Customer` | `IX_Customer(CompanyId, AgentUserId, Status)` filtered `WHERE IsDeleted=0` | Report clienti per agente. |
| `CustomerAddress` | `IX_CustomerAddress(CustomerId, AddressType)` | Indirizzi per cliente. |
| `CustomerContact` | `IX_CustomerContact(CustomerId)` | Contatti per cliente. |
| `CustomerCategory` | `UQ_CustomerCategory(CompanyId, Code)` filtered `WHERE IsDeleted=0` | Codice categoria univoco per tenant. |
| `Supplier` | `UQ_Supplier(CompanyId, Code)` filtered `WHERE IsDeleted=0` | Codice fornitore univoco per tenant. |
| `Supplier` | `IX_Supplier(CompanyId, VatNumber)` filtered `WHERE IsDeleted=0 AND VatNumber IS NOT NULL` | Lookup P.IVA. |
| `Supplier` | `IX_Supplier(CompanyId, Status, BusinessName) INCLUDE (Code, VatNumber)` | Lista fornitori. |
| `SupplierBankAccount` | `IX_SupplierBankAccount(SupplierId, IsDefault)` | Default IBAN. |
| `Item` | `UQ_Item(CompanyId, Code)` filtered `WHERE IsDeleted=0` | Codice articolo univoco per tenant. |
| `Item` | `IX_Item(CompanyId, CategoryId, Status) INCLUDE (Code, Name)` | Lista articoli filtrata per categoria. |
| `Item` | `IX_Item(CompanyId, Name)` | Ricerca testuale base. |
| `ItemAlternativeCode` | `UQ_ItemAlternativeCode(CompanyId, CodeType, Code)` filtered `WHERE IsDeleted=0` | Lookup per barcode/EAN/codice fornitore. |
| `ItemCategory` | `UQ_ItemCategory(CompanyId, Code)` filtered `WHERE IsDeleted=0` | — |
| `ItemCategory` | `IX_ItemCategory(CompanyId, ParentCategoryId)` | Navigazione gerarchica. |
| `PriceList` | `UQ_PriceList(CompanyId, Code)` filtered `WHERE IsDeleted=0` | — |
| `PriceListItem` | `UQ_PriceListItem(PriceListId, ItemId, ValidFrom)` | Versioning per item su listino. |
| `PriceListItem` | `IX_PriceListItem(ItemId, ValidFrom DESC)` | Lookup prezzo attivo per articolo. |
| `PaymentTerm` | `UQ_PaymentTerm(CompanyId, Code)` filtered `WHERE IsDeleted=0` (CompanyId nullable per sistema) | — |

### Transactions (MVP minimale)

| Tabella | Indice | Motivazione |
|---|---|---|
| `SalesOrder` | `UQ_SalesOrder(CompanyId, Number)` filtered `WHERE IsDeleted=0` | Numerazione univoca per tenant. |
| `SalesOrder` | `IX_SalesOrder(CompanyId, Status, OrderDate DESC) INCLUDE (CustomerId, TotalAmount)` | Lista ordini con filtro stato + ordinamento data. |
| `SalesOrder` | `IX_SalesOrder(CompanyId, CustomerId, OrderDate DESC)` | Storico ordini per cliente. |
| `SalesOrderLine` | `IX_SalesOrderLine(SalesOrderId, LineNumber)` | Righe per testata, ordinate. |
| `SalesOrderLine` | `IX_SalesOrderLine(ItemId) INCLUDE (SalesOrderId, OrderedQuantity)` | Lookup ordini per articolo (impegni). |
| `PurchaseOrder` | `UQ_PurchaseOrder(CompanyId, Number)` filtered `WHERE IsDeleted=0` | Numerazione univoca. |
| `PurchaseOrder` | `IX_PurchaseOrder(CompanyId, Status, OrderDate DESC) INCLUDE (SupplierId, TotalAmount)` | Lista ordini acquisto. |
| `PurchaseOrder` | `IX_PurchaseOrder(CompanyId, SupplierId, OrderDate DESC)` | Storico per fornitore. |
| `PurchaseOrderLine` | `IX_PurchaseOrderLine(PurchaseOrderId, LineNumber)` | Righe per testata. |
| `PurchaseOrderLine` | `IX_PurchaseOrderLine(ItemId) INCLUDE (PurchaseOrderId, OrderedQuantity)` | Acquisti per articolo. |

### Inventory (MVP minimale)

| Tabella | Indice | Motivazione |
|---|---|---|
| `Warehouse` | `UQ_Warehouse(CompanyId, Code)` filtered `WHERE IsDeleted=0` | — |
| `InventoryMovement` | `IX_InventoryMovement(CompanyId, WarehouseId, ItemId, MovementDate DESC)` | Storico movimenti per articolo/deposito. |
| `InventoryMovement` | `IX_InventoryMovement(CompanyId, MovementDate DESC) INCLUDE (ItemId, MovementType, Quantity)` | Pagina movimenti per data. |
| `InventoryMovement` | `IX_InventoryMovement(ItemId, MovementDate DESC)` | Cross-warehouse lookup per articolo. |
| `InventoryStock` | `UQ_InventoryStock(CompanyId, WarehouseId, ItemId)` | Una sola riga per coppia warehouse-item: aggregato corrente. |
| `InventoryStock` | `IX_InventoryStock(ItemId) INCLUDE (WarehouseId, QuantityOnHand, QuantityReserved)` | Disponibilità multi-deposito per articolo. |

---

## 3. Performance considerations

### 3.1 Covering vs key lookup
Le pagine "lista" (clienti, ordini, articoli) sono accessi a 1–4 colonne di filtro + 5–10 colonne mostrate. Senza `INCLUDE` → key lookup per ogni riga → tempi di query 10–50× peggiori su dataset MVP target (10K clienti, 50K articoli). Tutti gli indici `IX_*_Cov`-style sono dichiarati esplicitamente nella tabella sopra.

### 3.2 Fill factor
- Tabelle write-heavy con scritture sparse (`Customer`, `Item`, `SalesOrder`): `FILLFACTOR = 90`.
- Tabelle append-only (`AuditLog`, `InventoryMovement`): `FILLFACTOR = 100` (insert sequenziale, no split).
- Default SQL Server `FILLFACTOR = 0` (= 100) accettabile in MVP; tuning post go-live con dati reali.

### 3.3 Statistics
- `AUTO_CREATE_STATISTICS = ON`, `AUTO_UPDATE_STATISTICS = ON` (default).
- `AUTO_UPDATE_STATISTICS_ASYNC = ON` per evitare blocking su query critiche.
- Manutenzione: rebuild statistics settimanale via maintenance plan (`UPDATE STATISTICS ... WITH FULLSCAN` su tabelle <1M, sample 30% su tabelle maggiori).

### 3.4 Fragmentation
- Maintenance plan settimanale:
  - `> 30%` frammentazione → `ALTER INDEX ... REBUILD WITH (ONLINE = ON)`.
  - `5%–30%` → `REORGANIZE`.
  - `< 5%` → no action.

### 3.5 Trade-off write performance
Ogni indice è un costo su INSERT/UPDATE/DELETE. Il catalogo sopra è il minimo per coprire i pattern d'uso identificati. Nuovi indici si aggiungono solo dopo evidenza (Query Store, DMV `sys.dm_db_missing_index_*`) e validazione che non duplichino indici esistenti.

---

## 4. Strumenti di monitoraggio

- **Query Store**: abilitato `ON` su ogni DB applicativo, retention 30gg, capture mode `AUTO`. Report settimanale top-25 query per durata e per CPU.
- **Missing Indexes DMV**: `sys.dm_db_missing_index_details` rivisto mensile; ogni proposta di nuovo indice passa da review Architect (no auto-apply).
- **Unused Indexes**: `sys.dm_db_index_usage_stats` ogni trimestre; indici senza seek/scan da 90gg vengono valutati per rimozione.
- **Wait stats**: `sys.dm_os_wait_stats` monitorato; soglie di alert su `PAGEIOLATCH_*` e `LCK_M_*`.

---

## 5. Partitioning (roadmap post-MVP)

Non incluso in MVP. Pianificato per:
- `AuditLog`: partition function per anno (`PF_AuditLog_Year`) su `Timestamp`. Sliding window con sezione "ARCHIVE" per dati >5 anni.
- `InventoryMovement`: partition per anno su `MovementDate` quando volumi superano 10M righe.

Trigger di valutazione: tabella > 50M righe **o** query di range > 2s p95.

---

## 6. Full-Text Search

Non incluso in MVP. Da valutare per `Customer.BusinessName`, `Item.Name`, `Item.Description` quando ricerca like-base (`IX_Customer(CompanyId, Name)`) mostra degrado >500ms su dataset prod.

---

## 7. Indici **non** creati intenzionalmente

- `IX_*(CreatedAt)` standalone: nessuna query MVP filtra per data creazione tecnica senza altri filtri; report usano data dominio (`OrderDate`, `MovementDate`, ...).
- `IX_*(UpdatedBy)`: nessun caso d'uso applicativo; audit usa `AuditLog`.
- `IX_*(RowVersion)`: non indicizzabile sensatamente; usato solo come token concorrenza.
