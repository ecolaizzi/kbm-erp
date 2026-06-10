namespace KBM.Application.Security;

public static class PermissionCodes
{
    public const string UsersRead = "core.users.read";
    public const string UsersCreate = "core.users.create";
    public const string UsersEdit = "core.users.edit";
    public const string UsersDelete = "core.users.delete";

    public const string RolesRead = "core.roles.read";
    public const string RolesCreate = "core.roles.create";
    public const string RolesEdit = "core.roles.edit";
    public const string RolesDelete = "core.roles.delete";

    public const string CompaniesRead = "core.companies.read";
    public const string CompaniesCreate = "core.companies.create";
    public const string CompaniesEdit = "core.companies.edit";

    public const string AuditRead = "core.audit.read";

    public const string CustomersRead = "anagraphics.customers.read";
    public const string CustomersCreate = "anagraphics.customers.create";
    public const string CustomersEdit = "anagraphics.customers.edit";
    public const string CustomersDelete = "anagraphics.customers.delete";

    public const string SuppliersRead = "anagraphics.suppliers.read";
    public const string SuppliersCreate = "anagraphics.suppliers.create";
    public const string SuppliersEdit = "anagraphics.suppliers.edit";
    public const string SuppliersDelete = "anagraphics.suppliers.delete";

    public const string ItemsRead = "anagraphics.items.read";
    public const string ItemsCreate = "anagraphics.items.create";
    public const string ItemsEdit = "anagraphics.items.edit";
    public const string ItemsDelete = "anagraphics.items.delete";

    // Tabelle e Archivi di base (stile Business Cube subm0100)
    public const string PaymentTermsRead = "base.paymentterms.read";
    public const string PaymentTermsCreate = "base.paymentterms.create";
    public const string PaymentTermsEdit = "base.paymentterms.edit";
    public const string PaymentTermsDelete = "base.paymentterms.delete";

    // Modulo ordini: tabelle base + OV/ODA + listini (struttura ambsor02)
    public const string OrdersSetupRead = "orders.setup.read";
    public const string OrdersSetupManage = "orders.setup.manage";
    public const string SalesOrdersRead = "orders.sales.read";
    public const string SalesOrdersCreate = "orders.sales.create";
    public const string SalesOrdersEdit = "orders.sales.edit";
    public const string SalesOrdersDelete = "orders.sales.delete";
    public const string PurchaseOrdersRead = "orders.purchase.read";
    public const string PurchaseOrdersCreate = "orders.purchase.create";
    public const string PurchaseOrdersEdit = "orders.purchase.edit";
    public const string PurchaseOrdersDelete = "orders.purchase.delete";
    public const string PriceListsRead = "orders.pricelists.read";
    public const string PriceListsManage = "orders.pricelists.manage";

    // Contabilita: piano dei conti (sistema dei mastri)
    public const string ChartAccountsRead = "accounting.coa.read";
    public const string ChartAccountsManage = "accounting.coa.manage";

    // Modalita sviluppatore: configurazioni azienda/tecniche e report (gesture-gated)
    public const string DeveloperAccess = "system.developer.access";
    public const string ConfigRead = "system.config.read";
    public const string ConfigEdit = "system.config.edit";

    // Ciclo passivo: RDA (richieste di acquisto) e RDO (richieste di offerta)
    public const string PurchaseRequestsRead = "purchasing.rda.read";
    public const string PurchaseRequestsCreate = "purchasing.rda.create";
    public const string PurchaseRequestsEdit = "purchasing.rda.edit";
    public const string PurchaseRequestsDelete = "purchasing.rda.delete";

    public const string RfqRead = "purchasing.rdo.read";
    public const string RfqCreate = "purchasing.rdo.create";
    public const string RfqEdit = "purchasing.rdo.edit";
    public const string RfqDelete = "purchasing.rdo.delete";

    // Workflow: modelli di processo, esecuzione (consolle) e amministrazione
    public const string WorkflowRead = "workflow.read";
    public const string WorkflowParticipate = "workflow.participate";   // operare sui propri task in consolle
    public const string WorkflowStart = "workflow.start";               // avviare processi
    public const string WorkflowManage = "workflow.manage";             // creare/modificare modelli
    public const string WorkflowAdmin = "workflow.admin";               // annullare/sospendere processi, vedere tutto

    public static readonly string[] All =
    [
        UsersRead, UsersCreate, UsersEdit, UsersDelete,
        RolesRead, RolesCreate, RolesEdit, RolesDelete,
        CompaniesRead, CompaniesCreate, CompaniesEdit,
        AuditRead,
        CustomersRead, CustomersCreate, CustomersEdit, CustomersDelete,
        SuppliersRead, SuppliersCreate, SuppliersEdit, SuppliersDelete,
        ItemsRead, ItemsCreate, ItemsEdit, ItemsDelete,
        PaymentTermsRead, PaymentTermsCreate, PaymentTermsEdit, PaymentTermsDelete,
        ChartAccountsRead, ChartAccountsManage,
        OrdersSetupRead, OrdersSetupManage,
        SalesOrdersRead, SalesOrdersCreate, SalesOrdersEdit, SalesOrdersDelete,
        PurchaseOrdersRead, PurchaseOrdersCreate, PurchaseOrdersEdit, PurchaseOrdersDelete,
        PriceListsRead, PriceListsManage,
        DeveloperAccess, ConfigRead, ConfigEdit,
        PurchaseRequestsRead, PurchaseRequestsCreate, PurchaseRequestsEdit, PurchaseRequestsDelete,
        RfqRead, RfqCreate, RfqEdit, RfqDelete,
        WorkflowRead, WorkflowParticipate, WorkflowStart, WorkflowManage, WorkflowAdmin
    ];
}
