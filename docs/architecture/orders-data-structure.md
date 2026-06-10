# Struttura dati modulo Ordini (parità NTS ambsor02)

**Fonte**: [AM: La struttura dei dati del modulo ordini](https://servizi.ntsinformatica.it/BusHelpWs/helpnet2017sr5/html/ambsor02.htm)
**Data**: 2026-06-10

## Principio

Business Cube non parte da zero: il modulo ordini si appoggia su **tabelle base** condivise e su **archivi** anagrafici/non anagrafici. KBM replica questa separazione per riusare la stessa logica gestionale.

```
TABELLE BASE (tenant-scoped)     ARCHIVI ANAGRAFICI        ARCHIVI NON ANAGRAFICI
────────────────────────────     ───────────────────       ────────────────────────
Pagamenti (PAG) ✅               Clienti (ANACL) ✅        Listini (LIST) ✅
Codici IVA (IVA) ✅              Fornitori (ANAFO) ✅      Sconti ⬜
Magazzini (MAG) ✅               Articoli (ANART) ✅      Ordini OV/ODA ✅
Causali magazzino (CAU) ✅                                 Proposte ordine ⬜
Vettori, Tipi porto ✅
Tipi bolla/fattura ✅
Zone, UM, Valute ✅
Mastri contabilità (PDC) ✅
Numerazione OV/ODA ✅
```

## Tabelle implementate

| Tabella NTS | Entità KBM | API |
|---|---|---|
| Pagamenti | `PaymentTerm` | `/api/payment-terms` |
| Codici IVA | `VatCode` | `/api/vat-codes` |
| Unità di misura | `UnitOfMeasure` | `/api/order-lookups/units` |
| Zone | `Zone` | `/api/order-lookups/zones` |
| Magazzini/depositi | `Warehouse` | `/api/warehouses` |
| Causali di magazzino | `WarehouseReason` | `/api/warehouses/reasons` |
| Vettori | `Carrier` | `/api/order-lookups/carriers` |
| Tipi porto | `PortType` | `/api/order-lookups/port-types` |
| Tipi bolle/fatture | `DocumentType` | `/api/order-lookups/document-types` |
| Valute estere | `Currency` | `/api/order-lookups/currencies` |
| Mastri contabilità | `AccountMaster` | `/api/chart-accounts` |

Seed standard disponibile per: IVA, UM, valuta EUR, tipi porto, tipi documento OV/ODA, magazzino principale, causali VEN/ACQ.

## Archivi ordini

### Ordine cliente (OV) — `SalesOrder` + `SalesOrderLine`

Testata collegata a: Cliente, indirizzi, listino, condizioni pagamento, magazzino, valuta, vettore, tipo porto, tipo documento.

Riga con: articolo, quantità ordinata/evasa/fatturata, UM, codice IVA, prezzo, sconto, totali calcolati, stato riga (evasione parziale).

Stati documento: Bozza → Confermato → Parzialmente evaso → Evaso → Fatturato → Annullato.

### Ordine fornitore (ODA) — `PurchaseOrder` + `PurchaseOrderLine`

Stessa struttura del ciclo passivo; collegamento opzionale a RDA (`PurchaseRequestId`).
Generazione da RDA: `POST /api/purchase-orders/from-purchase-request/{id}`.

### Listino — `PriceList` + `PriceListLine`

Archivio prezzi per articolo (vendita/acquisto), con validità temporale e valuta.

## Logiche riutilizzabili

1. **Snapshot su riga** — prezzo, IVA e descrizione copiati al momento dell'ordine (non retroattivi).
2. **Totali** — calcolo riga (q.tà × prezzo × sconto × IVA) + sconto testata; `OrderTotalsHelper`.
3. **Evasione per riga** — `OrderedQuantity` / `DeliveredQuantity` / `ReceivedQuantity` / `InvoicedQuantity` abilitano DDT e fatture future.
4. **Numerazione** — `OV/AAAA/####` e `ODA/AAAA/####` per azienda/anno.

## Prossimi incrementi

- Proposte d'ordine (offerte/preventivi)
- Archivio sconti e classi sconto
- FK da anagrafiche (Cliente.Listino → `PriceListId`) al posto dei campi liberi
- DDT e movimenti magazzino guidati da `WarehouseReason`
