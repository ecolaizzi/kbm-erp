# KBM — Agenti Cursor (Pipeline Multi-Agente)

13 agenti specializzati orchestrati dal **Supervisor**. Monitoraggio live: [`pipeline/ralph/dashboard.html`](pipeline/ralph/dashboard.html).

## Avvio monitoraggio Ralph

```powershell
.\pipeline\ralph\serve.ps1
# → http://localhost:8765/pipeline/ralph/dashboard.html
```

## Regola obbligatoria per ogni agente

All'inizio e fine di ogni task, aggiorna lo stato:

```powershell
.\pipeline\scripts\pipeline-cli.ps1 status backend active "US-001"
.\pipeline\scripts\pipeline-cli.ps1 handoff backend frontend "auth API" in_progress
.\pipeline\scripts\pipeline-cli.ps1 task US-001 in_progress
```

(Node: `pipeline/scripts/pipeline-cli.mjs` se disponibile)

## Agenti

| ID | Rule | Quando usarlo |
|----|------|---------------|
| `supervisor` | `.cursor/rules/kbm-supervisor.mdc` | Sempre — orchestrazione, gate review, SCR |
| `chief-architect` | `kbm-architect.mdc` | ADR, architettura, review shared areas |
| `database-architect` | `kbm-database-architect.mdc` | Schema, migrations |
| `security-architect` | `kbm-security-architect.mdc` | Auth, RBAC, `KBM.Security` |
| `product-owner` | `kbm-product-owner.mdc` | Backlog, priorità settimanale |
| `ux-designer` | `kbm-ux.mdc` | UI WPF, wireframes |
| `devops` | `kbm-devops.mdc` | CI/CD, scaffold, `.github/` |
| `backend` | `kbm-backend.mdc` | API, business logic |
| `frontend` | `kbm-frontend.mdc` | WPF client |
| `module-coder` | `kbm-module-coder.mdc` | `KBM.Modules.*` |
| `qa` | `kbm-qa.mdc` | `tests/` |
| `documentation` | `kbm-docs.mdc` | Sync `docs/` |

## Flusso Week 1

Vedi [`pipeline/week-01-manifest.yaml`](pipeline/week-01-manifest.yaml):

```
Batch A (parallelo): DevOps + DB Architect review
    ↓ handoff: src/ scaffold
Batch B (parallelo): Backend + Frontend
    ↓ handoff: auth API contract
Batch C: migrations + Setup Wizard + QA
    ↓ handoff: test report
Batch D: Supervisor gate review
```

## Decisioni canoniche

Vedi [`docs/governance/canonical-decisions.md`](docs/governance/canonical-decisions.md).

- WPF desktop, Modular Monolith, .NET 8, SQL Server
- Argon2id + JWT RS256
- **MFA/TOTP: fine progetto** (non Week 1-8)
- DB dev: `kbmdbdev` su `95.110.142.60` (secrets locali)

## Shared areas (SCR obbligatoria)

`KBM.Core`, `KBM.Shared`, `KBM.Security`, migrations, API contracts → creare SCR in `docs/governance/shared-change-requests/`.
