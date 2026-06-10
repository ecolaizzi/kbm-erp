#!/usr/bin/env node
/**
 * KBM Pipeline CLI — aggiorna state.json (stile Ralph loop)
 */
import { readFileSync, writeFileSync } from 'fs';
import { join, dirname } from 'path';
import { fileURLToPath } from 'url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const ROOT = join(__dirname, '../..');
const STATE_PATH = join(ROOT, 'pipeline/ralph/state.json');

function loadState() {
  return JSON.parse(readFileSync(STATE_PATH, 'utf8'));
}

function saveState(state) {
  state.updatedAt = new Date().toISOString();
  writeFileSync(STATE_PATH, JSON.stringify(state, null, 2) + '\n');
}

function addEvent(state, type, agent, message) {
  state.events = state.events || [];
  state.events.unshift({ at: new Date().toISOString(), type, agent, message });
  if (state.events.length > 100) state.events = state.events.slice(0, 100);
  state.ralphLoop = state.ralphLoop || {};
  state.ralphLoop.lastAction = message;
}

const [,, cmd, ...args] = process.argv;
const state = loadState();

switch (cmd) {
  case 'status': {
    const [agentId, status, ...taskParts] = args;
    const agent = state.agents.find(a => a.id === agentId);
    if (!agent) { console.error(`Agent not found: ${agentId}`); process.exit(1); }
    agent.status = status;
    if (taskParts.length) agent.currentTask = taskParts.join(' ');
    addEvent(state, 'agent_status', agentId, `${agent.name} → ${status}`);
    break;
  }
  case 'handoff': {
    const [from, to, artifact, status] = args;
    let h = state.handoffs.find(x => x.from === from && x.to === to);
    if (!h) {
      h = { id: `h${state.handoffs.length + 1}`, from, to, artifact, status: 'pending', at: null };
      state.handoffs.push(h);
    }
    h.artifact = artifact;
    h.status = status;
    h.at = new Date().toISOString();
    addEvent(state, 'handoff', from, `${from} → ${to}: ${artifact} (${status})`);
    break;
  }
  case 'event': {
    const [agentId, ...msgParts] = args;
    addEvent(state, 'log', agentId, msgParts.join(' '));
    break;
  }
  case 'task': {
    const [taskId, status] = args;
    const task = state.tasks.find(t => t.id === taskId);
    if (task) task.status = status;
    addEvent(state, 'task', task?.agent || 'system', `${taskId} → ${status}`);
    break;
  }
  case 'batch': {
    const [batchId, status] = args;
    const batch = state.batches.find(b => b.id === batchId);
    if (batch) batch.status = status;
    if (status === 'in_progress') state.currentBatch = batchId;
    addEvent(state, 'batch', 'supervisor', `Batch ${batchId} → ${status}`);
    break;
  }
  case 'progress': {
    state.overallProgress = parseInt(args[0], 10);
    addEvent(state, 'progress', 'supervisor', `Overall: ${args[0]}%`);
    break;
  }
  default:
    console.log('Usage: pipeline-cli.mjs {status|handoff|event|task|batch|progress} ...');
    process.exit(cmd ? 1 : 0);
}

saveState(state);
console.log(`OK — ${state.updatedAt}`);
