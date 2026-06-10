# KBM — Solution Architecture

**Versione**: 1.0 | **Owner**: Chief Architect | **Client**: WPF Desktop

## Pattern

**Modular Monolith** con Clean Architecture per modulo.

```
┌─────────────┐     HTTPS/REST      ┌──────────────────────────────────┐
│ KBM.Client  │ ◄─────────────────► │ KBM.Api (ASP.NET Core)           │
│ WPF .NET 8  │                     │  ├─ KBM.Application (MediatR)    │
└─────────────┘                     │  ├─ KBM.Domain                   │
                                    │  └─ KBM.Infrastructure.Persistence│
                                    └──────────────┬───────────────────┘
                                                   │ EF Core + Dapper
                                    ┌──────────────▼───────────────────┐
                                    │ SQL Server (kbmdbdev / prod)     │
                                    └──────────────────────────────────┘
```

## Moduli

| Modulo | Progetto | Week |
|--------|----------|------|
| Core Platform | `KBM.Modules.Core` | W1-2 |
| Anagrafiche | `KBM.Modules.Anagraphics` | W3-4 |
| Ciclo Attivo | `KBM.Modules.Sales` | W5-6 |

## Principi

- API-first: client WPF oggi, web/mobile futuro via stesse API
- Multi-tenancy: `CompanyId` discriminator + EF global filter
- Optimistic concurrency: `RowVersion`
- Soft delete su entità dominio

## Deployment

IIS on-premise (target PMI italiane). Docker opzionale dev.
