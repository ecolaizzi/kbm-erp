# Wireframe 04 — User Detail (Dettaglio Utente)

**Schermata**: Dettaglio / Modifica Utente  
**Modulo**: Amministrazione → Utenti → [Utente]  
**Componenti**: KBMToolbar, KBMTabs, KBMForm, KBMAuditInfo, KBMStatusBadge

---

## ASCII Wireframe — Vista Lettura

```
╔══════════════════════════════════════════════════════════════════════╗
║ ▣ KBM │ Demo Srl ▼                        🔔3  👤Mario Rossi▼  [?]║
╠════════╦═════════════════════════════════════════════════════════════╣
║🏠 Core ║  ⚙ Amm. > 👤 Utenti > Mario Rossi                        ║
║        ╠═════════════════════════════════════════════════════════════╣
║👥 Ana. ║                                                             ║
║        ║  ┌───────────────────────────────────────────────────────┐ ║
║💲 List ║  │[✏ Modifica] [🔑 Reset Pwd] [🔒 Blocca] [🗑 Elimina] │ ║
║        ║  └───────────────────────────────────────────────────────┘ ║
║────────║                                                             ║
║⚙ Amm. ║  👤 Mario Rossi                           ● Attivo         ║
║ ├Utenti║  Username: m.rossi  │  Ruolo: Manager                     ║
║        ║                                                             ║
║        ║  [Dati Generali] [Ruoli e Permessi] [Storico Accessi]     ║
║        ║   ──────────────                                           ║
║        ║                                                             ║
║        ║  ┌─────────────────────────────────────────────────────┐  ║
║        ║  │  TAB: DATI GENERALI                                  │  ║
║        ║  │                                                       │  ║
║        ║  │  Nome *           Cognome *                          │  ║
║        ║  │  [Mario         ] [Rossi          ]                  │  ║
║        ║  │                                                       │  ║
║        ║  │  Email *                                              │  ║
║        ║  │  [mario.rossi@demo.it                            ]   │  ║
║        ║  │                                                       │  ║
║        ║  │  Telefono                                             │  ║
║        ║  │  [+39 02 1234567                                 ]   │  ║
║        ║  │                                                       │  ║
║        ║  │  Lingua UI        Fuso Orario                        │  ║
║        ║  │  [Italiano ▼   ] [Europe/Rome ▼               ]     │  ║
║        ║  │                                                       │  ║
║        ║  │  MFA                                                  │  ║
║        ║  │  ● Abilitato — TOTP (Google Authenticator)           │  ║
║        ║  │  [Disabilita MFA]                                    │  ║
║        ║  │                                                       │  ║
║        ║  │  Aziende abilitate                                   │  ║
║        ║  │  [✓] Demo Srl     [ ] Alfa Trading    [ ] Beta Dist. │  ║
║        ║  │                                                       │  ║
║        ║  └─────────────────────────────────────────────────────┘  ║
║        ║                                                             ║
║        ║  Creato: 01/01/2025 09:00 da admin │ Modif.: 05/06/2026  ║
╠════════╩═════════════════════════════════════════════════════════════╣
║ Pronto │ Mario Rossi — Lettura                                      ║
╚══════════════════════════════════════════════════════════════════════╝
```

---

## Tab: Ruoli e Permessi

```
┌─────────────────────────────────────────────────────────────────────┐
│  TAB: RUOLI E PERMESSI                                              │
│                                                                     │
│  Ruolo assegnato *                                                  │
│  ○ Amministratore ERP    ● Manager    ○ Utente    ○ Sola lettura  │
│                                                                     │
│  Permessi modulo (ereditati dal ruolo + override utente):          │
│                                                                     │
│  Modulo          │ Leggi │ Crea │ Modifica │ Elimina │ Admin       │
│  ─────────────   │ ───── │ ─── │ ──────── │ ─────── │ ─────       │
│  Anagrafiche     │  ✓    │  ✓  │    ✓     │    ✓    │   ✗         │
│  Clienti         │  ✓    │  ✓  │    ✓     │    ✗    │   ✗         │
│  Fornitori       │  ✓    │  ✓  │    ✓     │    ✗    │   ✗         │
│  Articoli        │  ✓    │  ✓  │    ✓     │    ✓    │   ✗         │
│  Listini         │  ✓    │  ✗  │    ✗     │    ✗    │   ✗         │
│  Amministrazione │  ✗    │  ✗  │    ✗     │    ✗    │   ✗         │
│  Audit Log       │  ✓    │  ✗  │    ✗     │    ✗    │   ✗         │
│                                                                     │
│  Legenda: ✓ = permesso dal ruolo  [✓] = override abilitato         │
│           ✗ = negato              [✗] = override disabilitato      │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Tab: Storico Accessi

```
┌─────────────────────────────────────────────────────────────────────┐
│  TAB: STORICO ACCESSI                                               │
│                                                                     │
│  Data/Ora            │ IP              │ Esito     │ Azienda        │
│  ─────────────────   │ ─────────────  │ ─────── │ ─────────────  │
│  09/06/2026 10:15   │ 192.168.1.50   │ ✓ OK    │ Demo Srl       │
│  09/06/2026 08:30   │ 192.168.1.50   │ ✓ OK    │ Demo Srl       │
│  08/06/2026 16:45   │ 10.0.0.12      │ ✓ OK    │ Alfa Trading   │
│  07/06/2026 09:00   │ 192.168.1.50   │ ✗ FAIL  │ —              │
│  07/06/2026 08:58   │ 192.168.1.50   │ ✗ FAIL  │ —              │
│  06/06/2026 14:30   │ 192.168.1.50   │ ✓ OK    │ Demo Srl       │
│                                                                     │
│  [Esporta storico]                            Pag 1/5 ◀ Avanti ▶  │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Vista Modifica (stessa schermata, campi editabili)

```
┌───────────────────────────────────────────────────────────────────┐
│[💾 Salva] [✕ Annulla] [🗑 Elimina]                               │
└───────────────────────────────────────────────────────────────────┘

  Nome *              Cognome *
  [Mario           ] [Rossi              ]

  Email *
  [mario.rossi@demo.it                                           ]
  ← bordo normale (nessun errore)

  Password (lascia vuoto per non modificare)
  [                                                              ] 👁
```

---

## Specifiche UX

| Aspetto | Specifica |
|---|---|
| **Header utente** | Avatar iniziali + nome + status badge |
| **Modifica password** | Campo separato, conferma richiesta |
| **Reset password** | Email inviata all'utente, conferma modal |
| **MFA toggle** | Richiede conferma dell'admin |
| **Tab Permessi** | Read-only per utente non-admin |
| **Navigazione** | Alt+← record precedente, Alt+→ successivo |
| **Unsaved guard** | Se modifica non salvata: "Vuoi salvare?" |
| **Elimina** | Soft delete con conferma modal |
