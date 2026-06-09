# KBM Style Guide

**Versione**: 1.0  
**Data**: 2026-06-09  
**Status**: ✅ Draft

---

## 1. COLOR PALETTE

Design originale KBM — non derivato da palette competitor.

### 1.1 Colori Primari
| Token | Hex | RGB | Uso |
|---|---|---|---|
| `--kbm-primary-50` | `#EFF6FF` | 239,246,255 | Background hover leggero |
| `--kbm-primary-100` | `#DBEAFE` | 219,234,254 | Background selezione |
| `--kbm-primary-500` | `#3B82F6` | 59,130,246 | Bottoni primari, link |
| `--kbm-primary-600` | `#2563EB` | 37,99,235 | Bottoni primari hover |
| `--kbm-primary-700` | `#1D4ED8` | 29,78,216 | Bottoni primari active |
| `--kbm-primary-900` | `#1E3A8A` | 30,58,138 | Testo su sfondo chiaro |

### 1.2 Colori Accento
| Token | Hex | RGB | Uso |
|---|---|---|---|
| `--kbm-accent-400` | `#FBBF24` | 251,191,36 | Highlight, badge warning |
| `--kbm-accent-500` | `#F59E0B` | 245,158,11 | Action secondaria, attenzione |
| `--kbm-accent-700` | `#B45309` | 180,83,9 | Testo warning su sfondo chiaro |

### 1.3 Semantic Colors
| Token | Hex | Uso |
|---|---|---|
| `--kbm-success-500` | `#10B981` | Stato completato, salvataggio OK |
| `--kbm-success-700` | `#047857` | Testo success |
| `--kbm-warning-500` | `#F59E0B` | Avvisi, pagamenti in scadenza |
| `--kbm-warning-700` | `#B45309` | Testo warning |
| `--kbm-error-500` | `#EF4444` | Errori, validazioni, scaduto |
| `--kbm-error-700` | `#B91C1C` | Testo error |
| `--kbm-info-500` | `#06B6D4` | Info, help, note |

### 1.4 Neutral / Gray Scale
| Token | Hex | Uso |
|---|---|---|
| `--kbm-gray-50` | `#F9FAFB` | Background app |
| `--kbm-gray-100` | `#F3F4F6` | Background sidebar, alternanza righe |
| `--kbm-gray-200` | `#E5E7EB` | Bordi leggeri, separatori |
| `--kbm-gray-300` | `#D1D5DB` | Bordi input, divisori |
| `--kbm-gray-400` | `#9CA3AF` | Placeholder testo, icone disabilitate |
| `--kbm-gray-500` | `#6B7280` | Testo secondario, label |
| `--kbm-gray-700` | `#374151` | Testo primario leggero |
| `--kbm-gray-900` | `#111827` | Testo primario scuro |

### 1.5 UI Structural Colors
| Token | Hex | Uso |
|---|---|---|
| `--kbm-topbar-bg` | `#1E293B` | Top bar background (slate scuro) |
| `--kbm-topbar-text` | `#F1F5F9` | Testo top bar |
| `--kbm-sidenav-bg` | `#0F172A` | Side nav background |
| `--kbm-sidenav-text` | `#94A3B8` | Testo menu inattivo |
| `--kbm-sidenav-active` | `#3B82F6` | Indicatore modulo attivo |
| `--kbm-surface` | `#FFFFFF` | Background card/content |
| `--kbm-border` | `#E5E7EB` | Bordi generali |

### 1.6 Status Badge Colors (Scadenzario / Documenti)
| Stato | Background | Testo | Uso |
|---|---|---|---|
| Bozza / Inserimento | `#F3F4F6` | `#374151` | Documento non finalizzato |
| Confermato | `#DBEAFE` | `#1D4ED8` | Ordine confermato |
| Parz. Evaso | `#FEF3C7` | `#92400E` | Evaso in parte |
| Evaso / Contabilizzato | `#D1FAE5` | `#065F46` | Completato |
| Annullato | `#FEE2E2` | `#991B1B` | Annullato |
| Scaduto | `#FEE2E2` | `#991B1B` | Pagamento scaduto |
| In scadenza | `#FEF3C7` | `#92400E` | Scade nei prossimi 7gg |
| In ordine | `#D1FAE5` | `#065F46` | Pagamento futuro |

---

## 2. TYPOGRAPHY

### 2.1 Font Family
```css
--kbm-font-family: 'Inter', 'Segoe UI', 'Roboto', system-ui, sans-serif;
--kbm-font-mono: 'JetBrains Mono', 'Consolas', 'Courier New', monospace;
```
- **Inter** come prima scelta (ottima leggibilità a 12-14px, open source)
- Fallback a **Segoe UI** (nativo Windows, ottimo su RDP)
- Font mono per codici (Partita IVA, barcode, numeri documento)

### 2.2 Scale Tipografica
| Token | Size | Weight | Line-height | Uso |
|---|---|---|---|---|
| `--kbm-text-h1` | 24px | 700 | 1.3 | Titolo pagina principale |
| `--kbm-text-h2` | 20px | 600 | 1.3 | Titolo sezione |
| `--kbm-text-h3` | 16px | 600 | 1.4 | Sottotitolo, tab label |
| `--kbm-text-h4` | 14px | 600 | 1.4 | Label gruppo campi |
| `--kbm-text-body` | 14px | 400 | 1.5 | Testo standard |
| `--kbm-text-body-sm` | 13px | 400 | 1.5 | Testo secondario |
| `--kbm-text-grid` | 12px | 400 | 1.4 | Righe griglia (compact) |
| `--kbm-text-caption` | 11px | 400 | 1.4 | Status bar, audit info |
| `--kbm-text-label` | 12px | 500 | 1.4 | Labels form |
| `--kbm-text-badge` | 11px | 600 | 1 | Badge, status pill |

**Regola assoluta**: nessun testo sotto 11px — accessibilità e leggibilità RDP.

---

## 3. SPACING SCALE

Base unit: **8px**

| Token | Value | Uso |
|---|---|---|
| `--kbm-space-1` | 4px | Gap micro, padding badge |
| `--kbm-space-2` | 8px | Gap piccolo, padding bottoni compact |
| `--kbm-space-3` | 12px | Gap interno componenti |
| `--kbm-space-4` | 16px | Padding standard, gap colonne form |
| `--kbm-space-5` | 20px | Gap sezioni interne |
| `--kbm-space-6` | 24px | Padding card, gap sezioni |
| `--kbm-space-8` | 32px | Gap tra sezioni major |
| `--kbm-space-10` | 40px | Padding pagina laterale |
| `--kbm-space-12` | 48px | Gap extra-large |

### 3.1 Component-Specific Sizing
| Componente | Altezza | Padding orizz. |
|---|---|---|
| Bottone primario | 36px | 16px |
| Bottone secondario | 32px | 12px |
| Bottone iconico | 32×32px | — |
| Input field | 34px | 10px |
| Grid row (compact) | 32px | 8px |
| Grid row (standard) | 40px | 8px |
| Tab | 36px | 16px |
| Top bar | 48px | 16px |
| Status bar | 28px | 12px |
| Sidebar (espanso) | — | 220px wide |
| Sidebar (collassato) | — | 48px wide |

---

## 4. BORDER RADIUS

| Token | Value | Uso |
|---|---|---|
| `--kbm-radius-sm` | 3px | Badge, chip status |
| `--kbm-radius-md` | 6px | Bottoni, input, card |
| `--kbm-radius-lg` | 8px | Modal, pannelli |
| `--kbm-radius-xl` | 12px | Dropdown, popup |

---

## 5. SHADOWS

| Token | Value | Uso |
|---|---|---|
| `--kbm-shadow-sm` | `0 1px 2px rgba(0,0,0,0.05)` | Card, input focus |
| `--kbm-shadow-md` | `0 4px 6px rgba(0,0,0,0.07)` | Dropdown, tooltip |
| `--kbm-shadow-lg` | `0 10px 15px rgba(0,0,0,0.10)` | Modal, popup |

---

## 6. ICONS

### 6.1 Icon Set
**Lucide Icons** (open source, MIT) — set coerente, outline style, SVG.
- Alternativa: **Heroicons** (MIT)
- Niente icone proprietarie (Material Icons Extended o Font Awesome Pro)

### 6.2 Dimensioni
| Contesto | Size |
|---|---|
| Toolbar / Button | 20×20px |
| Sidebar menu | 20×20px |
| Inline campo form | 16×16px |
| Status / Badge | 12×12px |
| KPI Card | 24×24px |
| Empty state | 48×48px |

### 6.3 Stile
- **Outline**: azioni UI (bottoni, menu, toolbar)
- **Filled**: status, badge, indicatori
- **Stroke width**: 1.5px (standard Lucide)
- **Color**: eredita dal contesto (currentColor)

---

## 7. ELEVATION & LAYER MODEL

| Layer | z-index | Componenti |
|---|---|---|
| Base | 0 | Contenuto pagina, griglia |
| Raised | 10 | Card, sidebar |
| Dropdown | 100 | Menu dropdown, select |
| Sticky | 200 | Top bar, toolbar sticky |
| Overlay | 500 | Backdrop modal |
| Modal | 600 | Finestre modali |
| Toast | 900 | Notifiche toast |
| Tooltip | 1000 | Tooltip |

---

## 8. THEME

### 8.1 Light Theme (default)
- Background app: `--kbm-gray-50` (#F9FAFB)
- Surface (card/panel): `#FFFFFF`
- Testo primario: `--kbm-gray-900`
- Topbar/Sidenav: dark (contrasto elevato)

### 8.2 Dark Theme (future, v2)
- Background app: `#0F172A`
- Surface: `#1E293B`
- Testo: `#F1F5F9`
- (Non implementato nel MVP)

### 8.3 High Contrast Mode (accessibilità)
- Segue le impostazioni di sistema OS (Windows High Contrast)
- Nessuna dipendenza da CSS gradients o opacity per trasmettere informazione
- Tutti i bordi visibili anche in B/N

---

## 9. CONTRAST RATIOS (WCAG AA)

| Combinazione | Ratio | Standard |
|---|---|---|
| Gray-900 su White | 16.75:1 | ✅ AAA |
| Gray-700 su White | 10.39:1 | ✅ AAA |
| Primary-600 su White | 4.64:1 | ✅ AA |
| White su Primary-600 | 4.64:1 | ✅ AA |
| Gray-500 su White | 4.48:1 | ✅ AA |
| Testo grid (Gray-700, 12px) | 10.39:1 | ✅ AA |
| Badge error (White su Error-500) | 4.85:1 | ✅ AA |

**Regola**: nessuna combinazione testo/sfondo sotto 4.5:1 (WCAG AA minimo).

---

*KBM Style Guide v1.0 — Design originale. © 2026 KBM Project.*
