using System.Text.Json;
using System.Windows;

namespace KBM.Client.Views;

public partial class WorkflowActionWindow : Window
{
    public string? Decision { get; private set; }
    public string? Notes { get; private set; }

    private readonly bool _decisionRequired;

    private WorkflowActionWindow(string title, string subtitle, IReadOnlyList<string>? options, bool notesRequired)
    {
        InitializeComponent();
        TitleText.Text = title;
        SubtitleText.Text = subtitle;
        _decisionRequired = options is { Count: > 0 };
        _notesRequired = notesRequired;

        if (_decisionRequired)
        {
            DecisionPanel.Visibility = Visibility.Visible;
            DecisionCombo.ItemsSource = options;
            DecisionCombo.SelectedIndex = 0;
        }
    }

    private readonly bool _notesRequired;

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (_decisionRequired && DecisionCombo.SelectedItem is null)
        {
            MessageBox.Show("Seleziona un esito.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (_notesRequired && string.IsNullOrWhiteSpace(NotesBox.Text))
        {
            MessageBox.Show("Inserisci una nota.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        Decision = DecisionCombo.SelectedItem as string;
        Notes = string.IsNullOrWhiteSpace(NotesBox.Text) ? null : NotesBox.Text.Trim();
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    /// <summary>Dialog per completare un task; mostra gli esiti se lo step e decisionale.</summary>
    public static WorkflowActionWindow Complete(string stepName, string? decisionOptionsJson, Window? owner)
    {
        var options = ParseLabels(decisionOptionsJson);
        var w = new WorkflowActionWindow("Completa task", $"Step: {stepName}", options, notesRequired: false) { Owner = owner };
        w.OkBtn.Content = "Completa";
        return w;
    }

    /// <summary>Dialog per respingere un task (nota consigliata).</summary>
    public static WorkflowActionWindow Reject(string stepName, Window? owner)
    {
        var w = new WorkflowActionWindow("Respingi task", $"Step: {stepName} \u00b7 verra riaperto lo step precedente.", null, notesRequired: true) { Owner = owner };
        w.OkBtn.Content = "Respingi";
        return w;
    }

    /// <summary>Dialog per aggiungere una nota.</summary>
    public static WorkflowActionWindow Note(string stepName, Window? owner)
    {
        var w = new WorkflowActionWindow("Aggiungi nota", $"Step: {stepName}", null, notesRequired: true) { Owner = owner };
        w.OkBtn.Content = "Aggiungi";
        return w;
    }

    private static List<string> ParseLabels(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try
        {
            using var doc = JsonDocument.Parse(json);
            var labels = new List<string>();
            foreach (var el in doc.RootElement.EnumerateArray())
                if (el.TryGetProperty("Label", out var l) || el.TryGetProperty("label", out l))
                    labels.Add(l.GetString() ?? "");
            return labels.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        }
        catch { return []; }
    }
}
