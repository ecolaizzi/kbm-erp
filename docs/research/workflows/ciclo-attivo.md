# Workflow Ciclo Attivo — Analisi ERP Enterprise
> Progetto KBM | Scouting Competitivo | Accesso: 2026-06-09
> Pattern da: Zucchetti Ad Hoc, SAP Business One, MS Dynamics 365 BC, Odoo Enterprise

---

## OVERVIEW DEL CICLO ATTIVO

Il **Ciclo Attivo** (Order-to-Cash, O2C) comprende tutte le operazioni dalla generazione di un'offerta commerciale fino all'incasso del pagamento dal cliente.

```
[Offerta/Preventivo]
       ↓
[Ordine Cliente Confermato]
       ↓
[DDT / Bolla di Consegna] ←→ [Scarico Magazzino]
       ↓
[Fattura Attiva / FE SDI]
       ↓
[Registrazione Contabile]
       ↓
[Scadenza / Partita Aperta]
       ↓
[Incasso / Chiusura Partita]
```

---

## STEP 1 — OFFERTA / PREVENTIVO

### Dati principali
- **Intestazione**: Cliente, data offerta, validità, agente
- **Righe articoli**: codice, descrizione, quantità, prezzo, sconto, aliquota IVA
- **Totali**: imponibile, IVA, totale documento
- **Note**: condizioni commerciali, termini di consegna

### Comportamento ERP comune
- Numerazione automatica serie preventivi (es. "PV2024/001")
- Stampa PDF o invio email diretta al cliente
- Stato: Bozza → Inviata → Accettata / Rifiutata
- Conversione in Ordine Cliente con un click
- Versioning (revisione offerta con storico)

### Pattern Zucchetti Ad Hoc
- Form con tab: "Testata", "Righe", "Pagamento", "Note"
- Campo "Validità offerta" (data scadenza)
- Button "Genera Ordine" per conversione diretta
[Fonte: Zucchetti Ad Hoc - Manuale utente pubblico - Accesso: 2026-06-09]

### Pattern SAP Business One
- Documento "Offerta di vendita" → conversione in Ordine Clienti
- Alert automatico per offerte scadute
[Fonte: SAP Business One Help Center - https://help.sap.com/docs/SAP_BUSINESS_ONE - Accesso: 2026-06-09]

---

## STEP 2 — ORDINE CLIENTE

### Dati principali
- **Intestazione**: Cliente, data ordine, n° ordine cliente (riferimento esterno), agente
- **Spedizione**: indirizzo consegna, data consegna richiesta, vettore, porto
- **Pagamento**: condizioni, banca, scadenze generate automaticamente
- **Righe**: articoli, quantità ordinata, prezzo, sconti, note riga

### Comportamento ERP comune
- Verifica disponibilità magazzino alla conferma
- Riserva giacenza (stock reservation)
- Generazione impegni magazzino
- Monitoraggio avanzamento: Quantità ordinata / Quantità da evadere / Evasa
- Stampa "Conferma d'ordine" da inviare al cliente

### Conversione da Offerta
- Tutti i dati copiati automaticamente
- Possibilità di modificare prezzi/quantità prima della conferma
- Link bidirezionale tra offerta e ordine generato

### Evasione Ordine (ciclo merce)
- Evasione totale: tutto in un'unica spedizione/DDT
- Evasione parziale: più DDT per un ordine (quantità rimanente da evadere)
- Stato ordine aggiornato automaticamente

---

## STEP 3 — DDT (DOCUMENTO DI TRASPORTO)

### Dati principali
- **Intestazione**: N° DDT, data, cliente destinatario, indirizzo consegna
- **Trasporto**: vettore, numero colli, peso, porto (franco/assegnato), causale trasporto
- **Righe**: articoli, quantità spedita, lotto (se gestito), ubicazione prelievo
- **Firma autista**: campo opzionale

### Tipi di causale trasporto standard
- Vendita
- Conto visione / Conto deposito
- Reso cliente
- Riparazione
- Omaggio / campione
- Trasferimento tra depositi

### Comportamento ERP comune
- Generazione automatica da ordine (picking list → DDT)
- Scarico automatico del magazzino alla conferma DDT
- Numerazione separata dalla fattura
- Stampa per il vettore
- Tracciabilità lotto/SN se attivata

### Normativa italiana
- Il DDT deve accompagnare la merce fisicamente
- Deve riportare: cedente, cessionario, data, descrizione beni, quantità, causale
- Non è un documento IVA ma ha valenza civile
[Fonte: DPR 472/1996 e normativa ADM (Agenzia delle Dogane e Monopoli) - Accesso: 2026-06-09]

---

## STEP 4 — FATTURA ATTIVA

### Tipi di fattura
- **Fattura immediata**: emessa contestualmente al DDT
- **Fattura differita**: emessa a fine mese da più DDT (D.Lgs. 472/1997)
- **Fattura pro-forma**: non fiscale, a fini commerciali
- **Nota credito (NC)**: storno totale o parziale di fattura precedente

### Dati principali
- **Intestazione**: N° fattura, data, cliente, P.IVA / CF cliente
- **Riferimenti**: DDT di riferimento, ordine cliente
- **Righe**: descrizione, quantità, prezzo, sconto, imponibile, aliquota IVA
- **Piede**: imponibile, IVA per aliquota, totale documento
- **Pagamento**: scadenze auto-generate, IBAN banca, metodo (bonifico/RI.BA./assegno)

### Fatturazione Elettronica SDI (obbligatoria in Italia)
- Formato XML FatturaPA (Schema UBL italiano)
- Trasmissione via SDI (Sistema di Interscambio AdE)
- Codici destinatario: Codice Univoco (7 car) o PEC
- Codici natura IVA: N1-N7 per operazioni esenti/escluse
- Firma digitale (opzionale per B2B dal 2023)
- Conservazione sostitutiva obbligatoria 10 anni
[Fonte: Agenzia delle Entrate - https://www.agenziaentrate.gov.it/portale/aree-tematiche/fatturazione-elettronica - Accesso: 2026-06-09]

### Numerazione
- Serie configurabile per anno (es. "FT2024/001")
- Reset automatico al 01/01 di ogni anno
- Numerazione progressiva senza buchi (obbligatoria per legge)

---

## STEP 5 — REGISTRAZIONE CONTABILE

### Scritture generate automaticamente
Alla contabilizzazione della fattura attiva:
```
DARE:  Crediti verso clienti (Conto cliXXXX)    [Totale fattura]
AVERE: Ricavi di vendita (Conto ric.YYY)         [Imponibile]
AVERE: IVA a debito (Conto IVA debito)           [IVA]
```

### Registro IVA
- Registrazione automatica nel Registro IVA Vendite
- Liquidazione IVA mensile/trimestrale

---

## STEP 6 — SCADENZARIO E INCASSO

### Scadenzario
- Generazione automatica scadenze dai termini di pagamento
- Esempio: "30/60/90 gg data fattura" → 3 rate
- Visualizzazione semaforo: Verde (futuro) / Giallo (prossimo) / Rosso (scaduto)

### Metodi di incasso comuni
1. **Bonifico bancario**: importazione estratto conto, riconciliazione
2. **RI.BA. (Ricevuta Bancaria)**: 
   - Generazione file CBI da partite aperte
   - Invio in banca (presentazione)
   - Accredito automatico alla scadenza o SBF (Salvo Buon Fine)
3. **Assegno**: registrazione manuale
4. **Contanti**: fino al limite normativo (€ 999,99)
5. **Carta di credito**: integrazione POS o gateway

### Riconciliazione bancaria
- Import estratto conto (OFX, MT940, CSV banca)
- Abbinamento automatico transazioni / partite aperte
- Gestione differenze, abbuoni, interessi

### Chiusura Partita
- Incasso totale → partita chiusa
- Incasso parziale → partita aggiornata (residuo)
- Nota credito → storno della partita

---

## VARIANTI DEL CICLO ATTIVO

### Acconto / Anticipo
1. Emissione fattura acconto (con IVA)
2. Alla fattura finale: storno dell'acconto
3. Gestione automatica in: SAP B1, Dynamics 365 BC, Odoo

### Ordine da Confermare (workflow approvazione)
- Ordine in stato "In attesa approvazione"
- Email automatica al responsabile
- Approvazione/rifiuto con motivazione
- Disponibile: Dynamics 365 BC, Odoo Enterprise

### Consegna Frazionata
- Ordine → DDT parziale (evasione % quantità)
- Fattura da DDT multipli (differita)
- Monitoraggio residuo evadere per ordine

[Fonte: SAP Business One Documentation, MS Docs Dynamics 365 BC, Odoo Documentation, Zucchetti brochure - Accesso: 2026-06-09]
