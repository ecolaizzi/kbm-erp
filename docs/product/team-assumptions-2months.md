# KBM — Team & Velocity Assumptions 2 Mesi

**Versione**: 1.0  
**Data**: 2026-06-09  
**Owner**: Product Owner Agent

---

## 1. Velocity Target

| Metrica | Valore |
|---------|--------|
| Durata sprint | 1 settimana |
| Story Points totali richiesti (P0) | ~285 SP |
| Settimane disponibili | 8 settimane |
| Velocity necessaria | **~35 SP/settimana** |
| SP per developer/settimana (senior) | ~15-20 SP |
| SP con AI Agent support (code gen) | ~25-35 SP/developer |

---

## 2. Scenari Team

### Scenario A — 1 Developer Senior + AI Agent (MINIMO FATTIBILE)
**Composizione**:
- 1 Full-Stack Developer Senior (.NET + WPF/Avalonia o Web)
- AI Agent (code generation, scaffolding, boilerplate)
- Product Owner (part-time, review, priority)

**Velocity stimata**: ~30-35 SP/settimana con AI support

**Fattibilità**: 
- ✅ Possibile MA richiede esecuzione disciplinatissima
- ✅ Zero scope creep
- ✅ AI agent deve generare almeno 40% del codice (CRUD, DTO, tests boilerplate)
- ⚠️ Zero buffer per malattia/imprevisti
- ⚠️ Timeline si slitta di 1-2 settimane se qualcosa va storto

**Raccomandazione**: Accettabile solo con AI agent molto attivo.

---

### Scenario B — 2 Developer (1 Backend + 1 Frontend) + AI Agent (OTTIMALE)
**Composizione**:
- 1 Backend Developer Senior (.NET 8, EF Core, SQL Server)
- 1 Frontend Developer (WPF/Avalonia o Blazor/React)
- AI Agent (code generation su entrambi i fronti)
- Product Owner (part-time)

**Velocity stimata**: ~50-60 SP/settimana

**Parallelizzazione possibile**:
- Backend developer: API, business logic, EF Core, migrations
- Frontend developer: UI components, screens, navigation
- Settimane 1-2: Setup in parallelo (Backend = API auth + DB; Frontend = shell + login UI)
- Settimane 3-6: Sviluppo parallelo per modulo (Backend = endpoint; Frontend = screens)

**Fattibilità**:
- ✅ Molto più confortevole
- ✅ Buffer ~20% su velocity per imprevedisti
- ✅ Demo UI Week 7-8 separabile completamente al frontend developer
- ✅ Code review possibile (back ↔ front)

**Raccomandazione**: **Scenario ottimale per la timeline 2 mesi.**

---

### Scenario C — 3 Developer + AI Agent (FAST TRACK)
**Composizione**:
- 1 Backend Developer Senior
- 1 Frontend Developer
- 1 Full-Stack Developer (supporto + moduli extra)
- AI Agent
- Product Owner (part-time)

**Velocity stimata**: ~70-80 SP/settimana

**Vantaggio**: Possibile includere più funzionalità production (Ciclo Passivo backend in Week 7 anziché demo-only)

**Fattibilità**: ✅ Timeline 2 mesi diventa confortevole con margine per extra feature

---

## 3. Ruoli e Responsabilità

| Ruolo | Responsabilità | % Tempo |
|-------|---------------|---------|
| Product Owner | Priority call, acceptance criteria, demo review | 30% |
| Backend Dev | API .NET 8, EF Core, business logic, DB migrations, SDI integration | 100% |
| Frontend Dev | UI screens, navigation, PDF generation, UX | 100% |
| AI Agent | Code gen CRUD, DTO boilerplate, test skeleton, migration scaffolding | on-demand |
| QA (opzionale) | Test UAT, bug reporting | 20% (Week 7-8) |

---

## 4. Assunzioni Critiche per Rispettare la Timeline

### Tecnologia
- ✅ Stack già scelto (.NET 8 + SQL Server + EF Core) — no deliberation
- ✅ UI framework già scelto (Avalonia o WPF) — no deliberation
- ✅ SDI integration via libreria esistente (es. `FatturaElettronica.Net`) — no build da zero
- ✅ PDF generation via libreria (es. `QuestPDF` o `FastReport`) — no build da zero
- ✅ Email via libreria (es. `MailKit`) — no build da zero
- ✅ Validazione P.IVA/CF via libreria (es. `CodiceFiscale.Net`) — no build da zero

### Processo
- ✅ No daily meeting lunghi — daily standup max 15 minuti
- ✅ No sprint retrospective in Week 1-6 (solo Week 7-8)
- ✅ Feature freeze a fine Week 6 per produzione — Week 7-8 = stabilizzazione e demo
- ✅ Code review asincrona (non blocca sviluppo)
- ✅ Testing: unit test su logica business critica, no 100% coverage
- ✅ Decisioni architetturali prese in Week 1, no revisioni in mid-sprint

### AI Agent Usage
- Scaffolding CRUD (Controller + Service + Repository + DTO + Tests) generato in < 30 min per modulo
- Migration EF Core generate da entità, no manuali
- Test boilerplate generato, solo logica specifica scritta a mano
- Swagger annotations generate automaticamente

---

## 5. Stima Effort per Modulo (Scenario A — 1 Dev + AI)

| Modulo | Effort Reale (Days) | AI Contribution | Net Dev Days |
|--------|--------------------|--------------------|--------------|
| Infra + Foundation | 3 | 20% | 2.5 |
| Core Auth + Sessions | 4 | 30% | 2.8 |
| RBAC + Multi-Azienda | 5 | 25% | 3.75 |
| Audit Log | 2 | 40% | 1.2 |
| Dashboard + Navigation | 3 | 20% | 2.4 |
| Anagrafiche base (seed) | 2 | 60% | 0.8 |
| Clienti completi | 4 | 35% | 2.6 |
| Fornitori | 3 | 40% | 1.8 |
| Articoli + Listini | 4 | 35% | 2.6 |
| Import/Export Excel | 3 | 30% | 2.1 |
| Numeratori + Offerte | 3 | 30% | 2.1 |
| Ordini Cliente | 3 | 35% | 1.95 |
| DDT | 3 | 30% | 2.1 |
| Fattura Attiva | 3 | 30% | 2.1 |
| SDI Integration | 4 | 20% | 3.2 |
| Scadenzario | 2 | 30% | 1.4 |
| Demo UI (4 moduli) | 5 | 25% | 3.75 |
| Bug Fix + UAT | 5 | 15% | 4.25 |
| **TOTALE** | **62 giorni** | | **~42 giorni net** |

**Giorni lavorativi in 8 settimane**: 40 giorni  
**Gap**: 2 giorni → gestibile con ore extra o AI generando più boilerplate

---

## 6. Piano di Contingenza

### Se in ritardo a fine Week 4 (> 1 settimana):
- **Cut**: Posticipa Import Excel a Post-MVP (W4 diventa W4+)
- **Cut**: Dashboard KPI avanzata → solo widget base
- **Mantieni**: Tutto il Core + Clienti + Fornitori + Articoli (non negoziabile)

### Se in ritardo a fine Week 6 (> 1 settimana):
- **Cut**: Demo UI CRM (Week 8 solo bug fix + UAT)
- **Cut**: SDI invio reale → genera XML, invio manuale
- **Mantieni**: Offerta + Ordine + DDT + Fattura (non negoziabile)

### Se team ridotto a 1 persona a settimana (malattia):
- **Week 1-2**: no alternative, critico
- **Week 3-6**: sviluppa solo backend, frontend posticipa
- **Week 7-8**: demo UI scalabile (solo 2-3 moduli invece di 4)

---

*Team Assumptions 2 Mesi v1.0 — 2026-06-09*
