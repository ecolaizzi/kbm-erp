# KBM — Authorization (RBAC)

## Permission format

`{modulo}.{risorsa}.{azione}` — es. `core.users.create`

## System roles

Admin, Manager, Operatore, ReadOnly

## Permessi acquisti (ciclo passivo)

| Permesso | Descrizione |
|----------|-------------|
| `purchasing.rda.read/create/edit/delete` | Richieste di acquisto (RDA) |
| `purchasing.rdo.read/create/edit/delete` | Richieste di offerta (RDO) |

Manager: tutti. Operatore: read/create/edit. ReadOnly: solo read.

## Permessi modalita sviluppatore (gesture-gated)

| Permesso | Descrizione |
|----------|-------------|
| `system.developer.access` | Apertura della finestra impostazioni sviluppatore |
| `system.config.read` | Lettura configurazioni azienda/tecniche e report |
| `system.config.edit` | Modifica configurazioni e definizioni report |

Questi permessi sono assegnati **solo al ruolo Admin**. L'accesso alle configurazioni
avviene tramite gesture nascosta (Ctrl+Shift + 3 click sul logo) seguita da una verifica
server-side (`GET /api/system-config/can-access`): senza permesso la finestra non si apre
e non viene mostrata alcuna traccia.

## 6-layer pipeline

1. Authentication (JWT valid)
2. Tenant context (`CompanyId` from claim)
3. Route authorization
4. Permission check (RBAC)
5. Resource-level check (ownership)
6. Audit log write

## Seed

Vedi `permission-catalog.md` e `permission-seed.sql`.
