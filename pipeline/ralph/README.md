# KBM Ralph — Monitoraggio pipeline in tempo reale

Sistema di monitoraggio stile **Ralph loop**: stato persistente, eventi append-only, dashboard live con grafo agenti e handoff.

## Avvio sviluppo iterativo

```powershell
.\pipeline\ralph\iterate.ps1 -Batch F
```

Riprende pipeline (status **running**), attiva batch, avvia heartbeat in background (PID in `heartbeat.pid`).

Ripristino stato Week 2:

```powershell
.\pipeline\ralph\sync-state.ps1
.\pipeline\ralph\iterate.ps1 -Batch F
```

## Avvio dashboard

```powershell
cd c:\Users\Administrator\Desktop\kbm-erp
.\pipeline\ralph\serve.ps1
```

Apri: **http://localhost:8765/pipeline/ralph/dashboard.html**

La dashboard fa polling di `state.json` ogni **2 secondi** e mostra:
- Grafo SVG degli 13 agenti con frecce handoff animate
- Batch settimanali (W1: A-D, W2: E-H)
- Heartbeat ogni 60s mantiene stato "Live"
- Event log live
- Progress bar globale

## Aggiornare stato (da agenti Cursor o script)

```powershell
# Aggiorna stato agente
.\pipeline\scripts\pipeline-cli.ps1 status devops active "US-INFRA-01 scaffold"
.\pipeline\scripts\pipeline-cli.ps1 event supervisor "Task completato"
.\pipeline\scripts\pipeline-cli.ps1 pause "In attesa input utente"
.\pipeline\scripts\pipeline-cli.ps1 resume "Ripresa Batch B"

# Heartbeat (mantiene dashboard viva — opzionale in background)
.\pipeline\ralph\heartbeat.ps1
node pipeline/scripts/pipeline-cli.mjs handoff devops backend "src/ scaffold" in_progress
node pipeline/scripts/pipeline-cli.mjs task US-INFRA-01 in_progress
node pipeline/scripts/pipeline-cli.mjs batch A done
node pipeline/scripts/pipeline-cli.mjs progress 25
node pipeline/scripts/pipeline-cli.mjs event supervisor "Batch B avviato"
```

## File

| File | Ruolo |
|------|-------|
| `state.json` | Stato corrente (agenti, batch, handoff, task, eventi) |
| `progress.md` | Checklist Ralph leggibile |
| `dashboard.html` | UI live |
| `serve.ps1` | Server HTTP locale |

## Integrazione agenti

Ogni agente Cursor deve aggiornare lo stato all'inizio e fine task (vedi `AGENTS.md`).
