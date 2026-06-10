using System.Windows.Controls;
using KBM.Client.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class ModuleHomeView : UserControl
{
    public ModuleHomeView(NavGroup group, Action<string> openFeature)
    {
        InitializeComponent();

        ModuleIcon.Text = group.Glyph;
        ModuleTitle.Text = group.Label;
        var enabled = group.Features.Count(f => f.Enabled);
        ModuleSubtitle.Text = $"{group.Features.Count} funzioni · {enabled} disponibili";

        foreach (var feature in group.Features)
            Tiles.Children.Add(NavTileFactory.BuildTile(feature, openFeature));
    }
}
