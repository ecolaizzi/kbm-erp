# Wireframe 02 — Dashboard

**Schermata**: Dashboard principale post-login  
**Componenti**: KBMPageHeader, KBMStatusBadge  
**Target utente**: Tutti i ruoli (contenuto personalizzato per ruolo)

---

## ASCII Wireframe

```
╔══════════════════════════════════════════════════════════════════════╗
║ ▣ KBM │ Demo Srl ▼                        🔔3  👤Mario Rossi▼  [?]║
╠════════╦═════════════════════════════════════════════════════════════╣
║🏠 Core ║  🏠 Dashboard                                              ║
║        ╠═════════════════════════════════════════════════════════════╣
║👥 Ana. ║                                                             ║
║        ║  Buongiorno, Mario. Oggi è lunedì 9 giugno 2026           ║
║💲 List ║                                                             ║
║        ║  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐    ║
║────────║  │ Clienti  │ │Fornitori │ │ Articoli │ │  Utenti  │    ║
║⚙ Amm. ║  │          │ │          │ │          │ │          │    ║
║        ║  │  1.247   │ │   389    │ │  8.432   │ │    12    │    ║
║        ║  │ +3 oggi  │ │ = stesso │ │ +15 oggi │ │ 4 attivi │    ║
║        ║  └──────────┘ └──────────┘ └──────────┘ └──────────┘    ║
║        ║                                                             ║
║        ║  ── Azioni Rapide ──────────────────────────────────────  ║
║        ║                                                             ║
║        ║  [+ Nuovo Cliente]  [+ Nuovo Fornitore]  [+ Nuovo Articolo]║
║        ║  [+ Nuovo Utente ]  [⚙ Configura azienda]                ║
║        ║                                                             ║
║        ║  ── Ultimi Accessi ─────────────────────────────────────  ║
║        ║                                                             ║
║        ║  ┌─────────────────────────────────────────────────────┐  ║
║        ║  │ Tipo   │ Codice      │ Descrizione    │ Data        │  ║
║        ║  ├────────┼─────────────┼────────────────┼─────────────┤  ║
║        ║  │ Cliente│ CLI-00042   │ Rossi & C. Srl │ oggi 10:15 │  ║
║        ║  │ Artic. │ ART-01234   │ Vite M5×20 Zn  │ oggi 09:30 │  ║
║        ║  │ Forni. │ FOR-00188   │ Acme Supply SpA│ ieri 16:45 │  ║
║        ║  │ Utente │ USR-00005   │ Anna Bianchi   │ ieri 14:20 │  ║
║        ║  └─────────────────────────────────────────────────────┘  ║
║        ║                                                             ║
║        ║  ── Notifiche di Sistema ───────────────────────────────  ║
║        ║                                                             ║
║        ║  ⚠  3 utenti con password in scadenza entro 7 giorni      ║
║        ║  ℹ  Ultimo backup: ieri alle 23:00 ✓                       ║
║        ║  ℹ  KBM v1.0.3 disponibile — Note rilascio                ║
║        ║                                                             ║
╠════════╩═════════════════════════════════════════════════════════════╣
║ Pronto │ Demo Srl │ Mario Rossi │ Versione 1.0.0                   ║
╚══════════════════════════════════════════════════════════════════════╝
```

---

## KPI Cards — Dettaglio

```
┌──────────────────────────┐
│  CLIENTI                 │
│                          │
│       1.247              │  ← numero grande, prominente
│                          │
│  ↑ +3 oggi               │  ← delta (verde se positivo)
│  📊 [Vai alla lista]     │
└──────────────────────────┘
```

**Colori delta**:
- ↑ positivo → testo verde (`--kbm-success-500`)
- = invariato → testo grigio
- ↓ negativo → testo rosso (`--kbm-error-500`)

---

## Dashboard Ruolo-Specifica

| Ruolo | KPI mostrate | Azioni rapide |
|---|---|---|
| **Admin ERP** | Utenti, Aziende, Sessioni attive, Log errori | Nuovo Utente, Configura Ruoli, Audit Log |
| **Resp. Vendite** | Clienti, Ordini aperti (futuro), Fatturato (futuro) | Nuovo Cliente, Nuovo Ordine |
| **Contabile** | Scaduti, In scadenza 30gg, Estratto conto (futuro) | Prima Nota, Scadenzario |
| **Read-Only** | Solo KPI visualizzazione, nessuna azione rapida | Solo navigazione |

---

## Specifiche UX

| Aspetto | Specifica |
|---|---|
| **KPI Card** | 4 per riga su 1920px; 2 per riga su 1280px |
| **Card click** | Naviga alla lista corrispondente |
| **Aggiornamento KPI** | F5 o pulsante refresh in angolo sezione |
| **Azioni rapide** | Bottoni secondari, stessa larghezza |
| **Ultimi accessi** | Max 10 righe, ordinati per data desc |
| **Click riga** | Apre direttamente il record |
| **Notifiche** | Max 5 visibili; link "Vedi tutte" |
| **Welcome message** | Con nome utente e data corrente |
| **Saluto contestuale** | Buongiorno / Buon pomeriggio in base all'ora |
