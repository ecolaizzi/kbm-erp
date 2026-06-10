using KBM.Application.Accounting;
using KBM.Application.Orders;
using KBM.Application.Audit;
using KBM.Application.Auth;
using KBM.Application.BaseTables;
using KBM.Application.Companies;
using KBM.Application.Configuration;
using KBM.Application.Customers;
using KBM.Application.Purchasing;
using KBM.Application.Reporting;
using KBM.Application.Items;
using KBM.Application.Roles;
using KBM.Application.Suppliers;
using KBM.Application.Security;
using KBM.Application.Setup;
using KBM.Application.Users;
using KBM.Application.Workflow;
using KBM.Infrastructure.Persistence.Accounting;
using KBM.Infrastructure.Persistence.Orders;
using KBM.Infrastructure.Persistence.Audit;
using KBM.Infrastructure.Persistence.Auth;
using KBM.Infrastructure.Persistence.BaseTables;
using KBM.Infrastructure.Persistence.Companies;
using KBM.Infrastructure.Persistence.Configuration;
using KBM.Infrastructure.Persistence.Customers;
using KBM.Infrastructure.Persistence.Purchasing;
using KBM.Infrastructure.Persistence.Items;
using KBM.Infrastructure.Persistence.Roles;
using KBM.Infrastructure.Persistence.Suppliers;
using KBM.Infrastructure.Persistence.Security;
using KBM.Infrastructure.Persistence.Seeding;
using KBM.Infrastructure.Persistence.Setup;
using KBM.Infrastructure.Persistence.Users;
using KBM.Infrastructure.Persistence.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KBM.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddDbContext<KbmDbContext>((sp, options) =>
            options.UseSqlServer(configuration.GetConnectionString("Default"))
                .AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>()));

        services.AddScoped<IPasswordHasher, Argon2idPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISetupService, SetupService>();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IItemService, ItemService>();
        services.AddScoped<IPaymentTermService, PaymentTermService>();
        services.AddScoped<IChartAccountService, ChartAccountService>();
        services.AddScoped<IVatCodeService, VatCodeService>();
        services.AddScoped<IOrderLookupService, OrderLookupService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<IPriceListService, PriceListService>();
        services.AddScoped<ISalesOrderService, SalesOrderService>();
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IAuditQueryService, AuditQueryService>();
        services.AddScoped<ISystemConfigService, SystemConfigService>();
        services.AddScoped<IReportDefinitionProvider, ReportDefinitionProvider>();
        services.AddScoped<IPurchaseRequestService, PurchaseRequestService>();
        services.AddScoped<IRfqService, RfqService>();
        services.AddScoped<IWorkflowDefinitionService, WorkflowDefinitionService>();
        services.AddScoped<IWorkflowEngineService, WorkflowEngineService>();
        services.AddScoped<IWorkflowConsoleService, WorkflowConsoleService>();
        services.AddScoped<RbacSeedService>();

        return services;
    }
}
