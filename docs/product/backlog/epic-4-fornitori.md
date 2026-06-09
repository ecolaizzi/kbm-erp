# Epic 4 - Fornitori (US-141 → US-160)

**Modulo**: Anagrafiche  
**Fase**: 2  
**Obiettivo**: Anagrafica completa fornitori con dati fiscali, bancari, condizioni di acquisto

---

## US-141: CRUD Fornitore — Dati Anagrafici Base

**Come** utente ufficio acquisti  
**Voglio** creare, visualizzare, modificare e archiviare fornitori  
**Per** gestire il registro completo dei fornitori dell'azienda

**Priorità**: P0 | **Estimation**: M | **Epic**: Fornitori | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente con permesso fornitori.create, When crea nuovo fornitore, Then inserisce: tipo (persona fisica/giuridica), ragione sociale, codice fornitore (auto o manuale), stato (attivo/inattivo), categoria fornitore
- [ ] Given fornitore creato, When visualizzato, Then scheda con tab: Dati generali, Contatti, Indirizzi, Dati bancari, Condizioni acquisto, Documenti
- [ ] Given fornitore con documenti (ordini, fatture), When utente tenta eliminazione, Then sistema blocca e propone archiviazione
- [ ] Given lista fornitori, When filtrata per stato, Then mostra solo fornitori corrispondenti con paginazione < 500ms

### Technical Notes
- Permessi: `anagrafiche.fornitori.read`, `anagrafiche.fornitori.create`, `anagrafiche.fornitori.edit`, `anagrafiche.fornitori.delete`
- Tabelle: `Suppliers`, `AuditLog`
- Codice: progressivo con prefisso configurabile (es. FOR-0001)

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Permessi | [ ] Audit log | [ ] Code review

---

## US-142: Fornitore — Dati Fiscali e Regime IVA

**Come** utente amministrativo  
**Voglio** inserire e validare i dati fiscali del fornitore  
**Per** garantire correttezza di registrazioni contabili e comunicazioni fiscali

**Priorità**: P0 | **Estimation**: M | **Epic**: Fornitori | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given fornitore italiano, When utente inserisce P.IVA, Then il sistema la valida con algoritmo check digit
- [ ] Given fornitore in regime forfettario o esente IVA, When impostato, Then il sistema applica correttamente sul documento (no IVA detraibile)
- [ ] Given fornitore estero UE, When inserito, Then VAT number EU viene validato tramite VIES (se disponibile)
- [ ] Given fornitore extra-UE, When inserito, Then gestione doganale e codici UN/LOCODE disponibili
- [ ] Given fornitore con SDI/PEC, When inseriti, Then vengono validati formato

### Technical Notes
- Tabelle: `Suppliers` (VatNumber, FiscalCode, VatRegime, SdiCode, PecEmail)
- Regime IVA: Ordinario, Forfettario, Esente, Agricoltore, PA

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests (validazioni) | [ ] Audit log | [ ] Code review

---

## US-143: Fornitore — Dati Bancari e Modalità Pagamento

**Come** utente ufficio acquisti  
**Voglio** gestire i dati bancari e le modalità di pagamento del fornitore  
**Per** effettuare pagamenti corretti e generare distinte bonifici

**Priorità**: P0 | **Estimation**: M | **Epic**: Fornitori | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given fornitore, When utente aggiunge conto bancario, Then inserisce: IBAN (validato), intestatario, banca, BIC/SWIFT, note, flag principale
- [ ] Given fornitore con più IBAN, When viene creata distinta pagamento, Then propone l'IBAN principale (modificabile)
- [ ] Given IBAN inserito, When sistema lo valida, Then verifica check digit MOD-97 e mostra banca estratta dal codice ABI
- [ ] Given fornitore, When utente configura modalità pagamento preferita (Bonifico, RID, Assegno, etc.), Then viene usata come default su ordini acquisto

### Technical Notes
- Tabelle: `SupplierBankAccounts`, `PaymentMethods`
- Validazione IBAN: algoritmo ISO 13616, decodifica ABI/CAB

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests (IBAN validation) | [ ] Audit log | [ ] Code review

---

## US-144: Fornitore — Contatti Multipli

**Come** utente ufficio acquisti  
**Voglio** gestire più contatti per ogni fornitore  
**Per** avere riferimenti distinti per: commerciale, amministrazione, logistica, tecnico

**Priorità**: P0 | **Estimation**: S | **Epic**: Fornitori | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given fornitore, When utente aggiunge contatto, Then inserisce: nome, cognome, ruolo, email, telefono, cellulare, note
- [ ] Given contatto per area (es. Ufficio Acquisti), When flaggato come principale per area, Then appare in evidenza e viene usato nelle comunicazioni automatiche per quell'area
- [ ] Given lista contatti fornitore, When visualizzata, Then è ordinabile per ruolo e nome

### Technical Notes
- Tabelle: `SupplierContacts`

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-145: Fornitore — Indirizzi Multipli

**Come** utente ufficio acquisti  
**Voglio** gestire più indirizzi per ogni fornitore  
**Per** avere sede legale, magazzino fornitori e indirizzo spedizione resi separati

**Priorità**: P0 | **Estimation**: S | **Epic**: Fornitori | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given fornitore, When utente aggiunge indirizzo, Then specifica: tipo (Legale/Operativa/Spedizioni/Magazzino), dati indirizzo completo
- [ ] Given ordine di acquisto, When creato per fornitore, Then indirizzo di consegna viene precompilato con indirizzo principale dell'azienda (non del fornitore)
- [ ] Given reso a fornitore, When si crea documento reso, Then propone indirizzo "Spedizioni" del fornitore come destinazione

### Technical Notes
- Tabelle: `SupplierAddresses`

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-146: Fornitore — Condizioni di Acquisto

**Come** responsabile acquisti  
**Voglio** configurare le condizioni di acquisto per ogni fornitore  
**Per** avere sconti e condizioni automatiche applicati sugli ordini di acquisto

**Priorità**: P0 | **Estimation**: M | **Epic**: Fornitori | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given fornitore, When utente configura condizioni acquisto, Then specifica: listino acquisto, sconto %, condizioni pagamento, valuta, tempi di consegna (gg), quantità minima ordine
- [ ] Given ordine di acquisto per fornitore, When creato, Then condizioni vengono applicate automaticamente (modificabili sul documento)
- [ ] Given tempi di consegna configurati, When si crea ordine, Then la data prevista arrivo viene calcolata automaticamente
- [ ] Given quantità minima, When riga ordine è sotto minimo, Then sistema avvisa (warning, non blocco)

### Technical Notes
- Tabelle: `SupplierPurchaseConditions`
- Lead time: calcolo date tenendo conto di giorni lavorativi (configurabile)

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-147: Fornitore — Articoli Fornitori (Catalogo)

**Come** responsabile acquisti  
**Voglio** associare articoli interni al catalogo del fornitore con i suoi codici e prezzi  
**Per** usare il codice articolo del fornitore negli ordini e mantenere storico prezzi acquisto

**Priorità**: P1 | **Estimation**: M | **Epic**: Fornitori | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given fornitore, tab "Articoli Fornitori", When utente aggiunge associazione, Then specifica: articolo interno, codice fornitore, descrizione fornitore, prezzo acquisto, UM fornitore, fattore conversione
- [ ] Given ordine di acquisto, When utente inserisce articolo fornitore per codice fornitore, Then il sistema lo riconosce e compila automaticamente articolo interno e prezzo
- [ ] Given storico prezzi acquisto, When visualizzato per articolo/fornitore, Then mostra andamento prezzi nel tempo
- [ ] Given fornitore preferito per articolo, When flaggato, Then viene proposto come primo fornitore in fase di acquisto

### Technical Notes
- Tabelle: `SupplierArticles`, `SupplierArticlePriceHistory`

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-148: Fornitore — Documenti Allegati

**Come** utente ufficio acquisti  
**Voglio** allegare documenti a un fornitore  
**Per** conservare contratti quadro, certificazioni qualità, visure camerali nella scheda fornitore

**Priorità**: P1 | **Estimation**: S | **Epic**: Fornitori | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente in scheda fornitore, When allega documento, Then specifica: file, tipo (Contratto/Certificazione/Visura/Altro), scadenza (opzionale), note
- [ ] Given documento con scadenza, When si avvicina la data (< 30 giorni), Then il sistema invia notifica in-app a utenti con permesso fornitori
- [ ] Given allegato riservato, When utente senza permesso view-riservati lo cerca, Then non lo vede

### Technical Notes
- Tabelle: `SupplierAttachments`
- Alert scadenza: job schedulato giornaliero

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Permessi | [ ] Audit log | [ ] Code review

---

## US-149: Fornitore — Valutazione e Rating

**Come** responsabile qualità / acquisti  
**Voglio** valutare i fornitori su qualità, puntualità e servizio  
**Per** supportare decisioni di selezione fornitore basate su performance storica

**Priorità**: P2 | **Estimation**: M | **Epic**: Fornitori | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given responsabile acquisti, When inserisce valutazione fornitore, Then specifica: data, periodo di riferimento, punteggio per dimensione (qualità 1-5, puntualità 1-5, supporto 1-5, prezzo 1-5), note
- [ ] Given valutazioni inserite, When visualizzate nella scheda fornitore, Then mostra media per dimensione e trend nel tempo (grafico)
- [ ] Given valutazioni, When responsabile qualità apre report fornitori, Then vede ranking fornitori per punteggio complessivo
- [ ] Given fornitore sotto soglia minima, When configurata soglia alert, Then il sistema notifica responsabile qualità

### Technical Notes
- Tabelle: `SupplierEvaluations`

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Permessi | [ ] Code review

---

## US-150: Fornitore — Ricerca e Filtri Avanzati

**Come** utente ufficio acquisti  
**Voglio** cercare e filtrare fornitori con criteri multipli  
**Per** trovare rapidamente il fornitore giusto

**Priorità**: P0 | **Estimation**: S | **Epic**: Fornitori | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente in lista fornitori, When usa ricerca rapida, Then cerca su: ragione sociale, codice, P.IVA, email (debounce 300ms)
- [ ] Given filtri avanzati, When utente applica, Then può filtrare per: stato, nazione, categoria, condizioni pagamento, agente acquisti, fornitore preferito
- [ ] Given filtri attivi, When utente salva come "Ricerca salvata", Then può richiamarla in futuro
- [ ] Given lista con 5K+ fornitori, When si naviga, Then paginazione < 500ms

### Technical Notes
- Full-text search su campi principali con indici

### Definition of Done
- [ ] Codice implementato | [ ] Performance test | [ ] Unit tests | [ ] Code review

---

## US-151: Fornitore — Note e Alert

**Come** utente ufficio acquisti  
**Voglio** aggiungere note e alert al fornitore  
**Per** condividere informazioni importanti con i colleghi (es. "chiamare solo di mattina", "fornitore critico")

**Priorità**: P1 | **Estimation**: S | **Epic**: Fornitori | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente, When aggiunge nota al fornitore, Then specifica: testo, tipo (interna/alert/promemoria), visibilità (tutti/solo acquisti/solo admin)
- [ ] Given nota tipo "Alert", When chiunque apre scheda fornitore, Then vede banner in evidenza
- [ ] Given promemoria con data, When la data si avvicina, Then utente che l'ha creato riceve notifica in-app

### Technical Notes
- Tabelle: `SupplierNotes`

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Permessi | [ ] Audit log | [ ] Code review

---

## US-152: Fornitore — Gestione Stato e Blocco

**Come** responsabile acquisti  
**Voglio** gestire lo stato del fornitore (attivo/inattivo/bloccato)  
**Per** impedire acquisti da fornitori non qualificati o con cui ci sono problemi

**Priorità**: P1 | **Estimation**: S | **Epic**: Fornitori | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given responsabile, When blocca fornitore, Then deve specificare motivazione e data blocco
- [ ] Given fornitore bloccato, When utente tenta di creare ordine acquisto, Then sistema blocca con motivo del blocco visibile
- [ ] Given fornitore inattivo, When utente tenta di usarlo, Then avvisa con "Fornitore non attivo" e chiede conferma
- [ ] Given lista fornitori default, When non filtrata, Then mostra solo fornitori "Attivi"

### Technical Notes
- Tabelle: `Suppliers.Status`, `SupplierBlockHistory`
- Permessi: `anagrafiche.fornitori.block`

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Permessi | [ ] Audit log | [ ] Code review

---

## US-153: Fornitore — Import da Excel

**Come** responsabile dati  
**Voglio** importare fornitori da file Excel  
**Per** migrare il parco fornitori da sistema legacy

**Priorità**: P1 | **Estimation**: M | **Epic**: Fornitori | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given template Excel fornitori, When utente lo scarica, Then include campi: codice, ragione sociale, P.IVA, CF, indirizzo, IBAN, email, telefono, condizioni pagamento
- [ ] Given file caricato, When sistema valida, Then riporta errori per campo con messaggio specifico
- [ ] Given import avviato, When completato, Then mostra riepilogo: N inseriti, N aggiornati, N scartati con dettaglio errori

### Technical Notes
- Upsert su codice fornitore

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Permessi | [ ] Audit log | [ ] Code review

---

## US-154: Fornitore — Esposizione e Scadenzario (Struttura)

**Come** responsabile acquisti  
**Voglio** vedere le scadenze dei pagamenti verso i fornitori  
**Per** pianificare la liquidità e non perdere scadenze di pagamento

**Priorità**: P1 | **Estimation**: S | **Epic**: Fornitori | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente in scheda fornitore, tab "Pagamenti", When apre, Then vede struttura placeholder con messaggio "Disponibile da Fase 3 (Ciclo Passivo)"
- [ ] Given struttura dati, When Fase 3 implementata, Then il tab mostrerà: fatture in scadenza, pagamenti eseguiti, saldo attuale

### Technical Notes
- Struttura base in Fase 2, dati reali in Fase 3

### Definition of Done
- [ ] Struttura UI base implementata | [ ] Unit tests | [ ] Code review

---

## US-155: Fornitore — Categorie Merceologiche Fornitura

**Come** responsabile acquisti  
**Voglio** classificare i fornitori per le categorie merceologiche che forniscono  
**Per** trovare rapidamente i fornitori di una specifica categoria in fase di richiesta acquisto

**Priorità**: P1 | **Estimation**: S | **Epic**: Fornitori | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given fornitore, When utente assegna categorie merceologiche, Then specifica una o più categorie dal catalogo categorie
- [ ] Given RDA o ordine acquisto per articolo di categoria X, When il sistema suggerisce fornitori, Then propone quelli abilitati per quella categoria
- [ ] Given report fornitori per categoria, When estratto, Then mostra: categoria, N. fornitori, fornitore preferito

### Technical Notes
- Tabelle: `SupplierCategories` (join table fornitori-categorie merceologiche)

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Code review

---

## US-156: Fornitore — Qualifica Fornitori

**Come** responsabile qualità  
**Voglio** gestire il processo di qualifica dei nuovi fornitori  
**Per** assicurarmi che solo fornitori approvati vengano usati negli acquisti

**Priorità**: P2 | **Estimation**: M | **Epic**: Fornitori | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given nuovo fornitore creato, When stato = "In Qualifica", Then non può essere usato su ordini di acquisto confermati
- [ ] Given fornitore in qualifica, When responsabile qualità approva, Then stato cambia a "Qualificato" e può ricevere ordini
- [ ] Given qualifica, When rifiutata, Then motivo obbligatorio e fornitore rimane in stato "Non Qualificato"
- [ ] Given qualifica approvata, When in audit log, Then riporta: chi ha qualificato, data, eventuali condizioni

### Technical Notes
- Tabelle: `Suppliers.QualificationStatus`, `SupplierQualificationHistory`
- Workflow base: No workflow engine necessario in Fase 2

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-157: Fornitore — Duplicazione Rapida

**Come** utente che inserisce fornitori simili  
**Voglio** duplicare un fornitore esistente  
**Per** velocizzare inserimento di fornitori dello stesso gruppo o simili

**Priorità**: P2 | **Estimation**: S | **Epic**: Fornitori | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente in scheda fornitore, When clicca "Duplica", Then crea nuovo fornitore con stessi dati (escluso codice, P.IVA, CF, IBAN)
- [ ] Given duplicazione, When completata, Then apertura immediata per modifica e audit log registra "Duplicato da FOR-XXXX"

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-158: Fornitore — Prezzi Acquisto Speciali per Articolo

**Come** responsabile acquisti  
**Voglio** definire prezzi di acquisto speciali per specifici articoli con un fornitore  
**Per** registrare accordi commerciali negoziati manualmente

**Priorità**: P1 | **Estimation**: S | **Epic**: Fornitori | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente in scheda fornitore, tab "Prezzi speciali", When aggiunge prezzo speciale, Then specifica: articolo, prezzo, sconto, quantità minima, validità
- [ ] Given ordine acquisto con articolo per fornitore, When prezzo speciale è valido, Then viene applicato automaticamente
- [ ] Given confronto prezzi, When utente cerca "miglior prezzo per articolo X", Then sistema mostra prezzi di tutti i fornitori per quell'articolo

### Technical Notes
- Tabelle: `SupplierSpecialPrices`

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-159: Fornitore — Comunicazioni e Email (Struttura)

**Come** utente ufficio acquisti  
**Voglio** inviare comunicazioni email ai fornitori direttamente dal sistema  
**Per** avere traccia di tutte le comunicazioni nello stesso posto degli ordini

**Priorità**: P2 | **Estimation**: M | **Epic**: Fornitori | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente in scheda fornitore, When clicca "Invia email", Then si apre editor email con: destinatario precompilato, template selezionabile, allegati
- [ ] Given email inviata, When visualizzata nella timeline fornitore, Then appare con oggetto, data, destinatario, stato (inviata/errore)
- [ ] Given SMTP non configurato, When utente tenta invio, Then messaggio chiaro "Configurare SMTP in impostazioni sistema"

### Technical Notes
- Configurazione SMTP in SystemSettings (US-010)

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-160: Fornitore — Report Fornitore Completo

**Come** responsabile acquisti  
**Voglio** estrarre un report completo di un fornitore  
**Per** avere una scheda riassuntiva da presentare in meeting di revisione fornitori

**Priorità**: P2 | **Estimation**: S | **Epic**: Fornitori | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente in scheda fornitore, When clicca "Stampa Report", Then genera PDF con: dati anagrafici, contatti principali, condizioni acquisto, valutazione media, ultimi N ordini (placeholder se non Fase 3)
- [ ] Given report generato, When utente lo vede, Then è formattato con logo azienda e data generazione
- [ ] Given report, When utente vuole solo alcuni tab, Then può selezionare sezioni da includere

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Code review
