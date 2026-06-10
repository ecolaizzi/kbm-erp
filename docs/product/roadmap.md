# KBM - Product Roadmap

> **SUPERSEDED** — Per la timeline attuale usa [`roadmap-2months.md`](roadmap-2months.md) e [`scope-2months.md`](scope-2months.md). Questo documento resta come riferimento Fase 1-7 originale (12-18 mesi).


**Versione**: 1.0  
**Data**: 2026-06-09  
**Owner**: Product Owner Agent  
**Status**: ✅ Approvato

---

## Overview Timeline

```
2026         Q3 2026      Q4 2026      Q1 2027      Q2 2027      Q3 2027      Q4 2027      2028+
 |             |            |            |            |            |            |            |
[Fase 0]----[Fase 1]-----[Fase 2]-----[Fase 3]-----[Fase 4]-----[Fase 5]-----[Fase 6]---[Fase 7]
 Foundation  Core          Anagrafiche  Ciclo        Ciclo        Magazzino    Contabilità  Estensioni
             Platform      Base         Passivo      Attivo       Avanzato
```

---

## Fase 0 - Foundation & Design (4-6 settimane | Effort: S)

**Timeline**: Giugno 2026 → Luglio 2026  
**Obiettivo**: Definire architettura, schema DB, linee guida UX, backlog prodotto

### Milestone
- ✅ M0.1: Decision Log e Governance setup
- ✅ M0.2: Expert specializzati creati e operativi
- 🔄 M0.3: Architecture Decision Records (ADR) completati
- 🔄 M0.4: Schema database Fase 1-2 approvato
- 🔄 M0.5: Security model e RBAC design approvato
- 🔄 M0.6: UX guidelines e component library definiti
- 🔄 M0.7: Product backlog Fase 1-2 con 50+ user stories

### Deliverable
- `vfs://org/kbm/deliverables/architecture/` - ADR, tech stack, solution architecture
- `vfs://org/kbm/deliverables/database/` - Schema SQL Server, migration strategy
- `vfs://org/kbm/deliverables/security/` - RBAC matrix, threat model, audit strategy
- `vfs://org/kbm/deliverables/ux/` - UX guidelines, design system
- `vfs://org/kbm/deliverables/product/` - Vision, roadmap, backlog

---

## Fase 1 - Core Platform (8-10 settimane | Effort: L)

**Timeline**: Luglio 2026 → Settembre 2026  
**Obiettivo**: Foundation tecnica dell'ERP — autenticazione, multi-azienda, RBAC, audit

### Milestone
- M1.1: Infrastruttura progetto (repository, CI/CD, solution structure)
- M1.2: Database schema Fase 1 applicato (migrations)
- M1.3: Autenticazione e sessioni (Login, MFA, password policy)
- M1.4: Gestione utenti (CRUD utenti, reset password, stato)
- M1.5: Gestione ruoli e permessi (RBAC granulare)
- M1.6: Gestione aziende (multi-azienda, selezione contesto)
- M1.7: Audit log completo (tutte le operazioni tracciate)
- M1.8: Dashboard home e shell applicativa

### Dipendenze
- 🔴 Dipende da: Fase 0 completata (ADR, DB schema, UX guidelines, security model)
- 🟡 Blocker per: tutte le fasi successive

### Effort: L (8-10 settimane, 2-3 sviluppatori)

---

## Fase 2 - Anagrafiche (8-10 settimane | Effort: L)

**Timeline**: Settembre 2026 → Novembre 2026  
**Obiettivo**: Dati master dell'ERP — clienti, fornitori, articoli, listini

> 🧭 **Parità Business Cube** — la mappatura completa delle macro-aree della
> [Guida Analitica](https://servizi.ntsinformatica.it/BusHelpWs/helpnet2017sr5/html/gaguidaan.htm)
> sul nostro stato è in [`business-cube-parity.md`](business-cube-parity.md).
> La Fase 2 è anticipata dalla sotto-fase **Tabelle e Archivi** (base normalizzata).

### Milestone
- **M2.0: Tabelle e Archivi (base normalizzata, stile Cube `subm0100`)**
  - ✅ Condizioni di pagamento (PAG) — rate + scadenziere automatico
  - 🟨 Codici IVA (IVA), Unità di misura (UM), Zone (ZONE)
  - ⬜ Causali documento/magazzino, Banche/ABI-CAB, Località, Valute
- M2.1: Anagrafiche comuni (nazioni, province, comuni, valute, banche)
- M2.2: Clienti (CRUD completo, contatti, indirizzi, condizioni commerciali)
- M2.3: Fornitori (CRUD, contatti, condizioni di acquisto)
- M2.4: Articoli (CRUD, categorie, unità di misura, codici alternativi)
- M2.5: Listini prezzi (clienti, fornitori, sconti, validità)
- M2.6: Import/Export dati (Excel, CSV)
- M2.7: Ricerca avanzata e filtri su tutte le anagrafiche

### Dipendenze
- 🔴 Dipende da: Fase 1 completata (Core Platform operativo)
- 🟡 Blocker per: Fase 3 (Ciclo Passivo), Fase 4 (Ciclo Attivo)

### Effort: L (8-10 settimane, 2-3 sviluppatori)

---

## Fase 3 - Ciclo Passivo (10-12 settimane | Effort: XL)

**Timeline**: Novembre 2026 → Febbraio 2027  
**Obiettivo**: Gestione acquisti — RDA, ordini, carichi, fatture passive

### Milestone
- M3.1: Richieste di Acquisto (RDA) e workflow approvazione
- M3.2: Ordini di Acquisto (ODA) a fornitore
- M3.3: Entrate Merce (DDT fornitore → carico magazzino)
- M3.4: Fatture Passive (registrazione, abbinamento a ODA/DDT)
- M3.5: Gestione resi a fornitore
- M3.6: Scadenzario fornitori (base)

### Dipendenze
- 🔴 Dipende da: Fase 2 (Anagrafiche completate)
- 🟡 Blocker per: Fase 5 (Magazzino avanzato)

### Effort: XL (10-12 settimane, 3 sviluppatori)

---

## Fase 4 - Ciclo Attivo (10-12 settimane | Effort: XL)

**Timeline**: Febbraio 2027 → Maggio 2027  
**Obiettivo**: Gestione vendite — offerte, ordini, DDT, fatture attive, SDI

### Milestone
- M4.1: Offerte commerciali (preventivi cliente)
- M4.2: Ordini di Vendita (OV) e conferma ordine
- M4.3: DDT (Documento di Trasporto) e spedizione
- M4.4: Fatture Attive (emissione, stampa)
- M4.5: Fattura Elettronica SDI (integrazione Agenzia Entrate)
- M4.6: Note di credito e resi cliente
- M4.7: Scadenzario clienti (base)

### Dipendenze
- 🔴 Dipende da: Fase 2 (Anagrafiche), Fase 3 (consigliata per flussi completi)
- 🟡 Blocker per: Fase 6 (Contabilità — partite aperte)

### Effort: XL (10-12 settimane, 3 sviluppatori)

---

## Fase 5 - Magazzino Avanzato (8-10 settimane | Effort: L)

**Timeline**: Maggio 2027 → Luglio 2027  
**Obiettivo**: Giacenze multi-magazzino, lotti, inventario, trasferimenti

### Milestone
- M5.1: Magazzini multipli e ubicazioni
- M5.2: Movimenti manuali e rettifiche
- M5.3: Gestione lotti e numeri seriali
- M5.4: Inventario fisico e rettifica
- M5.5: Valorizzazione magazzino (LIFO, FIFO, Costo Medio)
- M5.6: Reportistica giacenze e movimenti

### Dipendenze
- 🔴 Dipende da: Fase 3 (Ciclo Passivo), Fase 4 (Ciclo Attivo)

### Effort: L (8-10 settimane, 2-3 sviluppatori)

---

## Fase 6 - Contabilità (12-14 settimane | Effort: XL)

**Timeline**: Luglio 2027 → Ottobre 2027  
**Obiettivo**: Contabilità generale — piano conti, prima nota, IVA, scadenzario, bilancio

### Milestone
- M6.1: Piano dei conti (CoA italiano standardizzato)
- M6.2: Prima nota manuale e automatica (da fatture)
- M6.3: Registri IVA (acquisti, vendite, corrispettivi)
- M6.4: Liquidazione IVA periodica
- M6.5: Scadenzario completo (clienti + fornitori)
- M6.6: Estratto conto e partite aperte
- M6.7: Bilancio di verifica e situazione patrimoniale

### Dipendenze
- 🔴 Dipende da: Fase 3 e Fase 4 (fatturazione completa)

### Effort: XL (12-14 settimane, 3 sviluppatori)

---

## Fase 7 - Estensioni (Ongoing | Effort: XL+)

**Timeline**: 2028+  
**Obiettivo**: Moduli avanzati opzionali

### Moduli Pianificati
- **CRM**: Pipeline vendite, opportunità, attività
- **Workflow Engine**: Approvazioni configurabili, notifiche
- **Produzione / MRP**: Distinte basi, ordini di produzione, fabbisogni
- **Business Intelligence**: Dashboard KPI, report builder, export Power BI
- **E-Commerce Integration**: Connettori WooCommerce, Magento, Amazon
- **Mobile App**: App iOS/Android per agenti e magazzinieri

---

## Riepilogo Roadmap

| Fase | Descrizione | Timeline | Effort | Dipende da |
|---|---|---|---|---|
| Fase 0 | Foundation & Design | Giu-Lug 2026 | S | — |
| Fase 1 | Core Platform | Lug-Set 2026 | L | Fase 0 |
| Fase 2 | Anagrafiche | Set-Nov 2026 | L | Fase 1 |
| Fase 3 | Ciclo Passivo | Nov 2026-Feb 2027 | XL | Fase 2 |
| Fase 4 | Ciclo Attivo | Feb-Mag 2027 | XL | Fase 2 |
| Fase 5 | Magazzino Avanzato | Mag-Lug 2027 | L | Fase 3+4 |
| Fase 6 | Contabilità | Lug-Ott 2027 | XL | Fase 3+4 |
| Fase 7 | Estensioni | 2028+ | XL+ | Fase 6 |

---

*Roadmap v1.0 — Timeline indicativa, soggetta a revisione al termine di ogni fase*
