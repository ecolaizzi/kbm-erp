# ERD — Transactions (MVP minimale)

**Modulo**: Ordini di vendita e acquisto (struttura minima per validare schema)
**MVP Fase**: 4 (Vendite) / 3 (Acquisti) — **schema progettato in MVP, implementato in fasi successive**
**Owner**: Database Architect Agent

> **Nota**: il presente ERD è incluso per validare che il modello multi-tenant + audit + concorrenza regga sui workflow transazionali (cfr. workflow `ciclo-attivo.md`, `ciclo-passivo.md`). L'implementazione applicativa è fuori scope MVP. Estensioni (DDT, Fattura, NotaCredito) saranno aggiunte in fasi 3-4.

```mermaid
erDiagram
    Customer ||--o{ SalesOrder : "places"
    SalesOrder ||--o{ SalesOrderLine : "contains"
    Item ||--o{ SalesOrderLine : "ordered as"
    PriceList }o--o| SalesOrder : "applied"
    PaymentTerm }o--o| SalesOrder : "terms"
    User }o--o| SalesOrder : "agent"
    Warehouse }o--o| SalesOrder : "fulfilled from"

    Supplier ||--o{ PurchaseOrder : "receives"
    PurchaseOrder ||--o{ PurchaseOrderLine : "contains"
    Item ||--o{ PurchaseOrderLine : "ordered as"
    PaymentTerm }o--o| PurchaseOrder : "terms"
    Warehouse }o--o| PurchaseOrder : "delivered to"

    SalesOrder {
        BIGINT Id PK
        BIGINT CompanyId FK
        NVARCHAR Number "UQ per CompanyId"
        DATE OrderDate
        BIGINT CustomerId FK
        NVARCHAR CustomerOrderReference
        BIGINT BillingAddressId FK
        BIGINT ShippingAddressId FK
        BIGINT PriceListId FK
        BIGINT PaymentTermId FK
        BIGINT AgentUserId FK
        BIGINT WarehouseId FK
        BIGINT CurrencyId FK
        DECIMAL ExchangeRate
        DECIMAL HeaderDiscountPercent
        DECIMAL NetAmount
        DECIMAL VatAmount
        DECIMAL TotalAmount
        NVARCHAR Status "Draft|Confirmed|PartiallyShipped|Shipped|Invoiced|Cancelled"
        DATE ExpectedDeliveryDate
        NVARCHAR Notes
        DATETIME2 CreatedAt
        BIGINT CreatedBy FK
        DATETIME2 UpdatedAt
        BIGINT UpdatedBy FK
        BIT IsDeleted
        ROWVERSION RowVersion
    }

    SalesOrderLine {
        BIGINT Id PK
        BIGINT CompanyId FK
        BIGINT SalesOrderId FK
        INT LineNumber
        BIGINT ItemId FK
        NVARCHAR Description "snapshot at order time"
        DECIMAL OrderedQuantity
        DECIMAL DeliveredQuantity
        DECIMAL InvoicedQuantity
        BIGINT UnitOfMeasureId FK
        DECIMAL UnitPrice
        DECIMAL LineDiscountPercent
        DECIMAL VatPercent
        DECIMAL NetAmount
        DECIMAL VatAmount
        DECIMAL TotalAmount
        NVARCHAR LineStatus "Open|PartiallyShipped|Closed|Cancelled"
        DATETIME2 CreatedAt
        BIGINT CreatedBy FK
        DATETIME2 UpdatedAt
        BIGINT UpdatedBy FK
        ROWVERSION RowVersion
    }

    PurchaseOrder {
        BIGINT Id PK
        BIGINT CompanyId FK
        NVARCHAR Number "UQ per CompanyId"
        DATE OrderDate
        BIGINT SupplierId FK
        NVARCHAR SupplierOrderReference
        BIGINT DeliveryAddressId FK
        BIGINT PaymentTermId FK
        BIGINT WarehouseId FK
        BIGINT CurrencyId FK
        DECIMAL ExchangeRate
        DECIMAL HeaderDiscountPercent
        DECIMAL NetAmount
        DECIMAL VatAmount
        DECIMAL TotalAmount
        NVARCHAR Status "Draft|Sent|PartiallyReceived|Received|Invoiced|Cancelled"
        DATE ExpectedDeliveryDate
        NVARCHAR Notes
        DATETIME2 CreatedAt
        BIGINT CreatedBy FK
        DATETIME2 UpdatedAt
        BIGINT UpdatedBy FK
        BIT IsDeleted
        ROWVERSION RowVersion
    }

    PurchaseOrderLine {
        BIGINT Id PK
        BIGINT CompanyId FK
        BIGINT PurchaseOrderId FK
        INT LineNumber
        BIGINT ItemId FK
        NVARCHAR Description
        DECIMAL OrderedQuantity
        DECIMAL ReceivedQuantity
        DECIMAL InvoicedQuantity
        BIGINT UnitOfMeasureId FK
        DECIMAL UnitPrice
        DECIMAL LineDiscountPercent
        DECIMAL VatPercent
        DECIMAL NetAmount
        DECIMAL VatAmount
        DECIMAL TotalAmount
        NVARCHAR LineStatus "Open|PartiallyReceived|Closed|Cancelled"
        DATETIME2 CreatedAt
        BIGINT CreatedBy FK
        DATETIME2 UpdatedAt
        BIGINT UpdatedBy FK
        ROWVERSION RowVersion
    }
```

## Note di design

- **`Number` business vs `Id` tecnico**: `Number` è la numerazione visibile all'utente (es. `2026/000123`), univoca per CompanyId; `Id` è la PK tecnica. Numerazione gestita da un servizio `INumberingService` con sequenze per anno/tipo documento.
- **Snapshot prezzi/descrizione su riga**: `UnitPrice`, `VatPercent`, `Description` sono copiati dall'`Item` al momento della creazione riga — gli aggiornamenti master non devono retroattivamente cambiare gli ordini emessi.
- **`OrderedQuantity` vs `DeliveredQuantity` vs `InvoicedQuantity`**: i tre contatori abilitano stati intermedi (`PartiallyShipped`, `PartiallyReceived`). Gestiti applicativamente quando si introducono DDT e Fattura.
- **`HeaderDiscountPercent`**: sconto a livello testata aggiuntivo (oltre agli sconti riga); applicato in calcolo netto totale documento. Logica di calcolo nel domain layer, non in DB.
- **`CompanyId` ridondato su `*Line`**: i record figli portano `CompanyId` per consentire query filtrate per tenant senza join, e per supportare row-level security future. Constraint check: `*Line.CompanyId = parent.CompanyId` (enforced applicativamente; trigger evitato in MVP).
- **`Status` come stringa**: rappresentato come `NVARCHAR(32)` con `CHECK` constraint dei valori validi. Alternativa enum int rifiutata per leggibilità in tooling SQL e in audit log JSON.
