# Epic 5 - Articoli e Listini (US-161 → US-180)

**Modulo**: Anagrafiche  
**Fase**: 2  
**Obiettivo**: Catalogo articoli e servizi, gestione listini prezzi vendita e acquisto

---

## US-161: CRUD Articolo — Dati Base

**Come** utente gestione catalogo  
**Voglio** creare, visualizzare, modificare e archiviare articoli  
**Per** mantenere il catalogo prodotti e servizi aggiornato e completo

**Priorità**: P0 | **Estimation**: M | **Epic**: Articoli e Listini | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente con permesso articoli.create, When crea articolo, Then inserisce: codice (auto o manuale), descrizione, tipo (Prodotto/Servizio/Kit), categoria, UM principale, stato (attivo/inattivo)
- [ ] Given articolo creato, When visualizzato, Then scheda con tab: Dati generali, Prezzi e Listini, Fornitori, Magazzino (placeholder Fase 5), Documenti
- [ ] Given articolo con movimenti, When utente tenta eliminazione, Then sistema blocca e propone disattivazione
- [ ] Given lista articoli, When utente ricerca per codice o descrizione, Then risultati in < 300ms anche con 100K articoli

### Technical Notes
- Permessi: `anagrafiche.articoli.read`, `anagrafiche.articoli.create`, `anagrafiche.articoli.edit`, `anagrafiche.articoli.delete`
- Tabelle: `Articles`, `AuditLog`
- Codice: progressivo con prefisso configurabile (es. ART-00001) o libero

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Permessi | [ ] Audit log | [ ] Code review

---

## US-162: Articolo — Dati Fiscali e IVA

**Come** utente gestione catalogo  
**Voglio** configurare i dati fiscali dell'articolo  
**Per** applicare automaticamente IVA corretta sui documenti di vendita e acquisto

**Priorità**: P0 | **Estimation**: S | **Epic**: Articoli e Listini | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given articolo, When utente assegna categoria IVA vendita (es. 22%), Then tutti i documenti di vendita con quell'articolo useranno quell'aliquota
- [ ] Given articolo, When utente assegna categoria IVA acquisto (può differire da vendita), Then usata su fatture passive
- [ ] Given articolo esente IVA (es. servizi medici), When inserito su fattura, Then importo IVA = 0 con codice esenzione corretto
- [ ] Given codice NC (Nomenclatura Combinata EU), When inserito, Then usato per dichiarazioni Intrastat (rilevante per Fase 4+)

### Technical Notes
- Tabelle: `Articles` (VatCategoryId, VatCategoryPurchaseId, NcCode)

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-163: Articolo — Unità di Misura Multiple e Conversioni

**Come** utente gestione catalogo  
**Voglio** gestire più unità di misura per lo stesso articolo  
**Per** acquistare in pallet, gestire a collo e vendere a pezzo con conversioni automatiche

**Priorità**: P1 | **Estimation**: M | **Epic**: Articoli e Listini | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given articolo, When utente aggiunge UM alternativa, Then specifica: UM, fattore conversione rispetto a UM principale, UM per acquisto (default), UM per vendita (default)
- [ ] Given documento di acquisto con UM "Pallet" (24 pz), When inserita quantità 1 pallet, Then il magazzino viene caricato con 24 pezzi
- [ ] Given conversione definita, When documento usa UM alternativa, Then sistema mostra equivalente in UM principale nel tooltip
- [ ] Given UM non configurata, When utente inserisce UM diversa su documento, Then sistema avvisa "Conversione non definita"

### Technical Notes
- Tabelle: `ArticleUomConversions`

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests (conversioni) | [ ] Audit log | [ ] Code review

---

## US-164: Articolo — Codici Alternativi (Barcode, EAN, Codice Fornitore)

**Come** utente magazzino/acquisti  
**Voglio** gestire codici alternativi per ogni articolo  
**Per** identificare l'articolo tramite barcode scanner, codice fornitore o EAN senza conoscere il codice interno

**Priorità**: P1 | **Estimation**: M | **Epic**: Articoli e Listini | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given articolo, When utente aggiunge codice alternativo, Then specifica: tipo (EAN-13, EAN-8, Code128, Codice Fornitore, Codice Cliente), valore, fornitore/cliente di riferimento (per codici partner)
- [ ] Given ricerca articolo per codice EAN, When utente digita o scansiona, Then l'articolo viene trovato immediatamente
- [ ] Given ordine di acquisto con codice fornitore, When inserito, Then il sistema abbina all'articolo interno corrispondente
- [ ] Given EAN-13 inserito, When sistema valida, Then verifica check digit corretto

### Technical Notes
- Tabelle: `ArticleAlternativeCodes`
- Performance: indice su codice alternativo per lookup rapido

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests (EAN validation, lookup) | [ ] Code review

---

## US-165: Articolo — Immagini e Media

**Come** utente gestione catalogo  
**Voglio** associare immagini e documenti tecnici agli articoli  
**Per** avere il catalogo visuale e le schede tecniche direttamente nel sistema

**Priorità**: P2 | **Estimation**: M | **Epic**: Articoli e Listini | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente, When carica immagine articolo, Then specifica immagine principale e immagini aggiuntive (max 10, max 5MB l'una, formati JPG/PNG/WebP)
- [ ] Given immagine principale, When l'articolo appare in liste, Then thumbnail viene mostrato
- [ ] Given scheda tecnica (PDF), When allegata, Then accessibile dalla scheda articolo e scaricabile
- [ ] Given immagine caricata, When troppo grande, Then sistema la ridimensiona automaticamente per preview

### Technical Notes
- Storage: file system o blob storage
- Thumbnail: generato automaticamente (es. 200x200px)

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-166: Articolo — Dati Dimensionali e Logistici

**Come** utente magazzino / logistica  
**Voglio** inserire i dati dimensionali degli articoli  
**Per** calcolare volumi, pesi e costi di spedizione automaticamente

**Priorità**: P1 | **Estimation**: S | **Epic**: Articoli e Listini | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given articolo, When utente inserisce dati logistici, Then può specificare: peso netto (kg), peso lordo (kg), lunghezza (cm), larghezza (cm), altezza (cm), volume (dm³)
- [ ] Given articolo con dati dimensionali, When ordine ha N pezzi, Then sistema calcola totale peso e volume del documento
- [ ] Given collo standard configurato, When sistema calcola spedizione, Then usa peso/volume per calcolo colli necessari

### Technical Notes
- Tabelle: `Articles` (campi dimensionali)

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Code review

---

## US-167: Articolo — Prezzi Base Vendita e Acquisto

**Come** utente gestione catalogo  
**Voglio** inserire prezzi base di vendita e acquisto sull'articolo  
**Per** avere un riferimento immediato senza dover aprire il listino

**Priorità**: P0 | **Estimation**: S | **Epic**: Articoli e Listini | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given articolo, When utente inserisce prezzo base, Then specifica: prezzo vendita base (IVA esclusa), prezzo acquisto base, valuta
- [ ] Given prezzo base, When non è presente listino specifico per cliente, Then viene usato come fallback
- [ ] Given articolo con prezzo acquisto, When si crea ordine acquisto, Then il prezzo base acquisto viene proposto (modificabile)
- [ ] Given storico variazioni prezzo, When utente visualizza, Then vede log delle modifiche di prezzo con data e utente

### Technical Notes
- Tabelle: `Articles` (BasePrice, BasePurchasePrice), `ArticlePriceHistory`

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log (variazioni prezzo) | [ ] Code review

---

## US-168: Listino Prezzi — Creazione e Gestione

**Come** responsabile commerciale  
**Voglio** creare e gestire listini prezzi vendita  
**Per** applicare prezzi differenziati per segmento cliente, stagione o categoria

**Priorità**: P0 | **Estimation**: L | **Epic**: Articoli e Listini | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given responsabile, When crea listino, Then specifica: codice, descrizione, tipo (vendita/acquisto), valuta, validità (data inizio, data fine opzionale), metodo calcolo
- [ ] Given metodo "Prezzo fisso", When inserita riga listino articolo, Then utente inserisce prezzo direttamente
- [ ] Given metodo "Sconto su prezzo base", When inserita riga, Then utente inserisce % sconto e prezzo viene calcolato da prezzo base articolo
- [ ] Given metodo "Maggiorazione su prezzo acquisto", When inserita riga, Then prezzo = acquisto + % maggiorazione
- [ ] Given listino scaduto, When usato su documento con data fuori validità, Then sistema avvisa utente

### Technical Notes
- Tabelle: `PriceLists`, `PriceListItems`
- Metodi: FixedPrice, DiscountOnBase, MarkupOnCost, PercentageOfOther

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests (metodi calcolo) | [ ] Audit log | [ ] Code review

---

## US-169: Listino — Righe e Prezzi per Articolo

**Come** responsabile commerciale  
**Voglio** inserire e gestire le righe di un listino articolo per articolo  
**Per** avere prezzi specifici per ogni prodotto nel listino

**Priorità**: P0 | **Estimation**: M | **Epic**: Articoli e Listini | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given listino aperto, When utente aggiunge riga, Then cerca articolo (by codice o descrizione) e inserisce: prezzo, sconto, UM listino, quantità minima, note
- [ ] Given riga listino, When quantità scaglioni definiti, Then per quantità >= scaglione si applica prezzo/sconto corrispondente
- [ ] Given import righe da Excel, When caricato file, Then sistema inserisce/aggiorna tutte le righe con report errori
- [ ] Given listino con 10.000+ righe, When ricercato articolo specifico, Then risultato in < 500ms

### Technical Notes
- Tabelle: `PriceListItems`, `PriceListItemTiers` (scaglioni quantità)
- Import: template Excel con colonne standard

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests (scaglioni) | [ ] Performance test | [ ] Audit log | [ ] Code review

---

## US-170: Listino — Copia e Clonazione

**Come** responsabile commerciale  
**Voglio** copiare un listino esistente per creare varianti o nuova stagione  
**Per** velocizzare la creazione di listini simili

**Priorità**: P1 | **Estimation**: S | **Epic**: Articoli e Listini | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given responsabile, When clona listino, Then specifica: nuovo codice, nuova descrizione, nuove date validità, fattore rettifica globale (es. +5% su tutti i prezzi)
- [ ] Given clone avviato, When completato, Then tutte le righe sono copiate con prezzi rettificati
- [ ] Given operazione completata, When audit log verificato, Then riporta "Clonato da listino XXXX"

### Technical Notes
- Operazione batch in background per listini con molte righe

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-171: Listino — Assegnazione a Clienti e Categorie

**Come** responsabile commerciale  
**Voglio** assegnare listini a clienti o categorie di clienti  
**Per** applicare automaticamente il giusto listino senza selezione manuale su ogni documento

**Priorità**: P0 | **Estimation**: S | **Epic**: Articoli e Listini | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given cliente, When utente assegna listino nelle condizioni commerciali, Then su tutti i documenti per quel cliente viene usato il listino assegnato
- [ ] Given categoria clienti, When listino assegnato alla categoria, Then tutti i clienti di quella categoria usano quel listino (sovrascrivibile a livello cliente)
- [ ] Given priorità listini, When cliente ha listino personale E listino categoria, Then il listino cliente ha priorità
- [ ] Given documento in creazione, When nessun listino specifico applicabile, Then usa listino standard (default)

### Technical Notes
- Priorità: Listino cliente > Listino categoria cliente > Listino standard

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests (priorità) | [ ] Audit log | [ ] Code review

---

## US-172: Articolo — Ricerca e Filtri Avanzati

**Come** utente  
**Voglio** cercare articoli con filtri multipli  
**Per** trovare rapidamente prodotti in cataloghi con decine di migliaia di articoli

**Priorità**: P0 | **Estimation**: M | **Epic**: Articoli e Listini | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente in lista articoli, When cerca per testo, Then ricerca su: codice, descrizione, EAN, codice alternativo (debounce 300ms, < 500ms risposta)
- [ ] Given filtri avanzati, When applicati, Then filtra per: categoria, tipo (prodotto/servizio/kit), stato, UM, range prezzo, fornitore preferito
- [ ] Given ricerca con 100K+ articoli, When eseguita, Then risultati in < 500ms con indici appropriati
- [ ] Given risultati, When visualizzati, Then mostra thumbnail articolo se disponibile

### Technical Notes
- Full-text search con indici SQL Server su descrizione e codici

### Definition of Done
- [ ] Codice implementato | [ ] Performance test (100K articoli) | [ ] Unit tests | [ ] Code review

---

## US-173: Articolo — Import da Excel

**Come** responsabile catalogo  
**Voglio** importare articoli da Excel  
**Per** caricare in blocco il catalogo da sistema legacy

**Priorità**: P1 | **Estimation**: M | **Epic**: Articoli e Listini | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given template scaricabile, When include campi: codice, descrizione, tipo, categoria, UM, prezzo vendita, prezzo acquisto, IVA vendita, IVA acquisto, EAN, stato
- [ ] Given file con 50.000 righe, When importato, Then elaborazione in background con progressione visibile
- [ ] Given articolo con codice già esistente, When importato, Then aggiorna (upsert) invece di duplicare
- [ ] Given errori (es. categoria inesistente, UM non trovata), When riportati, Then messaggio specifico per riga

### Technical Notes
- Background job per file grandi (> 1000 righe)
- Chunk processing per evitare timeout

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Performance test | [ ] Permessi | [ ] Audit log | [ ] Code review

---

## US-174: Articolo — Storico Prezzi Acquisto

**Come** responsabile acquisti  
**Voglio** vedere lo storico dei prezzi di acquisto di un articolo per fornitore  
**Per** valutare trend prezzi e negoziare meglio con i fornitori

**Priorità**: P1 | **Estimation**: S | **Epic**: Articoli e Listini | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente in scheda articolo, tab "Prezzi acquisto", When visualizza, Then vede tabella: fornitore, data, prezzo acquisto, sconto, prezzo netto per ogni modifica registrata
- [ ] Given storico prezzi, When visualizzato come grafico, Then mostra andamento nel tempo per fornitore
- [ ] Given articolo multi-fornitore, When filtrato per fornitore specifico, Then mostra solo prezzi di quel fornitore

### Technical Notes
- Tabelle: `ArticlePurchasePriceHistory` (popolata da ordini/fatture in Fase 3)
- In Fase 2: struttura e inserimento manuale

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Code review

---

## US-175: Articolo — Gestione Kit e Distinta Base Semplice

**Come** responsabile commerciale  
**Voglio** creare articoli di tipo "Kit" composti da altri articoli  
**Per** vendere bundle di prodotti come unità singola con prezzo complessivo

**Priorità**: P2 | **Estimation**: L | **Epic**: Articoli e Listini | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given articolo tipo "Kit", When utente aggiunge componente, Then specifica: articolo componente, quantità, UM, obbligatorio/opzionale
- [ ] Given kit su documento di vendita, When inserito, Then le righe componenti vengono esplose automaticamente (o in forma compatta configurabile)
- [ ] Given prezzo kit, When configurato come "Somma componenti", Then viene calcolato automaticamente sommando prezzi componenti
- [ ] Given prezzo kit fisso, When configurato come "Prezzo fisso", Then usa il prezzo inserito indipendentemente dai componenti

### Technical Notes
- Tabelle: `ArticleBom` (Bill of Materials semplice)
- Questo è DIVERSO da MRP/produzione (Fase 7) — solo per vendita

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-176: Articolo — Descrizione Estesa e Multilingua

**Come** responsabile catalogo  
**Voglio** inserire descrizioni estese e traduzioni per gli articoli  
**Per** usare descrizioni dettagliate su offerte e documenti di vendita in diverse lingue

**Priorità**: P2 | **Estimation**: M | **Epic**: Articoli e Listini | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given articolo, When utente inserisce descrizione estesa (rich text), Then può usare formattazione base (grassetto, lista, tabella)
- [ ] Given lingua documento impostata su "EN", When articolo viene aggiunto a documento, Then usa descrizione inglese se disponibile, altrimenti italiano
- [ ] Given traduzione articolo, When inserita, Then specifica lingua e descrizione breve + estesa

### Technical Notes
- Tabelle: `ArticleTranslations`

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-177: Listino — Prezzi per Scaglioni Quantità

**Come** responsabile commerciale  
**Voglio** definire prezzi a scaglioni per quantità su un listino  
**Per** applicare prezzi decrescenti all'aumentare del quantitativo acquistato

**Priorità**: P1 | **Estimation**: M | **Epic**: Articoli e Listini | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given riga listino, When utente abilita scaglioni, Then aggiunge: qtà da, prezzo/sconto per quello scaglione
- [ ] Given documento con riga articolo, When quantità corrisponde a scaglione, Then prezzo aggiornato automaticamente
- [ ] Given scaglione applicato, When riga viene visualizzata, Then mostra prezzo scaglione e nota "Scaglione quantità applicato"
- [ ] Given quantità modificata sul documento, When cambia scaglione, Then prezzo viene ricalcolato in tempo reale

### Technical Notes
- Tabelle: `PriceListItemTiers`
- Logica: trova scaglione con qtà_da massima <= quantità inserita

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests (scaglioni) | [ ] Code review

---

## US-178: Articolo — Attributi Personalizzati

**Come** amministratore  
**Voglio** definire attributi personalizzati per gli articoli  
**Per** estendere il modello dati con caratteristiche specifiche del settore (colore, materiale, tensione, etc.)

**Priorità**: P2 | **Estimation**: L | **Epic**: Articoli e Listini | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given admin, When crea attributo personalizzato, Then specifica: nome, tipo (testo, numero, booleano, lista valori), obbligatorio sì/no, applicabile a categoria
- [ ] Given articolo di categoria con attributi, When creato, Then i campi attributo sono visibili e compilabili
- [ ] Given filtro per attributo, When utente cerca articoli con "Colore = Rosso", Then vede solo articoli rossi
- [ ] Given attributo eliminato, When aveva valori su articoli, Then sistema blocca o chiede conferma con impatto visibile

### Technical Notes
- Tabelle: `ArticleAttributeDefinitions`, `ArticleAttributeValues`
- Pattern: EAV (Entity-Attribute-Value) con tipo check

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-179: Listino — Validità Temporale e Archivio

**Come** responsabile commerciale  
**Voglio** gestire la validità temporale dei listini e archiviare quelli scaduti  
**Per** mantenere storico prezzi e garantire che su documenti passati vengano usati i prezzi dell'epoca

**Priorità**: P1 | **Estimation**: S | **Epic**: Articoli e Listini | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given listino con data fine validità, When data supera la scadenza, Then il listino viene marcato automaticamente come "Scaduto"
- [ ] Given listino scaduto, When visualizzato in lista, Then appare in sezione "Archivio" (non nella lista attiva)
- [ ] Given documento storico, When rivisualizzato, Then mostra i prezzi del listino che era attivo alla data del documento
- [ ] Given admin, When riattiva listino scaduto, Then deve inserire nuova data validità

### Technical Notes
- Job schedulato: controllo giornaliero validità listini

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Code review

---

## US-180: Articolo — Dashboard Catalogo KPI

**Come** responsabile prodotti  
**Voglio** vedere KPI sul catalogo articoli nella dashboard  
**Per** monitorare lo stato del catalogo e identificare articoli che necessitano attenzione

**Priorità**: P2 | **Estimation**: S | **Epic**: Articoli e Listini | **Modulo**: Anagrafiche

### Acceptance Criteria
- [ ] Given utente con permesso articoli.read, When vede widget catalogo, Then mostra: totale articoli attivi, articoli senza prezzo, articoli senza immagine, articoli senza EAN
- [ ] Given widget "Articoli senza prezzo", When cliccato, Then porta a lista filtrata con quegli articoli
- [ ] Given widget, When tutti gli articoli sono completi, Then mostra stato "OK" con checkmark verde

### Technical Notes
- Query aggregate con cache 5 minuti

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Code review
