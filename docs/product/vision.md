# KBM - Product Vision

**Versione**: 1.0  
**Data**: 2026-06-09  
**Owner**: Product Owner Agent  
**Status**: ✅ Approvato

---

## Vision Statement

**KBM (Kever Business Manager)** è il gestionale ERP di nuova generazione per le PMI italiane — un sistema modulare, robusto e intuitivo che accompagna l'azienda in ogni fase del suo ciclo operativo: dall'anagrafica alla contabilità, dagli acquisti alle vendite, dal magazzino alla produzione.

A differenza delle soluzioni legacy (monolitiche, costose, difficili da aggiornare), KBM nasce con un'architettura moderna client-server basata su .NET e SQL Server, progettata per essere mantenibile, scalabile e sicura. L'obiettivo è offrire alle PMI italiane un prodotto enterprise-grade con il TCO (Total Cost of Ownership) accessibile per aziende da 5 a 500 utenti.

---

## Target Users

### Segmento Primario: PMI Manifatturiere e Commerciali Italiane
- **Dimensione**: 10–200 dipendenti, 2–50 utenti ERP
- **Settori**: Commercio all'ingrosso, distribuzione, piccola manifattura, servizi B2B
- **Profilo IT**: Team IT interno minimo o assente; dipendenza da partner/rivenditori
- **Pain point**: Sistemi legacy costosi, scarsa usabilità, aggiornamenti difficili, nessuna mobilità

### Segmento Secondario: Studi Professionali e Partner ERP
- **Commercialisti e consulenti**: necessità di accesso multi-azienda, scadenzario, prima nota
- **System integrator e rivenditori**: cercano un prodotto moderno da rivendere/customizzare

### Ruoli Utente Principali
| Ruolo | Descrizione | Priorità |
|---|---|---|
| Amministratore ERP | Configura utenti, ruoli, aziende, audit | P0 |
| Responsabile Acquisti | Gestisce ordini, fornitori, carichi | P0 |
| Responsabile Vendite | Offerte, ordini, DDT, fatture | P0 |
| Magazziniere | Movimenti, giacenze, inventario | P1 |
| Contabile | Prima nota, scadenzario, bilancio | P1 |
| Agente di Vendita | Accesso limitato CRM e ordini | P2 |
| CEO/Management | Dashboard KPI, reportistica | P2 |

---

## Unique Value Proposition

### "ERP Enterprise per PMI: potenza senza complessità"

KBM differenzia rispetto ai competitor su 5 assi:

| Valore | KBM | Competitor Legacy (Zucchetti, Passepartout) | SaaS SMB (Fatture in Cloud, Odoo) |
|---|---|---|---|
| **Architettura** | .NET moderno, SQL Server, multi-tier | Tecnologie anni '90-2000, monolitico | Cloud-only, SaaS |
| **Deployment** | On-premise, RDS/Citrix, hybrid | On-premise legacy | Solo cloud |
| **Sicurezza** | RBAC granulare, audit log completo, MFA | Permessi rudimentali | Dipende da vendor |
| **Usabilità** | UX moderna, responsive, dark mode | UI datata, non intuitiva | Semplice ma limitato |
| **TCO** | Licenza one-time o subscription, no lock-in | Costoso, lock-in totale | Subscription cresce con volume |
| **Modulare** | Pay-per-modulo, crescita graduale | Bundle fisso | Limitato in funzionalità avanzate |
| **Italiano** | Conforme normativa italiana (IVA, SDI, Intrastat) | Sì ma costoso | Parziale |

---

## Success Metrics

### Metriche di Prodotto (Fase 1-2)
- ✅ Core Platform: login, multi-azienda, RBAC funzionanti in < 2 secondi response time
- ✅ Audit log: 100% delle operazioni tracciate con user, timestamp, delta
- ✅ Anagrafiche: CRUD completo su clienti, fornitori, articoli con validazione dati
- ✅ Performance: list pages con 10.000+ record in < 1 secondo

### Metriche di Qualità
- ✅ Test coverage > 80% su business logic (unit + integration)
- ✅ Zero security vulnerabilities critiche (OWASP Top 10)
- ✅ Uptime > 99.5% in produzione

### Metriche di Adozione (12 mesi dal lancio)
- 🎯 10+ aziende pilot (Fase 1-2)
- 🎯 50+ aziende attive (Fase 3-4)
- 🎯 NPS > 40 tra utenti attivi
- 🎯 < 5% churn mensile

### Metriche di Business
- 🎯 Breakeven entro 24 mesi
- 🎯 ARR (Annual Recurring Revenue) obiettivo: €500K a 3 anni
- 🎯 Costo di supporto < 15% del revenue

---

## Principi Guida

1. **Italian-first**: conformità normativa italiana (IVA, SDI fattura elettronica, Intrastat, F24) come requisito non negoziabile
2. **Security by design**: RBAC granulare e audit log da Fase 1, non retrofittato
3. **Modularità reale**: ogni modulo deployabile e acquistabile indipendentemente
4. **Performance su dati reali**: testato con dataset PMI (10K clienti, 100K articoli, 500K righe ordini)
5. **UX umana**: ogni schermata progettata con utente reale, non solo sviluppatore
6. **Manutenibilità**: architettura che permette aggiornamenti senza big bang migration

---

*Vision v1.0 — Da rivedere a fine Fase 2 con feedback pilot customers*
