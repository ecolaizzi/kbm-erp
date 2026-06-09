# KBM — Risk Register 2 Mesi

**Versione**: 1.0  
**Data**: 2026-06-09  
**Owner**: Product Owner Agent  
**Review**: Ogni 2 settimane

---

## Legenda

| Probabilità | Impatto | Rating |
|-------------|---------|--------|
| Alta (>60%) | Critico (blocca delivery) | 🔴 Rosso |
| Media (30-60%) | Alto (ritarda delivery) | 🟡 Giallo |
| Bassa (<30%) | Medio (riduce scope) | 🟢 Verde |

---

## Top Risk Register

| # | Rischio | Prob. | Impatto | Rating | Mitigation |
|---|---------|-------|---------|--------|------------|
| R01 | Timeline troppo aggressiva per team size | Alta | Critico | 🔴 | Cut scope Week 6: SDI → XML manual, demo-only; Focus P0 only |
| R02 | Team undersized (1 sola persona) | Media | Critico | 🔴 | AI agent code generation pesante; cut feature Week 7-8 |
| R03 | SDI integration complessità maggiore del previsto | Media | Alto | 🟡 | Usa libreria esistente (FatturaElettronica.Net); no build da zero; ambiente test AE disponibile |
| R04 | Scope creep (nuove richieste in corso d'opera) | Alta | Alto | 🔴 | Freeze scope document firmato Week 1; change request process formale; no nuove feature fino a Week 7 |
| R05 | Technical debt da sviluppo rapido | Alta | Medio | 🟡 | Accettato consapevolmente; refactor schedulato post-MVP; documentare debt esplicito |
| R06 | Bug critici scoperti tardi (Week 7-8) | Media | Critico | 🔴 | Test automation sin da Week 1; integration test per P0 flow; UAT interno Week 7 |
| R07 | Performance su dataset reale | Bassa | Alto | 🟡 | Indici DB definiti in Week 1; query profiling Week 8; target < 200ms API response |
| R08 | Setup infrastruttura più lento (CI/CD, ambienti) | Media | Medio | 🟡 | Usa template .NET esistente; GitHub Actions template standard; Day 1 priority |
| R09 | Dipendenza esterna: Hub SDI/fornitore non disponibile | Bassa | Alto | 🟡 | Ambiente test AE; fallback: genera XML per invio manuale |
| R10 | Cambio stack tecnico (Architect richiede modifica) | Bassa | Critico | 🟡 | Architettura già approvata ADR; no cambi in-flight; eventuali cambi solo in Week 1 |

---

## Dettaglio Rischi Critici

### R01 — Timeline Troppo Aggressiva 🔴

**Descrizione**: 8 settimane per Core Platform + Anagrafiche + Ciclo Attivo + Demo UI è una timeline molto compressa. Qualsiasi imprevisto può causare slittamento.

**Segnali di allarme**:
- Fine Week 2: RBAC o Multi-Azienda non completati → ALERT
- Fine Week 4: Clienti o Articoli non production-ready → ALERT CRITICO
- Fine Week 6: DDT o Fattura non funzionante → ESCALATION

**Piano di contingenza**:
1. **Cut L1** (ritardo < 1 settimana): Posticipa Import Excel, Dashboard KPI avanzata
2. **Cut L2** (ritardo 1-2 settimane): Posticipa Note di Credito, SDI invio reale (genera XML)
3. **Cut L3** (ritardo > 2 settimane): Ridefinisci deliverable: solo Core + Anagrafiche production; Ciclo Attivo demo-ready

**Owner**: Product Owner  
**Checkpoint**: Fine di ogni week

---

### R02 — Team Undersized 🔴

**Descrizione**: Con 1 solo developer, la velocity richiesta (35 SP/settimana) è al limite del fattibile anche con AI agent support.

**Segnali di allarme**:
- Week 1: velocity reale < 15 SP → timeline a rischio
- Week 3: nessun modulo Anagrafiche completato → impossibile rispettare Week 6

**Piano di contingenza**:
1. **Potenzia AI Agent**: scaffolding automatico di tutti i CRUD, non scrivere boilerplate a mano
2. **Recruitment rapido**: cerca secondo developer immediatamente se Week 1 < 15 SP
3. **Scope cut**: ridurre Ciclo Attivo a solo Ordini + Fattura (no Offerte, no SDI in 2 mesi)
4. **Extend deadline**: proporre +2 settimane se team non raggiunge velocity target

**Owner**: Product Owner / Management  
**Checkpoint**: Fine Week 1, Fine Week 3

---

### R04 — Scope Creep 🔴

**Descrizione**: Stakeholder richiedono nuove funzionalità in corso d'opera ("ma non possiamo aggiungere anche X?"). Nella timeline 2 mesi, ogni aggiunta non pianificata ritarda tutto.

**Regola ferrea**:
- **ZERO nuove feature in scope tra Week 1 e Week 6**
- Tutto ciò che non è nel backlog P0 approvato va in lista "Post-MVP"
- Change request formale richiede approvazione Product Owner + Architect + stima impatto

**Piano di contingenza**:
- Scope document firmato da tutti gli stakeholder in Week 1
- Weekly review: scope è cambiato? → escalation immediata
- Backlog P2 pubblica e accessibile ("potete vedere che è pianificato, ma non nelle 8 settimane")

**Owner**: Product Owner (gate keeper)  
**Checkpoint**: Ogni week

---

### R06 — Bug Critici Tardi 🔴

**Descrizione**: Bug scoperti in Week 7-8 su funzionalità core (es. SDI non funziona, tenant isolation con bug, RBAC con gap) possono invalidare il deliverable.

**Mitigazioni preventive**:
- Test automation sin da Week 1 per ogni modulo P0
- Integration test per flusso critico: Login → Seleziona Azienda → Crea Fattura → SDI
- Security test: tenant isolation testato esplicitamente (utente A non vede dati azienda B)
- Performance test: 10K clienti, 50K articoli, 1K fatture in ambiente staging Week 6

**Piano di contingenza**:
- Week 8 = 100% stabilizzazione (no nuove feature)
- Bug P0 (blocca uso): fix immediato, tutto il resto aspetta
- Bug P1 (degrada UX): fix in Week 8 se tempo, altrimenti post-MVP
- Bug P2 (cosmetic): post-MVP

**Owner**: Development Team  
**Checkpoint**: Fine Week 5, Fine Week 7

---

## Rischi di Contesto

### R11 — SDI: cambio normativa / formato FatturaPA
**Prob**: Bassa | **Impatto**: Alto  
**Mitigation**: Libreria FatturaElettronica.Net è aggiornata regolarmente; monitora comunicazioni AE

### R12 — Indisponibilità ambiente sviluppo (cloud, DB server)
**Prob**: Bassa | **Impatto**: Medio  
**Mitigation**: Sviluppo locale possibile su tutti i dev machine; SQL Server Express locale

### R13 — Cambio requirements dati fiscali italiani
**Prob**: Bassa | **Impatto**: Medio  
**Mitigation**: Parametrizzare aliquote IVA, codici SDI, formati CF/PIVA (non hardcode)

### R14 — Vendor lock-in su componenti critici (PDF, SDI)
**Prob**: Media | **Impatto**: Basso  
**Mitigation**: Interfacce astratte (IDocumentGenerator, ISdiProvider) — sostituibili senza impatto

---

## Risk Tracker

| # | Rischio | Stato | Note ultima review |
|---|---------|-------|-------------------|
| R01 | Timeline aggressiva | 🔴 Monitorato | — |
| R02 | Team undersized | 🔴 Monitorato | Dipende da team size effettivo |
| R03 | SDI complessità | 🟡 Mitigato | Libreria scelta |
| R04 | Scope creep | 🔴 Monitorato | Scope lock Week 1 |
| R05 | Technical debt | 🟡 Accettato | Documentare debt |
| R06 | Bug critici tardi | 🔴 Monitorato | Test automation richiesta |
| R07 | Performance | 🟡 Monitorato | Indici DB Week 1 |
| R08 | Setup infrastruttura | 🟡 Mitigato | Template CI/CD standard |
| R09 | Hub SDI down | 🟢 Mitigato | Ambiente test AE |
| R10 | Cambio stack | 🟢 Mitigato | ADR approvato |

---

## Decisioni di Go/No-Go per Settimana

| Checkpoint | Criterio Go | Criterio No-Go (Escalation) |
|------------|-------------|---------------------------|
| Fine Week 2 | Login + RBAC + Multi-Azienda funzionanti | Uno dei tre non completato |
| Fine Week 4 | Clienti + Fornitori + Articoli production-ready | Mancano > 30% delle story P0 |
| Fine Week 6 | DDT + Fattura + SDI funzionanti | SDI non genera XML valido |
| Fine Week 7 | Demo UI navigabile per 3+ moduli | Demo crasha / non navigabile |
| Fine Week 8 | 0 bug P0, UAT passato | Bug critici aperti su Core o SDI |

---

*Risk Register 2 Mesi v1.0 — 2026-06-09 — Review ogni 2 settimane*
