# KBM Pipeline CLI — aggiorna pipeline/ralph/state.json (UTF-8)
# Usage: .\pipeline-cli.ps1 <command> [arg1] [arg2] ...
param(
    [Parameter(Mandatory, Position = 0)][string]$Command,
    [Parameter(ValueFromRemainingArguments = $true)][string[]]$Rest
)

$StatePath = Join-Path $PSScriptRoot "..\ralph\state.json"
$utf8 = [System.Text.UTF8Encoding]::new($false)

function Read-State {
    if (-not (Test-Path $StatePath)) { throw "state.json not found: $StatePath" }
    $raw = [System.IO.File]::ReadAllText($StatePath, $utf8)
    return $raw | ConvertFrom-Json
}

function Write-State($state) {
    $state.updatedAt = (Get-Date).ToUniversalTime().ToString("o")
    $json = $state | ConvertTo-Json -Depth 12 -Compress:$false
    for ($i = 0; $i -lt 8; $i++) {
        try {
            [System.IO.File]::WriteAllText($StatePath, $json + "`n", $utf8)
            return $state.updatedAt
        } catch {
            if ($i -eq 7) { throw }
            Start-Sleep -Milliseconds 250
        }
    }
}

$state = Read-State

function Add-Event($type, $agent, $message) {
    $ev = [PSCustomObject]@{
        at      = (Get-Date).ToUniversalTime().ToString("o")
        type    = $type
        agent   = $agent
        message = $message
    }
    $state.events = @($ev) + @($state.events)
    if ($state.events.Count -gt 100) { $state.events = $state.events[0..99] }
    if (-not $state.ralphLoop) { $state.ralphLoop = [PSCustomObject]@{} }
    $state.ralphLoop.lastAction = $message
}

function Get-Batch($id) {
    $b = $state.batches | Where-Object { $_.id -eq $id } | Select-Object -First 1
    if (-not $b) {
        $b = [PSCustomObject]@{ id = $id; status = 'pending'; depends = @(); tasks = @() }
        $state.batches = @($state.batches) + @($b)
    }
    return $b
}

switch ($Command) {
    'status' {
        if ($Rest.Count -lt 2) { throw "Usage: status <agentId> <status> [task...]" }
        $agent = $state.agents | Where-Object { $_.id -eq $Rest[0] }
        if (-not $agent) { throw "Agent not found: $($Rest[0])" }
        $agent.status = $Rest[1]
        if ($Rest.Count -gt 2) { $agent.currentTask = ($Rest[2..($Rest.Count - 1)] -join ' ') }
        Add-Event 'agent_status' $Rest[0] "$($agent.name) -> $($Rest[1])"
    }
    'handoff' {
        if ($Rest.Count -lt 4) { throw "Usage: handoff <from> <to> <artifact> <status>" }
        $h = $state.handoffs | Where-Object { $_.from -eq $Rest[0] -and $_.to -eq $Rest[1] } | Select-Object -First 1
        if (-not $h) {
            $h = [PSCustomObject]@{
                id = "h$($state.handoffs.Count + 1)"; from = $Rest[0]; to = $Rest[1]
                artifact = $Rest[2]; status = $Rest[3]; at = $null
            }
            $state.handoffs = @($state.handoffs) + @($h)
        }
        $h.artifact = $Rest[2]; $h.status = $Rest[3]
        $h.at = (Get-Date).ToUniversalTime().ToString("o")
        Add-Event 'handoff' $Rest[0] "$($Rest[0]) -> $($Rest[1]): $($Rest[2]) ($($Rest[3]))"
    }
    'task' {
        if ($Rest.Count -lt 2) { throw "Usage: task <taskId> <status>" }
        $t = $state.tasks | Where-Object { $_.id -eq $Rest[0] }
        if ($t) { $t.status = $Rest[1] }
        else {
            $t = [PSCustomObject]@{ id = $Rest[0]; title = $Rest[0]; agent = 'backend'; status = $Rest[1]; week = $state.week }
            $state.tasks = @($state.tasks) + @($t)
        }
        $agentName = if ($t.agent) { $t.agent } else { 'system' }
        Add-Event 'task' $agentName "$($Rest[0]) -> $($Rest[1])"
    }
    'batch' {
        if ($Rest.Count -lt 2) { throw "Usage: batch <batchId> <status>" }
        $b = Get-Batch $Rest[0]
        $b.status = $Rest[1]
        if ($Rest[1] -eq 'in_progress') { $state.currentBatch = $Rest[0] }
        Add-Event 'batch' 'supervisor' "Batch $($Rest[0]) -> $($Rest[1])"
    }
    'progress' {
        if ($Rest.Count -lt 1) { throw "Usage: progress <percent>" }
        $state.overallProgress = [int]$Rest[0]
        Add-Event 'progress' 'supervisor' "Overall: $($Rest[0])%"
    }
    'week' {
        if ($Rest.Count -lt 1) { throw "Usage: week <n>" }
        $state.week = [int]$Rest[0]
        Add-Event 'log' 'supervisor' "Week $($Rest[0]) avviata"
    }
    'event' {
        if ($Rest.Count -lt 2) { throw "Usage: event <agentId> <message...>" }
        Add-Event 'log' $Rest[0] ($Rest[1..($Rest.Count - 1)] -join ' ')
    }
    'heartbeat' {
        if (-not $state.ralphLoop) { $state.ralphLoop = [PSCustomObject]@{} }
        $now = (Get-Date).ToUniversalTime().ToString("o")
        $state.ralphLoop | Add-Member -NotePropertyName lastHeartbeat -NotePropertyValue $now -Force
        if ($state.ralphLoop.status -ne 'paused') {
            $state.ralphLoop | Add-Member -NotePropertyName status -NotePropertyValue 'running' -Force
        }
        $lastHb = $state.events | Where-Object { $_.type -eq 'heartbeat' } | Select-Object -First 1
        $shouldLog = $true
        if ($lastHb -and $lastHb.at) {
            $elapsed = (Get-Date).ToUniversalTime() - [datetime]::Parse($lastHb.at)
            if ($elapsed.TotalMinutes -lt 5) { $shouldLog = $false }
        }
        if ($shouldLog) { Add-Event 'heartbeat' 'system' 'Monitor attivo' }
    }
    'pause' {
        if (-not $state.ralphLoop) { $state.ralphLoop = [PSCustomObject]@{} }
        $state.ralphLoop.status = 'paused'
        $msg = if ($Rest.Count -gt 0) { $Rest -join ' ' } else { 'Pipeline in pausa' }
        $state.ralphLoop.lastAction = $msg
        Add-Event 'pause' 'supervisor' $msg
    }
    'resume' {
        if (-not $state.ralphLoop) { $state.ralphLoop = [PSCustomObject]@{} }
        $state.ralphLoop.status = 'running'
        $msg = if ($Rest.Count -gt 0) { $Rest -join ' ' } else { 'Pipeline ripresa' }
        $state.ralphLoop.lastAction = $msg
        Add-Event 'resume' 'supervisor' $msg
    }
    default {
        Write-Host @"
KBM Pipeline CLI
  status <agentId> <status> [task]
  handoff <from> <to> <artifact> <status>
  task <taskId> <status>
  batch <batchId> <status>
  progress <percent>
  week <n>
  event <agentId> <message>
  heartbeat | pause [msg] | resume [msg]
"@
        throw "Unknown command: $Command"
    }
}

$ts = Write-State $state
Write-Host "OK - $ts"
