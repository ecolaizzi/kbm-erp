using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KBM.Client.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class WorkflowConsoleView : UserControl
{
    private readonly ApiClient _api;
    private readonly FilterableGrid _grid;
    private List<ConsoleRow> _rows = new();

    private string _visibility = "All";
    private string _taskState = "Open";

    public event Action<long>? RequestOpenInstance;

    public WorkflowConsoleView(ApiClient api)
    {
        InitializeComponent();
        _api = api;

        _grid = new FilterableGrid();
        _grid.AddTextColumn("Processo", nameof(ConsoleRow.Number), width: 110);
        _grid.AddTextColumn("Titolo", nameof(ConsoleRow.Title), star: true);
        _grid.AddTextColumn("Step", nameof(ConsoleRow.StepName), width: 170);
        _grid.AddTextColumn("Tipo", nameof(ConsoleRow.StepTypeLabel), width: 95);
        _grid.AddTextColumn("Stato", nameof(ConsoleRow.TaskStateLabel), width: 95);
        _grid.AddTextColumn("Scadenza", nameof(ConsoleRow.DueLabel), width: 95);
        _grid.AddTextColumn("Preso da", nameof(ConsoleRow.TakenLabel), width: 90);
        _grid.SelectionChanged += OnSelectionChanged;

        Page.GridContent = _grid;
        Page.PageKey = "wf-console";
        Page.PageTitle = "Consolle workflow";
        Page.Subtitle = "I miei task da gestire \u00b7 doppio click per aprire il processo";
        Page.EnableWings();
        Page.AddToolbarButton(IconCatalog.Open, "Apri processo", true, (_, _) => OpenInstance(), Key.F3);
        Page.AddToolbarSeparator();
        Page.AddToolbarButton(IconCatalog.Enable, "Completa", false, async (_, _) => await CompleteTask(), Key.F9);
        Page.AddToolbarButton(IconCatalog.Disable, "Respingi", false, async (_, _) => await RejectTask());
        Page.AddToolbarSeparator();
        Page.AddToolbarButton(IconCatalog.New, "Prendi in carico", false, async (_, _) => await TakeTask());
        Page.AddToolbarButton(IconCatalog.Edit, "Rilascia", false, async (_, _) => await ReleaseTask());
        Page.AddToolbarButton(IconCatalog.Edit, "Nota", false, async (_, _) => await AddNote());
        Page.AddToolbarSeparator();
        Page.AddToolbarButton(IconCatalog.Refresh, "Tutti", false, async (_, _) => { _visibility = "All"; await LoadAsync(); });
        Page.AddToolbarButton(IconCatalog.Refresh, "Miei", false, async (_, _) => { _visibility = "Mine"; await LoadAsync(); });
        Page.AddToolbarButton(IconCatalog.Refresh, "Non assegnati", false, async (_, _) => { _visibility = "Unassigned"; await LoadAsync(); });
        Page.AddToolbarButton(IconCatalog.Refresh, "Tutti gli stati", false, async (_, _) => { _taskState = _taskState == "Open" ? "All" : "Open"; await LoadAsync(); });
        Page.AddToolbarButton(IconCatalog.Refresh, "Aggiorna", false, async (_, _) => await LoadAsync(), Key.F5);
        Page.AttachRecordNavigator(_grid);
        Page.AttachPersonalization(_grid, "wf-console");
        _grid.ExportName = "ConsolleWorkflow";
        _grid.RowActivated += _ => OpenInstance();

        Loaded += async (_, _) => await LoadAsync();
    }

    private void OnSelectionChanged(object? item)
    {
        if (item is not ConsoleRow r) return;
        Page.SetWingDetail(r.Title, new (string, string)[]
        {
            ("Processo", r.Number),
            ("Step", r.StepName),
            ("Tipo", r.StepTypeLabel),
            ("Stato task", r.TaskStateLabel),
            ("Stato processo", r.ProcessStateLabel),
            ("Scadenza", r.DueLabel),
            ("Assegnazione", r.AssigneeLabel),
            ("Oggetto collegato", r.LinkLabel),
        });
    }

    private ConsoleRow? Selected => _grid.SelectedItem as ConsoleRow;

    private void OpenInstance()
    {
        if (Selected is { } r) RequestOpenInstance?.Invoke(r.InstanceId);
        else MessageBox.Show("Seleziona un task.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task CompleteTask()
    {
        if (Selected is not { } r) { Info(); return; }
        var dlg = WorkflowActionWindow.Complete(r.StepName, r.Item.DecisionOptionsJson, Window.GetWindow(this));
        if (dlg.ShowDialog() != true) return;
        var res = await _api.CompleteWorkflowTaskAsync(r.TaskId, new CompleteTaskDto(dlg.Decision, dlg.Notes));
        await AfterAction(res, "Task completato.");
    }

    private async Task RejectTask()
    {
        if (Selected is not { } r) { Info(); return; }
        var dlg = WorkflowActionWindow.Reject(r.StepName, Window.GetWindow(this));
        if (dlg.ShowDialog() != true) return;
        var res = await _api.RejectWorkflowTaskAsync(r.TaskId, dlg.Notes);
        await AfterAction(res, "Task respinto.");
    }

    private async Task TakeTask()
    {
        if (Selected is not { } r) { Info(); return; }
        if (await _api.TakeWorkflowTaskAsync(r.TaskId)) { ToastService.Success("Task preso in carico."); await LoadAsync(); }
        else Page.ShowError("Operazione non riuscita.");
    }

    private async Task ReleaseTask()
    {
        if (Selected is not { } r) { Info(); return; }
        if (await _api.ReleaseWorkflowTaskAsync(r.TaskId)) { ToastService.Info("Task rilasciato."); await LoadAsync(); }
        else Page.ShowError("Operazione non riuscita.");
    }

    private async Task AddNote()
    {
        if (Selected is not { } r) { Info(); return; }
        var dlg = WorkflowActionWindow.Note(r.StepName, Window.GetWindow(this));
        if (dlg.ShowDialog() != true || string.IsNullOrWhiteSpace(dlg.Notes)) return;
        var res = await _api.NoteWorkflowTaskAsync(r.TaskId, dlg.Notes!);
        await AfterAction(res, "Nota aggiunta.");
    }

    private async Task AfterAction(ApiWriteResult res, string okMessage)
    {
        if (res.Ok) { Page.HideError(); ToastService.Success(okMessage); await LoadAsync(); }
        else Page.ShowError(res.Error ?? "Operazione non riuscita.");
    }

    private void Info() => MessageBox.Show("Seleziona un task.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);

    public Task ReloadAsync() => LoadAsync();

    private async Task LoadAsync()
    {
        Page.HideError();
        try
        {
            var items = await _api.GetWorkflowConsoleAsync("Open", _taskState, _visibility);
            if (items is null) { Page.ShowError("Impossibile caricare la consolle workflow."); return; }
            _rows = items.Select(i => new ConsoleRow(i)).ToList();
            _grid.SetItems(_rows);
            Page.RecordCountLabel = $"{_rows.Count} task";
            Page.PaginationLabel = $"Vista: {_visibility} \u00b7 stato {_taskState} \u00b7 {_rows.Count} task";
            Page.UpdateRecordNavigator();
        }
        catch { Page.ShowError("Errore di connessione API."); }
    }
}

public sealed class ConsoleRow
{
    public ConsoleRow(WorkflowConsoleItem item) => Item = item;
    public WorkflowConsoleItem Item { get; }

    public long TaskId => Item.TaskId;
    public long InstanceId => Item.InstanceId;
    public string Number => Item.Number;
    public string Title => Item.Title;
    public string StepName => Item.StepName;
    public string StepTypeLabel => Item.StepType switch { "Approval" => "Approvazione", "Decision" => "Decisione", _ => "Attivit\u00e0" };
    public string TaskStateLabel => Item.TaskState switch { "Completed" => "Completato", "Rejected" => "Respinto", "Cancelled" => "Annullato", _ => "Aperto" };
    public string ProcessStateLabel => Item.ProcessState switch { "Completed" => "Completato", "Cancelled" => "Annullato", "Suspended" => "Sospeso", _ => "Aperto" };
    public string DueLabel => Item.DueDate?.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture) ?? "-";
    public string TakenLabel => Item.TakenByUserId is null ? "-" : $"#{Item.TakenByUserId}";
    public string AssigneeLabel => Item.AssigneeType switch
    {
        "User" => Item.AssigneeUserId is null ? "Utente" : $"Utente #{Item.AssigneeUserId}",
        "Initiator" => "Iniziatore",
        _ => Item.AssigneeRoleId is null ? "Ruolo" : $"Ruolo #{Item.AssigneeRoleId}"
    };
    public string LinkLabel => string.IsNullOrWhiteSpace(Item.LinkedEntityType) ? "-" : $"{Item.LinkedEntityType} #{Item.LinkedEntityId}";
}
