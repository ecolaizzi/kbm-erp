# UI/UX Study — Pattern ERP Enterprise Italiani
> Progetto KBM | Scouting Competitivo | Accesso: 2026-06-09

---

## 1. NAVIGATION PATTERN

### 1.1 Menu Principale
ERP italiani tradizionali (NTS Business Cube, Zucchetti Ad Hoc/Mago) usano prevalentemente:
- **Menu orizzontale a cascata** (stile MDI classico Windows)
- Struttura gerarchica: Modulo → Sezione → Funzione
- Esempio: `Vendite > Ciclo Attivo > Ordini Cliente`

ERP moderni (Dynamics 365 BC, Odoo) adottano:
- **Side navigation** con icone e label collassabile
- **Role Center / Dashboard** come home page personalizzata per ruolo
- Barra di ricerca rapida globale ("Tell Me" in BC, "/" in Odoo)

**Pattern consigliato per KBM**: Side nav collassabile + breadcrumb contestuale + ricerca globale

### 1.2 Breadcrumb
- Pattern universale: `Modulo > Sezione > Documento`
- Dynamics 365 BC: breadcrumb sempre visibile in alto
- Odoo: breadcrumb con link cliccabili per navigazione back
- **Anti-pattern**: finestre MDI sovrapposte (Zucchetti legacy) — utenti confusi

### 1.3 Toolbar / Action Bar
Pattern comune (tutti i competitor):
- Pulsanti primari: **Nuovo, Salva, Elimina, Stampa**
- Pulsanti contestuali: variano per documento (es. "Conferma", "Contabilizza", "Spedisci")
- Dynamics 365 BC: action bar con raggruppamenti categorizzati
- Odoo: bottone "Azioni" dropdown per operazioni secondarie

---

## 2. DATA GRIDS (Liste/Tabelle)

### 2.1 Struttura Base
Pattern comune in tutti gli ERP analizzati:
- Header con nome colonna cliccabile per **ordinamento** (ASC/DESC)
- Righe alternante (zebra striping) per leggibilità
- Selezione multipla con checkbox (colonna sinistra)
- Paginazione o scroll infinito (ERP moderni)

### 2.2 Filtri
- **Quick filter** / barra di ricerca inline (tutti)
- **Filtri avanzati** con AND/OR su campi multipli (Odoo, BC)
- **Filtri salvati** / viste personalizzate (Odoo, BC)
- Filtri per range di date (calendar picker) — essenziale
- Filtering per stato (es. "Solo aperti", "Da evadere") — standard

### 2.3 Bulk Actions
- Selezione multipla → azione collettiva
- Es. stampa multipla DDT, conferma multipla ordini
- Dynamics 365 BC: "Imposta per lista selezionata"
- **Gap NTS/Zucchetti**: operazioni bulk limitate

### 2.4 Colonne Configurabili
- Mostra/nascondi colonne — standard in BC, Odoo
- Riordina colonne (drag & drop) — Odoo, BC
- Salva layout per utente

### 2.5 Export
- Export Excel / CSV — universale
- PDF diretta — comune per documenti
- **Raccomandazione KBM**: Export Excel nativo con formattazione

---

## 3. FORMS (CRUD)

### 3.1 Layout Form
Pattern diffuso in ERP italiani:
- **Header** con campi chiave (numero doc, data, cliente/fornitore, stato)
- **Tabs/Schede** per raggruppare sezioni (es. "Dati generali", "Spedizione", "Pagamento")
- **Grid** righe documento (articoli, importi)
- **Footer** con totali (imponibile, IVA, totale)

### 3.2 Validazione
- Validazione in-line sui campi (rosso = errore)
- Messaggi di errore contestuali vicino al campo
- Conferma prima di azioni distruttive (elimina, annulla)
- Lock documento dopo contabilizzazione (sola lettura)

### 3.3 Ricerca/Lookup
- Lookup su anagrafiche con ricerca rapida (F4 o icona lente)
- Risultati in popup/dropdown con filtro inline
- **Zucchetti Ad Hoc**: lookup con F4, popup a cascata
- **Odoo**: Many2one field con search-as-you-type
- **Pattern KBM**: search-as-you-type con preview dati chiave

### 3.4 Auto-calcolo
- Totali riga aggiornati on-the-fly (Qt × Prezzo = Subtotale)
- IVA calcolata automaticamente da aliquota articolo/cliente
- Scadenze generate automaticamente dai termini di pagamento

---

## 4. DASHBOARD & KPI

### 4.1 Pattern Dashboard
- **Widget KPI** (numero con delta vs periodo precedente)
- **Grafici a barre/torta** per top clienti, top prodotti
- **Lista eventi** (scadenze prossime, ordini da evadere)
- **Quick actions** per azioni frequenti

### 4.2 Personalizzazione
- Dynamics 365 BC: Role Center completamente configurabile
- Odoo: dashboard drag & drop con widget scelti dall'utente
- **NTS/Zucchetti**: dashboard limitata, più statica

### 4.3 Refresh
- Dati in tempo reale o near-real-time (ERP moderni)
- Aggiornamento manuale (F5 / pulsante refresh) — pattern legacy

---

## 5. KEYBOARD SHORTCUTS E PRODUTTIVITÀ

### 5.1 Shortcut Standard ERP
| Azione | Zucchetti Ad Hoc | Dynamics 365 BC | Odoo | KBM (raccomandato) |
|---|---|---|---|---|
| Nuovo record | F3 / Ctrl+N | Alt+N | Nuovo | Ctrl+N |
| Salva | F10 | Ctrl+S | Salva | Ctrl+S |
| Cerca/Lookup | F4 | F4 | Search-type | F4 / Ctrl+F |
| Elimina riga | Del | Del | Elimina | Del |
| Conferma doc | F8 (variabile) | variabile | variabile | Ctrl+Enter |
| Stampa | Ctrl+P | Ctrl+P | Ctrl+P | Ctrl+P |
| Navigazione Tab | Tab | Tab | Tab | Tab |

### 5.2 Produttività Avanzata
- **Duplica record**: Ctrl+D (Odoo), Da copiare (BC)
- **History/Recenti**: lista ultimi documenti aperti
- **Preferiti / Segnalibri**: salva documenti frequenti
- **Quick Insert righe**: tasto Enter o freccia giù per nuova riga

---

## 6. REMOTE DESKTOP (RDP) OPTIMIZATION

NTS Business Cube e Zucchetti Mago/Ad Hoc sono tipicamente usati via RDP/Citrix:

### 6.1 Considerazioni RDP
- **Font rendering**: font system (non web) migliorano la nitidezza su RDP
- **Latenza**: evitare hover tooltips delay-based su RDP (latenza)
- **Colori**: usare colori con contrasto alto (no sfumature sottili)
- **Touch/Scroll**: mouse wheel support critico (no gesture-only)
- **Copy/Paste**: clipboard sharing RDP deve funzionare per dati

### 6.2 Best Practice per KBM (Web App in RDP)
- Usare Chrome/Edge in kiosk mode su RDP
- Font System UI o equivalente leggibile a 96dpi
- Bottoni almeno 32px height per click accuracy
- Evitare drag & drop complesso come funzione primaria
- Shortcut da tastiera come alternativa a ogni azione mouse

---

## 7. TERMINOLOGIA UI ITALIANA

### 7.1 Label Standard nei Form
| Contesto | Termine Italiano | Note |
|---|---|---|
| Dati fiscali cliente | Partita IVA | Campo obbligatorio per soggetti IVA |
| Dati fiscali persona fisica | Codice Fiscale | 16 caratteri alfanumerici |
| Documento di Trasporto | DDT / Bolla | Causale di trasporto, porto, ecc. |
| Documento di pagamento | RI.BA. | Ricevuta Bancaria — pagamento tipico ITA |
| Registro pagamenti | Scadenzario | Lista scadenze da incassare/pagare |
| Prima registrazione | Prima Nota | Scrittura contabile manuale |
| IVA da versare | Liquidazione IVA | Calcolo mensile/trimestrale |
| Codice articolo interno | Codice articolo | vs codice fornitore, codice cliente |
| Numero documento | N. / Nro. | Abbreviazione numerazione |
| Data documento | Data doc. | Data fattura/ordine ecc. |

### 7.2 Messaggi di Stato Documento
- **Inserimento** / Bozza — documento non finalizzato
- **Confermato** — ordine confermato, non ancora evaso
- **Parzialmente evaso** — evaso in parte
- **Evaso** / **Contabilizzato** — completato
- **Annullato** — cancellato con tracciabilità

---

## 8. ANTI-PATTERN DA EVITARE

1. **Finestre MDI sovrapposte** (stile Windows 3.1) — disorientante
2. **Popup modal bloccanti** per ogni azione secondaria
3. **Bottoni con solo icona** senza tooltip/label — problemi accessibilità
4. **Grid senza paginazione né virtualizzazione** con 10k+ righe
5. **Form con 50+ campi visibili** senza raggruppamento in tab
6. **Salvataggio implicito** senza conferma visiva
7. **Messaggi di errore generici** tipo "Errore: operazione non riuscita"
8. **Stampa bloccante** (sincrona nel thread UI)
9. **Lookup a codice solo** senza descrizione visibile
10. **Date in formato solo numerico** (gg/mm/aaaa più usabile di timestamp)

---

## 9. PATTERN SPECIFICI MERCATO ITALIANO

### 9.1 Gestione IVA
- Campo "Aliquota IVA" sempre visibile nei form fattura
- Scorporo IVA da imponibile (reverse)
- Split Payment per PA
- IVA in sospensione per cassa

### 9.2 Scadenzario
- Visualizzazione scadenze con semaforo (verde/giallo/rosso per scaduto/prossimo/futuro)
- Filtro "solo scadute", "prossime 30gg"
- Export RIBA per home banking (tracciato CBI)

### 9.3 Numerazione Documenti
- Serie numeriche configurabili per tipo documento e anno
- Reset automatico al cambio esercizio
- Prefisso configurabile (es. "FT2024/001")

[Fonte: analisi da documentazione pubblica Microsoft Docs (BC), Odoo Docs, brochure Zucchetti, sito NTS - Accesso: 2026-06-09]
