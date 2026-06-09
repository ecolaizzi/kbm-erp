# KBM - Decision Log

**Progetto**: Kever Business Manager (KBM)  
**Versione**: 1.0  
**Data creazione**: 2026-06-09  
**Owner**: KBM Supervisor

---

## Scopo

Questo documento traccia tutte le decisioni architetturali, strategiche e operative del progetto KBM.

Ogni decisione importante deve essere documentata qui con:
- **ID univoco**
- **Data**
- **Decisore** (Supervisor o altro expert con approvazione)
- **Contesto** e rationale
- **Decisione presa**
- **Alternative considerate**
- **Conseguenze** e impatti
- **Status** (Proposta, Approvata, Implementata, Deprecata)

---

## Decisioni

### DEC-001: Creazione Expert Personalizzati per KBM

**Data**: 2026-06-09  
**Decisore**: Supervisor (Edwin Colaizzi approval)  
**Status**: ✅ Approvata e Implementata  

**Contesto**:
KBM è un progetto enterprise complesso multi-fase che richiede coordinamento tra ricerca, architettura, database, security, UX, product e coding. Valutata scelta tra:
- Sub-agent generici (explore, plan, code, validate)
- Expert personalizzati specializzati su dominio ERP

**Decisione**:
Creare **7 expert personalizzati** per Fase 0:
1. KBM Supervisor (orchestratore)
2. KBM Competitive Scouting (analisi ERP competitor)
3. KBM Chief Architect (architettura, ADR, stack)
4. KBM Database Architect (schema SQL Server, migration)
5. KBM Security Architect (auth, authz, RBAC, audit, threat model)
6. KBM UI/UX Designer (UX guidelines, wireframes, component library)
7. KBM Product Owner (roadmap, backlog, user stories)

**Rationale**:
- **Specializzazione dominio**: expert con system prompt ottimizzati per ERP gestionale italiano
- **Governance**: ownership matrix chiara, capability precise per expert
- **Parallelizzazione**: worker paralleli per task indipendenti (Batch 1, Batch 2)
- **Memory**: VFS breadcrumbs e knowledge accumulation cross-session
- **Automazioni**: possibilità di trigger (es. PR review automatico)

**Alternative considerate**:
- Sub-agent generici: meno specializzati, no memory cross-session, orchestrazione manuale
- Monolithic agent: context window overflow, no parallelizzazione

**Conseguenze**:
- ✅ Setup time: ~30 min creazione expert
- ✅ Workflow efficiency: task paralleli possibili
- ✅ Quality: expert specializzati producono output migliore
- ⚠️ Complessità: gestione 7+ expert richiede orchestrazione attenta

**Expert creati**:
- Supervisor: `261c0a3e-6879-42b2-a060-aebbce2c9c01`
- Scouting: `ada7e554-ae5c-4838-89f3-d2833b125799`
- Architect: `13902456-9d01-4413-8429-8783d58b7a2d`
- Database: `c5a4b3c7-7a14-4426-9168-6643e0568e6e`
- Security: `63b6da33-93f5-4633-9601-7164cac8b420`
- UX: `9d406c1e-79a7-4211-a074-b625ef260080`
- Product: `3597c969-7ad8-4390-9593-de94896a71ef`

---

### DEC-002: VFS per Governance e Deliverables

**Data**: 2026-06-09  
**Decisore**: Supervisor  
**Status**: ✅ Approvata e Implementata  

**Contesto**:
Deliverable di Fase 0 (research, architecture, database design, security model, UX guidelines, product backlog) devono essere persistiti e condivisi tra expert.

**Decisione**:
Usare **VFS organization** per stato condiviso:
- `vfs://org/kbm/governance/` - Decision log, ownership matrix, risk register, SCR
- `vfs://org/kbm/deliverables/phase-0/` - Output Fase 0
- `vfs://org/kbm/deliverables/{research,architecture,database,security,ux,product}/` - Deliverable per dominio
- `vfs://org/kbm/agents/<agent-name>/` - Report e stato di ogni expert

**Rationale**:
- VFS è persistente cross-session
- Synced tra tutti gli agent del tenant
- Version history (6-tier retention)
- Accessibile da tutti gli expert con ownership chiara

**Alternative considerate**:
- Repository files: non disponibile in Fase 0 (repo non creato ancora)
- Session-only state: perso al termine session, no cross-agent sharing

**Conseguenze**:
- ✅ State persistente
- ✅ Collaboration tra expert
- ✅ Audit trail (VFS version history)
- ⚠️ Dimensione: monitorare quota VFS

**VFS Structure creata**:
```
vfs://org/kbm/
├── governance/
│   ├── decision-log.md (questo file)
│   ├── ownership-matrix.md
│   ├── risk-register.md (TODO)
│   └── shared-change-requests/ (TODO)
├── deliverables/
│   ├── phase-0/
│   ├── research/
│   ├── architecture/
│   ├── database/
│   ├── security/
│   ├── ux/
│   └── product/
└── agents/
    ├── supervisor/
    ├── scouting/
    ├── architect/
    ├── database/
    ├── security/
    ├── ux/
    └── product/
```

---

### DEC-003: Model Selection per Expert

**Data**: 2026-06-09  
**Decisore**: Supervisor  
**Status**: ✅ Approvata e Implementata  

**Contesto**:
Expert diversi hanno complessità diverse. Valutare trade-off costo/performance.

**Decisione**:
- **Supervisor**: Sonnet 4.6 (coordination, task routing - no heavy reasoning)
- **Scouting**: Sonnet 4.6 (research, web analysis)
- **Architect**: **Opus 4.8** (complex architectural decisions, ADR)
- **Database**: **Opus 4.7** (complex schema design, performance)
- **Security**: **Opus 4.7** (threat modeling, STRIDE analysis)
- **UX**: Sonnet 4.6 (wireframes, guidelines - più creative che reasoning-heavy)
- **Product**: Sonnet 4.6 (backlog, user stories - strutturato ma no heavy reasoning)

**Rationale**:
- Opus per decisioni architetturali critiche e complesse
- Sonnet per task strutturati, coordination, research

**Alternative considerate**:
- Opus 4.8 per tutti: costo elevato, overhead per task semplici
- Sonnet 4.6 per tutti: rischio quality su decisioni architetturali critiche

**Conseguenze**:
- ✅ Bilanciamento costo/qualità
- ✅ Opus dove serve davvero (Architecture, Database, Security)
- ⚠️ Monitorare token usage

---

### DEC-004: Opportunità Differenziazione KBM da Competitive Scouting

**Data**: 2026-06-09
**Decisore**: Supervisor (basato su Competitive Scouting report)
**Status**: ✅ Approvata

**Contesto**:
Competitive Scouting ha completato analisi di 6 ERP competitor (NTS Business Cube, Zucchetti Ad Hoc, Zucchetti Mago, SAP Business One, Dynamics 365 BC, Odoo Enterprise) producendo feature parity matrix, UI/UX study, glossario, workflow analysis.

**Decisione**:
KBM si differenzia sui seguenti assi **prioritari**:

1. **UX Moderna** - Competitor italiani (NTS, Zucchetti Mago) usano ancora architettura MDI Windows legacy → KBM offre UI web-first moderna
2. **Localizzazione Completa** - SAP/Dynamics coprono parzialmente normativa italiana → KBM supporta nativamente: SDI, RI.BA., LIFO, spesometro, Intrastat, IVA in sospensione
3. **API-First** - NTS e Zucchetti Mago non hanno API REST native → KBM espone API completa per ecosistema partner
4. **Mobile** - Assente in NTS e Zucchetti Mago → KBM avrà app mobile responsive
5. **Workflow Approvativi** - Deboli in tutti i competitor italiani → KBM offre workflow visuali configurabili dall'utente

**Rationale**:
Questi gap rappresentano opportunità concrete di mercato dove KBM può vincere vs competitor legacy mantenendo TCO accessibile.

**Impatto su Roadmap**:
- **API-first** diventa priorità Fase 1 (non postponibile)
- **Localizzazione italiana completa** entra in MVP Fase 2 (RI.BA., SDI)
- **Workflow approvativi** posticipato a Fase 7 (complessità alta, MVP può partire senza)
- **Mobile** posticipato post-MVP (Fase 6-7)
- **UX moderna** è baseline architetturale (Chief Architect + UX Designer Batch 2)

**Conseguenze**:
- ✅ Posizionamento chiaro vs competitor
- ✅ Feature prioritization guidata da gap analysis
- ⚠️ API-first aumenta scope Fase 1 (richiede OpenAPI spec, documentation)

**Fonte**:
- `vfs://org/kbm/deliverables/research/feature-parity-matrix.md`
- `vfs://org/kbm/agents/scouting/completion-report.md`

---

### DEC-005: Product Vision e MVP Scope Approvato

**Data**: 2026-06-09
**Decisore**: Supervisor (basato su Product Owner report)
**Status**: ✅ Approvata

**Contesto**:
Product Owner ha completato product vision, roadmap Fase 1-7, MVP scope, backlog con 50+ user stories.

**Decisione**:
Approvata **Product Vision**:
- Target: PMI manifatturiere e commerciali italiane (10-200 dipendenti)
- UVP: "ERP Enterprise per PMI: potenza senza complessità"
- Differenziatori: Architettura moderna, RBAC granulare, TCO accessibile, localizzazione italiana

Approvato **MVP Scope** (Fase 1 + Fase 2):
- **Fase 1**: Core Platform (login, utenti, ruoli, permessi, aziende, audit, API base)
- **Fase 2**: Anagrafiche (clienti, fornitori, articoli, listini) + Ciclo base minimo

**Out of Scope MVP**:
- Contabilità completa (solo scadenzario base in MVP)
- CRM avanzato (solo note contatto in MVP)
- Produzione, MRP, BI (Fase 7)

**Rationale**:
MVP focalizzato su "foundation solida + anagrafiche utilizzabili" permette go-to-market rapido (6-9 mesi) per validare mercato prima di investire in moduli avanzati.

**Conseguenze**:
- ✅ Roadmap chiara per Batch 2 (Architect può progettare su scope definito)
- ✅ Backlog 50+ stories pronto per estimation e sprint planning (post-Fase 0)
- ⚠️ Decisione finale su "Fase 2 include ciclo attivo/passivo?" da prendere dopo Architect + Database design

**Fonte**:
- `vfs://org/kbm/deliverables/product/vision.md`
- `vfs://org/kbm/deliverables/product/mvp-scope.md`
- `vfs://org/kbm/deliverables/product/roadmap.md`

---

### DEC-006: Batch 1 Completato - Approvazione Deliverable

**Data**: 2026-06-09
**Decisore**: Supervisor
**Status**: ✅ Approvata

**Contesto**:
Batch 1 (Competitive Scouting + Product Owner) completato. Tutti deliverable prodotti e validati.

**Risultati Batch 1**:

**Competitive Scouting**:
- 9 deliverable prodotti (feature matrix, UI/UX study, glossario 110+ termini, workflows, module map, citations)
- 6 competitor analizzati in profondità
- 5 opportunità differenziazione identificate
- Compliance 100% verificata (solo fonti pubbliche)

**Product Owner**:
- 10 deliverable prodotti (vision, roadmap, MVP scope, 5 epic, AC template)
- **80 user stories** (target 50+ superato del 60%)
- 39 P0 stories con AC completi Given-When-Then
- Backlog pronto per Fase 1 development

**Decisione**:
✅ **Approvo tutti i deliverable Batch 1**

**Quality Check**:
- ✅ Feature matrix completa e accurata
- ✅ Glossario 110+ termini vs target 100+
- ✅ Workflow analysis 4 aree complete
- ✅ Citations report con compliance verificata
- ✅ Product vision chiara, UVP ben definito
- ✅ User stories con AC dettagliati, technical notes, DoD
- ✅ Nessun gap critico identificato

**Actions**:
- ✅ Batch 2 (Architecture & Design) già lanciato
- ⏳ Attendere completamento Batch 2 per cross-validation
- 📋 Refinement backlog post-Architecture (validare technical notes vs schema DB effettivo)

**Impatto**:
- Fase 0 procede on-track
- Foundation solida per Batch 2 (input di qualità disponibili)
- Nessun blocco o re-work richiesto

---

### DEC-007: CRITICAL - Timeline Constraint 2 Mesi

**Data**: 2026-06-09
**Decisore**: Edwin Colaizzi (Stakeholder)
**Status**: 🚨 CRITICA - Impatto Massimo su Progetto

**Contesto**:
Roadmap iniziale prevedeva 12-18 mesi sviluppo (Fase 1-7).
Stakeholder richiede **deadline 2 mesi** (8 settimane).

**Gap**: ~10-16 mesi di differenza tra scope originale e timeline richiesta.

**Decisione**:
✅ **Accetto constraint 2 mesi** e **re-scope completo progetto**

**Nuovo Scope 2 Mesi (Hybrid Approach)**:

**PRODUCTION-READY** (backend + frontend funzionanti):
- Week 1-2: **Core Platform** (Auth, Users, Roles, Multi-azienda, Audit)
- Week 3-4: **Anagrafiche** (Clienti, Fornitori, Articoli, Listini)
- Week 5-6: **Ciclo Attivo** (Ordini Cliente, DDT, Fatture + SDI)

**DEMO-READY** (UI completo, dati mock):
- Week 7-8: **Ciclo Passivo, Magazzino, Contabilità, CRM** - UI navigabile, dati fake

**FOUNDATION** (preparato per futuro):
- Database schema completo (13 moduli)
- Architecture scalabile
- API skeleton tutti moduli
- Permission catalog completo

**Deliverable 2 Mesi**:
- 3 moduli **vendibili** (Core, Anagrafiche, Ciclo Attivo)
- UI **completo** tutti moduli (demo impressionante)
- Foundation **solida** (development continua post-MVP)

**Impatto su Fase 0**:
- ✅ Batch 2 worker aggiornati con constraint (Chief Architect, Database, UX)
- ✅ Product Owner rilanciato per roadmap 2 mesi dettagliata
- ⚠️ Scope ridotto drasticamente (accettato)
- ⚠️ Technical debt accettato (rapid development priorità)

**Impatto su Batch 2 Deliverable**:
- **Chief Architect**: stack per rapid dev (code gen, scaffolding, minimal ceremony)
- **Database Architect**: schema completo MA implementazione progressiva (3 moduli priority)
- **Security Architect**: già completato, MFA downgrade da P0 a P1 (post-MVP)
- **UX Designer**: wireframe priorità su 3 moduli production + sketch altri per demo

**Risks Accettati**:
- ⚠️ Technical debt (refactor post-MVP)
- ⚠️ Feature gap vs competitor (contabilità, magazzino avanzato posticipati)
- ⚠️ Team undersized (1-2 developer + AI agent required minimum)
- ⚠️ Scope creep risk (MUST lock scope Week 1)

**Mitigations**:
- ✅ AI agent heavy (code generation, scaffolding)
- ✅ Template-based development (copy-paste modules)
- ✅ Use existing libraries (Fatturazione SDI, no build da zero)
- ✅ Mock data per demo (UI completo senza backend completo)
- ✅ Progressive enhancement (foundation solida, feature incrementali)

**Team Assumption Minimo**:
- 1 Full-Stack Developer (o 1 Backend + 1 Frontend)
- AI Agent support (code generation)
- Product Owner (coordination)

**Alternative Considerate**:
- ❌ Scope originale (impossibile 2 mesi)
- ❌ Solo demo/prototype (stakeholder vuole prodotto vendibile)
- ❌ Solo 1 modulo completo (troppo limitato, no ERP)
- ✅ **Hybrid 3 moduli production + demo completo** (SCELTA)

**Conseguenze**:
- ✅ Prodotto vendibile limitato (ma funzionante)
- ✅ Demo impressionante (UI completo)
- ✅ Foundation per crescita post-MVP
- ⚠️ Gap vs competitor enterprise (accettato per go-to-market rapido)
- ⚠️ Refactor necessario post-MVP (technical debt)

**Actions Immediate**:
- [x] Aggiornato Batch 2 worker (Chief Architect, Database, UX) con constraint
- [x] Rilanciato Product Owner per roadmap 2 mesi dettagliata
- [ ] Attendere deliverable Product Owner (roadmap week-by-week)
- [ ] Validare feasibility con Chief Architect + Database Architect
- [ ] Lock scope Week 1 (no feature creep)

**Priorità**: 🔴 **MASSIMA** - questa decisione guida TUTTO il progetto

---

### DEC-008: Technology Stack Definito

**Data**: 2026-06-09
**Decisore**: Chief Architect Agent
**Status**: ✅ Approvata (pending conferma WPF vs Avalonia)

**Contesto**:
Batch 2 Chief Architect ha definito stack tecnologico completo per KBM.

**Decisione Stack**:

**Backend**:
- ✅ **.NET 8 LTS** (supporto fino 2026-11-10)
- ✅ **ASP.NET Core Web API** (REST + OpenAPI/Swagger)
- ✅ **SQL Server 2019+** (preferito 2022)
- ✅ **EF Core 8** (ORM primary, Code First migrations)
- ✅ **Dapper** (ORM secondary per query performance-critical)
- ✅ **MediatR** (in-process messaging, CQRS lite)
- ✅ **FluentValidation** (validation pipeline)
- ✅ **Serilog** (structured logging)

**Client**:
- ✅ **WPF (.NET 8)** (desktop nativo Windows)
- ⚠️ **Limitazione**: Windows-only (no macOS/Linux native)
- ✅ **Rationale**: RDS/Citrix optimized, densità UI ERP, standard mercato italiano

**Architecture**:
- ✅ **Modular Monolith** (no microservices)
- ✅ **Clean Architecture** per modulo (Core, Application, Infrastructure, Presentation)
- ✅ **REST API-first** (client web futuro possibile)
- ✅ **Multi-tenancy**: `CompanyId` discriminator + EF Global Query Filter
- ✅ **Optimistic concurrency**: `RowVersion` (SQL Server) → HTTP 409 Conflict

**Development**:
- ✅ **xUnit** (testing framework)
- ✅ **Testcontainers** (integration testing con SQL Server container)
- ✅ **NetArchTest** (enforce dependency rules)
- ✅ **Central Package Management** (version pinning centralizzato)
- ✅ **Roslyn Analyzers** (code quality gates)

**Deployment**:
- ✅ **IIS on-premise** (primary target, PMI italiane)
- ✅ **Docker** (optional, dev environment)
- ✅ **Cloud-ready** (Azure/AWS possibile ma non target MVP)

**Rationale**:
- .NET 8: LTS, performance, cross-platform backend, ecosystem maturo
- SQL Server: standard ERP italiano, tooling enterprise, multi-tenancy robusto
- WPF: RDS/Citrix friendly, UI densa per data entry, standard gestionale italiano
- EF Core + Dapper: bilanciamento productivity (EF) vs performance (Dapper)
- Modular Monolith: semplicità deployment, team piccolo, refactoring futuro possibile
- API-first: client web futuro, integrations, mobile (roadmap post-MVP)

**Alternative Considerate**:
- ❌ **Microservices**: complessità eccessiva per team piccolo e timeline 2 mesi
- ⚠️ **Avalonia** (client): cross-platform (Win/Mac/Linux) ma meno maturo, +1 settimana timeline
- ⚠️ **Blazor/React** (client web): cross-platform ma +2-3 settimane, troppo per 2 mesi
- ❌ **PostgreSQL**: meno standard in ERP italiano, tooling inferiore
- ❌ **Entity Framework solo**: performance insufficiente per report complessi

**Impatto 2 Mesi**:
- ✅ Stack supporta **rapid development** (scaffolding, hot reload, templates)
- ✅ **Module template** ready (copy-paste friendly)
- ✅ **Code generation** possibile (.NET CLI templates)
- ⚠️ **WPF Windows-only**: se serve macOS, serve Avalonia (+1 settimana) o web client (+3 settimane impossibile)

**Risks**:
- ⚠️ WPF Windows-only limita mercato (mitigazione: client web roadmap post-MVP, API-first ready)
- ⚠️ Modular Monolith scalability limit (mitigazione: refactoring future a microservices possibile, API boundaries già definiti)
- ⚠️ SQL Server licensing cost (mitigazione: Express gratis fino 10GB, target PMI on-premise hanno già licenze)

**Actions**:
- [x] Stack definito da Chief Architect
- [ ] **DECISIONE RICHIESTA**: Conferma WPF (Windows-only) vs Avalonia (cross-platform)
- [ ] Cross-validation con Database Architect (ADR-004, ADR-005, ADR-008) → ✅ già allineato
- [ ] Cross-validation con Security Architect (ADR-007, ADR-009) → ✅ già allineato
- [ ] Cross-validation con UX Designer (feasibility WPF RDS) → ⏳ pending UX completion

**Deliverable**:
- ✅ `architecture/solution-architecture.md` (C4 L1/L2/L3, deployment)
- ✅ `architecture/technology-stack.md` (stack + rationale + version pinning)
- ✅ `architecture/adr/` (12 ADR)
- ✅ `architecture/module-template.md` (scaffolding)
- ✅ `architecture/coding-standards.md` + `.editorconfig`
- ✅ `architecture/diagrams/` (4 Mermaid C4)

**Priorità**: 🔴 **MASSIMA** - stack guida tutto lo sviluppo

**Pending**: Conferma WPF vs Avalonia (se serve macOS support)

---

## Template per Nuove Decisioni

```markdown
### DEC-XXX: [Titolo Decisione]

**Data**: YYYY-MM-DD  
**Decisore**: [Expert o Supervisor]  
**Status**: 🟡 Proposta / ✅ Approvata / ⚙️ Implementata / ❌ Deprecata  

**Contesto**:
[Perché questa decisione è necessaria? Qual è il problema/opportunità?]

**Decisione**:
[Cosa abbiamo deciso di fare?]

**Rationale**:
[Perché questa è la scelta migliore?]

**Alternative considerate**:
- Alternativa A: [pros/cons]
- Alternativa B: [pros/cons]

**Conseguenze**:
- ✅ Pro 1
- ✅ Pro 2
- ⚠️ Trade-off 1
- ❌ Con 1

**Actions**:
- [ ] Action item 1
- [ ] Action item 2
```

---

**Fine Decision Log - Continuerà ad essere aggiornato durante tutto il progetto KBM**
