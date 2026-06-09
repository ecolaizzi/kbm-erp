# Wireframe 05 — Customer List (Lista Clienti)

**Schermata**: Lista Clienti — Vista Griglia  
**Modulo**: Anagrafiche → Clienti  
**Componenti**: KBMDataGrid, KBMToolbar, KBMSearchBox, KBMFilterPanel, KBMStatusBadge

---

## ASCII Wireframe

```
╔══════════════════════════════════════════════════════════════════════╗
║ ▣ KBM │ Demo Srl ▼                        🔔3  👤Mario Rossi▼  [?]║
╠════════╦═════════════════════════════════════════════════════════════╣
║🏠 Core ║  👥 Anagrafiche > Clienti                                  ║
║        ╠═════════════════════════════════════════════════════════════╣
║👥 Ana. ║                                                             ║
║ ├Clien.║  Clienti     1.247 totali                                  ║
║ ├Forn. ║                                                             ║
║ └Artic.║  ┌───────────────────────────────────────────────────────┐ ║
║        ║  │[+ Nuovo] [✏ Modifica] [🗑 Elimina] [↗ Esporta] [📥 Importa]│║
║💲 List ║  └───────────────────────────────────────────────────────┘ ║
║        ║                                                             ║
║────────║  [Tutti] [Attivi] [Inattivi] [Con P.IVA] [Solo CF]        ║
║⚙ Amm. ║                                                             ║
║        ║  Cerca: [_________________________________] 🔍  [⚙ Filtri]║
║        ║                                                             ║
║        ║  ┌─┬──────┬────────────────────┬──────────────┬────────┬─┐║
║        ║  │☐│Codice│ Ragione Sociale     │ P.IVA/C.F.   │ Città  │ │║
║        ║  ├─┼──────┼────────────────────┼──────────────┼────────┼─┤║
║        ║  │☐│CLI001│ Rossi & C. Srl     │ 01234567890  │ Milano │✏🗑│║
║        ║  │☑│CLI002│ Alfa Trading SpA    │ 09876543210  │ Roma   │✏🗑│║
║        ║  │☐│CLI003│ Beta Commerciale    │ 05555555550  │ Torino │✏🗑│║
║        ║  │☐│CLI004│ Mario Bianchi       │ BNRMRA80A01  │ Napoli │✏🗑│║
║        ║  │☐│CLI005│ Gamma Distribution  │ 03333333330  │ Bologna│✏🗑│║
║        ║  │☐│CLI006│ Delta Services Srl  │ 07777777770  │ Firenze│✏🗑│║
║        ║  │☐│CLI007│ Epsilon Tech        │ 02222222220  │ Genova │✏🗑│║
║        ║  │☐│CLI008│ Zeta Impianti Srl   │ 08888888880  │ Venezia│✏🗑│║
║        ║  └─┴──────┴────────────────────┴──────────────┴────────┴─┘║
║        ║                                                             ║
║        ║  Pag 1/50  [◀ Prec] [Avanti ▶]  Mostra: [25 ▾]  1247 tot║
╠════════╩═════════════════════════════════════════════════════════════╣
║ Pronto │ Selezionati: 1 │ Ultima azione: Caricato 10:30:15         ║
╚══════════════════════════════════════════════════════════════════════╝
```

---

## Pannello Filtri Avanzati

```
┌─────────────────────────────────────────────────────────────────┐
│  ⚙ Filtri Avanzati Clienti                           [✕ Chiudi] │
├─────────────────────────────────────────────────────────────────┤
│  Ragione Sociale  [contiene] [_______________________]          │
│  P.IVA            [uguale  ] [_______________________]          │
│  Cod. Fiscale     [uguale  ] [_______________________]          │
│  Città            [contiene] [_______________________]          │
│  Provincia        [è       ] [MI ▼                  ]           │
│  Nazione          [è       ] [Italia ▼              ]           │
│  Tipo             [è       ] [Azienda ▼             ]           │
│  Agente           [è       ] [_______________________] 🔍       │
│  Stato            [è       ] [Attivo ▼              ]           │
│  Creato dal       [__/___/___] al [__/___/___]                  │
├─────────────────────────────────────────────────────────────────┤
│  Filtri salvati: [Clienti attivi MI] [Aziende con P.IVA]       │
│  [Applica]  [Resetta]  [Salva filtro...]                        │
└─────────────────────────────────────────────────────────────────┘
```

---

## Colonne Griglia

| Campo | Larghezza | Sortable | Note |
|---|---|---|---|
| ☐ (checkbox) | 40px | No | Selezione |
| Codice | 80px | Sì | Link click |
| Ragione Sociale | 250px | Sì | — |
| P.IVA / C.F. | 140px | Sì | Font mono |
| Tipo | 90px | Sì | Azienda / Persona fisica |
| Città | 120px | Sì | — |
| Provincia | 50px | Sì | Sigla |
| Agente | 120px | Sì | — |
| Stato | 80px | Sì | Badge Attivo/Inattivo |
| Azioni | 80px | No | ✏ 🗑 |

**Colonne default visibili**: ☐, Codice, Ragione Sociale, P.IVA, Città, Stato, Azioni

---

## Context Menu

```
┌──────────────────────────────┐
│ ✏ Modifica cliente          │
│ 📋 Duplica                  │
│ 📧 Invia email              │
│ ────────────────────────── │
│ 📊 Storico ordini (futuro)  │
│ 💰 Estratto conto (futuro)  │
│ ────────────────────────── │
│ 🗑 Elimina / Disattiva      │
└──────────────────────────────┘
```

---

## Azioni Bulk

```
■ 5 selezionati: [↗ Esporta] [📧 Email massiva] [🗑 Elimina]  [✕]
```

---

## Specifiche UX

| Aspetto | Specifica |
|---|---|
| **Cerca** | Ricerca su: codice, ragione sociale, P.IVA, CF, email, città |
| **P.IVA/CF** | Font monospaziale per allineamento; 11 cifre P.IVA / 16 CF |
| **Importa** | Apre wizard importazione Excel/CSV |
| **Default sort** | Ragione Sociale ASC |
| **Duplica** | Crea nuovo cliente copiando tutti i campi (richede nuovi P.IVA/CF) |
| **Elimina** | Soft delete; se ha ordini collegati: solo disattivazione |
| **Performance** | Server-side pagination; 10.000+ record supportati < 1s |
