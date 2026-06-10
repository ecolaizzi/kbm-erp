using KBM.Domain.Entities;
using KBM.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;

namespace KBM.Infrastructure.Persistence;

public class KbmDbContext(DbContextOptions<KbmDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<UserCompany> UserCompanies => Set<UserCompany>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<ReportDefinition> ReportDefinitions => Set<ReportDefinition>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<CustomerContact> CustomerContacts => Set<CustomerContact>();
    public DbSet<CustomerBank> CustomerBanks => Set<CustomerBank>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SupplierAddress> SupplierAddresses => Set<SupplierAddress>();
    public DbSet<SupplierContact> SupplierContacts => Set<SupplierContact>();
    public DbSet<SupplierBank> SupplierBanks => Set<SupplierBank>();
    public DbSet<ItemCategory> ItemCategories => Set<ItemCategory>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<PaymentTerm> PaymentTerms => Set<PaymentTerm>();
    public DbSet<AccountMaster> AccountMasters => Set<AccountMaster>();
    public DbSet<PurchaseRequest> PurchaseRequests => Set<PurchaseRequest>();
    public DbSet<PurchaseRequestLine> PurchaseRequestLines => Set<PurchaseRequestLine>();
    public DbSet<PurchaseRequestLineSupplier> PurchaseRequestLineSuppliers => Set<PurchaseRequestLineSupplier>();
    public DbSet<Rfq> Rfqs => Set<Rfq>();
    public DbSet<RfqLine> RfqLines => Set<RfqLine>();
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
    public DbSet<WorkflowTask> WorkflowTasks => Set<WorkflowTask>();
    public DbSet<WorkflowEvent> WorkflowEvents => Set<WorkflowEvent>();

    // Modulo ordini (struttura dati ambsor02)
    public DbSet<VatCode> VatCodes => Set<VatCode>();
    public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<WarehouseReason> WarehouseReasons => Set<WarehouseReason>();
    public DbSet<Carrier> Carriers => Set<Carrier>();
    public DbSet<PortType> PortTypes => Set<PortType>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<PriceListLine> PriceListLines => Set<PriceListLine>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("User");
            e.HasIndex(x => x.Username).IsUnique().HasFilter("[IsDeleted] = 0");
            e.HasIndex(x => x.Email).IsUnique().HasFilter("[IsDeleted] = 0");
            e.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<Company>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique().HasFilter("[IsDeleted] = 0");
            e.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<UserCompany>(e =>
        {
            e.HasIndex(x => new { x.UserId, x.CompanyId }).IsUnique();
            e.HasOne(x => x.User).WithMany(u => u.UserCompanies).HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Company).WithMany(c => c.UserCompanies).HasForeignKey(x => x.CompanyId);
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasIndex(x => x.TokenHash).IsUnique();
            e.HasOne(x => x.User).WithMany(u => u.RefreshTokens).HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<SystemSetting>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.Key }).IsUnique();
            e.Property(x => x.Key).HasMaxLength(150);
            e.Property(x => x.Category).HasMaxLength(60);
        });

        modelBuilder.Entity<ReportDefinition>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.Key }).IsUnique();
            e.Property(x => x.Key).HasMaxLength(100);
            e.Property(x => x.Title).HasMaxLength(150);
            e.Property(x => x.TemplatePathOrName).HasMaxLength(400);
            e.Property(x => x.Engine).HasConversion<int>();
            e.Property(x => x.OutputFormat).HasConversion<int>();
        });

        modelBuilder.Entity<Role>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
            e.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).IsRequired(false);
            e.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<Permission>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<UserRole>(e =>
        {
            e.HasIndex(x => new { x.UserId, x.RoleId, x.CompanyId }).IsUnique();
            e.HasOne(x => x.User).WithMany(u => u.UserRoles).HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Role).WithMany(r => r.UserRoles).HasForeignKey(x => x.RoleId);
            e.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId);
        });

        modelBuilder.Entity<RolePermission>(e =>
        {
            e.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique();
            e.HasOne(x => x.Role).WithMany(r => r.RolePermissions).HasForeignKey(x => x.RoleId);
            e.HasOne(x => x.Permission).WithMany(p => p.RolePermissions).HasForeignKey(x => x.PermissionId);
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasIndex(x => x.Timestamp);
            e.HasIndex(x => new { x.CompanyId, x.Timestamp });
        });

        modelBuilder.Entity<Customer>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
            e.HasIndex(x => new { x.CompanyId, x.BusinessName });
            e.Property(x => x.Code).HasMaxLength(50);
            e.Property(x => x.BusinessName).HasMaxLength(200);
            e.Property(x => x.VatNumber).HasMaxLength(20);
            e.Property(x => x.FiscalCode).HasMaxLength(20);
            e.Property(x => x.SdiCode).HasMaxLength(7);
            e.Property(x => x.Country).HasMaxLength(2);
            e.Property(x => x.Status).HasMaxLength(20);
            e.Property(x => x.PaymentMethod).HasMaxLength(50);
            e.Property(x => x.PriceListCode).HasMaxLength(50);
            e.Property(x => x.AgentCode).HasMaxLength(50);
            e.Property(x => x.Zone).HasMaxLength(50);
            e.Property(x => x.VatExemptionCode).HasMaxLength(20);
            e.Property(x => x.AccountCode).HasMaxLength(50);
            e.Property(x => x.CreditLimit).HasColumnType("decimal(18,2)");
            e.Property(x => x.DiscountPercent).HasColumnType("decimal(5,2)");
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasMany(x => x.Addresses).WithOne(a => a.Customer).HasForeignKey(a => a.CustomerId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Contacts).WithOne(c => c.Customer).HasForeignKey(c => c.CustomerId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Banks).WithOne(b => b.Customer).HasForeignKey(b => b.CustomerId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomerAddress>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.CustomerId });
            e.Property(x => x.AddressType).HasMaxLength(20);
            e.Property(x => x.Description).HasMaxLength(150);
            e.Property(x => x.City).HasMaxLength(100);
            e.Property(x => x.Province).HasMaxLength(5);
            e.Property(x => x.PostalCode).HasMaxLength(10);
            e.Property(x => x.Country).HasMaxLength(2);
            e.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<CustomerContact>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.CustomerId });
            e.Property(x => x.Name).HasMaxLength(150);
            e.Property(x => x.Role).HasMaxLength(100);
            e.Property(x => x.Email).HasMaxLength(150);
            e.Property(x => x.Phone).HasMaxLength(40);
            e.Property(x => x.Mobile).HasMaxLength(40);
            e.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<CustomerBank>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.CustomerId });
            e.Property(x => x.BankName).HasMaxLength(150);
            e.Property(x => x.Iban).HasMaxLength(34);
            e.Property(x => x.Swift).HasMaxLength(11);
            e.Property(x => x.Abi).HasMaxLength(5);
            e.Property(x => x.Cab).HasMaxLength(5);
            e.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<Supplier>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
            e.HasIndex(x => new { x.CompanyId, x.BusinessName });
            e.Property(x => x.Code).HasMaxLength(50);
            e.Property(x => x.BusinessName).HasMaxLength(200);
            e.Property(x => x.VatNumber).HasMaxLength(20);
            e.Property(x => x.FiscalCode).HasMaxLength(20);
            e.Property(x => x.SdiCode).HasMaxLength(7);
            e.Property(x => x.Country).HasMaxLength(2);
            e.Property(x => x.Status).HasMaxLength(20);
            e.Property(x => x.PaymentMethod).HasMaxLength(50);
            e.Property(x => x.PriceListCode).HasMaxLength(50);
            e.Property(x => x.Zone).HasMaxLength(50);
            e.Property(x => x.VatExemptionCode).HasMaxLength(20);
            e.Property(x => x.AccountCode).HasMaxLength(50);
            e.Property(x => x.CreditLimit).HasColumnType("decimal(18,2)");
            e.Property(x => x.DiscountPercent).HasColumnType("decimal(5,2)");
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasMany(x => x.Addresses).WithOne(a => a.Supplier).HasForeignKey(a => a.SupplierId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Contacts).WithOne(c => c.Supplier).HasForeignKey(c => c.SupplierId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Banks).WithOne(b => b.Supplier).HasForeignKey(b => b.SupplierId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SupplierAddress>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.SupplierId });
            e.Property(x => x.AddressType).HasMaxLength(20);
            e.Property(x => x.Description).HasMaxLength(150);
            e.Property(x => x.City).HasMaxLength(100);
            e.Property(x => x.Province).HasMaxLength(5);
            e.Property(x => x.PostalCode).HasMaxLength(10);
            e.Property(x => x.Country).HasMaxLength(2);
            e.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<SupplierContact>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.SupplierId });
            e.Property(x => x.Name).HasMaxLength(150);
            e.Property(x => x.Role).HasMaxLength(100);
            e.Property(x => x.Email).HasMaxLength(150);
            e.Property(x => x.Phone).HasMaxLength(40);
            e.Property(x => x.Mobile).HasMaxLength(40);
            e.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<SupplierBank>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.SupplierId });
            e.Property(x => x.BankName).HasMaxLength(150);
            e.Property(x => x.Iban).HasMaxLength(34);
            e.Property(x => x.Swift).HasMaxLength(11);
            e.Property(x => x.Abi).HasMaxLength(5);
            e.Property(x => x.Cab).HasMaxLength(5);
            e.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<ItemCategory>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
            e.Property(x => x.Code).HasMaxLength(50);
            e.Property(x => x.Name).HasMaxLength(150);
            e.Property(x => x.Status).HasMaxLength(20);
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasMany(x => x.Items).WithOne(i => i.Category).HasForeignKey(i => i.CategoryId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Item>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
            e.HasIndex(x => new { x.CompanyId, x.Description });
            e.Property(x => x.Code).HasMaxLength(50);
            e.Property(x => x.Description).HasMaxLength(250);
            e.Property(x => x.UnitOfMeasure).HasMaxLength(10);
            e.Property(x => x.Barcode).HasMaxLength(30);
            e.Property(x => x.SupplierItemCode).HasMaxLength(50);
            e.Property(x => x.RevenueAccount).HasMaxLength(50);
            e.Property(x => x.CostAccount).HasMaxLength(50);
            e.Property(x => x.Status).HasMaxLength(20);
            e.Property(x => x.BasePrice).HasColumnType("decimal(18,4)");
            e.Property(x => x.VatRate).HasColumnType("decimal(5,2)");
            e.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<PaymentTerm>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
            e.Property(x => x.Code).HasMaxLength(20);
            e.Property(x => x.Description).HasMaxLength(150);
            e.Property(x => x.PaymentMethod).HasMaxLength(50);
            e.Property(x => x.Status).HasMaxLength(20);
            e.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<AccountMaster>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.FullCode }).IsUnique().HasFilter("[IsDeleted] = 0");
            e.HasIndex(x => new { x.CompanyId, x.Level });
            e.Property(x => x.Code).HasMaxLength(20);
            e.Property(x => x.FullCode).HasMaxLength(60);
            e.Property(x => x.Name).HasMaxLength(150);
            e.Property(x => x.Level).HasConversion<int>();
            e.Property(x => x.Nature).HasConversion<int>();
            e.Property(x => x.Sign).HasConversion<int>();
            e.Property(x => x.SubKind).HasConversion<int>();
            e.Property(x => x.BilCeeDare).HasMaxLength(20);
            e.Property(x => x.BilCeeAvere).HasMaxLength(20);
            e.Property(x => x.Status).HasMaxLength(20);
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasOne(x => x.Parent).WithMany(p => p.Children).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseRequest>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.Number }).IsUnique().HasFilter("[IsDeleted] = 0");
            e.HasIndex(x => new { x.CompanyId, x.Date });
            e.Property(x => x.Number).HasMaxLength(30);
            e.Property(x => x.RequestingUnit).HasMaxLength(100);
            e.Property(x => x.Status).HasMaxLength(30);
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasMany(x => x.Lines).WithOne(l => l.PurchaseRequest).HasForeignKey(l => l.PurchaseRequestId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PurchaseRequestLine>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.PurchaseRequestId });
            e.Property(x => x.Description).HasMaxLength(250);
            e.Property(x => x.UnitOfMeasure).HasMaxLength(10);
            e.Property(x => x.LineStatus).HasMaxLength(30);
            e.Property(x => x.Quantity).HasColumnType("decimal(18,4)");
            e.Property(x => x.ProposedPrice).HasColumnType("decimal(18,4)");
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasOne(x => x.Item).WithMany().HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.NoAction);
            e.HasMany(x => x.Suppliers).WithOne(s => s.PurchaseRequestLine).HasForeignKey(s => s.PurchaseRequestLineId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PurchaseRequestLineSupplier>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.PurchaseRequestLineId });
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Rfq>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.Number }).IsUnique().HasFilter("[IsDeleted] = 0");
            e.HasIndex(x => new { x.CompanyId, x.Date });
            e.Property(x => x.Number).HasMaxLength(30);
            e.Property(x => x.Status).HasMaxLength(30);
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.PurchaseRequest).WithMany().HasForeignKey(x => x.PurchaseRequestId).OnDelete(DeleteBehavior.NoAction);
            e.HasMany(x => x.Lines).WithOne(l => l.Rfq).HasForeignKey(l => l.RfqId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RfqLine>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.RfqId });
            e.Property(x => x.Description).HasMaxLength(250);
            e.Property(x => x.UnitOfMeasure).HasMaxLength(10);
            e.Property(x => x.Quantity).HasColumnType("decimal(18,4)");
            e.Property(x => x.UnitPrice).HasColumnType("decimal(18,4)");
            e.Property(x => x.DiscountPercent).HasColumnType("decimal(5,2)");
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasOne(x => x.Item).WithMany().HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.NoAction);
        });

        ConfigureOrderModule(modelBuilder);

        modelBuilder.Entity<WorkflowDefinition>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
            e.Property(x => x.Code).HasMaxLength(30);
            e.Property(x => x.Name).HasMaxLength(150);
            e.Property(x => x.TargetEntityType).HasMaxLength(50);
            e.Property(x => x.Status).HasMaxLength(20);
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasMany(x => x.Steps).WithOne(s => s.Definition).HasForeignKey(s => s.WorkflowDefinitionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkflowStep>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.WorkflowDefinitionId, x.StepOrder });
            e.Property(x => x.Code).HasMaxLength(30);
            e.Property(x => x.Name).HasMaxLength(150);
            e.Property(x => x.StepType).HasConversion<int>();
            e.Property(x => x.AssigneeType).HasConversion<int>();
            e.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<WorkflowInstance>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.Number }).IsUnique().HasFilter("[IsDeleted] = 0");
            e.HasIndex(x => new { x.CompanyId, x.State });
            e.HasIndex(x => new { x.CompanyId, x.LinkedEntityType, x.LinkedEntityId });
            e.Property(x => x.Number).HasMaxLength(30);
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.State).HasConversion<int>();
            e.Property(x => x.LinkedEntityType).HasMaxLength(50);
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasOne(x => x.Definition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.NoAction);
            e.HasMany(x => x.Tasks).WithOne(t => t.Instance).HasForeignKey(t => t.WorkflowInstanceId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Events).WithOne(v => v.Instance).HasForeignKey(v => v.WorkflowInstanceId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkflowTask>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.WorkflowInstanceId });
            e.HasIndex(x => new { x.CompanyId, x.State });
            e.Property(x => x.StepName).HasMaxLength(150);
            e.Property(x => x.StepType).HasConversion<int>();
            e.Property(x => x.State).HasConversion<int>();
            e.Property(x => x.AssigneeType).HasConversion<int>();
            e.Property(x => x.Decision).HasMaxLength(100);
            e.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<WorkflowEvent>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.WorkflowInstanceId, x.Timestamp });
            e.Property(x => x.Action).HasMaxLength(40);
        });
    }

    private static void ConfigureOrderModule(ModelBuilder modelBuilder)
    {
        void BaseCode<T>(string table) where T : class
        {
            modelBuilder.Entity<T>(e =>
            {
                e.ToTable(table);
                e.HasIndex("CompanyId", "Code").IsUnique().HasFilter("[IsDeleted] = 0");
                e.Property("Code").HasMaxLength(20);
                e.Property("Description").HasMaxLength(150);
                e.Property("Status").HasMaxLength(20);
                e.Property("RowVersion").IsRowVersion();
            });
        }

        BaseCode<VatCode>("VatCodes");
        modelBuilder.Entity<VatCode>(e =>
        {
            e.Property(x => x.Rate).HasColumnType("decimal(5,2)");
            e.Property(x => x.NatureCode).HasMaxLength(10);
            e.Property(x => x.DeductibilityPercent).HasColumnType("decimal(5,2)");
        });

        BaseCode<UnitOfMeasure>("UnitsOfMeasure");
        BaseCode<Zone>("Zones");
        BaseCode<Carrier>("Carriers");
        modelBuilder.Entity<Carrier>(e => e.Property(x => x.VatNumber).HasMaxLength(20));
        BaseCode<PortType>("PortTypes");
        modelBuilder.Entity<PortType>(e => e.Property(x => x.Charge).HasConversion<int>());
        BaseCode<DocumentType>("DocumentTypes");
        modelBuilder.Entity<DocumentType>(e =>
        {
            e.Property(x => x.Category).HasConversion<int>();
            e.Property(x => x.NumberingPrefix).HasMaxLength(20);
        });
        BaseCode<Currency>("Currencies");
        modelBuilder.Entity<Currency>(e =>
        {
            e.Property(x => x.Symbol).HasMaxLength(5);
        });

        modelBuilder.Entity<Warehouse>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
            e.Property(x => x.Code).HasMaxLength(20);
            e.Property(x => x.Description).HasMaxLength(150);
            e.Property(x => x.Kind).HasConversion<int>();
            e.Property(x => x.Status).HasMaxLength(20);
            e.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<WarehouseReason>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
            e.Property(x => x.Code).HasMaxLength(20);
            e.Property(x => x.Description).HasMaxLength(150);
            e.Property(x => x.MovementSign).HasConversion<int>();
            e.Property(x => x.Category).HasConversion<int>();
            e.Property(x => x.Status).HasMaxLength(20);
            e.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<PriceList>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
            e.Property(x => x.Code).HasMaxLength(20);
            e.Property(x => x.Description).HasMaxLength(150);
            e.Property(x => x.Kind).HasConversion<int>();
            e.Property(x => x.Status).HasMaxLength(20);
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasMany(x => x.Lines).WithOne(l => l.PriceList).HasForeignKey(l => l.PriceListId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PriceListLine>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.PriceListId, x.ItemId });
            e.Property(x => x.UnitPrice).HasColumnType("decimal(18,4)");
            e.Property(x => x.DiscountPercent).HasColumnType("decimal(5,2)");
            e.Property(x => x.MinQuantity).HasColumnType("decimal(18,4)");
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasOne(x => x.Item).WithMany().HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<SalesOrder>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.Number }).IsUnique().HasFilter("[IsDeleted] = 0");
            e.Property(x => x.Number).HasMaxLength(30);
            e.Property(x => x.CustomerOrderReference).HasMaxLength(50);
            e.Property(x => x.Status).HasConversion<int>();
            e.Property(x => x.ExchangeRate).HasColumnType("decimal(18,6)");
            e.Property(x => x.HeaderDiscountPercent).HasColumnType("decimal(5,2)");
            e.Property(x => x.NetAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.VatAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.NoAction);
            e.HasMany(x => x.Lines).WithOne(l => l.SalesOrder).HasForeignKey(l => l.SalesOrderId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SalesOrderLine>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.SalesOrderId });
            e.Property(x => x.Description).HasMaxLength(250);
            e.Property(x => x.UnitOfMeasure).HasMaxLength(10);
            e.Property(x => x.OrderedQuantity).HasColumnType("decimal(18,4)");
            e.Property(x => x.DeliveredQuantity).HasColumnType("decimal(18,4)");
            e.Property(x => x.InvoicedQuantity).HasColumnType("decimal(18,4)");
            e.Property(x => x.UnitPrice).HasColumnType("decimal(18,4)");
            e.Property(x => x.LineDiscountPercent).HasColumnType("decimal(5,2)");
            e.Property(x => x.VatPercent).HasColumnType("decimal(5,2)");
            e.Property(x => x.NetAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.VatAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.LineStatus).HasConversion<int>();
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasOne(x => x.Item).WithMany().HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<PurchaseOrder>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.Number }).IsUnique().HasFilter("[IsDeleted] = 0");
            e.Property(x => x.Number).HasMaxLength(30);
            e.Property(x => x.SupplierOrderReference).HasMaxLength(50);
            e.Property(x => x.Status).HasConversion<int>();
            e.Property(x => x.ExchangeRate).HasColumnType("decimal(18,6)");
            e.Property(x => x.HeaderDiscountPercent).HasColumnType("decimal(5,2)");
            e.Property(x => x.NetAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.VatAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.PurchaseRequest).WithMany().HasForeignKey(x => x.PurchaseRequestId).OnDelete(DeleteBehavior.NoAction);
            e.HasMany(x => x.Lines).WithOne(l => l.PurchaseOrder).HasForeignKey(l => l.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PurchaseOrderLine>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.PurchaseOrderId });
            e.Property(x => x.Description).HasMaxLength(250);
            e.Property(x => x.UnitOfMeasure).HasMaxLength(10);
            e.Property(x => x.OrderedQuantity).HasColumnType("decimal(18,4)");
            e.Property(x => x.ReceivedQuantity).HasColumnType("decimal(18,4)");
            e.Property(x => x.InvoicedQuantity).HasColumnType("decimal(18,4)");
            e.Property(x => x.UnitPrice).HasColumnType("decimal(18,4)");
            e.Property(x => x.LineDiscountPercent).HasColumnType("decimal(5,2)");
            e.Property(x => x.VatPercent).HasColumnType("decimal(5,2)");
            e.Property(x => x.NetAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.VatAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.LineStatus).HasConversion<int>();
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasOne(x => x.Item).WithMany().HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.NoAction);
        });
    }
}
