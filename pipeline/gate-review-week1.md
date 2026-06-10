# Gate Review Week 1 — KBM Supervisor

**Data**: 2026-06-09  
**Batch**: A, B, C, D  
**Esito**: GO

## Checklist Definition of Done

| Criterio | Stato |
|----------|-------|
| US-INFRA-01 Solution + CI | OK |
| US-INFRA-02 Migration Core + SystemSettings | OK |
| US-001 Login JWT Argon2id | OK |
| US-003 Logout refresh token | OK (API) |
| US-012 WPF shell + login | OK |
| US-018 Setup Wizard API + WPF | OK |
| QA smoke tests (3) | OK |
| Ralph pipeline running | OK |
| MFA/TOTP | Out of scope (fine progetto) |

## Test eseguiti

- `dotnet build KBM.slnx -c Release` — OK
- `dotnet test` — integration smoke auth OK
- `POST /api/auth/login` admin/Admin123! — OK (DB dev)

## Note

- JWT dev: symmetric key (RS256 in roadmap produzione)
- Setup wizard: 2 step WPF (Azienda + Admin)
- Prossima settimana: Batch Week 2 RBAC + multi-azienda
