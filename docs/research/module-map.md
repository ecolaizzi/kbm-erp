# Module Map — Struttura Modulare ERP Enterprise
> Progetto KBM | Scouting Competitivo | Accesso: 2026-06-09

---

## 1. ARCHITETTURA MODULARE TIPICA ERP ITALIANO

```
┌─────────────────────────────────────────────────────────────────┐
│                        CORE PLATFORM                            │
│  Utenti | Ruoli | Permessi | Multi-azienda | Audit | Lingua    │
└─────────────┬────────────────────────────────┬──────────────────┘
              │                                │
    ┌─────────┴──────────┐          ┌──────────┴──────────┐
    │   ANAGRAFICHE      │          │   CONFIGURAZIONI     │
    │  Clienti/Fornitori │          │  Piano conti | IVA   │
    │  Articoli | Prezzi │          │  Causali | Parametri │
    └─────────┬──────────┘          └─────────────────────┘
              │
    ┌─────────┴──────────────────────────────────────────┐
    │                 MODULI OPERATIVI                    │
    │                                                     │
    │  ┌───────────┐  ┌───────────┐  ┌─────────────┐    │
    │  │  VENDITE  │  │ ACQUISTI  │  │  MAGAZZINO  │    │
    │  │ (Ciclo    │  │ (Ciclo    │  │ Giacenze    │    │
    │  │  Attivo)  │  │  Passivo) │  │ Movimenti   │    │
    │  └─────┬─────┘  └─────┬─────┘  └──────┬──────┘    │
    │        └──────────────┴────────────────┘           │
    │                       │                            │
    │              ┌────────┴────────┐                   │
    │              │  CONTABILITÀ    │                   │
    │              │  Prima nota     │                   │
    │              │  IVA            │                   │
    │              │  Scadenzario    │                   │
    │              └─────────────────┘                   │
    └─────────────────────────────────────────────────────┘
              │
    ┌─────────┴──────────────────────────────────────────┐
    │              MODULI OPZIONALI                       │
    │                                                     │
    │  CRM  |  Produzione  |  HR  |  BI  |  E-commerce  │
    └─────────────────────────────────────────────────────┘
```

---

## 2. MODULI CORE (Obbligatori)

### 2.1 Core Platform
**Funzione**: infrastruttura trasversale del sistema
**Dipendenze**: nessuna (è la base)
**Componenti**:
- Gestione utenti e autenticazione
- Ruoli e profili di accesso
- Multi-azienda e multi-sede
- Audit log e sicurezza
- Gestione esercizi e periodi contabili
- Parametri di sistema
- Import/Export dati
- Gestione stampe e template documenti

### 2.2 Anagrafiche
**Funzione**: archivi dati di base condivisi da tutti i moduli
**Dipendenze**: Core Platform
**Componenti**:
- Anagrafica clienti (dati commerciali + fiscali)
- Anagrafica fornitori
- Anagrafica articoli / prodotti / servizi
- Categorie merceologiche
- Listini prezzi e sconti
- Unità di misura
- Piano dei conti contabili
- Tabelle IVA
- Causali contabili e di magazzino
- Banche e coordinate bancarie

---

## 3. MODULI OPERATIVI PRIMARI

### 3.1 Ciclo Attivo (Vendite)
**Funzione**: gestione del processo vendita dall'offerta all'incasso
**Dipendenze**: Core, Anagrafiche, Magazzino, Contabilità
**Componenti**:
- Offerte / Preventivi
- Ordini cliente
- DDT (Documenti di Trasporto)
- Fatture attive + integrazione SDI
- Note credito attive
- Gestione agenti e provvigioni
- Listini e condizioni commerciali

### 3.2 Ciclo Passivo (Acquisti)
**Funzione**: gestione del processo acquisto dalla RDA al pagamento
**Dipendenze**: Core, Anagrafiche, Magazzino, Contabilità
**Componenti**:
- Richieste di acquisto
- Ordini fornitori
- Carichi merce / DDT entrata
- Fatture passive + import SDI
- Note debito/credito passive
- Resi fornitori

### 3.3 Magazzino
**Funzione**: gestione fisica e contabile delle scorte
**Dipendenze**: Core, Anagrafiche
**Componenti**:
- Giacenze per articolo/deposito
- Movimenti di magazzino (causali configurabili)
- Multi-deposito
- Gestione lotti e serial number
- Inventario fisico
- Valorizzazione (FIFO, LIFO, costo medio)
- Trasferimenti interni

### 3.4 Contabilità Generale
**Funzione**: registrazioni contabili, IVA, bilancio
**Dipendenze**: Core, Anagrafiche
**Componenti**:
- Piano dei conti e causali
- Prima nota (manuale e automatica)
- Registri IVA (acquisti e vendite)
- Liquidazione IVA periodica
- Scadenzario clienti e fornitori
- Partite aperte
- Libro mastro
- Bilancio CEE (SP + CE)
- Riconciliazione bancaria
- Chiusura/apertura esercizio

---

## 4. MODULI SECONDARI (Comuni)

### 4.1 Contabilità Analitica / Controllo di Gestione
**Funzione**: analisi costi per centri di costo / commesse
**Dipendenze**: Contabilità Generale
**Componenti**:
- Centri di costo e centri di profitto
- Budget per CDC
- Report margini e varianze
- Ripartizione costi

### 4.2 Tesoreria
**Funzione**: gestione flussi di cassa e liquidità
**Dipendenze**: Contabilità, Ciclo Attivo, Ciclo Passivo
**Componenti**:
- Cash flow previsionale
- Gestione RI.BA. (presentazione, accredito)
- Bonifici SEPA (file CBI)
- Estratto conto bancario
- Multi-banca

### 4.3 Cespiti / Immobilizzazioni
**Funzione**: gestione beni strumentali aziendali
**Dipendenze**: Contabilità Generale
**Componenti**:
- Registro cespiti
- Ammortamenti (fiscali e civilistici)
- Dismissioni e vendite
- Rivalutazioni

---

## 5. MODULI OPZIONALI

### 5.1 CRM (Customer Relationship Management)
**Funzione**: gestione relazioni commerciali e pipeline vendite
**Dipendenze**: Anagrafiche, Ciclo Attivo
**Presente in**: Zucchetti Ad Hoc ✅, SAP B1 ✅, Dynamics 365 ✅, Odoo ✅
**Assente/Limitato**: NTS Business Cube ❌, Zucchetti Mago ➖

### 5.2 Produzione (MRP)
**Funzione**: pianificazione e gestione della produzione
**Dipendenze**: Anagrafiche (BOM), Magazzino
**Presente in**: Zucchetti Ad Hoc ✅, Zucchetti Mago 🚀, SAP B1 ✅, Dynamics 365 ✅, Odoo 🚀
**Assente/Limitato**: NTS Business Cube ➖

### 5.3 Business Intelligence (BI)
**Funzione**: analisi dati avanzata, KPI, reporting
**Dipendenze**: Tutti i moduli (fonte dati)
**Presente in**: Dynamics 365 (Power BI) 🚀, Odoo ✅, SAP B1 ✅
**Limitato**: Zucchetti Ad Hoc ⭐, NTS ⭐

### 5.4 E-commerce / Portale B2B
**Funzione**: vendita online integrata con ERP
**Dipendenze**: Ciclo Attivo, Anagrafiche, Magazzino
**Presente in**: Odoo 🚀, Dynamics 365 ✅
**Assente**: Zucchetti, NTS, SAP B1 (tramite addon)

### 5.5 Gestione Progetto / Commesse
**Funzione**: pianificazione e controllo progetti
**Dipendenze**: Contabilità Analitica
**Presente in**: Dynamics 365 🚀, Odoo ✅
**Limitato**: Zucchetti Ad Hoc ⭐, SAP B1 ⭐

### 5.6 HR / Paghe
**Funzione**: gestione dipendenti, presenze, paghe
**Dipendenze**: Core, Anagrafiche
**Nota**: In Italia spesso modulo separato (Zucchetti HR leader di mercato)

---

## 6. DIPENDENZE TRA MODULI

```
Core Platform → TUTTI i moduli (prerequisito assoluto)
Anagrafiche → Ciclo Attivo, Ciclo Passivo, Magazzino, Contabilità
Magazzino → Ciclo Attivo (scarico da DDT), Ciclo Passivo (carico da OA)
Ciclo Attivo → Contabilità (fatture attive)
Ciclo Passivo → Contabilità (fatture passive)
Contabilità → Tesoreria, Cespiti, Contabilità Analitica
Contabilità Analitica → BI, Controllo di Gestione
Ciclo Attivo + Anagrafiche → CRM (bidirezionale)
Magazzino + Anagrafiche (BOM) → Produzione
```

---

## 7. CONFRONTO LICENSING MODEL

| ERP | Modello | Core incluso | Opzionali |
|---|---|---|---|
| NTS Business Cube | Per modulo | Core + base | Produzione, CRM aggiuntivi |
| Zucchetti Ad Hoc Revolution | Per modulo | Base incluso | Ogni modulo a parte |
| Zucchetti Ad Hoc Enterprise | Suite completa | Tutto incluso | Customizzazioni a parte |
| SAP Business One | Suite (Starter/Professional) | Tutto incluso | Add-on partner a parte |
| MS Dynamics 365 BC | Per utente/mese (cloud) | Suite completa | AppSource extensions |
| Odoo Enterprise | Per utente/app/mese | Scelti dall'utente | Solo ciò che serve |

### Note KBM
- **Odoo model** (pay-per-app) è il più flessibile per crescita incrementale
- **Dynamics 365 BC model** (per utente) favorisce adoption ampia
- **Suite completa** (SAP, Zucchetti Enterprise) garantisce integrazione ma costo fisso alto
- KBM dovrebbe valutare: modulare con core obbligatorio + opzionali attivabili

[Fonte: siti ufficiali vendor, schede prodotto, brochure pubbliche - Accesso: 2026-06-09]
