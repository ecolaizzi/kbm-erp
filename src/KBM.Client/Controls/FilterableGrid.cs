using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using KBM.Client.Services;

// GridCustomizeWindow vive nel namespace radice KBM.Client
using KBM.Client;

namespace KBM.Client.Controls;

/// <summary>
/// Griglia enterprise stile NTS Business Cube: densa, ordinabile (click header),
/// filtrabile per colonna (casella filtro nell'intestazione) ed editabile in-line
/// con validazione in tempo reale (gli item devono implementare INotifyPropertyChanged
/// e, per la validazione, IDataErrorInfo).
/// </summary>
public sealed class FilterableGrid : UserControl
{
    private readonly DataGrid _grid;
    private readonly Dictionary<string, string> _filters = new();
    private readonly Dictionary<string, PropertyInfo?> _accessors = new();
    private readonly Dictionary<string, TextBox> _filterBoxes = new();
    private readonly List<ColumnMeta> _columns = new();
    private ListCollectionView? _view;
    private bool _filterBoxClicked;
    private string? _personalizationId;
    private GridLayoutStore? _store;

    private Border _finderPanel = null!;
    private TextBox _finderSearch = null!;
    private ListBox _finderList = null!;

    /// <summary>Nome usato come titolo/foglio nelle esportazioni.</summary>
    public string ExportName { get; set; } = "Griglia";

    private sealed record FinderItem(string Title, DataGridColumn Column)
    {
        public override string ToString() => Title;
    }

    /// <summary>Sollevato quando l'insieme dei layout (o quello attivo) cambia.</summary>
    public event Action? LayoutsChanged;

    private sealed record ColumnMeta(string Key, string Title, DataGridColumn Column, int DefaultIndex, DataGridLength DefaultWidth);

    /// <summary>Sollevato quando una riga viene confermata in edit in-line (commit).</summary>
    public event Action<object>? RowCommitted;
    public event Action<object?>? SelectionChanged;
    public event Action<object>? RowActivated;

    public FilterableGrid()
    {
        _grid = new DataGrid
        {
            Style = (Style)Application.Current.FindResource("KbmDataGrid"),
            AutoGenerateColumns = false,
            CanUserAddRows = false,
            CanUserDeleteRows = false,
            CanUserSortColumns = true,
            CanUserResizeColumns = true,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            ColumnHeaderHeight = 46,
            SelectionMode = DataGridSelectionMode.Single,
            SelectionUnit = DataGridSelectionUnit.FullRow
        };
        _grid.Sorting += OnSorting;
        _grid.RowEditEnding += OnRowEditEnding;
        _grid.SelectionChanged += (_, _) => SelectionChanged?.Invoke(_grid.SelectedItem);
        _grid.MouseDoubleClick += (_, _) =>
        {
            if (_grid.SelectedItem is { } item) RowActivated?.Invoke(item);
        };

        var root = new Grid();
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        System.Windows.Controls.Grid.SetColumn(_grid, 0);
        root.Children.Add(_grid);
        BuildFinderPanel();
        System.Windows.Controls.Grid.SetColumn(_finderPanel, 1);
        root.Children.Add(_finderPanel);
        Content = root;

        // Trova colonna (Ctrl+\) — stile Business Cube
        InputBindings.Add(new KeyBinding(new RelayCommand(() => ToggleFinder()), Key.Oem5, ModifierKeys.Control));
    }

    public DataGrid Grid => _grid;
    public object? SelectedItem => _grid.SelectedItem;

    /// <summary>True se la riga è in editing (in-line); blocca editing se la colonna non è editabile.</summary>
    public bool ReadOnly
    {
        get => _grid.IsReadOnly;
        set => _grid.IsReadOnly = value;
    }

    public void SetItems(IEnumerable items)
    {
        // Se è già una IList (es. ObservableCollection) la usiamo live, così add/remove si riflettono.
        var list = items as IList ?? items.Cast<object>().ToList();
        _view = new ListCollectionView(list) { Filter = FilterPredicate };
        _grid.ItemsSource = _view;
    }

    public void Refresh() => _view?.Refresh();
    public int Count => _view?.Count ?? 0;

    public void SelectItem(object? item)
    {
        if (item is null) return;
        _grid.SelectedItem = item;
        _grid.ScrollIntoView(item);
    }

    // ===================== Navigazione record (stile Business Cube: 1/196) =====================
    /// <summary>Indice 0-based della riga selezionata nella vista filtrata (-1 se nessuna).</summary>
    public int CurrentIndex => _view is null || _grid.SelectedItem is null ? -1 : _view.IndexOf(_grid.SelectedItem);

    public void MoveTo(int index)
    {
        if (_view is null || _view.Count == 0) return;
        index = Math.Clamp(index, 0, _view.Count - 1);
        SelectItem(_view.GetItemAt(index));
    }

    public void MoveFirst() => MoveTo(0);
    public void MoveLast() => MoveTo(Count - 1);
    public void MoveNext() => MoveTo(CurrentIndex < 0 ? 0 : CurrentIndex + 1);
    public void MovePrevious() => MoveTo(CurrentIndex < 0 ? 0 : CurrentIndex - 1);

    // ===================== Personalizzazione griglia (stile NTS) =====================
    public bool CanPersonalize => _personalizationId is not null;
    public IReadOnlyList<string> LayoutNames => _store?.Layouts.Select(l => l.Name).ToList() ?? new List<string>();
    public string ActiveLayout => _store?.ActiveLayout ?? "Standard";
    public IReadOnlyList<GridColumnInfo> ColumnCatalog => _columns.Select(m => new GridColumnInfo(m.Key, m.Title)).ToList();

    /// <summary>Abilita la personalizzazione (colonne visibili/larghezza/ordine, layout multipli) persistita per id.</summary>
    public void EnablePersonalization(string id)
    {
        _personalizationId = id;
        _store = LoadStore();
        ApplyActiveLayout();
    }

    public void SetActiveLayout(string name)
    {
        if (_store is null || !_store.Layouts.Any(l => l.Name == name)) return;
        _store.ActiveLayout = name;
        UiSettings.Save($"grid-{_personalizationId}", _store);
        ApplyActiveLayout();
    }

    /// <summary>Apre il dialog "Personalizzazione griglie" per gestire colonne e layout.</summary>
    public void OpenPersonalization(Window? owner)
    {
        if (_store is null || _personalizationId is null) return;
        var dlg = new GridCustomizeWindow(Clone(_store), ColumnCatalog) { Owner = owner };
        if (dlg.ShowDialog() != true || dlg.ResultStore is null) return;
        _store = dlg.ResultStore;
        UiSettings.Save($"grid-{_personalizationId}", _store);
        ApplyActiveLayout();
        LayoutsChanged?.Invoke();
        ToastService.Success($"Layout \u201c{_store.ActiveLayout}\u201d applicato.", "Personalizzazione griglie");
    }

    private void ApplyActiveLayout()
    {
        if (_store is null) return;
        var active = _store.Layouts.FirstOrDefault(l => l.Name == _store.ActiveLayout) ?? _store.Layouts.FirstOrDefault();
        if (active is not null) ApplyStates(active.Columns);
    }

    private void ApplyStates(IList<GridColumnState> states)
    {
        var map = states.ToDictionary(s => s.Key, s => s);
        foreach (var m in _columns)
        {
            if (map.TryGetValue(m.Key, out var st))
            {
                m.Column.Visibility = st.Visible ? Visibility.Visible : Visibility.Collapsed;
                m.Column.Width = st.Width > 0 ? new DataGridLength(st.Width) : m.DefaultWidth;
            }
            else
            {
                m.Column.Visibility = Visibility.Visible;
                m.Column.Width = m.DefaultWidth;
            }
        }

        var ordered = _columns
            .OrderBy(m => map.TryGetValue(m.Key, out var s) ? s.Order : m.DefaultIndex)
            .ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            try { ordered[i].Column.DisplayIndex = i; } catch { /* indice transitorio */ }
        }
    }

    private GridLayoutStore LoadStore()
    {
        var store = UiSettings.Load<GridLayoutStore>($"grid-{_personalizationId}") ?? new GridLayoutStore();
        if (store.Layouts.Count == 0) store.Layouts.Add(BuildDefaultLayout());
        if (!store.Layouts.Any(l => l.Name == store.ActiveLayout)) store.ActiveLayout = store.Layouts[0].Name;
        return store;
    }

    private GridLayout BuildDefaultLayout() => new()
    {
        Name = "Standard",
        Columns = _columns.Select((m, i) => new GridColumnState { Key = m.Key, Visible = true, Order = i, Width = 0 }).ToList()
    };

    private static GridLayoutStore Clone(GridLayoutStore s) => new()
    {
        ActiveLayout = s.ActiveLayout,
        Layouts = s.Layouts.Select(l => new GridLayout
        {
            Name = l.Name,
            Columns = l.Columns.Select(c => new GridColumnState { Key = c.Key, Visible = c.Visible, Order = c.Order, Width = c.Width }).ToList()
        }).ToList()
    };

    // ===================== Colonne =====================
    public DataGridTextColumn AddTextColumn(string header, string path, double width = 0, bool star = false,
        bool editable = false, bool realtime = false)
    {
        var binding = new Binding(path);
        if (editable)
        {
            binding.Mode = BindingMode.TwoWay;
            binding.UpdateSourceTrigger = realtime ? UpdateSourceTrigger.PropertyChanged : UpdateSourceTrigger.LostFocus;
            binding.ValidatesOnDataErrors = true;
            binding.NotifyOnValidationError = true;
        }
        else
        {
            binding.Mode = BindingMode.OneWay;
        }

        var col = new DataGridTextColumn
        {
            Binding = binding,
            SortMemberPath = path,
            IsReadOnly = !editable,
            ElementStyle = (Style)Application.Current.FindResource("KbmGridCellText"),
            Header = BuildHeader(header, path),
            Width = ResolveWidth(width, star)
        };
        if (editable)
            col.EditingElementStyle = (Style)Application.Current.FindResource("KbmGridCellEdit");

        _grid.Columns.Add(col);
        _columns.Add(new ColumnMeta(path, header, col, _grid.Columns.Count - 1, col.Width));
        RegisterAccessor(path);
        return col;
    }

    public DataGridComboBoxColumn AddComboColumn(string header, string path, IEnumerable<string> options,
        double width = 0, bool star = false)
    {
        var col = new DataGridComboBoxColumn
        {
            SelectedItemBinding = new Binding(path) { Mode = BindingMode.TwoWay },
            ItemsSource = options.ToList(),
            SortMemberPath = path,
            Header = BuildHeader(header, path),
            Width = ResolveWidth(width, star)
        };
        _grid.Columns.Add(col);
        _columns.Add(new ColumnMeta(path, header, col, _grid.Columns.Count - 1, col.Width));
        RegisterAccessor(path);
        return col;
    }

    public DataGridCheckBoxColumn AddCheckColumn(string header, string path, double width = 70)
    {
        var col = new DataGridCheckBoxColumn
        {
            Binding = new Binding(path) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
            SortMemberPath = path,
            Header = BuildHeader(header, path, filterable: false),
            Width = ResolveWidth(width, false)
        };
        _grid.Columns.Add(col);
        _columns.Add(new ColumnMeta(path, header, col, _grid.Columns.Count - 1, col.Width));
        return col;
    }

    private static DataGridLength ResolveWidth(double width, bool star) =>
        star ? new DataGridLength(width <= 0 ? 1 : width, DataGridLengthUnitType.Star)
             : (width <= 0 ? DataGridLength.Auto : new DataGridLength(width));

    private void RegisterAccessor(string path)
    {
        if (!_accessors.ContainsKey(path)) _accessors[path] = null; // risolto lazy sul primo item
    }

    // ===================== Header con filtro =====================
    private FrameworkElement BuildHeader(string title, string path, bool filterable = true)
    {
        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var titleBlock = new TextBlock
        {
            Text = title,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 3)
        };
        System.Windows.Controls.Grid.SetRow(titleBlock, 0);
        grid.Children.Add(titleBlock);

        // Menu colonna (tasto destro): ordina / filtri / congela / trova / esporta
        grid.ContextMenu = BuildColumnContextMenu(path, filterable);

        if (!filterable) return grid;

        var filter = new TextBox
        {
            Height = 18,
            FontSize = 11,
            FontWeight = FontWeights.Normal,
            Padding = new Thickness(4, 0, 4, 0),
            Tag = "\uE721",
            VerticalContentAlignment = VerticalAlignment.Center,
            ToolTip = $"Filtra per {title}"
        };
        filter.PreviewMouseLeftButtonDown += (_, _) => _filterBoxClicked = true;
        filter.TextChanged += (_, _) =>
        {
            _filters[path] = filter.Text;
            _view?.Refresh();
        };
        _filterBoxes[path] = filter;
        System.Windows.Controls.Grid.SetRow(filter, 1);
        grid.Children.Add(filter);
        return grid;
    }

    // ===================== Menu colonna + funzioni griglia (pc07) =====================
    private ContextMenu BuildColumnContextMenu(string path, bool filterable)
    {
        var menu = new ContextMenu();

        menu.Items.Add(MenuItem("Ordinamento ascendente", "\uE74A",
            () => SortColumn(path, ListSortDirection.Ascending)));
        menu.Items.Add(MenuItem("Ordinamento discendente", "\uE74B",
            () => SortColumn(path, ListSortDirection.Descending)));
        menu.Items.Add(MenuItem("Rimuovi ordinamento", null, () => RemoveSort(path)));

        if (filterable)
        {
            menu.Items.Add(new Separator());
            menu.Items.Add(MenuItem("Cancella filtro colonna", null, () => ClearColumnFilter(path)));
            menu.Items.Add(MenuItem("Cancella tutti i filtri", null, ClearAllFilters));
        }

        menu.Items.Add(new Separator());
        menu.Items.Add(MenuItem("Congela / libera colonna", "\uE840", () => ToggleFreeze(path)));
        menu.Items.Add(MenuItem("Trova colonna (Ctrl+\\)", "\uE721", () => ToggleFinder(true)));

        menu.Items.Add(new Separator());
        var export = new MenuItem { Header = "Esporta" };
        export.Items.Add(MenuItem("Excel (.xlsx)", null, () => Export(Services.GridExportFormat.Excel)));
        export.Items.Add(MenuItem("CSV", null, () => Export(Services.GridExportFormat.Csv)));
        export.Items.Add(MenuItem("HTML", null, () => Export(Services.GridExportFormat.Html)));
        menu.Items.Add(export);

        if (_personalizationId is not null)
        {
            menu.Items.Add(new Separator());
            menu.Items.Add(MenuItem("Personalizza griglia\u2026", "\uE71D",
                () => OpenPersonalization(Window.GetWindow(this))));
        }

        return menu;
    }

    private static MenuItem MenuItem(string header, string? glyph, Action action)
    {
        var item = new MenuItem { Header = header };
        if (glyph is not null)
            item.Icon = new TextBlock
            {
                Text = glyph,
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                FontSize = 12
            };
        item.Click += (_, _) => action();
        return item;
    }

    private DataGridColumn? ColumnByPath(string path) => _columns.FirstOrDefault(m => m.Key == path)?.Column;

    private void SortColumn(string path, ListSortDirection dir)
    {
        if (_view is null) return;
        _view.SortDescriptions.Clear();
        foreach (var c in _grid.Columns) c.SortDirection = null;
        _view.SortDescriptions.Add(new SortDescription(path, dir));
        if (ColumnByPath(path) is { } col) col.SortDirection = dir;
    }

    private void RemoveSort(string path)
    {
        if (_view is null) return;
        for (var i = _view.SortDescriptions.Count - 1; i >= 0; i--)
            if (_view.SortDescriptions[i].PropertyName == path)
                _view.SortDescriptions.RemoveAt(i);
        if (ColumnByPath(path) is { } col) col.SortDirection = null;
    }

    private void ClearColumnFilter(string path)
    {
        if (_filterBoxes.TryGetValue(path, out var tb)) tb.Text = "";
        else { _filters[path] = ""; _view?.Refresh(); }
    }

    private void ClearAllFilters()
    {
        foreach (var tb in _filterBoxes.Values) tb.Text = "";
        _filters.Clear();
        _view?.Refresh();
    }

    private void ToggleFreeze(string path)
    {
        var col = ColumnByPath(path);
        if (col is null) return;
        var target = col.DisplayIndex + 1;
        _grid.FrozenColumnCount = _grid.FrozenColumnCount >= target ? 0 : target;
    }

    /// <summary>Istantanea (intestazioni + righe) delle colonne visibili e dei record filtrati/ordinati.</summary>
    public (IReadOnlyList<string> Headers, IReadOnlyList<IReadOnlyList<string>> Rows) Snapshot()
    {
        var cols = _columns
            .Where(m => m.Column.Visibility == Visibility.Visible)
            .OrderBy(m => m.Column.DisplayIndex)
            .ToList();
        var headers = cols.Select(c => c.Title).ToList();
        var rows = new List<IReadOnlyList<string>>();
        if (_view is not null)
            foreach (var item in _view)
                rows.Add((IReadOnlyList<string>)cols.Select(c => GetValue(item!, c.Key)).ToList());
        return (headers, rows);
    }

    /// <summary>Costruisce il modello report (stampa interna) dall'elenco corrente.</summary>
    public ReportDocumentDto BuildReportModel(string title, string? subtitle = null,
        string? company = null, string? footer = null)
    {
        var (headers, rows) = Snapshot();
        var header = new List<ReportFieldDto>
        {
            new("Data stampa", DateTime.Now.ToString("dd/MM/yyyy HH:mm")),
            new("Record", rows.Count.ToString())
        };
        return new ReportDocumentDto(title, subtitle, header, headers, rows, null, footer, company);
    }

    private void Export(Services.GridExportFormat format)
    {
        var (headers, rows) = Snapshot();
        if (headers.Count == 0) return;

        try
        {
            var p = Services.GridExporter.ExportToDesktop(ExportName, format, headers, rows);
            ToastService.Success($"Esportati {rows.Count} record sul Desktop.", "Esportazione griglia");
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(p) { UseShellExecute = true }); }
            catch { /* file generato comunque */ }
        }
        catch (Exception ex)
        {
            ToastService.Error("Esportazione non riuscita: " + ex.Message, "Esportazione griglia");
        }
    }

    // ===================== Trova colonna (Ctrl+\) =====================
    private void BuildFinderPanel()
    {
        _finderSearch = new TextBox
        {
            Height = 26,
            Margin = new Thickness(0, 0, 0, 6),
            VerticalContentAlignment = VerticalAlignment.Center,
            Tag = "Cerca colonna\u2026"
        };
        _finderSearch.TextChanged += (_, _) => RefreshFinderList();
        _finderSearch.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter) GoToSelectedColumn();
            else if (e.Key == Key.Escape) ToggleFinder(false);
        };

        _finderList = new ListBox { BorderThickness = new Thickness(1) };
        _finderList.MouseDoubleClick += (_, _) => GoToSelectedColumn();
        _finderList.KeyDown += (_, e) => { if (e.Key == Key.Enter) GoToSelectedColumn(); };

        var header = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
        var close = new Button
        {
            Content = "\uE711",
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            FontSize = 10,
            Width = 22,
            Height = 22,
            Padding = new Thickness(0),
            ToolTip = "Chiudi"
        };
        close.Click += (_, _) => ToggleFinder(false);
        DockPanel.SetDock(close, Dock.Right);
        header.Children.Add(close);
        header.Children.Add(new TextBlock
        {
            Text = "Trova colonna",
            FontWeight = FontWeights.SemiBold,
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center
        });

        var inner = new DockPanel { Margin = new Thickness(10) };
        DockPanel.SetDock(header, Dock.Top);
        DockPanel.SetDock(_finderSearch, Dock.Top);
        inner.Children.Add(header);
        inner.Children.Add(_finderSearch);
        inner.Children.Add(_finderList);

        _finderPanel = new Border
        {
            Width = 220,
            Visibility = Visibility.Collapsed,
            BorderThickness = new Thickness(1, 0, 0, 0),
            BorderBrush = (Brush)(Application.Current.TryFindResource("Brush.Border") ?? Brushes.LightGray),
            Background = (Brush)(Application.Current.TryFindResource("Brush.SurfaceAlt") ?? Brushes.WhiteSmoke),
            Child = inner
        };
    }

    private void ToggleFinder(bool? show = null)
    {
        var visible = show ?? _finderPanel.Visibility != Visibility.Visible;
        _finderPanel.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        if (visible)
        {
            RefreshFinderList();
            _finderSearch.Focus();
        }
    }

    private void RefreshFinderList()
    {
        var q = _finderSearch.Text?.Trim() ?? "";
        _finderList.ItemsSource = _columns
            .Where(m => m.Column.Visibility == Visibility.Visible)
            .Where(m => q.Length == 0 || m.Title.Contains(q, StringComparison.OrdinalIgnoreCase))
            .OrderBy(m => m.Column.DisplayIndex)
            .Select(m => new FinderItem(m.Title, m.Column))
            .ToList();
        if (_finderList.Items.Count > 0) _finderList.SelectedIndex = 0;
    }

    private void GoToSelectedColumn()
    {
        if (_finderList.SelectedItem is not FinderItem fi) return;
        var target = _grid.SelectedItem ?? (_view is { Count: > 0 } ? _view.GetItemAt(0) : null);
        if (target is not null) _grid.ScrollIntoView(target, fi.Column);
    }

    private bool FilterPredicate(object item)
    {
        foreach (var kv in _filters)
        {
            if (string.IsNullOrWhiteSpace(kv.Value)) continue;
            var text = GetValue(item, kv.Key);
            if (text.IndexOf(kv.Value.Trim(), StringComparison.OrdinalIgnoreCase) < 0)
                return false;
        }
        return true;
    }

    private string GetValue(object item, string path)
    {
        if (!_accessors.TryGetValue(path, out var pi) || pi is null)
        {
            pi = item.GetType().GetProperty(path);
            _accessors[path] = pi;
        }
        return pi?.GetValue(item)?.ToString() ?? string.Empty;
    }

    // ===================== Sort vs filtro =====================
    private void OnSorting(object? sender, DataGridSortingEventArgs e)
    {
        // Se il click è avvenuto sulla casella filtro, non ordinare.
        if (_filterBoxClicked)
        {
            _filterBoxClicked = false;
            e.Handled = true;
        }
    }

    // ===================== Edit in-line =====================
    private void OnRowEditEnding(object? sender, DataGridRowEditEndingEventArgs e)
    {
        if (e.EditAction != DataGridEditAction.Commit) return;
        var item = e.Row.Item;

        if (item is IDataErrorInfo dei && HasErrors(dei))
        {
            e.Cancel = true;
            return;
        }

        // Posticipa: il commit del binding avviene dopo questo evento.
        Dispatcher.BeginInvoke(new Action(() => RowCommitted?.Invoke(item)),
            System.Windows.Threading.DispatcherPriority.Background);
    }

    private static bool HasErrors(IDataErrorInfo item)
    {
        if (!string.IsNullOrEmpty(item.Error)) return true;
        foreach (var p in item.GetType().GetProperties())
        {
            if (p.GetIndexParameters().Length > 0) continue;
            if (!string.IsNullOrEmpty(item[p.Name])) return true;
        }
        return false;
    }
}
