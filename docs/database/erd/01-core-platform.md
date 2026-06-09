# ERD — Core Platform

**Modulo**: Autenticazione, autorizzazione, multi-tenant, audit
**MVP Fase**: 1
**Owner**: Database Architect Agent

```mermaid
erDiagram
    Company ||--o{ UserCompany : "has members"
    Company ||--o{ Role : "owns custom roles"
    Company ||--o{ AuditLog : "scoped to"
    Company ||--o{ CompanySetting : "configures"

    User ||--o{ UserCompany : "belongs to"
    User ||--o{ UserRole : "is assigned"
    User ||--o{ RefreshToken : "issues"
    User ||--o{ AuditLog : "performs"

    UserCompany }o--|| Company : "in company"

    Role ||--o{ UserRole : "assigned via"
    Role ||--o{ RolePermission : "grants"
    Role }o--o| Company : "scoped to (NULL = system role)"

    Permission ||--o{ RolePermission : "granted by"

    UserRole }o--|| Company : "scoped to"

    Company {
        BIGINT Id PK
        NVARCHAR Code "UQ"
        NVARCHAR BusinessName
        NVARCHAR VatNumber
        NVARCHAR FiscalCode
        NVARCHAR Sdi
        NVARCHAR Pec
        BIGINT CountryId FK
        NVARCHAR Status "Active|Suspended|Closed"
        DATETIME2 CreatedAt
        BIGINT CreatedBy FK
        DATETIME2 UpdatedAt
        BIGINT UpdatedBy FK
        BIT IsDeleted
        ROWVERSION RowVersion
    }

    User {
        BIGINT Id PK
        NVARCHAR Username "UQ"
        NVARCHAR Email "UQ"
        NVARCHAR PasswordHash
        NVARCHAR FirstName
        NVARCHAR LastName
        NVARCHAR Status "Active|Locked|Disabled"
        BIT MfaEnabled
        NVARCHAR MfaSecret
        DATETIME2 LastLoginAt
        INT FailedLoginAttempts
        DATETIME2 PasswordChangedAt
        DATETIME2 CreatedAt
        BIGINT CreatedBy FK
        DATETIME2 UpdatedAt
        BIGINT UpdatedBy FK
        BIT IsDeleted
        ROWVERSION RowVersion
    }

    UserCompany {
        BIGINT Id PK
        BIGINT UserId FK
        BIGINT CompanyId FK
        BIT IsDefault
        DATETIME2 CreatedAt
        BIGINT CreatedBy FK
    }

    Role {
        BIGINT Id PK
        BIGINT CompanyId FK "NULL = system role"
        NVARCHAR Code
        NVARCHAR Name
        NVARCHAR Description
        BIT IsSystem
        DATETIME2 CreatedAt
        BIGINT CreatedBy FK
        DATETIME2 UpdatedAt
        BIGINT UpdatedBy FK
        BIT IsDeleted
        ROWVERSION RowVersion
    }

    Permission {
        BIGINT Id PK
        NVARCHAR Code "UQ - module.entity.action"
        NVARCHAR Module
        NVARCHAR Description
    }

    UserRole {
        BIGINT Id PK
        BIGINT UserId FK
        BIGINT RoleId FK
        BIGINT CompanyId FK
        DATETIME2 CreatedAt
        BIGINT CreatedBy FK
    }

    RolePermission {
        BIGINT Id PK
        BIGINT RoleId FK
        BIGINT PermissionId FK
        DATETIME2 CreatedAt
        BIGINT CreatedBy FK
    }

    RefreshToken {
        BIGINT Id PK
        BIGINT UserId FK
        BIGINT CompanyId FK "active company context"
        NVARCHAR TokenHash "UQ"
        DATETIME2 IssuedAt
        DATETIME2 ExpiresAt
        DATETIME2 RevokedAt
        NVARCHAR IpAddress
        NVARCHAR UserAgent
    }

    AuditLog {
        BIGINT Id PK
        DATETIME2 Timestamp
        BIGINT UserId FK
        BIGINT CompanyId FK
        NVARCHAR Action "Create|Update|Delete|Login|..."
        NVARCHAR EntityType
        BIGINT EntityId
        NVARCHAR OldValue "JSON"
        NVARCHAR NewValue "JSON"
        NVARCHAR IpAddress
        NVARCHAR UserAgent
        NVARCHAR CorrelationId
    }

    CompanySetting {
        BIGINT Id PK
        BIGINT CompanyId FK
        NVARCHAR Key
        NVARCHAR Value
        NVARCHAR ValueType "String|Int|Bool|Json"
        DATETIME2 UpdatedAt
        BIGINT UpdatedBy FK
        ROWVERSION RowVersion
    }
```

## Note di design

- **Multi-tenant model**: la relazione `User ↔ Company` è many-to-many tramite `UserCompany`. Un utente può accedere a più aziende (es. commercialista, gruppo aziendale). Il contesto attivo (Company corrente) è scelto al login e propagato in `RefreshToken.CompanyId`.
- **Ruoli di sistema vs custom**: `Role.CompanyId` è nullable: NULL = ruolo di sistema (Admin, Manager, User, ReadOnly) visibile a tutte le aziende; valorizzato = ruolo custom locale al tenant.
- **Permission**: catalogo globale immutabile, definito via seed migration. Naming `module.entity.action` (es. `sales.order.create`, `anagraphics.customer.read`).
- **AuditLog**: append-only, niente `IsDeleted`/`RowVersion`/`UpdatedAt`. Vedi `schema-design.md` §3.8 per dettagli immutabilità.
- **PasswordHash**: BCrypt (cost ≥ 12) o Argon2id; il tipo SQL è `NVARCHAR(255)`. Lo schema non vincola l'algoritmo.
- **MfaSecret**: cifrato applicativo (AES-GCM con master key da Key Vault); SQL salva il cipher text in `NVARCHAR(255)`.
