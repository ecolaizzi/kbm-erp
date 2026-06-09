/* =================================================================
   KBM - SCHEMA CREATION SCRIPT (DRAFT)
   Versione: 1.0
   Data: 2026-06-09
   Target: SQL Server 2019+ (preferred 2022)

   NOTA: questo script è una rappresentazione SQL fedele dello schema
   progettato. Lo schema effettivo è prodotto da EF Core Code First
   Migrations (vedi migration-strategy.md). Questo file serve come
   reference per DBA review, documentation, smoke test out-of-band.

   Sequenza: lookup globali → core platform → anagraphics → inventory
   → transactions → indexes → seed minimo.
   ================================================================= */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* -----------------------------------------------------------------
   1. LOOKUP GLOBALI
   ----------------------------------------------------------------- */

CREATE TABLE dbo.Country (
    Id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Country PRIMARY KEY CLUSTERED,
    IsoCode2        NVARCHAR(2)   NOT NULL,
    IsoCode3        NVARCHAR(3)   NOT NULL,
    Name            NVARCHAR(100) NOT NULL,
    IsEuMember      BIT           NOT NULL CONSTRAINT DF_Country_IsEuMember DEFAULT(0),
    PhonePrefix     NVARCHAR(8)   NULL,
    CONSTRAINT UQ_Country_IsoCode2 UNIQUE (IsoCode2),
    CONSTRAINT UQ_Country_IsoCode3 UNIQUE (IsoCode3)
);
GO

CREATE TABLE dbo.Currency (
    Id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Currency PRIMARY KEY CLUSTERED,
    IsoCode         NVARCHAR(3)   NOT NULL,
    Name            NVARCHAR(64)  NOT NULL,
    Symbol          NVARCHAR(8)   NULL,
    DecimalDigits   TINYINT       NOT NULL CONSTRAINT DF_Currency_Decimals DEFAULT(2),
    CONSTRAINT UQ_Currency_IsoCode UNIQUE (IsoCode)
);
GO

CREATE TABLE dbo.Province (
    Id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Province PRIMARY KEY CLUSTERED,
    CountryId       BIGINT        NOT NULL CONSTRAINT FK_Province_Country REFERENCES dbo.Country(Id),
    Code            NVARCHAR(8)   NOT NULL,
    Name            NVARCHAR(100) NOT NULL,
    CONSTRAINT UQ_Province_Country_Code UNIQUE (CountryId, Code)
);
GO

CREATE TABLE dbo.UnitOfMeasure (
    Id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_UnitOfMeasure PRIMARY KEY CLUSTERED,
    Code            NVARCHAR(8)   NOT NULL,
    Name            NVARCHAR(64)  NOT NULL,
    UnitType        NVARCHAR(16)  NOT NULL,
    CONSTRAINT UQ_UnitOfMeasure_Code UNIQUE (Code),
    CONSTRAINT CK_UnitOfMeasure_UnitType CHECK (UnitType IN
        (N'Quantity', N'Weight', N'Volume', N'Length', N'Time'))
);
GO

CREATE TABLE dbo.UnitOfMeasureConversion (
    Id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_UnitOfMeasureConversion PRIMARY KEY CLUSTERED,
    FromUnitId      BIGINT NOT NULL CONSTRAINT FK_UomConv_From REFERENCES dbo.UnitOfMeasure(Id),
    ToUnitId        BIGINT NOT NULL CONSTRAINT FK_UomConv_To   REFERENCES dbo.UnitOfMeasure(Id),
    Factor          DECIMAL(18,8) NOT NULL,
    CONSTRAINT UQ_UomConv_From_To UNIQUE (FromUnitId, ToUnitId),
    CONSTRAINT CK_UomConv_Factor CHECK (Factor > 0)
);
GO

/* -----------------------------------------------------------------
   2. CORE PLATFORM
   ----------------------------------------------------------------- */

CREATE TABLE dbo.Company (
    Id                  BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Company PRIMARY KEY CLUSTERED,
    Code                NVARCHAR(32)  NOT NULL,
    BusinessName        NVARCHAR(200) NOT NULL,
    LegalName           NVARCHAR(200) NULL,
    VatNumber           NVARCHAR(20)  NULL,
    FiscalCode          NVARCHAR(16)  NULL,
    Sdi                 NVARCHAR(7)   NULL,
    Pec                 NVARCHAR(254) NULL,
    CountryId           BIGINT        NOT NULL CONSTRAINT FK_Company_Country REFERENCES dbo.Country(Id),
    DefaultCurrencyId   BIGINT        NOT NULL CONSTRAINT FK_Company_Currency REFERENCES dbo.Currency(Id),
    [Status]            NVARCHAR(16)  NOT NULL CONSTRAINT DF_Company_Status DEFAULT(N'Active'),
    CreatedAt           DATETIME2(3)  NOT NULL CONSTRAINT DF_Company_CreatedAt DEFAULT(SYSUTCDATETIME()),
    CreatedBy           BIGINT        NOT NULL,
    UpdatedAt           DATETIME2(3)  NULL,
    UpdatedBy           BIGINT        NULL,
    IsDeleted           BIT           NOT NULL CONSTRAINT DF_Company_IsDeleted DEFAULT(0),
    RowVersion          ROWVERSION    NOT NULL,
    CONSTRAINT CK_Company_Status CHECK ([Status] IN (N'Active', N'Suspended', N'Closed'))
);
GO

CREATE UNIQUE NONCLUSTERED INDEX UQ_Company_Code
    ON dbo.Company(Code) WHERE IsDeleted = 0;
GO

CREATE TABLE dbo.[User] (
    Id                    BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_User PRIMARY KEY CLUSTERED,
    Username              NVARCHAR(64)  NOT NULL,
    Email                 NVARCHAR(254) NOT NULL,
    PasswordHash          NVARCHAR(255) NOT NULL,
    FirstName             NVARCHAR(100) NOT NULL,
    LastName              NVARCHAR(100) NOT NULL,
    [Status]              NVARCHAR(16)  NOT NULL CONSTRAINT DF_User_Status DEFAULT(N'PendingActivation'),
    MfaEnabled            BIT           NOT NULL CONSTRAINT DF_User_MfaEnabled DEFAULT(0),
    MfaSecret             NVARCHAR(255) NULL,
    LastLoginAt           DATETIME2(3)  NULL,
    FailedLoginAttempts   INT           NOT NULL CONSTRAINT DF_User_FailedAttempts DEFAULT(0),
    LockedUntil           DATETIME2(3)  NULL,
    PasswordChangedAt     DATETIME2(3)  NOT NULL CONSTRAINT DF_User_PwdChangedAt DEFAULT(SYSUTCDATETIME()),
    PreferredLanguage     NVARCHAR(8)   NOT NULL CONSTRAINT DF_User_Language DEFAULT(N'it-IT'),
    CreatedAt             DATETIME2(3)  NOT NULL CONSTRAINT DF_User_CreatedAt DEFAULT(SYSUTCDATETIME()),
    CreatedBy             BIGINT        NOT NULL,
    UpdatedAt             DATETIME2(3)  NULL,
    UpdatedBy             BIGINT        NULL,
    IsDeleted             BIT           NOT NULL CONSTRAINT DF_User_IsDeleted DEFAULT(0),
    RowVersion            ROWVERSION    NOT NULL,
    CONSTRAINT CK_User_Status CHECK ([Status] IN
        (N'Active', N'Locked', N'Disabled', N'PendingActivation'))
);
GO

CREATE UNIQUE NONCLUSTERED INDEX UQ_User_Username
    ON dbo.[User](Username) WHERE IsDeleted = 0;
CREATE UNIQUE NONCLUSTERED INDEX UQ_User_Email
    ON dbo.[User](Email)    WHERE IsDeleted = 0;
GO

/* FK self-reference su User per CreatedBy/UpdatedBy aggiunte
   dopo il bootstrap (vedi appendix). */

CREATE TABLE dbo.UserCompany (
    Id          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_UserCompany PRIMARY KEY CLUSTERED,
    UserId      BIGINT NOT NULL CONSTRAINT FK_UserCompany_User    REFERENCES dbo.[User](Id),
    CompanyId   BIGINT NOT NULL CONSTRAINT FK_UserCompany_Company REFERENCES dbo.Company(Id),
    IsDefault   BIT    NOT NULL CONSTRAINT DF_UserCompany_IsDefault DEFAULT(0),
    CreatedAt   DATETIME2(3) NOT NULL CONSTRAINT DF_UserCompany_CreatedAt DEFAULT(SYSUTCDATETIME()),
    CreatedBy   BIGINT NOT NULL,
    CONSTRAINT UQ_UserCompany_User_Company UNIQUE (UserId, CompanyId)
);
CREATE NONCLUSTERED INDEX IX_UserCompany_Company_User
    ON dbo.UserCompany(CompanyId, UserId);
GO

CREATE TABLE dbo.Role (
    Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Role PRIMARY KEY CLUSTERED,
    CompanyId     BIGINT        NULL CONSTRAINT FK_Role_Company REFERENCES dbo.Company(Id),
    Code          NVARCHAR(64)  NOT NULL,
    Name          NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    IsSystem      BIT           NOT NULL CONSTRAINT DF_Role_IsSystem DEFAULT(0),
    CreatedAt     DATETIME2(3)  NOT NULL CONSTRAINT DF_Role_CreatedAt DEFAULT(SYSUTCDATETIME()),
    CreatedBy     BIGINT        NOT NULL,
    UpdatedAt     DATETIME2(3)  NULL,
    UpdatedBy     BIGINT        NULL,
    IsDeleted     BIT           NOT NULL CONSTRAINT DF_Role_IsDeleted DEFAULT(0),
    RowVersion    ROWVERSION    NOT NULL,
    CONSTRAINT CK_Role_SystemHasNullCompany CHECK
        ((IsSystem = 1 AND CompanyId IS NULL) OR (IsSystem = 0 AND CompanyId IS NOT NULL))
);
CREATE UNIQUE NONCLUSTERED INDEX UQ_Role_Company_Code
    ON dbo.Role(CompanyId, Code) WHERE IsDeleted = 0;
GO

CREATE TABLE dbo.Permission (
    Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Permission PRIMARY KEY CLUSTERED,
    Code          NVARCHAR(128) NOT NULL,
    Module        NVARCHAR(64)  NOT NULL,
    [Description] NVARCHAR(255) NOT NULL,
    CONSTRAINT UQ_Permission_Code UNIQUE (Code)
);
GO

CREATE TABLE dbo.UserRole (
    Id           BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_UserRole PRIMARY KEY CLUSTERED,
    UserId       BIGINT NOT NULL CONSTRAINT FK_UserRole_User    REFERENCES dbo.[User](Id),
    RoleId       BIGINT NOT NULL CONSTRAINT FK_UserRole_Role    REFERENCES dbo.Role(Id),
    CompanyId    BIGINT NOT NULL CONSTRAINT FK_UserRole_Company REFERENCES dbo.Company(Id),
    CreatedAt    DATETIME2(3) NOT NULL CONSTRAINT DF_UserRole_CreatedAt DEFAULT(SYSUTCDATETIME()),
    CreatedBy    BIGINT NOT NULL,
    CONSTRAINT UQ_UserRole_User_Role_Company UNIQUE (UserId, RoleId, CompanyId)
);
CREATE NONCLUSTERED INDEX IX_UserRole_Company_User ON dbo.UserRole(CompanyId, UserId);
GO

CREATE TABLE dbo.RolePermission (
    Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RolePermission PRIMARY KEY CLUSTERED,
    RoleId        BIGINT NOT NULL CONSTRAINT FK_RolePermission_Role       REFERENCES dbo.Role(Id),
    PermissionId  BIGINT NOT NULL CONSTRAINT FK_RolePermission_Permission REFERENCES dbo.Permission(Id),
    CreatedAt     DATETIME2(3) NOT NULL CONSTRAINT DF_RolePermission_CreatedAt DEFAULT(SYSUTCDATETIME()),
    CreatedBy     BIGINT NOT NULL,
    CONSTRAINT UQ_RolePermission_Role_Permission UNIQUE (RoleId, PermissionId)
);
CREATE NONCLUSTERED INDEX IX_RolePermission_Permission ON dbo.RolePermission(PermissionId);
GO

CREATE TABLE dbo.RefreshToken (
    Id                BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RefreshToken PRIMARY KEY CLUSTERED,
    UserId            BIGINT NOT NULL CONSTRAINT FK_RefreshToken_User    REFERENCES dbo.[User](Id),
    CompanyId         BIGINT NOT NULL CONSTRAINT FK_RefreshToken_Company REFERENCES dbo.Company(Id),
    TokenHash         NVARCHAR(128) NOT NULL,
    IssuedAt          DATETIME2(3)  NOT NULL CONSTRAINT DF_RefreshToken_IssuedAt DEFAULT(SYSUTCDATETIME()),
    ExpiresAt         DATETIME2(3)  NOT NULL,
    RevokedAt         DATETIME2(3)  NULL,
    RevocationReason  NVARCHAR(64)  NULL,
    IpAddress         NVARCHAR(45)  NULL,
    UserAgent         NVARCHAR(500) NULL,
    CONSTRAINT UQ_RefreshToken_TokenHash UNIQUE (TokenHash)
);
CREATE NONCLUSTERED INDEX IX_RefreshToken_User_Expires
    ON dbo.RefreshToken(UserId, ExpiresAt);
GO

CREATE TABLE dbo.AuditLog (
    Id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLog PRIMARY KEY CLUSTERED,
    [Timestamp]     DATETIME2(3) NOT NULL CONSTRAINT DF_AuditLog_Timestamp DEFAULT(SYSUTCDATETIME()),
    UserId          BIGINT       NULL CONSTRAINT FK_AuditLog_User    REFERENCES dbo.[User](Id),
    CompanyId       BIGINT       NULL CONSTRAINT FK_AuditLog_Company REFERENCES dbo.Company(Id),
    [Action]        NVARCHAR(50) NOT NULL,
    EntityType      NVARCHAR(100) NULL,
    EntityId        BIGINT        NULL,
    OldValue        NVARCHAR(MAX) NULL,
    NewValue        NVARCHAR(MAX) NULL,
    IpAddress       NVARCHAR(45)  NULL,
    UserAgent       NVARCHAR(500) NULL,
    CorrelationId   NVARCHAR(64)  NULL,
    CONSTRAINT CK_AuditLog_OldValueJson CHECK (OldValue IS NULL OR ISJSON(OldValue) = 1),
    CONSTRAINT CK_AuditLog_NewValueJson CHECK (NewValue IS NULL OR ISJSON(NewValue) = 1)
);
CREATE NONCLUSTERED INDEX IX_AuditLog_Company_Timestamp
    ON dbo.AuditLog(CompanyId, [Timestamp] DESC);
CREATE NONCLUSTERED INDEX IX_AuditLog_Company_Entity_Timestamp
    ON dbo.AuditLog(CompanyId, EntityType, EntityId, [Timestamp] DESC);
CREATE NONCLUSTERED INDEX IX_AuditLog_Company_User_Timestamp
    ON dbo.AuditLog(CompanyId, UserId, [Timestamp] DESC);
GO

CREATE TABLE dbo.CompanySetting (
    Id           BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CompanySetting PRIMARY KEY CLUSTERED,
    CompanyId    BIGINT NOT NULL CONSTRAINT FK_CompanySetting_Company REFERENCES dbo.Company(Id),
    [Key]        NVARCHAR(128) NOT NULL,
    [Value]      NVARCHAR(MAX) NULL,
    ValueType    NVARCHAR(16)  NOT NULL CONSTRAINT DF_CompanySetting_ValueType DEFAULT(N'String'),
    UpdatedAt    DATETIME2(3) NOT NULL CONSTRAINT DF_CompanySetting_UpdatedAt DEFAULT(SYSUTCDATETIME()),
    UpdatedBy    BIGINT       NOT NULL,
    RowVersion   ROWVERSION   NOT NULL,
    CONSTRAINT UQ_CompanySetting_Company_Key UNIQUE (CompanyId, [Key]),
    CONSTRAINT CK_CompanySetting_ValueType CHECK (ValueType IN
        (N'String', N'Int', N'Decimal', N'Bool', N'Json')),
    CONSTRAINT CK_CompanySetting_JsonValid CHECK
        (ValueType <> N'Json' OR [Value] IS NULL OR ISJSON([Value]) = 1)
);
GO

/* -----------------------------------------------------------------
   3. ANAGRAFICHE
   ----------------------------------------------------------------- */

CREATE TABLE dbo.[Address] (
    Id           BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Address PRIMARY KEY CLUSTERED,
    CompanyId    BIGINT NOT NULL CONSTRAINT FK_Address_Company  REFERENCES dbo.Company(Id),
    Street       NVARCHAR(200) NOT NULL,
    StreetExtra  NVARCHAR(200) NULL,
    City         NVARCHAR(100) NOT NULL,
    ZipCode      NVARCHAR(16)  NOT NULL,
    ProvinceId   BIGINT NULL CONSTRAINT FK_Address_Province REFERENCES dbo.Province(Id),
    CountryId    BIGINT NOT NULL CONSTRAINT FK_Address_Country  REFERENCES dbo.Country(Id),
    Latitude     DECIMAL(9,6) NULL,
    Longitude    DECIMAL(9,6) NULL,
    CreatedAt    DATETIME2(3) NOT NULL CONSTRAINT DF_Address_CreatedAt DEFAULT(SYSUTCDATETIME()),
    CreatedBy    BIGINT NOT NULL,
    UpdatedAt    DATETIME2(3) NULL,
    UpdatedBy    BIGINT NULL,
    IsDeleted    BIT NOT NULL CONSTRAINT DF_Address_IsDeleted DEFAULT(0),
    RowVersion   ROWVERSION NOT NULL
);
CREATE NONCLUSTERED INDEX IX_Address_Company ON dbo.[Address](CompanyId);
GO

CREATE TABLE dbo.CustomerCategory (
    Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerCategory PRIMARY KEY CLUSTERED,
    CompanyId     BIGINT NOT NULL CONSTRAINT FK_CustomerCategory_Company REFERENCES dbo.Company(Id),
    Code          NVARCHAR(32)  NOT NULL,
    Name          NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    CreatedAt     DATETIME2(3) NOT NULL CONSTRAINT DF_CustomerCategory_CreatedAt DEFAULT(SYSUTCDATETIME()),
    CreatedBy     BIGINT NOT NULL,
    UpdatedAt     DATETIME2(3) NULL,
    UpdatedBy     BIGINT NULL,
    IsDeleted     BIT NOT NULL CONSTRAINT DF_CustomerCategory_IsDeleted DEFAULT(0),
    RowVersion    ROWVERSION NOT NULL
);
CREATE UNIQUE NONCLUSTERED INDEX UQ_CustomerCategory_Company_Code
    ON dbo.CustomerCategory(CompanyId, Code) WHERE IsDeleted = 0;
GO

CREATE TABLE dbo.PaymentTerm (
    Id                  BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PaymentTerm PRIMARY KEY CLUSTERED,
    CompanyId           BIGINT NULL CONSTRAINT FK_PaymentTerm_Company REFERENCES dbo.Company(Id),
    Code                NVARCHAR(32)  NOT NULL,
    Name                NVARCHAR(100) NOT NULL,
    PaymentMethod       NVARCHAR(32)  NOT NULL,
    InstallmentsJson    NVARCHAR(MAX) NOT NULL,
    EndOfMonth          BIT NOT NULL CONSTRAINT DF_PaymentTerm_EndOfMonth DEFAULT(0),
    ExtraDays           INT NOT NULL CONSTRAINT DF_PaymentTerm_ExtraDays DEFAULT(0),
    CreatedAt           DATETIME2(3) NOT NULL CONSTRAINT DF_PaymentTerm_CreatedAt DEFAULT(SYSUTCDATETIME()),
    CreatedBy           BIGINT NOT NULL,
    UpdatedAt           DATETIME2(3) NULL,
    UpdatedBy           BIGINT NULL,
    IsDeleted           BIT NOT NULL CONSTRAINT DF_PaymentTerm_IsDeleted DEFAULT(0),
    RowVersion          ROWVERSION NOT NULL,
    CONSTRAINT CK_PaymentTerm_Method CHECK (PaymentMethod IN
        (N'BankTransfer', N'RiBa', N'Cash', N'Check', N'SDD', N'Other')),
    CONSTRAINT CK_PaymentTerm_InstallmentsJson CHECK (ISJSON(InstallmentsJson) = 1)
);
CREATE UNIQUE NONCLUSTERED INDEX UQ_PaymentTerm_Company_Code
    ON dbo.PaymentTerm(CompanyId, Code) WHERE IsDeleted = 0;
GO

CREATE TABLE dbo.PriceList (
    Id                  BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PriceList PRIMARY KEY CLUSTERED,
    CompanyId           BIGINT NOT NULL CONSTRAINT FK_PriceList_Company  REFERENCES dbo.Company(Id),
    Code                NVARCHAR(32)  NOT NULL,
    Name                NVARCHAR(100) NOT NULL,
    PriceListType       NVARCHAR(16)  NOT NULL,
    CurrencyId          BIGINT NOT NULL CONSTRAINT FK_PriceList_Currency REFERENCES dbo.Currency(Id),
    CustomerCategoryId  BIGINT NULL CONSTRAINT FK_PriceList_CustomerCategory REFERENCES dbo.CustomerCategory(Id),
    ValidFrom           DATE NULL,
    ValidTo             DATE NULL,
    [Status]            NVARCHAR(16) NOT NULL CONSTRAINT DF_PriceList_Status DEFAULT(N'Draft'),
    CreatedAt           DATETIME2(3) NOT NULL CONSTRAINT DF_PriceList_CreatedAt DEFAULT(SYSUTCDATETIME()),
    CreatedBy           BIGINT NOT NULL,
    UpdatedAt           DATETIME2(3) NULL,
    UpdatedBy           BIGINT NULL,
    IsDeleted           BIT NOT NULL CONSTRAINT DF_PriceList_IsDeleted DEFAULT(0),
    RowVersion          ROWVERSION NOT NULL,
    CONSTRAINT CK_PriceList_Type   CHECK (PriceListType IN (N'Sales', N'Purchase')),
    CONSTRAINT CK_PriceList_Status CHECK ([Status] IN (N'Draft', N'Active', N'Archived'))
);
CREATE UNIQUE NONCLUSTERED INDEX UQ_PriceList_Company_Code
    ON dbo.PriceList(CompanyId, Code) WHERE IsDeleted = 0;
GO

CREATE TABLE dbo.Customer (
    Id                     BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Customer PRIMARY KEY CLUSTERED,
    CompanyId              BIGINT NOT NULL CONSTRAINT FK_Customer_Company REFERENCES dbo.Company(Id),
    Code                   NVARCHAR(32)  NOT NULL,
    [Type]                 NVARCHAR(16)  NOT NULL,
    BusinessName           NVARCHAR(200) NOT NULL,
    FirstName              NVARCHAR(100) NULL,
    LastName               NVARCHAR(100) NULL,
    VatNumber              NVARCHAR(20)  NULL,
    FiscalCode             NVARCHAR(16)  NULL,
    Sdi                    NVARCHAR(7)   NULL,
    Pec                    NVARCHAR(254) NULL,
    CountryId              BIGINT NOT NULL CONSTRAINT FK_Customer_Country REFERENCES dbo.Country(Id),
    CurrencyId             BIGINT NOT NULL CONSTRAINT FK_Customer_Currency REFERENCES dbo.Currency(Id),
    CustomerCategoryId     BIGINT NULL CONSTRAINT FK_Customer_Category REFERENCES dbo.CustomerCategory(Id),
    DefaultPriceListId     BIGINT NULL CONSTRAINT FK_Customer_PriceList REFERENCES dbo.PriceList(Id),
    DefaultPaymentTermId   BIGINT NULL CONSTRAINT FK_Customer_PaymentTerm REFERENCES dbo.PaymentTerm(Id),
    AgentUserId            BIGINT NULL CONSTRAINT FK_Customer_AgentUser REFERENCES dbo.[User](Id),
    CreditLimit            DECIMAL(18,2) NULL,
    DefaultDiscountPercent DECIMAL(5,2) NOT NULL CONSTRAINT DF_Customer_DiscountPct DEFAULT(0),
    InvoiceDeliveryMethod  NVARCHAR(16) NOT NULL CONSTRAINT DF_Customer_InvDelivery DEFAULT(N'Sdi'),
    IsPriceTaxIncluded     BIT NOT NULL CONSTRAINT DF_Customer_TaxIncluded DEFAULT(0),
    [Status]               NVARCHAR(16) NOT NULL CONSTRAINT DF_Customer_Status DEFAULT(N'Active'),
    Notes                  NVARCHAR(MAX) NULL,
    CreatedAt              DATETIME2(3) NOT NULL CONSTRAINT DF_Customer_CreatedAt DEFAULT(SYSUTCDATETIME()),
    CreatedBy              BIGINT NOT NULL,
    UpdatedAt              DATETIME2(3) NULL,
    UpdatedBy              BIGINT NULL,
    IsDeleted              BIT NOT NULL CONSTRAINT DF_Customer_IsDeleted DEFAULT(0),
    RowVersion             ROWVERSION NOT NULL,
    CONSTRAINT CK_Customer_Type   CHECK ([Type] IN (N'Company', N'Individual', N'Prospect')),
    CONSTRAINT CK_Customer_Status CHECK ([Status] IN (N'Active', N'Inactive', N'Prospect', N'Blocked')),
    CONSTRAINT CK_Customer_InvDelivery CHECK (InvoiceDeliveryMethod IN
        (N'Sdi', N'Pec', N'Email', N'Paper'))
);
CREATE UNIQUE NONCLUSTERED INDEX UQ_Customer_Company_Code
    ON dbo.Customer(CompanyId, Code) WHERE IsDeleted = 0;
CREATE NONCLUSTERED INDEX IX_Customer_Company_Vat
    ON dbo.Customer(CompanyId, VatNumber) WHERE IsDeleted = 0 AND VatNumber IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_Customer_Company_Cf
    ON dbo.Customer(CompanyId, FiscalCode) WHERE IsDeleted = 0 AND FiscalCode IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_Customer_Company_Status_Name
    ON dbo.Customer(CompanyId, [Status], BusinessName)
    INCLUDE (Code, VatNumber, AgentUserId);
GO

CREATE TABLE dbo.CustomerAddress (
    Id           BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerAddress PRIMARY KEY CLUSTERED,
    CompanyId    BIGINT NOT NULL CONSTRAINT FK_CustomerAddress_Company  REFERENCES dbo.Company(Id),
    CustomerId   BIGINT NOT NULL CONSTRAINT FK_CustomerAddress_Customer REFERENCES dbo.Customer(Id),
    AddressType  NVARCHAR(16) NOT NULL,
    IsDefault    BIT NOT NULL CONSTRAINT DF_CustomerAddress_IsDefault DEFAULT(0),
    AddressId    BIGINT NOT NULL CONSTRAINT FK_CustomerAddress_Address REFERENCES dbo.[Address](Id),
    CONSTRAINT CK_CustomerAddress_Type CHECK (AddressType IN
        (N'Legal', N'Shipping', N'Billing', N'Other'))
);
CREATE NONCLUSTERED INDEX IX_CustomerAddress_Customer_Type
    ON dbo.CustomerAddress(CustomerId, AddressType);
GO

CREATE TABLE dbo.CustomerContact (
    Id           BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerContact PRIMARY KEY CLUSTERED,
    CompanyId    BIGINT NOT NULL CONSTRAINT FK_CustomerContact_Company  REFERENCES dbo.Company(Id),
    CustomerId   BIGINT NOT NULL CONSTRAINT FK_CustomerContact_Customer REFERENCES dbo.Customer(Id),
    ContactType  NVARCHAR(16) NOT NULL,
    ContactName  NVARCHAR(100) NULL,
    [Value]      NVARCHAR(254) NOT NULL,
    [Role]       NVARCHAR(100) NULL,
    IsDefault    BIT NOT NULL CONSTRAINT DF_CustomerContact_IsDefault DEFAULT(0),
    CONSTRAINT CK_CustomerContact_Type CHECK (ContactType IN
        (N'Phone', N'Email', N'Mobile', N'Fax', N'Other'))
);
CREATE NONCLUSTERED INDEX IX_CustomerContact_Customer
    ON dbo.CustomerContact(CustomerId);
GO

CREATE TABLE dbo.CustomerSpecialPrice (
    Id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerSpecialPrice PRIMARY KEY CLUSTERED,
    CompanyId       BIGINT NOT NULL CONSTRAINT FK_CustomerSpecialPrice_Company REFERENCES dbo.Company(Id),
    CustomerId      BIGINT NOT NULL CONSTRAINT FK_CustomerSpecialPrice_Customer REFERENCES dbo.Customer(Id),
    ItemId          BIGINT NOT NULL,    /* FK creata dopo dbo.Item */
    UnitPrice       DECIMAL(18,4) NOT NULL,
    DiscountPercent DECIMAL(5,2)  NOT NULL CONSTRAINT DF_CustomerSpecialPrice_Disc DEFAULT(0),
    ValidFrom       DATE NOT NULL,
    ValidTo         DATE NULL,
    CreatedAt       DATETIME2(3) NOT NULL CONSTRAINT DF_CustomerSpecialPrice_CreatedAt DEFAULT(SYSUTCDATETIME()),
    CreatedBy       BIGINT NOT NULL,
    UpdatedAt       DATETIME2(3) NULL,
    UpdatedBy       BIGINT NULL,
    IsDeleted       BIT NOT NULL CONSTRAINT DF_CustomerSpecialPrice_IsDeleted DEFAULT(0),
    RowVersion      ROWVERSION NOT NULL,
    CONSTRAINT UQ_CustomerSpecialPrice_Cust_Item_ValidFrom UNIQUE (CustomerId, ItemId, ValidFrom)
);
GO

CREATE TABLE dbo.Supplier (
    Id                       BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Supplier PRIMARY KEY CLUSTERED,
    CompanyId                BIGINT NOT NULL CONSTRAINT FK_Supplier_Company REFERENCES dbo.Company(Id),
    Code                     NVARCHAR(32)  NOT NULL,
    [Type]                   NVARCHAR(16)  NOT NULL,
    BusinessName             NVARCHAR(200) NOT NULL,
    VatNumber                NVARCHAR(20)  NULL,
    FiscalCode               NVARCHAR(16)  NULL,
    Sdi                      NVARCHAR(7)   NULL,
    Pec                      NVARCHAR(254) NULL,
    VatRegime                NVARCHAR(16)  NOT NULL CONSTRAINT DF_Supplier_VatRegime DEFAULT(N'Ordinary'),
    WithholdingTaxApplicable BIT NOT NULL CONSTRAINT DF_Supplier_Withholding DEFAULT(0),
    CountryId                BIGINT NOT NULL CONSTRAINT FK_Supplier_Country REFERENCES dbo.Country(Id),
    CurrencyId               BIGINT NOT NULL CONSTRAINT FK_Supplier_Currency REFERENCES dbo.Currency(Id),
    DefaultPaymentTermId     BIGINT NULL CONSTRAINT FK_Supplier_PaymentTerm REFERENCES dbo.PaymentTerm(Id),
    [Status]                 NVARCHAR(16) NOT NULL CONSTRAINT DF_Supplier_Status DEFAULT(N'Active'),
    Notes                    NVARCHAR(MAX) NULL,
    CreatedAt                DATETIME2(3) NOT NULL CONSTRAINT DF_Supplier_CreatedAt DEFAULT(SYSUTCDATETIME()),
    CreatedBy                BIGINT NOT NULL,
    UpdatedAt                DATETIME2(3) NULL,
    UpdatedBy                BIGINT NULL,
    IsDeleted                BIT NOT NULL CONSTRAINT DF_Supplier_IsDeleted DEFAULT(0),
    RowVersion               ROWVERSION NOT NULL,
    CONSTRAINT CK_Supplier_Type      CHECK ([Type] IN (N'Company', N'Individual')),
    CONSTRAINT CK_Supplier_Status    CHECK ([Status] IN (N'Active', N'Inactive', N'Blocked')),
    CONSTRAINT CK_Supplier_VatRegime CHECK (VatRegime IN
        (N'Ordinary', N'Forfettario', N'MinimiPa', N'Other'))
);
CREATE UNIQUE NONCLUSTERED INDEX UQ_Supplier_Company_Code
    ON dbo.Supplier(CompanyId, Code) WHERE IsDeleted = 0;
CREATE NONCLUSTERED INDEX IX_Supplier_Company_Vat
    ON dbo.Supplier(CompanyId, VatNumber) WHERE IsDeleted = 0 AND VatNumber IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_Supplier_Company_Status_Name
    ON dbo.Supplier(CompanyId, [Status], BusinessName)
    INCLUDE (Code, VatNumber);
GO

CREATE TABLE dbo.SupplierAddress (
    Id           BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SupplierAddress PRIMARY KEY CLUSTERED,
    CompanyId    BIGINT NOT NULL CONSTRAINT FK_SupplierAddress_Company  REFERENCES dbo.Company(Id),
    SupplierId   BIGINT NOT NULL CONSTRAINT FK_SupplierAddress_Supplier REFERENCES dbo.Supplier(Id),
    AddressType  NVARCHAR(16) NOT NULL,
    IsDefault    BIT NOT NULL CONSTRAINT DF_SupplierAddress_IsDefault DEFAULT(0),
    AddressId    BIGINT NOT NULL CONSTRAINT FK_SupplierAddress_Address REFERENCES dbo.[Address](Id),
    CONSTRAINT CK_SupplierAddress_Type CHECK (AddressType IN
        (N'Legal', N'Shipping', N'Billing', N'Other'))
);
CREATE NONCLUSTERED INDEX IX_SupplierAddress_Supplier_Type
    ON dbo.SupplierAddress(SupplierId, AddressType);
GO

CREATE TABLE dbo.SupplierContact (
    Id           BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SupplierContact PRIMARY KEY CLUSTERED,
    CompanyId    BIGINT NOT NULL CONSTRAINT FK_SupplierContact_Company  REFERENCES dbo.Company(Id),
    SupplierId   BIGINT NOT NULL CONSTRAINT FK_SupplierContact_Supplier REFERENCES dbo.Supplier(Id),
    ContactType  NVARCHAR(16) NOT NULL,
    ContactName  NVARCHAR(100) NULL,
    [Value]      NVARCHAR(254) NOT NULL,
    [Role]       NVARCHAR(100) NULL,
    IsDefault    BIT NOT NULL CONSTRAINT DF_SupplierContact_IsDefault DEFAULT(0),
    CONSTRAINT CK_SupplierContact_Type CHECK (ContactType IN
        (N'Phone', N'Email', N'Mobile', N'Fax', N'Other'))
);
CREATE NONCLUSTERED INDEX IX_SupplierContact_Supplier
    ON dbo.SupplierContact(SupplierId);
GO

CREATE TABLE dbo.SupplierBankAccount (
    Id          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SupplierBankAccount PRIMARY KEY CLUSTERED,
    CompanyId   BIGINT NOT NULL CONSTRAINT FK_SupplierBankAccount_Company  REFERENCES dbo.Company(Id),
    SupplierId  BIGINT NOT NULL CONSTRAINT FK_SupplierBankAccount_Supplier REFERENCES dbo.Supplier(Id),
    Iban        NVARCHAR(34)  NOT NULL,
    Bic         NVARCHAR(11)  NULL,
    BankName    NVARCHAR(200) NULL,
    IsDefault   BIT NOT NULL CONSTRAINT DF_SupplierBankAccount_IsDefault DEFAULT(0),
    CreatedAt   DATETIME2(3) NOT NULL CONSTRAINT DF_SupplierBankAccount_CreatedAt DEFAULT(SYSUTCDATETIME()),
    CreatedBy   BIGINT NOT NULL,
    UpdatedAt   DATETIME2(3) NULL,
    UpdatedBy   BIGINT NULL,
    IsDeleted   BIT NOT NULL CONSTRAINT DF_SupplierBankAccount_IsDeleted DEFAULT(0),
    RowVersion  ROWVERSION NOT NULL
);
CREATE NONCLUSTERED INDEX IX_SupplierBankAccount_Supplier_Default
    ON dbo.SupplierBankAccount(SupplierId, IsDefault);
GO

CREATE TABLE dbo.ItemCategory (
    Id                  BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ItemCategory PRIMARY KEY CLUSTERED,
    CompanyId           BIGINT NOT NULL CONSTRAINT FK_ItemCategory_Company REFERENCES dbo.Company(Id),
    Code                NVARCHAR(32)  NOT NULL,
    Name                NVARCHAR(100) NOT NULL,
    [Description]       NVARCHAR(500) NULL,
    ParentCategoryId    BIGINT NULL CONSTRAINT FK_ItemCategory_Parent REFERENCES dbo.ItemCategory(Id),
    DisplayOrder        INT NOT NULL CONSTRAINT DF_ItemCategory_DisplayOrder DEFAULT(0),
    CreatedAt           DATETIME2(3) NOT NULL CONSTRAINT DF_ItemCategory_CreatedAt DEFAULT(SYSUTCDATETIME()),
    CreatedBy           BIGINT NOT NULL,
    UpdatedAt           DATETIME2(3) NULL,
    UpdatedBy           BIGINT NULL,
    IsDeleted           BIT NOT NULL CONSTRAINT DF_ItemCategory_IsDeleted DEFAULT(0),
    RowVersion          ROWVERSION NOT NULL
);
CREATE UNIQUE NONCLUSTERED INDEX UQ_ItemCategory_Company_Code
    ON dbo.ItemCategory(CompanyId, Code) WHERE IsDeleted = 0;
CREATE NONCLUSTERED INDEX IX_ItemCategory_Company_Parent
    ON dbo.ItemCategory(CompanyId, ParentCategoryId);
GO

CREATE TABLE dbo.Item (
    Id                       BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Item PRIMARY KEY CLUSTERED,
    CompanyId                BIGINT NOT NULL CONSTRAINT FK_Item_Company  REFERENCES dbo.Company(Id),
    Code                     NVARCHAR(32)  NOT NULL,
    Name                     NVARCHAR(200) NOT NULL,
    [Description]            NVARCHAR(MAX) NULL,
    ItemCategoryId           BIGINT NULL CONSTRAINT FK_Item_Category REFERENCES dbo.ItemCategory(Id),
    PrimaryUnitOfMeasureId   BIGINT NOT NULL CONSTRAINT FK_Item_Uom REFERENCES dbo.UnitOfMeasure(Id),
    ItemType                 NVARCHAR(16) NOT NULL CONSTRAINT DF_Item_Type DEFAULT(N'Goods'),
    StandardCost             DECIMAL(18,4) NULL,
    ListPrice                DECIMAL(18,4) NULL,
    VatPercent               DECIMAL(5,2)  NOT NULL CONSTRAINT DF_Item_VatPercent DEFAULT(22),
    [Status]                 NVARCHAR(16)  NOT NULL CONSTRAINT DF_Item_Status DEFAULT(N'Active'),
    IsStockManaged           BIT NOT NULL CONSTRAINT DF_Item_IsStockManaged DEFAULT(1),
    CreatedAt                DATETIME2(3) NOT NULL CONSTRAINT DF_Item_CreatedAt DEFAULT(SYSUTCDATETIME()),
    CreatedBy                BIGINT NOT NULL,
    UpdatedAt                DATETIME2(3) NULL,
    UpdatedBy                BIGINT NULL,
    IsDeleted                BIT NOT NULL CONSTRAINT DF_Item_IsDeleted DEFAULT(0),
    RowVersion               ROWVERSION NOT NULL,
    CONSTRAINT CK_Item_Type   CHECK (ItemType IN (N'Goods', N'Service')),
    CONSTRAINT CK_Item_Status CHECK ([Status] IN (N'Active', N'Inactive', N'Discontinued'))
);
CREATE UNIQUE NONCLUSTERED INDEX UQ_Item_Company_Code
    ON dbo.Item(CompanyId, Code) WHERE IsDeleted = 0;
CREATE NONCLUSTERED INDEX IX_Item_Company_Category_Status
    ON dbo.Item(CompanyId, ItemCategoryId, [Status]) INCLUDE (Code, Name);
CREATE NONCLUSTERED INDEX IX_Item_Company_Name
    ON dbo.Item(CompanyId, Name);
GO

/* FK posticipata: CustomerSpecialPrice → Item */
ALTER TABLE dbo.CustomerSpecialPrice
    ADD CONSTRAINT FK_CustomerSpecialPrice_Item FOREIGN KEY (ItemId) REFERENCES dbo.Item(Id);
GO

CREATE TABLE dbo.ItemAlternativeCode (
    Id          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ItemAlternativeCode PRIMARY KEY CLUSTERED,
    CompanyId   BIGINT NOT NULL CONSTRAINT FK_ItemAlternativeCode_Company  REFERENCES dbo.Company(Id),
    ItemId      BIGINT NOT NULL CONSTRAINT FK_ItemAlternativeCode_Item     REFERENCES dbo.Item(Id),
    CodeType    NVARCHAR(16) NOT NULL,
    Code        NVARCHAR(64) NOT NULL,
    SupplierId  BIGINT NULL CONSTRAINT FK_ItemAlternativeCode_Supplier REFERENCES dbo.Supplier(Id),
    IsDeleted   BIT NOT NULL CONSTRAINT DF_ItemAlternativeCode_IsDeleted DEFAULT(0),
    CONSTRAINT CK_ItemAlternativeCode_Type CHECK (CodeType IN
        (N'Barcode', N'Ean', N'SupplierCode', N'InternalAlt'))
);
CREATE UNIQUE NONCLUSTERED INDEX UQ_ItemAlternativeCode_Company_Type_Code
    ON dbo.ItemAlternativeCode(CompanyId, CodeType, Code) WHERE IsDeleted = 0;
GO

CREATE TABLE dbo.PriceListItem (
    Id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PriceListItem PRIMARY KEY CLUSTERED,
    CompanyId       BIGINT NOT NULL CONSTRAINT FK_PriceListItem_Company   REFERENCES dbo.Company(Id),
    PriceListId     BIGINT NOT NULL CONSTRAINT FK_PriceListItem_PriceList REFERENCES dbo.PriceList(Id),
    ItemId          BIGINT NOT NULL CONSTRAINT FK_PriceListItem_Item      REFERENCES dbo.Item(Id),
    UnitPrice       DECIMAL(18,4) NOT NULL,
    DiscountPercent DECIMAL(5,2)  NOT NULL CONSTRAINT DF_PriceListItem_Disc DEFAULT(0),
    MinQuantity     DECIMAL(18,4) NOT NULL CONSTRAINT DF_PriceListItem_MinQty DEFAULT(1),
    ValidFrom       DATE NOT NULL,
    ValidTo         DATE NULL,
    RowVersion      ROWVERSION NOT NULL,
    CONSTRAINT UQ_PriceListItem_PriceList_Item_ValidFrom UNIQUE (PriceListId, ItemId, ValidFrom)
);
CREATE NONCLUSTERED INDEX IX_PriceListItem_Item_ValidFrom
    ON dbo.PriceListItem(ItemId, ValidFrom DESC);
GO

/* -----------------------------------------------------------------
   4. INVENTORY (MVP minimale)
   ----------------------------------------------------------------- */

CREATE TABLE dbo.Warehouse (
    Id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Warehouse PRIMARY KEY CLUSTERED,
    CompanyId       BIGINT NOT NULL CONSTRAINT FK_Warehouse_Company REFERENCES dbo.Company(Id),
    Code            NVARCHAR(32)  NOT NULL,
    Name            NVARCHAR(100) NOT NULL,
    WarehouseType   NVARCHAR(16)  NOT NULL CONSTRAINT DF_Warehouse_Type DEFAULT(N'Main'),
    AddressId       BIGINT NULL CONSTRAINT FK_Warehouse_Address REFERENCES dbo.[Address](Id),
    IsDefault       BIT NOT NULL CONSTRAINT DF_Warehouse_IsDefault DEFAULT(0),
    [Status]        NVARCHAR(16) NOT NULL CONSTRAINT DF_Warehouse_Status DEFAULT(N'Active'),
    CreatedAt       DATETIME2(3) NOT NULL CONSTRAINT DF_Warehouse_CreatedAt DEFAULT(SYSUTCDATETIME()),
    CreatedBy       BIGINT NOT NULL,
    UpdatedAt       DATETIME2(3) NULL,
    UpdatedBy       BIGINT NULL,
    IsDeleted       BIT NOT NULL CONSTRAINT DF_Warehouse_IsDeleted DEFAULT(0),
    RowVersion      ROWVERSION NOT NULL,
    CONSTRAINT CK_Warehouse_Type   CHECK (WarehouseType IN
        (N'Main', N'Branch', N'Transit', N'Quarantine')),
    CONSTRAINT CK_Warehouse_Status CHECK ([Status] IN (N'Active', N'Inactive'))
);
CREATE UNIQUE NONCLUSTERED INDEX UQ_Warehouse_Company_Code
    ON dbo.Warehouse(CompanyId, Code) WHERE IsDeleted = 0;
GO

CREATE TABLE dbo.InventoryMovement (
    Id                          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InventoryMovement PRIMARY KEY CLUSTERED,
    CompanyId                   BIGINT NOT NULL CONSTRAINT FK_InventoryMovement_Company   REFERENCES dbo.Company(Id),
    WarehouseId                 BIGINT NOT NULL CONSTRAINT FK_InventoryMovement_Warehouse REFERENCES dbo.Warehouse(Id),
    ItemId                      BIGINT NOT NULL CONSTRAINT FK_InventoryMovement_Item      REFERENCES dbo.Item(Id),
    MovementDate                DATETIME2(3) NOT NULL CONSTRAINT DF_InventoryMovement_Date DEFAULT(SYSUTCDATETIME()),
    MovementType                NVARCHAR(16) NOT NULL,
    MovementReason              NVARCHAR(16) NULL,
    Quantity                    DECIMAL(18,4) NOT NULL,
    UnitOfMeasureId             BIGINT NOT NULL CONSTRAINT FK_InventoryMovement_Uom REFERENCES dbo.UnitOfMeasure(Id),
    UnitCost                    DECIMAL(18,4) NULL,
    SourceDocumentType          NVARCHAR(32) NULL,
    SourceDocumentId            BIGINT NULL,
    SourceDocumentLineNumber    INT NULL,
    CounterpartWarehouseId      BIGINT NULL CONSTRAINT FK_InventoryMovement_Counterpart REFERENCES dbo.Warehouse(Id),
    Notes                       NVARCHAR(500) NULL,
    CreatedAt                   DATETIME2(3) NOT NULL CONSTRAINT DF_InventoryMovement_CreatedAt DEFAULT(SYSUTCDATETIME()),
    CreatedBy                   BIGINT NOT NULL,
    CONSTRAINT CK_InventoryMovement_Type CHECK (MovementType IN
        (N'Inbound', N'Outbound', N'Transfer', N'Adjustment', N'Reserve', N'Release')),
    CONSTRAINT CK_InventoryMovement_QtySign CHECK
        ((MovementType IN (N'Inbound', N'Reserve', N'Release') AND Quantity >= 0)
         OR (MovementType = N'Outbound' AND Quantity <= 0)
         OR (MovementType IN (N'Transfer', N'Adjustment')))
);
CREATE NONCLUSTERED INDEX IX_InventoryMovement_Company_Wh_Item_Date
    ON dbo.InventoryMovement(CompanyId, WarehouseId, ItemId, MovementDate DESC);
CREATE NONCLUSTERED INDEX IX_InventoryMovement_Company_Date
    ON dbo.InventoryMovement(CompanyId, MovementDate DESC) INCLUDE (ItemId, MovementType, Quantity);
CREATE NONCLUSTERED INDEX IX_InventoryMovement_Item_Date
    ON dbo.InventoryMovement(ItemId, MovementDate DESC);
GO

CREATE TABLE dbo.InventoryStock (
    Id                  BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InventoryStock PRIMARY KEY CLUSTERED,
    CompanyId           BIGINT NOT NULL CONSTRAINT FK_InventoryStock_Company   REFERENCES dbo.Company(Id),
    WarehouseId         BIGINT NOT NULL CONSTRAINT FK_InventoryStock_Warehouse REFERENCES dbo.Warehouse(Id),
    ItemId              BIGINT NOT NULL CONSTRAINT FK_InventoryStock_Item      REFERENCES dbo.Item(Id),
    QuantityOnHand      DECIMAL(18,4) NOT NULL CONSTRAINT DF_InventoryStock_OnHand   DEFAULT(0),
    QuantityReserved    DECIMAL(18,4) NOT NULL CONSTRAINT DF_InventoryStock_Reserved DEFAULT(0),
    QuantityAvailable   AS (QuantityOnHand - QuantityReserved) PERSISTED,
    AverageCost         DECIMAL(18,4) NULL,
    LastMovementAt      DATETIME2(3) NULL,
    UpdatedAt           DATETIME2(3) NOT NULL CONSTRAINT DF_InventoryStock_UpdatedAt DEFAULT(SYSUTCDATETIME()),
    RowVersion          ROWVERSION NOT NULL,
    CONSTRAINT UQ_InventoryStock_Company_Wh_Item UNIQUE (CompanyId, WarehouseId, ItemId)
);
CREATE NONCLUSTERED INDEX IX_InventoryStock_Item
    ON dbo.InventoryStock(ItemId) INCLUDE (WarehouseId, QuantityOnHand, QuantityReserved);
GO

/* -----------------------------------------------------------------
   5. TRANSACTIONS (preview MVP minimale)
   ----------------------------------------------------------------- */

CREATE TABLE dbo.DocumentNumberSequence (
    Id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DocumentNumberSequence PRIMARY KEY CLUSTERED,
    CompanyId       BIGINT NOT NULL CONSTRAINT FK_DocNumSeq_Company REFERENCES dbo.Company(Id),
    DocumentType    NVARCHAR(32) NOT NULL,
    [Year]          INT NOT NULL,
    Prefix          NVARCHAR(16) NULL,
    LastNumber      BIGINT NOT NULL CONSTRAINT DF_DocNumSeq_Last DEFAULT(0),
    NumberFormat    NVARCHAR(64) NOT NULL CONSTRAINT DF_DocNumSeq_Format DEFAULT(N'{prefix}{year}/{number:D6}'),
    RowVersion      ROWVERSION NOT NULL,
    CONSTRAINT UQ_DocNumSeq_Company_Type_Year UNIQUE (CompanyId, DocumentType, [Year])
);
GO

CREATE TABLE dbo.SalesOrder (
    Id                       BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SalesOrder PRIMARY KEY CLUSTERED,
    CompanyId                BIGINT NOT NULL CONSTRAINT FK_SalesOrder_Company  REFERENCES dbo.Company(Id),
    Number                   NVARCHAR(32) NOT NULL,
    OrderDate                DATE NOT NULL,
    CustomerId               BIGINT NOT NULL CONSTRAINT FK_SalesOrder_Customer REFERENCES dbo.Customer(Id),
    CustomerOrderReference   NVARCHAR(64) NULL,
    BillingAddressId         BIGINT NULL CONSTRAINT FK_SalesOrder_Billing  REFERENCES dbo.[Address](Id),
    ShippingAddressId        BIGINT NULL CONSTRAINT FK_SalesOrder_Shipping REFERENCES dbo.[Address](Id),
    PriceListId              BIGINT NULL CONSTRAINT FK_SalesOrder_PriceList REFERENCES dbo.PriceList(Id),
    PaymentTermId            BIGINT NULL CONSTRAINT FK_SalesOrder_PaymentTerm REFERENCES dbo.PaymentTerm(Id),
    AgentUserId              BIGINT NULL CONSTRAINT FK_SalesOrder_Agent REFERENCES dbo.[User](Id),
    WarehouseId              BIGINT NULL CONSTRAINT FK_SalesOrder_Warehouse REFERENCES dbo.Warehouse(Id),
    CurrencyId               BIGINT NOT NULL CONSTRAINT FK_SalesOrder_Currency REFERENCES dbo.Currency(Id),
    ExchangeRate             DECIMAL(18,6) NOT NULL CONSTRAINT DF_SalesOrder_ExchangeRate DEFAULT(1),
    HeaderDiscountPercent    DECIMAL(5,2)  NOT NULL CONSTRAINT DF_SalesOrder_HeaderDisc DEFAULT(0),
    NetAmount                DECIMAL(18,2) NOT NULL CONSTRAINT DF_SalesOrder_Net DEFAULT(0),
    VatAmount                DECIMAL(18,2) NOT NULL CONSTRAINT DF_SalesOrder_Vat DEFAULT(0),
    TotalAmount              DECIMAL(18,2) NOT NULL CONSTRAINT DF_SalesOrder_Total DEFAULT(0),
    [Status]                 NVARCHAR(24) NOT NULL CONSTRAINT DF_SalesOrder_Status DEFAULT(N'Draft'),
    ExpectedDeliveryDate     DATE NULL,
    Notes                    NVARCHAR(MAX) NULL,
    CreatedAt                DATETIME2(3) NOT NULL CONSTRAINT DF_SalesOrder_CreatedAt DEFAULT(SYSUTCDATETIME()),
    CreatedBy                BIGINT NOT NULL,
    UpdatedAt                DATETIME2(3) NULL,
    UpdatedBy                BIGINT NULL,
    IsDeleted                BIT NOT NULL CONSTRAINT DF_SalesOrder_IsDeleted DEFAULT(0),
    RowVersion               ROWVERSION NOT NULL,
    CONSTRAINT CK_SalesOrder_Status CHECK ([Status] IN
        (N'Draft', N'Confirmed', N'PartiallyShipped', N'Shipped', N'Invoiced', N'Cancelled'))
);
CREATE UNIQUE NONCLUSTERED INDEX UQ_SalesOrder_Company_Number
    ON dbo.SalesOrder(CompanyId, Number) WHERE IsDeleted = 0;
CREATE NONCLUSTERED INDEX IX_SalesOrder_Company_Status_OrderDate
    ON dbo.SalesOrder(CompanyId, [Status], OrderDate DESC)
    INCLUDE (CustomerId, TotalAmount);
CREATE NONCLUSTERED INDEX IX_SalesOrder_Company_Customer_OrderDate
    ON dbo.SalesOrder(CompanyId, CustomerId, OrderDate DESC);
GO

CREATE TABLE dbo.SalesOrderLine (
    Id                  BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SalesOrderLine PRIMARY KEY CLUSTERED,
    CompanyId           BIGINT NOT NULL CONSTRAINT FK_SalesOrderLine_Company REFERENCES dbo.Company(Id),
    SalesOrderId        BIGINT NOT NULL CONSTRAINT FK_SalesOrderLine_Order   REFERENCES dbo.SalesOrder(Id),
    LineNumber          INT NOT NULL,
    ItemId              BIGINT NOT NULL CONSTRAINT FK_SalesOrderLine_Item    REFERENCES dbo.Item(Id),
    [Description]       NVARCHAR(500) NOT NULL,
    OrderedQuantity     DECIMAL(18,4) NOT NULL,
    DeliveredQuantity   DECIMAL(18,4) NOT NULL CONSTRAINT DF_SalesOrderLine_Delivered DEFAULT(0),
    InvoicedQuantity    DECIMAL(18,4) NOT NULL CONSTRAINT DF_SalesOrderLine_Invoiced  DEFAULT(0),
    UnitOfMeasureId     BIGINT NOT NULL CONSTRAINT FK_SalesOrderLine_Uom REFERENCES dbo.UnitOfMeasure(Id),
    UnitPrice           DECIMAL(18,4) NOT NULL,
    LineDiscountPercent DECIMAL(5,2)  NOT NULL CONSTRAINT DF_SalesOrderLine_Disc DEFAULT(0),
    VatPercent          DECIMAL(5,2)  NOT NULL,
    NetAmount           DECIMAL(18,2) NOT NULL,
    VatAmount           DECIMAL(18,2) NOT NULL,
    TotalAmount         DECIMAL(18,2) NOT NULL,
    LineStatus          NVARCHAR(24)  NOT NULL CONSTRAINT DF_SalesOrderLine_Status DEFAULT(N'Open'),
    CreatedAt           DATETIME2(3) NOT NULL CONSTRAINT DF_SalesOrderLine_CreatedAt DEFAULT(SYSUTCDATETIME()),
    CreatedBy           BIGINT NOT NULL,
    UpdatedAt           DATETIME2(3) NULL,
    UpdatedBy           BIGINT NULL,
    RowVersion          ROWVERSION NOT NULL,
    CONSTRAINT CK_SalesOrderLine_Status CHECK (LineStatus IN
        (N'Open', N'PartiallyShipped', N'Closed', N'Cancelled')),
    CONSTRAINT UQ_SalesOrderLine_Order_LineNumber UNIQUE (SalesOrderId, LineNumber)
);
CREATE NONCLUSTERED INDEX IX_SalesOrderLine_Item
    ON dbo.SalesOrderLine(ItemId) INCLUDE (SalesOrderId, OrderedQuantity);
GO

CREATE TABLE dbo.PurchaseOrder (
    Id                      BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PurchaseOrder PRIMARY KEY CLUSTERED,
    CompanyId               BIGINT NOT NULL CONSTRAINT FK_PurchaseOrder_Company  REFERENCES dbo.Company(Id),
    Number                  NVARCHAR(32) NOT NULL,
    OrderDate               DATE NOT NULL,
    SupplierId              BIGINT NOT NULL CONSTRAINT FK_PurchaseOrder_Supplier REFERENCES dbo.Supplier(Id),
    SupplierOrderReference  NVARCHAR(64) NULL,
    DeliveryAddressId       BIGINT NULL CONSTRAINT FK_PurchaseOrder_Delivery REFERENCES dbo.[Address](Id),
    PaymentTermId           BIGINT NULL CONSTRAINT FK_PurchaseOrder_PaymentTerm REFERENCES dbo.PaymentTerm(Id),
    WarehouseId             BIGINT NULL CONSTRAINT FK_PurchaseOrder_Warehouse REFERENCES dbo.Warehouse(Id),
    CurrencyId              BIGINT NOT NULL CONSTRAINT FK_PurchaseOrder_Currency REFERENCES dbo.Currency(Id),
    ExchangeRate            DECIMAL(18,6) NOT NULL CONSTRAINT DF_PurchaseOrder_ExchangeRate DEFAULT(1),
    HeaderDiscountPercent   DECIMAL(5,2)  NOT NULL CONSTRAINT DF_PurchaseOrder_HeaderDisc DEFAULT(0),
    NetAmount               DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseOrder_Net DEFAULT(0),
    VatAmount               DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseOrder_Vat DEFAULT(0),
    TotalAmount             DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseOrder_Total DEFAULT(0),
    [Status]                NVARCHAR(24) NOT NULL CONSTRAINT DF_PurchaseOrder_Status DEFAULT(N'Draft'),
    ExpectedDeliveryDate    DATE NULL,
    Notes                   NVARCHAR(MAX) NULL,
    CreatedAt               DATETIME2(3) NOT NULL CONSTRAINT DF_PurchaseOrder_CreatedAt DEFAULT(SYSUTCDATETIME()),
    CreatedBy               BIGINT NOT NULL,
    UpdatedAt               DATETIME2(3) NULL,
    UpdatedBy               BIGINT NULL,
    IsDeleted               BIT NOT NULL CONSTRAINT DF_PurchaseOrder_IsDeleted DEFAULT(0),
    RowVersion              ROWVERSION NOT NULL,
    CONSTRAINT CK_PurchaseOrder_Status CHECK ([Status] IN
        (N'Draft', N'Sent', N'PartiallyReceived', N'Received', N'Invoiced', N'Cancelled'))
);
CREATE UNIQUE NONCLUSTERED INDEX UQ_PurchaseOrder_Company_Number
    ON dbo.PurchaseOrder(CompanyId, Number) WHERE IsDeleted = 0;
CREATE NONCLUSTERED INDEX IX_PurchaseOrder_Company_Status_OrderDate
    ON dbo.PurchaseOrder(CompanyId, [Status], OrderDate DESC)
    INCLUDE (SupplierId, TotalAmount);
CREATE NONCLUSTERED INDEX IX_PurchaseOrder_Company_Supplier_OrderDate
    ON dbo.PurchaseOrder(CompanyId, SupplierId, OrderDate DESC);
GO

CREATE TABLE dbo.PurchaseOrderLine (
    Id                  BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PurchaseOrderLine PRIMARY KEY CLUSTERED,
    CompanyId           BIGINT NOT NULL CONSTRAINT FK_PurchaseOrderLine_Company REFERENCES dbo.Company(Id),
    PurchaseOrderId     BIGINT NOT NULL CONSTRAINT FK_PurchaseOrderLine_Order   REFERENCES dbo.PurchaseOrder(Id),
    LineNumber          INT NOT NULL,
    ItemId              BIGINT NOT NULL CONSTRAINT FK_PurchaseOrderLine_Item    REFERENCES dbo.Item(Id),
    [Description]       NVARCHAR(500) NOT NULL,
    OrderedQuantity     DECIMAL(18,4) NOT NULL,
    ReceivedQuantity    DECIMAL(18,4) NOT NULL CONSTRAINT DF_PurchaseOrderLine_Received DEFAULT(0),
    InvoicedQuantity    DECIMAL(18,4) NOT NULL CONSTRAINT DF_PurchaseOrderLine_Invoiced DEFAULT(0),
    UnitOfMeasureId     BIGINT NOT NULL CONSTRAINT FK_PurchaseOrderLine_Uom REFERENCES dbo.UnitOfMeasure(Id),
    UnitPrice           DECIMAL(18,4) NOT NULL,
    LineDiscountPercent DECIMAL(5,2)  NOT NULL CONSTRAINT DF_PurchaseOrderLine_Disc DEFAULT(0),
    VatPercent          DECIMAL(5,2)  NOT NULL,
    NetAmount           DECIMAL(18,2) NOT NULL,
    VatAmount           DECIMAL(18,2) NOT NULL,
    TotalAmount         DECIMAL(18,2) NOT NULL,
    LineStatus          NVARCHAR(24)  NOT NULL CONSTRAINT DF_PurchaseOrderLine_Status DEFAULT(N'Open'),
    CreatedAt           DATETIME2(3) NOT NULL CONSTRAINT DF_PurchaseOrderLine_CreatedAt DEFAULT(SYSUTCDATETIME()),
    CreatedBy           BIGINT NOT NULL,
    UpdatedAt           DATETIME2(3) NULL,
    UpdatedBy           BIGINT NULL,
    RowVersion          ROWVERSION NOT NULL,
    CONSTRAINT CK_PurchaseOrderLine_Status CHECK (LineStatus IN
        (N'Open', N'PartiallyReceived', N'Closed', N'Cancelled')),
    CONSTRAINT UQ_PurchaseOrderLine_Order_LineNumber UNIQUE (PurchaseOrderId, LineNumber)
);
CREATE NONCLUSTERED INDEX IX_PurchaseOrderLine_Item
    ON dbo.PurchaseOrderLine(ItemId) INCLUDE (PurchaseOrderId, OrderedQuantity);
GO

/* -----------------------------------------------------------------
   6. FK POSTICIPATE
   ----------------------------------------------------------------- */
ALTER TABLE dbo.[User]
    ADD CONSTRAINT FK_User_User_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.[User](Id),
        CONSTRAINT FK_User_User_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.[User](Id);
GO
/* Pattern equivalente per CreatedBy/UpdatedBy su Company, Role, Customer, ecc.
   è applicato in migration EF (omesso qui per concisione). */

/* -----------------------------------------------------------------
   7. SEED MINIMO (esempio - dettagli in migration EF)
   ----------------------------------------------------------------- */
-- INSERT INTO dbo.Country (IsoCode2, IsoCode3, Name, IsEuMember, PhonePrefix)
--   VALUES (N'IT', N'ITA', N'Italia', 1, N'+39');
-- INSERT INTO dbo.Currency (IsoCode, Name, Symbol, DecimalDigits)
--   VALUES (N'EUR', N'Euro', N'€', 2);
-- INSERT INTO dbo.UnitOfMeasure (Code, Name, UnitType)
--   VALUES (N'PZ', N'Pezzi', N'Quantity'), (N'KG', N'Chilogrammi', N'Weight');
-- INSERT INTO dbo.Permission (Code, Module, Description) VALUES ...
-- (popolato via EF HasData; vedi migration SeedSystemRolesAndPermissions)
GO
