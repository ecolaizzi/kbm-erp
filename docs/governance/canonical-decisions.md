# KBM — Decisioni Canoniche

**Versione**: 1.0 | **Data**: 2026-06-09 | **Owner**: Supervisor

Fonte di verità per tutti gli agenti Cursor. Sovrascrive testi contraddittori nei documenti legacy.

| Area | Decisione |
|------|-----------|
| Client UI | **WPF (.NET 8)** desktop Windows, RDP-optimized, palette `docs/ux/style-guide.md` |
| Architettura | Modular Monolith + Clean Architecture per modulo, REST API-first |
| Backend | .NET 8, ASP.NET Core, EF Core + Dapper, SQL Server 2019+ |
| Password | **Argon2id** (no bcrypt) |
| JWT | **RS256** + Refresh Token rotation |
| MFA/TOTP | **Fine progetto** — fuori scope Week 1-8 |
| Timeline | **2 mesi** — `roadmap-2months.md` sostituisce `roadmap.md` e `mvp-scope.md` |
| Migration | Incrementale per settimana — W1 solo Core Platform |
| Pipeline | **100% Cursor** — `experts/` deprecato |
| Ruoli sistema | Admin, Manager, Operatore, ReadOnly |
| DB dev | `95.110.142.60` / `kbmdbdev` — credenziali in User Secrets |

## Documenti superseded

- `docs/product/roadmap.md` → usa `roadmap-2months.md`
- `docs/product/mvp-scope.md` → usa `scope-2months.md`

## Monitoraggio

Pipeline Ralph: `pipeline/ralph/dashboard.html`
