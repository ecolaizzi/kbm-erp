# Workflow Contabilità — Analisi ERP Enterprise
> Progetto KBM | Scouting Competitivo | Accesso: 2026-06-09
> Pattern da: Zucchetti Ad Hoc, SAP Business One, MS Dynamics 365 BC, Odoo Enterprise

---

## OVERVIEW MODULO CONTABILITÀ

Il modulo contabilità gestisce tutte le scritture contabili, l'IVA, le partite aperte e la produzione dei report fiscali e gestionali obbligatori.

```
DOCUMENTI OPERATIVI            CONTABILITÀ GENERALE           OUTPUT
────────────────────           ─────────────────────          ──────────
Fatture attive (SDI)      →    Prima nota / Journal            Bilancio CEE
Fatture passive (SDI)     →    Partita doppia                  Conto economico
DDT, OA, OC               →    Piano dei conti                 Stato patrimoniale
Prima nota manuale        →    Registri IVA                   Dichiarazione IVA
Banca / Cassa             →    Scadenzario                    Intrastat
                          →    Libro mastro                   Esterometro
                          →    Partite aperte
```

---

## 1. PIANO DEI CONTI

### 1.1 Struttura Standard Italiano
```
1 - STATO PATRIMONIALE ATTIVO
    10 - Immobilizzazioni materiali
    11 - Immobilizzazioni immateriali
    12 - Rimanenze
    13 - Crediti verso clienti
    14 - Crediti diversi
    15 - Disponibilità liquide
    16 - Ratei e risconti attivi

2 - STATO PATRIMONIALE PASSIVO
    20 - Patrimonio netto
    21 - Fondi rischi e oneri
    22 - TFR
    23 - Debiti verso fornitori
    24 - Debiti diversi
    25 - Ratei e risconti passivi

3 - CONTO ECONOMICO
    30 - Ricavi di vendita
    31 - Variazione rimanenze
    32 - Costi per materie prime
    33 - Costi per servizi
    34 - Costo del lavoro
    35 - Ammortamenti
    36 - Oneri finanziari
    37 - Imposte
```

### 1.2 Gestione Piano dei Conti
- Struttura gerarchica: Mastro → Sottoconto
- Conti automatici per clienti/fornitori (un conto per anagrafica)
- Numerazione configurabile
- Import piano dei conti standard (OIC, XBRL)

---

## 2. PRIMA NOTA E REGISTRAZIONI CONTABILI

### 2.1 Tipi di Registrazione
- **Automatica**: generata da documenti (fatture, pagamenti, chiusure)
- **Manuale**: inserita direttamente dal contabile

### 2.2 Struttura Registrazione
```
Testata:
  - Numero registrazione (progressivo automatico)
  - Data registrazione
  - Causale contabile (tipo di operazione)
  - Descrizione libera
  - Protocollo IVA (se fiscale)

Righe:
  - Conto contabile
  - Dare / Avere
  - Importo
  - Centro di costo (se contabilità analitica)
  - Descrizione riga
  - Scadenza (per partite aperte)
```

### 2.3 Causali Contabili Comuni
| Causale | Descrizione | Uso |
|---|---|---|
| FTV | Fattura vendita | Ciclo attivo |
| FAC | Fattura acquisto | Ciclo passivo |
| NCR | Nota credito ricevuta | Rettifica da fornitore |
| NCE | Nota credito emessa | Rettifica a cliente |
| PAG | Pagamento a fornitore | Ciclo passivo |
| INC | Incasso da cliente | Ciclo attivo |
| GCF | Giroconto tra c/c | Banca |
| PRN | Prima nota generica | Scrittura manuale |
| AMM | Ammortamento | Fine periodo |
| ACQ | Acquisto cespite | Immobilizzazioni |

### 2.4 Chiusure di Periodo
- **Mensile**: liquidazione IVA, report di chiusura
- **Trimestrale**: liquidazione IVA per contribuenti trimestrali
- **Annuale**: chiusura esercizio, apertura nuovo esercizio

### 2.5 Riapertura/Chiusura Esercizio
```
Fine esercizio (31/12):
1. Registrazioni di assestamento (ratei, risconti, ammortamenti)
2. Chiusura conti economici → Utile/Perdita
3. Scrittura di chiusura patrimoniale
4. Stampa bilancio finale

Apertura nuovo esercizio (01/01):
1. Riporto saldi patrimoniali
2. Riapertura partite aperte
3. Nuovo esercizio attivo
```

---

## 3. GESTIONE IVA

### 3.1 Registri IVA Obbligatori
- **Registro IVA Vendite**: tutte le fatture emesse con IVA
- **Registro IVA Acquisti**: tutte le fatture ricevute con IVA detratta
- **Registro corrispettivi**: per vendite al dettaglio (se applicabile)

### 3.2 Aliquote IVA Italiane (2024)
| Aliquota | Applicazione |
|---|---|
| 22% | Standard (maggioranza beni/servizi) |
| 10% | Agevolata (alimentari, turismo, costruzioni) |
| 5% | Ridotta (specifici beni sociali) |
| 4% | Super-ridotta (beni di prima necessità, editoria) |
| 0% | Esenti / Fuori campo IVA |

### 3.3 Nature IVA Speciali
| Natura | Descrizione |
|---|---|
| N1 | Escluse ex art. 15 DPR 633/72 |
| N2 | Non soggette |
| N3 | Non imponibili (N3.1 - N3.6 sottocategorie) |
| N4 | Esenti |
| N5 | Regime del margine |
| N6 | Inversione contabile (reverse charge) |
| N7 | IVA assolta in altro stato UE |

### 3.4 Liquidazione IVA
```
Periodicità: mensile (default) o trimestrale (per regimi agevolati)

Calcolo:
  IVA a debito (vendite)    100.000
  IVA a credito (acquisti)  (70.000)
  IVA da versare = 30.000

  o credito IVA riportato al periodo successivo
```

### 3.5 Adempimenti Fiscali Supportati

| Adempimento | Frequenza | ERP che lo supportano |
|---|---|---|
| Liquidazione IVA | Mensile/Trimestrale | Tutti |
| Dichiarazione IVA annuale (Modello IVA) | Annuale | NTS, Zucchetti, SAP B1 (IT) |
| Intrastat acquisti | Mensile/Trimestrale | Tutti (IT) |
| Intrastat vendite | Mensile/Trimestrale | Tutti (IT) |
| Esterometro (operaz. estero pre-2022) | Trimestrale | NTS, Zucchetti |
| Comunicazione IVA transfrontaliera | Trimestrale | Tutti via SDI |
| Certificazione ritenute (CU) | Annuale | NTS, Zucchetti (modulo) |

---

## 4. SCADENZARIO E PARTITE APERTE

### 4.1 Struttura Scadenzario
- **Partita aperta**: credito o debito non ancora incassato/pagato
- Ogni fattura genera una o più partite aperte (secondo termini pagamento)
- Abbinamento incasso/pagamento → chiusura partita (totale o parziale)

### 4.2 Aging Report (Partite Aperte per Fascia di Anzianità)
```
Fascia     | Importo    | % sul totale
-----------|------------|-------------
Non scaduto| 50.000     | 45%
0-30 gg    | 20.000     | 18%
31-60 gg   | 15.000     | 13%
61-90 gg   | 10.000     | 9%
> 90 gg    | 16.000     | 15%
-----------|------------|-------------
TOTALE     | 111.000    | 100%
```

### 4.3 Solleciti e Gestione Crediti
- Stampa lettere di sollecito per clienti in ritardo
- Template sollecito 1° / 2° / 3° avviso
- Blocco automatico ordini per clienti oltre fido o con scaduti
- Invio email automatico (ERP avanzati: Odoo, BC)

### 4.4 Abbuoni e Arrotondamenti
- Differenze di piccolo importo alla chiusura della partita
- Registrazione automatica su conto abbuoni attivi/passivi

---

## 5. RICONCILIAZIONE BANCARIA

### 5.1 Flusso Riconciliazione
```
Import estratto conto banca (OFX, MT940, CSV)
    ↓
Confronto movimenti banca vs. registrazioni contabili
    ↓
Abbinamento automatico (per importo, data, riferimento)
    ↓
Gestione movimenti non abbinati (ricerca manuale o registrazione)
    ↓
Validazione riconciliazione
    ↓
Saldo banca = Saldo contabile
```

### 5.2 Formati import estratto conto
- **OFX**: Open Financial Exchange (standard internazionale)
- **MT940**: SWIFT standard per estratti conto
- **CSV banca**: formato proprietario della banca
- **API bancaria**: Open Banking (disponibile in Odoo, BC)

---

## 6. CONTABILITÀ ANALITICA

### 6.1 Struttura Analitica
- **Centri di Costo (CDC)**: unità organizzative per imputazione costi
- **Centri di Profitto**: per analisi margini
- **Commesse**: per progetto/contratto

### 6.2 Imputazione Analitica
- Ogni riga di prima nota può avere un CDC / commessa
- Report per CDC: costi, ricavi, margine
- Budget analitico vs consuntivo

### 6.3 Ripartizione Costi Generali
- Costi comuni ripartiti su CDC con chiavi di riparto
- Esempio: affitto ripartito per mq, stipendi per ore

---

## 7. REPORT CONTABILI PRINCIPALI

| Report | Descrizione | Frequenza |
|---|---|---|
| Bilancio di verifica | Tutti i conti con saldi Dare/Avere | Mensile |
| Stato patrimoniale | Attivo/Passivo al momento | Mensile/Annuale |
| Conto economico | Ricavi e costi del periodo | Mensile/Annuale |
| Libro mastro | Movimenti dettagliati per conto | On-demand |
| Estratto conto cliente | Fatture, pagamenti, saldo | On-demand |
| Estratto conto fornitore | Fatture, pagamenti, saldo | On-demand |
| Aging receivables | Partite aperte clienti per fascia | Settimanale |
| Aging payables | Partite aperte fornitori per fascia | Settimanale |
| Cash flow previsionale | Scadenze future IN/OUT | Settimanale |
| Registro IVA acquisti | Dettaglio IVA detratta | Mensile |
| Registro IVA vendite | Dettaglio IVA addebitata | Mensile |

[Fonte: OIC - Principi Contabili Italiani, Agenzia delle Entrate, SAP Business One Help, Odoo Accounting Docs, MS Dynamics 365 BC Docs - Accesso: 2026-06-09]
