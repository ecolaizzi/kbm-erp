using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace KBM.Client.Services;

public enum ToastKind { Info, Success, Warning, Error }

/// <summary>
/// Messaggi popup in basso a destra (stile Business Cube): notifiche non bloccanti
/// che sostituiscono i MessageBox informativi. Vedi help pc09 "Messaggi popup".
/// </summary>
public static class ToastService
{
    private static ItemsControl? _host;

    public static void Initialize(ItemsControl host) => _host = host;

    public static void Info(string message, string? title = null) => Show(message, title, ToastKind.Info);
    public static void Success(string message, string? title = null) => Show(message, title, ToastKind.Success);
    public static void Warning(string message, string? title = null) => Show(message, title, ToastKind.Warning);
    public static void Error(string message, string? title = null) => Show(message, title, ToastKind.Error);

    public static void Show(string message, string? title, ToastKind kind, int milliseconds = 4000)
    {
        var host = _host;
        if (host is null) return;

        if (!host.Dispatcher.CheckAccess())
        {
            host.Dispatcher.BeginInvoke(new Action(() => Show(message, title, kind, milliseconds)));
            return;
        }

        var (accent, glyph) = Style(kind);

        var icon = new TextBlock
        {
            Text = glyph,
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            FontSize = 16,
            Foreground = accent,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 1, 10, 0)
        };

        var stack = new StackPanel();
        if (!string.IsNullOrWhiteSpace(title))
            stack.Children.Add(new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.SemiBold,
                FontSize = 12.5,
                Foreground = new SolidColorBrush(Color.FromRgb(0x1F, 0x29, 0x37))
            });
        stack.Children.Add(new TextBlock
        {
            Text = message,
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 300,
            Foreground = new SolidColorBrush(Color.FromRgb(0x4B, 0x55, 0x63)),
            Margin = new Thickness(0, string.IsNullOrWhiteSpace(title) ? 0 : 2, 0, 0)
        });

        var content = new DockPanel();
        DockPanel.SetDock(icon, Dock.Left);
        content.Children.Add(icon);
        content.Children.Add(stack);

        var card = new Border
        {
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xE5, 0xE7, 0xEB)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(14, 11, 14, 11),
            Margin = new Thickness(0, 8, 0, 0),
            MinWidth = 280,
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Black,
                Opacity = 0.18,
                BlurRadius = 14,
                ShadowDepth = 2
            },
            Child = new Grid
            {
                Children =
                {
                    new Border { Width = 3, HorizontalAlignment = HorizontalAlignment.Left, Background = accent,
                                 CornerRadius = new CornerRadius(2), Margin = new Thickness(-9, -7, 0, -7) },
                    new Grid { Margin = new Thickness(6, 0, 0, 0), Children = { content } }
                }
            },
            Opacity = 0,
            RenderTransform = new TranslateTransform(20, 0)
        };

        host.Items.Add(card);

        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180));
        var slideIn = new DoubleAnimation(20, 0, TimeSpan.FromMilliseconds(220))
        { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
        card.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        ((TranslateTransform)card.RenderTransform).BeginAnimation(TranslateTransform.XProperty, slideIn);

        void Dismiss()
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(220));
            fadeOut.Completed += (_, _) => host.Items.Remove(card);
            card.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        card.MouseLeftButtonUp += (_, _) => Dismiss();

        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(milliseconds) };
        timer.Tick += (_, _) => { timer.Stop(); Dismiss(); };
        timer.Start();
    }

    private static (SolidColorBrush accent, string glyph) Style(ToastKind kind) => kind switch
    {
        ToastKind.Success => (new SolidColorBrush(Color.FromRgb(0x16, 0xA3, 0x4A)), "\uE73E"),
        ToastKind.Warning => (new SolidColorBrush(Color.FromRgb(0xD9, 0x77, 0x06)), "\uE7BA"),
        ToastKind.Error => (new SolidColorBrush(Color.FromRgb(0xDC, 0x26, 0x26)), "\uEA39"),
        _ => (new SolidColorBrush(Color.FromRgb(0x25, 0x63, 0xEB)), "\uE946"),
    };
}
