using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using KBM.Client.Services;

namespace KBM.Client.Controls;

/// <summary>Crea le tile/card di funzione condivise tra Module Home e Launchpad.</summary>
public static class NavTileFactory
{
    private static Brush Res(string key) => (Brush)Application.Current.FindResource(key);

    public static Border BuildTile(NavFeature feature, Action<string> openFeature, bool showFavorite = true)
    {
        var card = new Border
        {
            Width = 268,
            Height = 96,
            Margin = new Thickness(0, 0, 14, 14),
            CornerRadius = new CornerRadius(4),
            Background = Brushes.White,
            BorderBrush = Res("Brush.Border"),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(14, 12, 14, 12),
            Cursor = feature.Enabled ? Cursors.Hand : Cursors.Arrow,
            Tag = feature
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var iconBox = new Border
        {
            Width = 40,
            Height = 40,
            CornerRadius = new CornerRadius(3),
            Margin = new Thickness(0, 0, 12, 0),
            VerticalAlignment = VerticalAlignment.Top,
            Background = feature.Enabled ? Res("Brush.PrimarySelect")
                : new SolidColorBrush(Color.FromRgb(0xF1, 0xF3, 0xF5))
        };
        iconBox.Child = new TextBlock
        {
            Text = feature.Glyph,
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            FontSize = 19,
            Foreground = feature.Enabled ? Res("Brush.Primary")
                : new SolidColorBrush(Color.FromRgb(0x9C, 0xA3, 0xAF)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(iconBox, 0);
        grid.Children.Add(iconBox);

        var texts = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        var titleRow = new DockPanel { LastChildFill = false };

        if (!feature.Enabled)
        {
            titleRow.Children.Add(MakeBadge());
        }
        else if (showFavorite)
        {
            titleRow.Children.Add(MakeStar(feature.Key));
        }

        var title = new TextBlock
        {
            Text = feature.Label,
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = Res("Brush.Text"),
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        DockPanel.SetDock(title, Dock.Left);
        titleRow.Children.Add(title);
        texts.Children.Add(titleRow);

        texts.Children.Add(new TextBlock
        {
            Text = feature.Description,
            FontSize = 11,
            Foreground = Res("Brush.TextSecondary"),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 4, 0, 0)
        });
        Grid.SetColumn(texts, 1);
        grid.Children.Add(texts);

        card.Child = grid;

        if (feature.Enabled)
        {
            card.MouseLeftButtonUp += (_, _) => openFeature(feature.Key);
            card.MouseEnter += (_, _) =>
            {
                card.BorderBrush = Res("Brush.Primary");
                card.Background = Res("Brush.PrimarySelect");
            };
            card.MouseLeave += (_, _) =>
            {
                card.BorderBrush = Res("Brush.Border");
                card.Background = Brushes.White;
            };
        }
        else
        {
            card.Opacity = 0.75;
        }

        return card;
    }

    private static Border MakeBadge()
    {
        var badge = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0xF1, 0xF3, 0xF5)),
            CornerRadius = new CornerRadius(2),
            Padding = new Thickness(6, 1, 6, 1),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                Text = "Prossimamente",
                FontSize = 9,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80))
            }
        };
        DockPanel.SetDock(badge, Dock.Right);
        return badge;
    }

    private static TextBlock MakeStar(string key)
    {
        var gold = new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B));
        var star = new TextBlock
        {
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            FontSize = 14,
            VerticalAlignment = VerticalAlignment.Center,
            Cursor = Cursors.Hand,
            ToolTip = "Aggiungi/Rimuovi dai preferiti"
        };
        DockPanel.SetDock(star, Dock.Right);

        void Render()
        {
            var fav = NavState.IsFavorite(key);
            star.Text = fav ? "\uE735" : "\uE734";
            star.Foreground = fav ? gold : Res("Brush.TextMuted");
        }
        Render();

        star.MouseLeftButtonUp += (_, e) =>
        {
            e.Handled = true;
            NavState.ToggleFavorite(key);
            Render();
        };
        return star;
    }
}
