using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace KBM.Client.Services;

public sealed class ApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly HttpClient _http;

    /// <summary>Azienda della sessione corrente (intestazione report/stampe).</summary>
    public string? CompanyName { get; }
    public string? OperatorName { get; }

    public ApiClient(LoginSession session, string baseUrl = "http://localhost:5262")
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/") };
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        CompanyName = session.CompanyName;
        OperatorName = session.DisplayName;
    }

    public async Task<IReadOnlyList<UserListItem>?> GetUsersAsync(string? search = null)
    {
        var url = string.IsNullOrWhiteSpace(search)
            ? "api/users?page=1&pageSize=50"
            : $"api/users?search={Uri.EscapeDataString(search)}&page=1&pageSize=50";
        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        var page = await response.Content.ReadFromJsonAsync<PagedUsers>(JsonOptions);
        return page?.Items;
    }

    public async Task<UserDetailDto?> GetUserAsync(long id)
    {
        var response = await _http.GetAsync($"api/users/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<UserDetailDto>(JsonOptions);
    }

    public async Task<ApiWriteResult> CreateUserAsync(CreateUserDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/users", dto);
        if (response.IsSuccessStatusCode)
        {
            var created = await response.Content.ReadFromJsonAsync<UserDetailDto>(JsonOptions);
            return new ApiWriteResult(true, null, created?.Id);
        }
        return new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<ApiWriteResult> UpdateUserAsync(long id, UpdateUserDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/users/{id}", dto);
        return response.IsSuccessStatusCode
            ? new ApiWriteResult(true, null, id)
            : new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<ApiWriteResult> SetUserEnabledAsync(long id, bool enabled)
    {
        var response = await _http.PostAsync($"api/users/{id}/{(enabled ? "enable" : "disable")}", null);
        return response.IsSuccessStatusCode
            ? new ApiWriteResult(true, null, id)
            : new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var err = await response.Content.ReadFromJsonAsync<ApiError>(JsonOptions);
            if (!string.IsNullOrWhiteSpace(err?.Message)) return err!.Message;
        }
        catch { /* corpo non JSON */ }
        return $"Errore {(int)response.StatusCode} ({response.ReasonPhrase}).";
    }

    public async Task<IReadOnlyList<CompanyListItem>?> GetCompaniesAsync()
    {
        var response = await _http.GetAsync("api/companies");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<CompanyListItem>>(JsonOptions);
    }

    public async Task<CompanyDetailDto?> GetCompanyAsync(long id)
    {
        var response = await _http.GetAsync($"api/companies/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CompanyDetailDto>(JsonOptions);
    }

    public async Task<ApiWriteResult> CreateCompanyAsync(CreateCompanyDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/companies", dto);
        if (response.IsSuccessStatusCode)
        {
            var created = await response.Content.ReadFromJsonAsync<CompanyDetailDto>(JsonOptions);
            return new ApiWriteResult(true, null, created?.Id);
        }
        return new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<ApiWriteResult> UpdateCompanyAsync(long id, UpdateCompanyDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/companies/{id}", dto);
        return response.IsSuccessStatusCode
            ? new ApiWriteResult(true, null, id)
            : new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<RoleDetailDto?> GetRoleAsync(long id)
    {
        var response = await _http.GetAsync($"api/roles/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<RoleDetailDto>(JsonOptions);
    }

    public async Task<ApiWriteResult> CreateRoleAsync(CreateRoleDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/roles", dto);
        if (response.IsSuccessStatusCode)
        {
            var created = await response.Content.ReadFromJsonAsync<RoleDetailDto>(JsonOptions);
            return new ApiWriteResult(true, null, created?.Id);
        }
        return new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<ApiWriteResult> UpdateRoleAsync(long id, UpdateRoleDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/roles/{id}", dto);
        return response.IsSuccessStatusCode
            ? new ApiWriteResult(true, null, id)
            : new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<IReadOnlyList<RoleListItem>?> GetRolesAsync()
    {
        var response = await _http.GetAsync("api/roles");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<RoleListItem>>(JsonOptions);
    }

    public async Task<IReadOnlyList<CustomerListItem>?> GetCustomersAsync(string? search = null)
    {
        var url = string.IsNullOrWhiteSpace(search)
            ? "api/customers"
            : $"api/customers?search={Uri.EscapeDataString(search)}";
        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<CustomerListItem>>(JsonOptions);
    }

    public async Task<CustomerDetailDto?> GetCustomerAsync(long id)
    {
        var response = await _http.GetAsync($"api/customers/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CustomerDetailDto>(JsonOptions);
    }

    public async Task<ApiWriteResult> CreateCustomerAsync(CreateCustomerDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/customers", dto);
        if (response.IsSuccessStatusCode)
        {
            var created = await response.Content.ReadFromJsonAsync<CustomerDetailDto>(JsonOptions);
            return new ApiWriteResult(true, null, created?.Id);
        }
        return new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<ApiWriteResult> UpdateCustomerAsync(long id, UpdateCustomerDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/customers/{id}", dto);
        return response.IsSuccessStatusCode
            ? new ApiWriteResult(true, null, id)
            : new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<bool> SetCustomerEnabledAsync(long id, bool enabled)
    {
        var response = await _http.PostAsync($"api/customers/{id}/{(enabled ? "enable" : "disable")}", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<CustomerAggregateDto?> GetCustomerFullAsync(long id)
    {
        var response = await _http.GetAsync($"api/customers/{id}/full");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CustomerAggregateDto>(JsonOptions);
    }

    public async Task<ApiWriteResult> CreateCustomerFullAsync(CreateCustomerAggregateDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/customers/full", dto);
        if (response.IsSuccessStatusCode)
        {
            var created = await response.Content.ReadFromJsonAsync<CustomerAggregateDto>(JsonOptions);
            return new ApiWriteResult(true, null, created?.Detail.Id);
        }
        return new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<ApiWriteResult> SaveCustomerFullAsync(long id, SaveCustomerAggregateDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/customers/{id}/full", dto);
        return response.IsSuccessStatusCode
            ? new ApiWriteResult(true, null, id)
            : new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    // ===================== Fornitori =====================
    public async Task<IReadOnlyList<SupplierListItem>?> GetSuppliersAsync(string? search = null)
    {
        var url = string.IsNullOrWhiteSpace(search) ? "api/suppliers" : $"api/suppliers?search={Uri.EscapeDataString(search)}";
        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<SupplierListItem>>(JsonOptions);
    }

    public async Task<ApiWriteResult> UpdateSupplierAsync(long id, UpdateSupplierDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/suppliers/{id}", dto);
        return response.IsSuccessStatusCode
            ? new ApiWriteResult(true, null, id)
            : new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<bool> SetSupplierEnabledAsync(long id, bool enabled)
    {
        var response = await _http.PostAsync($"api/suppliers/{id}/{(enabled ? "enable" : "disable")}", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<SupplierAggregateDto?> GetSupplierFullAsync(long id)
    {
        var response = await _http.GetAsync($"api/suppliers/{id}/full");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<SupplierAggregateDto>(JsonOptions);
    }

    public async Task<ApiWriteResult> CreateSupplierFullAsync(CreateSupplierAggregateDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/suppliers/full", dto);
        if (response.IsSuccessStatusCode)
        {
            var created = await response.Content.ReadFromJsonAsync<SupplierAggregateDto>(JsonOptions);
            return new ApiWriteResult(true, null, created?.Detail.Id);
        }
        return new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<ApiWriteResult> SaveSupplierFullAsync(long id, SaveSupplierAggregateDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/suppliers/{id}/full", dto);
        return response.IsSuccessStatusCode
            ? new ApiWriteResult(true, null, id)
            : new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    // ===================== Articoli =====================
    public async Task<IReadOnlyList<ItemCategoryListItem>?> GetItemCategoriesAsync()
    {
        var response = await _http.GetAsync("api/items/categories");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<ItemCategoryListItem>>(JsonOptions);
    }

    public async Task<ApiWriteResult> CreateItemCategoryAsync(CreateItemCategoryDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/items/categories", dto);
        if (response.IsSuccessStatusCode)
        {
            var created = await response.Content.ReadFromJsonAsync<ItemCategoryDetailDto>(JsonOptions);
            return new ApiWriteResult(true, null, created?.Id);
        }
        return new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<ApiWriteResult> UpdateItemCategoryAsync(long id, UpdateItemCategoryDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/items/categories/{id}", dto);
        return response.IsSuccessStatusCode
            ? new ApiWriteResult(true, null, id)
            : new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<IReadOnlyList<ItemListItem>?> GetItemsAsync(string? search = null)
    {
        var url = string.IsNullOrWhiteSpace(search) ? "api/items" : $"api/items?search={Uri.EscapeDataString(search)}";
        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<ItemListItem>>(JsonOptions);
    }

    public async Task<ItemDetailDto?> GetItemAsync(long id)
    {
        var response = await _http.GetAsync($"api/items/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ItemDetailDto>(JsonOptions);
    }

    public async Task<ApiWriteResult> CreateItemAsync(CreateItemDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/items", dto);
        if (response.IsSuccessStatusCode)
        {
            var created = await response.Content.ReadFromJsonAsync<ItemDetailDto>(JsonOptions);
            return new ApiWriteResult(true, null, created?.Id);
        }
        return new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<ApiWriteResult> UpdateItemAsync(long id, UpdateItemDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/items/{id}", dto);
        return response.IsSuccessStatusCode
            ? new ApiWriteResult(true, null, id)
            : new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<bool> SetItemEnabledAsync(long id, bool enabled)
    {
        var response = await _http.PostAsync($"api/items/{id}/{(enabled ? "enable" : "disable")}", null);
        return response.IsSuccessStatusCode;
    }

    // ===================== Tabelle base: Condizioni di pagamento =====================
    public async Task<IReadOnlyList<PaymentTermListItem>?> GetPaymentTermsAsync(string? search = null)
    {
        var url = string.IsNullOrWhiteSpace(search) ? "api/payment-terms" : $"api/payment-terms?search={Uri.EscapeDataString(search)}";
        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<PaymentTermListItem>>(JsonOptions);
    }

    public async Task<PaymentTermDetailDto?> GetPaymentTermAsync(long id)
    {
        var response = await _http.GetAsync($"api/payment-terms/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<PaymentTermDetailDto>(JsonOptions);
    }

    public async Task<ApiWriteResult> CreatePaymentTermAsync(CreatePaymentTermDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/payment-terms", dto);
        if (response.IsSuccessStatusCode)
        {
            var created = await response.Content.ReadFromJsonAsync<PaymentTermDetailDto>(JsonOptions);
            return new ApiWriteResult(true, null, created?.Id);
        }
        return new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<ApiWriteResult> UpdatePaymentTermAsync(long id, UpdatePaymentTermDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/payment-terms/{id}", dto);
        return response.IsSuccessStatusCode
            ? new ApiWriteResult(true, null, id)
            : new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<bool> SetPaymentTermEnabledAsync(long id, bool enabled)
    {
        var response = await _http.PostAsync($"api/payment-terms/{id}/{(enabled ? "enable" : "disable")}", null);
        return response.IsSuccessStatusCode;
    }

    // ----- Piano dei conti (mastri) -----
    public async Task<IReadOnlyList<ChartAccountListItem>?> GetChartAccountsAsync(string? search = null, bool postableOnly = false)
    {
        var url = "api/chart-accounts";
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");
        if (postableOnly) query.Add("postableOnly=true");
        if (query.Count > 0) url += "?" + string.Join("&", query);
        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<ChartAccountListItem>>(JsonOptions);
    }

    public async Task<ChartAccountDetailDto?> GetChartAccountAsync(long id)
    {
        var response = await _http.GetAsync($"api/chart-accounts/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ChartAccountDetailDto>(JsonOptions);
    }

    public async Task<ApiWriteResult> CreateChartAccountAsync(CreateChartAccountDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/chart-accounts", dto);
        if (response.IsSuccessStatusCode)
        {
            var created = await response.Content.ReadFromJsonAsync<ChartAccountDetailDto>(JsonOptions);
            return new ApiWriteResult(true, null, created?.Id);
        }
        return new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<ApiWriteResult> UpdateChartAccountAsync(long id, UpdateChartAccountDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/chart-accounts/{id}", dto);
        return response.IsSuccessStatusCode
            ? new ApiWriteResult(true, null, id)
            : new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<bool> SetChartAccountEnabledAsync(long id, bool enabled)
    {
        var response = await _http.PostAsync($"api/chart-accounts/{id}/{(enabled ? "enable" : "disable")}", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<int> SeedStandardChartAsync()
    {
        var response = await _http.PostAsync("api/chart-accounts/seed-standard", null);
        if (!response.IsSuccessStatusCode) return -1;
        var result = await response.Content.ReadFromJsonAsync<SeedResultDto>(JsonOptions);
        return result?.Created ?? 0;
    }

    // ===================== Workflow: modelli =====================
    public async Task<IReadOnlyList<WorkflowDefinitionListItem>?> GetWorkflowDefinitionsAsync(string? search = null)
    {
        var url = string.IsNullOrWhiteSpace(search) ? "api/workflow-definitions" : $"api/workflow-definitions?search={Uri.EscapeDataString(search)}";
        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<WorkflowDefinitionListItem>>(JsonOptions);
    }

    public async Task<WorkflowDefinitionDetailDto?> GetWorkflowDefinitionAsync(long id)
    {
        var response = await _http.GetAsync($"api/workflow-definitions/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<WorkflowDefinitionDetailDto>(JsonOptions);
    }

    public async Task<ApiWriteResult> CreateWorkflowDefinitionAsync(CreateWorkflowDefinitionDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/workflow-definitions", dto);
        if (response.IsSuccessStatusCode)
        {
            var created = await response.Content.ReadFromJsonAsync<WorkflowDefinitionDetailDto>(JsonOptions);
            return new ApiWriteResult(true, null, created?.Id);
        }
        return new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<ApiWriteResult> UpdateWorkflowDefinitionAsync(long id, UpdateWorkflowDefinitionDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/workflow-definitions/{id}", dto);
        return response.IsSuccessStatusCode
            ? new ApiWriteResult(true, null, id)
            : new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<bool> SetWorkflowDefinitionStatusAsync(long id, string status)
    {
        var response = await _http.PostAsync($"api/workflow-definitions/{id}/status/{status}", null);
        return response.IsSuccessStatusCode;
    }

    // ===================== Workflow: consolle / istanze / task =====================
    public async Task<IReadOnlyList<WorkflowConsoleItem>?> GetWorkflowConsoleAsync(string? processState, string? taskState, string? visibility)
    {
        var url = $"api/workflow/console?processState={processState}&taskState={taskState}&visibility={visibility}";
        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<WorkflowConsoleItem>>(JsonOptions);
    }

    public async Task<IReadOnlyList<WorkflowInstanceListItem>?> GetWorkflowInstancesAsync(string? search = null)
    {
        var url = string.IsNullOrWhiteSpace(search) ? "api/workflow/instances" : $"api/workflow/instances?search={Uri.EscapeDataString(search)}";
        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<WorkflowInstanceListItem>>(JsonOptions);
    }

    public async Task<WorkflowInstanceDetailDto?> GetWorkflowInstanceAsync(long id)
    {
        var response = await _http.GetAsync($"api/workflow/instances/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<WorkflowInstanceDetailDto>(JsonOptions);
    }

    public async Task<ApiWriteResult> StartWorkflowAsync(StartWorkflowDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/workflow/instances", dto);
        if (response.IsSuccessStatusCode)
        {
            var created = await response.Content.ReadFromJsonAsync<WorkflowInstanceDetailDto>(JsonOptions);
            return new ApiWriteResult(true, null, created?.Id);
        }
        return new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<ApiWriteResult> SetWorkflowInstanceStateAsync(long id, string action, string? notes)
    {
        var response = await _http.PostAsJsonAsync($"api/workflow/instances/{id}/state/{action}", new TaskNoteDto(notes ?? ""));
        return response.IsSuccessStatusCode
            ? new ApiWriteResult(true, null, id)
            : new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<ApiWriteResult> CompleteWorkflowTaskAsync(long id, CompleteTaskDto dto)
    {
        var response = await _http.PostAsJsonAsync($"api/workflow/tasks/{id}/complete", dto);
        return response.IsSuccessStatusCode
            ? new ApiWriteResult(true, null, id)
            : new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<ApiWriteResult> RejectWorkflowTaskAsync(long id, string? notes)
    {
        var response = await _http.PostAsJsonAsync($"api/workflow/tasks/{id}/reject", new TaskNoteDto(notes ?? ""));
        return response.IsSuccessStatusCode
            ? new ApiWriteResult(true, null, id)
            : new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<bool> TakeWorkflowTaskAsync(long id)
    {
        var response = await _http.PostAsync($"api/workflow/tasks/{id}/take", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ReleaseWorkflowTaskAsync(long id)
    {
        var response = await _http.PostAsync($"api/workflow/tasks/{id}/release", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<ApiWriteResult> NoteWorkflowTaskAsync(long id, string notes)
    {
        var response = await _http.PostAsJsonAsync($"api/workflow/tasks/{id}/note", new TaskNoteDto(notes));
        return response.IsSuccessStatusCode
            ? new ApiWriteResult(true, null, id)
            : new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    // ===================== Richieste di acquisto (RDA) =====================
    public async Task<IReadOnlyList<PurchaseRequestListItem>?> GetPurchaseRequestsAsync(string? search = null)
    {
        var url = string.IsNullOrWhiteSpace(search) ? "api/purchase-requests" : $"api/purchase-requests?search={Uri.EscapeDataString(search)}";
        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<PurchaseRequestListItem>>(JsonOptions);
    }

    public async Task<PurchaseRequestDetailDto?> GetPurchaseRequestAsync(long id)
    {
        var response = await _http.GetAsync($"api/purchase-requests/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<PurchaseRequestDetailDto>(JsonOptions);
    }

    public async Task<ApiWriteResult> CreatePurchaseRequestAsync(CreatePurchaseRequestDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/purchase-requests", dto);
        if (!response.IsSuccessStatusCode) return new ApiWriteResult(false, await ReadErrorAsync(response), null);
        var detail = await response.Content.ReadFromJsonAsync<PurchaseRequestDetailDto>(JsonOptions);
        return new ApiWriteResult(true, null, detail?.Id);
    }

    public async Task<ApiWriteResult> SavePurchaseRequestAsync(long id, SavePurchaseRequestDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/purchase-requests/{id}", dto);
        return response.IsSuccessStatusCode
            ? new ApiWriteResult(true, null, id)
            : new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<bool> DeletePurchaseRequestAsync(long id) =>
        (await _http.DeleteAsync($"api/purchase-requests/{id}")).IsSuccessStatusCode;

    // ===================== Ordini cliente (OV) / fornitore (ODA) =====================
    public async Task<IReadOnlyList<SalesOrderListItem>?> GetSalesOrdersAsync(string? search = null)
    {
        var url = string.IsNullOrWhiteSpace(search) ? "api/sales-orders" : $"api/sales-orders?search={Uri.EscapeDataString(search)}";
        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<SalesOrderListItem>>(JsonOptions);
    }

    public async Task<SalesOrderDetailDto?> GetSalesOrderAsync(long id)
    {
        var response = await _http.GetAsync($"api/sales-orders/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<SalesOrderDetailDto>(JsonOptions);
    }

    public async Task<IReadOnlyList<PurchaseOrderListItem>?> GetPurchaseOrdersAsync(string? search = null)
    {
        var url = string.IsNullOrWhiteSpace(search) ? "api/purchase-orders" : $"api/purchase-orders?search={Uri.EscapeDataString(search)}";
        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<PurchaseOrderListItem>>(JsonOptions);
    }

    public async Task<PurchaseOrderDetailDto?> GetPurchaseOrderAsync(long id)
    {
        var response = await _http.GetAsync($"api/purchase-orders/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<PurchaseOrderDetailDto>(JsonOptions);
    }

    public async Task<ApiWriteResult> CreatePurchaseOrderFromRdaAsync(long purchaseRequestId)
    {
        var response = await _http.PostAsync($"api/purchase-orders/from-purchase-request/{purchaseRequestId}", null);
        if (!response.IsSuccessStatusCode) return new ApiWriteResult(false, await ReadErrorAsync(response), null);
        var detail = await response.Content.ReadFromJsonAsync<PurchaseOrderDetailDto>(JsonOptions);
        return new ApiWriteResult(true, null, detail?.Id);
    }

    public async Task<int> SeedOrderLookupsAsync()
    {
        var response = await _http.PostAsync("api/order-lookups/seed-standard", null);
        if (!response.IsSuccessStatusCode) return -1;
        var r = await response.Content.ReadFromJsonAsync<SeedResultDto>(JsonOptions);
        return r?.Created ?? 0;
    }

    // ===================== Richieste di offerta (RDO) =====================
    public async Task<IReadOnlyList<RfqListItem>?> GetRfqsAsync(string? search = null)
    {
        var url = string.IsNullOrWhiteSpace(search) ? "api/rfqs" : $"api/rfqs?search={Uri.EscapeDataString(search)}";
        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<RfqListItem>>(JsonOptions);
    }

    public async Task<RfqDetailDto?> GetRfqAsync(long id)
    {
        var response = await _http.GetAsync($"api/rfqs/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<RfqDetailDto>(JsonOptions);
    }

    public async Task<ApiWriteResult> CreateRfqAsync(CreateRfqDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/rfqs", dto);
        if (!response.IsSuccessStatusCode) return new ApiWriteResult(false, await ReadErrorAsync(response), null);
        var detail = await response.Content.ReadFromJsonAsync<RfqDetailDto>(JsonOptions);
        return new ApiWriteResult(true, null, detail?.Id);
    }

    public async Task<ApiWriteResult> CreateRfqFromPurchaseRequestAsync(CreateRfqFromRequestDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/rfqs/from-purchase-request", dto);
        if (!response.IsSuccessStatusCode) return new ApiWriteResult(false, await ReadErrorAsync(response), null);
        var detail = await response.Content.ReadFromJsonAsync<RfqDetailDto>(JsonOptions);
        return new ApiWriteResult(true, null, detail?.Id);
    }

    public async Task<ApiWriteResult> SaveRfqAsync(long id, SaveRfqDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/rfqs/{id}", dto);
        return response.IsSuccessStatusCode
            ? new ApiWriteResult(true, null, id)
            : new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<bool> DeleteRfqAsync(long id) =>
        (await _http.DeleteAsync($"api/rfqs/{id}")).IsSuccessStatusCode;

    // ===================== Modalita sviluppatore / config =====================
    public async Task<bool> CanAccessDeveloperAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/system-config/can-access");
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<IReadOnlyList<SystemSettingDto>?> GetSettingsAsync(long? companyId, string? category = null)
    {
        var qs = new List<string>();
        if (companyId.HasValue) qs.Add($"companyId={companyId.Value}");
        if (!string.IsNullOrWhiteSpace(category)) qs.Add($"category={Uri.EscapeDataString(category)}");
        var url = "api/system-config/settings" + (qs.Count > 0 ? "?" + string.Join("&", qs) : "");
        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<SystemSettingDto>>(JsonOptions);
    }

    public async Task<ApiWriteResult> UpsertSettingAsync(UpsertSettingDto dto)
    {
        var response = await _http.PutAsJsonAsync("api/system-config/settings", dto);
        return response.IsSuccessStatusCode
            ? new ApiWriteResult(true, null, null)
            : new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    public async Task<bool> DeleteSettingAsync(long id)
    {
        var response = await _http.DeleteAsync($"api/system-config/settings/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<IReadOnlyList<ReportDefinitionDto>?> GetReportDefinitionsAsync(long? companyId = null)
    {
        var url = "api/system-config/reports" + (companyId.HasValue ? $"?companyId={companyId.Value}" : "");
        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<ReportDefinitionDto>>(JsonOptions);
    }

    public async Task<ApiWriteResult> UpsertReportDefinitionAsync(UpsertReportDefinitionDto dto)
    {
        var response = await _http.PutAsJsonAsync("api/system-config/reports", dto);
        return response.IsSuccessStatusCode
            ? new ApiWriteResult(true, null, null)
            : new ApiWriteResult(false, await ReadErrorAsync(response), null);
    }

    // ===================== Reportistica =====================
    public async Task<ReportRenderOutcome> RenderReportAsync(string key, ReportDocumentDto model)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"api/reports/{key}", model);
            if (!response.IsSuccessStatusCode)
                return new ReportRenderOutcome(false, null, null, await ReadErrorAsync(response));
            var bytes = await response.Content.ReadAsByteArrayAsync();
            var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                           ?? response.Content.Headers.ContentDisposition?.FileName
                           ?? $"{key}.pdf";
            return new ReportRenderOutcome(true, bytes, fileName.Trim('"'), null);
        }
        catch { return new ReportRenderOutcome(false, null, null, "Errore di connessione API."); }
    }

    public async Task<LoginSession?> SwitchCompanyAsync(string refreshToken, long companyId)
    {
        var response = await _http.PostAsJsonAsync("api/auth/refresh", new { refreshToken, companyId });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<LoginSession>(JsonOptions);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        await _http.PostAsJsonAsync("api/auth/logout", new { refreshToken });
    }
}

public record UserListItem(long Id, string Username, string Email, string FirstName, string LastName, string Status, IReadOnlyList<string>? Roles);
public record UserDetailDto(long Id, string Username, string Email, string FirstName, string LastName, string Status, DateTime? LastLoginAt, IReadOnlyList<long>? CompanyIds, IReadOnlyList<string>? RoleCodes);
public record CreateUserDto(string Username, string Email, string Password, string FirstName, string LastName, IReadOnlyList<long> CompanyIds, IReadOnlyList<string> RoleCodes);
public record UpdateUserDto(string Email, string FirstName, string LastName, string Status, IReadOnlyList<long> CompanyIds, IReadOnlyList<string> RoleCodes);
public record ApiError(string Code, string Message);
public record ApiWriteResult(bool Ok, string? Error, long? Id);
public record PagedUsers(IReadOnlyList<UserListItem> Items, int Total, int Page, int PageSize);
public record CompanyListItem(long Id, string Code, string BusinessName, string Status);
public record CompanyDetailDto(long Id, string Code, string BusinessName, string? LegalName, string? VatNumber, string Status);
public record CreateCompanyDto(string Code, string BusinessName, string? LegalName, string? VatNumber);
public record UpdateCompanyDto(string BusinessName, string? LegalName, string? VatNumber, string Status);
public record RoleListItem(long Id, string Code, string Name, bool IsSystem, IReadOnlyList<string>? Permissions);
public record RoleDetailDto(long Id, string Code, string Name, string? Description, bool IsSystem, IReadOnlyList<string>? Permissions);
public record CreateRoleDto(string Code, string Name, string? Description, IReadOnlyList<string> PermissionCodes);
public record UpdateRoleDto(string Name, string? Description, IReadOnlyList<string> PermissionCodes);
public record CustomerListItem(long Id, string Code, string BusinessName, string? VatNumber, string? City, string Status);
public record CustomerDetailDto(long Id, string Code, string BusinessName, string? VatNumber, string? FiscalCode, string? SdiCode, string? PecEmail, string? Email, string? Phone, string? Address, string? City, string? Province, string? PostalCode, string Country, string? Iban, string? PaymentTerms, string? Notes, string Status);
public record CreateCustomerDto(string Code, string BusinessName, string? VatNumber, string? FiscalCode, string? SdiCode, string? PecEmail, string? Email, string? Phone, string? Address, string? City, string? Province, string? PostalCode, string? Country, string? Iban, string? PaymentTerms, string? Notes);
public record UpdateCustomerDto(string BusinessName, string? VatNumber, string? FiscalCode, string? SdiCode, string? PecEmail, string? Email, string? Phone, string? Address, string? City, string? Province, string? PostalCode, string? Country, string? Iban, string? PaymentTerms, string? Notes, string Status);

public record CustomerAddressDto(long Id, string AddressType, string Description, string? Address, string? City, string? Province, string? PostalCode, string Country, string? Phone, string? Email, bool IsDefault);
public record CustomerContactDto(long Id, string Name, string? Role, string? Email, string? Phone, string? Mobile, string? Notes, bool IsPrimary);
public record CustomerBankDto(long Id, string BankName, string? Iban, string? Swift, string? Abi, string? Cab, bool IsDefault);
public record CustomerAccountingDto(string? PaymentMethod, string? PriceListCode, string? AgentCode, string? Zone, decimal? CreditLimit, decimal? DiscountPercent, bool SplitPayment, bool WithholdingTax, string? VatExemptionCode, string? AccountCode);
public record CustomerAggregateDto(CustomerDetailDto Detail, CustomerAccountingDto Accounting, IReadOnlyList<CustomerAddressDto> Addresses, IReadOnlyList<CustomerContactDto> Contacts, IReadOnlyList<CustomerBankDto> Banks);
public record SaveCustomerAggregateDto(UpdateCustomerDto Core, CustomerAccountingDto Accounting, IReadOnlyList<CustomerAddressDto> Addresses, IReadOnlyList<CustomerContactDto> Contacts, IReadOnlyList<CustomerBankDto> Banks);
public record CreateCustomerAggregateDto(CreateCustomerDto Core, CustomerAccountingDto Accounting, IReadOnlyList<CustomerAddressDto> Addresses, IReadOnlyList<CustomerContactDto> Contacts, IReadOnlyList<CustomerBankDto> Banks);

public record SupplierListItem(long Id, string Code, string BusinessName, string? VatNumber, string? City, string Status);
public record SupplierDetailDto(long Id, string Code, string BusinessName, string? VatNumber, string? FiscalCode, string? SdiCode, string? PecEmail, string? Email, string? Phone, string? Address, string? City, string? Province, string? PostalCode, string Country, string? Iban, string? PaymentTerms, string? Notes, string Status);
public record CreateSupplierDto(string Code, string BusinessName, string? VatNumber, string? FiscalCode, string? SdiCode, string? PecEmail, string? Email, string? Phone, string? Address, string? City, string? Province, string? PostalCode, string? Country, string? Iban, string? PaymentTerms, string? Notes);
public record UpdateSupplierDto(string BusinessName, string? VatNumber, string? FiscalCode, string? SdiCode, string? PecEmail, string? Email, string? Phone, string? Address, string? City, string? Province, string? PostalCode, string? Country, string? Iban, string? PaymentTerms, string? Notes, string Status);
public record SupplierAddressDto(long Id, string AddressType, string Description, string? Address, string? City, string? Province, string? PostalCode, string Country, string? Phone, string? Email, bool IsDefault);
public record SupplierContactDto(long Id, string Name, string? Role, string? Email, string? Phone, string? Mobile, string? Notes, bool IsPrimary);
public record SupplierBankDto(long Id, string BankName, string? Iban, string? Swift, string? Abi, string? Cab, bool IsDefault);
public record SupplierAccountingDto(string? PaymentMethod, string? PriceListCode, string? Zone, decimal? CreditLimit, decimal? DiscountPercent, bool SplitPayment, bool WithholdingTax, string? VatExemptionCode, string? AccountCode);
public record SupplierAggregateDto(SupplierDetailDto Detail, SupplierAccountingDto Accounting, IReadOnlyList<SupplierAddressDto> Addresses, IReadOnlyList<SupplierContactDto> Contacts, IReadOnlyList<SupplierBankDto> Banks);
public record SaveSupplierAggregateDto(UpdateSupplierDto Core, SupplierAccountingDto Accounting, IReadOnlyList<SupplierAddressDto> Addresses, IReadOnlyList<SupplierContactDto> Contacts, IReadOnlyList<SupplierBankDto> Banks);
public record CreateSupplierAggregateDto(CreateSupplierDto Core, SupplierAccountingDto Accounting, IReadOnlyList<SupplierAddressDto> Addresses, IReadOnlyList<SupplierContactDto> Contacts, IReadOnlyList<SupplierBankDto> Banks);

public record ItemCategoryListItem(long Id, string Code, string Name, string Status);
public record ItemCategoryDetailDto(long Id, string Code, string Name, string? Description, string Status);
public record CreateItemCategoryDto(string Code, string Name, string? Description);
public record UpdateItemCategoryDto(string Name, string? Description, string Status);
public record ItemListItem(long Id, string Code, string Description, string? CategoryName, string UnitOfMeasure, decimal BasePrice, decimal VatRate, string Status);
public record ItemDetailDto(long Id, string Code, string Description, long? CategoryId, string UnitOfMeasure, string? Barcode, string? SupplierItemCode, decimal BasePrice, decimal VatRate, string? RevenueAccount, string? CostAccount, string? Notes, string Status);
public record CreateItemDto(string Code, string Description, long? CategoryId, string UnitOfMeasure, string? Barcode, string? SupplierItemCode, decimal BasePrice, decimal VatRate, string? RevenueAccount, string? CostAccount, string? Notes);
public record UpdateItemDto(string Description, long? CategoryId, string UnitOfMeasure, string? Barcode, string? SupplierItemCode, decimal BasePrice, decimal VatRate, string? RevenueAccount, string? CostAccount, string? Notes, string Status);

public record PaymentTermListItem(long Id, string Code, string Description, int InstallmentsCount, int FirstDueDays, int IntervalDays, bool EndOfMonth, string? PaymentMethod, string Status);
public record PaymentTermDetailDto(long Id, string Code, string Description, int InstallmentsCount, int FirstDueDays, int IntervalDays, bool EndOfMonth, string? PaymentMethod, string? Notes, string Status);
public record CreatePaymentTermDto(string Code, string Description, int InstallmentsCount, int FirstDueDays, int IntervalDays, bool EndOfMonth, string? PaymentMethod, string? Notes);
public record UpdatePaymentTermDto(string Description, int InstallmentsCount, int FirstDueDays, int IntervalDays, bool EndOfMonth, string? PaymentMethod, string? Notes, string Status);

public record ChartAccountListItem(long Id, long? ParentId, int Level, string Code, string FullCode, string Name, int Nature, int Sign, int SubKind, bool AllowsPosting, string Status);
public record ChartAccountDetailDto(long Id, long? ParentId, int Level, string Code, string FullCode, string Name, int Nature, int Sign, int SubKind, bool AllowsPosting, string? BilCeeDare, string? BilCeeAvere, string Status);
public record CreateChartAccountDto(long? ParentId, string Code, string Name, int? Nature, int Sign, int SubKind, string? BilCeeDare, string? BilCeeAvere);
public record UpdateChartAccountDto(string Name, int Sign, int SubKind, string? BilCeeDare, string? BilCeeAvere, string Status);
public record SeedResultDto(int Created);

public record WorkflowDefinitionListItem(long Id, string Code, string Name, string? TargetEntityType, string Status, int StepCount);
public record WorkflowStepDto(long Id, int StepOrder, string Code, string Name, int StepType, int AssigneeType, long? AssigneeUserId, long? AssigneeRoleId, int? DueDays, string? DecisionOptionsJson, string? ActionsJson);
public record WorkflowStepInputDto(long? Id, int StepOrder, string Code, string Name, int StepType, int AssigneeType, long? AssigneeUserId, long? AssigneeRoleId, int? DueDays, string? DecisionOptionsJson, string? ActionsJson);
public record WorkflowDefinitionDetailDto(long Id, string Code, string Name, string? Description, string? TargetEntityType, string Status, string? FieldsJson, List<WorkflowStepDto> Steps);
public record CreateWorkflowDefinitionDto(string Code, string Name, string? Description, string? TargetEntityType, string? FieldsJson, List<WorkflowStepInputDto> Steps);
public record UpdateWorkflowDefinitionDto(string Name, string? Description, string? TargetEntityType, string Status, string? FieldsJson, List<WorkflowStepInputDto> Steps);

public record WorkflowInstanceListItem(long Id, string Number, string Title, string DefinitionName, string State, string? CurrentStepName, DateTime StartedAt, string? LinkedEntityType, long? LinkedEntityId);
public record WorkflowTaskDto(long Id, int StepOrder, string StepName, string StepType, string State, string AssigneeType, long? AssigneeUserId, long? AssigneeRoleId, long? TakenByUserId, DateTime? DueDate, string? Decision, DateTime? CompletedAt);
public record WorkflowEventDto(long Id, string Action, long? ActorUserId, string? Notes, DateTime Timestamp);
public record WorkflowInstanceDetailDto(long Id, string Number, string Title, string DefinitionName, string State, string? LinkedEntityType, long? LinkedEntityId, int CurrentStepOrder, string? FieldValuesJson, long StartedBy, DateTime StartedAt, DateTime? CompletedAt, List<WorkflowTaskDto> Tasks, List<WorkflowEventDto> Events);
public record StartWorkflowDto(long WorkflowDefinitionId, string? Title, string? LinkedEntityType, long? LinkedEntityId, string? FieldValuesJson);
public record WorkflowConsoleItem(long TaskId, long InstanceId, string Number, string Title, string StepName, string StepType, string TaskState, string ProcessState, string AssigneeType, long? AssigneeRoleId, long? TakenByUserId, long? AssigneeUserId, DateTime? DueDate, string? DecisionOptionsJson, string? LinkedEntityType, long? LinkedEntityId);
public record CompleteTaskDto(string? Decision, string? Notes);
public record TaskNoteDto(string Notes);

// Ordini OV/ODA
public record SalesOrderListItem(long Id, string Number, DateTime OrderDate, string CustomerName, int LineCount, decimal TotalAmount, int Status);
public record PurchaseOrderListItem(long Id, string Number, DateTime OrderDate, string SupplierName, int LineCount, decimal TotalAmount, int Status);
public record OrderLineDto(long? Id, int LineNumber, long? ItemId, string? ItemCode, string Description, decimal OrderedQuantity, decimal FulfilledQuantity, decimal InvoicedQuantity, string? UnitOfMeasure, long? VatCodeId, decimal UnitPrice, decimal LineDiscountPercent, decimal VatPercent, decimal NetAmount, decimal VatAmount, decimal TotalAmount, int LineStatus);
public record SalesOrderDetailDto(long Id, string Number, DateTime OrderDate, long CustomerId, string CustomerName, string? CustomerOrderReference, long? BillingAddressId, long? ShippingAddressId, long? PriceListId, long? PaymentTermId, long? WarehouseId, long? CurrencyId, long? CarrierId, long? PortTypeId, long? DocumentTypeId, decimal ExchangeRate, decimal HeaderDiscountPercent, decimal NetAmount, decimal VatAmount, decimal TotalAmount, int Status, DateTime? ExpectedDeliveryDate, string? Notes, IReadOnlyList<OrderLineDto> Lines);
public record PurchaseOrderDetailDto(long Id, string Number, DateTime OrderDate, long SupplierId, string SupplierName, string? SupplierOrderReference, long? DeliveryAddressId, long? PaymentTermId, long? WarehouseId, long? CurrencyId, long? DocumentTypeId, long? PurchaseRequestId, decimal ExchangeRate, decimal HeaderDiscountPercent, decimal NetAmount, decimal VatAmount, decimal TotalAmount, int Status, DateTime? ExpectedDeliveryDate, string? Notes, IReadOnlyList<OrderLineDto> Lines);

// Acquisti - RDA
public record PurchaseRequestListItem(long Id, string Number, DateTime Date, string? RequestingUnit, int LineCount, string Status);
public record PurchaseRequestLineDto(long Id, long? ItemId, string? ItemCode, string Description, decimal Quantity, string? UnitOfMeasure, DateTime? RequiredDate, decimal? ProposedPrice, string LineStatus, IReadOnlyList<long> SupplierIds);
public record PurchaseRequestDetailDto(long Id, string Number, DateTime Date, string? RequestingUnit, string Status, string? Notes, IReadOnlyList<PurchaseRequestLineDto> Lines);
public record SavePurchaseRequestLineDto(long Id, long? ItemId, string Description, decimal Quantity, string? UnitOfMeasure, DateTime? RequiredDate, decimal? ProposedPrice, string LineStatus, IReadOnlyList<long> SupplierIds);
public record CreatePurchaseRequestDto(DateTime Date, string? RequestingUnit, string? Notes, IReadOnlyList<SavePurchaseRequestLineDto> Lines);
public record SavePurchaseRequestDto(DateTime Date, string? RequestingUnit, string Status, string? Notes, IReadOnlyList<SavePurchaseRequestLineDto> Lines);

// Acquisti - RDO
public record RfqListItem(long Id, string Number, DateTime Date, long SupplierId, string SupplierName, int LineCount, string Status);
public record RfqLineDto(long Id, long? ItemId, string? ItemCode, string Description, decimal Quantity, string? UnitOfMeasure, decimal? UnitPrice, decimal? DiscountPercent, bool Available, string? Notes);
public record RfqDetailDto(long Id, string Number, DateTime Date, long SupplierId, string SupplierName, long? PurchaseRequestId, string? PurchaseRequestNumber, DateTime? ResponseDueDate, string Status, string? Notes, IReadOnlyList<RfqLineDto> Lines);
public record SaveRfqLineDto(long Id, long? ItemId, string Description, decimal Quantity, string? UnitOfMeasure, decimal? UnitPrice, decimal? DiscountPercent, bool Available, string? Notes);
public record CreateRfqDto(DateTime Date, long SupplierId, long? PurchaseRequestId, DateTime? ResponseDueDate, string? Notes, IReadOnlyList<SaveRfqLineDto> Lines);
public record SaveRfqDto(DateTime Date, long SupplierId, DateTime? ResponseDueDate, string Status, string? Notes, IReadOnlyList<SaveRfqLineDto> Lines);
public record CreateRfqFromRequestDto(long PurchaseRequestId, long SupplierId, DateTime? ResponseDueDate);

// Config / modalita sviluppatore
public record SystemSettingDto(long Id, long? CompanyId, string Key, string Value, string Category, string? Description, DateTime UpdatedAt);
public record UpsertSettingDto(long? CompanyId, string Key, string Value, string Category, string? Description);
public record ReportDefinitionDto(long Id, long? CompanyId, string Key, string Title, int Engine, int OutputFormat, string? TemplatePathOrName, bool Enabled, DateTime UpdatedAt);
public record UpsertReportDefinitionDto(long Id, long? CompanyId, string Key, string Title, int Engine, int OutputFormat, string? TemplatePathOrName, bool Enabled);

// Reportistica
public record ReportFieldDto(string Label, string Value);
public record ReportDocumentDto(string Title, string? Subtitle, IReadOnlyList<ReportFieldDto> Header, IReadOnlyList<string> Columns, IReadOnlyList<IReadOnlyList<string>> Rows, IReadOnlyList<ReportFieldDto>? Totals, string? Footer, string? CompanyName);
public record ReportRenderOutcome(bool Ok, byte[]? Content, string? FileName, string? Error);
