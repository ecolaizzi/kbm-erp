# Epic 3 - Clienti (US-121 → US-140)

**Modulo**: Anagrafiche  
**Fase**: 2  
**Obiettivo**: Anagrafica completa clienti con dati fiscali, contatti, indirizzi, condizioni commerciali

---

## US-121: CRUD Cliente — Dati Anagrafici Base

**Come** utente commerciale  
**Voglio** creare, visualizzare, modificare e archiviare clienti  
**Per** gestire il registro completo dei clienti dell'azienda

**Priorità**: P0 | **Estimation**: M | **Epic**: Clienti | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente con permesso clienti.create, When crea nuovo cliente, Then inserisce: tipo (persona fisica/giuridica), ragione sociale/nome cognome, codice cliente (auto o manuale), stato (attivo/inattivo)
- [ ] Given cliente creato, When utente lo visualizza, Then vede scheda completa con tab: Dati generali, Contatti, Indirizzi, Condizioni commerciali, Documenti
- [ ] Given cliente, When utente lo modifica, Then le modifiche vengono salvate con audit trail completo
- [ ] Given cliente con documenti (fatture, ordini), When utente tenta eliminazione definitiva, Then il sistema blocca e propone archiviazione
- [ ] Given lista clienti, When utente filtra per stato attivo/inattivo, Then vede solo i clienti corrispondenti
- [ ] Given lista clienti, When utente ordina per ragione sociale o codice, Then la lista si riordina correttamente

### Technical Notes
- Permessi: `anagrafiche.clienti.read`, `anagrafiche.clienti.create`, `anagrafiche.clienti.edit`, `anagrafiche.clienti.delete`
- Tabelle: `Customers`, `AuditLog`
- Codice cliente: progressivo automatico con prefisso configurabile (es. CLI-0001)
- Soft delete: archiviazione non cancellazione fisica

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Permessi | [ ] Audit log | [ ] Code review

---

## US-122: Cliente — Dati Fiscali Italiani

**Come** utente amministrativo  
**Voglio** inserire e validare i dati fiscali del cliente  
**Per** garantire la correttezza di fatture e comunicazioni con l'Agenzia delle Entrate

**Priorità**: P0 | **Estimation**: M | **Epic**: Clienti | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given cliente persona giuridica, When utente inserisce P.IVA italiana, Then il sistema valida il check digit (algoritmo di controllo)
- [ ] Given cliente persona fisica, When utente inserisce Codice Fiscale, Then il sistema valida il codice (algoritmo Luhn EE)
- [ ] Given cliente estero, When nazione diversa da Italia, Then campi P.IVA/CF non obbligatori ma VAT number estero disponibile
- [ ] Given cliente con P.IVA duplicata, When utente tenta salvataggio, Then il sistema avvisa (warning, non blocco) e mostra il cliente esistente con quella P.IVA
- [ ] Given cliente, When utente inserisce codice SDI (destinatario fattura elettronica), Then viene validato il formato (7 caratteri alfanumerici)
- [ ] Given cliente con PEC, When viene inserita, Then viene validato il formato email

### Technical Notes
- Tabelle: `Customers` (colonne: `VatNumber`, `FiscalCode`, `SdiCode`, `PecEmail`, `EInvoiceEnabled`)
- Validazione P.IVA IT: algoritmo check digit standard
- Validazione CF: algoritmo codice fiscale italiano

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests (validazione P.IVA, CF, SDI) | [ ] Audit log | [ ] Code review

---

## US-123: Cliente — Indirizzi Multipli

**Come** utente commerciale  
**Voglio** gestire più indirizzi per ogni cliente  
**Per** avere sede legale, destinazione merce e indirizzo fatturazione separati

**Priorità**: P0 | **Estimation**: M | **Epic**: Clienti | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given cliente, When utente aggiunge indirizzo, Then specifica: tipo (Legale/Amministrativa/Spedizione/Fatturazione), indirizzo, CAP, comune, provincia, nazione, note
- [ ] Given cliente con più indirizzi, When viene creato un ordine/DDT, Then il sistema propone come destinazione l'indirizzo di spedizione default
- [ ] Given indirizzo di spedizione, When è il principale, Then è marcato con badge "Principale" e viene sempre proposto per default
- [ ] Given cliente, When si crea fattura, Then indirizzo fatturazione viene usato nell'intestazione documento
- [ ] Given indirizzo default spedizione, When utente lo cambia su un documento, Then la modifica vale solo per quel documento

### Technical Notes
- Tabelle: `CustomerAddresses`
- Vincolo: almeno un indirizzo principale per tipo (spedizione, fatturazione)

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-124: Cliente — Contatti Multipli

**Come** utente commerciale  
**Voglio** gestire più contatti per ogni cliente  
**Per** avere i riferimenti corretti per diverse tipologie di comunicazione

**Priorità**: P0 | **Estimation**: S | **Epic**: Clienti | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given cliente, When utente aggiunge contatto, Then inserisce: nome, cognome, ruolo (es. Responsabile Acquisti, Amministratore), email, telefono, cellulare, note
- [ ] Given cliente con più contatti, When utente visualizza lista, Then vede elenco con ruolo e dati di contatto rapidamente
- [ ] Given contatto principale, When è flaggato come tale, Then appare in evidenza nella scheda cliente e nelle comunicazioni automatiche
- [ ] Given click su email contatto, When in scheda cliente, Then apre client email con indirizzo precompilato

### Technical Notes
- Tabelle: `CustomerContacts`

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-125: Cliente — Condizioni Commerciali

**Come** utente commerciale  
**Voglio** definire le condizioni commerciali specifiche per ogni cliente  
**Per** applicare automaticamente sconti, listini e condizioni di pagamento sugli ordini

**Priorità**: P0 | **Estimation**: M | **Epic**: Clienti | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given cliente, When utente configura condizioni commerciali, Then specifica: listino prezzi, sconto %, condizioni pagamento, agente, limite credito, valuta
- [ ] Given ordine per quel cliente, When creato, Then listino, sconto e condizioni pagamento vengono applicati automaticamente (modificabili sul documento)
- [ ] Given limite credito impostato, When cliente supera il credito con nuovi ordini, Then il sistema avvisa l'utente (warning configurabile: solo avviso o blocco)
- [ ] Given sconto personalizzato, When viene applicato, Then viene mostrato separatamente sul documento (sconto cliente vs sconto riga)

### Technical Notes
- Tabelle: `CustomerCommercialConditions`
- Credit limit: calcolo esposizione (fatture non pagate + ordini aperti)

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests (calcolo esposizione) | [ ] Audit log | [ ] Code review

---

## US-126: Cliente — Documenti Allegati

**Come** utente  
**Voglio** allegare documenti a un cliente  
**Per** conservare contratti, visure, certificazioni direttamente nella scheda cliente

**Priorità**: P1 | **Estimation**: S | **Epic**: Clienti | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente in scheda cliente, When clicca "Allega documento", Then può caricare file (PDF, DOCX, XLSX, immagini) fino a 20MB per file
- [ ] Given allegato caricato, When utente lo visualizza, Then vede: nome file, tipo, dimensione, data upload, utente che ha caricato
- [ ] Given allegato, When utente clicca, Then apre/scarica il file
- [ ] Given allegato sensibile, When utente con permesso limitato, Then non vede gli allegati marcati come "riservati"

### Technical Notes
- Storage: file system locale o blob storage (configurabile)
- Permessi: `anagrafiche.clienti.attachments.read`, `anagrafiche.clienti.attachments.upload`

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Permessi | [ ] Audit log | [ ] Code review

---

## US-127: Cliente — Storico Attività (Timeline)

**Come** utente commerciale  
**Voglio** vedere una timeline delle attività relative a un cliente  
**Per** avere contesto storico completo prima di chiamate o riunioni

**Priorità**: P1 | **Estimation**: M | **Epic**: Clienti | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente in scheda cliente, When va nel tab "Storico", Then vede timeline cronologica: modifiche anagrafica, documenti creati (ordini, fatture, DDT), note inserite
- [ ] Given evento in timeline, When utente clicca, Then va direttamente al documento o attività correlata
- [ ] Given utente, When aggiunge nota manuale, Then appare nella timeline con data, autore e testo
- [ ] Given timeline, When utente filtra per tipo evento, Then vede solo le voci del tipo selezionato

### Technical Notes
- Tabelle: `CustomerActivities` (aggregato da AuditLog + documenti + note manuali)
- Performance: timeline paginata, no full table scan

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Permessi | [ ] Code review

---

## US-128: Cliente — Ricerca e Filtri Avanzati

**Come** utente  
**Voglio** cercare e filtrare clienti con criteri multipli  
**Per** trovare rapidamente i clienti giusti in liste potenzialmente molto lunghe

**Priorità**: P0 | **Estimation**: M | **Epic**: Clienti | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente in lista clienti, When usa ricerca rapida, Then cerca su: ragione sociale, codice, P.IVA, email (istantaneo, debounce 300ms)
- [ ] Given filtri avanzati, When utente li apre, Then può filtrare per: stato, nazione, provincia, agente, listino, condizioni pagamento, limite credito
- [ ] Given ricerca con risultati, When lista visualizzata, Then supporta ordinamento per ogni colonna visibile
- [ ] Given filtri applicati, When utente chiude e riapre la lista, Then i filtri vengono mantenuti nella sessione
- [ ] Given lista con paginazione, When 10K+ clienti, Then navigazione tra pagine in < 500ms

### Technical Notes
- Implementazione: query builder server-side con paginazione cursor-based
- Performance: indici su campi di ricerca principali

### Definition of Done
- [ ] Codice implementato | [ ] Performance test (10K record) | [ ] Unit tests | [ ] Code review

---

## US-129: Cliente — Duplicazione Rapida

**Come** utente che inserisce clienti simili  
**Voglio** duplicare un cliente esistente come base per uno nuovo  
**Per** velocizzare l'inserimento quando il nuovo cliente ha dati simili

**Priorità**: P2 | **Estimation**: S | **Epic**: Clienti | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente in scheda cliente, When clicca "Duplica", Then viene creato nuovo cliente con stessi dati (eccetto: codice cliente nuovo, P.IVA vuota, CF vuoto, SDI vuoto)
- [ ] Given cliente duplicato, When viene creato, Then viene aperto immediatamente per la modifica
- [ ] Given duplicazione, When completata, Then nell'audit log appare "Duplicato da cliente XXXX"

### Technical Notes
- Tabelle: `Customers` (nuovo record)

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-130: Cliente — Merge Duplicati

**Come** amministratore dati  
**Voglio** unire due clienti duplicati in uno solo  
**Per** eliminare duplicati mantenendo tutta la storia documentale

**Priorità**: P2 | **Estimation**: L | **Epic**: Clienti | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given admin che identifica due clienti duplicati, When avvia merge, Then vede schermata confronto side-by-side con selezione campo per campo (quale valore mantenere)
- [ ] Given merge confermato, When eseguito, Then tutti i documenti del cliente secondario vengono riassegnati al cliente principale
- [ ] Given merge completato, When utente cerca il cliente secondario, Then viene reindirizzato al cliente principale con messaggio esplicativo
- [ ] Given merge, When eseguito, Then audit log registra merge con dettaglio completo (quale cliente eliminato, quali record migrati)

### Technical Notes
- Operazione critica: transazionale, rollback automatico su errore
- Permessi: `anagrafiche.clienti.merge` (permesso speciale admin)

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests (transazionalità) | [ ] Permessi | [ ] Audit log | [ ] Code review

---

## US-131: Cliente — Saldo e Esposizione Finanziaria

**Come** responsabile commerciale  
**Voglio** vedere il saldo e l'esposizione finanziaria del cliente  
**Per** valutare il rischio di credito prima di accettare nuovi ordini

**Priorità**: P1 | **Estimation**: M | **Epic**: Clienti | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente in scheda cliente con tab "Finanziario", When lo apre, Then vede: fatture scadute non pagate (con dettaglio), fatture in scadenza, ordini aperti, totale esposizione
- [ ] Given cliente con scaduto, When esposizione supera limite credito, Then nella scheda appare alert visivo (es. banda rossa)
- [ ] Given widget esposizione, When creato nuovo documento per quel cliente, Then l'esposizione viene ricalcolata in tempo reale

### Technical Notes
- Dipende da: Fase 3 (Ciclo Passivo) e Fase 4 (Ciclo Attivo) per dati reali
- In MVP (Fase 2): dati placeholder con messaggio "Disponibile da Fase 4"

### Definition of Done
- [ ] Codice implementato (struttura base) | [ ] Unit tests | [ ] Code review

---

## US-132: Cliente — Categorie e Classificazione

**Come** responsabile commerciale  
**Voglio** classificare i clienti per categoria e segmento  
**Per** fare analisi di vendita per segmento e applicare condizioni diverse per categoria

**Priorità**: P1 | **Estimation**: S | **Epic**: Clienti | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given admin, When crea categoria cliente (es. "A - Strategic", "B - Standard", "C - Small"), Then è disponibile per assegnazione
- [ ] Given cliente, When utente assegna categoria, Then la categoria appare nella scheda e nei report
- [ ] Given lista clienti, When filtro per categoria, Then mostra solo clienti di quella categoria
- [ ] Given report vendite, When raggruppato per categoria cliente, Then mostra fatturato e statistiche per segmento

### Technical Notes
- Tabelle: `CustomerCategories`, `Customers.CategoryId`

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-133: Cliente — Import da Excel (Specifica Clienti)

**Come** responsabile dati  
**Voglio** importare clienti da un file Excel con template dedicato  
**Per** migrare il parco clienti da sistema legacy

**Priorità**: P1 | **Estimation**: M | **Epic**: Clienti | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente, When scarica template Excel clienti, Then template include: codice, ragione sociale, tipo, P.IVA, CF, indirizzo, CAP, comune, provincia, nazione, email, telefono, agente, listino, pagamento
- [ ] Given file compilato, When caricato, Then sistema valida: P.IVA (per clienti IT), email format, duplicati interni al file, duplicati con DB
- [ ] Given errori nel file, When utente vede report, Then ogni riga con errore mostra il campo specifico e il motivo
- [ ] Given file senza errori, When utente avvia import, Then vede progressione e al termine: N inseriti, N aggiornati (se codice esiste), N saltati

### Technical Notes
- Gestione upsert: se codice cliente esiste → aggiorna, se non esiste → crea

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Permessi | [ ] Audit log | [ ] Code review

---

## US-134: Cliente — Gestione Stato (Attivo/Inattivo/Prospect)

**Come** responsabile commerciale  
**Voglio** gestire il ciclo di vita del cliente (prospect → attivo → inattivo)  
**Per** distinguere clienti reali da potenziali e mantenere dati storici senza cancellare

**Priorità**: P1 | **Estimation**: S | **Epic**: Clienti | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given cliente di tipo "Prospect", When viene inserito, Then non ha P.IVA obbligatoria e non può essere usato per fatture
- [ ] Given prospect che diventa cliente, When utente cambia stato a "Attivo", Then può ora essere usato su documenti fiscali
- [ ] Given cliente "Inattivo", When utente tenta di usarlo su nuovo ordine, Then il sistema avvisa con "Cliente non attivo" e chiede conferma
- [ ] Given lista clienti default, When non filtrata, Then mostra solo clienti "Attivi"

### Technical Notes
- Tabelle: `Customers.Status` (Prospect/Active/Inactive/Blocked)

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-135: Cliente — Note e Annotazioni

**Come** utente commerciale  
**Voglio** aggiungere note libere a un cliente  
**Per** conservare informazioni informali utili per i colleghi che gestiscono lo stesso cliente

**Priorità**: P2 | **Estimation**: S | **Epic**: Clienti | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente in scheda cliente, When aggiunge nota, Then inserisce testo libero (max 5000 caratteri), tipo nota (interna/visibile/alert), priorità
- [ ] Given nota di tipo "Alert", When qualsiasi utente apre la scheda del cliente, Then appare in evidenza con banner
- [ ] Given nota interna, When utente senza permesso la visualizza, Then non la vede

### Technical Notes
- Tabelle: `CustomerNotes`

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Permessi | [ ] Audit log | [ ] Code review

---

## US-136: Cliente — Prezzi Speciali per Articolo

**Come** responsabile vendite  
**Voglio** definire prezzi speciali per specifici articoli per un cliente  
**Per** applicare accordi commerciali personalizzati senza creare listini dedicati

**Priorità**: P1 | **Estimation**: M | **Epic**: Clienti | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente in scheda cliente, tab "Prezzi speciali", When inserisce prezzo speciale, Then specifica: articolo, prezzo unitario, sconto%, validità (inizio/fine)
- [ ] Given ordine per quel cliente, When viene inserito articolo con prezzo speciale valido, Then il prezzo viene applicato automaticamente (priorità su listino standard)
- [ ] Given prezzo speciale scaduto, When viene usato documento con data fuori validità, Then il sistema usa il prezzo da listino standard

### Technical Notes
- Tabelle: `CustomerSpecialPrices`
- Priorità prezzi: Prezzo speciale cliente > Listino cliente > Listino standard

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests (priorità prezzi) | [ ] Audit log | [ ] Code review

---

## US-137: Cliente — Partita IVA Lookup (Verifica su Registri)

**Come** utente che inserisce cliente  
**Voglio** verificare i dati fiscali del cliente tramite lookup esterno  
**Per** precompilare automaticamente ragione sociale e indirizzo dalla P.IVA

**Priorità**: P2 | **Estimation**: M | **Epic**: Clienti | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente che inserisce P.IVA italiana, When clicca "Verifica P.IVA", Then il sistema chiama API VIES o Registro Imprese e propone: ragione sociale, sede legale, stato attività
- [ ] Given lookup riuscito, When dati proposti, Then utente può accettare (precompila form) o ignorare
- [ ] Given P.IVA non trovata, When lookup fallisce, Then messaggio chiaro "P.IVA non trovata nei registri" (non blocca salvataggio)

### Technical Notes
- API esterna: VIES EU (per P.IVA EU) o integrazione Registro Imprese IT
- Configurabile: funzionalità disattivabile se API non disponibile

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests (mock API) | [ ] Code review

---

## US-138: Cliente — Gestione Blocco Cliente

**Come** responsabile crediti  
**Voglio** bloccare un cliente per impedire nuovi ordini in caso di insolvenza  
**Per** proteggere l'azienda da ulteriore esposizione verso clienti non affidabili

**Priorità**: P1 | **Estimation**: S | **Epic**: Clienti | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given responsabile crediti, When blocca un cliente, Then deve inserire motivazione blocco e data prevista sblocco (opzionale)
- [ ] Given cliente bloccato, When utente tenta di creare ordine o offerta, Then il sistema blocca con messaggio "Cliente bloccato: [motivazione]" e chi ha bloccato
- [ ] Given admin, When sblocca il cliente, Then il cliente può tornare ad ordinare
- [ ] Given blocco/sblocco, When eseguito, Then audit log registra operazione con motivazione

### Technical Notes
- Tabelle: `Customers.Status = Blocked`, `CustomerBlockHistory`
- Permessi: `anagrafiche.clienti.block` (permesso specifico)

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Permessi | [ ] Audit log | [ ] Code review

---

## US-139: Cliente — Visualizzazione Ultimi Documenti

**Come** utente commerciale  
**Voglio** vedere gli ultimi documenti associati al cliente direttamente dalla sua scheda  
**Per** avere contesto immediato senza dover cercare in ogni modulo separatamente

**Priorità**: P1 | **Estimation**: S | **Epic**: Clienti | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente in scheda cliente, tab "Documenti", When lo apre, Then vede lista degli ultimi 20 documenti (ordini, DDT, fatture) con: tipo, numero, data, importo, stato
- [ ] Given documento in lista, When utente clicca, Then viene reindirizzato al documento corrispondente nel modulo relativo
- [ ] Given filtro tipo documento, When utente seleziona solo "Fatture", Then vede solo le fatture di quel cliente

### Technical Notes
- Dipende da moduli futuri (Ciclo Attivo). In MVP: placeholder con messaggio "Disponibile da Fase 4"

### Definition of Done
- [ ] Codice implementato (struttura) | [ ] Unit tests | [ ] Code review

---

## US-140: Cliente — Report Clienti per Agente/Zona

**Come** responsabile vendite  
**Voglio** estrarre un report dei clienti raggruppati per agente o zona  
**Per** valutare la distribuzione del portafoglio clienti e supportare le decisioni commerciali

**Priorità**: P2 | **Estimation**: M | **Epic**: Clienti | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente con permesso report, When apre report "Clienti per Agente", Then vede tabella con: agente, numero clienti assegnati, ultimo ordine (data), fatturato YTD (se disponibile)
- [ ] Given report, When utente esporta, Then scarica Excel con tutti i dati
- [ ] Given report "Clienti per Zona", When apre, Then vede mappa o tabella con distribuzione geografica clienti per provincia/regione

### Technical Notes
- Permessi: `report.anagrafiche.read`
- Fatturato YTD: disponibile solo da Fase 4

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Permessi | [ ] Code review
