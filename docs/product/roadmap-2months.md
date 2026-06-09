# KBM — Roadmap 2 Mesi (8 Settimane)

**Versione**: 2.0 — Re-Scope Critico  
**Data**: 2026-06-09  
**Owner**: Product Owner Agent  
**Status**: 🚨 PRIORITÀ ASSOLUTA — Timeline 2 Mesi  
**Baseline**: Roadmap originale 12-18 mesi → compresso a 8 settimane

---

## Strategia Generale: Vertical Slice + Horizontal Mockup

**Production-Ready** (Week 1-6): Core Platform + Anagrafiche + Ciclo Attivo  
**Demo-Ready** (Week 7-8): UI completo tutti i moduli restanti + dati mock  
**Foundation** (parallelo): DB schema completo, API skeleton, Permission catalog

---

## Week 1: Foundation & Core Platform — Auth + Infra
**Date**: 09/06 – 13/06/2026  
**Focus**: Repository, CI/CD, Database schema completo, Login, JWT, Setup Wizard

### Deliverable
- [ ] Solution structure (.NET 8 Modular Monolith, cartelle per modulo)
- [ ] Database schema completo creato via EF Core migrations (tutte le tabelle Fase 1-7)
- [ ] CI/CD pipeline base (build + test automatici)
- [ ] Login funzionante (POST /api/auth/login → JWT)
- [ ] Password hashing (bcrypt) + lockout dopo 5 tentativi
- [ ] Setup Wizard first-run (Azienda + Admin user)
- [ ] Client shell: navigation, login screen, routing base

### User Stories Completate
- US-001 Login con username/password (M)
- US-003 Gestione Sessioni e Logout (S)
- US-018 Setup Wizard (L)
- US-INFRA-01 Repository + CI/CD setup (S)
- US-INFRA-02 DB schema completo + migrations (M)

**Story Points**: ~20 SP

### Risks
- Setup environment/tooling > 2 giorni → *mitigation*: usare template .NET esistente
- EF migrations schema completo (13+ moduli) → *mitigation*: definire tutte le entità base in bulk

---

## Week 2: RBAC + Multi-Azienda + Utenti
**Date**: 16/06 – 20/06/2026  
**Focus**: Gestione utenti, Ruoli, Permessi granulari, Multi-tenant isolation, Audit log

### Deliverable
- [ ] CRUD Utenti completo (create, edit, disable, enable, list + filtri)
- [ ] Ruoli predefiniti (Admin, Manager, Operatore, ReadOnly) + CRUD ruoli
- [ ] Permessi granulari: struttura `{modulo}.{risorsa}.{azione}`, permission seeding
- [ ] Multi-azienda: CRUD aziende, selezione contesto al login, tenant isolation su tutte le query
- [ ] Audit log middleware: intercetta tutte le operazioni CUD con before/after
- [ ] MFA (TOTP opzionale, obbligatorio per Admin)
- [ ] Password policy + reset password via email

### User Stories Completate
- US-004 Password Policy e Reset (M)
- US-005 CRUD Utenti (M)
- US-006 Ruoli e Permessi RBAC (L)
- US-007 Multi-Azienda creazione/gestione (L)
- US-008 Selezione contesto azienda (S)
- US-009 Audit Log (L)
- US-013 Assegnazione utenti ad aziende (M)
- US-002 MFA (L)

**Story Points**: ~30 SP

### Risks
- Tenant isolation richiede revisione su tutte le query → *mitigation*: EF Core global query filter
- RBAC caching invalidation → *mitigation*: refresh token claim su cambio ruolo

---

## Week 3: Anagrafiche Base + Clienti
**Date**: 23/06 – 27/06/2026  
**Focus**: Tabelle di sistema, CRUD Clienti completo con dati fiscali IT

### Deliverable
- [ ] Seed data: Nazioni (ISO 3166), Province/Comuni ISTAT, Valute (ISO 4217), UM base
- [ ] CRUD Clienti: dati anagrafici, dati fiscali (P.IVA validata, CF, SDI code, PEC)
- [ ] Clienti: contatti multipli (email, tel, referente)
- [ ] Clienti: indirizzi multipli (sede legale, spedizione, fatturazione)
- [ ] Clienti: condizioni commerciali (pagamento, agente, listino associato)
- [ ] Ricerca/filtri clienti: ragione sociale, codice, P.IVA, stato
- [ ] Codici bancari IBAN per clienti
- [ ] Dashboard home con widget base (accessi recenti, notifiche)

### User Stories Completate
- US-101 Gestione Nazioni (S)
- US-102 Province e Comuni ISTAT (S)
- US-103 Valute (M)
- US-104 Unità di Misura (S)
- US-121 CRUD Cliente dati base (M)
- US-122 Cliente dati fiscali italiani (M)
- US-123 Cliente contatti multipli (S)
- US-124 Cliente indirizzi multipli (S)
- US-125 Cliente condizioni commerciali (M)
- US-011 Dashboard Home base (M)
- US-012 Navigazione shell (M)

**Story Points**: ~28 SP

### Risks
- Validazione P.IVA/CF → *mitigation*: usare libreria NuGet esistente (CodiceFiscale.Net)

---

## Week 4: Fornitori + Articoli + Listini
**Date**: 30/06 – 04/07/2026  
**Focus**: CRUD Fornitori, Articoli con categorie/UM, Listini prezzi

### Deliverable
- [ ] CRUD Fornitori: dati anagrafici, fiscali (P.IVA, SDI), bancari (IBAN)
- [ ] Fornitori: contatti e indirizzi multipli
- [ ] Fornitori: condizioni acquisto (pagamento, listino acquisto)
- [ ] CRUD Articoli: codice, descrizione, categoria merceologica, UM principale
- [ ] Articoli: codici alternativi (barcode EAN, codice fornitore)
- [ ] Articoli: dati commerciali (prezzo base, IVA, conto ricavi/costi)
- [ ] Categorie merceologiche CRUD
- [ ] Listini prezzi vendita (per cliente, per categoria, con date validità)
- [ ] Import Excel: Clienti, Fornitori, Articoli (template fornito)
- [ ] Export Excel su tutte le anagrafiche

### User Stories Completate
- US-131 CRUD Fornitore base (M)
- US-132 Fornitore dati fiscali/bancari (M)
- US-141 CRUD Articolo (M)
- US-142 Articolo categorie/UM (S)
- US-143 Articolo codici alternativi (S)
- US-151 Listini vendita (L)
- US-152 Import Excel anagrafiche (L)
- US-153 Export Excel (S)

**Story Points**: ~28 SP

### Risks
- Import Excel con dati malformati → *mitigation*: validazione riga per riga con report errori

---

## Week 5: Ciclo Attivo — Offerte + Ordini Cliente
**Date**: 07/07 – 11/07/2026  
**Focus**: Preventivi/Offerte, Ordini di Vendita, conferma, stato avanzamento

### Deliverable
- [ ] Numeratori documenti configurabili (per tipo, per anno, per azienda)
- [ ] CRUD Offerta commerciale: testata cliente, righe articoli, prezzi da listino, sconti
- [ ] Offerta: conversione in Ordine Cliente (1-click)
- [ ] Offerta: stati (bozza, inviata, accettata, rifiutata, scaduta)
- [ ] CRUD Ordine Cliente: testata, righe, quantità, prezzi, date consegna
- [ ] Ordine: stato avanzamento (confermato, parzialmente evaso, evaso, annullato)
- [ ] Ordine: stampa PDF conferma ordine
- [ ] Agente vendita: assegnazione a cliente/ordine

### User Stories Completate
- US-NUMER-01 Numeratori documenti (M)
- US-201 CRUD Offerta (L)
- US-202 Offerta → Ordine conversione (M)
- US-203 CRUD Ordine Cliente (L)
- US-204 Stato avanzamento ordine (M)
- US-205 Stampa PDF documenti (M)

**Story Points**: ~28 SP

### Risks
- Gestione numeratori multi-azienda/multi-anno → *mitigation*: sequenze DB con lock ottimistico
- Calcolo sconti cumulativi → *mitigation*: motore sconti centralizzato riutilizzabile

---

## Week 6: Ciclo Attivo — DDT + Fattura Attiva + SDI
**Date**: 14/07 – 18/07/2026  
**Focus**: DDT, Fattura Attiva, Fattura Elettronica SDI, Note credito, Scadenzario base

### Deliverable
- [ ] CRUD DDT (da Ordine o manuale): testata, righe, causale trasporto, vettore
- [ ] DDT: stampa PDF (layout standard italiano)
- [ ] Fattura Attiva: emissione da DDT o da ordine, righe IVA, totali
- [ ] Fattura: stampa PDF (layout A4 standard)
- [ ] **Fattura Elettronica SDI**: generazione XML FatturaPA (FPR12), invio Hub
- [ ] SDI: gestione stati (inviata, consegnata, scartata, notifica mancata consegna)
- [ ] Nota di Credito cliente (storno fattura, parziale/totale)
- [ ] Scadenzario clienti base (partite aperte da fatture emesse)
- [ ] Export Excel scadenzario

### User Stories Completate
- US-211 CRUD DDT (L)
- US-212 Fattura Attiva (L)
- US-213 Fattura Elettronica SDI XML+invio (L)
- US-214 Gestione stati SDI (M)
- US-215 Nota di Credito cliente (M)
- US-216 Scadenzario clienti base (M)

**Story Points**: ~30 SP

### Risks
- SDI integration complessità → *mitigation*: usare libreria esistente (FatturaElettronica.Net / Spesometro.it SDK)
- SDI sandbox testing → *mitigation*: ambiente test Agenzia Entrate disponibile

---

## Week 7: Demo UI — Ciclo Passivo + Magazzino + Contabilità (Mock)
**Date**: 21/07 – 25/07/2026  
**Focus**: UI completo moduli non production con dati mock / seed per demo

### Deliverable
**Ciclo Passivo (UI Demo)**:
- [ ] UI Ordini Fornitore: lista, form testata+righe, stati (navigabile, dati fake)
- [ ] UI Carico Merce DDT Fornitore (navigabile)
- [ ] UI Fatture Passive (navigabile, workflow visibile)
- [ ] API skeleton Ciclo Passivo (endpoint stub con 200 OK + dati fake)

**Magazzino (UI Demo)**:
- [ ] UI Giacenze articoli per deposito (tabella con dati seeded)
- [ ] UI Movimenti magazzino (lista filtri + dati fake)
- [ ] UI Inventario (form inventario fisico, navigabile)
- [ ] API skeleton Magazzino

**Contabilità (UI Demo)**:
- [ ] UI Piano dei conti (albero conti precaricato)
- [ ] UI Prima Nota (form registrazione manuale, navigabile)
- [ ] UI Registri IVA (tabelle con dati fake)
- [ ] API skeleton Contabilità

**Demo Data Seeder**:
- [ ] Script di seed completo (100 clienti, 50 fornitori, 200 articoli, 20 fatture)

### User Stories Completate
- US-DEMO-01 Demo UI Ciclo Passivo (L)
- US-DEMO-02 Demo UI Magazzino (L)
- US-DEMO-03 Demo UI Contabilità (M)
- US-SEED-01 Demo Data Seeder completo (M)

**Story Points**: ~25 SP

### Risks
- Tempo UI mock sottostimato → *mitigation*: usare componenti riutilizzabili dalla Week 1-6

---

## Week 8: Demo UI — CRM + Dashboard KPI + Polish + Bug Fix
**Date**: 28/07 – 01/08/2026  
**Focus**: CRM UI mock, Dashboard KPI completa, Bug fix, Performance, Go-live prep

### Deliverable
**CRM (UI Demo)**:
- [ ] UI Contatti/Leads (lista + form, dati fake)
- [ ] UI Pipeline opportunità (kanban view, dati fake)
- [ ] API skeleton CRM

**Dashboard & Reporting**:
- [ ] Dashboard KPI: fatturato mese, ordini aperti, scadenzario (dati reali Ciclo Attivo)
- [ ] Dashboard KPI: widget mock per moduli non-production (giacenze, DA pagare)
- [ ] Export Excel base: fatture, ordini, scadenzario
- [ ] Report preview PDF (template fattura/DDT per branded output)

**Quality & Polish**:
- [ ] Bug fix sprint finale (priorità P0/P1 issues aperti)
- [ ] Performance testing (1K utenti simulati, 100K record)
- [ ] Security review checklist (OWASP top 10 basic)
- [ ] Documentazione API Swagger completa
- [ ] User acceptance testing (UAT) interno
- [ ] Preparazione ambiente demo/staging

### User Stories Completate
- US-DEMO-04 Demo UI CRM (M)
- US-KPI-01 Dashboard KPI Ciclo Attivo (M)
- US-KPI-02 Dashboard KPI Widget Mock (S)
- US-EXPORT-01 Export Excel documenti (M)
- US-UAT-01 UAT + Bug Fix sprint (L)

**Story Points**: ~22 SP

### Risks
- Bug fix backlog > capacità Week 8 → *mitigation*: freeze feature a fine Week 7, tutta Week 8 = stabilizzazione
- Performance su dataset large → *mitigation*: indici DB definiti in Week 1, EXPLAIN ANALYZE queries

---

## Riepilogo Timeline 8 Settimane

| Week | Focus | SP | Output Tipo |
|------|-------|-----|-------------|
| Week 1 | Foundation + Auth + CI/CD | 20 | Production |
| Week 2 | RBAC + Multi-Azienda + Audit | 30 | Production |
| Week 3 | Clienti completi + Dashboard | 28 | Production |
| Week 4 | Fornitori + Articoli + Listini + Import | 28 | Production |
| Week 5 | Offerte + Ordini Cliente | 28 | Production |
| Week 6 | DDT + Fattura + SDI + Scadenzario | 30 | Production |
| Week 7 | Demo UI: Passivo + Magazzino + Contabilità | 25 | Demo + API Skeleton |
| Week 8 | Demo UI: CRM + KPI + Polish + UAT | 22 | Demo + Stabilizzazione |
| **Totale** | | **211 SP** | |

**Nota**: Timeline richiede almeno **1 Full-Stack Developer Senior + AI Agent support** (code generation, scaffolding). Con 1 developer solo senza AI: non fattibile.

---

*Roadmap 2 Mesi v1.0 — 2026-06-09 — Baseline aggressiva, richiede esecuzione disciplinata*
