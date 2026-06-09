# KBM Component Library Plan

**Versione**: 1.0  
**Data**: 2026-06-09  
**Prefix**: `KBM` (evita conflitti con lib terze)  
**Status**: ✅ Draft

---

## 1. PRINCIPI COMPONENT LIBRARY

- **Prefix KBM**: ogni componente prefissato `KBM` (es. `KBMDataGrid`)
- **Composable**: componenti atomici combinabili in pattern complessi
- **Accessible**: ogni componente conforme WCAG 2.1 AA by default
- **Keyboard-first**: interazione completa via tastiera
- **Framework agnostic spec**: specifiche qui sono framework-agnostic; implementazione dipende da decisione Chief Architect (WPF/Avalonia/MAUI)

---

## 2. CATALOG COMPONENTI CORE

### 2.1 KBMDataGrid

**Scopo**: Griglia dati sortable, filtrable, paginata. Componente più usato in KBM — ogni vista lista lo utilizza.

| Prop | Tipo | Default | Descrizione |
|---|---|---|---|
| `columns` | `ColumnDef[]` | — | Definizione colonne (field, header, width, sortable, render) |
| `data` | `T[]` | `[]` | Array di record da visualizzare |
| `totalCount` | `number` | — | Totale record (per paginazione server-side) |
| `loading` | `bool` | `false` | Mostra skeleton loading |
| `pagination` | `PaginationConfig` | `{page:1, size:25}` | Configurazione paginazione |
| `onPageChange` | `(page, size) => void` | — | Callback cambio pagina |
| `onSortChange` | `(field, dir) => void` | — | Callback cambio ordinamento |
| `onRowClick` | `(row) => void` | — | Click su riga |
| `onRowDoubleClick` | `(row) => void` | — | Doppio click → modifica |
| `onSelectionChange` | `(rows[]) => void` | — | Selezione multipla |
| `selectedRows` | `T[]` | `[]` | Righe selezionate (controlled) |
| `selectable` | `bool` | `true` | Mostra checkbox selezione |
| `rowActions` | `RowAction[]` | `[]` | Azioni inline per riga (✏ 🗑) |
| `emptyMessage` | `string` | "Nessun risultato" | Messaggio vista vuota |
| `rowHeight` | `'compact' \| 'standard'` | `'compact'` | Altezza riga (32px / 40px) |
| `stickyHeader` | `bool` | `true` | Header colonne sticky |
| `zebra` | `bool` | `true` | Alternanza colore righe |
| `columnConfig` | `bool` | `true` | Mostra pulsante configura colonne |

**Comportamenti obbligatori**:
- Header click → sort ASC → DESC → nessuno
- Selezione multipla: checkbox + Shift+click range + Ctrl+click
- Context menu tasto destro: Apri / Modifica / Duplica / Elimina
- Arrow keys navigazione righe
- Column resize via drag bordo header
- Export Excel dei dati correnti (server-side)

---

### 2.2 KBMForm

**Scopo**: Container form con validazione, layout a griglia, gestione stato.

| Prop | Tipo | Default | Descrizione |
|---|---|---|---|
| `fields` | `FieldDef[]` | — | Definizione campi (name, label, type, required, validators) |
| `values` | `Record<string, any>` | — | Valori correnti (controlled) |
| `onChange` | `(field, value) => void` | — | Callback modifica campo |
| `onSubmit` | `(values) => void` | — | Callback submit |
| `onCancel` | `() => void` | — | Callback annulla |
| `mode` | `'view' \| 'edit' \| 'create'` | `'view'` | Modalità form |
| `validationSchema` | `ValidationSchema` | — | Schema validazione (required, pattern, min, max) |
| `errors` | `Record<string, string>` | `{}` | Errori validazione (da server o locale) |
| `loading` | `bool` | `false` | Mostra overlay loading |
| `columns` | `1 \| 2 \| 3 \| 4` | `2` | Colonne layout griglia |
| `dirty` | `bool` | `false` | Form ha modifiche non salvate |

**Tipi campo supportati**: `text`, `number`, `date`, `datetime`, `select`, `lookup`, `checkbox`, `textarea`, `currency`, `percentage`, `email`, `phone`, `iva`, `cf`, `readonly`, `hidden`

---

### 2.3 KBMToolbar

**Scopo**: Barra azioni contestuale sopra griglie e form.

| Prop | Tipo | Descrizione |
|---|---|---|
| `actions` | `ToolbarAction[]` | Array azioni; ogni azione: `{id, label, icon, shortcut, onClick, disabled, hidden, variant}` |
| `moreActions` | `ToolbarAction[]` | Azioni nel menu "Altro ▾" (secondary) |
| `separator` | `boolean[]` | Posizioni separatori tra gruppi |
| `loading` | `bool` | Disabilita toolbar durante operazioni |

**Variant azioni**: `primary` (bottone pieno), `secondary` (outline), `danger` (rosso per elimina), `ghost` (solo icona)

**Comportamento**: su schermi stretti o sidebar espansa, collassa label e mostra solo icone + tooltip.

---

### 2.4 KBMFilterPanel

**Scopo**: Pannello filtri avanzati multi-campo con builder logico.

| Prop | Tipo | Descrizione |
|---|---|---|
| `fields` | `FilterFieldDef[]` | Campi filtrabili (name, label, type, operators) |
| `value` | `FilterGroup` | Filtro corrente (struttura AND/OR) |
| `onChange` | `(filter) => void` | Callback modifica filtro |
| `onApply` | `(filter) => void` | Applica filtro |
| `onReset` | `() => void` | Resetta filtro |
| `savedFilters` | `SavedFilter[]` | Filtri salvati utente |
| `onSaveFilter` | `(name, filter) => void` | Salva filtro con nome |
| `onDeleteFilter` | `(id) => void` | Elimina filtro salvato |
| `open` | `bool` | Pannello aperto/chiuso |

**Operatori per tipo**:
- Testo: `contiene`, `non contiene`, `uguale`, `inizia con`, `finisce con`, `vuoto`, `non vuoto`
- Numero: `=`, `≠`, `>`, `<`, `≥`, `≤`, `tra`
- Data: `=`, `prima di`, `dopo di`, `tra`, `questo mese`, `scorso mese`, `questo anno`
- Booleano: `sì`, `no`
- Select: `è`, `non è`, `uno di`, `nessuno di`

---

### 2.5 KBMSearchBox

**Scopo**: Campo ricerca testuale con debounce, integrato nella toolbar della griglia.

| Prop | Tipo | Default | Descrizione |
|---|---|---|---|
| `placeholder` | `string` | "Cerca..." | Testo placeholder |
| `value` | `string` | `''` | Valore corrente (controlled) |
| `onSearch` | `(term: string) => void` | — | Callback ricerca |
| `debounce` | `number` | `300` | Debounce ms |
| `minLength` | `number` | `2` | Caratteri minimi per trigger |
| `loading` | `bool` | `false` | Mostra spinner |
| `clearable` | `bool` | `true` | Bottone svuota campo |
| `width` | `string` | `'240px'` | Larghezza campo |

---

### 2.6 KBMLookup

**Scopo**: Selettore entità con autocomplete. Usato per foreign key (cliente, articolo, fornitore...).

| Prop | Tipo | Descrizione |
|---|---|---|
| `entity` | `string` | Tipo entità (`'clienti'`, `'articoli'`, `'fornitori'`...) |
| `value` | `EntityRef \| null` | Selezione corrente `{id, code, label}` |
| `onChange` | `(ref) => void` | Callback selezione |
| `onSearch` | `(term) => Promise<EntityRef[]>` | Funzione ricerca (debounced) |
| `multi` | `bool` | Selezione multipla |
| `disabled` | `bool` | Campo disabilitato |
| `required` | `bool` | Campo obbligatorio |
| `displayFields` | `string[]` | Campi da mostrare nella preview (es. `['codice', 'ragioneSociale', 'piva']`) |
| `openModalShortcut` | `string` | Default: F4 |
| `modalTitle` | `string` | Titolo modale ricerca completa |

**Comportamento**:
- Digita testo → search-as-you-type (debounce 400ms)
- F4 → apre modale ricerca avanzata con KBMDataGrid
- Mostra codice + descrizione (mai solo codice)
- Campo vuoto = nessuna selezione (non null-breaking)

---

### 2.7 KBMDatePicker

**Scopo**: Selettore data con shortcuts, range e formato italiano.

| Prop | Tipo | Default | Descrizione |
|---|---|---|---|
| `value` | `Date \| null` | `null` | Data selezionata |
| `onChange` | `(date) => void` | — | Callback selezione |
| `range` | `bool` | `false` | Selezione intervallo date |
| `valueRange` | `[Date, Date] \| null` | — | Range selezionato |
| `onRangeChange` | `([from, to]) => void` | — | Callback range |
| `format` | `string` | `'dd/MM/yyyy'` | Formato visualizzazione |
| `shortcuts` | `DateShortcut[]` | default | Scorciatoie (Oggi, Ieri, Sett. scorsa, Questo mese) |
| `minDate` | `Date` | — | Data minima selezionabile |
| `maxDate` | `Date` | — | Data massima selezionabile |
| `disabled` | `bool` | `false` | — |
| `clearable` | `bool` | `true` | Bottone svuota |

---

### 2.8 KBMModal

**Scopo**: Dialog modale con focus trap, ARIA, bottoni azione.

| Prop | Tipo | Default | Descrizione |
|---|---|---|---|
| `open` | `bool` | `false` | Mostra/nasconde |
| `title` | `string` | — | Titolo dialog |
| `content` | `ReactNode \| string` | — | Contenuto |
| `actions` | `ModalAction[]` | — | Bottoni footer `{label, variant, onClick}` |
| `size` | `'sm' \| 'md' \| 'lg' \| 'xl'` | `'md'` | Larghezza (400/600/900/1200px) |
| `onClose` | `() => void` | — | Callback chiusura (Esc o click fuori) |
| `closeOnBackdrop` | `bool` | `true` | Click backdrop chiude |
| `loading` | `bool` | `false` | Footer loading state |

---

### 2.9 KBMTabs

**Scopo**: Navigazione a schede per viste dettaglio.

| Prop | Tipo | Descrizione |
|---|---|---|
| `tabs` | `TabDef[]` | Array tab `{id, label, icon?, badge?, content, lazy}` |
| `activeTab` | `string` | Tab attiva (controlled) |
| `onTabChange` | `(id) => void` | Callback cambio tab |
| `lazy` | `bool` | Carica contenuto solo al primo accesso tab |
| `keepAlive` | `bool` | Mantiene contenuto DOM dopo cambio tab |

---

### 2.10 KBMToast

**Scopo**: Notifiche toast non bloccanti.

| Prop | Tipo | Default | Descrizione |
|---|---|---|---|
| `message` | `string` | — | Testo notifica |
| `type` | `'success' \| 'error' \| 'warning' \| 'info'` | `'info'` | Tipo |
| `duration` | `number \| null` | `3000` | Auto-dismiss ms; null = persistente |
| `action` | `{label, onClick}` | — | Azione opzionale (es. "Annulla") |
| `position` | `'top-right' \| 'top-center' \| 'bottom-right'` | `'top-right'` | Posizione |

**Metodi imperativi** (via ref o context):
- `toast.success(message, options?)`
- `toast.error(message, options?)`
- `toast.warning(message, options?)`
- `toast.info(message, options?)`
- `toast.dismiss(id?)`

---

### 2.11 KBMBreadcrumb

**Scopo**: Breadcrumb di navigazione con link cliccabili.

| Prop | Tipo | Descrizione |
|---|---|---|
| `items` | `BreadcrumbItem[]` | `{label, href?, onClick?}` — ultimo item non cliccabile |
| `onNavigate` | `(item) => void` | Callback navigazione |
| `maxItems` | `number` | Collassa in "..." se più di N elementi |

---

### 2.12 KBMAuditInfo

**Scopo**: Visualizza informazioni di audit (creazione/modifica) nel footer form.

| Prop | Tipo | Descrizione |
|---|---|---|
| `createdAt` | `Date` | Data creazione |
| `createdBy` | `string` | Utente creazione |
| `updatedAt` | `Date \| null` | Data ultima modifica |
| `updatedBy` | `string \| null` | Utente ultima modifica |

**Render**: `Creato: gg/mm/aaaa hh:mm da [Nome Utente] | Modificato: gg/mm/aaaa hh:mm da [Nome Utente]`

---

### 2.13 KBMAttachments

**Scopo**: Gestione allegati per entità.

| Prop | Tipo | Descrizione |
|---|---|---|
| `entityType` | `string` | Tipo entità (es. `'ordini'`) |
| `entityId` | `string` | ID record |
| `files` | `Attachment[]` | Lista allegati correnti |
| `onUpload` | `(file: File) => Promise<void>` | Callback upload |
| `onDownload` | `(id: string) => void` | Callback download |
| `onDelete` | `(id: string) => void` | Callback elimina |
| `maxSize` | `number` | Dimensione massima file (bytes) |
| `allowedTypes` | `string[]` | MIME types accettati |
| `readonly` | `bool` | Solo visualizzazione |

---

### 2.14 KBMStatusBadge

**Scopo**: Indicatore visivo stato documento/record.

| Prop | Tipo | Descrizione |
|---|---|---|
| `status` | `string` | Chiave stato (es. `'confermato'`, `'evaso'`) |
| `statusMap` | `Record<string, BadgeConfig>` | Mapping `{label, color, icon}` |
| `size` | `'sm' \| 'md'` | Dimensione badge |

**Status predefiniti sistema**:
```
bozza → Gray "Bozza"
confermato → Blue "Confermato"
parz_evaso → Amber "Parz. Evaso"
evaso → Green "Evaso"
contabilizzato → Green "Contabilizzato"
annullato → Red "Annullato"
```

---

### 2.15 KBMPageHeader

**Scopo**: Header pagina con titolo, contatore risultati e azioni globali.

| Prop | Tipo | Descrizione |
|---|---|---|
| `title` | `string` | Titolo pagina |
| `subtitle` | `string?` | Sottotitolo opzionale |
| `totalCount` | `number?` | Contatore record |
| `actions` | `HeaderAction[]` | Azioni header-level |

---

## 3. PATTERN COMPOSIZIONE

### 3.1 Vista Lista Completa
```
<KBMPageHeader title="Clienti" totalCount={287} />
<KBMToolbar actions={[Nuovo, Modifica, Elimina, Esporta]} />
<KBMSearchBox onSearch={handleSearch} />
<KBMFilterPanel fields={clienteFields} onApply={handleFilter} />
<KBMDataGrid columns={colonneClienti} data={clienti} onRowClick={apriDettaglio} />
```

### 3.2 Vista Dettaglio Completa
```
<KBMBreadcrumb items={[Home, Clienti, "Rossi & C."]} />
<KBMToolbar actions={[Modifica, Salva, Annulla, Elimina, Stampa]} />
<KBMTabs tabs={[Generale, Contatti, Indirizzi, Ordini, Pagamenti]} lazy>
  <tab:Generale>
    <KBMForm fields={campiGenerali} values={cliente} mode="view" />
  </tab:Generale>
  ...
</KBMTabs>
<KBMAuditInfo createdAt={...} createdBy={...} updatedAt={...} updatedBy={...} />
```

---

## 4. NAMING CONVENTIONS

| Tipo | Convention | Esempio |
|---|---|---|
| Componente | PascalCase con prefisso KBM | `KBMDataGrid` |
| Props | camelCase | `onRowClick`, `totalCount` |
| CSS Token | `--kbm-` prefix, kebab-case | `--kbm-primary-500` |
| CSS Class | `kbm-` prefix, BEM-light | `kbm-grid__row--selected` |
| Events | `on` + PascalCase | `onSelectionChange` |
| Types/Interfaces | PascalCase | `ColumnDef`, `FilterGroup` |
| Enums | PascalCase | `FormMode`, `ToastType` |

---

*KBM Component Library Plan v1.0 — © 2026 KBM Project.*
