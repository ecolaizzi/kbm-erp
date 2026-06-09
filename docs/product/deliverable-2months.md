# KBM — Deliverable 2 Mesi: Cosa Avremo Davvero

**Versione**: 1.0  
**Data**: 2026-06-09  
**Owner**: Product Owner Agent  
**Audience**: Management, Investitori, Team di Sviluppo

---

## Executive Summary

Dopo 2 mesi (8 settimane), KBM sarà un **ERP parzialmente funzionante** con:
- **Prodotto vendibile** oggi: Core Platform + Anagrafiche + Ciclo Attivo (clienti reali possono usarlo)
- **Demo completa** di tutti i moduli: UI navigabile per Ciclo Passivo, Magazzino, Contabilità, CRM
- **Foundation solida** per espansione rapida: schema DB completo, architettura scalabile

---

## 1. FUNZIONANTE — Production-Ready (Vendibile Subito)

### 1.1 Core Platform
- ✅ **Login multi-utente** con JWT, MFA opzionale (obbligatorio Admin)
- ✅ **RBAC granulare**: ruoli + permessi per modulo/risorsa/azione
- ✅ **Multi-azienda**: tenant isolation completa, selezione contesto
- ✅ **Gestione utenti**: CRUD, disabilita/riabilita, reset password
- ✅ **Audit Log**: ogni operazione tracciata (chi, cosa, quando, prima/dopo)
- ✅ **Setup Wizard**: configurazione guidata primo avvio
- ✅ **Dashboard**: widget contestuali per ruolo, accessi recenti
- ✅ **Navigation shell**: menu dinamico per permessi, ricerca globale

### 1.2 Anagrafiche
- ✅ **Clienti completi**: dati anagrafici, P.IVA/CF validati, SDI code, PEC, contatti multipli, indirizzi multipli, condizioni commerciali, IBAN
- ✅ **Fornitori completi**: dati anagrafici, fiscali, IBAN, condizioni acquisto
- ✅ **Articoli completi**: codice, descrizione, categoria, UM, barcode EAN, codice fornitore, IVA, conto contabile
- ✅ **Listini prezzi**: vendita per cliente/categoria, validità temporale
- ✅ **Sconti**: per cliente, per categoria, sconto riga su offerta/ordine
- ✅ **Import Excel**: upload clienti, fornitori, articoli con report errori
- ✅ **Export Excel**: su tutte le anagrafiche
- ✅ **Dati di sistema**: Nazioni (250+), Province, Comuni ISTAT, Valute (ISO 4217), UM, Aliquote IVA, Condizioni pagamento

### 1.3 Ciclo Attivo (Vendite)
- ✅ **Offerte/Preventivi**: crea, modifica, invia, stati (bozza/inviata/accettata/rifiutata)
- ✅ **Conversione documenti**: Offerta → Ordine → DDT → Fattura (1-click)
- ✅ **Ordini Cliente**: CRUD, righe, prezzi da listino, sconti, stato avanzamento
- ✅ **DDT**: Documento di Trasporto, causale, vettore, stampa PDF
- ✅ **Fattura Attiva**: emissione, calcolo IVA multi-aliquota, stampa PDF
- ✅ **Fattura Elettronica SDI**: generazione XML FatturaPA (FPR12), invio Hub, gestione stati (inviata/consegnata/scartata)
- ✅ **Nota di Credito**: storno parziale o totale fattura cliente
- ✅ **Scadenzario Clienti**: partite aperte da fatture, export Excel
- ✅ **Numeratori**: configurabili per tipo documento/anno/azienda
- ✅ **Stampa PDF**: offerta, ordine, DDT, fattura (layout standard IT)

### 1.4 Export & Reporting Base
- ✅ **Export Excel**: clienti, fornitori, articoli, ordini, fatture, scadenzario
- ✅ **Dashboard KPI** (dati reali): fatturato mese, ordini aperti, partite scadute, DDT da fatturare
- ✅ **Swagger API**: documentazione completa di tutti gli endpoint

---

## 2. DEMO-READY — Presentabile (Dati Mock)

### 2.1 Ciclo Passivo (UI navigabile, workflow visibile)
- 🎭 Lista Ordini Fornitore con filtri e stati
- 🎭 Form Ordine Fornitore con righe e prezzi
- 🎭 Lista e form Carico Merce / DDT Entrata
- 🎭 Lista Fatture Passive con stati abbinamento
- *Dati: 20 ordini fornitore fake, 15 fatture passive seed*

### 2.2 Magazzino (UI navigabile, giacenze simulate)
- 🎭 Giacenze per articolo/deposito (tabella paginata)
- 🎭 Lista movimenti magazzino (filtri per tipo, data, articolo)
- 🎭 Form inventario fisico (conteggio)
- *Dati: giacenze simulate per 200 articoli seed*

### 2.3 Contabilità (UI navigabile, dati fake)
- 🎭 Piano dei conti (albero standard italiano precaricato)
- 🎭 Form prima nota (doppia entrata)
- 🎭 Registri IVA vendite e acquisti
- *Dati: registrazioni contabili fake per il periodo demo*
- **+ PRODUCTION**: Scadenzario clienti (partite aperte reali)

### 2.4 CRM (UI navigabile, kanban visibile)
- 🎭 Lista Contatti/Lead con form dettaglio
- 🎭 Pipeline opportunità (kanban view: Prospect/Qualificato/Proposta/Chiuso)
- *Dati: 50 contatti fake, 20 opportunità in vari stadi*

### 2.5 Dashboard KPI completa
- 🎭 Widget mock: giacenze magazzino, DA pagare fornitori, Pipeline CRM valore
- ✅ Widget reali: fatturato, ordini aperti, scaduto clienti, DDT da fatturare

---

## 3. FOUNDATION — Preparato per Futuro

### 3.1 Database Schema Completo
- ✅ **Tutte le tabelle** di tutti i moduli create (Fase 1-7)
- ✅ EF Core migrations applicate — DB pronto per sviluppo futuro senza migration gap
- ✅ Indici di performance su tutte le foreign key e colonne di ricerca frequente
- ✅ Soft delete su tutte le entità principali
- Tabelle production con dati reali: ~40 tabelle (Core + Anagrafiche + Ciclo Attivo)
- Tabelle foundation con struttura pronta: ~60 tabelle (Passivo, Magazzino, Contab., CRM, MRP, BI)

### 3.2 Architecture Scalabile
- ✅ **.NET 8** Modular Monolith — cartella separata per modulo, interfacce chiare
- ✅ **EF Core** Code-First — migration-based, facile aggiungere nuove entità
- ✅ **REST API** con Swagger — documentazione automatica, testabile
- ✅ **Clean Architecture lite** — nessun CQRS pesante, rapid development
- ✅ **Logging** (Serilog): structured logs su file + DB
- ✅ **Error handling**: middleware globale con response standard

### 3.3 API Skeleton Tutti i Moduli
- ✅ Controller con routing per tutti i moduli (anche non production)
- ✅ Swagger completo con tutti gli endpoint — frontend può sviluppare parallelamente
- ✅ Permission checks su ogni endpoint (anche stub)
- ✅ Response DTO strutturati per tutti i moduli

### 3.4 Permission Catalog Completo
- ✅ **Tutti i permessi** di tutti i moduli definiti (`{modulo}.{risorsa}.{azione}`)
- ✅ Permission seeding DB — già pronti per assegnazione a ruoli
- ✅ Sezione Configurazione Permessi in UI admin

### 3.5 CI/CD Pipeline
- ✅ Build automatico su ogni commit
- ✅ Test automatici (unit + integration) su PR
- ✅ Deploy su ambiente staging automatizzato

---

## 4. VALUE PROPOSITION DOPO 2 MESI

### Scenario A: Cliente Piccola Impresa (1-5 utenti)
**Può usare KBM dal giorno 1 per**:
1. Creare e gestire la propria anagrafica clienti completa (migrazione da Excel)
2. Emettere offerte, ordini, DDT e fatture ai clienti
3. Inviare fatture elettroniche all'Agenzia delle Entrate via SDI
4. Tenere traccia delle partite aperte (scadenzario)
5. Gestire ruoli (Admin + Operatore) con permessi separati

**Non può ancora fare**: acquisti, magazzino, contabilità completa

### Scenario B: Demo per Prospect Enterprise
**KBM mostra**:
1. Login, selezione azienda, dashboard professionale
2. Anagrafica completa + ciclo attivo funzionante (live demo)
3. Navigate to: Acquisti (UI completa, workflow visibile)
4. Navigate to: Magazzino (UI completa, giacenze simulate)
5. Navigate to: Contabilità (UI completa, prima nota visibile)
6. Navigate to: CRM (pipeline kanban professionale)
7. "Il backend di questi moduli sarà completato nel trimestre successivo"

### Scenario C: Pitch Investitori
**Cosa presentare**:
- ERP funzionante su core commerciale (dimostrabile live)
- UI completo di tutta la suite (impressione visiva completa)
- Architettura documentata, DB schema completo, API swagger
- Timeline: moduli aggiuntivi ogni 4-6 settimane

---

## 5. COSA NON AVREMO DOPO 2 MESI (Onestà Critica)

| Funzionalità | Status | Disponibile quando |
|---|---|---|
| Ciclo Passivo reale (acquisti con backend) | ❌ | +4-6 settimane (Fase 2) |
| Magazzino giacenze reali | ❌ | +4-6 settimane (Fase 2) |
| Contabilità prima nota reale | ❌ | +8-10 settimane (Fase 3) |
| Scadenzario fornitori | ❌ | +4-6 settimane (Fase 2) |
| Pagamenti SEPA / RI.BA. | ❌ | +8-10 settimane (Fase 3) |
| CRM funzionante | ❌ | +10-12 settimane (Fase 4) |
| Produzione / MRP | ❌ | +6 mesi (Fase 5) |
| Bilancio / Dichiarazione IVA | ❌ | +4-6 mesi (Fase 4) |
| Mobile App | ❌ | +6 mesi (Fase 5) |
| SSO / Active Directory | ❌ | +4 settimane (Fase 2) |
| Multi-lingua EN | ❌ | +2-3 settimane (Fase 2) |

---

## 6. METRICHE DI SUCCESSO 2 MESI

### Funzionale
- [ ] Login funzionante per almeno 3 ruoli diversi
- [ ] 10K clienti importabili via Excel senza errori
- [ ] Fattura elettronica SDI inviata e consegnata in ambiente test
- [ ] Almeno 1 azienda pilot che usa il sistema in produzione (Core + Anagrafiche + Ciclo Attivo)

### Tecnico
- [ ] Tempo risposta API < 200ms per operazioni standard (95° percentile)
- [ ] 0 vulnerabilità critiche OWASP Top 10
- [ ] Test coverage > 70% su moduli production
- [ ] CI/CD pipeline green su main branch

### Demo
- [ ] Demo completa navigabile senza crash in 30 minuti
- [ ] Tutti i moduli visibili e navigabili (anche se demo only)
- [ ] Dashboard KPI con dati significativi (reali + mock)

---

*Deliverable 2 Mesi v1.0 — 2026-06-09*
