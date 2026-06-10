namespace KBM.Domain.Entities;

/// <summary>Tipo di step di un modello di workflow.</summary>
public enum WorkflowStepType
{
    /// <summary>Attivit&#224; operativa da completare.</summary>
    Task = 0,
    /// <summary>Approvazione (esito implicito approva/avanza).</summary>
    Approval = 1,
    /// <summary>Decisione con pi&#249; esiti possibili che instradano il flusso.</summary>
    Decision = 2
}

/// <summary>Come viene assegnato un task generato dallo step.</summary>
public enum WorkflowAssigneeType
{
    /// <summary>Utente specifico.</summary>
    User = 0,
    /// <summary>Ruolo (task non assegnato finch&#233; non preso in carico da un utente del ruolo).</summary>
    Role = 1,
    /// <summary>Chi ha avviato il processo.</summary>
    Initiator = 2
}

/// <summary>Stato del processo (istanza).</summary>
public enum WorkflowInstanceState
{
    Open = 0,
    Completed = 1,
    Cancelled = 2,
    Suspended = 3
}

/// <summary>Stato del singolo task.</summary>
public enum WorkflowTaskState
{
    Open = 0,
    Completed = 1,
    Rejected = 2,
    Cancelled = 3
}
