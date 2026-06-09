# KBM UX Guidelines — Enterprise Desktop Client

**Versione**: 1.0  
**Data**: 2026-06-09  
**Owner**: UI/UX Designer Agent  
**Status**: ✅ Draft — In revisione

---

## 1. DESIGN PRINCIPLES

### 1.1 Productivity First
Ogni decisione di design si misura in click e secondi risparmiati. Obiettivo: un operatore esperto completa un ordine senza mai staccare le mani dalla tastiera.
- Azioni primarie sempre raggiungibili in ≤ 2 click
- Scorciatoie da tastiera per ogni operazione (vedi Keyboard Shortcuts Reference)
- Default intelligenti (data odierna, cliente frequente, ultima aliquota IVA usata)
- Nessun dialogo modale per conferme banali (usa toast/undo pattern)

### 1.2 Data Density
Gli utenti ERP gestiscono grandi volumi di dati. Il design privilegia la densità informativa rispetto al whitespace estetico.
- Font body 14px, grids 12px — minimo 12px assoluto
- Righe griglia altezza 32px (compact) — opzione 40px per accessibilità
- Massimo utilizzo dello spazio viewport; nessun hero-section / decorativo
- Pannelli collassabili per nascondere sezioni meno frequenti

### 1.3 Consistency
Lo stesso pattern CRUD su ogni modulo. L'utente che impara Clienti sa già usare Fornitori, Articoli, Ordini.
- Stesso layout: Toolbar → Filtri → Griglia → Paginazione
- Stesse shortcut in ogni vista lista
- Stesse posizioni per azioni (Nuovo sempre in alto-sinistra toolbar)
- Naming uniforme: "Nuovo", "Modifica", "Elimina", "Salva", "Annulla"

### 1.4 Familiarity
Pattern consolidati dal mondo ERP italiano (Zucchetti, Business Cube) astratti e migliorati — senza copiare aspetto visivo.
- Terminologia italiana standard: P.IVA, DDT, RIBA, Scadenzario, Prima Nota
- F4 per lookup (comportamento noto agli utenti ERP)
- Numerazione documenti visibile e prominente (FT2024/001)
- Semaforo scaduto/prossimo/futuro nel scadenzario

### 1.5 RDP-Optimized
KBM viene usato prevalentemente via Remote Desktop/Citrix. Design che funziona con latenza 50-150ms e banda limitata.
- Nessuna animazione fluida come requisito funzionale (solo feedback visivo)
- Bottoni minimum 32×32px per click accuracy su RDP
- Niente drag & drop come unico modo di eseguire un'azione
- Debounce su search/filter (min 300ms) per evitare flood richieste
- Indicatori di caricamento sincroni visibili

### 1.6 Performance
- Paginazione server-side su ogni lista (default 25, opzioni 50/100)
- Virtualizzazione griglia per render veloce su dataset grandi
- Lazy loading sezioni tab: carica solo tab attiva
- Ricerca globale con indice server-side (< 500ms)

### 1.7 Keyboard-First
Ogni funzione accessibile da tastiera. Il mouse è opzionale.
- Focus management esplicito: Esc chiude, Tab avanza, Enter conferma
- Skip-to-content link per screen reader
- Visual focus ring sempre visibile (non rimosso con CSS)

---

## 2. LAYOUT STRUCTURE

```
┌──────────────────────────────────────────────────────────────────┐
│  ▣ KBM │ [Azienda Demo Srl ▼]    [🔔 3] [👤 Mario Rossi ▼] [?] │  ← TOP BAR (48px)
├─────────┬────────────────────────────────────────────────────────┤
│  ▣      │  Home > Vendite > Ordini Cliente                       │  ← BREADCRUMB (36px)
│ 🏠 Core  ├────────────────────────────────────────────────────────┤
│ 👥 Cli.  │                                                        │
│ 🏭 Art.  │                                                        │
│ 📦 Mag.  │         MAIN CONTENT AREA                             │
│ 🛒 Vend  │         (Grid / Form / Dashboard)                     │
│ 📥 Acq.  │                                                        │
│ 💰 Cont  │                                                        │
│ ⚙ Amm.  │                                                        │
│  ──────  ├────────────────────────────────────────────────────────┤
│ [◀ <<]  │  Pronto │ Selezionati: 5 │ Ultima azione: 10:23:45    │  ← STATUS BAR (28px)
└─────────┴────────────────────────────────────────────────────────┘
     ↑
  SIDE NAV
  (220px espanso / 48px collassato)
```

### 2.1 Top Bar (altezza: 48px)
| Zona | Contenuto | Note |
|---|---|---|
| Sinistra | Logo KBM + nome app | Click → Dashboard |
| Centro-sinistra | Selettore azienda (dropdown) | Visibile solo se multi-azienda |
| Destra | Notifiche badge, Menu utente, Help | Menu utente: Profilo, Password, Logout |

### 2.2 Side Navigation (larghezza: 220px espanso / 48px collassato)
- Icona + label per ogni modulo; solo icona quando collassato
- Modulo attivo evidenziato con indicatore colorato sinistra
- Submenu espandibile per moduli con molte sezioni
- Toggle collassa/espandi in fondo

**Moduli MVP**:
1. 🏠 Dashboard (Core)
2. ⚙️ Amministrazione (Utenti, Aziende, Ruoli, Audit)
3. 👥 Anagrafiche (Clienti, Fornitori, Articoli)
4. 💲 Listini (Prezzi vendita, prezzi acquisto)

**Moduli futuri** (placeholder nel menu, disabilitati):
5. 🛒 Vendite | 📥 Acquisti | 📦 Magazzino | 💰 Contabilità | 🤝 CRM

### 2.3 Breadcrumb (altezza: 36px)
- Formato: `Modulo > Sezione > Documento`
- Ogni segmento cliccabile per navigazione back
- Ultimo segmento non cliccabile (contesto corrente)

### 2.4 Content Area
- **Vista Lista (Grid)**: Toolbar → Filtri rapidi → Griglia → Paginazione
- **Vista Dettaglio (Form)**: Toolbar → Header campi chiave → Tabs → Contenuto tab → Footer audit

### 2.5 Status Bar (altezza: 28px, font 11px)
| Posizione | Info |
|---|---|
| Sinistra | Stato sistema (Pronto / Caricamento... / Errore) |
| Centro | Record selezionati / totale |
| Destra | Ultima operazione + timestamp / Connessione |

---

## 3. NAVIGATION PATTERNS

### 3.1 Vista Lista → Dettaglio
1. Click riga → apre dettaglio (stessa finestra, breadcrumb aggiornato)
2. Enter su riga selezionata → apre dettaglio
3. Doppio click riga → apre in modalità modifica
4. Back button / breadcrumb → torna alla lista (con posizione preservata)

### 3.2 Tabs nei Dettagli
- Tabs visibili in alto nel content area, sotto toolbar
- Tab attiva: indicatore sottolineato colorato
- Navigazione: Ctrl+→ / Ctrl+← tra tabs, o click diretto
- Lazy load: contenuto caricato solo quando tab diventa attiva

### 3.3 Modali
- Usate SOLO per: conferme distruttive, lookup entità, selezione multipla
- Sempre con pulsanti Conferma / Annulla
- Esc chiude sempre
- Dimensioni: SM (400px), MD (600px), LG (900px), XL (1200px)

---

## 4. GRID VIEW PATTERNS

### 4.1 Struttura Standard Griglia
```
[Toolbar: Nuovo | Modifica | Elimina | Stampa | Esporta | ... Altro ▾]
[Quick filter: Tutti | Oggi | Questo mese | Aperti | Da evadere]
[Search: _______________ 🔍]  [Filtro avanzato: ⚙]  [Colonne: ☰]
┌──────────────────────────────────────────────────────────────────┐
│ ☐ │ Col1 ↕ │ Col2 ↕ │ Col3 ↕ │ Col4 ↕ │ Stato │ Azioni       │
├──────────────────────────────────────────────────────────────────┤
│ ☐ │ ...    │ ...    │ ...    │ ...    │ ●     │ ✏ 🗑         │
│ ☑ │ ...    │ ...    │ ...    │ ...    │ ●     │ ✏ 🗑         │ ← selezionata
│ ☐ │ ...    │ ...    │ ...    │ ...    │ ●     │ ✏ 🗑         │
└──────────────────────────────────────────────────────────────────┘
[Pag 1 / 12 │ ◀ Prec │ Avanti ▶ │ Mostra: 25 ▾ │ Tot: 287 record]
```

### 4.2 Comportamenti Griglia
- **Ordinamento**: click header colonna → ASC → DESC → nessuno; indicatore freccia
- **Zebra striping**: righe alternate per leggibilità
- **Selezione multipla**: checkbox nella prima colonna; Shift+click per range
- **Selezione tutto**: checkbox nell'header seleziona pagina corrente; "seleziona tutto" per query intera
- **Inline edit**: doppio click su cella → edit inline se campo editabile
- **Context menu**: tasto destro su riga → menu contestuale (Apri, Modifica, Duplica, Elimina)
- **Column resize**: drag bordo header colonna
- **Column reorder**: drag & drop header (alternativa da menu ☰ per RDP)

### 4.3 Quick Filters
Filtri predefiniti in pill/chip clickabili sopra la griglia. Esempi:
- Clienti: [Tutti] [Attivi] [Inattivi] [Con debiti scaduti]
- Ordini: [Tutti] [Aperti] [Da evadere] [Evasi] [Annullati] [Questo mese]
- Filtro attivo: pill evidenziata con colore primario

### 4.4 Azioni Bulk
Quando 1+ righe selezionate, appare barra azioni bulk:
```
■ 5 selezionati: [Stampa] [Esporta] [Elimina] [Cambia stato ▾]  [✕ Deseleziona]
```

---

## 5. FORM VIEW PATTERNS

### 5.1 Struttura Standard Form
```
[Toolbar: Modifica | Salva | Annulla | Elimina | Stampa | ... Altro ▾]
┌─── HEADER ────────────────────────────────────────────────────────┐
│ N° Documento: FT2024/001  │ Data: 01/06/2024  │ Stato: ● Bozza  │
│ Cliente: [Rossi & C. Srl              ▼]  P.IVA: 01234567890    │
└───────────────────────────────────────────────────────────────────┘
[Tab: Dati Generali] [Righe] [Pagamenti] [Spedizione] [Note] [Storico]
┌─── TAB CONTENT ───────────────────────────────────────────────────┐
│                                                                    │
│  (contenuto tab corrente)                                         │
│                                                                    │
└───────────────────────────────────────────────────────────────────┘
┌─── FOOTER AUDIT ──────────────────────────────────────────────────┐
│ Creato: 01/06/2024 09:15 da Mario Rossi │ Modificato: 01/06/2024 11:30 da Anna Bianchi │
└───────────────────────────────────────────────────────────────────┘
```

### 5.2 Campi Form
- **Label**: sempre visibile (no placeholder-only pattern)
- **Campo obbligatorio**: asterisco rosso (*) accanto alla label
- **Errore validazione**: bordo rosso + messaggio sotto il campo
- **Lookup field**: input testo + icona 🔍 (o F4) → apre KBMLookup
- **Campo read-only**: sfondo grigio chiaro, cursore non-editable
- **Campo calcolato**: sfondo leggermente diverso + tooltip "calcolato automaticamente"

### 5.3 Stato Form
| Stato | Behavior |
|---|---|
| **Visualizzazione** | Tutti i campi read-only, toolbar mostra "Modifica" |
| **Modifica** | Campi editabili, toolbar mostra "Salva" / "Annulla" |
| **Nuovo** | Form vuoto con default precompilati, toolbar mostra "Salva" / "Annulla" |
| **Contabilizzato/Bloccato** | Read-only forzato, nessun edit permesso; tooltip spiega perché |

---

## 6. FEEDBACK PATTERNS

### 6.1 Toast Notifications (KBMToast)
- **Success** (verde): "Ordine FT2024/001 salvato con successo" — auto-dismiss 3s
- **Error** (rosso): "Errore: Partita IVA non valida" — no auto-dismiss, richiede click
- **Warning** (ambra): "Attenzione: cliente con pagamenti scaduti" — auto-dismiss 5s
- **Info** (blu): "Importazione in corso..." — progress o auto-dismiss
- Posizione: angolo in alto a destra, stacking verticale

### 6.2 Loading States
- Griglia: skeleton rows durante caricamento
- Form: spinner overlay semi-trasparente
- Toolbar button: spinner inline (no disabled senza feedback)
- Status bar: "Caricamento..." durante operazioni

### 6.3 Conferme Distruttive
```
┌─────────────────────────────────────────┐
│ ⚠  Elimina record                       │
├─────────────────────────────────────────┤
│ Stai per eliminare il cliente           │
│ "Rossi & C. Srl". Questa operazione     │
│ non può essere annullata.               │
│                                         │
│         [Annulla]  [Elimina]            │
└─────────────────────────────────────────┘
```
- Focus default su "Annulla" (sicuro)
- Tasto Enter non deve attivare "Elimina"
- Esc chiude come "Annulla"

---

## 7. RDP OPTIMIZATION NOTES

| Aspetto | Implementazione |
|---|---|
| Animazioni | Ridotte a 100ms max; nessuna per azioni di navigazione |
| Font | System UI o Inter subset; no Google Fonts CDN esterni |
| Bitmap | Icone SVG inline; nessuna immagine decorativa bitmap |
| Debounce | Search: 300ms; Filter: 500ms; Autocomplete: 400ms |
| Button size | Min 32px height; target click area min 32×32px |
| Scroll | Mouse wheel supportato ovunque; no swipe-only |
| Clipboard | Ctrl+C / Ctrl+V su tutti i campi testo |
| Bandwidth | Payload JSON compresso; paginazione server-side; no SSE/WebSocket per MVP |
| Cursor | Cursore loading (busy) durante operazioni server |
| Input lag | Feedback locale immediato (ottimistic UI dove possibile) |

---

*KBM UX Guidelines v1.0 — © 2026 KBM Project. Design originale, nessuna copia da competitor.*
