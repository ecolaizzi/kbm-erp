# Workflow Magazzino — Analisi ERP Enterprise
> Progetto KBM | Scouting Competitivo | Accesso: 2026-06-09
> Pattern da: Zucchetti Ad Hoc/Mago, SAP Business One, MS Dynamics 365 BC, Odoo Enterprise

---

## OVERVIEW GESTIONE MAGAZZINO

Il modulo magazzino gestisce la movimentazione fisica e contabile delle scorte:

```
ENTRATE                    GIACENZE                    USCITE
─────────────────────      ──────────────────────────  ─────────────────────
Carico da OA           →   Deposito A / Ubicazione X  → Scarico da DDT
Reso clienti           →   Multi-deposito             → Trasferimento depositi
Produzione (output)    →   Multi-lotto                → Produzione (input)
Inventario (aggiust.)  →   Valorizzazione             → Reso fornitori
```

---

## 1. MOVIMENTI DI MAGAZZINO

### 1.1 Tipi di Movimento
| Tipo | Effetto Giacenza | Causale standard |
|---|---|---|
| Carico da acquisto | + | Ricevimento merce |
| Reso da cliente | + | Rientro merce da cliente |
| Carico da produzione | + | Output prodotto finito |
| Rettifica positiva | + | Aggiustamento inventario |
| Scarico per vendita | - | Spedizione merce a cliente |
| Reso a fornitore | - | Restituzione merce |
| Scarico per produzione | - | Consumo materia prima |
| Rettifica negativa | - | Aggiustamento inventario |
| Trasferimento | 0 (spostamento) | Da deposito A a deposito B |

### 1.2 Dati del Movimento
- Data movimento, causale, articolo, quantità, unità misura
- Deposito mittente e/o destinatario
- Lotto / Serial Number (se gestito)
- Valore unitario (per valorizzazione)
- Documento collegato (DDT, OA, OC, documento produzione)
- Operatore, note

---

## 2. GIACENZE

### 2.1 Struttura Giacenze
```
Articolo
  └── Deposito 1
        └── Ubicazione A1 (se WMS avanzato)
              └── Lotto L001
              └── Lotto L002
        └── Ubicazione A2
  └── Deposito 2
        └── ...
```

### 2.2 Tipi di Giacenza
- **Giacenza fisica**: quantità realmente presente
- **Giacenza disponibile**: fisica - impegnata per OC - in transito
- **Giacenza impegnata**: riservata per ordini clienti confermati
- **Giacenza in arrivo**: attesa da OA confermati

### 2.3 Visualizzazione Giacenze (standard ERP)
- Lista per articolo con giacenza per deposito
- Filtri: per articolo, per deposito, per categoria, per fornitore
- Export Excel per analisi
- Alert giacenza minima / punto di riordino

### 2.4 Disponibilità Articolo
Formula standard:
```
Disponibile = Giacenza Fisica - Impegnato per OC - Riserve
Netto = Disponibile + In Arrivo da OA
```

---

## 3. GESTIONE LOTTI

### 3.1 Quando si usa
- Prodotti alimentari (tracciabilità + scadenza)
- Farmaceutici e cosmesi
- Prodotti con garanzia a lotto
- Componenti elettronici con revision

### 3.2 Dati Lotto
- N° lotto (alfanumerico configurabile)
- Data produzione
- Data scadenza
- Fornitore / produttore
- Certificati di conformità
- Note

### 3.3 Tracciabilità Lotto
```
Acquisto (lotto assegnato al carico)
    ↓
Giacenza per lotto nel deposito
    ↓
Scarico (lotto selezionato / FIFO automatico)
    ↓
Vendita (lotto riportato sul DDT/FT)
    ↓
Rintracciabilità: da lotto → a quali clienti è andato
                  da cliente → da quale lotto ha ricevuto
```

### 3.4 Comportamento FIFO per Lotti
- Scarico automatico del lotto più vecchio (FIFO)
- Alert scadenza imminente
- Blocco lotto scaduto (no scarico automatico se scaduto)

---

## 4. SERIAL NUMBER (SN)

### 4.1 Caratteristiche
- Ogni pezzo ha il proprio SN univoco
- Tracciabilità 1:1 (un SN = un pezzo)
- Richiede più gestione rispetto ai lotti

### 4.2 Casi d'uso
- Elettronica di consumo
- Macchinari / attrezzature
- Prodotti con garanzia su seriale

### 4.3 Workflow SN
- Assegnazione SN al carico (manuale o da file importazione)
- Gestione SN in giacenza (un record per SN)
- Selezione SN alla spedizione
- Storico SN: acquistato in data X, venduto a cliente Y in data Z

---

## 5. INVENTARIO FISICO

### 5.1 Tipi di Inventario
- **Inventario generale**: conteggio totale di tutto il magazzino
- **Inventario a rotazione**: conteggio per categorie/zone in periodi distribuiti
- **Inventario spot**: conteggio di un articolo specifico

### 5.2 Procedura Standard (tutti i competitor)

**Fase 1 — Preparazione**
1. Blocco movimenti in entrata/uscita (opzionale)
2. Generazione lista di conta (foglio inventario)
3. Stampa fogli con: articolo, deposito, giacenza teorica (nascosta o visibile)

**Fase 2 — Conta Fisica**
1. Personale conta fisicamente gli articoli
2. Registrazione quantità contate nei fogli (o terminali/barcode)

**Fase 3 — Riconciliazione**
1. Confronto giacenza sistema vs contata
2. Report scarti (differenze positive e negative)
3. Approvazione rettifiche da responsabile magazzino

**Fase 4 — Rettifica**
1. Generazione movimenti di rettifica automatici
2. Aggiornamento giacenze al valore contato
3. Valorizzazione differenze (perdite/guadagni)

### 5.3 Supporto Terminali/Barcode
- Odoo Enterprise: WMS con terminali RF
- Dynamics 365 BC: integrazione Warehouse Management
- **NTS/Zucchetti legacy**: tipicamente foglio Excel + import CSV

---

## 6. VALORIZZAZIONE MAGAZZINO

### 6.1 Metodi di Valorizzazione
| Metodo | Descrizione | Disponibilità |
|---|---|---|
| **FIFO** | Prima entrata = prima uscita; valorizzazione al costo del lotto più vecchio | Tutti |
| **LIFO** | Ultima entrata = prima uscita; vietato IFRS, ammesso in Italia | NTS, Zucchetti, SAP B1 |
| **Costo Medio (WAC)** | Media ponderata di tutti i carichi | Tutti |
| **Costo Standard** | Costo predefinito per l'articolo | SAP B1, BC, Odoo |
| **Costo Specifico** | Costo del singolo lotto/SN | Odoo, BC |

### 6.2 Variazione Costo
- Al cambio del costo di acquisto: ricalcolo automatico o manuale
- Report variazione costo vs. periodo precedente

---

## 7. MULTI-DEPOSITO E TRASFERIMENTI

### 7.1 Struttura Multi-deposito
- Depositi fisicamente distinti (stabilimenti, capannoni)
- Depositi logici (conto deposito presso clienti, conto visione)
- Deposito virtuale per merce in transito

### 7.2 Trasferimento tra Depositi
```
Richiesta trasferimento
    ↓
Documento di trasferimento (pick list)
    ↓
Scarico da Deposito A
    ↓
[Merce in transito - opzionale]
    ↓
Carico in Deposito B
    ↓
Chiusura trasferimento
```

### 7.3 Conto Deposito (presso clienti)
- Merce fisicamente dal cliente ma di proprietà dell'azienda
- Scarico solo al momento della vendita/consumo
- Inventario periodico conto deposito cliente

---

## 8. KPI E REPORT MAGAZZINO

### 8.1 KPI Standard
- **Giacenza totale valorizzata** (per periodo)
- **Rotazione magazzino** (Vendite / Giacenza Media)
- **Giorni di copertura** (Giacenza / Consumo Medio Giornaliero)
- **Valore merce in scadenza** (prossimi 30/60/90 giorni)
- **Articoli sotto giacenza minima** (da riordinare)

### 8.2 Report Operativi
- Storico movimenti per articolo (con filtri data, causale, deposito)
- Giacenza per data specifica (retroattiva)
- Valorizzazione magazzino per data di chiusura periodo
- Report lotti in scadenza
- Articoli con giacenza negativa (errori di sistema)

[Fonte: SAP Business One Help, Odoo Documentation, MS Dynamics 365 BC Docs, Zucchetti brochure - Accesso: 2026-06-09]
