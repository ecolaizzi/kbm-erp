# Modulo Workflow (motore di processo configurabile)

**Versione**: 1.0 · **Data**: 2026-06-10
**Ispirazione**: NTS Business Cube — [Consolle Workflow (`bswfcons.htm`)](https://servizi.ntsinformatica.it/BusHelpWs/helpnet2017sr5/html/bswfcons.htm)

Sistema di workflow integrato per modellare **processi aziendali guidati con
approvazioni** di figure diverse, standardizzabili tramite campi e azioni definite nel
modello (o impostate a runtime).

## Concetti

| Concetto | Entità | Descrizione |
|---|---|---|
| **Modello** (template) | `WorkflowDefinition` + `WorkflowStep` | Processo riusabile: step ordinati, tipo step, assegnatari, SLA, campi e azioni |
| **Processo** (istanza) | `WorkflowInstance` | Esecuzione di un modello, con stato e oggetto gestionale collegato |
| **Task** | `WorkflowTask` | Attività generata da uno step, assegnata a utente/ruolo/iniziatore |
| **Flusso** (storico) | `WorkflowEvent` | Tracciamento di tutte le attività (avvio, completamenti, respingimenti, note, azioni) |

## Step

- **Tipo**: `Task` (attività), `Approval` (approvazione), `Decision` (decisione multi-esito).
- **Assegnazione**: `User` (utente), `Role` (ruolo — task non assegnato finché non preso in
  carico), `Initiator` (chi ha avviato).
- **SLA**: `DueDays` → scadenza del task.
- **Decisioni** (`DecisionOptionsJson`): lista di `{Label, Outcome}` con `Outcome` ∈
  `Next` (avanza), `Reject` (riapre lo step precedente), `Complete` (chiude il processo).
- **Azioni** (`ActionsJson`): lista di `{Type, Value}` eseguite al completamento dello step.

### Azioni supportate (integrazione moduli)

| Type | Effetto |
|---|---|
| `SetEntityStatus` | Imposta lo stato dell'oggetto gestionale collegato (es. RDA/RDO → `Approvato`). È il punto di integrazione con i moduli. |
| `Note` | Aggiunge una nota al flusso del processo. |

## Motore (`WorkflowEngineService`)

- **Start**: crea l'istanza (numero `WF/anno/####`), genera il primo task, registra l'evento.
- **Complete**: valida autorizzazione (utente assegnato o membro del ruolo), registra esito
  e note, esegue le azioni dello step, quindi instrada:
  - decisione → secondo l'`Outcome` scelto;
  - altrimenti → step successivo per ordine; se assente → processo completato.
- **Reject**: respinge il task e **riapre lo step precedente** (logica Cube).
- **Take charge / Release**: presa in carico/rilascio dei task assegnati a ruolo.
- **Note**: aggiunta note al flusso.
- **Stato processo**: `cancel`, `suspend`, `reactivate`, `force-close`.

## Consolle (`WorkflowConsoleService`)

Inbox dei task con filtri stile Business Cube:

- **Stato processo**: Tutti / Aperto / Completato / Annullato / Sospeso.
- **Stato task**: Tutti / Aperto / Completato / Respinto.
- **Visibilità**: Tutti (assegnati a me + miei ruoli), Miei (diretti), Non assegnati
  (task di ruolo non ancora presi in carico).

## Sicurezza (RBAC)

| Permesso | Significato |
|---|---|
| `workflow.read` | Visualizzare consolle, processi e modelli |
| `workflow.participate` | Operare sui task (completa/respingi/prendi in carico/note) |
| `workflow.start` | Avviare processi |
| `workflow.manage` | Creare/modificare e attivare modelli |
| `workflow.admin` | Annullare/sospendere/chiudere forzatamente i processi |

## UI

Gruppo di navigazione **Workflow**: **Consolle** (WFCON), **Processi** (WFPRO),
**Modelli** (WFMOD). I modelli si progettano con una griglia step editabile (tipo,
assegnatario, SLA, decisioni e azioni in JSON); i processi si avviano collegando
opzionalmente un oggetto gestionale (RDA/RDO).

## Estensioni previste

- Diagramma di flusso/stato visuale (sezioni Cube "Diagramma di flusso/stato").
- Allegati ai task e task custom/previsionali.
- Trigger automatici (avvio processo all'evento di un modulo) e notifiche.
- Routing condizionale sui valori dei campi configurabili.
