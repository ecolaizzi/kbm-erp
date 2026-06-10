# KBM ↔ NTS Business Cube — Mappa di parità funzionale

**Versione**: 1.0
**Data**: 2026-06-10
**Owner**: Product Owner / Chief Architect
**Fonte di riferimento**: [Guida Analitica Business Cube](https://servizi.ntsinformatica.it/BusHelpWs/helpnet2017sr5/html/gaguidaan.htm)

> Obiettivo: riutilizzare le **logiche gestionali consolidate** di Business Cube per
> raggiungere (e poi superare) la parità funzionale, senza copiarne la UI ma adottandone
> le logiche di dominio che "funzionano". Questo documento mappa ogni macro-area della
> Guida Analitica sul nostro stato e definisce l'approccio implementativo.

## Legenda stato

| Stato | Significato |
|-------|-------------|
| ✅ Done | Implementato end-to-end (dominio + API + client) |
| 🟦 Partial | Presente ma incompleto / solo alcune funzioni |
| 🟨 Planned-Next | Prossimo incremento (questa fase / fase successiva) |
| ⬜ Backlog | A roadmap, non ancora iniziato |

---

## 1. Tabelle e Archivi (`subm0100`)

Archivi di base **riusati da tutti i moduli**: sono la fondazione delle "logiche che
funzionano" (anagrafiche normalizzate invece di campi liberi).

| Funzione Business Cube | KBM | Stato | Note |
|---|---|---|---|
| Anagrafica Clienti | Clienti (ANACL) | ✅ | indirizzi/contatti/banche, condizioni |
| Anagrafica Fornitori | Fornitori (ANAFO) | ✅ | |
| Anagrafica Articoli | Articoli (ANART) | ✅ | categorie, UM, prezzi, IVA |
| **Condizioni di pagamento** | Condizioni di pagamento (PAG) | 🟦→✅ | **primo incremento di questa fase** — rate, giorni, fine mese, scadenziere |
| **Codici IVA** | Codici IVA (IVA) | 🟨 | aliquota, natura esenzione, indeducibilità |
| **Unità di misura** | UM (UM) | 🟨 | con UM secondarie e fattori di conversione |
| **Zone / aree commerciali** | Zone (ZONE) | 🟨 | per clienti/agenti/statistiche |
| **Causali (magazzino/contabili)** | Causali (CAU) | ⬜ | guida carico/scarico e segno contabile |
| Banche / ABI-CAB | Banche | 🟦 | dati banca già su cliente/fornitore, manca tabella centrale |
| Nazioni / Province / Comuni | Località | ⬜ | normalizzazione indirizzi |
| Valute e cambi | Valute | ⬜ | abilita multivaluta dei report polivalenti |
| Listini | Listini (LIST) | ⬜ | nav presente disabilitata |

**Approccio**: introdurre le tabelle come entità tenant-scoped autonome (additive, senza
rompere i campi liberi esistenti su Cliente/Articolo). I documenti futuri le referenzieranno;
le anagrafiche verranno gradualmente migrate da campo libero a FK.

## 2. Gestione Ordini (`subm0300`)

| Funzione | KBM | Stato | Note |
|---|---|---|---|
| Richieste di acquisto (RDA) | RDA | ✅ | righe, fornitori per riga, totali, lock per stato |
| Richieste di offerta (RDO) | RDO | ✅ | |
| Ordini clienti (OV) | Ordini cliente (OV) | ✅ | testata+riga, totali, stati evasione, tabelle base collegate |
| Ordini fornitori (ODA) | Ordini fornitore (ODA) | ✅ | da RDA (`from-purchase-request`), stessa struttura righe |
| Archivio listini | Listini (LIST) | 🟦 | API + schema; UI lista prossimo incremento |
| Evasione ordini / stato riga | — | 🟦 | campi q.tà evasa/fatturata su riga; DDT/magazzino ⬜ |

## 3. Vendite / Magazzino (`subm0400`)

| Funzione | KBM | Stato | Note |
|---|---|---|---|
| Giacenze / disponibilità | Magazzino (MAG) | 🟨 | **fondamentale per inventario e report (pc10)** |
| Movimenti di carico/scarico | — | 🟨 | guidati da causali |
| DDT / Bolle | — | ⬜ | |
| Fatturazione attiva | — | ⬜ | |
| Valorizzazione (LIFO/FIFO/Medio) | — | ⬜ | Fase 5 |

## 4. Contabilità (`subm0200`) e Contab. analitica (`subm1100`/`subm2200`)

| Funzione | KBM | Stato |
|---|---|---|
| Piano dei conti (sistema dei mastri) | ✅ | Mastro→Conto→Sottoconto, natura/segno a cascata, tipo conto (cliente/fornitore/banca/cassa/IVA), Bil. CEE, seed piano standard |
| Prima nota | ⬜ | Fase 6 |
| Registri / Liquidazione IVA | ⬜ | Fase 6 |
| Scadenzario | ⬜ | abilitato da Condizioni di pagamento |
| Centri di costo / Budget | ⬜ | estensione |

## 5. Aree estese (parità a lungo termine)

| Area Guida Analitica | KBM | Stato |
|---|---|---|
| Agenti e Provvigioni (`subm0500`) | ⬜ Backlog |
| Statistiche / Datawarehouse (`subm0600`) | 🟦 export griglie + report interni (pc07/pc10) |
| Produzione / D.B. (`subm0700`) | ⬜ Fase 7 |
| Import/Export Dati (`subm0800`) | 🟦 export xlsx/csv/html; manca import |
| Sistema Qualità (`subm0900`) | ⬜ |
| Project Management (`subm1000`) | ⬜ |
| Cespiti (`subm1200`) | ⬜ |
| Ritenute d'acconto (`subm1300`) | ⬜ |
| Transazioni Intracomunitarie (`subm1400`) | ⬜ |
| Tesoreria (`subm1500`) | ⬜ |
| Business CRM (`subm1600`) | ⬜ Fase 7 |
| Customer Service (`subm1800`) | ⬜ |
| Parcellazione (`subm1900`) | ⬜ |
| Punti Vendita / POS (`subm2000`) | ⬜ |
| E-mail integrata (`subm2100`) | 🟦 motore stampa unione presente |
| Stampe Parametriche (`subm3000`) | 🟦 framework report pluggable + anteprima interna |
| Utility (`subm3100`) | 🟦 personalizzazione griglie, layout, developer settings |
| Workflow (`subm3200`) | ✅ Modulo Workflow: modelli (step/approvazioni/decisioni), motore istanze+task, Consolle, integrazione moduli (azioni su RDA/RDO) |

---

## Logiche gestionali "che funzionano" da riutilizzare subito

Estratte dalla guida e prioritizzate perché **abilitano** molti moduli a valle:

1. **Archivi di base normalizzati** (Tabelle e Archivi) — condizioni di pagamento, codici
   IVA, UM, zone, causali. Senza questi i documenti restano campi liberi non coerenti.
2. **Condizioni di pagamento con scadenziere** — generazione automatica delle rate
   (n. rate, giorni, intervallo, fine mese) → riutilizzabile da fatture e scadenzario.
3. **Causali documento/magazzino** — un'unica tabella che pilota segno (+/-), aggiornamento
   giacenza e impatto contabile: è il cuore della coerenza del gestionale.
4. **Stato riga e evasione** — ordini e RDA/RDO con avanzamento per riga (aperto/evaso/
   parziale): logica già impostata su RDA, da estendere come pattern comune.

## Incremento corrente (questa iterazione)

Implementato **Condizioni di pagamento (PAG)** come prima tabella della macro-area
"Tabelle e Archivi", end-to-end (dominio → API → client) e completa di **logica di
scadenziere** (`PaymentTerm.BuildSchedule`) riutilizzabile dai documenti futuri.
Le altre tabelle base (Codici IVA, UM, Zone, Causali) seguono lo stesso pattern.
