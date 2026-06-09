# Wireframe 10 — Sales Order Detail (Dettaglio Ordine Cliente)

**Schermata**: Dettaglio / Modifica Ordine Cliente  
**Modulo**: Vendite → Ordini Cliente → [Ordine] (Fase 4 — placeholder)  
**Componenti**: KBMToolbar, KBMTabs, KBMForm, KBMDataGrid (righe), KBMAuditInfo

> **Nota MVP**: Placeholder per Fase 4. Design definitivo per coerenza dell'architettura UX.

---

## ASCII Wireframe — Vista Lettura

```
╔══════════════════════════════════════════════════════════════════════╗
║ ▣ KBM │ Demo Srl ▼                        🔔3  👤Mario Rossi▼  [?]║
╠════════╦═════════════════════════════════════════════════════════════╣
║🏠 Core ║  🛒 Vendite > Ordini > OC24/001                            ║
║        ╠═════════════════════════════════════════════════════════════╣
║🛒 Vend.║  ┌─────────────────────────────────────────────────────┐  ║
║ ├Ordini║  │[✏Modifica] [📄DDT] [🧾Fattura] [🖨Stampa] [✗Annulla]│  ║
║        ║  └─────────────────────────────────────────────────────┘  ║
║        ║                                                             ║
║        ║  OC24/001 — Ordine Cliente                  ● Aperto      ║
║        ║  Cliente: Rossi & C. Srl │ Data: 01/06/2024 │ Age: Ferrari║
║        ║                                                             ║
║        ║  [Dati Generali] [Righe] [Spedizione] [Pagamenti] [Note] [Storico]║
║        ║   ──────────────                                           ║
║        ║                                                             ║
║        ║  ┌─────────────────────────────────────────────────────┐  ║
║        ║  │  TAB: DATI GENERALI                                  │  ║
║        ║  │                                                       │  ║
║        ║  │  N° Ordine *        Data ordine *                    │  ║
║        ║  │  [OC24/001    ]     [01/06/2024      ]              │  ║
║        ║  │                                                       │  ║
║        ║  │  Cliente *                                            │  ║
║        ║  │  [🔍 Rossi & C. Srl — CLI001                     ]  │  ║
║        ║  │  P.IVA: IT01234567890  SDI: ABC1234               │  ║
║        ║  │                                                       │  ║
║        ║  │  Agente                  Data consegna richiesta      │  ║
║        ║  │  [🔍 Mario Ferrari    ] [15/06/2024           ]      │  ║
║        ║  │                                                       │  ║
║        ║  │  Listino               Valuta                        │  ║
║        ║  │  [Listino Standard ▼ ] [EUR - Euro ▼          ]     │  ║
║        ║  │                                                       │  ║
║        ║  │  Cond. pagamento *     Tipo pagamento                 │  ║
║        ║  │  [30gg d.f.f.m. ▼  ] [RIBA ▼               ]       │  ║
║        ║  │                                                       │  ║
║        ║  │  Riferimento cliente   Ns riferimento                │  ║
║        ║  │  [VS ORD 2024-0542  ] [—                    ]       │  ║
║        ║  └─────────────────────────────────────────────────────┘  ║
║        ║                                                             ║
╠════════╩═════════════════════════════════════════════════════════════╣
║ Pronto │ OC24/001 │ Rossi & C. Srl │ Lettura                       ║
╚══════════════════════════════════════════════════════════════════════╝
```

---

## Tab: Righe Ordine

```
┌────────────────────────────────────────────────────────────────────┐
│  TAB: RIGHE                             [+ Aggiungi riga] [Import]│
│                                                                    │
│  ┌──┬──────┬──────────────────────┬─────┬──────────┬───────┬────┐│
│  │Rg│Codice│ Descrizione          │ Qt. │ Pr. netto│ Sc.%  │Tot.││
│  ├──┼──────┼──────────────────────┼─────┼──────────┼───────┼────┤│
│  │ 1│ART001│ Vite M5×20 Zincata   │1000 │ € 0,045  │  5,00%│€42,75││
│  │ 2│ART002│ Dado M5 Zincato      │ 500 │ € 0,030  │  5,00%│€14,25││
│  │ 3│ART003│ Lastra 100×100×2mm   │  10 │ € 45,00  │  0,00%│€450,00││
│  │  │      │ ──────────────────── │─────│──────────│───────│─────││
│  │  │      │ + Nuova riga...      │     │          │       │     ││
│  └──┴──────┴──────────────────────┴─────┴──────────┴───────┴────┘│
│                                                                    │
│  ┌─────────────────────────────────────────────────────────────┐  │
│  │                    Imponibile:    €   507,00               │  │
│  │                    IVA 22%:       €   111,54               │  │
│  │                    ─────────────────────────               │  │
│  │                    Totale:        €   618,54               │  │
│  └─────────────────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────────────┘
```

---

## Tab: Spedizione

```
┌─────────────────────────────────────────────────────────────────┐
│  TAB: SPEDIZIONE                                                 │
│                                                                  │
│  Indirizzo di spedizione *                                       │
│  ● Sede legale (Via Roma 15, Milano)                            │
│  ○ [Seleziona altro indirizzo...          🔍]                   │
│                                                                  │
│  Porto *               Vettore                                   │
│  [Franco ▼        ]  [🔍 DHL Express                        ]  │
│                                                                  │
│  Causale trasporto *   Colli                                    │
│  [Vendita ▼        ]  [3                 ]                      │
│                                                                  │
│  Note spedizione                                                 │
│  [Chiamare prima della consegna: 02-12345                   ]   │
└─────────────────────────────────────────────────────────────────┘
```

---

## Tab: Pagamenti

```
┌─────────────────────────────────────────────────────────────────┐
│  TAB: PAGAMENTI (Scadenze generate automaticamente)              │
│                                                                  │
│  Condizioni: 30gg d.f.f.m. │ Tipo: RIBA │ Banca: IT60X0...     │
│                                                                  │
│  ┌──────────────┬───────────┬──────────┬────────────────────┐  │
│  │ Scadenza     │ Importo   │ Tipo     │ Stato              │  │
│  ├──────────────┼───────────┼──────────┼────────────────────┤  │
│  │ 31/07/2024   │ € 618,54  │ RIBA     │ ○ Da emettere      │  │
│  └──────────────┴───────────┴──────────┴────────────────────┘  │
│                                                                  │
│  [Rigenera scadenze]  [Aggiungi scadenza manuale]               │
└─────────────────────────────────────────────────────────────────┘
```

---

## Tab: Storico

```
┌─────────────────────────────────────────────────────────────────┐
│  TAB: STORICO                                                    │
│                                                                  │
│  Data/Ora            │ Utente       │ Azione          │ Dettaglio│
│  ────────────────    │ ──────────   │ ───────────     │ ────────│
│  01/06/2024 09:00   │ m.rossi      │ Creazione       │ —       │
│  01/06/2024 11:30   │ m.rossi      │ Modifica riga 2 │ Qt: 400→500│
│  02/06/2024 14:00   │ a.bianchi    │ Modifica stato  │ →Conferm│
└─────────────────────────────────────────────────────────────────┘
```

---

## Comportamenti Griglia Righe

| Comportamento | Specifica |
|---|---|
| **Enter su riga** | Aggiunge riga sotto se ultima; conferma edit se in edit |
| **F4 su codice** | Apre lookup articoli (KBMLookup) |
| **Auto-fill** | Selezionando articolo: pre-compila descrizione, prezzo, IVA |
| **Totale riga** | Calcolato live: Qt × Prezzo × (1 - Sconto%) |
| **Totali footer** | Aggiornati live ad ogni modifica riga |
| **IVA** | Raggruppata per aliquota (no mix insolito) |

---

## Specifiche UX

| Aspetto | Specifica |
|---|---|
| **Header documento** | N°, cliente, data, agente, stato sempre visibili |
| **Stato documento** | Cambia colore TopBar/Header in base allo stato (futuro) |
| **Righe ordine** | Grid inline editabile con Enter/Tab per inserimento rapido |
| **Totali** | Sempre aggiornati in tempo reale |
| **Scadenze** | Generate automaticamente dai termini di pagamento |
| **Stampa** | Anteprima PDF prima della stampa fisica |
| **DDT/Fattura** | Azioni che avviano workflow (futuro — Fase 4) |
| **Annulla** | Solo con nota obbligatoria di annullamento |
| **Storico** | Ogni modifica tracciata con utente, campo, valore vecchio/nuovo |
| **Navigazione** | Alt+← / Alt+→ tra ordini nella lista |

---

*Wireframe completato — Design originale KBM. © 2026 KBM Project.*
