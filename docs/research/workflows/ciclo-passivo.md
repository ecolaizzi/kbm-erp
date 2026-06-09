# Workflow Ciclo Passivo — Analisi ERP Enterprise
> Progetto KBM | Scouting Competitivo | Accesso: 2026-06-09
> Pattern da: Zucchetti Ad Hoc, SAP Business One, MS Dynamics 365 BC, Odoo Enterprise

---

## OVERVIEW DEL CICLO PASSIVO

Il **Ciclo Passivo** (Purchase-to-Pay, P2P) comprende tutte le operazioni dall'identificazione del fabbisogno di acquisto fino al pagamento al fornitore.

```
[Richiesta di Acquisto (RDA)]
       ↓
[Offerta Fornitore / Confronto Prezzi]
       ↓
[Ordine di Acquisto (OA)] ←→ [Impegno di spesa]
       ↓
[Carico Merce / DDT Entrata] ←→ [Carico Magazzino]
       ↓
[Ricevimento Fattura Fornitore / FE SDI]
       ↓
[Verifica 3-way match (OA + Carico + Fattura)]
       ↓
[Registrazione Contabile]
       ↓
[Scadenza / Partita Aperta Fornitore]
       ↓
[Pagamento / Chiusura Partita]
```

---

## STEP 1 — RICHIESTA DI ACQUISTO (RDA)

### Dati principali
- **Richiedente**: utente/reparto che fa la richiesta
- **Articoli**: codice, descrizione, quantità richiesta, unità misura
- **Data necessità**: quando serve la merce
- **Fornitore suggerito**: opzionale
- **Note**: motivazione acquisto, centro di costo

### Comportamento ERP
- Disponibile in: SAP B1, Dynamics 365 BC, Odoo Enterprise, Zucchetti Ad Hoc
- Workflow di approvazione (responsabile centro di costo / direttore acquisti)
- Conversione in Ordine di Acquisto automatica o manuale
- **Assente o limitato in**: NTS Business Cube, Zucchetti Mago base

### Processo di approvazione (ERP avanzati)
```
RDA Inserita → Email responsabile → Approvazione/Rifiuto → Genera OA
```
- Soglie di approvazione per importo
- Escalation automatica se non approvato entro X giorni

---

## STEP 2 — OFFERTA E ORDINE DI ACQUISTO

### Confronto Offerte (Request for Quotation - RFQ)
- Invio richiesta offerta a più fornitori
- Raccolta offerte con prezzi/tempi
- Confronto tabellare e selezione migliore offerta
- Disponibile in: SAP B1, Dynamics 365 BC, Odoo
- **Limitato in**: Zucchetti Ad Hoc, NTS

### Ordine di Acquisto (OA)

#### Dati principali
- **Intestazione**: Fornitore, data OA, riferimento fornitore, agente acquisti
- **Condizioni**: termini di pagamento, condizioni di consegna (Incoterms), valuta
- **Righe**: codice articolo, descrizione, quantità, prezzo unitario, sconto, imponibile
- **Riepilogo**: totale imponibile, IVA, totale OA

#### Comportamento ERP comune
- Numerazione automatica (es. "OA2024/001")
- Stampa o invio email al fornitore
- Impegno di spesa nel budget (ERP avanzati)
- Monitoraggio quantità ordinata / ricevuta / fatturata
- Stato: Bozza → Confermato → In consegna → Completato

#### Gestione multi-consegna
- Un OA può generare più carichi (consegne frazionate)
- Tracciamento quantità residua da ricevere per riga

---

## STEP 3 — CARICO MERCE (DDT ENTRATA)

### Tipi di carico
- **Carico da ordine**: collegato a un OA specifico (verifica automatica)
- **Carico senza ordine**: carico libero (emergenze, campioni)
- **Carico conto deposito/visione**: merce non di proprietà
- **Reso a fornitore**: movimento inverso (scarico verso fornitore)

### Dati principali
- **Intestazione**: Fornitore, data carico, N° DDT fornitore, numero colli
- **Righe**: articolo, quantità ricevuta, lotto (se gestito), ubicazione destinazione, note
- **Riferimento OA**: riga OA collegata per verifica

### Verifica conformità (2-way e 3-way match)
- **2-way match**: OA vs Fattura (quantità e prezzi)
- **3-way match**: OA + Carico + Fattura (standard ERP enterprise)
- Alert automatico se quantità carico ≠ quantità OA
- Alert se prezzo fattura ≠ prezzo OA (soglia tolleranza configurabile)

### Effetti automatici
- **Carico magazzino**: incremento giacenza articolo nel deposito
- **Tracciabilità lotti**: assegnazione numero lotto alla giacenza
- Aggiornamento stato OA (quantità ricevuta)

---

## STEP 4 — FATTURA PASSIVA

### Ricezione fattura (FE SDI obbligatoria)
- Le fatture da fornitori B2B italiani arrivano via SDI in formato XML
- Import automatico dal canale SDI (integrazione con AdE)
- Abbinamento automatico a OA / carico esistente
- OCR per fatture cartacee residue (fornitori esteri non FE)

### Dati principali
- **Intestazione**: Fornitore, data fattura fornitore, N° fattura fornitore
- **Righe**: descrizione, quantità, prezzo, sconto, imponibile, aliquota IVA
- **Piede**: imponibile, IVA, totale, scadenze di pagamento

### Verifica 3-way match
```
OA (prezzo/quantità) + Carico (quantità ricevuta) + Fattura (prezzo/quantità fatturata)
```
Se tutto corrisponde → approvazione automatica
Se discrepanza → alert per verifica manuale

### Storno (Nota Debito/Credito)
- Nota credito fornitore: riduce il debito
- Nota debito fornitore: aumenta il debito (raro)
- Ricevute via SDI come altri documenti fiscali

---

## STEP 5 — REGISTRAZIONE CONTABILE

### Scritture automatiche alla contabilizzazione
```
Carico merce:
  DARE:  Conto Rimanenze / Costo merce     [Valore a costo]
  AVERE: Fornitori conto carico (transitorio) [Valore a costo]

Fattura fornitore:
  DARE:  Fornitori conto carico (transitorio) [Imponibile]
  DARE:  IVA a credito                         [IVA]
  AVERE: Fornitori (conto fornitore)           [Totale fattura]
```

### Registro IVA acquisti
- Registrazione automatica nel Registro IVA Acquisti
- Detraibilità IVA per acquisti inerenti all'attività
- Reverse charge (inversione contabile) per alcuni servizi

---

## STEP 6 — PAGAMENTO FORNITORE

### Scadenzario passivo
- Scadenze generate automaticamente dai termini di pagamento
- Visualizzazione per fornitore o aggregata
- Filtri: per scadenza, per fornitore, per importo, per banca

### Metodi di pagamento comuni
1. **Bonifico bancario SEPA**:
   - Selezione partite da pagare
   - Generazione file SEPA/CBI per home banking
   - Invio batch via internet banking
   
2. **RI.BA. passiva** (meno comune, quando il fornitore preleva):
   - Gestione autorizzazione addebito
   
3. **Assegno / Contanti**: registrazione manuale

4. **Carta di credito aziendale**: riconciliazione con estratto conto

### Gestione pagamenti (flow standard)
```
Selezione partite → Proposta pagamento → Validazione → Generazione file CBI → 
Invio banca → Conferma pagamento → Chiusura partite
```

### Effetti contabili del pagamento
```
DARE:  Fornitori (conto fornitore)     [Importo pagato]
AVERE: Banca c/c (o cassa)            [Importo pagato]
```

---

## VARIANTI DEL CICLO PASSIVO

### Reso a Fornitore
1. Scarico magazzino con causale "Reso fornitore"
2. Generazione DDT di reso
3. Fornitore emette nota credito
4. Storno contabile della fattura originale

### Acquisti Intracomunitari (UE)
- Integrazione Intrastat automatica
- Reverse charge IVA (auto-fatturazione)
- Dichiarazione Intrastat mensile/trimestrale

### Acquisti Extra-UE (importazioni)
- Gestione bolletta doganale (MRN)
- IVA all'importazione
- Dazi doganali come costo

### Servizi (no merce)
- Nessun movimento magazzino
- Solo registrazione fattura passiva
- Eventuale competenza temporale (ratei/risconti)

[Fonte: SAP Business One Documentation, MS Docs Dynamics 365 BC, Odoo Documentation, Normativa italiana - Accesso: 2026-06-09]
