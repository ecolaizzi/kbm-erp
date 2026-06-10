using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using KBM.Client.Services;

namespace KBM.Client.Controls;

public partial class ListPageView : UserControl
{
    private readonly List<Button> _filters = new();
    private readonly List<(string Label, Button Button)> _toolbarButtons = new();
    private readonly List<(Key Key, ModifierKeys Mods, Action Action)> _shortcuts = new();
    private FilterableGrid? _navGrid;
    private bool _wingsEnabled;

    public static readonly DependencyProperty GridContentProperty =
        DependencyProperty.Register(nameof(GridContent), typeof(UIElement), typeof(ListPageView),
            new PropertyMetadata(null, (d, e) =>
            {
                if (d is ListPageView view && e.NewValue is UIElement el)
                    view.GridHost.Content = el;
            }));

    public ListPageView()
    {
        InitializeComponent();
        PreviewKeyDown += OnPreviewKeyDown;
    }

    /// <summary>Chiave per persistere le preferenze toolbar (per pagina).</summary>
    public string PageKey { get; set; } = "default";

    public string PageTitle
    {
        get => TitleText.Text;
        set => TitleText.Text = value;
    }

    public string Subtitle
    {
        get => SubtitleText.Text;
        set
        {
            SubtitleText.Text = value;
            SubtitleText.Visibility = string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public string RecordCountLabel
    {
        get => CountText.Text;
        set => CountText.Text = value;
    }

    public string PaginationLabel
    {
        get => PaginationText.Text;
        set => PaginationText.Text = value;
    }

    public UIElement GridContent
    {
        get => (UIElement)GetValue(GridContentProperty);
        set => SetValue(GridContentProperty, value);
    }

    public TextBox SearchInput => SearchBox;

    // ===================== Toolbar =====================
    public Button AddToolbarButton(string glyph, string label, bool primary, RoutedEventHandler click,
        Key shortcut = Key.None, ModifierKeys modifiers = ModifierKeys.None)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal };
        if (!string.IsNullOrEmpty(glyph))
        {
            content.Children.Add(new TextBlock
            {
                Text = glyph,
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                FontSize = 13,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 7, 0)
            });
        }
        content.Children.Add(new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center });

        var btn = new Button
        {
            Content = content,
            Style = (Style)FindResource(primary ? "KbmToolbarPrimary" : "KbmToolbarButton")
        };
        btn.Click += click;

        if (shortcut != Key.None)
        {
            var hint = ShortcutText(shortcut, modifiers);
            btn.ToolTip = $"{label} ({hint})";
            _shortcuts.Add((shortcut, modifiers, () => InvokeButton(btn, click)));
        }

        ToolbarPanel.Children.Add(btn);
        _toolbarButtons.Add((label, btn));
        ApplyPersistedVisibility(label, btn);
        return btn;
    }

    private static void InvokeButton(Button btn, RoutedEventHandler click)
    {
        if (!btn.IsEnabled || btn.Visibility != Visibility.Visible) return;
        click(btn, new RoutedEventArgs(ButtonBase.ClickEvent, btn));
    }

    private static string ShortcutText(Key key, ModifierKeys mods)
    {
        var prefix = "";
        if (mods.HasFlag(ModifierKeys.Control)) prefix += "Ctrl+";
        if (mods.HasFlag(ModifierKeys.Shift)) prefix += "Shift+";
        if (mods.HasFlag(ModifierKeys.Alt)) prefix += "Alt+";
        return prefix + key;
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Non intercettare i tasti mentre si digita in un campo (ricerca, filtro, edit in-line).
        if (Keyboard.FocusedElement is TextBoxBase) return;

        var mods = Keyboard.Modifiers;
        foreach (var (key, sMods, action) in _shortcuts)
        {
            if (key == e.Key && sMods == mods)
            {
                action();
                e.Handled = true;
                return;
            }
        }
    }

    // ===================== Navigatore record =====================
    /// <summary>Collega il navigatore record (footer + Ctrl+P/R/S/U) alla griglia.</summary>
    public void AttachRecordNavigator(FilterableGrid grid)
    {
        _navGrid = grid;
        RecordNav.Visibility = Visibility.Visible;
        grid.SelectionChanged += _ => UpdateRecordNavigator();

        _shortcuts.Add((Key.P, ModifierKeys.Control, () => { grid.MoveFirst(); UpdateRecordNavigator(); }));
        _shortcuts.Add((Key.R, ModifierKeys.Control, () => { grid.MovePrevious(); UpdateRecordNavigator(); }));
        _shortcuts.Add((Key.S, ModifierKeys.Control, () => { grid.MoveNext(); UpdateRecordNavigator(); }));
        _shortcuts.Add((Key.U, ModifierKeys.Control, () => { grid.MoveLast(); UpdateRecordNavigator(); }));
        UpdateRecordNavigator();
    }

    /// <summary>Aggiorna l'indicatore "N / totale". Chiamare dopo il (ri)caricamento dati.</summary>
    public void UpdateRecordNavigator()
    {
        if (_navGrid is null) return;
        var total = _navGrid.Count;
        var pos = _navGrid.CurrentIndex + 1;
        NavPosText.Text = $"{pos} / {total}";
        NavFirst.IsEnabled = NavPrev.IsEnabled = pos > 1;
        NavNext.IsEnabled = NavLast.IsEnabled = pos < total && total > 0;
    }

    // ===================== Personalizzazione griglia / Layout corpo =====================
    private FilterableGrid? _persoGrid;
    private bool _loadingLayouts;

    /// <summary>Abilita personalizzazione colonne + selettore layout per la griglia (stile NTS).</summary>
    public void AttachPersonalization(FilterableGrid grid, string gridId)
    {
        _persoGrid = grid;
        grid.EnablePersonalization(gridId);
        grid.LayoutsChanged += RefreshLayoutCombo;
        PersonalizeBtn.Visibility = Visibility.Visible;
        LayoutPanel.Visibility = Visibility.Visible;
        RefreshLayoutCombo();
    }

    private void RefreshLayoutCombo()
    {
        if (_persoGrid is null) return;
        _loadingLayouts = true;
        LayoutCombo.ItemsSource = _persoGrid.LayoutNames;
        LayoutCombo.SelectedItem = _persoGrid.ActiveLayout;
        LayoutPanel.Visibility = _persoGrid.LayoutNames.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        _loadingLayouts = false;
    }

    private void LayoutCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loadingLayouts || _persoGrid is null || LayoutCombo.SelectedItem is not string name) return;
        _persoGrid.SetActiveLayout(name);
    }

    private void PersonalizeBtn_Click(object sender, RoutedEventArgs e)
    {
        _persoGrid?.OpenPersonalization(Window.GetWindow(this));
        RefreshLayoutCombo();
    }

    private void NavFirst_Click(object sender, RoutedEventArgs e) { _navGrid?.MoveFirst(); UpdateRecordNavigator(); }
    private void NavPrev_Click(object sender, RoutedEventArgs e) { _navGrid?.MovePrevious(); UpdateRecordNavigator(); }
    private void NavNext_Click(object sender, RoutedEventArgs e) { _navGrid?.MoveNext(); UpdateRecordNavigator(); }
    private void NavLast_Click(object sender, RoutedEventArgs e) { _navGrid?.MoveLast(); UpdateRecordNavigator(); }

    public void AddToolbarSeparator()
    {
        ToolbarPanel.Children.Add(new Border
        {
            Width = 1,
            Background = (Brush)FindResource("Brush.Border"),
            Margin = new Thickness(4, 4, 12, 4)
        });
    }

    private void ApplyPersistedVisibility(string label, Button btn)
    {
        var flags = UiSettings.LoadFlags($"toolbar-{PageKey}");
        if (flags.TryGetValue(label, out var visible) && !visible)
            btn.Visibility = Visibility.Collapsed;
    }

    private void ConfigBtn_Click(object sender, RoutedEventArgs e)
    {
        ConfigList.Children.Clear();
        foreach (var (label, btn) in _toolbarButtons)
        {
            var cb = new CheckBox
            {
                Content = label,
                IsChecked = btn.Visibility == Visibility.Visible,
                Margin = new Thickness(0, 4, 0, 4)
            };
            cb.Checked += (_, _) => SetCommandVisible(label, btn, true);
            cb.Unchecked += (_, _) => SetCommandVisible(label, btn, false);
            ConfigList.Children.Add(cb);
        }
        ConfigPopup.IsOpen = true;
    }

    private void SetCommandVisible(string label, Button btn, bool visible)
    {
        btn.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        var flags = UiSettings.LoadFlags($"toolbar-{PageKey}");
        flags[label] = visible;
        UiSettings.SaveFlags($"toolbar-{PageKey}", flags);
    }

    // ===================== Quick filters =====================
    public Button AddQuickFilter(string label, RoutedEventHandler click, bool active = false)
    {
        var btn = new Button
        {
            Content = label,
            Height = 28,
            Padding = new Thickness(12, 0, 12, 0),
            Margin = new Thickness(0, 0, 6, 0),
            FontSize = 12,
            Style = (Style)FindResource("KbmToolbarButton")
        };
        btn.Click += (s, e) =>
        {
            SetActiveFilter(btn);
            click(s, e);
        };
        _filters.Add(btn);
        FilterPanel.Children.Add(btn);
        if (active) SetActiveFilter(btn);
        return btn;
    }

    private void SetActiveFilter(Button active)
    {
        foreach (var f in _filters)
            f.Style = (Style)FindResource("KbmToolbarButton");
        active.Style = (Style)FindResource("KbmToolbarPrimary");
        active.Height = 28;
    }

    // ===================== Wings (dettaglio contestuale) =====================
    public void EnableWings(bool openByDefault = false)
    {
        _wingsEnabled = true;
        WingsToggle.Visibility = Visibility.Visible;
        WingsPanel.Visibility = openByDefault ? Visibility.Visible : Visibility.Collapsed;
    }

    private void WingsToggle_Click(object sender, RoutedEventArgs e)
    {
        WingsPanel.Visibility = WingsPanel.Visibility == Visibility.Visible
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    /// <summary>Mostra il dettaglio di un record nel pannello Wings.</summary>
    public void SetWingDetail(string title, IEnumerable<(string Label, string Value)> fields,
        (string Text, Brush Background, Brush Foreground)? badge = null)
    {
        WingTitle.Text = title;
        WingHost.Children.Clear();

        if (badge is { } b)
        {
            var badgeBorder = new Border
            {
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10, 3, 10, 3),
                Background = b.Background,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 12),
                Child = new TextBlock { Text = b.Text, Foreground = b.Foreground, FontSize = 11, FontWeight = FontWeights.SemiBold }
            };
            WingHost.Children.Add(badgeBorder);
        }

        foreach (var (label, value) in fields)
        {
            WingHost.Children.Add(new TextBlock
            {
                Text = label.ToUpperInvariant(),
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("Brush.TextMuted"),
                Margin = new Thickness(0, 10, 0, 2)
            });
            WingHost.Children.Add(new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(value) ? "—" : value,
                FontSize = 13,
                Foreground = (Brush)FindResource("Brush.Text"),
                TextWrapping = TextWrapping.Wrap
            });
        }

        if (_wingsEnabled && WingsPanel.Visibility != Visibility.Visible)
            WingsPanel.Visibility = Visibility.Visible;
    }

    public void ClearWing()
    {
        WingTitle.Text = "Dettaglio";
        WingHost.Children.Clear();
        WingHost.Children.Add(new TextBlock
        {
            Text = "Seleziona un record per vederne il dettaglio.",
            Foreground = (Brush)FindResource("Brush.TextMuted"),
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap
        });
    }

    // ===================== Error =====================
    public void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }

    public void HideError() => ErrorText.Visibility = Visibility.Collapsed;
}
