# Epic 1 - Core Platform (US-001 → US-020)

**Modulo**: Core  
**Fase**: 1  
**Obiettivo**: Fondamenta tecniche dell'ERP — autenticazione, multi-azienda, RBAC, audit log

---

## US-001: Login con Username e Password

**Come** utente del sistema  
**Voglio** accedere con username e password  
**Per** autenticarmi in modo sicuro e accedere alle funzionalità ERP

**Priorità**: P0 | **Estimation**: M | **Epic**: Core Platform | **Modulo**: Core

### Acceptance Criteria
- [ ] Given credenziali valide, When inserisco username/password e clicco Login, Then accedo al sistema e vedo la dashboard home
- [ ] Given credenziali errate, When inserisco password sbagliata, Then vedo messaggio di errore generico (no hint su quale campo è sbagliato)
- [ ] Given 5 tentativi falliti consecutivi, When il sistema rileva il limite, Then l'account viene bloccato per 15 minuti
- [ ] Given account bloccato, When tento di accedere, Then vedo messaggio "Account temporaneamente bloccato" con tempo rimanente
- [ ] Given login riuscito, When il sistema elabora l'autenticazione, Then viene creato un token JWT con scadenza configurabile (default 8h)

### Technical Notes
- Permessi richiesti: nessuno (endpoint pubblico)
- Tabelle coinvolte: `Users`, `UserSessions`, `LoginAttempts`
- API endpoints: `POST /api/auth/login`
- Hash password: Argon2id (OWASP 2024)
- Audit: loggare login riusciti e falliti con IP e user-agent

### Definition of Done
- [ ] Codice implementato
- [ ] Unit tests passati (>= 90% coverage su AuthService)
- [ ] Integration tests passati
- [ ] Audit log implementato (login/logout)
- [ ] Documentazione API aggiornata
- [ ] Code review approvata

---

## US-002: Multi-Factor Authentication (MFA)

**Come** amministratore di sistema  
**Voglio** abilitare MFA per gli utenti  
**Per** aumentare la sicurezza degli accessi e conformarmi a requisiti enterprise

**Priorità**: P0 | **Estimation**: L | **Epic**: Core Platform | **Modulo**: Core

### Acceptance Criteria
- [ ] Given MFA abilitato per un utente, When inserisce credenziali corrette, Then il sistema chiede il codice OTP (TOTP/HOTP)
- [ ] Given codice OTP valido, When l'utente lo inserisce, Then accede al sistema normalmente
- [ ] Given codice OTP scaduto o errato, When l'utente lo inserisce, Then vede errore e può riprovare (max 3 tentativi)
- [ ] Given utente che vuole configurare MFA, When va nelle impostazioni account, Then vede QR code per configurare app authenticator (Google Auth, Authy)
- [ ] Given MFA obbligatorio per ruolo admin, When admin tenta accesso senza MFA configurato, Then viene forzato a configurarlo

### Technical Notes
- Permessi richiesti: `auth.mfa.configure` (per admin), utente proprio account
- Tabelle coinvolte: `Users`, `UserMfaSettings`
- API endpoints: `POST /api/auth/mfa/setup`, `POST /api/auth/mfa/verify`, `POST /api/auth/mfa/disable`
- Libreria: RFC 6238 TOTP standard
- Codici di backup: generare 10 codici one-time alla configurazione

### Definition of Done
- [ ] Codice implementato
- [ ] Unit tests passati
- [ ] Permessi implementati
- [ ] Audit log: configurazione MFA, login con MFA, disable MFA
- [ ] Documentazione aggiornata
- [ ] Code review approvata

---

## US-003: Gestione Sessioni e Logout

**Come** utente autenticato  
**Voglio** effettuare logout e gestire le mie sessioni attive  
**Per** proteggere il mio account su dispositivi condivisi

**Priorità**: P0 | **Estimation**: S | **Epic**: Core Platform | **Modulo**: Core

### Acceptance Criteria
- [ ] Given utente loggato, When clicca Logout, Then la sessione viene invalidata e viene reindirizzato a pagina login
- [ ] Given token scaduto, When l'utente tenta un'operazione, Then riceve 401 e viene reindirizzato a login
- [ ] Given utente con sessioni multiple, When va in "Le mie sessioni", Then vede lista sessioni attive con IP, device, data
- [ ] Given sessione sospetta, When l'utente la revoca, Then quella sessione viene invalidata immediatamente
- [ ] Given admin, When revoca sessione di un utente, Then quell'utente viene disconnesso immediatamente

### Technical Notes
- Tabelle coinvolte: `UserSessions`, `RefreshTokens`
- API endpoints: `POST /api/auth/logout`, `GET /api/auth/sessions`, `DELETE /api/auth/sessions/{id}`
- JWT + Refresh Token pattern con revocation list

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-004: Password Policy e Reset Password

**Come** amministratore  
**Voglio** configurare una password policy e permettere il reset password  
**Per** garantire sicurezza degli accessi e gestire utenti che dimenticano la password

**Priorità**: P0 | **Estimation**: M | **Epic**: Core Platform | **Modulo**: Core

### Acceptance Criteria
- [ ] Given admin nella configurazione sicurezza, When imposta policy (lunghezza min, complessità, scadenza), Then la policy viene applicata a tutti i nuovi utenti
- [ ] Given utente che dimentica password, When clicca "Password dimenticata", Then riceve email con link reset (valido 30 min)
- [ ] Given link reset valido, When inserisce nuova password che rispetta policy, Then la password viene cambiata e il link invalidato
- [ ] Given primo accesso utente, When accede per la prima volta, Then viene forzato al cambio password
- [ ] Given password scaduta (se policy attiva), When utente accede, Then viene reindirizzato obbligatoriamente al cambio password

### Technical Notes
- Tabelle coinvolte: `Users`, `PasswordResetTokens`, `PasswordHistory`, `SecuritySettings`
- API endpoints: `POST /api/auth/forgot-password`, `POST /api/auth/reset-password`, `POST /api/auth/change-password`
- No riuso ultime N password (configurabile, default 5)

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-005: CRUD Utenti (Amministrazione)

**Come** amministratore  
**Voglio** creare, modificare, disabilitare e eliminare utenti  
**Per** gestire gli accessi al sistema ERP

**Priorità**: P0 | **Estimation**: M | **Epic**: Core Platform | **Modulo**: Core

### Acceptance Criteria
- [ ] Given admin nella sezione Utenti, When crea nuovo utente con dati obbligatori (email, nome, cognome, ruolo), Then l'utente viene creato e riceve email di attivazione
- [ ] Given admin, When modifica dati utente, Then le modifiche sono salvate e loggate nell'audit
- [ ] Given admin, When disabilita utente, Then l'utente non può più accedere (sessioni esistenti invalidate)
- [ ] Given admin, When riabilita utente, Then l'utente può tornare ad accedere
- [ ] Given utente disabilitato, When tenta login, Then vede messaggio "Account disabilitato, contattare amministratore"
- [ ] Given admin che cerca utente, When usa filtri (nome, email, ruolo, stato), Then vede risultati filtrati con paginazione

### Technical Notes
- Permessi richiesti: `users.read`, `users.create`, `users.edit`, `users.delete`
- Tabelle coinvolte: `Users`, `UserRoles`, `AuditLog`
- API endpoints: `GET/POST /api/users`, `GET/PUT/DELETE /api/users/{id}`
- Email: template di benvenuto con link attivazione account

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Permessi | [ ] Audit log | [ ] Code review

---

## US-006: Definizione Ruoli e Permessi

**Come** amministratore  
**Voglio** creare e configurare ruoli con permessi granulari  
**Per** controllare esattamente cosa ogni utente può vedere e fare nel sistema

**Priorità**: P0 | **Estimation**: L | **Epic**: Core Platform | **Modulo**: Core

### Acceptance Criteria
- [ ] Given admin nella sezione Ruoli, When crea nuovo ruolo con nome e descrizione, Then il ruolo è disponibile per assegnazione agli utenti
- [ ] Given admin, When assegna permessi specifici a un ruolo (es. `anagrafiche.clienti.read`, `anagrafiche.clienti.create`), Then utenti con quel ruolo hanno esattamente quei permessi
- [ ] Given modifica permessi ruolo, When admin salva, Then le modifiche sono effettive immediatamente (no cache stantia)
- [ ] Given ruoli predefiniti (Admin, Manager, Operatore, ReadOnly), When sistema viene installato, Then questi ruoli esistono con permessi standard preconfigurati
- [ ] Given tentativo di accesso non autorizzato, When utente chiama API senza permesso, Then riceve 403 Forbidden

### Technical Notes
- Permessi richiesti: `roles.read`, `roles.create`, `roles.edit`, `roles.delete`
- Tabelle coinvolte: `Roles`, `Permissions`, `RolePermissions`, `UserRoles`
- Pattern: RBAC (Role-Based Access Control) con permessi atomici per risorsa+azione
- Permessi struttura: `{modulo}.{risorsa}.{azione}` es. `core.users.create`

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests (RBAC logic) | [ ] Permessi | [ ] Audit log | [ ] Code review

---

## US-007: Multi-Azienda — Creazione e Gestione Aziende

**Come** amministratore di sistema  
**Voglio** creare e gestire più aziende nel sistema  
**Per** supportare gruppi aziendali o studi professionali che gestiscono più entità

**Priorità**: P0 | **Estimation**: L | **Epic**: Core Platform | **Modulo**: Core

### Acceptance Criteria
- [ ] Given super-admin, When crea nuova azienda con dati obbligatori (ragione sociale, P.IVA, sede legale), Then l'azienda viene creata e è selezionabile al login
- [ ] Given azienda creata, When admin configura dati completi (logo, dati fiscali, firmatario, regime IVA), Then tutti i documenti futuri useranno questi dati
- [ ] Given admin, When modifica un'azienda, Then modifiche salvate e audit loggato
- [ ] Given admin, When disabilita azienda, Then nessun utente può selezionarla al login
- [ ] Given dati fiscali azienda, When sistema genera documenti (fatture, DDT), Then usa sempre i dati dell'azienda corrente nel contesto

### Technical Notes
- Permessi richiesti: `companies.read`, `companies.create`, `companies.edit`
- Tabelle coinvolte: `Companies`, `CompanySettings`, `CompanyFiscalData`
- Tenant isolation: ogni query deve filtrare per `CompanyId`
- Storage logo: file system o blob storage configurabile

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Tenant isolation testata | [ ] Audit log | [ ] Code review

---

## US-008: Selezione Contesto Azienda al Login

**Come** utente con accesso a più aziende  
**Voglio** selezionare l'azienda con cui operare dopo il login  
**Per** lavorare nel contesto corretto senza confusione tra dati di aziende diverse

**Priorità**: P0 | **Estimation**: S | **Epic**: Core Platform | **Modulo**: Core

### Acceptance Criteria
- [ ] Given utente con accesso a una sola azienda, When effettua login, Then viene indirizzato direttamente alla dashboard senza selezione azienda
- [ ] Given utente con accesso a più aziende, When effettua login, Then vede schermata selezione azienda con lista delle aziende autorizzate
- [ ] Given utente nella schermata selezione, When seleziona un'azienda, Then tutto il sistema opera nel contesto di quella azienda
- [ ] Given utente loggato in un'azienda, When vuole cambiare azienda, Then può farlo dal menu utente senza effettuare logout
- [ ] Given cambio azienda, When confermato, Then vengono svuotati tutti i dati in memoria del contesto precedente

### Technical Notes
- Tabelle coinvolte: `Users`, `UserCompanyAccess`, `Companies`
- JWT claim: `company_id` nel token o session state
- UI: dropdown azienda sempre visibile nella navbar

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-009: Audit Log — Tracciamento Operazioni

**Come** amministratore  
**Voglio** visualizzare un audit log completo di tutte le operazioni  
**Per** garantire tracciabilità, conformità normativa e rilevare attività anomale

**Priorità**: P0 | **Estimation**: L | **Epic**: Core Platform | **Modulo**: Core

### Acceptance Criteria
- [ ] Given qualsiasi operazione di Create/Update/Delete, When viene eseguita, Then viene creato un record audit con: user, timestamp, IP, entità, azione, valori prima/dopo
- [ ] Given admin nella sezione Audit Log, When filtra per data/utente/entità/azione, Then vede i record corrispondenti con paginazione
- [ ] Given record audit, When admin lo espande, Then vede il delta completo (campo per campo, valore vecchio → nuovo)
- [ ] Given audit log, When admin esporta periodo selezionato, Then scarica CSV/Excel con tutti i record
- [ ] Given operazione di sistema (background job), When avviene, Then viene loggata con user "SYSTEM"

### Technical Notes
- Permessi richiesti: `audit.read`, `audit.export`
- Tabelle coinvolte: `AuditLog` (append-only, no delete, no update)
- Implementazione: AOP/middleware/decorator che intercetta operazioni
- Retention: configurabile (default 2 anni), archivio separato per storico
- Performance: scrittura asincrona per non impattare operazioni utente

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Permessi | [ ] Performance test | [ ] Code review

---

## US-010: Configurazione Sistema (Parametri Globali)

**Come** amministratore di sistema  
**Voglio** configurare i parametri globali del sistema  
**Per** personalizzare il comportamento dell'ERP per la specifica installazione

**Priorità**: P1 | **Estimation**: M | **Epic**: Core Platform | **Modulo**: Core

### Acceptance Criteria
- [ ] Given admin in sezione Configurazione, When modifica parametri (formato data, decimali, valuta default, lingua), Then il sistema usa i nuovi parametri immediatamente
- [ ] Given configurazione email (SMTP), When admin la imposta e testa, Then il sistema invia email di test con esito visibile
- [ ] Given configurazione backup, When admin imposta schedulazione, Then il sistema esegue backup automatico nei tempi configurati
- [ ] Given parametri cambiati, When admin salva, Then le modifiche sono loggate nell'audit con i valori precedenti e nuovi

### Technical Notes
- Permessi richiesti: `system.config.read`, `system.config.edit`
- Tabelle coinvolte: `SystemSettings`
- Config: chiave-valore typed con versioning

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Permessi | [ ] Audit log | [ ] Code review

---

## US-011: Dashboard Home

**Come** utente loggato  
**Voglio** vedere una dashboard con informazioni rilevanti al mio ruolo  
**Per** avere una panoramica rapida della situazione aziendale e navigare facilmente

**Priorità**: P1 | **Estimation**: M | **Epic**: Core Platform | **Modulo**: Core

### Acceptance Criteria
- [ ] Given utente loggato, When accede alla home, Then vede dashboard con widget contestuali al suo ruolo
- [ ] Given widget configurabili, When utente sposta o nasconde widget, Then il layout viene salvato per le sessioni future
- [ ] Given widget "Accessi Recenti", When utente lo vede, Then mostra gli ultimi 5 documenti/anagrafiche visitati
- [ ] Given admin, When accede alla home, Then vede widget con: utenti attivi, ultime attività, notifiche sistema
- [ ] Given dashboard, When carica, Then tutti i widget si caricano in < 2 secondi anche con DB popolato

### Technical Notes
- Permessi richiesti: dipende dai widget (ogni widget verifica propri permessi)
- Tabelle coinvolte: `UserDashboardConfig`, `RecentActivity`
- Pattern: lazy loading widget, dati mockati se modulo non attivo

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Performance test | [ ] Code review

---

## US-012: Navigazione e Shell Applicativa

**Come** utente  
**Voglio** navigare facilmente tra i moduli del sistema  
**Per** trovare rapidamente le funzionalità di cui ho bisogno

**Priorità**: P1 | **Estimation**: M | **Epic**: Core Platform | **Modulo**: Core

### Acceptance Criteria
- [ ] Given utente loggato, When vede la navigazione principale, Then vede solo i moduli per cui ha almeno un permesso di lettura
- [ ] Given menu principale, When utente espande un modulo, Then vede le sottosezioni disponibili
- [ ] Given navigazione, When utente usa shortcut tastiera (es. Ctrl+K), Then apre ricerca rapida globale
- [ ] Given ricerca rapida, When utente digita, Then vede risultati contestuali (clienti, fornitori, articoli, documenti) in tempo reale
- [ ] Given breadcrumb, When utente è in una schermata annidata, Then vede il percorso completo e può tornare indietro

### Technical Notes
- Menu generato dinamicamente da permessi utente
- Shortcut keyboard: implementare command palette (stile VS Code)
- Responsive: funziona su schermi 1024px+ (target: 1280px+)

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Permessi (menu filtering) | [ ] Code review

---

## US-013: Assegnazione Utenti ad Aziende

**Come** amministratore  
**Voglio** assegnare utenti a una o più aziende con ruoli specifici per azienda  
**Per** gestire correttamente accessi in contesti multi-azienda

**Priorità**: P0 | **Estimation**: M | **Epic**: Core Platform | **Modulo**: Core

### Acceptance Criteria
- [ ] Given admin, When assegna utente a un'azienda con ruolo specifico, Then l'utente può selezionare quella azienda al login con i permessi del ruolo assegnato
- [ ] Given utente con ruoli diversi per aziende diverse, When cambia azienda, Then i permessi cambiano automaticamente secondo il ruolo di quella azienda
- [ ] Given admin, When rimuove utente da un'azienda, Then l'utente non può più selezionare quell'azienda
- [ ] Given utente senza accesso ad alcuna azienda, When tenta login, Then vede messaggio appropriato e contatto admin

### Technical Notes
- Tabelle coinvolte: `UserCompanyAccess`, `Users`, `Companies`, `Roles`
- Modello: User → (Company, Role) → Permissions (per-azienda)

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-014: Profilo Utente e Preferenze

**Come** utente  
**Voglio** visualizzare e modificare il mio profilo e le mie preferenze  
**Per** personalizzare l'esperienza e mantenere i miei dati aggiornati

**Priorità**: P1 | **Estimation**: S | **Epic**: Core Platform | **Modulo**: Core

### Acceptance Criteria
- [ ] Given utente nel proprio profilo, When modifica nome, cognome, email, tel, Then le modifiche vengono salvate
- [ ] Given modifica email, When l'utente la cambia, Then riceve email di conferma al nuovo indirizzo
- [ ] Given sezione preferenze, When utente cambia lingua (IT/EN), Then l'interfaccia si aggiorna nella lingua selezionata
- [ ] Given sezione preferenze, When utente cambia tema (light/dark), Then il tema si applica immediatamente
- [ ] Given sezione sicurezza del profilo, When utente cambia password, Then deve inserire password corrente per conferma

### Technical Notes
- Tabelle coinvolte: `Users`, `UserPreferences`
- Email change: double opt-in con token temporaneo

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-015: Notifiche In-App

**Come** utente  
**Voglio** ricevere notifiche in-app per eventi rilevanti  
**Per** essere informato su approvazioni, scadenze o messaggi senza dover controllare ogni sezione

**Priorità**: P1 | **Estimation**: M | **Epic**: Core Platform | **Modulo**: Core

### Acceptance Criteria
- [ ] Given notifica non letta, When utente guarda l'icona notifiche in header, Then vede badge con contatore
- [ ] Given click su icona notifiche, When si apre pannello, Then vede lista notifiche con icona tipo, titolo, data, link contestuale
- [ ] Given notifica, When utente la clicca, Then viene reindirizzato all'entità correlata e la notifica viene marcata come letta
- [ ] Given notifiche non lette, When utente clicca "Segna tutte come lette", Then tutte vengono marcate lette
- [ ] Given admin, When vuole inviare notifica a tutti gli utenti di un'azienda, Then può creare notifica broadcast

### Technical Notes
- Tabelle coinvolte: `Notifications`, `NotificationRecipients`
- Real-time: WebSocket o SignalR per push immediato
- Tipi: `info`, `warning`, `error`, `success`

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Permessi | [ ] Code review

---

## US-016: Log di Sistema e Monitoraggio

**Come** amministratore di sistema  
**Voglio** visualizzare i log di sistema e gli errori applicativi  
**Per** diagnosticare problemi e monitorare la salute del sistema

**Priorità**: P1 | **Estimation**: M | **Epic**: Core Platform | **Modulo**: Core

### Acceptance Criteria
- [ ] Given admin in sezione Log di Sistema, When filtra per livello (Info/Warning/Error), Then vede i log corrispondenti con timestamp, messaggio, stack trace se disponibile
- [ ] Given errore applicativo, When si verifica, Then viene loggato automaticamente con contesto (user, IP, request, stack trace)
- [ ] Given admin, When vede errori critici non risolti, Then vede badge di allerta nella navbar
- [ ] Given log, When admin esporta, Then scarica file con periodo selezionato

### Technical Notes
- Permessi richiesti: `system.logs.read`
- Implementazione: Serilog o NLog con sink su DB e file
- Livelli: Trace, Debug, Info, Warning, Error, Fatal

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Permessi | [ ] Code review

---

## US-017: Gestione Password — Politica di Scadenza

**Come** amministratore  
**Voglio** configurare la scadenza delle password per utente o per ruolo  
**Per** mantenere la sicurezza degli accessi nel tempo

**Priorità**: P1 | **Estimation**: S | **Epic**: Core Platform | **Modulo**: Core

### Acceptance Criteria
- [ ] Given admin, When imposta scadenza password a 90 giorni, Then tutti gli utenti devono cambiare password ogni 90 giorni
- [ ] Given utente con password in scadenza (< 7 giorni), When accede, Then vede banner di avviso con giorni rimanenti
- [ ] Given password scaduta, When utente accede, Then viene reindirizzato obbligatoriamente al cambio password prima di poter fare qualsiasi altra cosa
- [ ] Given utente che cambia password, When inserisce password già usata (ultimi N), Then viene rifiutata con messaggio "Password già utilizzata"

### Technical Notes
- Tabelle coinvolte: `Users`, `PasswordHistory`, `SecuritySettings`

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Audit log | [ ] Code review

---

## US-018: Import Configurazione Iniziale (Setup Wizard)

**Come** amministratore che installa KBM per la prima volta  
**Voglio** un wizard guidato per la configurazione iniziale  
**Per** completare il setup in modo rapido senza conoscenza tecnica del sistema

**Priorità**: P1 | **Estimation**: L | **Epic**: Core Platform | **Modulo**: Core

### Acceptance Criteria
- [ ] Given prima installazione, When admin accede per la prima volta, Then vede wizard multi-step: Azienda → Admin → Parametri → Completato
- [ ] Given wizard step "Azienda", When admin inserisce i dati fiscali, Then vengono validati (P.IVA check digit per Italia)
- [ ] Given wizard step "Admin", When si crea l'utente amministratore principale, Then la password deve rispettare policy di sicurezza base
- [ ] Given wizard completato, When admin clicca Fine, Then viene indirizzato alla dashboard e il wizard non si ripresenta

### Technical Notes
- Tabelle coinvolte: `Companies`, `Users`, `SystemSettings`
- Wizard: non saltabile finché non completato (primo accesso)

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Code review

---

## US-019: Backup e Restore (Configurazione)

**Come** amministratore  
**Voglio** configurare e avviare backup del database  
**Per** proteggere i dati aziendali e poter ripristinare in caso di problemi

**Priorità**: P2 | **Estimation**: M | **Epic**: Core Platform | **Modulo**: Core

### Acceptance Criteria
- [ ] Given admin in sezione Backup, When configura schedulazione (es. ogni notte alle 2:00), Then il sistema esegue backup automatico
- [ ] Given backup disponibili, When admin visualizza la lista, Then vede data, dimensione, stato (completato/fallito)
- [ ] Given backup completato, When admin avvia restore da file selezionato, Then il sistema chiede conferma esplicita (operazione distruttiva)
- [ ] Given backup fallito, When si verifica, Then admin riceve notifica di sistema con dettaglio errore

### Technical Notes
- Backup: SQL Server native backup (.bak) o mysqldump/pg_dump
- Storage: locale configurabile con path o network share
- Monitoring: alerting su fallimento backup

### Definition of Done
- [ ] Codice implementato | [ ] Unit tests | [ ] Permessi | [ ] Code review

---

## US-020: Internazionalizzazione (i18n) — Italiano e Inglese

**Come** utente  
**Voglio** usare l'interfaccia in italiano o inglese  
**Per** adattare il sistema alla mia lingua preferita

**Priorità**: P1 | **Estimation**: M | **Epic**: Core Platform | **Modulo**: Core

### Acceptance Criteria
- [ ] Given sistema installato, When l'utente seleziona lingua italiana, Then tutta l'interfaccia (label, messaggi, errori, tooltip) è in italiano
- [ ] Given lingua inglese selezionata, When utente naviga, Then tutta l'interfaccia è in inglese
- [ ] Given date e numeri, When visualizzati, Then rispettano il formato locale (IT: dd/mm/yyyy e 1.234,56; EN: mm/dd/yyyy e 1,234.56)
- [ ] Given validazione P.IVA o CF, When eseguita, Then usa regole specifiche per paese (configurabile per multi-nazionale)

### Technical Notes
- Implementazione: resource files (.resx) o JSON locale files
- Default: italiano
- Pattern: ICU message format per plurali e genere

### Definition of Done
- [ ] Codice implementato | [ ] Tutte stringhe UI tradotte (IT+EN) | [ ] Unit tests | [ ] Code review
