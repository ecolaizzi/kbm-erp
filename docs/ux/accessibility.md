# KBM Accessibility Requirements

**Versione**: 1.0  
**Data**: 2026-06-09  
**Standard di riferimento**: WCAG 2.1 AA  
**Status**: ✅ Draft

---

## 1. OBIETTIVI ACCESSIBILITÀ

KBM deve essere utilizzabile da utenti con diverse abilità, inclusi utenti che:
- Usano solo la tastiera (motorio)
- Usano screen reader (visivo)
- Hanno difficoltà visive (contrasto, dimensioni)
- Usano KBM su RDP con latenza e risoluzione ridotta

**Target**: conformità WCAG 2.1 livello AA per tutte le schermate MVP.

---

## 2. KEYBOARD NAVIGATION

### 2.1 Requisiti
- **100% keyboard accessible**: ogni azione deve essere eseguibile senza mouse
- **Focus management**: focus sempre visibile, posizione logica dopo ogni azione
- **Tab order**: sequenza tab segue ordine logico visivo (sinistra→destra, alto→basso)
- **Skip links**: link "Vai al contenuto" in cima alla pagina (visibile al focus)
- **Keyboard traps**: EVITARE — il focus non deve mai rimanere bloccato

### 2.2 Focus Indicator
```css
/* Regola assoluta: nessun outline: none senza alternativa */
:focus-visible {
  outline: 2px solid var(--kbm-primary-500);
  outline-offset: 2px;
  border-radius: var(--kbm-radius-sm);
}
```
- Focus ring: 2px solid `#3B82F6` (primary-500)
- Visibile su qualsiasi sfondo (contrasto ≥ 3:1)
- Mai rimosso con `outline: none` senza sostituto

### 2.3 Modali e Dialog
- Quando si apre un modale: focus si sposta al primo elemento focusabile interno
- Focus trap attivo all'interno del modale (tab non esce dal modale)
- Quando si chiude: focus torna all'elemento che ha aperto il modale
- Esc chiude sempre i modali

---

## 3. SCREEN READER SUPPORT

### 3.1 ARIA Labels
Tutti i componenti interattivi hanno label accessibile:

| Componente | Attributo ARIA | Esempio |
|---|---|---|
| Bottone icona senza testo | `aria-label` | `aria-label="Elimina cliente"` |
| Campo input | `aria-label` o `<label for>` | Label esplicita sempre |
| Griglia dati | `role="grid"` + `aria-label` | `aria-label="Lista clienti, 287 risultati"` |
| Riga griglia | `role="row"` | Associato alla griglia |
| Cella griglia | `role="gridcell"` | Associata alla riga |
| Header griglia | `role="columnheader"` + `aria-sort` | `aria-sort="ascending"` |
| Tabs | `role="tablist"`, `role="tab"`, `role="tabpanel"` | Pattern ARIA tabs |
| Dialog/Modal | `role="dialog"` + `aria-labelledby` | Titolo come label |
| Alert/Toast | `role="alert"` (errori) o `aria-live="polite"` | Annunci live region |
| Status bar | `aria-live="polite"` | Aggiornamenti automatici |
| Loading | `aria-busy="true"` su container | Durante caricamento |
| Menu | `role="menu"`, `role="menuitem"` | Dropdown menu |
| Breadcrumb | `aria-label="Percorso navigazione"` + `role="list"` | Nav breadcrumb |
| Paginazione | `aria-label="Paginazione"` | Nav paginazione |

### 3.2 HTML Semantico
- Usare elementi HTML nativi dove possibile (`<button>`, `<input>`, `<select>`, `<nav>`, `<main>`, `<aside>`)
- `<main>` per content area principale
- `<nav>` per sidebar e breadcrumb
- `<header>` per top bar
- `<footer>` per status bar
- Evitare `<div>` con `role="button"` dove `<button>` funzionerebbe

### 3.3 Annunci Live Region
Feedback automatici annunciati a screen reader:
- Toast success/error: `role="alert"` (urgente) o `aria-live="polite"` (non urgente)
- Aggiornamento contatore: `aria-live="polite"`
- Completamento operazione: messaggio in live region

### 3.4 Immagini e Icone
- Icone decorative: `aria-hidden="true"`
- Icone con significato: `aria-label` sul componente padre
- Nessuna immagine con testo importante (no bitmap text)

---

## 4. COLOR AND CONTRAST

### 4.1 Requisiti Minimi
| Tipo testo | Requisito | Note |
|---|---|---|
| Testo normale (< 18px) | ≥ 4.5:1 | WCAG AA |
| Testo grande (≥ 18px o 14px bold) | ≥ 3:1 | WCAG AA |
| Componenti UI (bordi, icone) | ≥ 3:1 | WCAG AA |
| Stato focus | ≥ 3:1 | WCAG AA |

### 4.2 Regole Progetto
- **Nessuna informazione trasmessa solo tramite colore** — sempre accompagnata da icona o testo
  - ✅ Corretto: ● Verde "Confermato" — testo + colore
  - ❌ Sbagliato: ● (solo colore senza testo)
- Semaforo scadenzario: icona + colore + testo ("Scaduto", "In scadenza", "In ordine")
- Status badge: colore + etichetta testuale sempre visibile

### 4.3 Daltonismo
- Palette scelta evita combinazioni rosso-verde come UNICO differenziatore
- Usare forme/icone aggiuntive per trasmettere stati critici
- Test con simulatori daltonismo prima del rilascio

---

## 5. FONT E DIMENSIONI

### 5.1 Regole
| Regola | Valore | Note |
|---|---|---|
| Font minimo assoluto | 11px | Solo status bar e badge |
| Font minimo contenuto | 12px | Righe griglia compact |
| Font body standard | 14px | Testo standard form e liste |
| Line-height minimo | 1.4 | Per leggibilità |

### 5.2 Zoom e Ridimensionamento
- Layout funziona correttamente fino a 200% zoom browser
- Nessun overflow orizzontale a 100% zoom su 1280px
- Testo scalabile con zoom browser (no font in px fissi non scalabili)
- Responsive minimale per 1280×720 (target RDP secondario)

---

## 6. FORM ACCESSIBILITY

### 6.1 Requisiti Form
- Ogni `<input>` ha un `<label>` associato (for/id o aria-label)
- Campi obbligatori: `aria-required="true"` + indicatore visivo (asterisco *)
- Messaggi di errore:
  - `aria-describedby` sul campo → id del messaggio errore
  - `role="alert"` su messaggi errore critici
  - Descrizione dell'errore: specifica e azionabile ("Partita IVA non valida: devono essere 11 cifre numeriche")
- Gruppo campi correlati: `<fieldset>` + `<legend>`

### 6.2 Autocomplete / Lookup
- Usare `aria-autocomplete`, `aria-expanded`, `aria-controls` pattern ARIA combobox
- Annunciare numero di risultati ("5 risultati disponibili")
- Annunciare selezione ("Rossi & C. Srl selezionato")

---

## 7. MOTION E ANIMAZIONI

### 7.1 Prefers-Reduced-Motion
```css
@media (prefers-reduced-motion: reduce) {
  *, *::before, *::after {
    animation-duration: 0.01ms !important;
    transition-duration: 0.01ms !important;
  }
}
```
- Tutte le animazioni rispettano `prefers-reduced-motion`
- Le animazioni non devono superare 3 flash/secondo (epilessia fotosensibile)
- Nessuna animazione come unico vettore di informazione

---

## 8. TOUCH E POINTER

### 8.1 Target Size
- Dimensione minima area cliccabile: **32×32px** (target WCAG 2.5.8)
- Consigliato: **44×44px** per bottoni principali
- Spaziatura tra target: minimo 4px per evitare errori

### 8.2 Pointer Events
- Nessuna funzionalità accessibile solo con hover (tooltip informativi OK, ma non funzionalità)
- Mouse wheel support ovunque (critico su RDP)

---

## 9. ERROR HANDLING ACCESSIBILE

### 9.1 Errori Validazione Form
```
Formato corretto:
┌─────────────────────────────────────────────────┐
│ Partita IVA *                                   │
│ [IT01234567890          ] ← bordo rosso         │
│ ⚠ La Partita IVA deve contenere 11 cifre        │
│   numeriche. Formato: IT seguito da 11 cifre.   │
└─────────────────────────────────────────────────┘
```

### 9.2 Errori di Sistema
- Errori server: messaggio chiaro, azione suggerita ("Riprova" o "Contatta supporto")
- Timeout: avviso anticipato, possibilità di estendere sessione
- Perdita connessione: indicatore visivo in status bar + toast

---

## 10. TESTING ACCESSIBILITÀ

### 10.1 Strumenti Consigliati
- **axe DevTools**: test automatizzati WCAG
- **NVDA** (Windows): screen reader free per testing
- **Windows Narrator**: test su RDP nativo
- **Chrome Accessibility Audit**: Lighthouse integration
- **Color Contrast Analyzer**: verifica rapporto contrasto

### 10.2 Checklist Pre-Release
- [ ] Navigazione completa keyboard-only su ogni schermata
- [ ] Screen reader NVDA: tutte le azioni annunciate correttamente
- [ ] Contrasto: nessuna coppia testo/sfondo sotto 4.5:1
- [ ] Zoom 200%: nessun contenuto perso o overflow
- [ ] axe DevTools: zero violation critici
- [ ] Focus ring visibile su ogni elemento interattivo
- [ ] Modali: focus trap funzionante, Esc chiude
- [ ] Toast: annunciati da live region
- [ ] Form: tutti i campi hanno label, errori descrittivi

---

*KBM Accessibility Requirements v1.0 — WCAG 2.1 AA. © 2026 KBM Project.*
