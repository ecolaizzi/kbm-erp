# KBM Pipeline — Orchestrazione Multi-Agente Cursor

## Componenti

| Path | Descrizione |
|------|-------------|
| `week-01-manifest.yaml` | Batch Week 1 con dipendenze e handoff |
| `prompts/` | Template prompt per agente |
| `ralph/` | **Monitoraggio live** — dashboard + state.json |
| `scripts/pipeline-cli.mjs` | CLI aggiornamento stato |

## Avvio

1. **Dashboard Ralph** (monitoraggio tempo reale):
   ```powershell
   .\pipeline\ralph\serve.ps1
   ```
   Apri http://localhost:8765/pipeline/ralph/dashboard.html

2. **Esegui batch** (in Cursor): Supervisor carica `week-01-manifest.yaml`, lancia subagent per ogni task parallelo.

3. **Aggiorna stato** dopo ogni task:
   ```bash
   node pipeline/scripts/pipeline-cli.mjs status backend done "US-001"
   ```

## Modello Ralph

- `state.json` — stato machine-readable (polling 2s)
- `progress.md` — checklist umana
- `events` in state — log live nella dashboard
- Grafo SVG — agenti + frecce handoff animate
