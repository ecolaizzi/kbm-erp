# KBM — Backlog Re-Prioritizzato 2 Mesi

**Versione**: 2.0  
**Data**: 2026-06-09  
**Owner**: Product Owner Agent  
**Status**: 🚨 Re-prioritizzato per timeline 2 mesi

---

## Legenda Priorità 2 Mesi

| Priorità | Significato | Target |
|----------|-------------|--------|
| **P0** | Critical — deve essere production-ready in 2 mesi | Week 1-6 |
| **P1** | High — UI demo o funzionalità base | Week 7-8 |
| **P2** | Medium — posticipato dopo 2 mesi | Post-MVP |

---

## EPIC CORE PLATFORM (US-001 → US-020)

| Story | Titolo | Priorità 2M | Estimation | Week |
|-------|--------|-------------|------------|------|
| US-001 | Login con username/password | **P0** | M | W1 |
| US-003 | Gestione Sessioni e Logout | **P0** | S | W1 |
| US-018 | Setup Wizard first-run | **P0** | L | W1 |
| US-004 | Password Policy e Reset Password | **P0** | M | W2 |
| US-005 | CRUD Utenti (admin) | **P0** | M | W2 |
| US-006 | Definizione Ruoli e Permessi RBAC | **P0** | L | W2 |
| US-007 | Multi-Azienda — Creazione e Gestione | **P0** | L | W2 |
| US-008 | Selezione Contesto Azienda al Login | **P0** | S | W2 |
| US-009 | Audit Log — Tracciamento Operazioni | **P0** | L | W2 |
| US-013 | Assegnazione Utenti ad Aziende | **P0** | M | W2 |
| US-002 | Multi-Factor Authentication (MFA) | **P2** | L | Fine progetto |
| US-011 | Dashboard Home base | **P0** | M | W3 |
| US-012 | Navigazione e Shell Applicativa | **P0** | M | W3 |
| US-010 | Configurazione Sistema (Parametri) | **P1** | M | W7-8 |
| US-014 | Profilo Utente e Preferenze | **P1** | S | W8 |
| US-015 | Notifiche In-App | **P1** | M | W8 |
| US-016 | Log di Sistema e Monitoraggio | **P1** | M | W8 |
| US-017 | Password Scadenza Policy avanzata | **P1** | S | W2 |
| US-019 | Backup e Restore | **P2** | M | Post-MVP |
| US-020 | Internazionalizzazione i18n EN | **P2** | M | Post-MVP |

---

## EPIC ANAGRAFICHE BASE (US-101 → US-120)

| Story | Titolo | Priorità 2M | Estimation | Week |
|-------|--------|-------------|------------|------|
| US-101 | Gestione Nazioni (seed ISO) | **P0** | S | W3 |
| US-102 | Province e Comuni ISTAT | **P0** | S | W3 |
| US-103 | Valute e Tassi di Cambio | **P0** | M | W3 |
| US-104 | Unità di Misura | **P0** | S | W3 |
| US-105 | Condizioni di Pagamento | **P0** | S | W3 |
| US-106 | Aliquote IVA | **P0** | S | W3 |
| US-107 | Causali magazzino | **P1** | S | W7 |
| US-108 | Agenti di vendita | **P1** | S | W5 |
| US-109 | Vettori / Spedizionieri | **P1** | S | W6 |
| US-110 | Zone geografiche | **P2** | S | Post-MVP |

---

## EPIC CLIENTI (US-121 → US-140)

| Story | Titolo | Priorità 2M | Estimation | Week |
|-------|--------|-------------|------------|------|
| US-121 | CRUD Cliente dati base | **P0** | M | W3 |
| US-122 | Cliente dati fiscali italiani (P.IVA, CF, SDI) | **P0** | M | W3 |
| US-123 | Cliente contatti multipli | **P0** | S | W3 |
| US-124 | Cliente indirizzi multipli | **P0** | S | W3 |
| US-125 | Cliente condizioni commerciali (pagamento, agente) | **P0** | M | W3 |
| US-126 | Cliente dati bancari (IBAN) | **P0** | S | W3 |
| US-127 | Ricerca/filtri avanzati clienti | **P0** | M | W3 |
| US-128 | Import clienti da Excel | **P0** | M | W4 |
| US-129 | Export clienti Excel | **P1** | S | W4 |
| US-130 | Storico documenti cliente | **P2** | M | Post-MVP |

---

## EPIC FORNITORI (US-131 → US-140)

| Story | Titolo | Priorità 2M | Estimation | Week |
|-------|--------|-------------|------------|------|
| US-131 | CRUD Fornitore dati base | **P0** | M | W4 |
| US-132 | Fornitore dati fiscali e bancari | **P0** | M | W4 |
| US-133 | Fornitore contatti e indirizzi | **P0** | S | W4 |
| US-134 | Fornitore condizioni acquisto | **P0** | M | W4 |
| US-135 | Ricerca/filtri fornitori | **P0** | S | W4 |
| US-136 | Import fornitori Excel | **P0** | M | W4 |
| US-137 | Export fornitori Excel | **P1** | S | W4 |
| US-138 | Storico ordini fornitore | **P2** | M | Post-MVP |

---

## EPIC ARTICOLI E LISTINI (US-141 → US-160)

| Story | Titolo | Priorità 2M | Estimation | Week |
|-------|--------|-------------|------------|------|
| US-141 | CRUD Articolo base (codice, descr, cat, UM) | **P0** | M | W4 |
| US-142 | Articolo categorie merceologiche | **P0** | S | W4 |
| US-143 | Articolo codici alternativi (barcode EAN, cod. forn.) | **P0** | S | W4 |
| US-144 | Articolo dati fiscali (IVA, conto ricavi/costi) | **P0** | S | W4 |
| US-145 | Ricerca/filtri articoli | **P0** | M | W4 |
| US-146 | Import articoli Excel | **P0** | M | W4 |
| US-147 | Export articoli Excel | **P1** | S | W4 |
| US-148 | Listini prezzi vendita multipli | **P0** | L | W4 |
| US-149 | Listino prezzi acquisto (base) | **P1** | M | W4 |
| US-150 | Sconti per cliente/categoria | **P0** | M | W5 |
| US-151 | Varianti articolo (taglie, colori) | **P2** | XL | Post-MVP |
| US-152 | Distinta Base (BOM) | **P2** | XL | Post-MVP |

---

## EPIC CICLO ATTIVO (US-201 → US-230) — NUOVE STORY

| Story | Titolo | Priorità 2M | Estimation | Week |
|-------|--------|-------------|------------|------|
| US-NUMER-01 | Numeratori documenti configurabili | **P0** | M | W5 |
| US-201 | CRUD Offerta/Preventivo | **P0** | L | W5 |
| US-202 | Calcolo prezzi da listino + sconti su offerta | **P0** | M | W5 |
| US-203 | Conversione Offerta → Ordine Cliente | **P0** | M | W5 |
| US-204 | CRUD Ordine Cliente | **P0** | L | W5 |
| US-205 | Stato avanzamento ordine | **P0** | M | W5 |
| US-206 | Stampa PDF offerta/ordine | **P0** | M | W5 |
| US-207 | Agente vendita su ordine/offerta | **P1** | S | W5 |
| US-208 | CRUD DDT (da ordine o manuale) | **P0** | L | W6 |
| US-209 | Conversione Ordine → DDT | **P0** | M | W6 |
| US-210 | Stampa PDF DDT | **P0** | M | W6 |
| US-211 | Fattura Attiva (da DDT o da ordine) | **P0** | L | W6 |
| US-212 | Calcolo IVA e totali fattura | **P0** | M | W6 |
| US-213 | Stampa PDF Fattura | **P0** | M | W6 |
| US-214 | Fattura Elettronica SDI — XML FatturaPA | **P0** | L | W6 |
| US-215 | SDI — Invio Hub e gestione stati | **P0** | L | W6 |
| US-216 | Nota di Credito cliente | **P0** | M | W6 |
| US-217 | Scadenzario clienti base (partite aperte) | **P0** | M | W6 |
| US-218 | Export Excel scadenzario clienti | **P0** | S | W6 |
| US-219 | Fattura da DDT differita (raggruppamento) | **P1** | M | W8 |
| US-220 | Provvigioni agenti — calcolo | **P2** | L | Post-MVP |
| US-221 | RI.BA. e SEPA — generazione file | **P2** | L | Post-MVP |

---

## EPIC CICLO PASSIVO (US-301 → US-320) — DEMO ONLY

| Story | Titolo | Priorità 2M | Tipo | Week |
|-------|--------|-------------|------|------|
| US-DEMO-301 | UI Demo Ordini Fornitore | **P1** | Demo | W7 |
| US-DEMO-302 | UI Demo Carico Merce / DDT Entrata | **P1** | Demo | W7 |
| US-DEMO-303 | UI Demo Fatture Passive | **P1** | Demo | W7 |
| US-DEMO-304 | API Skeleton Ciclo Passivo | **P1** | Skeleton | W7 |
| US-301 | CRUD Ordine Fornitore (reale) | **P2** | Production | Post-MVP |
| US-302 | RDA — Richiesta di Acquisto + workflow | **P2** | Production | Post-MVP |
| US-303 | Carico Merce + aggiornamento giacenze | **P2** | Production | Post-MVP |
| US-304 | Fattura Passiva registrazione + abbinamento | **P2** | Production | Post-MVP |

---

## EPIC MAGAZZINO (US-401 → US-420) — DEMO ONLY

| Story | Titolo | Priorità 2M | Tipo | Week |
|-------|--------|-------------|------|------|
| US-DEMO-401 | UI Demo Giacenze per articolo/deposito | **P1** | Demo | W7 |
| US-DEMO-402 | UI Demo Movimenti Magazzino | **P1** | Demo | W7 |
| US-DEMO-403 | UI Demo Inventario Fisico | **P1** | Demo | W7 |
| US-DEMO-404 | API Skeleton Magazzino | **P1** | Skeleton | W7 |
| US-401 | Giacenze reali da movimenti | **P2** | Production | Post-MVP |
| US-402 | Multi-deposito / trasferimenti | **P2** | Production | Post-MVP |
| US-403 | Gestione Lotti | **P2** | Production | Post-MVP |
| US-404 | Inventario fisico reale | **P2** | Production | Post-MVP |

---

## EPIC CONTABILITÀ (US-501 → US-520) — DEMO + SCADENZARIO

| Story | Titolo | Priorità 2M | Tipo | Week |
|-------|--------|-------------|------|------|
| US-217 | Scadenzario clienti (partite aperte) | **P0** | Production | W6 |
| US-DEMO-501 | UI Demo Piano dei Conti | **P1** | Demo | W7 |
| US-DEMO-502 | UI Demo Prima Nota | **P1** | Demo | W7 |
| US-DEMO-503 | UI Demo Registri IVA | **P1** | Demo | W7 |
| US-DEMO-504 | API Skeleton Contabilità | **P1** | Skeleton | W7 |
| US-501 | Prima Nota manuale completa | **P2** | Production | Post-MVP |
| US-502 | Contabilizzazione automatica fatture | **P2** | Production | Post-MVP |
| US-503 | Liquidazione IVA periodica | **P2** | Production | Post-MVP |
| US-504 | Bilancio CEE | **P2** | Production | Post-MVP |

---

## EPIC CRM (US-601 → US-620) — DEMO ONLY

| Story | Titolo | Priorità 2M | Tipo | Week |
|-------|--------|-------------|------|------|
| US-DEMO-601 | UI Demo Contatti/Lead | **P1** | Demo | W8 |
| US-DEMO-602 | UI Demo Pipeline Opportunità (Kanban) | **P1** | Demo | W8 |
| US-DEMO-603 | API Skeleton CRM | **P1** | Skeleton | W8 |
| US-601 | CRM Contatti reali | **P2** | Production | Post-MVP |
| US-602 | CRM Pipeline e opportunità | **P2** | Production | Post-MVP |
| US-603 | CRM Attività / Task | **P2** | Production | Post-MVP |

---

## EPIC INFRASTRUCTURE & DEVELOPER PRODUCTIVITY (Nuove Story)

| Story | Titolo | Priorità 2M | Estimation | Week |
|-------|--------|-------------|------------|------|
| US-INFRA-01 | Repository setup + CI/CD pipeline | **P0** | S | W1 |
| US-INFRA-02 | DB schema completo (EF Core migrations tutte le tabelle) | **P0** | M | W1 |
| US-INFRA-03 | Swagger / OpenAPI documentation | **P0** | S | W1 |
| US-INFRA-04 | Logging strutturato (Serilog) | **P0** | S | W1 |
| US-INFRA-05 | Error handling middleware globale | **P0** | S | W1 |
| US-SEED-01 | Demo Data Seeder completo | **P1** | M | W7 |
| US-KPI-01 | Dashboard KPI Ciclo Attivo (dati reali) | **P0** | M | W8 |
| US-KPI-02 | Dashboard KPI widget mock (altri moduli) | **P1** | S | W8 |
| US-EXPORT-01 | Export Excel: fatture, ordini, scadenzario | **P0** | M | W8 |
| US-UAT-01 | Bug fix + UAT sprint finale | **P0** | L | W8 |
| US-PERF-01 | Performance testing + ottimizzazione query | **P0** | M | W8 |
| US-SEC-01 | Security review checklist OWASP | **P0** | M | W8 |

---

## Riepilogo Story Points 2 Mesi

| Categoria | P0 (Production) | P1 (Demo/Base) | P2 (Posticipato) |
|-----------|----------------|----------------|-----------------|
| Core Platform | 15 story (80 SP) | 5 story | 2 story |
| Anagrafiche | 25 story (85 SP) | 5 story | 4 story |
| Ciclo Attivo | 19 story (90 SP) | 3 story | 3 story |
| Infra/Dev | 8 story (30 SP) | 4 story | 0 story |
| Demo (Passivo+Mag+Cont+CRM) | 0 | 16 story (40 SP) | 16 story |
| **Totale** | **67 story P0** | **33 story P1** | **25 story P2** |

**Story P0 (production-ready)**: 67 storie → Week 1-6 + Week 8 stabilizzazione  
**Story P1 (demo/base)**: 33 storie → Week 7-8  
**Story P2 (post-MVP)**: 25 storie → dopo 2 mesi

---

*Backlog 2 Mesi v1.0 — 2026-06-09*
