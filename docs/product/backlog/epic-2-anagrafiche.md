# Epic 2 - Anagrafiche Base (US-101 → US-120)

**Modulo**: Anagrafiche  
**Fase**: 2  
**Obiettivo**: Tabelle di decodifica e anagrafiche di sistema comuni a tutti i moduli

---

## US-101: Gestione Nazioni

**Come** amministratore  
**Voglio** gestire l'anagrafica delle nazioni  
**Per** usarle in indirizzi, dati fiscali e configurazioni internazionali

**Priorità**: P0 | **Estimation**: S | **Epic**: Anagrafiche Base | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given admin in Anagrafiche > Nazioni, When visualizza la lista, Then vede tutte le nazioni precaricate (ISO 3166-1 alpha-2) con codice ISO, nome, nazionalità
- [ ] Given nazione presente, When admin la modifica (es. aggiorna nome), Then la modifica è salvata e audit loggata
- [ ] Given admin, When aggiunge nazione personalizzata, Then è disponibile in tutti i dropdown nazione del sistema
- [ ] Given nazione usata in almeno un'anagrafica, When admin tenta eliminazione, Then il sistema blocca con messaggio esplicativo

### Technical Notes
- Permessi: `anagrafiche.nazioni.read`, `anagrafiche.nazioni.edit`
- Tabelle: `Nations` (precaricata con dati ISO)
- Seed data: tutti i paesi ISO 3166-1 (250+)

### Definition of Done
- [ ] Codice implementato | [ ] Seed data | [ ] Unit tests | [ ] Permessi | [ ] Audit log | [ ] Code review

---

## US-102: Gestione Province e Comuni Italiani

**Come** amministratore  
**Voglio** che il sistema abbia l'elenco completo di province e comuni italiani  
**Per** usarli nella compilazione di indirizzi con autocompletamento preciso

**Priorità**: P0 | **Estimation**: S | **Epic**: Anagrafiche Base | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente che compila indirizzo italiano, When seleziona provincia, Then il campo comune si filtra automaticamente mostrando solo comuni di quella provincia
- [ ] Given campo comune, When utente digita, Then appare autocompletamento con suggerimenti in tempo reale
- [ ] Given comune selezionato, When viene compilato CAP, Then viene proposto il CAP principale del comune (autocompletamento)
- [ ] Given dati ISTAT, When installazione iniziale, Then database è precaricato con tutti i comuni italiani aggiornati

### Technical Notes
- Tabelle: `Provinces`, `Municipalities` (seed con dati ISTAT)
- Aggiornamento annuale dati ISTAT (script manuale o automatico)

### Definition of Done
- [ ] Codice implementato | [ ] Seed data ISTAT | [ ] Unit tests | [ ] Code review

---

## US-103: Gestione Valute e Tassi di Cambio

**Come** amministratore  
**Voglio** gestire le valute utilizzate nel sistema e i relativi tassi di cambio  
**Per** supportare operazioni con clienti e fornitori esteri

**Priorità**: P0 | **Estimation**: M | **Epic**: Anagrafiche Base | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given admin in Anagrafiche > Valute, When visualizza lista, Then vede valute abilitate con codice ISO 4217, simbolo, decimali
- [ ] Given admin, When aggiunge tasso di cambio (data, valuta, tasso vs EUR), Then è disponibile per conversioni in quel giorno
- [ ] Given documento in valuta estera, When il sistema calcola importi in EUR, Then usa il tasso del giorno del documento
- [ ] Given valuta EUR, When admin tenta di modificarla o eliminarla, Then il sistema blocca (valuta base non modificabile)

### Technical Notes
- Tabelle: `Currencies`, `ExchangeRates`
- Valuta base: EUR (configurabile in installazioni future)
- Seed: principali valute mondiali (USD, GBP, CHF, JPY, CNY, etc.)

### Definition of Done
- [ ] Codice implementato | [ ] Seed data | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-104: Gestione Unità di Misura

**Come** amministratore  
**Voglio** gestire le unità di misura per articoli e documenti  
**Per** avere coerenza nelle quantità su ordini, DDT e fatture

**Priorità**: P0 | **Estimation**: S | **Epic**: Anagrafiche Base | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given admin in Anagrafiche > Unità di Misura, When visualizza lista, Then vede UM con codice, descrizione, tipo (peso, volume, lunghezza, pezzo, servizio)
- [ ] Given admin, When crea nuova UM, Then è disponibile sugli articoli
- [ ] Given UM con conversione definita, When sistema calcola equivalenza, Then usa il fattore di conversione corretto
- [ ] Given UM usata su articoli, When admin tenta eliminazione, Then il sistema blocca con messaggio

### Technical Notes
- Tabelle: `UnitsOfMeasure`, `UomConversions`
- Seed: UM comuni (PZ, KG, MT, LT, ORA, GG, MQ, MC, etc.)

### Definition of Done
- [ ] Codice implementato | [ ] Seed data | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-105: Gestione Categorie IVA

**Come** amministratore  
**Voglio** gestire i codici e le aliquote IVA  
**Per** applicare correttamente l'IVA su tutti i documenti fiscali italiani

**Priorità**: P0 | **Estimation**: M | **Epic**: Anagrafiche Base | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given admin in Anagrafiche > IVA, When visualizza lista, Then vede aliquote con codice, descrizione, percentuale, tipo (Ordinaria, Ridotta, Esente, Esclusa, FC)
- [ ] Given articolo o servizio su documento, When viene selezionata categoria IVA, Then il sistema calcola automaticamente imponibile e IVA
- [ ] Given aliquote italiane standard (22%, 10%, 5%, 4%, 0%), When sistema è installato, Then sono precaricate con descrizioni normative corrette
- [ ] Given aliquota usata su documenti, When admin tenta eliminazione, Then il sistema blocca

### Technical Notes
- Tabelle: `VatCategories`, `VatRates` (storico aliquote per data)
- Importante: mantenere storico aliquote (modifiche normative nel tempo)
- Seed: tutte le aliquote IVA italiane vigenti

### Definition of Done
- [ ] Codice implementato | [ ] Seed data normativa | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-106: Gestione Condizioni di Pagamento

**Come** amministratore  
**Voglio** definire le condizioni di pagamento standard  
**Per** applicarle automaticamente a clienti, fornitori e documenti

**Priorità**: P0 | **Estimation**: S | **Epic**: Anagrafiche Base | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given admin, When crea condizione pagamento (es. "30 GG DFFM"), Then definisce: giorni, fine mese sì/no, numero rate, tipo calcolo
- [ ] Given condizione pagamento assegnata a un cliente, When si crea fattura, Then le scadenze vengono calcolate automaticamente
- [ ] Given fattura con scadenza calcolata, When admin verifica, Then la data scadenza rispetta esattamente la formula della condizione
- [ ] Given condizioni comuni, When installazione, Then sono precaricate (Contanti, 30gg, 60gg, 30+60gg, etc.)

### Technical Notes
- Tabelle: `PaymentTerms`, `PaymentTermLines`
- Algoritmo: calcolo scadenze complesso (fine mese, giorni fissi, rate multiple)

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests (algoritmo scadenze) | [ ] Seed data | [ ] Code review

---

## US-107: Gestione Banche e Coordinate Bancarie

**Come** amministratore  
**Voglio** gestire l'anagrafica delle banche e le coordinate bancarie aziendali  
**Per** usarle su fatture, pagamenti e distinte di incasso

**Priorità**: P1 | **Estimation**: S | **Epic**: Anagrafiche Base | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given admin in Anagrafiche > Banche, When crea banca, Then inserisce: nome, ABI, CAB, SWIFT/BIC, indirizzo sede
- [ ] Given admin, When aggiunge conto corrente aziendale, Then specifica: banca, IBAN, intestazione, valuta, conto principale sì/no
- [ ] Given IBAN inserito, When sistema lo valida, Then verifica check digit IBAN (algoritmo MOD-97)
- [ ] Given conto principale aziendale, When si emette fattura, Then viene proposto automaticamente nell'IBAN pagamento

### Technical Notes
- Tabelle: `Banks`, `CompanyBankAccounts`, `ContactBankAccounts`
- Validazione IBAN: algoritmo ISO 13616

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests (validazione IBAN) | [ ] Audit log | [ ] Code review

---

## US-108: Gestione Agenti e Zone

**Come** amministratore  
**Voglio** creare l'anagrafica degli agenti di vendita e delle zone commerciali  
**Per** assegnare agenti a clienti e calcolare provvigioni in fase di fatturazione

**Priorità**: P1 | **Estimation**: M | **Epic**: Anagrafiche Base | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given admin, When crea agente, Then inserisce: nome, cognome, codice, P.IVA/CF, zona, percentuale provvigione base
- [ ] Given zona, When viene creata, Then include: codice, descrizione, province/comuni assegnati
- [ ] Given cliente, When viene assegnato agente, Then nelle righe ordine/fattura viene riportato automaticamente l'agente
- [ ] Given agente con clienti assegnati, When admin tenta eliminazione agente, Then il sistema blocca o richiede riassegnazione

### Technical Notes
- Tabelle: `Agents`, `SalesZones`, `AgentZones`

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-109: Gestione Categorie Merceologiche

**Come** amministratore  
**Voglio** definire una struttura gerarchica di categorie merceologiche  
**Per** organizzare il catalogo articoli e filtrare per categoria in documenti e report

**Priorità**: P0 | **Estimation**: S | **Epic**: Anagrafiche Base | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given admin, When crea categoria, Then specifica: codice, descrizione, categoria padre (opzionale), codice IVA default
- [ ] Given struttura gerarchica, When viene visualizzata, Then appare come albero navigabile (es. Informatica > Hardware > Laptop)
- [ ] Given categoria con sotto-categorie o articoli, When admin tenta eliminazione, Then il sistema blocca
- [ ] Given articolo, When viene creato, Then può essere assegnato a una foglia della gerarchia

### Technical Notes
- Tabelle: `ProductCategories` (self-referencing per gerarchia)
- Profondità massima gerarchia: 5 livelli

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-110: Gestione Vettori e Spedizionieri

**Come** amministratore  
**Voglio** gestire l'anagrafica dei vettori e spedizionieri  
**Per** usarli nei DDT per indicare il vettore che effettua la consegna

**Priorità**: P1 | **Estimation**: S | **Epic**: Anagrafiche Base | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given admin, When crea vettore, Then inserisce: ragione sociale, P.IVA, autista default, targa default
- [ ] Given DDT in creazione, When utente seleziona vettore, Then vengono proposti autista e targa preconfigurati (modificabili)
- [ ] Given vettore usato su DDT, When admin tenta eliminazione, Then il sistema blocca

### Technical Notes
- Tabelle: `Carriers`

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-111: Gestione Causali Documento

**Come** amministratore  
**Voglio** definire le causali per i diversi tipi di documenti  
**Per** classificare e gestire correttamente le operazioni commerciali

**Priorità**: P1 | **Estimation**: S | **Epic**: Anagrafiche Base | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given admin in Anagrafiche > Causali, When crea causale, Then specifica: codice, descrizione, tipo documento (DDT, Fattura, Ordine), effetto magazzino (carico/scarico/nessuno), IVA default
- [ ] Given documento in creazione, When utente seleziona causale, Then il comportamento del documento (magazzino, contabile) segue la configurazione della causale
- [ ] Given causali predefinite, When installazione, Then sono precaricate (Vendita, Acquisto, Reso, Conto Lavorazione, etc.)

### Technical Notes
- Tabelle: `DocumentTypes`, `DocumentCausals`

### Definition of Done
- [ ] Codice implementato | [ ] Seed data | [ ] Unit tests | [ ] Code review

---

## US-112: Gestione Magazzini (Anagrafica Base)

**Come** amministratore  
**Voglio** definire i magazzini fisici dell'azienda  
**Per** gestire giacenze e movimenti per ubicazione

**Priorità**: P0 | **Estimation**: S | **Epic**: Anagrafiche Base | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given admin, When crea magazzino, Then inserisce: codice, descrizione, tipo (fisico/virtuale/conto terzi), indirizzo
- [ ] Given magazzino principale, When installazione, Then ne esiste almeno uno di default
- [ ] Given magazzino con movimenti, When admin tenta eliminazione, Then il sistema blocca
- [ ] Given articolo, When viene visualizzato, Then mostra giacenza per magazzino

### Technical Notes
- Tabelle: `Warehouses`

### Definition of Done
- [ ] Codice implementato | [ ] Seed data (magazzino default) | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-113: Ricerca Globale Anagrafiche

**Come** utente  
**Voglio** cercare in tutte le anagrafiche con un'unica ricerca globale  
**Per** trovare rapidamente clienti, fornitori o articoli senza navigare sezione per sezione

**Priorità**: P1 | **Estimation**: M | **Epic**: Anagrafiche Base | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente nella ricerca globale, When digita almeno 2 caratteri, Then vede risultati raggruppati per tipo (Clienti, Fornitori, Articoli) in tempo reale (< 300ms)
- [ ] Given risultato trovato, When utente clicca, Then viene reindirizzato alla scheda del record
- [ ] Given ricerca, When nessun risultato, Then mostra messaggio "Nessun risultato" con suggerimento
- [ ] Given ricerca con filtro tipo, When utente seleziona solo "Clienti", Then mostra solo risultati clienti

### Technical Notes
- Implementazione: full-text search SQL Server o Elasticsearch (configurabile)
- Performance: indici su campi frequentemente cercati (ragione sociale, codice, P.IVA, email)

### Definition of Done
- [ ] Codice implementato | [ ] Performance test | [ ] Unit tests | [ ] Code review

---

## US-114: Export Anagrafiche (Excel/CSV)

**Come** utente  
**Voglio** esportare qualsiasi anagrafica in formato Excel o CSV  
**Per** elaborare dati in fogli di calcolo o importarli in altri sistemi

**Priorità**: P1 | **Estimation**: S | **Epic**: Anagrafiche Base | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente in qualsiasi lista anagrafica, When clicca "Esporta", Then può scegliere formato (Excel .xlsx o CSV)
- [ ] Given export avviato, When i dati sono > 10.000 righe, Then il file viene generato in background e l'utente riceve notifica con link download
- [ ] Given export con filtri attivi, When avviato, Then esporta solo i record filtrati, non tutti
- [ ] Given file esportato, When utente lo apre, Then le intestazioni colonna corrispondono ai nomi campo italiani

### Technical Notes
- Libreria: ClosedXML o EPPlus per Excel
- Background job: Hangfire o simile per export grandi

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Permessi (export.read) | [ ] Code review

---

## US-115: Import Anagrafiche da Excel

**Come** utente responsabile dati  
**Voglio** importare anagrafiche da file Excel  
**Per** migrare dati da sistemi legacy senza inserimento manuale

**Priorità**: P1 | **Estimation**: L | **Epic**: Anagrafiche Base | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente in sezione Import, When carica file Excel con template fornito, Then il sistema valida il file e mostra anteprima dei dati con errori evidenziati
- [ ] Given file con errori (es. P.IVA non valida, campo obbligatorio mancante), When utente vede anteprima, Then vede riga per riga gli errori specifici
- [ ] Given file validato senza errori, When utente conferma import, Then tutti i record vengono importati e viene mostrato riepilogo (N inseriti, N aggiornati, N scartati)
- [ ] Given import completato, When admin controlla audit log, Then vede operazione import con dettaglio file e numero record
- [ ] Given template Excel da scaricare, When utente clicca, Then scarica file con intestazioni corrette e istruzioni nella prima riga

### Technical Notes
- Permessi: `anagrafiche.import`
- Tabelle: tutte le anagrafiche importabili + `ImportJobs`
- Validazione: P.IVA, CF, email, codici obbligatori

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests (validazione, import logic) | [ ] Permessi | [ ] Audit log | [ ] Code review

---

## US-116: Gestione Listini — Struttura Base

**Come** amministratore  
**Voglio** definire la struttura dei listini prezzi  
**Per** avere un sistema flessibile di prezzi applicabili a clienti/fornitori

**Priorità**: P0 | **Estimation**: M | **Epic**: Anagrafiche Base | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given admin, When crea listino, Then specifica: codice, descrizione, tipo (vendita/acquisto), valuta, data validità inizio/fine, metodo calcolo (prezzo fisso, sconto su prezzo base, maggiorazione)
- [ ] Given listino di vendita, When viene assegnato a un cliente, Then è il listino default per quell'utente su tutti i documenti
- [ ] Given più listini validi per stessa data, When sistema calcola prezzo, Then applica priorità listino (configurabile)
- [ ] Given listino scaduto, When viene usato su documento con data fuori validità, Then il sistema avvisa l'utente

### Technical Notes
- Tabelle: `PriceLists`, `PriceListItems`
- Supporto: prezzi fissi, sconti percentuali, prezzi a scaglione quantità

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-117: Gestione Sedi Operative Azienda

**Come** amministratore  
**Voglio** registrare le sedi operative dell'azienda  
**Per** usarle come punto di partenza/arrivo su DDT e documenti di trasporto

**Priorità**: P1 | **Estimation**: S | **Epic**: Anagrafiche Base | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given admin, When crea sede operativa, Then specifica: descrizione, indirizzo completo, tipo (legale/amministrativa/magazzino/spedizione), telefono, email
- [ ] Given DDT in creazione, When si seleziona sede mittente, Then l'indirizzo viene precompilato automaticamente
- [ ] Given azienda con più sedi, When utente crea documento, Then può scegliere da quale sede parte la merce

### Technical Notes
- Tabelle: `CompanyLocations`

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-118: Configurazione Numeratori Documento

**Come** amministratore  
**Voglio** configurare i numeratori automatici per ogni tipo di documento  
**Per** avere numerazione progressiva corretta per fatture, ordini, DDT con formato personalizzabile

**Priorità**: P0 | **Estimation**: M | **Epic**: Anagrafiche Base | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given admin, When configura numeratore, Then specifica: tipo documento, prefisso, suffisso, anno nel numero sì/no, reset annuale sì/no, numero corrente
- [ ] Given numeratore configurato, When viene creato un documento, Then riceve numero automatico (es. FT-2026-0001)
- [ ] Given due utenti che creano documento simultaneamente, When il sistema assegna numero, Then non si generano duplicati (gestione concorrenza)
- [ ] Given reset annuale abilitato, When sistema supera 31 dicembre, Then il contatore riparte da 1 all'1 gennaio

### Technical Notes
- Tabelle: `DocumentCounters`
- Concorrenza: lock ottimistico o sequence SQL Server

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests (concorrenza) | [ ] Audit log | [ ] Code review

---

## US-119: Gestione Cause di Perdita e Stato Lead (Base CRM)

**Come** responsabile vendite  
**Voglio** definire le cause di perdita di offerte e opportunità  
**Per** analizzare perché i clienti non acquistano e migliorare la strategia commerciale

**Priorità**: P2 | **Estimation**: S | **Epic**: Anagrafiche Base | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given admin, When crea causa perdita, Then inserisce: codice, descrizione (es. "Prezzo troppo alto", "Scelto competitor", "Progetto rimandato")
- [ ] Given offerta persa, When utente la chiude come "Persa", Then deve obbligatoriamente selezionare causa perdita
- [ ] Given report perdite, When responsabile vendite lo visualizza, Then vede ranking cause perdita per periodo

### Technical Notes
- Tabelle: `LossReasons`

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Code review

---

## US-120: Dashboard Anagrafiche — KPI e Contatori

**Come** responsabile  
**Voglio** vedere i principali KPI delle anagrafiche sulla dashboard  
**Per** avere una panoramica immediata della qualità e completezza dei dati master

**Priorità**: P2 | **Estimation**: S | **Epic**: Anagrafiche Base | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente con permesso anagrafiche.read, When vede widget dashboard anagrafiche, Then vede: totale clienti attivi, totale fornitori attivi, totale articoli attivi, nuovi inseriti questo mese
- [ ] Given widget, When utente clicca contatore, Then viene reindirizzato alla lista con filtro corrispondente
- [ ] Given anagrafiche con dati incompleti (es. P.IVA mancante), When widget "Completezza dati" è visibile, Then mostra percentuale completezza con link a lista record incompleti

### Technical Notes
- Widget: query aggregate, cache 5 minuti

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Performance test | [ ] Code review
