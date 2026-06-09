# Wireframe 06 — Customer Detail (Dettaglio Cliente)

**Schermata**: Dettaglio / Modifica Cliente  
**Modulo**: Anagrafiche → Clienti → [Cliente]  
**Componenti**: KBMToolbar, KBMTabs, KBMForm, KBMAuditInfo, KBMLookup

---

## ASCII Wireframe — Vista Lettura

```
╔══════════════════════════════════════════════════════════════════════╗
║ ▣ KBM │ Demo Srl ▼                        🔔3  👤Mario Rossi▼  [?]║
╠════════╦═════════════════════════════════════════════════════════════╣
║🏠 Core ║  👥 Anagrafiche > Clienti > Rossi & C. Srl               ║
║        ╠═════════════════════════════════════════════════════════════╣
║👥 Ana. ║  ┌───────────────────────────────────────────────────────┐ ║
║ ├Clien.║  │[✏ Modifica] [📋 Duplica] [🗑 Elimina] [🖨 Stampa]    │ ║
║        ║  └───────────────────────────────────────────────────────┘ ║
║💲 List ║                                                             ║
║        ║  CLI001 — Rossi & C. Srl                    ● Attivo      ║
║────────║  P.IVA: IT01234567890  │  SDI: ABC1234  │  PEC: pec@... ║
║⚙ Amm. ║                                                             ║
║        ║  [Dati Generali] [Contatti] [Indirizzi] [Cond. Commerciali]║
║        ║   ──────────────                                           ║
║        ║                                                             ║
║        ║  ┌─────────────────────────────────────────────────────┐  ║
║        ║  │  TAB: DATI GENERALI                                  │  ║
║        ║  │                                                       │  ║
║        ║  │  Tipo Cliente *        Codice *                      │  ║
║        ║  │  [● Azienda ○ Persona]  [CLI001     ]               │  ║
║        ║  │                                                       │  ║
║        ║  │  Ragione Sociale *                                   │  ║
║        ║  │  [Rossi & C. Srl                              ]      │  ║
║        ║  │                                                       │  ║
║        ║  │  Partita IVA *         Codice Fiscale                │  ║
║        ║  │  [IT01234567890   ]    [                    ]       │  ║
║        ║  │                                                       │  ║
║        ║  │  Codice SDI (FE)       PEC                           │  ║
║        ║  │  [ABC1234        ]    [pec@rossiec.it        ]      │  ║
║        ║  │                                                       │  ║
║        ║  │  Indirizzo sede legale                               │  ║
║        ║  │  [Via Roma, 15                   ] Cap: [20100]     │  ║
║        ║  │  Città: [Milano          ] Prov: [MI] Nazione: [IT] │  ║
║        ║  │                                                       │  ║
║        ║  │  Categoria          Agente                           │  ║
║        ║  │  [Grossisti ▼   ] [🔍 M. Ferrari - Agente Nord]     │  ║
║        ║  │                                                       │  ║
║        ║  │  Note                                                │  ║
║        ║  │  [Cliente storico dal 2015. Pagamenti puntuali.   ]  │  ║
║        ║  │                                                       │  ║
║        ║  └─────────────────────────────────────────────────────┘  ║
║        ║                                                             ║
║        ║  Creato: 01/01/2015 da admin │ Modif.: 09/06/2026 10:00 ║
╠════════╩═════════════════════════════════════════════════════════════╣
║ Pronto │ CLI001 — Rossi & C. Srl — Lettura                         ║
╚══════════════════════════════════════════════════════════════════════╝
```

---

## Tab: Contatti

```
┌─────────────────────────────────────────────────────────────────┐
│  TAB: CONTATTI                               [+ Aggiungi]       │
│                                                                  │
│  ┌──┬──────────────────┬─────────────────┬──────────┬─────────┐│
│  │  │ Referente        │ Ruolo           │ Telefono │ Email   ││
│  ├──┼──────────────────┼─────────────────┼──────────┼─────────┤│
│  │⭐│ Marco Rossi      │ Titolare        │ 02-12345 │ m@r.it  ││  ← principale
│  │  │ Giulia Bianchi   │ Amm. Acquisti   │ 02-12346 │ g@r.it  ││
│  │  │ Antonio Verdi    │ Magazzino       │ 02-12347 │ a@r.it  ││
│  └──┴──────────────────┴─────────────────┴──────────┴─────────┘│
│  ⭐ = Contatto principale                                        │
└─────────────────────────────────────────────────────────────────┘
```

---

## Tab: Indirizzi

```
┌─────────────────────────────────────────────────────────────────┐
│  TAB: INDIRIZZI                              [+ Aggiungi]       │
│                                                                  │
│  ┌──┬────────────────┬──────────────────────┬─────────┬───────┐│
│  │  │ Tipo           │ Indirizzo            │ Città   │ Prov. ││
│  ├──┼────────────────┼──────────────────────┼─────────┼───────┤│
│  │⭐│ Sede Legale    │ Via Roma 15          │ Milano  │ MI    ││
│  │  │ Magazzino      │ Via Industria 30     │ Sesto   │ MI    ││
│  │  │ Fatturazione   │ = Sede Legale        │ —       │ —     ││
│  └──┴────────────────┴──────────────────────┴─────────┴───────┘│
│  Tipi: Sede Legale / Fatturazione / Spedizione / Magazzino      │
└─────────────────────────────────────────────────────────────────┘
```

---

## Tab: Condizioni Commerciali

```
┌─────────────────────────────────────────────────────────────────┐
│  TAB: CONDIZIONI COMMERCIALI                                     │
│                                                                  │
│  Listino prezzi *          Valuta *                             │
│  [Listino Standard ▼  ]  [EUR - Euro ▼          ]              │
│                                                                  │
│  Condizioni di pagamento *    Banca d'appoggio                  │
│  [30gg d.f.f.m. ▼        ]  [IBAN IT60X054280 ▼ ]             │
│                                                                  │
│  % Sconto cliente             Fido creditizio                   │
│  [  5,00 %               ]  [€ 10.000,00         ]             │
│                                                                  │
│  Modalità pagamento *         Tipo pagamento                    │
│  [RIBA ▼                 ]  [Ricevuta Bancaria ▼ ]             │
│                                                                  │
│  IVA predefinita              Esenzione IVA                     │
│  [22% - Ordinaria ▼      ]  [—                   ]             │
│                                                                  │
│  Agente *                                                        │
│  [🔍 Mario Ferrari — Agente Nord                              ] │
└─────────────────────────────────────────────────────────────────┘
```

---

## Specifiche UX

| Aspetto | Specifica |
|---|---|
| **Header** | Codice + Ragione Sociale + P.IVA + SDI + PEC ben visibili |
| **P.IVA validazione** | Real-time: algoritmo check P.IVA italiana |
| **CF validazione** | Real-time: algoritmo check CF (persone fisiche) |
| **Tipo toggle** | Azienda/Persona fisica cambia campi visibili (P.IVA vs CF) |
| **Contatto principale** | Solo uno; cambio con ⭐ click |
| **Indirizzo fatturazione** | Può essere uguale alla sede (flag "= sede legale") |
| **Lookup agente** | KBMLookup su entità Agenti |
| **Listino** | KBMLookup su Listini |
| **Condiz. pagamento** | KBMLookup su Termini Pagamento |
| **Salvataggio tab** | Salva tutte le tab insieme (Ctrl+S globale) |
| **Navigazione** | Alt+← / Alt+→ tra clienti in lista |
