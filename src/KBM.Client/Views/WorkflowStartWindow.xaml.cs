using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class WorkflowStartWindow : Window
{
    public StartWorkflowDto? Result { get; private set; }

    private WorkflowStartWindow(IReadOnlyList<WorkflowDefinitionListItem> activeDefinitions, string? presetLinkType, long? presetLinkId)
    {
        InitializeComponent();
        DefinitionCombo.ItemsSource = activeDefinitions;
        if (activeDefinitions.Count > 0) DefinitionCombo.SelectedIndex = 0;

        if (!string.IsNullOrWhiteSpace(presetLinkType))
        {
            foreach (var item in LinkTypeCombo.Items)
                if (item is ComboBoxItem cbi && (string)cbi.Tag == presetLinkType) { LinkTypeCombo.SelectedItem = cbi; break; }
            if (presetLinkId is > 0) LinkIdBox.Text = presetLinkId.Value.ToString(CultureInfo.InvariantCulture);
        }
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (DefinitionCombo.SelectedValue is not long defId)
        {
            MessageBox.Show("Seleziona un modello.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var linkType = (LinkTypeCombo.SelectedItem as ComboBoxItem)?.Tag as string;
        long? linkId = long.TryParse(LinkIdBox.Text?.Trim(), out var v) && v > 0 ? v : null;

        Result = new StartWorkflowDto(
            defId,
            string.IsNullOrWhiteSpace(TitleBox.Text) ? null : TitleBox.Text.Trim(),
            string.IsNullOrWhiteSpace(linkType) ? null : linkType,
            linkId,
            null);
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    /// <summary>Carica i modelli attivi e mostra la finestra di avvio.</summary>
    public static async Task<StartWorkflowDto?> PromptAsync(ApiClient api, Window? owner, string? presetLinkType = null, long? presetLinkId = null)
    {
        var all = await api.GetWorkflowDefinitionsAsync() ?? new List<WorkflowDefinitionListItem>();
        var active = all.Where(d => d.Status == "Active").ToList();
        if (active.Count == 0)
        {
            MessageBox.Show("Nessun modello di workflow attivo. Crea e attiva un modello prima di avviare un processo.",
                "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
            return null;
        }
        var w = new WorkflowStartWindow(active, presetLinkType, presetLinkId) { Owner = owner };
        return w.ShowDialog() == true ? w.Result : null;
    }
}
