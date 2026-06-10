using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KBM.Client.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class WorkflowDefinitionsView : UserControl
{
    private readonly ApiClient _api;
    private readonly FilterableGrid _grid;
    private List<DefRow> _rows = new();

    public event Action? RequestNew;
    public event Action<long>? RequestEdit;
    public event Action? RequestStart;

    public WorkflowDefinitionsView(ApiClient api)
    {
        InitializeComponent();
        _api = api;

        _grid = new FilterableGrid();
        _grid.AddTextColumn("Codice", nameof(DefRow.Code), width: 120);
        _grid.AddTextColumn("Nome", nameof(DefRow.Name), star: true);
        _grid.AddTextColumn("Oggetto", nameof(DefRow.TargetEntityType), width: 150);
        _grid.AddTextColumn("Step", nameof(DefRow.StepCount), width: 70);
        _grid.AddTextColumn("Stato", nameof(DefRow.StatusLabel), width: 110);
        _grid.SelectionChanged += OnSelectionChanged;

        Page.GridContent = _grid;
        Page.PageKey = "wf-definitions";
        Page.PageTitle = "Modelli di workflow";
        Page.Subtitle = "Template di processo \u00b7 step, approvazioni, campi e azioni";
        Page.EnableWings();
        Page.AddToolbarButton(IconCatalog.New, "Nuovo", true, (_, _) => RequestNew?.Invoke(), Key.F2);
        Page.AddToolbarButton(IconCatalog.Edit, "Modifica", false, (_, _) => EditSelected(), Key.F3);
        Page.AddToolbarSeparator();
        Page.AddToolbarButton(IconCatalog.Enable, "Attiva", false, async (_, _) => await SetStatus("Active"));
        Page.AddToolbarButton(IconCatalog.Disable, "Archivia", false, async (_, _) => await SetStatus("Archived"));
        Page.AddToolbarSeparator();
        Page.AddToolbarButton(IconCatalog.Open, "Avvia processo", false, (_, _) => RequestStart?.Invoke());
        Page.AddToolbarButton(IconCatalog.Refresh, "Aggiorna", false, async (_, _) => await LoadAsync(), Key.F5);
        Page.AttachRecordNavigator(_grid);
        Page.AttachPersonalization(_grid, "wf-definitions");
        _grid.ExportName = "ModelliWorkflow";
        _grid.RowActivated += _ => EditSelected();

        Loaded += async (_, _) => await LoadAsync();
    }

    private void OnSelectionChanged(object? item)
    {
        if (item is not DefRow d) return;
        Page.SetWingDetail(d.Name, new (string, string)[]
        {
            ("Codice", d.Code),
            ("Nome", d.Name),
            ("Oggetto collegato", string.IsNullOrWhiteSpace(d.TargetEntityType) ? "(generico)" : d.TargetEntityType!),
            ("N. step", d.StepCount.ToString()),
            ("Stato", d.StatusLabel),
        });
    }

    private void EditSelected()
    {
        if (_grid.SelectedItem is DefRow d) RequestEdit?.Invoke(d.Id);
        else MessageBox.Show("Seleziona un modello da modificare.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task SetStatus(string status)
    {
        if (_grid.SelectedItem is not DefRow d)
        {
            MessageBox.Show("Seleziona un modello.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (await _api.SetWorkflowDefinitionStatusAsync(d.Id, status))
        {
            ToastService.Success(status == "Active" ? "Modello attivato." : "Modello archiviato.");
            await LoadAsync();
        }
        else Page.ShowError("Operazione non riuscita.");
    }

    public Task ReloadAsync() => LoadAsync();

    private async Task LoadAsync()
    {
        Page.HideError();
        try
        {
            var defs = await _api.GetWorkflowDefinitionsAsync();
            if (defs is null) { Page.ShowError("Impossibile caricare i modelli."); return; }
            _rows = defs.Select(d => new DefRow(d)).ToList();
            _grid.SetItems(_rows);
            Page.RecordCountLabel = $"{_rows.Count} modelli";
            Page.PaginationLabel = $"Tot: {_rows.Count} record";
            Page.UpdateRecordNavigator();
        }
        catch { Page.ShowError("Errore di connessione API."); }
    }
}

public sealed class DefRow
{
    public DefRow(WorkflowDefinitionListItem d)
    {
        Id = d.Id; Code = d.Code; Name = d.Name; TargetEntityType = d.TargetEntityType; StepCount = d.StepCount; Status = d.Status;
    }
    public long Id { get; }
    public string Code { get; }
    public string Name { get; }
    public string? TargetEntityType { get; }
    public int StepCount { get; }
    public string Status { get; }
    public string StatusLabel => Status switch { "Active" => "Attivo", "Archived" => "Archiviato", _ => "Bozza" };
}
