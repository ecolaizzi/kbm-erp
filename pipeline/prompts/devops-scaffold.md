# Prompt DevOps — US-INFRA-01

Crea solution KBM Modular Monolith:

- `src/KBM.Domain` (class lib)
- `src/KBM.Application` (class lib, ref Domain)
- `src/KBM.Infrastructure.Persistence` (class lib, EF Core)
- `src/KBM.Api` (ASP.NET Core Web API)
- `src/KBM.Client` (WPF)
- `src/KBM.Modules.Core` (class lib)
- `tests/KBM.UnitTests`, `tests/KBM.IntegrationTests`

Aggiungi CI `.github/workflows/ci.yml`, `Directory.Build.props`, `appsettings.Development.json.example`.

Aggiorna Ralph:
```
node pipeline/scripts/pipeline-cli.mjs status devops active "US-INFRA-01"
node pipeline/scripts/pipeline-cli.mjs handoff devops backend "src/ scaffold" completed
```
