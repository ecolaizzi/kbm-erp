# Gate Review Week 2 — KBM Supervisor

**Data**: 2026-06-09  
**Batch**: E, F, G  
**Esito**: GO (con riserve minori)

## Deliverable

| US | Descrizione | Stato |
|----|-------------|-------|
| US-005 | CRUD Utenti API | OK |
| US-006 | RBAC Ruoli + permessi | OK |
| US-007 | Multi-Azienda API | OK |
| US-008 | Selezione contesto azienda | OK (API + WPF combo) |
| US-009 | Audit Log interceptor | OK |
| US-003 | Logout | OK (API + WPF) |
| US-005-ui | WPF lista utenti | OK |
| US-007-ui | WPF lista aziende | OK |
| US-008-ui | Cambio azienda shell | OK |
| smoke-rbac | Test integrazione RBAC | OK (2 test) |

## Out of scope Week 2

- US-004 Reset password email — Week 3+
- MFA/TOTP — fine progetto
- CRUD utenti/aziende form WPF (solo liste read-only MVP)

## Test

- `dotnet build KBM.slnx -c Release` — OK
- `dotnet test` — 6/6 integration tests OK

## Riserve

- Password policy solo su create user (no reset flow)
- Audit log: scrittura OK, UI consultazione non ancora in WPF

## Prossimo: Week 3 Anagrafiche + Clienti
