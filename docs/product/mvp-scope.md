# KBM - MVP Scope Definition

> **SUPERSEDED** — MVP 2 mesi definito in [`scope-2months.md`](scope-2months.md) (include Ciclo Attivo). MFA posticipato a fine progetto.


**Versione**: 1.0  
**Data**: 2026-06-09  
**Owner**: Product Owner Agent  
**Status**: ✅ Approvato

---

## Definizione MVP

Il **MVP (Minimum Viable Product)** di KBM copre **Fase 1 (Core Platform)** e **Fase 2 (Anagrafiche)**.

L'obiettivo del MVP è dimostrare:
1. **Solidità tecnica**: architettura scalabile, sicura, manutenibile
2. **Valore operativo**: un'azienda reale può iniziare a usare KBM per gestire i propri dati master
3. **Base per espansione**: ogni fase successiva si appoggia su MVP solido

---

## MVP Must-Have (Fase 1 + Fase 2)

### Core Platform (Fase 1)
| Feature | Priorità | Rationale |
|---|---|---|
| Login con username/password | P0 | Entry point di tutto il sistema |
| Multi-Factor Authentication (MFA) | P0 | Sicurezza enterprise richiesta da clienti |
| Gestione sessioni e token JWT | P0 | Fondamento sicurezza |
| Password policy configurabile | P0 | Compliance e sicurezza |
| CRUD utenti (admin) | P0 | Necessario per onboarding clienti |
| Ruoli predefiniti (Admin, Manager, User, ReadOnly) | P0 | Permessi di base funzionanti |
| RBAC granulare per modulo/azione | P0 | Differenziatore chiave vs competitor |
| Gestione aziende (multi-tenant per azienda) | P0 | Requisito fondamentale PMI italiane |
| Selezione contesto azienda al login | P0 | UX multi-azienda |
| Audit log completo (tutte le operazioni) | P0 | Compliance, tracciabilità, sicurezza |
| Dashboard home con widget configurabili | P1 | Prima schermata post-login |
| Notifiche in-app (base) | P1 | Comunicazione sistema utente |

### Anagrafiche (Fase 2)
| Feature | Priorità | Rationale |
|---|---|---|
| Anagrafiche comuni (nazioni, province, valute) | P0 | Fondamento di ogni anagrafica |
| CRUD Clienti completo | P0 | Core business — senza clienti non c'è ciclo attivo |
| Clienti: contatti multipli (email, tel, referente) | P0 | Necessario per comunicazioni |
| Clienti: indirizzi multipli (sede, spedizione, fatturazione) | P0 | Indispensabile per DDT e fatture |
| Clienti: dati fiscali (P.IVA, CF, SDI, PEC) | P0 | Obbligatorio per fattura elettronica |
| Clienti: condizioni commerciali (pagamenti, agente) | P1 | Importante per ciclo attivo |
| CRUD Fornitori completo | P0 | Core business — senza fornitori non c'è ciclo passivo |
| Fornitori: dati fiscali e bancari | P0 | Obbligatorio per pagamenti e fatturazione |
| CRUD Articoli completo | P0 | Base magazzino, acquisti e vendite |
| Articoli: categorie e gruppi merceologici | P0 | Organizzazione catalogo |
| Articoli: unità di misura e conversioni | P0 | Necessario per ordini e magazzino |
| Articoli: codici alternativi (barcode, codice fornitore) | P1 | Importante per operatività |
| Listini prezzi vendita (per cliente/categoria) | P0 | Necessario per offerte e ordini |
| Listini prezzi acquisto (per fornitore) | P1 | Utile per Fase 3 |
| Import da Excel/CSV (clienti, fornitori, articoli) | P1 | Migrazione dati da sistemi esistenti |
| Ricerca/filtri avanzati su tutte le anagrafiche | P1 | UX operativa fondamentale |
| Export Excel su tutte le anagrafiche | P2 | Reporting base |

---

## MVP Nice-to-Have (Posticipabili a Fase 3+)

| Feature | Fase Suggerita | Motivazione posticipo |
|---|---|---|
| Single Sign-On (SSO) / Active Directory | Fase 3 | Non bloccante, aggiungibile senza breaking change |
| API REST pubblica (per integrations) | Fase 3 | Dipende da stabilità dati master |
| Articoli: gestione varianti (taglie, colori) | Fase 5 | Complessità alta, serve Magazzino avanzato |
| Clienti: storico acquisti embedded | Fase 4 | Dipende da Ciclo Attivo |
| Listini: sconti a cascata (categoria → cliente) | Fase 3 | Complessità gestibile in Ciclo Passivo |
| Notifiche email automatiche | Fase 3 | SMTP configurabile — non critico MVP |
| Widget dashboard KPI finanziari | Fase 6 | Dipende da Contabilità |
| Mobile app (lettura dati) | Fase 7 | Out of scope MVP |
| Portale self-service clienti | Fase 7 | Out of scope MVP |

---

## Out of Scope MVP

I seguenti moduli sono **esplicitamente esclusi dal MVP** e non devono essere pianificati né implementati in Fase 1-2:

| Modulo | Motivazione |
|---|---|
| Ciclo Passivo (ordini, carichi, fatture fornitore) | Fase 3 |
| Ciclo Attivo (offerte, ordini, DDT, fatture cliente) | Fase 4 |
| Fattura Elettronica SDI | Fase 4 |
| Magazzino (giacenze, movimenti, lotti) | Fase 5 |
| Contabilità (prima nota, IVA, bilancio) | Fase 6 |
| CRM (pipeline, opportunità) | Fase 7 |
| Produzione / MRP | Fase 7 |
| Business Intelligence / Report Builder | Fase 7 |
| E-Commerce connectors | Fase 7 |
| Mobile App nativa | Fase 7 |
| Workflow engine custom | Fase 7 |

---

## Rationale Decisioni Scope

### Perché includere MFA nel MVP?
Le PMI italiane che adottano ERP hanno spesso requisiti assicurativi e normativi che richiedono autenticazione forte. Retrofittare MFA dopo go-live è molto più costoso e impattante. Costo implementazione in Fase 1: basso. Valore: alto.

### Perché RBAC granulare dal MVP?
I competitor legacy hanno permessi rudimentali. RBAC granulare è un differenziatore chiave citato nelle analisi di mercato. Aggiungerlo in seguito richiederebbe refactoring massiccio dell'intera API layer.

### Perché audit log completo dal MVP?
L'audit log non può essere retrofittato su dati storici. Ogni operazione senza audit è un gap permanente. Compliance GDPR e normativa italiana richiedono tracciabilità. Costo: basso (decorator/middleware). Valore: critico.

### Perché posticipare il Ciclo Attivo a Fase 4?
Il Ciclo Attivo (fatturazione) dipende fortemente da Anagrafiche stabili (clienti, articoli, listini) e da infrastrutture tecniche matura (gestione documenti, numeratori, SDI). Forzarlo nel MVP degraderebbe qualità di tutto il sistema.

### Perché includere Import Excel nel MVP?
Senza importazione dati, nessuna PMI può migrare da sistema legacy. Questo è un requisito di onboarding critico. Senza import, il MVP non è vendibile a clienti esistenti.

### Criterio di Prioritizzazione
- **P0**: Bloccante per operatività — senza questa feature il sistema non funziona
- **P1**: Alta importanza operativa — richiesta dalla maggioranza dei clienti target
- **P2**: Valore aggiunto — migliora esperienza ma non blocca adozione

---

## MVP Success Criteria

Al termine di Fase 2, KBM MVP è considerato completo quando:
- [ ] Un'azienda reale può fare login, configurare utenti/ruoli, selezionare azienda
- [ ] Può inserire/importare clienti, fornitori, articoli con tutti i dati fiscali corretti
- [ ] Ogni operazione è tracciata nell'audit log
- [ ] Nessun utente può accedere a dati di un'altra azienda
- [ ] Performance accettabile con dataset realistico (10K clienti, 50K articoli)
- [ ] Almeno 1 cliente pilot validato il sistema in produzione

---

*MVP Scope v1.0 — Da rivedere con feedback Competitive Scouting Agent*
