# Sincronizza state.json con Week 2 (batch E-H, task, eventi puliti)
$StatePath = Join-Path $PSScriptRoot "state.json"
$now = (Get-Date).ToUniversalTime().ToString("o")

$state = @{
    version         = 1
    updatedAt       = $now
    project         = "KBM"
    phase           = 1
    week            = 2
    currentBatch    = "F"
    overallProgress = 65
    ralphLoop       = @{
        iteration     = 4
        status        = "running"
        lastAction    = "Batch F - WPF admin + smoke RBAC"
        lastHeartbeat = $now
    }
    agents          = @(
        @{ id = "supervisor"; name = "Supervisor"; layer = 0; role = "orchestrator"; status = "active"; currentTask = "Batch F orchestration"; progress = 65 }
        @{ id = "product-owner"; name = "Product Owner"; layer = 1; role = "design"; status = "idle"; currentTask = $null; progress = 100 }
        @{ id = "chief-architect"; name = "Chief Architect"; layer = 1; role = "design"; status = "done"; currentTask = $null; progress = 100 }
        @{ id = "database-architect"; name = "Database Architect"; layer = 1; role = "design"; status = "done"; currentTask = "RBAC migration"; progress = 100 }
        @{ id = "security-architect"; name = "Security Architect"; layer = 1; role = "design"; status = "done"; currentTask = $null; progress = 100 }
        @{ id = "ux-designer"; name = "UX Designer"; layer = 1; role = "design"; status = "idle"; currentTask = $null; progress = 100 }
        @{ id = "scouting"; name = "Scouting"; layer = 1; role = "design"; status = "done"; currentTask = $null; progress = 100 }
        @{ id = "devops"; name = "DevOps"; layer = 2; role = "code"; status = "done"; currentTask = "US-INFRA-01"; progress = 100 }
        @{ id = "backend"; name = "Backend Coder"; layer = 2; role = "code"; status = "done"; currentTask = "US-005/006/007/009"; progress = 100 }
        @{ id = "frontend"; name = "Frontend Coder"; layer = 2; role = "code"; status = "active"; currentTask = "US-005-ui WPF utenti"; progress = 30 }
        @{ id = "module-coder"; name = "Module Coder"; layer = 2; role = "code"; status = "idle"; currentTask = $null; progress = 0 }
        @{ id = "qa"; name = "QA Agent"; layer = 2; role = "code"; status = "pending"; currentTask = "smoke-rbac-users"; progress = 0 }
        @{ id = "documentation"; name = "Documentation"; layer = 2; role = "code"; status = "done"; currentTask = $null; progress = 100 }
    )
    batches         = @(
        @{ id = "A"; status = "done"; depends = @(); tasks = @("devops:US-INFRA-01") }
        @{ id = "B"; status = "done"; depends = @("A"); tasks = @("backend:US-001", "frontend:US-012") }
        @{ id = "C"; status = "done"; depends = @("B"); tasks = @("backend:US-018", "qa:smoke-auth") }
        @{ id = "D"; status = "done"; depends = @("C"); tasks = @("supervisor:gate-review-w1") }
        @{ id = "E"; status = "done"; depends = @("D"); tasks = @("database-architect:RBAC", "backend:US-006", "backend:US-009") }
        @{ id = "F"; status = "in_progress"; depends = @("E"); tasks = @("frontend:US-005-ui", "qa:smoke-rbac") }
        @{ id = "G"; status = "pending"; depends = @("F"); tasks = @("frontend:US-007-ui", "frontend:US-008-ui") }
        @{ id = "H"; status = "pending"; depends = @("G"); tasks = @("supervisor:gate-review-w2") }
    )
    handoffs        = @(
        @{ id = "h3"; from = "devops"; to = "backend"; artifact = "src/ scaffold"; status = "completed"; at = "2026-06-09T12:52:00Z" }
        @{ id = "h5"; from = "backend"; to = "frontend"; artifact = "POST /api/auth/login"; status = "completed"; at = "2026-06-09T12:55:00Z" }
        @{ id = "h6"; from = "backend"; to = "qa"; artifact = "auth endpoints"; status = "completed"; at = "2026-06-09T12:58:00Z" }
        @{ id = "h7"; from = "database-architect"; to = "backend"; artifact = "RBAC migration + seed"; status = "completed"; at = "2026-06-09T13:01:00Z" }
        @{ id = "h8"; from = "backend"; to = "frontend"; artifact = "Users/Companies/Roles API"; status = "in_progress"; at = $now }
    )
    tasks           = @(
        @{ id = "US-INFRA-01"; title = "Repository + CI/CD"; agent = "devops"; status = "done"; week = 1 }
        @{ id = "US-001"; title = "Login JWT"; agent = "backend"; status = "done"; week = 1 }
        @{ id = "US-012"; title = "WPF navigation shell"; agent = "frontend"; status = "done"; week = 1 }
        @{ id = "US-018"; title = "Setup Wizard"; agent = "backend"; status = "done"; week = 1 }
        @{ id = "US-005"; title = "CRUD Utenti"; agent = "backend"; status = "done"; week = 2 }
        @{ id = "US-006"; title = "RBAC Ruoli"; agent = "backend"; status = "done"; week = 2 }
        @{ id = "US-007"; title = "Multi-Azienda"; agent = "backend"; status = "done"; week = 2 }
        @{ id = "US-008"; title = "Selezione azienda"; agent = "backend"; status = "done"; week = 2 }
        @{ id = "US-009"; title = "Audit Log"; agent = "backend"; status = "done"; week = 2 }
        @{ id = "US-005-ui"; title = "WPF gestione utenti"; agent = "frontend"; status = "in_progress"; week = 2 }
        @{ id = "smoke-rbac"; title = "Smoke test RBAC"; agent = "qa"; status = "pending"; week = 2 }
    )
    events          = @(
        @{ at = $now; type = "resume"; agent = "supervisor"; message = "Monitor Ralph ripristinato - Week 2 Batch F" }
        @{ at = $now; type = "batch"; agent = "supervisor"; message = "Batch E -> done" }
        @{ at = $now; type = "batch"; agent = "supervisor"; message = "Batch F -> in_progress" }
        @{ at = $now; type = "handoff"; agent = "backend"; message = "backend -> frontend: Users API (in_progress)" }
        @{ at = "2026-06-09T12:58:03Z"; type = "log"; agent = "supervisor"; message = "Gate Review Week 1 GO" }
        @{ at = "2026-06-09T12:58:03Z"; type = "progress"; agent = "supervisor"; message = "Overall: 55%" }
        @{ at = $now; type = "heartbeat"; agent = "system"; message = "Monitor attivo" }
    )
}

$json = $state | ConvertTo-Json -Depth 12
[System.IO.File]::WriteAllText($StatePath, $json + "`n", [System.Text.UTF8Encoding]::new($false))
Write-Host "state.json sincronizzato - Week 2 Batch F"
