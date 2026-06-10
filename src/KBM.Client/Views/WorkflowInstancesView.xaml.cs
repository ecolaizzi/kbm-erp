using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KBM.Client.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class WorkflowInstancesView : UserControl
{
    private readonly ApiClient _api;
    private readonly FilterableGrid _grid;
    private List<InstanceRow> _rows = new();

    public WorkflowInstancesView(ApiClient api)
    {
        InitializeComponent();
        _api = api;

        _grid = new FilterableGrid();
        _grid.AddTextColumn("Numero", nameof(InstanceRow.Number), width: 120);
        _grid.AddTextColumn("Titolo", nameof(InstanceRow.Title), star: true);
        _grid.AddTextColumn("Modello", nameof(InstanceRow.DefinitionName), width: 160);
        _grid.AddTextColumn("Step corrente", nameof(InstanceRow.CurrentStepName), width: 150);
        _grid.AddTextColumn("Stato", nameof(InstanceRow.StateLabel), width: 100);
        _grid.AddTextColumn("Avviato", nameof(InstanceRow.StartedLabel), width: 100);
        _grid.AddTextColumn("Collegato", nameof(InstanceRow.LinkLabel), width: 140);
        _grid.SelectionChanged += async item => await OnSelectionChanged(item);

        Page.GridContent = _grid;
        Page.PageKey = "wf-instances";
        Page.PageTitle = "Processi";
        Page.Subtitle = "Processi avviati \u00b7 stato, flusso e azioni amministrative";
        Page.EnableWings();
        Page.AddToolbarButton(IconCatalog.New, "Avvia processo", true, async (_, _) => await StartProcess(), Key.F2);
        Page.AddToolbarSeparator();
        Page.AddToolbarButton(IconCatalog.Lock, "Sospendi", false, async (_, _) => await SetState("suspend"));
        Page.AddToolbarButton(IconCatalog.Enable, "Riattiva", false, async (_, _) => await SetState("reactivate"));
        Page.AddToolbarButton(IconCatalog.Disable, "Annulla", false, async (_, _) => await SetState("cancel"));
        Page.AddToolbarButton(IconCatalog.Save, "Chiudi forzato", false, async (_, _) => await SetState("force-close"));
        Page.AddToolbarSeparator();
        Page.AddToolbarButton(IconCatalog.Refresh, "Aggiorna", false, async (_, _) => await LoadAsync(), Key.F5);
        Page.AttachRecordNavigator(_grid);
        Page.AttachPersonalization(_grid, "wf-instances");
        _grid.ExportName = "ProcessiWorkflow";

        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task OnSelectionChanged(object? item)
    {
        if (item is not InstanceRow r) return;
        var detail = await _api.GetWorkflowInstanceAsync(r.Id);
        var fields = new List<(string, string)>
        {
            ("Numero", r.Number),
            ("Titolo", r.Title),
            ("Modello", r.DefinitionName),
            ("Stato", r.StateLabel),
            ("Step corrente", string.IsNullOrWhiteSpace(r.CurrentStepName) ? "-" : r.CurrentStepName!),
            ("Collegato", r.LinkLabel),
        };
        if (detail is not null)
        {
            foreach (var ev in detail.Events.OrderByDescending(e => e.Timestamp).Take(8))
                fields.Add(($"{ev.Timestamp:dd/MM HH:mm} {ev.Action}", ev.Notes ?? "-"));
        }
        Page.SetWingDetail($"Flusso \u00b7 {r.Number}", fields.ToArray());
    }

    private InstanceRow? Selected => _grid.SelectedItem as InstanceRow;

    private async Task StartProcess()
    {
        var dto = await WorkflowStartWindow.PromptAsync(_api, Window.GetWindow(this));
        if (dto is null) return;
        var res = await _api.StartWorkflowAsync(dto);
        if (res.Ok) { ToastService.Success("Processo avviato."); await LoadAsync(); }
        else Page.ShowError(res.Error ?? "Avvio non riuscito.");
    }

    private async Task SetState(string action)
    {
        if (Selected is not { } r)
        {
            MessageBox.Show("Seleziona un processo.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var res = await _api.SetWorkflowInstanceStateAsync(r.Id, action, null);
        if (res.Ok) { ToastService.Success("Operazione eseguita."); await LoadAsync(); }
        else Page.ShowError(res.Error ?? "Operazione non riuscita.");
    }

    public async Task SelectInstanceAsync(long instanceId)
    {
        await LoadAsync();
        var row = _rows.FirstOrDefault(r => r.Id == instanceId);
        if (row is not null) _grid.SelectItem(row);
    }

    public Task ReloadAsync() => LoadAsync();

    private async Task LoadAsync()
    {
        Page.HideError();
        try
        {
            var items = await _api.GetWorkflowInstancesAsync();
            if (items is null) { Page.ShowError("Impossibile caricare i processi."); return; }
            _rows = items.Select(i => new InstanceRow(i)).ToList();
            _grid.SetItems(_rows);
            Page.RecordCountLabel = $"{_rows.Count} processi";
            Page.PaginationLabel = $"Tot: {_rows.Count} record";
            Page.UpdateRecordNavigator();
        }
        catch { Page.ShowError("Errore di connessione API."); }
    }
}

public sealed class InstanceRow
{
    public InstanceRow(WorkflowInstanceListItem i)
    {
        Id = i.Id; Number = i.Number; Title = i.Title; DefinitionName = i.DefinitionName;
        State = i.State; CurrentStepName = i.CurrentStepName; StartedAt = i.StartedAt;
        LinkedEntityType = i.LinkedEntityType; LinkedEntityId = i.LinkedEntityId;
    }
    public long Id { get; }
    public string Number { get; }
    public string Title { get; }
    public string DefinitionName { get; }
    public string State { get; }
    public string? CurrentStepName { get; }
    public DateTime StartedAt { get; }
    public string? LinkedEntityType { get; }
    public long? LinkedEntityId { get; }

    public string StateLabel => State switch { "Completed" => "Completato", "Cancelled" => "Annullato", "Suspended" => "Sospeso", _ => "Aperto" };
    public string StartedLabel => StartedAt.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture);
    public string LinkLabel => string.IsNullOrWhiteSpace(LinkedEntityType) ? "-" : $"{LinkedEntityType} #{LinkedEntityId}";
}
