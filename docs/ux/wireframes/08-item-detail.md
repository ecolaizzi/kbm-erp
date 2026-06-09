# Wireframe 08 — Item Detail (Dettaglio Articolo)

**Schermata**: Dettaglio / Modifica Articolo  
**Modulo**: Anagrafiche → Articoli → [Articolo]  
**Componenti**: KBMToolbar, KBMTabs, KBMForm, KBMAuditInfo

---

## ASCII Wireframe — Vista Lettura

```
╔══════════════════════════════════════════════════════════════════════╗
║ ▣ KBM │ Demo Srl ▼                        🔔3  👤Mario Rossi▼  [?]║
╠════════╦═════════════════════════════════════════════════════════════╣
║🏠 Core ║  👥 Anagrafiche > Articoli > Vite M5×20 Zincata           ║
║        ╠═════════════════════════════════════════════════════════════╣
║👥 Ana. ║  ┌───────────────────────────────────────────────────────┐ ║
║ └Artic.║  │[✏ Modifica] [📋 Duplica] [🗑 Elimina] [🖨 Stampa]    │ ║
║        ║  └───────────────────────────────────────────────────────┘ ║
║💲 List ║                                                             ║
║        ║  ART001 — Vite M5×20 Zincata                 ● Attivo     ║
║────────║  Barcode: 8001234567890  │  U.M.: PZ  │  Cat.: Viterie   ║
║⚙ Amm. ║                                                             ║
║        ║  [Dati Generali] [Prezzi] [Codici Alternativi] [Note]     ║
║        ║   ──────────────                                           ║
║        ║                                                             ║
║        ║  ┌─────────────────────────────────────────────────────┐  ║
║        ║  │  TAB: DATI GENERALI                                  │  ║
║        ║  │                                                       │  ║
║        ║  │  Codice articolo *       Stato                       │  ║
║        ║  │  [ART001          ]      [● Attivo ○ Inattivo]      │  ║
║        ║  │                                                       │  ║
║        ║  │  Descrizione *                                        │  ║
║        ║  │  [Vite M5×20 Zincata                           ]     │  ║
║        ║  │                                                       │  ║
║        ║  │  Descrizione aggiuntiva (lunga)                       │  ║
║        ║  │  [Vite a testa esagonale M5×20mm,               ]   │  ║
║        ║  │  [zincatura elettrolitica, classe 8.8            ]   │  ║
║        ║  │                                                       │  ║
║        ║  │  Barcode (EAN)            Cod. fornitore              │  ║
║        ║  │  [8001234567890    ]      [VIT-M5-20-ZN        ]     │  ║
║        ║  │                                                       │  ║
║        ║  │  Unità di misura *        U.M. acquisto               │  ║
║        ║  │  [PZ — Pezzo ▼    ]      [CF — Confezione ▼   ]     │  ║
║        ║  │  Conversione: 1 CF = 100 PZ                          │  ║
║        ║  │                                                       │  ║
║        ║  │  Categoria *              Gruppo merceologico          │  ║
║        ║  │  [🔍 Viterie              ] [🔍 Ferramenta      ]    │  ║
║        ║  │                                                       │  ║
║        ║  │  Aliquota IVA *           Conto contabile             │  ║
║        ║  │  [22% - Ordinaria ▼  ]  [501000 - Merci ▼      ]    │  ║
║        ║  │                                                       │  ║
║        ║  │  Peso lordo (kg)          Volume (m³)                 │  ║
║        ║  │  [   0,012         ]      [   0,0001          ]      │  ║
║        ║  │                                                       │  ║
║        ║  └─────────────────────────────────────────────────────┘  ║
║        ║                                                             ║
║        ║  Creato: 15/03/2024 da admin │ Modif.: 09/06/2026 09:00  ║
╠════════╩═════════════════════════════════════════════════════════════╣
║ Pronto │ ART001 — Vite M5×20 Zincata — Lettura                     ║
╚══════════════════════════════════════════════════════════════════════╝
```

---

## Tab: Prezzi

```
┌─────────────────────────────────────────────────────────────────┐
│  TAB: PREZZI                                                     │
│                                                                  │
│  ── Prezzi di Vendita ─────────────────────────────────────   │
│                                                                  │
│  ┌─────────────────────────┬──────────────┬──────────────────┐ │
│  │ Listino                 │ Prezzo netto │ Valuta           │ │
│  ├─────────────────────────┼──────────────┼──────────────────┤ │
│  │ Listino Standard        │ € 0,045      │ EUR              │ │
│  │ Listino Grossisti       │ € 0,038      │ EUR              │ │
│  │ Listino Export          │ $ 0,050      │ USD              │ │
│  └─────────────────────────┴──────────────┴──────────────────┘ │
│                                                                  │
│  ── Prezzi di Acquisto ────────────────────────────────────   │
│                                                                  │
│  ┌─────────────────────────┬──────────────┬──────────────────┐ │
│  │ Fornitore               │ Prezzo acquis│ Valuta           │ │
│  ├─────────────────────────┼──────────────┼──────────────────┤ │
│  │ 🔍 Acme Supply SpA      │ € 0,025      │ EUR              │ │
│  │ 🔍 Global Parts Srl     │ € 0,028      │ EUR              │ │
│  └─────────────────────────┴──────────────┴──────────────────┘ │
│  [+ Aggiungi prezzo fornitore]                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## Tab: Codici Alternativi

```
┌─────────────────────────────────────────────────────────────────┐
│  TAB: CODICI ALTERNATIVI                     [+ Aggiungi]       │
│                                                                  │
│  ┌───────────────┬──────────────────┬──────────────────────────┐│
│  │ Tipo          │ Codice           │ Descrizione tipo         ││
│  ├───────────────┼──────────────────┼──────────────────────────┤│
│  │ Barcode EAN   │ 8001234567890    │ EAN-13 principale        ││
│  │ Cod. fornitore│ VIT-M5-20-ZN    │ Acme Supply SpA          ││
│  │ Cod. cliente  │ C-BOLT-05-020   │ Rossi & C. Srl           ││
│  └───────────────┴──────────────────┴──────────────────────────┘│
└─────────────────────────────────────────────────────────────────┘
```

---

## Specifiche UX

| Aspetto | Specifica |
|---|---|
| **Barcode** | Font monospaziale; validazione EAN-13 checksum |
| **U.M. conversione** | Se U.M. acquisto ≠ U.M. vendita: campo conversione obbligatorio |
| **Prezzo netto** | Formato `€ 0,045` con 3-4 decimali per articoli piccoli |
| **IVA** | KBMLookup su tabella aliquote IVA |
| **Duplica** | Copia tutti i campi, nuovo codice richiesto |
| **Peso/Volume** | Utili per calcolo spedizioni (futuro) |
| **Conto contabile** | Lookup su piano dei conti (futuro Fase 6) |
