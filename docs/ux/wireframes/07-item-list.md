# Wireframe 07 — Item List (Lista Articoli)

**Schermata**: Lista Articoli — Vista Griglia  
**Modulo**: Anagrafiche → Articoli  
**Componenti**: KBMDataGrid, KBMToolbar, KBMSearchBox, KBMFilterPanel, KBMStatusBadge

---

## ASCII Wireframe

```
╔══════════════════════════════════════════════════════════════════════╗
║ ▣ KBM │ Demo Srl ▼                        🔔3  👤Mario Rossi▼  [?]║
╠════════╦═════════════════════════════════════════════════════════════╣
║🏠 Core ║  👥 Anagrafiche > Articoli                                 ║
║        ╠═════════════════════════════════════════════════════════════╣
║👥 Ana. ║                                                             ║
║ ├Clien.║  Articoli     8.432 totali                                 ║
║ ├Forn. ║                                                             ║
║ └Artic.║  ┌──────────────────────────────────────────────────────┐  ║
║        ║  │[+Nuovo] [✏Modifica] [🗑Elimina] [↗Esporta] [📥Import]│  ║
║💲 List ║  └──────────────────────────────────────────────────────┘  ║
║        ║                                                             ║
║────────║  [Tutti] [Attivi] [Inattivi] [Con giacenza] [Sotto minimo]║
║⚙ Amm. ║                                                             ║
║        ║  Cerca: [_______________________________] 🔍  [⚙ Filtri]  ║
║        ║                                                             ║
║        ║  ┌─┬──────┬──────────────────────────┬────────┬───────┬──┐║
║        ║  │☐│Codice│ Descrizione               │ U.M.   │ Categ │  │║
║        ║  ├─┼──────┼──────────────────────────┼────────┼───────┼──┤║
║        ║  │☐│ART001│ Vite M5×20 Zincata        │ PZ     │ Viterie│✏🗑│║
║        ║  │☐│ART002│ Dado M5 Zincato            │ PZ     │ Viterie│✏🗑│║
║        ║  │☐│ART003│ Lastra Acciaio 100×100×2  │ KG     │ Lamiere│✏🗑│║
║        ║  │☑│ART004│ Olio lubrificante 5L      │ LT     │ Chimici│✏🗑│║
║        ║  │☐│ART005│ Guanto protezione L       │ PA     │ DPI    │✏🗑│║
║        ║  │☐│ART006│ Cavo elettrico 3×1.5mm    │ MT     │ Elettr.│✏🗑│║
║        ║  │☐│ART007│ Bullone M8×30 Inox        │ PZ     │ Viterie│✏🗑│║
║        ║  └─┴──────┴──────────────────────────┴────────┴───────┴──┘║
║        ║                                                             ║
║        ║  Pag 1/338  [◀ Prec] [Avanti ▶] Mostra:[25▾] 8432 tot   ║
╠════════╩═════════════════════════════════════════════════════════════╣
║ Pronto │ Selezionati: 1 │ Ultima azione: Caricato 10:35:00         ║
╚══════════════════════════════════════════════════════════════════════╝
```

---

## Filtri Avanzati

```
┌─────────────────────────────────────────────────────────────────┐
│  ⚙ Filtri Avanzati Articoli                          [✕ Chiudi] │
├─────────────────────────────────────────────────────────────────┤
│  Codice           [contiene] [_____________________]            │
│  Descrizione      [contiene] [_____________________]            │
│  Barcode          [uguale  ] [_____________________]            │
│  Categoria        [è       ] [Viterie ▼            ]            │
│  Gruppo merc.     [è       ] [Ferramenta ▼         ]            │
│  Unità di misura  [è       ] [PZ ▼                 ]            │
│  Stato            [è       ] [Attivo ▼              ]            │
│  Prezzo da        [€_______] a [€_______]                       │
│  Cod. fornitore   [contiene] [_____________________]            │
├─────────────────────────────────────────────────────────────────┤
│  [Applica]  [Resetta]  [Salva filtro...]                        │
└─────────────────────────────────────────────────────────────────┘
```

---

## Colonne Griglia

| Campo | Larghezza | Sortable | Note |
|---|---|---|---|
| ☐ | 40px | No | Selezione |
| Codice | 90px | Sì | Font mono, link |
| Barcode | 110px | Sì | EAN-13 — font mono |
| Descrizione | 280px | Sì | — |
| U.M. | 60px | Sì | Unità di misura |
| Categoria | 120px | Sì | — |
| Gruppo Merceol. | 120px | Sì | — |
| Prezzo vendita | 100px | Sì | Formato valuta |
| Stato | 80px | Sì | Attivo/Inattivo |
| Azioni | 80px | No | ✏ 🗑 |

**Colonne default**: ☐, Codice, Descrizione, U.M., Categoria, Azioni

---

## Quick Filters

| Filtro | Condizione |
|---|---|
| **Tutti** | Nessun filtro |
| **Attivi** | Stato = Attivo |
| **Inattivi** | Stato = Inattivo |
| **Con giacenza** | Giacenza > 0 (futuro Fase 5) |
| **Sotto minimo** | Giacenza < scorta minima (futuro Fase 5) |

---

## Specifiche UX

| Aspetto | Specifica |
|---|---|
| **Cerca** | Codice, barcode, descrizione, cod. fornitore |
| **Barcode** | Font monospaziale per allineamento |
| **Import** | Wizard per importazione da Excel (migrazione) |
| **Default sort** | Codice ASC |
| **Elimina** | Solo se non in uso (no ordini collegati) |
| **Performance** | 10.000+ articoli con virtual scrolling |
