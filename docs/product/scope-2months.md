# KBM — Scope Definition 2 Mesi

**Versione**: 1.0  
**Data**: 2026-06-09  
**Owner**: Product Owner Agent  
**Status**: 🚨 Approvato — Timeline 2 Mesi

---

## Tabella Scope per Modulo

| Modulo | Production-Ready | Demo-Ready | Posticipato |
|--------|-----------------|------------|-------------|
| **Core Platform** | ✅ Week 1-2 | — | ❌ SSO/AD, Workflow engine |
| **Anagrafiche** | ✅ Week 3-4 | — | ❌ Varianti articolo, BOM |
| **Ciclo Attivo** | ✅ Week 5-6 | — | ❌ Provvigioni agenti avanzate, Pianificazione consegne |
| **Ciclo Passivo** | ❌ Backend posticipato | ✅ Week 7 UI + skeleton | ⏸️ Backend completo post-MVP |
| **Magazzino** | ❌ Backend posticipato | ✅ Week 7 UI + skeleton | ⏸️ Giacenze reali, lotti, SN post-MVP |
| **Contabilità** | ✅ Scadenzario base Week 6 | ✅ Week 7 UI (prima nota, IVA) | ⏸️ Bilancio, Intrastat, Dichiarazione IVA |
| **CRM** | ❌ | ✅ Week 8 UI (contatti, pipeline) | ⏸️ Backend completo post-MVP |
| **Produzione/MRP** | ❌ | ❌ | ⏸️ Fase 2 progetto (post-2 mesi) |
| **BI/Reportistica** | ✅ Export Excel base Week 8 | ✅ Week 8 Dashboard KPI | ⏸️ Report designer, Power BI connector |
| **Foundation DB** | ✅ Schema completo Week 1 | — | — |
| **API Skeleton** | ✅ Production per Week 1-6 | ✅ Stub per Week 7-8 moduli | — |

---

## Definizione Production-Ready vs Demo-Ready

### ✅ Production-Ready
Modulo con:
- Backend API completo, validato, testato
- Frontend UI completo e funzionale
- Dati reali persistiti su DB
- RBAC/permessi implementati
- Audit log attivo
- Pronto per uso cliente reale

### 🎭 Demo-Ready
Modulo con:
- Frontend UI completo e navigabile
- Dati di demo (seed/mock) precaricati
- API stub/skeleton (restituisce dati fake strutturati)
- Workflow visibile ma non operativo
- Adatto a presentazioni/vendita

### ⏸️ Posticipato (Post-MVP)
- Non implementato in 2 mesi
- DB schema creato (tabelle presenti)
- Nessuna UI, nessuna API reale

---

## Scope Production-Ready Dettagliato (Week 1-6)

### Core Platform (Week 1-2)
**IN SCOPE**:
- ✅ Login + JWT + Refresh Token
- ✅ MFA (TOTP) — opzionale utente, obbligatorio Admin
- ✅ CRUD Utenti (create, edit, disable, enable)
- ✅ RBAC granulare (ruoli + permessi atomici per modulo/risorsa/azione)
- ✅ Ruoli predefiniti (Admin, Manager, Operatore, ReadOnly)
- ✅ Multi-azienda (CRUD, tenant isolation globale)
- ✅ Selezione contesto azienda al login
- ✅ Audit Log middleware (tutte le operazioni CUD)
- ✅ Password policy + reset via email
- ✅ Setup Wizard first-run
- ✅ Sessioni + logout + revoca sessioni

**OUT OF SCOPE (2 mesi)**:
- ❌ SSO / Active Directory / SAML
- ❌ Backup automatico configurabile (manuale da DBA)
- ❌ Notifiche email automatiche avanzate
- ❌ Internazionalizzazione EN (solo IT per ora)

### Anagrafiche (Week 3-4)
**IN SCOPE**:
- ✅ Seed data: Nazioni, Province, Comuni ISTAT, Valute, UM
- ✅ CRUD Clienti completo (dati anagrafici, fiscali IT, contatti, indirizzi, condizioni)
- ✅ Validazione P.IVA + CF + Codice SDI + PEC
- ✅ CRUD Fornitori completo (dati anagrafici, fiscali, bancari IBAN)
- ✅ CRUD Articoli (codice, descrizione, categoria, UM, barcode, prezzi)
- ✅ Categorie merceologiche
- ✅ Listini prezzi vendita multipli (per cliente, categoria, validità)
- ✅ Import Excel (Clienti, Fornitori, Articoli)
- ✅ Export Excel anagrafiche
- ✅ Ricerca/filtri avanzati

**OUT OF SCOPE (2 mesi)**:
- ❌ Varianti articolo (taglie, colori)
- ❌ Distinta base / BOM
- ❌ Listini acquisto fornitore (struttura base, UI posticipata)
- ❌ Sconti a cascata avanzati (politiche sconti semplici solo)
- ❌ Classificazione ATECO dettagliata

### Ciclo Attivo (Week 5-6)
**IN SCOPE**:
- ✅ Numeratori documenti configurabili (per tipo/anno/azienda)
- ✅ Offerte/Preventivi CRUD (testata + righe + prezzi da listino + sconti)
- ✅ Conversione Offerta → Ordine Cliente
- ✅ Ordini Cliente CRUD (stati, righe, date consegna)
- ✅ DDT CRUD (da ordine o manuale, causale, vettore)
- ✅ Fattura Attiva (da DDT o da ordine, IVA, totali)
- ✅ Fattura Elettronica SDI (XML FatturaPA FPR12, invio Hub, gestione stati)
- ✅ Nota di Credito cliente
- ✅ Scadenzario clienti base (partite aperte)
- ✅ Stampa PDF offerta, ordine, DDT, fattura

**OUT OF SCOPE (2 mesi)**:
- ❌ Provvigioni agenti avanzate (campo agente sì, calcolo no)
- ❌ RI.BA. e SEPA (struttura dati sì, generazione file no)
- ❌ Pianificazione consegne avanzata
- ❌ Portale agente/cliente

---

## Scope Demo-Ready Dettagliato (Week 7-8)

### Ciclo Passivo (Demo Week 7)
**INCLUSO NELLA DEMO**:
- 🎭 UI Ordini Fornitore (lista, filtri, form testata+righe — dati fake)
- 🎭 UI Carico Merce / DDT Fornitore (form, navigabile)
- 🎭 UI Fatture Passive (lista con stati — abbinamento manuale visibile)
- 🎭 API stub: `GET /api/purchase-orders`, `GET /api/supplier-invoices` → dati seed

**NON INCLUSO**:
- ❌ Salvataggio reale ordini fornitore a DB con logica
- ❌ Workflow approvazione RDA
- ❌ Abbinamento automatico DDT ↔ OA ↔ Fattura

### Magazzino (Demo Week 7)
**INCLUSO NELLA DEMO**:
- 🎭 UI Giacenze per articolo/deposito (tabella con dati seeded)
- 🎭 UI Movimenti magazzino (lista con tipi: carico, scarico, rettifica)
- 🎭 UI Inventario fisico (form conteggio — navigabile)
- 🎭 API stub: `GET /api/inventory/stock`, `GET /api/inventory/movements`

**NON INCLUSO**:
- ❌ Giacenze reali calcolate da movimenti reali
- ❌ Lotti e numeri seriali
- ❌ Valorizzazione FIFO/LIFO/CMP
- ❌ Trasferimenti inter-deposito reali

### Contabilità (Demo Week 7, Scadenzario Production Week 6)
**PRODUCTION** (Week 6):
- ✅ Scadenzario clienti: partite aperte reali da fatture emesse
- ✅ Export Excel scadenzario

**DEMO** (Week 7):
- 🎭 UI Piano dei conti (albero precaricato standard italiano)
- 🎭 UI Prima Nota (form registrazione doppia entrata — navigabile)
- 🎭 UI Registri IVA vendite/acquisti (tabelle con dati fake)
- 🎭 API stub: `GET /api/accounting/journal`, `GET /api/accounting/vat-registers`

**NON INCLUSO**:
- ❌ Prima nota automatica da fatture (contabilizzazione)
- ❌ Liquidazione IVA periodica reale
- ❌ Bilancio CEE
- ❌ Intrastat, Esterometro, Dichiarazione IVA

### CRM (Demo Week 8)
**DEMO**:
- 🎭 UI Contatti/Lead (lista + form — dati fake)
- 🎭 UI Pipeline opportunità (kanban view — dati fake)
- 🎭 API stub: `GET /api/crm/contacts`, `GET /api/crm/opportunities`

**NON INCLUSO**:
- ❌ Qualsiasi logica CRM reale
- ❌ Integrazione email/calendario
- ❌ Ticketing / post-vendita

---

## Foundation (Parallelo, Week 1)

- ✅ **Database schema completo**: tutte le tabelle Fase 1-7 create con EF Core migrations
  - Tabelle production: Core + Anagrafiche + Ciclo Attivo = dati reali
  - Tabelle demo: Ciclo Passivo + Magazzino + Contabilità completa + CRM = struttura pronta per sviluppo futuro
- ✅ **API skeleton**: tutti i moduli hanno controller con routing, permessi, Swagger
- ✅ **Permission catalog completo**: tutti i permessi di tutti i moduli definiti e seeded
- ✅ **CI/CD pipeline**: GitHub Actions / Azure DevOps build+test automatici
- ✅ **Architecture**: Modular Monolith, Clean Architecture lite, EF Core, REST + Swagger

---

## Competitive Gap Analysis — 2 Mesi

| Feature Essenziale (Feature Matrix) | Status 2 Mesi | Note |
|--------------------------------------|---------------|------|
| Multi-azienda | ✅ Production | |
| Gestione utenti e ruoli | ✅ Production | |
| Audit trail | ✅ Production | |
| Anagrafiche complete | ✅ Production | |
| Listini prezzi | ✅ Production | |
| Ciclo Attivo completo (offerta→DDT→FT) | ✅ Production | |
| Fattura Elettronica SDI | ✅ Production | Libreria esistente |
| Scadenzario clienti | ✅ Production base | |
| Ordine Fornitore / Ciclo Passivo | 🎭 Demo UI | Backend post-MVP |
| Magazzino base | 🎭 Demo UI | Backend post-MVP |
| Contabilità (prima nota, IVA) | 🎭 Demo UI | Scadenzario production |
| Import/Export Excel | ✅ Production | |
| CRM | 🎭 Demo UI | Post-MVP |
| API REST | ✅ Production | Swagger incluso |

**Gap accettabili 2 mesi** (allineato con manager):
- ❌ Contabilità completa (scadenzario base sì, prima nota completa no)
- ❌ Magazzino avanzato (no lotti/SN)
- ❌ Produzione/MRP (posticipato interamente)

---

*Scope 2 Mesi v1.0 — 2026-06-09*
