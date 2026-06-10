# KBM — Connection Strategy

## Ambienti

| Ambiente | Server | Database | Secrets |
|----------|--------|----------|---------|
| Development | `95.110.142.60` | `kbmdbdev` | User Secrets / `.env` |
| Staging | TBD | `kbmstaging` | CI secrets |
| Production | On-premise IIS | `kbmprod` | Windows Credential / Key Vault |

## Regole

- **Mai** committare password in git
- `appsettings.Development.json` in `.gitignore`
- Template: `appsettings.Development.json.example` con placeholder `<SECRET>`

## Connection string (dev)

```
Server=95.110.142.60;Database=kbmdbdev;User Id=keverdbuser1;Password=<SECRET>;TrustServerCertificate=True;Encrypt=True;
```

## User Secrets (.NET)

```bash
cd src/KBM.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default" "<connection-string>"
```

## Pooling

- Min pool size: 5, Max: 100 (default SqlClient)
- Retry: Polly per transient errors in API startup
