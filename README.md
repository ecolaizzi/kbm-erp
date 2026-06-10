# KBM (Kever Business Manager)

**Enterprise ERP System for Italian SMEs**

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2019%2B-red.svg)](https://www.microsoft.com/sql-server)

---

## 📋 Overview

**KBM** è un ERP enterprise modulare progettato per PMI italiane (10-200 dipendenti). Combina la **potenza** di sistemi enterprise con la **semplicità** d'uso, eliminando la complessità inutile.

**Timeline**: MVP 2 mesi (8 settimane) - **Fase 0 completata** ✅

---

## 🎯 Value Proposition

> **"ERP Enterprise per PMI: Potenza senza Complessità"**

- ✅ **Moduli production-ready**: Core Platform, Anagrafiche, Ciclo Attivo (Vendite)
- ✅ **Fatturazione Elettronica** (SDI) integrata
- ✅ **Multi-azienda** nativa (gestione più società in un'unica installazione)
- ✅ **Desktop nativo Windows** (RDP/Citrix optimized per lavoro remoto)
- ✅ **API-first** (integrazioni + client web futuro)

---

## 🏗️ Architecture

**Pattern**: Modular Monolith + Clean Architecture  
**Backend**: .NET 8 LTS + ASP.NET Core Web API  
**Database**: Microsoft SQL Server 2019+  
**Client**: WPF (.NET 8) - Desktop nativo Windows  
**ORM**: EF Core 8 (Code First) + Dapper (performance queries)  

**Key Features**:
- Multi-tenancy via `CompanyId` discriminator
- RBAC (Role-Based Access Control) granulare
- Audit log immutabile
- Optimistic concurrency (RowVersion)
- Soft delete con filtered unique constraints

---

## 📦 Moduli

### ✅ Production-Ready (MVP 2 mesi)

1. **Core Platform** (Week 1-2)
   - Autenticazione JWT + Refresh Token
   - Gestione utenti e ruoli (RBAC)
   - Multi-azienda (tenant isolation)
   - Audit log completo

2. **Anagrafiche** (Week 3-4)
   - Clienti (con P.IVA, CF, SDI, PEC)
   - Fornitori (con coordinate bancarie)
   - Articoli (con listini, codici alternativi)
   - Listini prezzi

3. **Ciclo Attivo - Vendite** (Week 5-6)
   - Offerte
   - Ordini Cliente
   - DDT (Documento di Trasporto)
   - Fatture Attive + **SDI** (Fatturazione Elettronica)
   - Scadenzario base

### 🎨 UI Demo-Ready (Week 7-8)

- Ciclo Passivo (Acquisti)
- Magazzino
- Contabilità
- CRM

UI completo navigabile con dati mock, backend riservato a fasi successive.

---

## 📚 Documentation

Tutti i deliverable di **Fase 0** (Research & Foundation) sono in [`/docs`](./docs):

- [`/docs/architecture`](./docs/architecture) - Solution architecture, ADR, tech stack
- [`/docs/database`](./docs/database) - Schema SQL, ERD, migrations strategy
- [`/docs/security`](./docs/security) - Auth model, RBAC, threat model (report)
- [`/docs/ux`](./docs/ux) - Guidelines, wireframes, component library
- [`/docs/product`](./docs/product) - Vision, roadmap, backlog (80 user stories)
- [`/docs/research`](./docs/research) - Competitive analysis, workflows, glossario
- [`/docs/governance`](./docs/governance) - Decision log, ownership matrix

**Pipeline multi-agente**: [`AGENTS.md`](./AGENTS.md) + [`.cursor/rules/`](./.cursor/rules/) — monitoraggio live [`pipeline/ralph/dashboard.html`](./pipeline/ralph/dashboard.html)

---

## 🚀 Quick Start

```powershell
# Build
dotnet build KBM.slnx -c Release

# Monitoraggio pipeline multi-agente (Ralph)
.\pipeline\ralph\serve.ps1
# → http://localhost:8765/pipeline/ralph/dashboard.html

# DB dev: copia .env.example → .env, poi User Secrets su KBM.Api
```

Vedi [`AGENTS.md`](./AGENTS.md) per orchestrazione agenti Cursor.

---

## 📅 Roadmap 2 Mesi

| Week | Focus | Deliverable |
|---|---|---|
| **W1-2** | Core Platform | Auth, Users, Roles, Multi-tenancy |
| **W3-4** | Anagrafiche | Clienti, Fornitori, Articoli, Listini |
| **W5-6** | Ciclo Attivo | Ordini, DDT, Fatture + SDI |
| **W7-8** | UI + Polish | Demo UI altri moduli, performance tuning |

Dettagli: [`docs/product/roadmap-2months.md`](./docs/product/roadmap-2months.md)

---

## 🛠️ Technology Stack

- **Language**: C# 12 (.NET 8)
- **Backend**: ASP.NET Core Web API, MediatR, FluentValidation
- **Database**: SQL Server 2019+, EF Core 8, Dapper
- **Client**: WPF (.NET 8)
- **Testing**: xUnit, Testcontainers
- **Logging**: Serilog
- **API**: REST + OpenAPI/Swagger

Rationale completo: [`docs/architecture/technology-stack.md`](./docs/architecture/technology-stack.md)

---

## 🎨 Design System

- **UI Framework**: WPF Material Design inspired
- **Design Principles**: Productivity First, Data Density, Keyboard-First, RDP-Optimized
- **Component Library**: 15 componenti reusable (KBMDataGrid, KBMForm, KBMLookup...)
- **Accessibility**: WCAG 2.1 AA compliant

Dettagli: [`docs/ux/guidelines.md`](./docs/ux/guidelines.md)

---

## 📊 Project Status

**Current Phase**: ✅ **Fase 0 - Research & Foundation (COMPLETATA)**

- [x] Competitive scouting (6 ERP competitor analizzati)
- [x] Product vision & backlog (80 user stories)
- [x] Solution architecture (C4 L1/L2/L3, 12 ADR)
- [x] Database schema (completo 13 moduli, migrations 8-week)
- [x] Security model (JWT, RBAC, audit, threat STRIDE)
- [x] UX design (10 wireframes, 15 componenti, style guide)
- [x] Roadmap 2 mesi (week-by-week)

**Next Phase**: Fase 1 - Core Platform Development (Week 1-2)

---

## 📄 License

MIT License - see [LICENSE](LICENSE) file for details.

---

## 🤝 Contributing

_(Coming soon)_

---

## 📧 Contact

**Project Owner**: Edwin Colaizzi  
**Email**: edwin.colaizzi@gmail.com

---

**Built with ❤️ for Italian SMEs**
