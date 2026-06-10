using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ControlzEx.Theming;
using KBM.Client.Services;

namespace KBM.Client;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Accent blu corporate (look gestionale enterprise, non Windows 11 flat)
        var theme = RuntimeThemeGenerator.Current.GenerateRuntimeTheme("Light", Color.FromRgb(0x1C, 0x5D, 0x99));
        if (theme is not null)
            ThemeManager.Current.ChangeTheme(this, theme);

        var shotPath = e.Args.FirstOrDefault(a => a.StartsWith("--shot="))?["--shot=".Length..];
        if (shotPath is not null)
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;

        if (e.Args.Contains("--preview-login"))
        {
            var w = new LoginWindow();
            w.Show();
            if (shotPath is not null) CaptureAfter(w, shotPath, 1200);
            return;
        }

        if (e.Args.Contains("--preview-setup"))
        {
            var w = new SetupWizardWindow();
            w.Show();
            if (shotPath is not null) CaptureAfter(w, shotPath, 1200);
            return;
        }

        if (e.Args.Contains("--preview-shell"))
        {
            var fake = new LoginSession("x", "x", DateTime.UtcNow.AddHours(1), 1, "admin",
                "Amministratore", 1, "Kever S.r.l.", new[] { "SuperAdmin" });
            var pw = new ShellWindow(fake) { WindowState = WindowState.Maximized };
            pw.Show();
            if (e.Args.Contains("--module"))
                pw.PreviewOpen("module:amministrazione");
            var openArg = e.Args.FirstOrDefault(a => a.StartsWith("--open="));
            if (openArg is not null)
                pw.PreviewOpen(openArg["--open=".Length..]);
            if (shotPath is not null) CaptureAfter(pw, shotPath, 2500);
            return;
        }

        var api = new AuthApiClient();
        try
        {
            var status = await api.GetSetupStatusAsync();
            if (status?.Required == true)
            {
                new SetupWizardWindow().Show();
                return;
            }
        }
        catch
        {
            // API non raggiungibile: mostra login comunque
        }

        new LoginWindow().Show();
    }

    /// <summary>Renderizza la finestra in PNG (per screenshot affidabili anche senza desktop interattivo).</summary>
    private void CaptureAfter(Window w, string path, int delayMs)
    {
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(delayMs) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            try
            {
                var root = w.Content as FrameworkElement ?? w;
                root.UpdateLayout();
                var width = (int)Math.Ceiling(root.ActualWidth);
                var height = (int)Math.Ceiling(root.ActualHeight);
                if (width > 0 && height > 0)
                {
                    var dv = new DrawingVisual();
                    using (var ctx = dv.RenderOpen())
                    {
                        var vb = new VisualBrush(root) { Stretch = Stretch.None };
                        ctx.DrawRectangle(vb, null, new Rect(0, 0, width, height));
                    }
                    var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                    rtb.Render(dv);
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(rtb));
                    var dir = Path.GetDirectoryName(Path.GetFullPath(path));
                    if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                    using var fs = File.Create(path);
                    encoder.Save(fs);
                }
            }
            catch { /* best-effort */ }
            Shutdown();
        };
        timer.Start();
    }
}
