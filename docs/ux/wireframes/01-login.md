# Wireframe 01 — Login Screen

**Schermata**: Login / Accesso al sistema  
**Componenti**: KBMForm, KBMToast  
**Risoluzione target**: 1366×768 minimo

---

## ASCII Wireframe

```
╔══════════════════════════════════════════════════════════════════════╗
║                                                                      ║
║                                                                      ║
║                    ┌─────────────────────────────┐                  ║
║                    │                             │                  ║
║                    │      ▣  KBM               │                  ║
║                    │   Kever Business Manager    │                  ║
║                    │                             │                  ║
║                    ├─────────────────────────────┤                  ║
║                    │                             │                  ║
║                    │  Utente                     │                  ║
║                    │  ┌─────────────────────┐   │                  ║
║                    │  │ mario.rossi         │   │                  ║
║                    │  └─────────────────────┘   │                  ║
║                    │                             │                  ║
║                    │  Password                   │                  ║
║                    │  ┌─────────────────────┐   │                  ║
║                    │  │ ••••••••••••••      │👁 │                  ║
║                    │  └─────────────────────┘   │                  ║
║                    │                             │                  ║
║                    │  ☐ Ricordami su questo PC  │                  ║
║                    │                             │                  ║
║                    │  ┌─────────────────────┐   │                  ║
║                    │  │      ACCEDI          │   │                  ║
║                    │  └─────────────────────┘   │                  ║
║                    │                             │                  ║
║                    │  Password dimenticata?       │                  ║
║                    │                             │                  ║
║                    └─────────────────────────────┘                  ║
║                                                                      ║
║                    ┌─────────────────────────────┐                  ║
║                    │ ── oppure ──                 │                  ║
║                    │  Codice di verifica (MFA)   │                  ║
║                    │  ┌─────────────────────┐   │                  ║
║                    │  │  _ _ _ _ _ _        │   │                  ║
║                    │  └─────────────────────┘   │                  ║
║                    │  [VERIFICA]  [Invia nuovo] │                  ║
║                    └─────────────────────────────┘                  ║
║                                                                      ║
║   KBM v1.0 — © 2026 Kever Srl  ·  Privacy Policy · Assistenza     ║
╚══════════════════════════════════════════════════════════════════════╝
```

---

## Selezione Azienda (post-login, se multi-azienda)

```
╔══════════════════════════════════════════════════════════════════════╗
║  ▣ KBM — Seleziona Azienda                                         ║
╠══════════════════════════════════════════════════════════════════════╣
║                                                                      ║
║  Benvenuto, Mario Rossi. Seleziona l'azienda di lavoro:            ║
║                                                                      ║
║  ┌─────────────────────────────────────────────────────────────┐   ║
║  │  🏢  Demo Srl                     P.IVA: 01234567890       │   ║
║  │      Ultimo accesso: ieri 15:30                             │   ║
║  ├─────────────────────────────────────────────────────────────┤   ║
║  │  🏢  Alfa Trading SpA             P.IVA: 09876543210       │   ║
║  │      Ultimo accesso: 3 giorni fa                            │   ║
║  ├─────────────────────────────────────────────────────────────┤   ║
║  │  🏢  Beta Distribuzione Srl       P.IVA: 05555555550       │   ║
║  │      Ultimo accesso: mai                                    │   ║
║  └─────────────────────────────────────────────────────────────┘   ║
║                                                                      ║
║                              [Continua]                             ║
╚══════════════════════════════════════════════════════════════════════╝
```

---

## Specifiche UX

| Aspetto | Specifica |
|---|---|
| **Background** | Gradiente neutro o solido `--kbm-gray-100` |
| **Card login** | Centrata, max-width 400px, shadow-lg |
| **Logo** | SVG inline, non immagine bitmap |
| **Input utente** | type="text" autocomplete="username" |
| **Input password** | type="password" autocomplete="current-password" |
| **Toggle mostra pw** | 👁 icona a destra campo password |
| **Bottone Accedi** | Full-width, variant primary, 40px height |
| **MFA Panel** | Appare solo se MFA abilitato per utente |
| **Input OTP** | 6 campi separati o singolo campo con validazione |
| **Errori** | Toast rosso + messaggio sotto campo errato |
| **Loading** | Spinner nel bottone Accedi durante chiamata API |
| **Selezione azienda** | Lista card cliccabili; Enter su riga selezionata |
| **Keyboard flow** | Username → Tab → Password → Tab → Checkbox → Tab → Accedi → Enter |

---

## Stati

| Stato | Comportamento |
|---|---|
| **Errore credenziali** | "Nome utente o password errati" (no distinzione per sicurezza) |
| **Account bloccato** | "Account bloccato dopo N tentativi. Contatta l'amministratore." |
| **MFA richiesto** | Mostra sezione OTP dopo login OK |
| **OTP errato** | "Codice non valido. Riprova." |
| **OTP scaduto** | "Codice scaduto. Invia nuovo codice." |
| **Password scaduta** | Redirect a pagina cambio password |
| **Sessione scaduta** | Torna al login con toast "Sessione scaduta" |
