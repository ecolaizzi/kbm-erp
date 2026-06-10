using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using KBM.Client.Controls;
using KBM.Client.Services;

namespace KBM.Client.Views;

public partial class WorkflowDefinitionEditView : UserControl
{
    private static readonly string[] TypeOptions = ["Attivit\u00e0", "Approvazione", "Decisione"];
    private static readonly string[] AssigneeOptions = ["Ruolo", "Utente", "Iniziatore"];

    private readonly ApiClient _api;
    private readonly long? _id;
    private bool IsEdit => _id.HasValue;

    private readonly FilterableGrid _grid;
    private List<StepRow> _steps = new();

    public event Action? Saved;
    public event Action? Cancelled;

    public WorkflowDefinitionEditView(ApiClient api, long? id)
    {
        InitializeComponent();
        _api = api;
        _id = id;

        _grid = new FilterableGrid();
        _grid.AddTextColumn("#", nameof(StepRow.Order), width: 44, editable: true, realtime: true);
        _grid.AddTextColumn("Codice", nameof(StepRow.Code), width: 90, editable: true, realtime: true);
        _grid.AddTextColumn("Nome", nameof(StepRow.Name), star: true, editable: true, realtime: true);
        _grid.AddComboColumn("Tipo", nameof(StepRow.TypeLabel), TypeOptions, width: 120);
        _grid.AddComboColumn("Assegna", nameof(StepRow.AssigneeLabel), AssigneeOptions, width: 110);
        _grid.AddTextColumn("RuoloId", nameof(StepRow.RoleId), width: 70, editable: true, realtime: true);
        _grid.AddTextColumn("UtenteId", nameof(StepRow.UserId), width: 70, editable: true, realtime: true);
        _grid.AddTextColumn("SLA gg", nameof(StepRow.DueDays), width: 60, editable: true, realtime: true);
        _grid.AddTextColumn("Decisioni (JSON)", nameof(StepRow.DecisionOptionsJson), width: 200, editable: true, realtime: true);
        _grid.AddTextColumn("Azioni (JSON)", nameof(StepRow.ActionsJson), width: 200, editable: true, realtime: true);
        StepsHost.Child = _grid;

        if (IsEdit) FormTitle.Text = "Modifica modello di workflow";

        FormShortcuts.Apply(this,
            save: () => Save_Click(this, new RoutedEventArgs()),
            exit: () => Cancel_Click(this, new RoutedEventArgs()));

        Loaded += async (_, _) =>
        {
            if (IsEdit) await LoadAsync();
            else { _steps = new List<StepRow> { NewStep(1) }; _grid.SetItems(_steps); }
        };
    }

    private async Task LoadAsync()
    {
        HideError();
        try
        {
            var d = await _api.GetWorkflowDefinitionAsync(_id!.Value);
            if (d is null) { ShowError("Impossibile caricare il modello."); return; }
            CodeBox.Text = d.Code;
            CodeBox.IsEnabled = false;
            NameBox.Text = d.Name;
            FieldsBox.Text = d.FieldsJson ?? "";
            FormSubtitle.Text = $"{d.Code} \u00b7 {d.Name}";
            SelectStatus(d.Status);
            _steps = d.Steps.OrderBy(s => s.StepOrder).Select(s => new StepRow(s)).ToList();
            if (_steps.Count == 0) _steps.Add(NewStep(1));
            _grid.SetItems(_steps);
        }
        catch { ShowError("Errore di connessione API."); }
    }

    private void SelectStatus(string status)
    {
        foreach (var item in StatusCombo.Items)
            if (item is ComboBoxItem cbi && (string)cbi.Tag == status) { StatusCombo.SelectedItem = cbi; return; }
        StatusCombo.SelectedIndex = 0;
    }

    private static StepRow NewStep(int order) => new()
    {
        Order = order.ToString(CultureInfo.InvariantCulture),
        Code = $"S{order}",
        Name = "",
        TypeLabel = "Approvazione",
        AssigneeLabel = "Ruolo"
    };

    private void AddStep_Click(object sender, RoutedEventArgs e)
    {
        _steps.Add(NewStep(_steps.Count + 1));
        _grid.SetItems(_steps.ToList());
    }

    private void RemoveStep_Click(object sender, RoutedEventArgs e)
    {
        if (_grid.SelectedItem is StepRow r) { _steps.Remove(r); _grid.SetItems(_steps.ToList()); }
        else MessageBox.Show("Seleziona uno step da rimuovere.", "KBM", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        HideError();
        var name = NameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name)) { ShowError("Il nome \u00e8 obbligatorio."); return; }
        if (_steps.Count == 0) { ShowError("Definisci almeno uno step."); return; }

        var status = (StatusCombo.SelectedItem as ComboBoxItem)?.Tag as string ?? "Draft";
        var fields = string.IsNullOrWhiteSpace(FieldsBox.Text) ? null : FieldsBox.Text.Trim();

        var ordered = _steps.OrderBy(s => ParseInt(s.Order, int.MaxValue)).ToList();
        var stepInputs = ordered.Select((s, idx) => new WorkflowStepInputDto(
            s.Id, idx + 1,
            string.IsNullOrWhiteSpace(s.Code) ? $"S{idx + 1}" : s.Code.Trim(),
            s.Name?.Trim() ?? "",
            TypeToInt(s.TypeLabel),
            AssigneeToInt(s.AssigneeLabel),
            ParseNullableLong(s.UserId),
            ParseNullableLong(s.RoleId),
            ParseNullableInt(s.DueDays),
            string.IsNullOrWhiteSpace(s.DecisionOptionsJson) ? null : s.DecisionOptionsJson!.Trim(),
            string.IsNullOrWhiteSpace(s.ActionsJson) ? null : s.ActionsJson!.Trim())).ToList();

        SaveBtn.IsEnabled = false;
        try
        {
            ApiWriteResult result;
            if (IsEdit)
                result = await _api.UpdateWorkflowDefinitionAsync(_id!.Value,
                    new UpdateWorkflowDefinitionDto(name, null, null, status, fields, stepInputs));
            else
            {
                var code = CodeBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(code)) { ShowError("Il codice \u00e8 obbligatorio."); return; }
                result = await _api.CreateWorkflowDefinitionAsync(
                    new CreateWorkflowDefinitionDto(code, name, null, null, fields, stepInputs));
            }

            if (result.Ok) { ToastService.Success("Modello salvato."); Saved?.Invoke(); }
            else ShowError(result.Error ?? "Salvataggio non riuscito.");
        }
        catch { ShowError("Errore di connessione API durante il salvataggio."); }
        finally { SaveBtn.IsEnabled = true; }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Cancelled?.Invoke();

    private static int TypeToInt(string? label) => label switch { "Attivit\u00e0" => 0, "Decisione" => 2, _ => 1 };
    private static int AssigneeToInt(string? label) => label switch { "Utente" => 0, "Iniziatore" => 2, _ => 1 };

    private static int ParseInt(string? t, int fallback) => int.TryParse((t ?? "").Trim(), out var v) ? v : fallback;
    private static int? ParseNullableInt(string? t) => int.TryParse((t ?? "").Trim(), out var v) && v > 0 ? v : null;
    private static long? ParseNullableLong(string? t) => long.TryParse((t ?? "").Trim(), out var v) && v > 0 ? v : null;

    private void ShowError(string message) { ErrorText.Text = message; ErrorBox.Visibility = Visibility.Visible; }
    private void HideError() => ErrorBox.Visibility = Visibility.Collapsed;
}

public sealed class StepRow : INotifyPropertyChanged
{
    public StepRow() { }

    public StepRow(WorkflowStepDto s)
    {
        Id = s.Id;
        _order = s.StepOrder.ToString(CultureInfo.InvariantCulture);
        _code = s.Code;
        _name = s.Name;
        _typeLabel = s.StepType switch { 0 => "Attivit\u00e0", 2 => "Decisione", _ => "Approvazione" };
        _assigneeLabel = s.AssigneeType switch { 0 => "Utente", 2 => "Iniziatore", _ => "Ruolo" };
        _roleId = s.AssigneeRoleId?.ToString(CultureInfo.InvariantCulture) ?? "";
        _userId = s.AssigneeUserId?.ToString(CultureInfo.InvariantCulture) ?? "";
        _dueDays = s.DueDays?.ToString(CultureInfo.InvariantCulture) ?? "";
        _decision = s.DecisionOptionsJson ?? "";
        _actions = s.ActionsJson ?? "";
    }

    public long? Id { get; }

    private string _order = "1";
    private string _code = "";
    private string _name = "";
    private string _typeLabel = "Approvazione";
    private string _assigneeLabel = "Ruolo";
    private string _roleId = "";
    private string _userId = "";
    private string _dueDays = "";
    private string _decision = "";
    private string _actions = "";

    public string Order { get => _order; set { _order = value; On(nameof(Order)); } }
    public string Code { get => _code; set { _code = value; On(nameof(Code)); } }
    public string Name { get => _name; set { _name = value; On(nameof(Name)); } }
    public string TypeLabel { get => _typeLabel; set { _typeLabel = value; On(nameof(TypeLabel)); } }
    public string AssigneeLabel { get => _assigneeLabel; set { _assigneeLabel = value; On(nameof(AssigneeLabel)); } }
    public string RoleId { get => _roleId; set { _roleId = value; On(nameof(RoleId)); } }
    public string UserId { get => _userId; set { _userId = value; On(nameof(UserId)); } }
    public string DueDays { get => _dueDays; set { _dueDays = value; On(nameof(DueDays)); } }
    public string DecisionOptionsJson { get => _decision; set { _decision = value; On(nameof(DecisionOptionsJson)); } }
    public string ActionsJson { get => _actions; set { _actions = value; On(nameof(ActionsJson)); } }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void On(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
}
