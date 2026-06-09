# ERD — Anagrafiche

**Modulo**: Clienti, Fornitori, Articoli, Listini, Categorie, lookup geografici
**MVP Fase**: 2
**Owner**: Database Architect Agent

```mermaid
erDiagram
    Country ||--o{ Province : "contains"
    Country ||--o{ Customer : "country of"
    Country ||--o{ Supplier : "country of"
    Country ||--o{ Address : "located in"

    Currency ||--o{ Customer : "default currency"
    Currency ||--o{ Supplier : "default currency"
    Currency ||--o{ PriceList : "expressed in"

    Customer ||--o{ CustomerAddress : "has"
    Customer ||--o{ CustomerContact : "has"
    Customer }o--o| CustomerCategory : "classified as"
    Customer }o--o| PriceList : "default price list"
    Customer }o--o| PaymentTerm : "default payment term"
    Customer }o--o| User : "assigned agent"
    CustomerAddress }o--|| Address : "address data"

    Supplier ||--o{ SupplierAddress : "has"
    Supplier ||--o{ SupplierContact : "has"
    Supplier ||--o{ SupplierBankAccount : "has"
    Supplier }o--o| PaymentTerm : "default payment term"
    SupplierAddress }o--|| Address : "address data"

    Item }o--o| ItemCategory : "categorized as"
    Item }o--o| UnitOfMeasure : "primary UoM"
    Item ||--o{ ItemAlternativeCode : "has alt codes"
    Item ||--o{ PriceListItem : "priced in lists"
    ItemCategory }o--o| ItemCategory : "parent category"

    PriceList ||--o{ PriceListItem : "contains"
    PriceList }o--o| CustomerCategory : "restricted to"

    Customer {
        BIGINT Id PK
        BIGINT CompanyId FK
        NVARCHAR Code "UQ per CompanyId"
        NVARCHAR Type "Company|Individual|Prospect"
        NVARCHAR BusinessName
        NVARCHAR FirstName "nullable - Individual"
        NVARCHAR LastName "nullable - Individual"
        NVARCHAR VatNumber
        NVARCHAR FiscalCode
        NVARCHAR Sdi
        NVARCHAR Pec
        BIGINT CountryId FK
        BIGINT CurrencyId FK
        BIGINT CustomerCategoryId FK
        BIGINT DefaultPriceListId FK
        BIGINT DefaultPaymentTermId FK
        BIGINT AgentUserId FK
        DECIMAL CreditLimit
        DECIMAL DefaultDiscountPercent
        NVARCHAR Status "Active|Inactive|Prospect|Blocked"
        NVARCHAR Notes
        DATETIME2 CreatedAt
        BIGINT CreatedBy FK
        DATETIME2 UpdatedAt
        BIGINT UpdatedBy FK
        BIT IsDeleted
        ROWVERSION RowVersion
    }

    CustomerAddress {
        BIGINT Id PK
        BIGINT CustomerId FK
        NVARCHAR AddressType "Legal|Shipping|Billing|Other"
        BIT IsDefault
        BIGINT AddressId FK
    }

    CustomerContact {
        BIGINT Id PK
        BIGINT CustomerId FK
        NVARCHAR ContactType "Phone|Email|Mobile|Fax|Other"
        NVARCHAR ContactName
        NVARCHAR Value
        NVARCHAR Role
        BIT IsDefault
    }

    Supplier {
        BIGINT Id PK
        BIGINT CompanyId FK
        NVARCHAR Code "UQ per CompanyId"
        NVARCHAR Type "Company|Individual"
        NVARCHAR BusinessName
        NVARCHAR VatNumber
        NVARCHAR FiscalCode
        NVARCHAR Sdi
        NVARCHAR Pec
        NVARCHAR VatRegime "Ordinary|Forfettario|MinimiPa|Other"
        BIGINT CountryId FK
        BIGINT CurrencyId FK
        BIGINT DefaultPaymentTermId FK
        NVARCHAR Status "Active|Inactive|Blocked"
        DATETIME2 CreatedAt
        BIGINT CreatedBy FK
        DATETIME2 UpdatedAt
        BIGINT UpdatedBy FK
        BIT IsDeleted
        ROWVERSION RowVersion
    }

    SupplierBankAccount {
        BIGINT Id PK
        BIGINT SupplierId FK
        NVARCHAR Iban
        NVARCHAR Bic
        NVARCHAR BankName
        BIT IsDefault
    }

    Item {
        BIGINT Id PK
        BIGINT CompanyId FK
        NVARCHAR Code "UQ per CompanyId"
        NVARCHAR Name
        NVARCHAR Description
        BIGINT ItemCategoryId FK
        BIGINT PrimaryUnitOfMeasureId FK
        NVARCHAR ItemType "Goods|Service"
        DECIMAL StandardCost
        DECIMAL ListPrice
        DECIMAL VatPercent
        NVARCHAR Status "Active|Inactive|Discontinued"
        BIT IsStockManaged
        DATETIME2 CreatedAt
        BIGINT CreatedBy FK
        DATETIME2 UpdatedAt
        BIGINT UpdatedBy FK
        BIT IsDeleted
        ROWVERSION RowVersion
    }

    ItemAlternativeCode {
        BIGINT Id PK
        BIGINT CompanyId FK
        BIGINT ItemId FK
        NVARCHAR CodeType "Barcode|Ean|SupplierCode|InternalAlt"
        NVARCHAR Code
        BIGINT SupplierId FK "nullable - per SupplierCode"
        BIT IsDeleted
    }

    PriceList {
        BIGINT Id PK
        BIGINT CompanyId FK
        NVARCHAR Code "UQ per CompanyId"
        NVARCHAR Name
        NVARCHAR PriceListType "Sales|Purchase"
        BIGINT CurrencyId FK
        BIGINT CustomerCategoryId FK
        DATE ValidFrom
        DATE ValidTo
        NVARCHAR Status "Draft|Active|Archived"
        DATETIME2 CreatedAt
        BIGINT CreatedBy FK
        DATETIME2 UpdatedAt
        BIGINT UpdatedBy FK
        BIT IsDeleted
        ROWVERSION RowVersion
    }

    PriceListItem {
        BIGINT Id PK
        BIGINT PriceListId FK
        BIGINT ItemId FK
        DECIMAL UnitPrice
        DECIMAL DiscountPercent
        INT MinQuantity
        DATE ValidFrom
        DATE ValidTo
        ROWVERSION RowVersion
    }
```

## Note di design

- **Address normalizzato**: tabella `Address` (vedi `schema-design.md`) condivisa fra `CustomerAddress` e `SupplierAddress` per evitare duplicazione e centralizzare validazioni geografiche (Country, Province).
- **Customer/Supplier indipendenti**: niente eredità o tabella `BusinessPartner` unica — i due workflow divergono presto (regime IVA, dati bancari, agenti, ecc.).
- **`PriceListItem.ValidFrom`** consente versioning del prezzo nel tempo (es. ribasso stagionale). Il prezzo "attivo" è la riga con `ValidFrom ≤ today < ValidTo` o `ValidTo IS NULL`.
- **Prezzi speciali per cliente (US-136)**: rappresentati come listino dedicato `PriceList(PriceListType='Sales')` legato al cliente via `Customer.DefaultPriceListId` oppure tramite tabella `CustomerSpecialPrice` (vedi schema design §4.6).
- **Lookup geografici (`Country`, `Province`, `Currency`)**: globali, niente `CompanyId`, niente `IsDeleted` (gestiti via seed migration, modifiche solo lato sviluppo).
