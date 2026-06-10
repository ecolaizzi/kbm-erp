# KBM — Authentication

**MFA/TOTP**: posticipato a **fine progetto** (non implementare in Week 1-8).

## Flow

1. `POST /api/auth/login` — username + password
2. Verifica Argon2id hash su `User.PasswordHash`
3. Lockout dopo 5 tentativi (`User.FailedLoginAttempts`)
4. Emette JWT access token (RS256, 15 min) + refresh token (7 giorni)
5. `POST /api/auth/refresh` — rotation refresh token
6. `POST /api/auth/logout` — revoca refresh token

## Password

- **Argon2id** (OWASP 2024) — no bcrypt
- Policy: min 8 char, maiusc, minus, numero (configurabile)

## JWT

- Algoritmo: **RS256**
- Claims: `sub`, `name`, `company_id`, `roles`, `permissions`
- Chiavi RSA in configurazione (dev: file locale; prod: certificato)

## Sessioni

Tabella `RefreshToken` — no tabella `UserSessions` separata.
