# KBM - File Ownership Matrix

**Versione**: 1.0  
**Data**: 2026-06-09  
**Stato**: FASE 0 - Research & Foundation

---

## Scopo

Questo documento definisce chi può modificare quali file/cartelle per prevenire collisioni nella codebase tra agenti.

---

## Ownership Rules

| Path/Pattern | Owner Primario | Reviewer Obbligatori | Modifica Consentita |
|---|---|---|---|
| **VFS Governance** |
| `vfs://org/kbm/governance/**` | Supervisor | N/A | Solo Supervisor |
| **VFS Deliverables - Fase 0** |
| `vfs://org/kbm/deliverables/research/**` | Competitive Scouting | Supervisor, Product Owner | Scouting write, Supervisor approve |
| `vfs://org/kbm/deliverables/architecture/**` | Chief Architect | Supervisor, Database, Security | Architect write, multi-review |
| `vfs://org/kbm/deliverables/database/**` | Database Architect | Supervisor, Chief Architect | Database write, Architect approve |
| `vfs://org/kbm/deliverables/security/**` | Security Architect | Supervisor, Chief Architect | Security write, Architect approve |
| `vfs://org/kbm/deliverables/ux/**` | UI/UX Designer | Supervisor, Product Owner | UX write, Product approve |
| `vfs://org/kbm/deliverables/product/**` | Product Owner | Supervisor, Scouting | Product write, Supervisor approve |
| **VFS Agent Reports** |
| `vfs://org/kbm/agents/<agent-name>/**` | Respective Agent | Supervisor (read-only) | Agent owns its folder |
| **Repository (FASE 1+)** |
| `/docs/**` | Documentation Agent | Supervisor | Docs write, Supervisor approve |
| `/src/KBM.Core/**` | **SHARED AREA** | Supervisor, Chief Architect | Shared Change Request required |
| `/src/KBM.Shared/**` | **SHARED AREA** | Supervisor, Chief Architect | Shared Change Request required |
| `/src/KBM.Security/**` | Security Architect | Supervisor, Chief Architect, Security | SCR required |
| `/src/KBM.Database/**` | Database Architect | Supervisor, Chief Architect, Database | SCR required |
| `/src/KBM.Api/**` | Backend Core Coder | Supervisor, Chief Architect | SCR for contracts |
| `/src/KBM.Client/**` | Frontend Coder | Supervisor, Chief Architect | SCR for shared components |
| `/src/KBM.Modules.<ModuleName>/**` | Module Coder Agent | Code Quality, Supervisor | Module coder owns its module |
| `/database/migrations/**` | Database Architect | Supervisor, Database | Only Database Architect creates migrations |
| `/tests/**` | QA Agent + respective coder | Code Quality | Coder writes, QA validates |
| `/.editorconfig`, `/Directory.Build.props` | Chief Architect | Supervisor | SCR required |
| `/.github/**` | DevOps Agent | Supervisor | DevOps write, Supervisor approve |

---

## Shared Change Request (SCR) Process

Se un agente deve modificare un'area **SHARED AREA** o file **critico**, deve:

1. **Creare SCR** in `vfs://org/kbm/governance/shared-change-requests/<scr-id>.md`
2. **Includere**:
   - Richiedente (agent)
   - Motivazione
   - File richiesti
   - Contratti/API impattati
   - Tabelle impattate
   - Alternative considerate
   - Rischio breaking change
   - Reviewer richiesti
3. **Attendere approvazione** da Supervisor + domain expert
4. **Procedere** solo dopo approvazione esplicita

**Nessuna modifica a shared area senza SCR approvata.**

---

## Conflict Resolution

Se **due agenti** tentano di modificare:
- Stesso file
- Stessa tabella database
- Stesso DTO/contratto
- Stesso componente UI

→ **Supervisor blocca entrambi i task**, crea conflict resolution plan, serializza le modifiche.

---

## Aggiornamenti

Ownership matrix viene aggiornata quando:
- Nuovo modulo viene aggiunto
- Nuovo agente viene creato
- Nuova area condivisa viene identificata
- Fase del progetto cambia (es. Fase 0 → Fase 1)

**Solo Supervisor può modificare ownership-matrix.md**

---

## Expert ID Reference (Fase 0)

| Expert Nome | Expert ID | Ownership Area |
|---|---|---|
| KBM Supervisor | `261c0a3e-6879-42b2-a060-aebbce2c9c01` | Governance, coordination, decision log |
| KBM Competitive Scouting | `ada7e554-ae5c-4838-89f3-d2833b125799` | Research deliverables |
| KBM Chief Architect | `13902456-9d01-4413-8429-8783d58b7a2d` | Architecture deliverables |
| KBM Database Architect | `c5a4b3c7-7a14-4426-9168-6643e0568e6e` | Database deliverables, migrations |
| KBM Security Architect | `63b6da33-93f5-4633-9601-7164cac8b420` | Security deliverables, auth/authz |
| KBM UI/UX Designer | `9d406c1e-79a7-4211-a074-b625ef260080` | UX deliverables, wireframes |
| KBM Product Owner | `3597c969-7ad8-4390-9593-de94896a71ef` | Product deliverables, backlog |

---

**Fine Ownership Matrix v1.0**
