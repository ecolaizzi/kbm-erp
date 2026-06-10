using System.Windows;
using System.Windows.Controls;
using KBM.Client.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class LaunchpadView : UserControl
{
    private readonly Action<string> _openFeature;
    private readonly string _displayName;

    public LaunchpadView(string displayName, Action<string> openFeature)
    {
        InitializeComponent();
        _openFeature = openFeature;
        _displayName = displayName;

        Greeting.Text = $"{TimeGreeting()}, {displayName}.";
        SubGreeting.Text = DateTime.Now.ToString("dddd d MMMM yyyy");

        NavState.Changed += Rebuild;
        Unloaded += (_, _) => NavState.Changed -= Rebuild;
        Rebuild();
    }

    private static string TimeGreeting()
    {
        var h = DateTime.Now.Hour;
        if (h < 12) return "Buongiorno";
        if (h < 18) return "Buon pomeriggio";
        return "Buonasera";
    }

    private void Rebuild()
    {
        Sections.Children.Clear();

        var favorites = NavState.Favorites
            .Select(NavigationRegistry.Find).OfType<NavFeature>().ToList();
        if (favorites.Count > 0)
            AddSection("\uE735  PREFERITI", favorites);

        var recent = NavState.Recent
            .Select(NavigationRegistry.Find).OfType<NavFeature>().ToList();
        if (recent.Count > 0)
            AddSection("\uE823  RECENTI", recent);

        foreach (var group in NavigationRegistry.Groups)
            AddSection(group.Label.ToUpperInvariant(), group.Features);
    }

    private void AddSection(string title, IReadOnlyList<NavFeature> features)
    {
        Sections.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            Foreground = (System.Windows.Media.Brush)FindResource("Brush.TextSecondary"),
            Margin = new Thickness(0, 0, 0, 12)
        });

        var wrap = new WrapPanel { Margin = new Thickness(0, 0, 0, 22) };
        foreach (var feature in features)
            wrap.Children.Add(NavTileFactory.BuildTile(feature, _openFeature));
        Sections.Children.Add(wrap);
    }
}
