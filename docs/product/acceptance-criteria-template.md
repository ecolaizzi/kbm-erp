# KBM - Acceptance Criteria Template

**Versione**: 1.0  
**Data**: 2026-06-09  
**Owner**: Product Owner Agent  
**Scopo**: Template riutilizzabile per tutte le user stories del progetto KBM

---

## Template User Story (Copia-Incolla)

```markdown
## US-XXX: [Titolo Breve e Descrittivo]

**Come** [ruolo utente specifico]
**Voglio** [funzionalità specifica e misurabile]
**Per** [beneficio business tangibile]

**Priorità**: P0 / P1 / P2
**Estimation**: S / M / L / XL
**Epic**: [Nome Epic]
**Modulo**: [Core / Anagrafiche / Vendite / Acquisti / Magazzino / Contabilità]
**Dipende da**: [US-XXX o N/A]
**Blocca**: [US-XXX o N/A]

### Acceptance Criteria

- [ ] **Given** [precondizione / contesto iniziale], **When** [azione utente o evento], **Then** [risultato atteso verificabile]
- [ ] **Given** [scenario alternativo], **When** [azione], **Then** [risultato]
- [ ] **Given** [caso limite / edge case], **When** [azione], **Then** [comportamento atteso]
- [ ] **Given** [scenario di errore], **When** [azione errata], **Then** [messaggio di errore appropriato]
- [ ] **Given** [requisito non funzionale], **When** [azione con volume], **Then** [performance attesa]

### Technical Notes

- **Permessi richiesti**: [lista permessi format `{modulo}.{risorsa}.{azione}`]
- **Tabelle coinvolte**: [lista tabelle DB]
- **API endpoints**: [lista REST endpoints format `VERB /api/path`]
- **Dipendenze esterne**: [servizi esterni, integrazioni, librerie]
- **Note di implementazione**: [vincoli tecnici, algoritmi, pattern architetturali]

### Definition of Done

- [ ] Codice implementato e funzionante
- [ ] Unit tests passati (coverage >= 80% su business logic)
- [ ] Integration tests passati
- [ ] Permessi implementati e testati
- [ ] Audit log implementato (se operazione di write)
- [ ] Validazione input implementata (lato server)
- [ ] Messaggi di errore utente-friendly in italiano
- [ ] Performance verificata (response time < 2s per operazioni standard)
- [ ] Documentazione API aggiornata (OpenAPI/Swagger)
- [ ] Code review approvata
- [ ] QA validation approvata
```

---

## Guida Compilazione

### Ruoli Utente KBM

| Ruolo | Descrizione | Codice |
|---|---|---|
| Amministratore di sistema | Configura sistema, utenti, ruoli, aziende | `admin` |
| Responsabile Acquisti | Gestisce ciclo passivo, ordini fornitori | `resp_acquisti` |
| Operatore Acquisti | Inserisce RDA, ordini, ricevimenti | `op_acquisti` |
| Responsabile Vendite | Gestisce offerte, ordini, clienti | `resp_vendite` |
| Agente di Vendita | Inserisce offerte e ordini per propri clienti | `agente` |
| Magazziniere | Movimenti magazzino, inventario | `magazziniere` |
| Contabile | Prima nota, IVA, scadenzario | `contabile` |
| Responsabile Qualità | Qualifica fornitori, gestione non conformità | `resp_qualita` |
| CEO / Management | Dashboard, KPI, report di sintesi | `management` |

### Scale di Priorità

| Priorità | Definizione | Criterio |
|---|---|---|
| **P0** | Must-have critico | Il sistema non funziona senza questa feature. Bloccante per go-live. |
| **P1** | Alta importanza | Richiesto dalla maggioranza degli utenti. Necessario per adozione commerciale. |
| **P2** | Valore aggiunto | Migliora l'esperienza ma non blocca l'adozione. Posticipabile a sprint successivo. |

### T-Shirt Sizing

| Size | Story Points | Descrizione | Giorni sviluppatore stimati |
|---|---|---|---|
| **S** (Small) | 1-2 | Feature semplice, poche tabelle, UI minimale | 0.5 - 1 giorno |
| **M** (Medium) | 3-5 | Feature con logica media, 2-3 tabelle, form strutturato | 2 - 3 giorni |
| **L** (Large) | 8-13 | Feature complessa, algoritmi, multi-step, performance critica | 4 - 7 giorni |
| **XL** (Extra Large) | 20+ | Modulo intero, integrazioni esterne, migrazione dati | > 1 settimana → splittare in storie più piccole |

---

## Acceptance Criteria — Best Practices

### ✅ Buoni Acceptance Criteria

```
Given utente loggato con permesso clienti.create,
When inserisce cliente con P.IVA "12345678901",
Then il sistema valida il check digit e se errato mostra "P.IVA non valida"
```

Caratteristiche:
- **Specifici**: contengono valori esatti (es. P.IVA concreta, non generica)
- **Verificabili**: qualcuno può testare meccanicamente il comportamento
- **Indipendenti**: ogni criterio è autonomo
- **Utente-centriici**: descrivono comportamento visibile all'utente

### ❌ Cattivi Acceptance Criteria

```
Il sistema deve funzionare bene
Il sistema deve essere veloce
L'interfaccia deve essere intuitiva
```

Problemi:
- Non misurabili (cosa significa "bene"?)
- Non verificabili meccanicamente
- Soggettivi

---

## Pattern di Acceptance Criteria per Tipo di Feature

### CRUD Standard
```
Given utente con permesso {risorsa}.create, When [azione create], Then [risultato create]
Given utente con permesso {risorsa}.read, When [azione list/view], Then [risultato visualizzazione]
Given utente con permesso {risorsa}.edit, When [azione edit], Then [risultato edit + audit]
Given utente senza permesso {risorsa}.delete, When [azione delete], Then [403 Forbidden]
Given [record con dipendenze], When utente tenta delete, Then [blocco con messaggio]
```

### Validazione Input
```
Given campo obbligatorio vuoto, When utente salva, Then messaggio "Campo X obbligatorio"
Given formato email errato, When utente inserisce, Then messaggio "Formato email non valido"
Given valore numerico fuori range, When inserito, Then messaggio con range accettabile
Given duplicato rilevato, When utente salva, Then warning con link al record esistente
```

### Permessi e Sicurezza
```
Given utente senza permesso X, When accede a funzione X, Then riceve 403 Forbidden
Given utente di azienda A, When tenta di vedere dati di azienda B, Then riceve 403 (tenant isolation)
Given utente non autenticato, When chiama endpoint protetto, Then riceve 401 Unauthorized
```

### Performance
```
Given lista con 10.000+ record, When utente la apre, Then carica in < 1 secondo
Given ricerca full-text, When utente digita 3+ caratteri, Then risultati in < 300ms
Given export di 50.000 righe, When avviato, Then job in background con notifica al completamento
```

### Audit Log
```
Given operazione di Create/Update/Delete, When eseguita da utente X, Then AuditLog contiene: userId, timestamp, entità, azione, valori prima/dopo
Given audit log, When admin filtra per utente e periodo, Then vede solo record corrispondenti
```

---

## Moduli e Prefissi Permessi

| Modulo | Prefisso Permesso | Esempio |
|---|---|---|
| Core - Utenti | `core.users` | `core.users.create` |
| Core - Ruoli | `core.roles` | `core.roles.edit` |
| Core - Aziende | `core.companies` | `core.companies.read` |
| Core - Audit | `core.audit` | `core.audit.read` |
| Anagrafiche - Clienti | `anagrafiche.clienti` | `anagrafiche.clienti.delete` |
| Anagrafiche - Fornitori | `anagrafiche.fornitori` | `anagrafiche.fornitori.create` |
| Anagrafiche - Articoli | `anagrafiche.articoli` | `anagrafiche.articoli.edit` |
| Anagrafiche - Listini | `anagrafiche.listini` | `anagrafiche.listini.edit` |
| Vendite - Offerte | `vendite.offerte` | `vendite.offerte.approve` |
| Vendite - Ordini | `vendite.ordini` | `vendite.ordini.confirm` |
| Acquisti - ODA | `acquisti.oda` | `acquisti.oda.create` |
| Magazzino - Movimenti | `magazzino.movimenti` | `magazzino.movimenti.create` |
| Contabilità - Prima Nota | `contabilita.primanota` | `contabilita.primanota.post` |
| Report | `report.{modulo}` | `report.vendite.read` |

---

## Checklist Pre-Sprint per User Story

Prima di portare una user story in sprint, verificare:

- [ ] **INVEST check**: Independent, Negotiable, Valuable, Estimable, Small, Testable
- [ ] **Acceptance Criteria**: almeno 3 criteri, incluso 1 scenario negativo
- [ ] **P0 stories**: tutti i criteri hanno Given-When-Then esplicito
- [ ] **Technical Notes**: tabelle, API, permessi definiti
- [ ] **Dependencies**: dipendenze da altre storie/epic dichiarate
- [ ] **Mockup/wireframe**: link a Figma/sketch se feature UI rilevante
- [ ] **Team review**: Product Owner + Tech Lead hanno approvato scope

---

*Template v1.0 - Aggiornare a fine ogni fase con pattern emersi durante sviluppo*
