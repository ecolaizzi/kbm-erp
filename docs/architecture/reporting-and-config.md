# KBM — Reporting framework, configurazione tecnica e modalita sviluppatore

## Obiettivi

- Motore di stampa **pluggable** per report (PDF moderno, stampa unione Word, predisposizione Crystal Reports).
- Configurazioni **a livello di azienda utilizzatrice** e **tecniche** (motore/percorso/nome report)
  raggiungibili dal programmatore tramite una **gesture nascosta** e protette da permessi dedicati.

## Reporting

### Astrazione (Clean Architecture)

Contratti in `KBM.Application/Reporting`:

- `ReportDocumentModel` — modello documento **generico e tabellare** (testata a campi
  etichetta/valore, colonne, righe, totali, footer), indipendente dal motore.
- `IReportEngine` — un motore per ogni `ReportEngineType`.
- `IReportingService` — risolve la `ReportDefinition` per chiave/azienda e delega al motore.
- `IReportDefinitionProvider` — risolve la definizione (prima specifica dell'azienda, poi globale).

Implementazioni in `KBM.Reporting` (progetto infrastrutturale):

| Motore (`ReportEngineType`) | Stato | Tecnologia |
|------------------------------|-------|------------|
| `QuestPdf` | Attivo (default) | QuestPDF (PDF A4 code-based) |
| `WordMailMerge` | Attivo | DocumentFormat.OpenXml su template `.docx` |
| `Crystal` | Segnaposto | non supportato nativamente su .NET 8 |

> **Crystal Reports**: non gira in-process su .NET 8. Il provider e gia predisposto
> (`CrystalReportEngine`) e si attivera tramite un host out-of-process .NET Framework /
> Report Server senza modificare i chiamanti.

### Definizione report (`ReportDefinition`)

Persistita su DB (config tecnica): `Key`, `Title`, `Engine`, `OutputFormat`,
`TemplatePathOrName`, `Enabled`, `CompanyId` (null = globale).
Report predefiniti seedati all'avvio: `rda.print`, `rdo.print` (QuestPDF/PDF).

### Priorità di risoluzione e report polivalenti (stile Cube)

Ispirato a [Nuove funzionalità comuni CUBE](https://servizi.ntsinformatica.it/BusHelpWs/helpnet2017sr5/html/pc09.htm)
(priorità tra report standard e `\PERS`, report polivalenti `*x.rpt`):

- **Priorità di risoluzione** in `IReportDefinitionProvider`: definizione **specifica dell'azienda**
  (`CompanyId = corrente`) ha precedenza su quella **globale** (`CompanyId = null`). È l'equivalente
  della precedenza `rpt\pers` sullo standard di Cube.
- **Report polivalenti**: un'unica `ReportDefinition` per documento (es. `rda.print`) serve euro/valuta/IVA
  inclusa e multilingua, evitando la proliferazione di varianti — analogo all'unificazione dei report
  `*x.rpt` di Cube.
- **Override per azienda**: per personalizzare layout/motore/template di un report basta creare una
  `ReportDefinition` con la stessa `Key` e la `CompanyId` dell'azienda; lo standard resta invariato.

### Stampa unione (token Word)

Nel template `.docx`: `{{Title}}`, `{{Subtitle}}`, `{{CompanyName}}`, `{{Footer}}`,
`{{Campo:Etichetta}}` (campo di testata), `{{Lines}}` (righe tabulate).

### API

`POST /api/reports/{key}` — il client invia il `ReportDocumentModel`; il server risolve
motore/template dalla `ReportDefinition` e restituisce il file (PDF/DOCX). Il client lo
apre con l'applicazione predefinita.

### Anteprima di stampa interna (stile Business Cube, pc10)

Riferimento NTS: [Anteprima report di stampa](https://servizi.ntsinformatica.it/BusHelpWs/helpnet2017sr5/html/pc10.htm).

`ReportPreviewWindow` (client) mostra il documento in **anteprima paginata** non bloccante con
le funzioni della toolbar di Business:

- **Visualizzazione** via `FlowDocumentReader`: **zoom**, **navigazione pagine**, **Trova testo** (Ctrl+F), modalità pagina.
- **Stampa** (Ctrl+P) tramite `PrintDialog` (impagina un `FlowDocument` adattato alla stampante scelta).
- **Esporta**: **PDF** (motore server via `POST /api/reports/{key}`), **Excel `.xlsx` / CSV / HTML** (client, `GridExporter`).
- Il documento è costruito da `ReportFlowBuilder` a partire dal `ReportDocumentModel` (testata, tabella zebrata, totali, piè di pagina) → stesso modello del rendering server.

Le stampe RDA/RDO e gli elenchi dei moduli aprono questa anteprima invece del file esterno.

### Reportistica interna dei moduli (elenchi)

Ogni lista densa (`FilterableGrid`) può produrre la **stampa dell'elenco corrente** (record
filtrati/ordinati, colonne visibili nell'ordine del layout attivo) via `FilterableGrid.BuildReportModel(...)`
e il pulsante **Stampa (F6)**. Report predefiniti seedati (`EnsureDefaultReportsAsync`):

| Chiave | Modulo | Contenuto |
|--------|--------|-----------|
| `items.catalog` | Articoli / Magazzino | Listino / catalogo articoli |
| `items.inventory` | Articoli / Magazzino | Situazione articoli di magazzino |
| `customers.list` | Clienti | Elenco clienti |
| `suppliers.list` | Fornitori | Elenco fornitori |
| `list.print` | generico | Stampa elenco di qualsiasi griglia |

Ogni chiave è una `ReportDefinition` globale (motore QuestPDF/PDF) personalizzabile per azienda
(override con stessa `Key` e `CompanyId`) dalla modalità sviluppatore.

## Configurazione (`SystemSetting`)

Chiave/valore con `CompanyId` (null = tecnica globale), `Category`, `Description`.
Due ambiti nella UI sviluppatore:

- **Configurazione azienda** — impostazioni dell'azienda utilizzatrice corrente.
- **Impostazioni tecniche** — globali (percorsi, motori, parametri di sistema).

## Modalita sviluppatore (gesture nascosta)

1. Sul logo dell'header: **Ctrl + Shift + 3 click** entro 1,2s.
2. Verifica server-side `GET /api/system-config/can-access` (permesso `system.developer.access`).
3. Se autorizzato si apre `DeveloperSettingsWindow` (report/motori, config azienda, impostazioni tecniche, guida stampa unione). Altrimenti nessun effetto visibile.

Endpoint config (tutti gated `system.config.*`): `GET/PUT/DELETE /api/system-config/settings`,
`GET/PUT/DELETE /api/system-config/reports`.

## RDA/RDO — logica business e usabilita

### Selezione articoli da catalogo
Negli editor RDA e RDO il pulsante **Da articolo** apre `ItemPickerWindow`
(ricerca per codice/descrizione, multi-selezione): per ogni articolo scelto viene
aggiunta una riga con codice, descrizione, UM e prezzo precompilati.

### Fornitori candidati per riga (RDA)
Ogni riga RDA puo avere piu **fornitori da interpellare** (`SupplierMultiPickerWindow`).
La generazione **RDA→RDO** rispetta questi fornitori: per il fornitore scelto vengono
incluse solo le righe a lui associate (se nessuna riga ha fornitori, vengono incluse tutte).

### Totali e importi (RDO)
Colonna **Importo** calcolata in tempo reale: `Q.ta x Prezzo x (1 - Sconto%)`.
Footer con **Totale imponibile** (solo righe disponibili), riportato anche in stampa.

### Workflow e blocchi di stato
- **RDA**: `Generato → Confermato → EmessaRDO → Chiuso`. Stato `Chiuso` ⇒ documento in sola lettura.
- **RDO**: `Generato → Inviata → OffertaRicevuta → Confermato → Annullato`. Stati `Confermato`/`Annullato` ⇒ sola lettura.
- Passaggio a `OffertaRicevuta`/`Confermato` ⇒ prezzo unitario obbligatorio su tutte le righe.
- Le stesse regole sono applicate **lato server** (difesa in profondita) oltre che nella UI.

## Sicurezza

Vedi `docs/security/authorization.md` per i permessi `system.*` e del ciclo passivo.
