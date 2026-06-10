using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KBM.Client.Services;

namespace KBM.Client;

public partial class GridCustomizeWindow : Window
{
    private readonly IReadOnlyList<GridColumnInfo> _catalog;
    private readonly ObservableCollection<LayoutVm> _layouts = new();
    private LayoutVm? _current;
    private bool _suppress;

    public GridLayoutStore? ResultStore { get; private set; }

    public GridCustomizeWindow(GridLayoutStore store, IReadOnlyList<GridColumnInfo> catalog)
    {
        InitializeComponent();
        _catalog = catalog;

        foreach (var l in store.Layouts)
            _layouts.Add(LayoutVm.From(l, catalog));
        if (_layouts.Count == 0)
            _layouts.Add(LayoutVm.Default(catalog));

        LayoutCombo.ItemsSource = _layouts;
        var active = _layouts.FirstOrDefault(l => l.Name == store.ActiveLayout) ?? _layouts[0];
        LayoutCombo.SelectedItem = active;
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }

    private void LayoutCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        CommitName();
        _current = LayoutCombo.SelectedItem as LayoutVm;
        _suppress = true;
        NameBox.Text = _current?.Name ?? "";
        ColumnsList.ItemsSource = _current?.Columns;
        _suppress = false;
    }

    private void CommitName()
    {
        if (_current is null || _suppress) return;
        var name = NameBox.Text?.Trim();
        if (!string.IsNullOrEmpty(name) && name != _current.Name)
        {
            _current.Name = name;
            LayoutCombo.Items.Refresh();
        }
    }

    private void NewLayout_Click(object sender, RoutedEventArgs e)
    {
        CommitName();
        var baseVm = _current ?? LayoutVm.Default(_catalog);
        var name = UniqueName("Nuovo layout");
        var vm = new LayoutVm { Name = name };
        foreach (var c in baseVm.Columns)
            vm.Columns.Add(new ColRowVm { Key = c.Key, Title = c.Title, Visible = c.Visible, Width = c.Width });
        _layouts.Add(vm);
        LayoutCombo.SelectedItem = vm;
        NameBox.Focus();
        NameBox.SelectAll();
    }

    private void DeleteLayout_Click(object sender, RoutedEventArgs e)
    {
        if (_current is null) return;
        if (_layouts.Count <= 1)
        {
            MessageBox.Show("Deve esistere almeno un layout.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var toRemove = _current;
        _layouts.Remove(toRemove);
        LayoutCombo.SelectedItem = _layouts[0];
    }

    private void MoveUp_Click(object sender, RoutedEventArgs e)
    {
        if (_current is null || (sender as FrameworkElement)?.Tag is not ColRowVm row) return;
        var i = _current.Columns.IndexOf(row);
        if (i > 0) _current.Columns.Move(i, i - 1);
    }

    private void MoveDown_Click(object sender, RoutedEventArgs e)
    {
        if (_current is null || (sender as FrameworkElement)?.Tag is not ColRowVm row) return;
        var i = _current.Columns.IndexOf(row);
        if (i >= 0 && i < _current.Columns.Count - 1) _current.Columns.Move(i, i + 1);
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        if (_current is null) return;
        _current.Columns.Clear();
        foreach (var c in _catalog)
            _current.Columns.Add(new ColRowVm { Key = c.Key, Title = c.Title, Visible = true, Width = 0 });
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        CommitName();
        var store = new GridLayoutStore
        {
            ActiveLayout = _current?.Name ?? _layouts[0].Name,
            Layouts = _layouts.Select(l => new GridLayout
            {
                Name = l.Name,
                Columns = l.Columns.Select((c, idx) => new GridColumnState
                {
                    Key = c.Key,
                    Visible = c.Visible,
                    Order = idx,
                    Width = c.Width
                }).ToList()
            }).ToList()
        };
        ResultStore = store;
        DialogResult = true;
    }

    private string UniqueName(string baseName)
    {
        var name = baseName;
        var n = 2;
        while (_layouts.Any(l => l.Name == name)) name = $"{baseName} {n++}";
        return name;
    }

    public sealed class LayoutVm
    {
        public string Name { get; set; } = "Standard";
        public ObservableCollection<ColRowVm> Columns { get; } = new();
        public override string ToString() => Name;

        public static LayoutVm From(GridLayout l, IReadOnlyList<GridColumnInfo> catalog)
        {
            var vm = new LayoutVm { Name = l.Name };
            var byKey = l.Columns.ToDictionary(c => c.Key, c => c);
            // ordine come salvato, poi eventuali colonne mancanti (nuove) in coda
            foreach (var st in l.Columns.OrderBy(c => c.Order))
            {
                var info = catalog.FirstOrDefault(c => c.Key == st.Key);
                if (info is null) continue;
                vm.Columns.Add(new ColRowVm { Key = info.Key, Title = info.Title, Visible = st.Visible, Width = st.Width });
            }
            foreach (var info in catalog.Where(c => !byKey.ContainsKey(c.Key)))
                vm.Columns.Add(new ColRowVm { Key = info.Key, Title = info.Title, Visible = true, Width = 0 });
            return vm;
        }

        public static LayoutVm Default(IReadOnlyList<GridColumnInfo> catalog)
        {
            var vm = new LayoutVm { Name = "Standard" };
            foreach (var c in catalog)
                vm.Columns.Add(new ColRowVm { Key = c.Key, Title = c.Title, Visible = true, Width = 0 });
            return vm;
        }
    }

    public sealed class ColRowVm : INotifyPropertyChanged
    {
        private bool _visible = true;
        private double _width;
        public string Key { get; set; } = "";
        public string Title { get; set; } = "";

        public bool Visible
        {
            get => _visible;
            set { _visible = value; OnChanged(nameof(Visible)); }
        }

        public double Width
        {
            get => _width;
            set { _width = value < 0 ? 0 : value; OnChanged(nameof(Width)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnChanged(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}
