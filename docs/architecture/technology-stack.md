# KBM — Technology Stack

| Layer | Tecnologia | Versione |
|-------|------------|----------|
| Runtime | .NET | 8.0 LTS |
| API | ASP.NET Core Web API | 8.x |
| Client | **WPF** | 8.x |
| ORM | EF Core + Dapper | 8.x |
| Database | SQL Server | 2019+ |
| Messaging | MediatR | 12.x |
| Validation | FluentValidation | 11.x |
| Logging | Serilog | 4.x |
| Auth | JWT RS256, Argon2id | — |
| Testing | xUnit, Testcontainers | — |

**Non in scope MVP**: MFA/TOTP (fine progetto), Blazor/React client.

Rationale: stack standard ERP italiano, RDS-friendly WPF, rapid development con scaffolding.
