# Wireframe 09 — Sales Order List (Lista Ordini Cliente)

**Schermata**: Lista Ordini Cliente — Vista Griglia  
**Modulo**: Vendite → Ordini Cliente (Fase 4 — placeholder MVP)  
**Componenti**: KBMDataGrid, KBMToolbar, KBMSearchBox, KBMFilterPanel, KBMStatusBadge

> **Nota MVP**: Questa schermata è un placeholder per Fase 4 (Ciclo Attivo). Il design è definito ora per coerenza ma non implementato nel MVP Fase 1-2. Il wireframe serve come riferimento per il team di sviluppo.

---

## ASCII Wireframe

```
╔══════════════════════════════════════════════════════════════════════╗
║ ▣ KBM │ Demo Srl ▼                        🔔3  👤Mario Rossi▼  [?]║
╠════════╦═════════════════════════════════════════════════════════════╣
║🏠 Core ║  🛒 Vendite > Ordini Cliente                               ║
║        ╠═════════════════════════════════════════════════════════════╣
║👥 Ana. ║                                                             ║
║        ║  Ordini Cliente     287 totali │ Da evadere: 45            ║
║💲 List ║                                                             ║
║────────║  ┌──────────────────────────────────────────────────────┐  ║
║🛒 Vend.║  │[+Nuovo] [✏Modifica] [🗑Elimina] [📄Evasione] [↗Esporta]│ ║
║ ├Ordini║  └──────────────────────────────────────────────────────┘  ║
║ ├DDT   ║                                                             ║
║ └Fatt. ║  [Tutti] [Aperti] [Da evadere] [Evasi] [Questo mese]     ║
║        ║                                                             ║
║⚙ Amm. ║  Cerca: [_______________________________] 🔍  [⚙ Filtri]  ║
║        ║                                                             ║
║        ║  ┌─┬──────────┬────────┬──────────────────┬────────┬────┐ ║
║        ║  │☐│ N° Ordine│ Data   │ Cliente           │ Totale │ St.│ ║
║        ║  ├─┼──────────┼────────┼──────────────────┼────────┼────┤ ║
║        ║  │☐│ OC24/001 │01/06/24│ Rossi & C. Srl   │€ 1.250 │●Ap.│ ║
║        ║  │☐│ OC24/002 │02/06/24│ Alfa Trading SpA  │€ 3.500 │●Ap.│ ║
║        ║  │☑│ OC24/003 │02/06/24│ Beta Commerciale  │€   890 │◑Pa.│ ║
║        ║  │☐│ OC24/004 │03/06/24│ Gamma Distrib.    │€ 5.200 │●Ev.│ ║
║        ║  │☐│ OC24/005 │03/06/24│ Delta Services    │€ 2.100 │●Ap.│ ║
║        ║  │☐│ OC24/006 │04/06/24│ Epsilon Tech      │€ 4.300 │●Ap.│ ║
║        ║  │☐│ OC24/007 │05/06/24│ Zeta Impianti     │€   750 │✗An.│ ║
║        ║  └─┴──────────┴────────┴──────────────────┴────────┴────┘ ║
║        ║                                                             ║
║        ║  Pag 1/12  [◀ Prec] [Avanti ▶] Mostra:[25▾] 287 totali  ║
╠════════╩═════════════════════════════════════════════════════════════╣
║ Pronto │ Sel.: 1 │ Tot. selezionati: € 890,00 │ 10:40:00           ║
╚══════════════════════════════════════════════════════════════════════╝
```

---

## Status Badge Ordini

```
● Aperto        → Blue    "Aperto"
◑ Parz. Evaso  → Amber   "Parz. Evaso"
● Evaso         → Green   "Evaso"
✗ Annullato    → Red     "Annullato"
⊙ Bozza        → Gray    "Bozza"
```

---

## Filtri Avanzati

```
┌─────────────────────────────────────────────────────────────────┐
│  ⚙ Filtri Avanzati — Ordini Cliente              [✕ Chiudi]    │
├─────────────────────────────────────────────────────────────────┤
│  N° Ordine      [uguale  ] [_____________________]              │
│  Cliente        [è       ] [🔍 Cerca cliente...  ]              │
│  Agente         [è       ] [🔍 Cerca agente...   ]              │
│  Data ordine    [dal     ] [__/___/____] al [__/___/____]       │
│  Data consegna  [dal     ] [__/___/____] al [__/___/____]       │
│  Stato          [è       ] [Aperto ▼             ]              │
│  Totale da      [€ _____] a [€ _____]                           │
│  Articolo       [contiene] [🔍 Cerca articolo... ]              │
│  Destinazione   [contiene] [_____________________]              │
├─────────────────────────────────────────────────────────────────┤
│  Filtri salvati: [Aperti questo mese] [Da evadere oggi]         │
│  [Applica]  [Resetta]  [Salva filtro...]                        │
└─────────────────────────────────────────────────────────────────┘
```

---

## Colonne Griglia

| Campo | Larghezza | Sortable | Note |
|---|---|---|---|
| ☐ | 40px | No | Selezione |
| N° Ordine | 100px | Sì | Font mono, link |
| Data ordine | 95px | Sì | gg/mm/aaaa |
| Data consegna | 95px | Sì | gg/mm/aaaa |
| Cliente | 220px | Sì | — |
| Agente | 120px | Sì | — |
| Imponibile | 110px | Sì | Allineato destra |
| IVA | 90px | Sì | Allineato destra |
| Totale | 110px | Sì | **Bold**, destra |
| Stato | 100px | Sì | Badge colorato |
| Azioni | 80px | No | ✏ 🗑 |

**Colonne default**: ☐, N°, Data, Cliente, Totale, Stato, Azioni

---

## Status Bar con Aggregati

Quando righe selezionate:
```
Pronto │ Selezionati: 3 │ Tot. imponibile: € 5.640,00 │ Tot. IVA: € 1.240,80 │ Totale: € 6.880,80
```

---

## Azioni Bulk Ordini

```
■ 3 selezionati: [📄 Crea DDT] [📧 Invia conferma] [↗ Esporta] [🗑 Elimina] [✕]
```

---

## Context Menu

```
┌──────────────────────────────┐
│ ✏ Modifica ordine           │
│ 📋 Duplica ordine           │
│ ────────────────────────── │
│ 📄 Crea DDT                │
│ 🧾 Crea Fattura             │
│ ────────────────────────── │
│ 📧 Invia conferma email     │
│ 🖨 Stampa ordine            │
│ ────────────────────────── │
│ ✗ Annulla ordine            │
│ 🗑 Elimina                  │
└──────────────────────────────┘
```

---

## Specifiche UX

| Aspetto | Specifica |
|---|---|
| **Totali** | Sempre con 2 decimali e simbolo valuta |
| **Aggregati status bar** | Calcolati su selezione (non tutta la query) |
| **Data consegna** | Colorata in rosso se scaduta, ambra se oggi |
| **Evasione** | Azione multipla per creare DDT da ordini selezionati |
| **Default sort** | Data ordine DESC (più recente in cima) |
| **Modulo disabilitato MVP** | Nel menu: grigio + tooltip "Disponibile Fase 4" |
