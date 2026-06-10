using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using KBM.Client.Controls;
using KBM.Client.Services;
using KBM.Client.Views;

namespace KBM.Client;

public partial class ShellWindow : Window
{
    private LoginSession _session;
    private ApiClient _api;
    private readonly ObservableCollection<DocumentTab> _tabs = new();
    private readonly DispatcherTimer _searchTimer = new() { Interval = TimeSpan.FromMilliseconds(300) };

    public ShellWindow(LoginSession session)
    {
        InitializeComponent();
        ToastService.Initialize(ToastHost);
        _session = session;
        _api = new ApiClient(session);
        UpdateStatus();

        DocTabs.ItemsSource = _tabs;
        _tabs.CollectionChanged += (_, _) => UpdateEmptyState();
        DocTabs.SelectionChanged += DocTabs_SelectionChanged;
        UpdateEmptyState();

        _searchTimer.Tick += async (_, _) => { _searchTimer.Stop(); await RunGlobalSearchAsync(); };

        BuildNavTree();
        OpenTab("home");
    }

    private void UpdateStatus()
    {
        StatusText.Text = $"Operatore: {_session.DisplayName}   ·   Azienda: {_session.CompanyName}   ·   Ruoli: {string.Join(", ", _session.Roles ?? [])}";
        BrandUser.Text = _session.DisplayName;
        CompanyText.Text = _session.CompanyName;
    }

    // ===================== Gesture nascosta (modalita sviluppatore) =====================
    private int _logoClicks;
    private DateTime _lastLogoClick = DateTime.MinValue;

    private async void BrandLogo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var mods = Keyboard.Modifiers;
        if ((mods & ModifierKeys.Control) == 0 || (mods & ModifierKeys.Shift) == 0)
        {
            _logoClicks = 0;
            return;
        }
        e.Handled = true;

        var now = DateTime.UtcNow;
        if ((now - _lastLogoClick).TotalMilliseconds > 1200) _logoClicks = 0;
        _lastLogoClick = now;
        _logoClicks++;
        if (_logoClicks < 3) return;
        _logoClicks = 0;

        try
        {
            if (!await _api.CanAccessDeveloperAsync()) return; // nessun permesso: nessuna traccia visibile
            var win = new DeveloperSettingsWindow(_api, _session.CompanyId, _session.CompanyName) { Owner = this };
            win.ShowDialog();
        }
        catch { /* silenzioso */ }
    }

    private static string ModuleGlyph(string key) => NavigationRegistry.GlyphFor(key);

    // Mappature per la ricerca/filtro dell'albero
    private readonly Dictionary<TreeViewItem, NavFeature> _featureItems = new();
    private readonly Dictionary<TreeViewItem, NavGroup> _groupItems = new();

    private void BuildNavTree()
    {
        NavTree.Items.Clear();
        _featureItems.Clear();
        _groupItems.Clear();

        NavTree.Items.Add(BuildFeatureItem(NavigationRegistry.Home, 14, "#1C5D99"));
        NavTree.Items.Add(BuildFeatureItem(NavigationRegistry.Dashboard, 14, "#1C5D99"));

        foreach (var group in NavigationRegistry.Groups)
        {
            var groupItem = new TreeViewItem
            {
                Header = BuildHeader(group.Glyph, group.Label, null, 14, "#64748B", bold: true),
                IsExpanded = group.Expanded,
                Tag = NavigationRegistry.ModulePrefix + group.Key
            };
            _groupItems[groupItem] = group;
            foreach (var feature in group.Features)
                groupItem.Items.Add(BuildFeatureItem(feature, 13, feature.Enabled ? "#1C5D99" : "#9CA3AF"));
            NavTree.Items.Add(groupItem);
        }
    }

    private TreeViewItem BuildFeatureItem(NavFeature feature, double iconSize, string iconColor)
    {
        var item = new TreeViewItem
        {
            Tag = feature.Enabled ? feature.Key : null,
            IsEnabled = feature.Enabled,
            Header = BuildHeader(feature.Glyph, feature.Label, feature.Code, iconSize, iconColor, bold: false)
        };
        _featureItems[item] = feature;
        return item;
    }

    private static StackPanel BuildHeader(string glyph, string label, string? code, double iconSize, string iconColor, bool bold)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        panel.Children.Add(new TextBlock
        {
            Text = glyph,
            FontFamily = new System.Windows.Media.FontFamily("Segoe MDL2 Assets"),
            FontSize = iconSize,
            Foreground = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(iconColor)!,
            Margin = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Center
        });
        panel.Children.Add(new TextBlock
        {
            Text = label,
            FontWeight = bold ? FontWeights.SemiBold : FontWeights.Normal,
            VerticalAlignment = VerticalAlignment.Center
        });
        if (!string.IsNullOrWhiteSpace(code))
        {
            panel.Children.Add(new TextBlock
            {
                Text = $"({code})",
                FontSize = 10,
                Foreground = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#9CA3AF")!,
                Margin = new Thickness(6, 1, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            });
        }
        return panel;
    }

    // ===================== Ricerca modulo nell'albero (stile NTS) =====================
    private void NavSearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var query = NavSearchBox.Text?.Trim() ?? string.Empty;
        NavSearchClear.Visibility = query.Length == 0 ? Visibility.Collapsed : Visibility.Visible;
        ApplyNavFilter(query);
    }

    private void ApplyNavFilter(string query)
    {
        var empty = string.IsNullOrWhiteSpace(query);

        foreach (var obj in NavTree.Items)
        {
            if (obj is not TreeViewItem item) continue;

            if (_groupItems.TryGetValue(item, out var group))
            {
                var anyChildVisible = false;
                foreach (var childObj in item.Items)
                {
                    if (childObj is not TreeViewItem child || !_featureItems.TryGetValue(child, out var feature))
                        continue;
                    var match = empty || FeatureMatches(feature, query);
                    child.Visibility = match ? Visibility.Visible : Visibility.Collapsed;
                    anyChildVisible |= match;
                }

                var groupMatch = empty || group.Label.Contains(query, StringComparison.OrdinalIgnoreCase);
                item.Visibility = (anyChildVisible || groupMatch) ? Visibility.Visible : Visibility.Collapsed;
                item.IsExpanded = empty ? group.Expanded : anyChildVisible;
            }
            else if (_featureItems.TryGetValue(item, out var feature))
            {
                var match = empty || FeatureMatches(feature, query);
                item.Visibility = match ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }

    private static bool FeatureMatches(NavFeature feature, string query) =>
        feature.Label.Contains(query, StringComparison.OrdinalIgnoreCase)
        || (!string.IsNullOrEmpty(feature.Code) && feature.Code.Contains(query, StringComparison.OrdinalIgnoreCase));

    private void NavSearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            ClearNavSearch();
            e.Handled = true;
            return;
        }
        if (e.Key != Key.Enter) return;

        var query = NavSearchBox.Text?.Trim() ?? string.Empty;
        if (query.Length == 0) return;

        // 1) match esatto per codice mnemonico (es. ANACL)
        var target = NavigationRegistry.FindByCode(query);

        // 2) altrimenti, se il filtro individua un'unica funzione abilitata, aprila
        if (target is null)
        {
            var matches = NavigationRegistry.AllFeatures()
                .Where(f => f.Enabled && FeatureMatches(f, query))
                .ToList();
            if (matches.Count == 1) target = matches[0];
        }

        if (target is not null)
        {
            OpenTab(target.Key);
            ClearNavSearch();
            e.Handled = true;
        }
    }

    private void NavSearchClear_Click(object sender, RoutedEventArgs e) => ClearNavSearch();

    private void ClearNavSearch()
    {
        NavSearchBox.Text = string.Empty;
        ApplyNavFilter(string.Empty);
    }

    private void UpdateBreadcrumb(string key, string title)
    {
        Breadcrumb.Text = title;
        BreadcrumbIcon.Text = ModuleGlyph(key);
    }

    private void UpdateEmptyState()
    {
        EmptyState.Visibility = _tabs.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        DocTabs.Visibility = _tabs.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    private void NavTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is TreeViewItem { Tag: string key } && !string.IsNullOrEmpty(key))
            OpenTab(key);
    }

    private void OpenTab(string key)
    {
        NavState.TrackRecent(key);
        var existing = _tabs.FirstOrDefault(t => t.Key == key);
        if (existing is not null)
        {
            DocTabs.SelectedItem = existing;
            UpdateBreadcrumb(existing.Key, existing.Title);
            return;
        }

        var (title, content) = CreateContent(key);
        if (content is null) return;

        DocumentTab tab = null!;
        tab = new DocumentTab(key, title, content, new RelayCommand(() => CloseTab(tab)));
        _tabs.Add(tab);
        DocTabs.SelectedItem = tab;
        UpdateBreadcrumb(key, title);
    }

    private (string Title, UserControl? Content) CreateContent(string key)
    {
        if (key.StartsWith(NavigationRegistry.ModulePrefix))
        {
            var group = NavigationRegistry.FindGroup(key);
            return group is null ? ("", null) : (group.Label, new ModuleHomeView(group, OpenTab));
        }

        return key switch
        {
            "home" => ("Home", new LaunchpadView(_session.DisplayName, OpenTab)),
            "dashboard" => ("Dashboard", BuildDashboard()),
            "users" => ("Utenti", BuildUsers()),
            "companies" => ("Aziende", BuildCompanies()),
            "customers" => ("Clienti", BuildCustomers()),
            "suppliers" => ("Fornitori", BuildSuppliers()),
            "items" => ("Articoli", BuildItems()),
            "payment-terms" => ("Condizioni di pagamento", BuildPaymentTerms()),
            "chart-accounts" => ("Piano dei conti", BuildChartAccounts()),
            "wf-console" => ("Consolle workflow", BuildWorkflowConsole()),
            "wf-instances" => ("Processi", BuildWorkflowInstances()),
            "wf-definitions" => ("Modelli di workflow", BuildWorkflowDefinitions()),
            "sales-orders" => ("Ordini cliente", BuildSalesOrders()),
            "purchase-orders" => ("Ordini fornitore", BuildPurchaseOrders()),
            "purchase-requests" => ("Richieste di acquisto", BuildPurchaseRequests()),
            "rfqs" => ("Richieste di offerta", BuildRfqs()),
            "roles" => ("Ruoli", BuildRoles()),
            _ => ("", null)
        };
    }

    private UsersView BuildUsers()
    {
        var view = new UsersView(_api);
        view.RequestNewUser += () => OpenUserEditor(null);
        view.RequestEditUser += id => OpenUserEditor(id);
        return view;
    }

    private void OpenUserEditor(long? id)
    {
        var key = id.HasValue ? $"user-{id}" : "user-new";
        var existing = _tabs.FirstOrDefault(t => t.Key == key);
        if (existing is not null)
        {
            DocTabs.SelectedItem = existing;
            UpdateBreadcrumb("users", existing.Title);
            return;
        }

        var editor = new UserEditView(_api, id);
        var title = id.HasValue ? "Modifica utente" : "Nuovo utente";
        DocumentTab tab = null!;
        tab = new DocumentTab(key, title, editor, new RelayCommand(() => CloseTab(tab)));

        editor.Cancelled += () => CloseTab(tab);
        editor.Saved += () =>
        {
            CloseTab(tab);
            var usersTab = _tabs.FirstOrDefault(t => t.Key == "users");
            if (usersTab?.Content is UsersView uv)
            {
                DocTabs.SelectedItem = usersTab;
                UpdateBreadcrumb("users", usersTab.Title);
                _ = uv.ReloadAsync();
            }
            else
            {
                OpenTab("users");
            }
        };

        _tabs.Add(tab);
        DocTabs.SelectedItem = tab;
        UpdateBreadcrumb("users", title);
    }

    private CompaniesView BuildCompanies()
    {
        var view = new CompaniesView(_api);
        view.RequestNewCompany += () => OpenCompanyEditor(null);
        view.RequestEditCompany += id => OpenCompanyEditor(id);
        return view;
    }

    private void OpenCompanyEditor(long? id)
    {
        var key = id.HasValue ? $"company-{id}" : "company-new";
        var existing = _tabs.FirstOrDefault(t => t.Key == key);
        if (existing is not null)
        {
            DocTabs.SelectedItem = existing;
            UpdateBreadcrumb("companies", existing.Title);
            return;
        }

        var editor = new CompanyEditView(_api, id);
        var title = id.HasValue ? "Modifica azienda" : "Nuova azienda";
        DocumentTab tab = null!;
        tab = new DocumentTab(key, title, editor, new RelayCommand(() => CloseTab(tab)));

        editor.Cancelled += () => CloseTab(tab);
        editor.Saved += () =>
        {
            CloseTab(tab);
            var listTab = _tabs.FirstOrDefault(t => t.Key == "companies");
            if (listTab?.Content is CompaniesView cv)
            {
                DocTabs.SelectedItem = listTab;
                UpdateBreadcrumb("companies", listTab.Title);
                _ = cv.ReloadAsync();
            }
            else OpenTab("companies");
        };

        _tabs.Add(tab);
        DocTabs.SelectedItem = tab;
        UpdateBreadcrumb("companies", title);
    }

    private CustomersView BuildCustomers()
    {
        var view = new CustomersView(_api);
        view.RequestNewCustomer += () => OpenCustomerEditor(null);
        view.RequestEditCustomer += id => OpenCustomerEditor(id);
        return view;
    }

    private void OpenCustomerEditor(long? id)
    {
        var key = id.HasValue ? $"customer-{id}" : "customer-new";
        var existing = _tabs.FirstOrDefault(t => t.Key == key);
        if (existing is not null)
        {
            DocTabs.SelectedItem = existing;
            UpdateBreadcrumb("customers", existing.Title);
            return;
        }

        var editor = new CustomerEditView(_api, id);
        var title = id.HasValue ? "Modifica cliente" : "Nuovo cliente";
        DocumentTab tab = null!;
        tab = new DocumentTab(key, title, editor, new RelayCommand(() => CloseTab(tab)));

        editor.Cancelled += () => CloseTab(tab);
        editor.Saved += () =>
        {
            CloseTab(tab);
            var listTab = _tabs.FirstOrDefault(t => t.Key == "customers");
            if (listTab?.Content is CustomersView cv)
            {
                DocTabs.SelectedItem = listTab;
                UpdateBreadcrumb("customers", listTab.Title);
                _ = cv.ReloadAsync();
            }
            else OpenTab("customers");
        };

        _tabs.Add(tab);
        DocTabs.SelectedItem = tab;
        UpdateBreadcrumb("customers", title);
    }

    private SuppliersView BuildSuppliers()
    {
        var view = new SuppliersView(_api);
        view.RequestNewSupplier += () => OpenSupplierEditor(null);
        view.RequestEditSupplier += id => OpenSupplierEditor(id);
        return view;
    }

    private void OpenSupplierEditor(long? id)
    {
        var key = id.HasValue ? $"supplier-{id}" : "supplier-new";
        var existing = _tabs.FirstOrDefault(t => t.Key == key);
        if (existing is not null)
        {
            DocTabs.SelectedItem = existing;
            UpdateBreadcrumb("suppliers", existing.Title);
            return;
        }

        var editor = new SupplierEditView(_api, id);
        var title = id.HasValue ? "Modifica fornitore" : "Nuovo fornitore";
        DocumentTab tab = null!;
        tab = new DocumentTab(key, title, editor, new RelayCommand(() => CloseTab(tab)));

        editor.Cancelled += () => CloseTab(tab);
        editor.Saved += () =>
        {
            CloseTab(tab);
            var listTab = _tabs.FirstOrDefault(t => t.Key == "suppliers");
            if (listTab?.Content is SuppliersView sv)
            {
                DocTabs.SelectedItem = listTab;
                UpdateBreadcrumb("suppliers", listTab.Title);
                _ = sv.ReloadAsync();
            }
            else OpenTab("suppliers");
        };

        _tabs.Add(tab);
        DocTabs.SelectedItem = tab;
        UpdateBreadcrumb("suppliers", title);
    }

    private ItemsView BuildItems()
    {
        var view = new ItemsView(_api);
        view.RequestNewItem += () => OpenItemEditor(null);
        view.RequestEditItem += id => OpenItemEditor(id);
        return view;
    }

    private void OpenItemEditor(long? id)
    {
        var key = id.HasValue ? $"item-{id}" : "item-new";
        var existing = _tabs.FirstOrDefault(t => t.Key == key);
        if (existing is not null)
        {
            DocTabs.SelectedItem = existing;
            UpdateBreadcrumb("items", existing.Title);
            return;
        }

        var editor = new ItemEditView(_api, id);
        var title = id.HasValue ? "Modifica articolo" : "Nuovo articolo";
        DocumentTab tab = null!;
        tab = new DocumentTab(key, title, editor, new RelayCommand(() => CloseTab(tab)));

        editor.Cancelled += () => CloseTab(tab);
        editor.Saved += () =>
        {
            CloseTab(tab);
            var listTab = _tabs.FirstOrDefault(t => t.Key == "items");
            if (listTab?.Content is ItemsView iv)
            {
                DocTabs.SelectedItem = listTab;
                UpdateBreadcrumb("items", listTab.Title);
                _ = iv.ReloadAsync();
            }
            else OpenTab("items");
        };

        _tabs.Add(tab);
        DocTabs.SelectedItem = tab;
        UpdateBreadcrumb("items", title);
    }

    // ===================== Tabelle base: Condizioni di pagamento =====================
    private PaymentTermsView BuildPaymentTerms()
    {
        var view = new PaymentTermsView(_api);
        view.RequestNew += () => OpenPaymentTermEditor(null);
        view.RequestEdit += id => OpenPaymentTermEditor(id);
        return view;
    }

    private void OpenPaymentTermEditor(long? id)
    {
        var key = id.HasValue ? $"payterm-{id}" : "payterm-new";
        var existing = _tabs.FirstOrDefault(t => t.Key == key);
        if (existing is not null)
        {
            DocTabs.SelectedItem = existing;
            UpdateBreadcrumb("payment-terms", existing.Title);
            return;
        }

        var editor = new PaymentTermEditView(_api, id);
        var title = id.HasValue ? "Modifica condizione di pagamento" : "Nuova condizione di pagamento";
        DocumentTab tab = null!;
        tab = new DocumentTab(key, title, editor, new RelayCommand(() => CloseTab(tab)));

        editor.Cancelled += () => CloseTab(tab);
        editor.Saved += () =>
        {
            CloseTab(tab);
            var listTab = _tabs.FirstOrDefault(t => t.Key == "payment-terms");
            if (listTab?.Content is PaymentTermsView pv)
            {
                DocTabs.SelectedItem = listTab;
                UpdateBreadcrumb("payment-terms", listTab.Title);
                _ = pv.ReloadAsync();
            }
            else OpenTab("payment-terms");
        };

        _tabs.Add(tab);
        DocTabs.SelectedItem = tab;
        UpdateBreadcrumb("payment-terms", title);
    }

    // ===================== Contabilita: Piano dei conti (mastri) =====================
    private ChartAccountsView BuildChartAccounts()
    {
        var view = new ChartAccountsView(_api);
        view.RequestNew += () => OpenChartAccountEditor(null);
        view.RequestEdit += id => OpenChartAccountEditor(id);
        return view;
    }

    private void OpenChartAccountEditor(long? id)
    {
        var key = id.HasValue ? $"coa-{id}" : "coa-new";
        var existing = _tabs.FirstOrDefault(t => t.Key == key);
        if (existing is not null)
        {
            DocTabs.SelectedItem = existing;
            UpdateBreadcrumb("chart-accounts", existing.Title);
            return;
        }

        var editor = new ChartAccountEditView(_api, id);
        var title = id.HasValue ? "Modifica conto" : "Nuovo conto";
        DocumentTab tab = null!;
        tab = new DocumentTab(key, title, editor, new RelayCommand(() => CloseTab(tab)));

        editor.Cancelled += () => CloseTab(tab);
        editor.Saved += () =>
        {
            CloseTab(tab);
            var listTab = _tabs.FirstOrDefault(t => t.Key == "chart-accounts");
            if (listTab?.Content is ChartAccountsView cv)
            {
                DocTabs.SelectedItem = listTab;
                UpdateBreadcrumb("chart-accounts", listTab.Title);
                _ = cv.ReloadAsync();
            }
            else OpenTab("chart-accounts");
        };

        _tabs.Add(tab);
        DocTabs.SelectedItem = tab;
        UpdateBreadcrumb("chart-accounts", title);
    }

    // ===================== Workflow =====================
    private WorkflowConsoleView BuildWorkflowConsole()
    {
        var view = new WorkflowConsoleView(_api);
        view.RequestOpenInstance += id => OpenWorkflowInstance(id);
        return view;
    }

    private WorkflowInstancesView BuildWorkflowInstances()
    {
        return new WorkflowInstancesView(_api);
    }

    private WorkflowDefinitionsView BuildWorkflowDefinitions()
    {
        var view = new WorkflowDefinitionsView(_api);
        view.RequestNew += () => OpenWorkflowDefinitionEditor(null);
        view.RequestEdit += id => OpenWorkflowDefinitionEditor(id);
        view.RequestStart += async () =>
        {
            var dto = await WorkflowStartWindow.PromptAsync(_api, this);
            if (dto is null) return;
            var res = await _api.StartWorkflowAsync(dto);
            if (res.Ok) { ToastService.Success("Processo avviato."); OpenTab("wf-instances"); }
            else MessageBox.Show(res.Error ?? "Avvio non riuscito.", "KBM", MessageBoxButton.OK, MessageBoxImage.Warning);
        };
        return view;
    }

    private void OpenWorkflowInstance(long instanceId)
    {
        OpenTab("wf-instances");
        var tab = _tabs.FirstOrDefault(t => t.Key == "wf-instances");
        if (tab?.Content is WorkflowInstancesView wv) _ = wv.SelectInstanceAsync(instanceId);
    }

    private void OpenWorkflowDefinitionEditor(long? id)
    {
        var key = id.HasValue ? $"wfdef-{id}" : "wfdef-new";
        var existing = _tabs.FirstOrDefault(t => t.Key == key);
        if (existing is not null)
        {
            DocTabs.SelectedItem = existing;
            UpdateBreadcrumb("wf-definitions", existing.Title);
            return;
        }

        var editor = new WorkflowDefinitionEditView(_api, id);
        var title = id.HasValue ? "Modifica modello" : "Nuovo modello";
        DocumentTab tab = null!;
        tab = new DocumentTab(key, title, editor, new RelayCommand(() => CloseTab(tab)));

        editor.Cancelled += () => CloseTab(tab);
        editor.Saved += () =>
        {
            CloseTab(tab);
            var listTab = _tabs.FirstOrDefault(t => t.Key == "wf-definitions");
            if (listTab?.Content is WorkflowDefinitionsView dv)
            {
                DocTabs.SelectedItem = listTab;
                UpdateBreadcrumb("wf-definitions", listTab.Title);
                _ = dv.ReloadAsync();
            }
            else OpenTab("wf-definitions");
        };

        _tabs.Add(tab);
        DocTabs.SelectedItem = tab;
        UpdateBreadcrumb("wf-definitions", title);
    }

    // ===================== Acquisti: RDA =====================
    private SalesOrdersView BuildSalesOrders() => new(_api);

    private PurchaseOrdersView BuildPurchaseOrders() => new(_api);

    private PurchaseRequestsView BuildPurchaseRequests()
    {
        var view = new PurchaseRequestsView(_api);
        view.RequestNew += () => OpenPurchaseRequestEditor(null);
        view.RequestEdit += id => OpenPurchaseRequestEditor(id);
        view.RequestGenerateRfq += async id => await GenerateRfqFromRequestAsync(id);
        view.RequestGenerateOda += async id => await GenerateOdaFromRequestAsync(id);
        return view;
    }

    private void OpenPurchaseRequestEditor(long? id)
    {
        var key = id.HasValue ? $"rda-{id}" : "rda-new";
        var existing = _tabs.FirstOrDefault(t => t.Key == key);
        if (existing is not null)
        {
            DocTabs.SelectedItem = existing;
            UpdateBreadcrumb("purchase-requests", existing.Title);
            return;
        }

        var editor = new PurchaseRequestEditView(_api, id, _session.CompanyName);
        var title = id.HasValue ? "Modifica RDA" : "Nuova RDA";
        DocumentTab tab = null!;
        tab = new DocumentTab(key, title, editor, new RelayCommand(() => CloseTab(tab)));

        editor.Cancelled += () => CloseTab(tab);
        editor.Saved += () =>
        {
            CloseTab(tab);
            var listTab = _tabs.FirstOrDefault(t => t.Key == "purchase-requests");
            if (listTab?.Content is PurchaseRequestsView pv)
            {
                DocTabs.SelectedItem = listTab;
                UpdateBreadcrumb("purchase-requests", listTab.Title);
                _ = pv.ReloadAsync();
            }
            else OpenTab("purchase-requests");
        };

        _tabs.Add(tab);
        DocTabs.SelectedItem = tab;
        UpdateBreadcrumb("purchase-requests", title);
    }

    private async Task GenerateRfqFromRequestAsync(long purchaseRequestId)
    {
        var picker = new SupplierPickerWindow(_api) { Owner = this };
        if (picker.ShowDialog() != true || picker.SelectedSupplierId is not { } supplierId) return;

        var res = await _api.CreateRfqFromPurchaseRequestAsync(new CreateRfqFromRequestDto(purchaseRequestId, supplierId, null));
        if (!res.Ok)
        {
            MessageBox.Show(res.Error ?? "Generazione RDO non riuscita.", "KBM", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (res.Id is { } rfqId) OpenRfqEditor(rfqId);
    }

    private async Task GenerateOdaFromRequestAsync(long purchaseRequestId)
    {
        var res = await _api.CreatePurchaseOrderFromRdaAsync(purchaseRequestId);
        if (!res.Ok)
        {
            MessageBox.Show(res.Error ?? "Generazione ODA non riuscita.", "KBM", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        ToastService.Success("Ordine fornitore generato dalla RDA.");
        OpenTab("purchase-orders");
        if (_tabs.FirstOrDefault(t => t.Key == "purchase-orders")?.Content is PurchaseOrdersView pov)
            await pov.ReloadAsync();
    }

    // ===================== Acquisti: RDO =====================
    private RfqsView BuildRfqs()
    {
        var view = new RfqsView(_api);
        view.RequestNew += () => OpenRfqEditor(null);
        view.RequestEdit += id => OpenRfqEditor(id);
        return view;
    }

    private void OpenRfqEditor(long? id)
    {
        var key = id.HasValue ? $"rdo-{id}" : "rdo-new";
        var existing = _tabs.FirstOrDefault(t => t.Key == key);
        if (existing is not null)
        {
            DocTabs.SelectedItem = existing;
            UpdateBreadcrumb("rfqs", existing.Title);
            return;
        }

        var editor = new RfqEditView(_api, id, _session.CompanyName);
        var title = id.HasValue ? "Modifica RDO" : "Nuova RDO";
        DocumentTab tab = null!;
        tab = new DocumentTab(key, title, editor, new RelayCommand(() => CloseTab(tab)));

        editor.Cancelled += () => CloseTab(tab);
        editor.Saved += () =>
        {
            CloseTab(tab);
            var listTab = _tabs.FirstOrDefault(t => t.Key == "rfqs");
            if (listTab?.Content is RfqsView rv)
            {
                DocTabs.SelectedItem = listTab;
                UpdateBreadcrumb("rfqs", listTab.Title);
                _ = rv.ReloadAsync();
            }
            else OpenTab("rfqs");
        };

        _tabs.Add(tab);
        DocTabs.SelectedItem = tab;
        UpdateBreadcrumb("rfqs", title);
    }

    private RolesView BuildRoles()
    {
        var view = new RolesView(_api);
        view.RequestNewRole += () => OpenRoleEditor(null);
        view.RequestEditRole += id => OpenRoleEditor(id);
        return view;
    }

    private void OpenRoleEditor(long? id)
    {
        var key = id.HasValue ? $"role-{id}" : "role-new";
        var existing = _tabs.FirstOrDefault(t => t.Key == key);
        if (existing is not null)
        {
            DocTabs.SelectedItem = existing;
            UpdateBreadcrumb("roles", existing.Title);
            return;
        }

        var editor = new RoleEditView(_api, id);
        var title = id.HasValue ? "Modifica ruolo" : "Nuovo ruolo";
        DocumentTab tab = null!;
        tab = new DocumentTab(key, title, editor, new RelayCommand(() => CloseTab(tab)));

        editor.Cancelled += () => CloseTab(tab);
        editor.Saved += () =>
        {
            CloseTab(tab);
            var listTab = _tabs.FirstOrDefault(t => t.Key == "roles");
            if (listTab?.Content is RolesView rv)
            {
                DocTabs.SelectedItem = listTab;
                UpdateBreadcrumb("roles", listTab.Title);
                _ = rv.ReloadAsync();
            }
            else OpenTab("roles");
        };

        _tabs.Add(tab);
        DocTabs.SelectedItem = tab;
        UpdateBreadcrumb("roles", title);
    }

    private DashboardView BuildDashboard()
    {
        var view = new DashboardView(_session, _api);
        view.NavigateUsers += () => OpenTab("users");
        view.NavigateCompanies += () => OpenTab("companies");
        view.NavigateRoles += () => OpenTab("roles");
        return view;
    }

    private void CloseTab(DocumentTab tab) => _tabs.Remove(tab);

    /// <summary>Solo per preview/screenshot: apre un tab dato il key.</summary>
    public void PreviewOpen(string key) => OpenTab(key);

    private void DocTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!ReferenceEquals(e.OriginalSource, DocTabs)) return;
        if (DocTabs.SelectedItem is DocumentTab tab)
            UpdateBreadcrumb(tab.Key, tab.Title);
    }

    // ===================== Ricerca globale =====================
    private void GlobalSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        _searchTimer.Stop();
        _searchTimer.Start();
    }

    private async Task RunGlobalSearchAsync()
    {
        var term = GlobalSearch.Text?.Trim() ?? "";
        if (term.Length < 2)
        {
            SearchPopup.IsOpen = false;
            return;
        }

        var results = new List<SearchResult>();
        try
        {
            var users = await _api.GetUsersAsync(term);
            if (users is not null)
            {
                results.AddRange(users.Take(6).Select(u => new SearchResult(
                    "users", u.Id, "UTENTE",
                    $"{u.FirstName} {u.LastName}".Trim() is { Length: > 0 } n ? n : u.Username,
                    $"{u.Username} · {u.Email}")));
            }

            var companies = await _api.GetCompaniesAsync();
            if (companies is not null)
            {
                results.AddRange(companies
                    .Where(c => Contains(c.BusinessName, term) || Contains(c.Code, term))
                    .Take(6)
                    .Select(c => new SearchResult("companies", c.Id, "AZIENDA", c.BusinessName, $"Codice {c.Code}")));
            }

            var roles = await _api.GetRolesAsync();
            if (roles is not null)
            {
                results.AddRange(roles
                    .Where(r => Contains(r.Name, term) || Contains(r.Code, term))
                    .Take(6)
                    .Select(r => new SearchResult("roles", r.Id, "RUOLO", r.Name, r.Code)));
            }
        }
        catch { /* ricerca best-effort */ }

        SearchResults.ItemsSource = results;
        SearchEmpty.Visibility = results.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        SearchPopup.IsOpen = true;
    }

    private static bool Contains(string? source, string term) =>
        source?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false;

    private void SearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SearchResults.SelectedItem is not SearchResult r) return;
        SearchPopup.IsOpen = false;
        GlobalSearch.Text = "";
        SearchResults.SelectedItem = null;
        OpenTabAndSelect(r.Module, r.Id);
    }

    private void OpenTabAndSelect(string module, long id)
    {
        OpenTab(module);
        var tab = _tabs.FirstOrDefault(t => t.Key == module);
        switch (tab?.Content)
        {
            case UsersView uv: uv.TrySelect(id); break;
            case CompaniesView cv: cv.TrySelect(id); break;
        }
    }

    // ===================== Controlli finestra custom =====================
    private void MinBtn_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void MaxBtn_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var handle = new WindowInteropHelper(this).Handle;
        HwndSource.FromHwnd(handle)?.AddHook(WindowProc);
        UpdateMaxGlyph();
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        UpdateMaxGlyph();
    }

    private void UpdateMaxGlyph()
    {
        if (MaxBtn is not null)
            MaxBtn.Content = WindowState == WindowState.Maximized ? "\uE923" : "\uE922";
    }

    // Vincola la finestra borderless massimizzata all'area di lavoro (non copre la taskbar)
    private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WM_GETMINMAXINFO = 0x0024;
        if (msg == WM_GETMINMAXINFO)
        {
            var mmi = Marshal.PtrToStructure<MINMAXINFO>(lParam);
            const int MONITOR_DEFAULTTONEAREST = 0x00000002;
            var monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                var info = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
                GetMonitorInfo(monitor, ref info);
                var work = info.rcWork;
                var area = info.rcMonitor;
                mmi.ptMaxPosition.X = work.Left - area.Left;
                mmi.ptMaxPosition.Y = work.Top - area.Top;
                mmi.ptMaxSize.X = work.Right - work.Left;
                mmi.ptMaxSize.Y = work.Bottom - work.Top;
                mmi.ptMinTrackSize.X = 1120;
                mmi.ptMinTrackSize.Y = 660;
                Marshal.StructureToPtr(mmi, lParam, true);
            }
            handled = true;
        }
        return IntPtr.Zero;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

    [DllImport("user32.dll")]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left, Top, Right, Bottom; }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X, Y; }

    [StructLayout(LayoutKind.Sequential)]
    private struct MINMAXINFO
    {
        public POINT ptReserved, ptMaxSize, ptMaxPosition, ptMinTrackSize, ptMaxTrackSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor, rcWork;
        public uint dwFlags;
    }

    private async void Logout_Click(object sender, RoutedEventArgs e)
    {
        try { await _api.LogoutAsync(_session.RefreshToken); } catch { /* ignore */ }
        new LoginWindow().Show();
        Close();
    }
}

public sealed record SearchResult(string Module, long Id, string ModuleLabel, string Title, string Subtitle);
