# 🎨 Design System — Apple Design Director Approach

> Pharmacy Inventory Management System  
> Primary Color: Mint Green (#00C781) · Framework: PrimeNG · Font: Inter

---

## Design Philosophy

> *"Precision, clarity, and calm confidence — the hallmarks of a system people trust with patient safety."*

The Design System draws from Apple's Human Interface Guidelines to create a **premium, medically trustworthy** experience. Mint Green conveys clinical cleanliness, while generous whitespace and crisp typography ensure readability during high-stakes operations.

### Principles

| Principle | Application |
|---|---|
| **Clarity** | Content is king — large readable text, ample spacing, clear hierarchy |
| **Deference** | UI fades into the background; data takes center stage |
| **Depth** | Subtle shadows and layering create spatial hierarchy |
| **Consistency** | Tokens and components enforce uniform visual language |

---

## 1. Color Palette

### Light Mode

| Token | Hex | Role | Preview |
|---|---|---|---|
| `--color-primary` | `#00C781` | Primary actions, active states, navigation accents | 🟩 |
| `--color-primary-hover` | `#00A368` | Hover/pressed state for primary elements | 🟩 |
| `--color-primary-light` | `#E6F9F1` | Subtle primary tint for backgrounds, badges | |
| `--color-secondary` | `#F2F2F7` | Page background (Apple System Gray 6) | |
| `--color-surface` | `#FFFFFF` | Cards, tables, modals, form backgrounds | ⬜ |
| `--color-surface-elevated` | `#FFFFFF` | Elevated surface with shadow | |
| `--color-text-primary` | `#1C1C1E` | Primary body text | |
| `--color-text-secondary` | `#8E8E93` | Secondary/helper text | |
| `--color-text-tertiary` | `#C7C7CC` | Placeholder text, disabled states | |
| `--color-border` | `#E5E5EA` | Dividers, input borders | |
| `--color-error` | `#FF3B30` | Errors, expired drug alerts, destructive actions | 🟥 |
| `--color-error-light` | `#FFF0EF` | Error background tint | |
| `--color-warning` | `#FFCC00` | Low stock warnings, caution states | 🟨 |
| `--color-warning-light` | `#FFFBE6` | Warning background tint | |
| `--color-success` | `#34C759` | Success confirmations, in-stock indicators | 🟩 |
| `--color-info` | `#007AFF` | Informational badges, links | 🟦 |

### Dark Mode

| Token | Hex | Role | Preview |
|---|---|---|---|
| `--color-primary` | `#34E89E` | Brighter mint for dark contrast | 🟩 |
| `--color-primary-hover` | `#2BC885` | Hover state in dark mode | |
| `--color-primary-light` | `#1A3D2E` | Muted primary background | |
| `--color-secondary` | `#1C1C1E` | Page background | ⬛ |
| `--color-surface` | `#2C2C2E` | Card/table backgrounds | |
| `--color-surface-elevated` | `#3A3A3C` | Elevated cards, dropdowns | |
| `--color-text-primary` | `#F2F2F7` | Primary text (high contrast) | |
| `--color-text-secondary` | `#AEAEB2` | Secondary text | |
| `--color-text-tertiary` | `#636366` | Disabled/placeholder | |
| `--color-border` | `#48484A` | Dividers, input borders | |
| `--color-error` | `#FF453A` | Slightly brighter red for dark backgrounds | 🟥 |
| `--color-warning` | `#FFD60A` | Brighter yellow for dark mode | 🟨 |
| `--color-success` | `#30D158` | Vivid green on dark | |
| `--color-info` | `#0A84FF` | Brighter blue for dark mode | |

### CSS Custom Properties

```css
:root {
  /* Light Mode (Default) */
  --color-primary: #00C781;
  --color-primary-hover: #00A368;
  --color-primary-light: #E6F9F1;
  --color-secondary: #F2F2F7;
  --color-surface: #FFFFFF;
  --color-surface-elevated: #FFFFFF;
  --color-text-primary: #1C1C1E;
  --color-text-secondary: #8E8E93;
  --color-text-tertiary: #C7C7CC;
  --color-border: #E5E5EA;
  --color-error: #FF3B30;
  --color-error-light: #FFF0EF;
  --color-warning: #FFCC00;
  --color-warning-light: #FFFBE6;
  --color-success: #34C759;
  --color-info: #007AFF;

  /* Shadows */
  --shadow-sm: 0 1px 3px rgba(0, 0, 0, 0.06);
  --shadow-md: 0 4px 12px rgba(0, 0, 0, 0.08);
  --shadow-lg: 0 12px 40px rgba(0, 0, 0, 0.12);

  /* Radii */
  --radius-sm: 8px;
  --radius-md: 12px;
  --radius-lg: 16px;
  --radius-xl: 20px;
  --radius-full: 9999px;
}

@media (prefers-color-scheme: dark) {
  :root {
    --color-primary: #34E89E;
    --color-primary-hover: #2BC885;
    --color-primary-light: #1A3D2E;
    --color-secondary: #1C1C1E;
    --color-surface: #2C2C2E;
    --color-surface-elevated: #3A3A3C;
    --color-text-primary: #F2F2F7;
    --color-text-secondary: #AEAEB2;
    --color-text-tertiary: #636366;
    --color-border: #48484A;
    --color-error: #FF453A;
    --color-warning: #FFD60A;
    --color-success: #30D158;
    --color-info: #0A84FF;

    --shadow-sm: 0 1px 3px rgba(0, 0, 0, 0.2);
    --shadow-md: 0 4px 12px rgba(0, 0, 0, 0.3);
    --shadow-lg: 0 12px 40px rgba(0, 0, 0, 0.4);
  }
}
```

---

## 2. Typography Scale

Font Stack: `'Inter', -apple-system, BlinkMacSystemFont, 'SF Pro Display', 'Segoe UI', sans-serif`

| Level | Name | Size | Weight | Line Height | Use Case |
|:---:|---|:---:|---|:---:|---|
| 1 | **Large Title** | 34px | SemiBold (600) | 41px | Dashboard hero numbers, KPI counters |
| 2 | **Title 1** | 28px | Bold (700) | 34px | Page headings ("Inventory", "Dashboard") |
| 3 | **Title 2** | 22px | Bold (700) | 28px | Card titles, modal headings |
| 4 | **Title 3** | 20px | SemiBold (600) | 25px | Section headings, sidebar group labels |
| 5 | **Headline** | 17px | SemiBold (600) | 22px | Table column headers, form section labels |
| 6 | **Body** | 17px | Regular (400) | 22px | Default body text, table cell content |
| 7 | **Callout** | 16px | Regular (400) | 21px | Emphasized descriptions, callout cards |
| 8 | **Subhead** | 15px | Regular (400) | 20px | Secondary info, metadata, breadcrumbs |
| 9 | **Caption** | 12px | Regular (400) | 16px | Timestamps, labels, error messages, badges |

### CSS Classes

```css
/* Typography Scale */
.text-large-title { font-size: 34px; font-weight: 600; line-height: 41px; letter-spacing: -0.4px; }
.text-title-1    { font-size: 28px; font-weight: 700; line-height: 34px; letter-spacing: -0.3px; }
.text-title-2    { font-size: 22px; font-weight: 700; line-height: 28px; letter-spacing: -0.2px; }
.text-title-3    { font-size: 20px; font-weight: 600; line-height: 25px; }
.text-headline   { font-size: 17px; font-weight: 600; line-height: 22px; }
.text-body       { font-size: 17px; font-weight: 400; line-height: 22px; }
.text-callout    { font-size: 16px; font-weight: 400; line-height: 21px; }
.text-subhead    { font-size: 15px; font-weight: 400; line-height: 20px; }
.text-caption    { font-size: 12px; font-weight: 400; line-height: 16px; letter-spacing: 0.2px; }
```

---

## 3. Spacing Scale

Based on a 4px base unit for consistent rhythm:

| Token | Value | Use Case |
|---|---|---|
| `--space-1` | 4px | Tight inline spacing |
| `--space-2` | 8px | Icon-to-text gap, badge padding |
| `--space-3` | 12px | Compact list gaps |
| `--space-4` | 16px | Default component padding |
| `--space-5` | 20px | Card internal padding |
| `--space-6` | 24px | Section gaps |
| `--space-8` | 32px | Page margins (mobile) |
| `--space-10` | 40px | Card gaps on desktop |
| `--space-12` | 48px | Section dividers |
| `--space-16` | 64px | Page top/bottom margins |

---

## 4. Component Styling — PrimeNG Overrides

### 4.1 Buttons

```css
/* Primary Button */
.p-button {
  background: var(--color-primary);
  border: none;
  border-radius: var(--radius-sm);
  font-size: 15px;
  font-weight: 600;
  padding: 10px 20px;
  transition: background 0.2s ease, transform 0.1s ease;
}
.p-button:hover {
  background: var(--color-primary-hover);
  transform: translateY(-1px);
}
.p-button:active {
  transform: translateY(0);
}

/* Destructive */
.p-button.p-button-danger {
  background: var(--color-error);
}

/* Secondary / Outlined */
.p-button.p-button-outlined {
  background: transparent;
  border: 1.5px solid var(--color-primary);
  color: var(--color-primary);
}

/* Ghost / Text */
.p-button.p-button-text {
  background: transparent;
  color: var(--color-primary);
}
```

### 4.2 Data Table

```css
.p-datatable .p-datatable-thead > tr > th {
  background: var(--color-secondary);
  color: var(--color-text-primary);
  font-size: 15px;
  font-weight: 600;
  padding: 12px 16px;
  border-bottom: 1px solid var(--color-border);
}

.p-datatable .p-datatable-tbody > tr > td {
  padding: 12px 16px;
  font-size: 15px;
  color: var(--color-text-primary);
  border-bottom: 1px solid var(--color-border);
}

.p-datatable .p-datatable-tbody > tr:hover {
  background: var(--color-primary-light);
}

/* Alternating rows */
.p-datatable .p-datatable-tbody > tr:nth-child(even) {
  background: var(--color-secondary);
}
```

### 4.3 Cards

```css
.p-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-sm);
  transition: box-shadow 0.2s ease;
}
.p-card:hover {
  box-shadow: var(--shadow-md);
}
```

### 4.4 Input Fields

```css
.p-inputtext {
  border: 1.5px solid var(--color-border);
  border-radius: var(--radius-sm);
  padding: 10px 14px;
  font-size: 15px;
  color: var(--color-text-primary);
  background: var(--color-surface);
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
}
.p-inputtext:focus {
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px rgba(0, 199, 129, 0.15);
  outline: none;
}
.p-inputtext.ng-invalid.ng-dirty {
  border-color: var(--color-error);
  box-shadow: 0 0 0 3px rgba(255, 59, 48, 0.15);
}
```

### 4.5 Dialog / Modal

```css
.p-dialog {
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-lg);
  border: none;
}
.p-dialog .p-dialog-header {
  padding: 20px 24px;
  border-bottom: 1px solid var(--color-border);
  font-size: 20px;
  font-weight: 600;
}
.p-dialog .p-dialog-content {
  padding: 24px;
}
.p-dialog .p-dialog-footer {
  padding: 16px 24px;
  border-top: 1px solid var(--color-border);
}
```

### 4.6 Toast / Notifications

```css
/* Success */
.p-toast-message-success {
  background: var(--color-success);
  border-left: 4px solid #2DA44E;
}

/* Error */
.p-toast-message-error {
  background: var(--color-error-light);
  border-left: 4px solid var(--color-error);
  color: var(--color-error);
}

/* Warning */
.p-toast-message-warn {
  background: var(--color-warning-light);
  border-left: 4px solid var(--color-warning);
}
```

### 4.7 Badges & Tags

```css
.badge {
  display: inline-flex;
  align-items: center;
  padding: 2px 10px;
  border-radius: var(--radius-full);
  font-size: 12px;
  font-weight: 600;
  letter-spacing: 0.2px;
}

.badge-success  { background: #E6F9F1; color: #00875A; }
.badge-error    { background: #FFF0EF; color: #D32011; }
.badge-warning  { background: #FFFBE6; color: #B58B00; }
.badge-info     { background: #E8F4FD; color: #0062B3; }
.badge-neutral  { background: #F2F2F7; color: #636366; }
```

---

## 5. Iconography

Use **PrimeIcons** bundled with PrimeNG, supplemented by a cohesive set:

| Context | Icon | PrimeIcon Class |
|---|---|---|
| Dashboard | Grid/Home | `pi pi-th-large` |
| Medicine | Pill/Box | `pi pi-box` |
| Inventory | Warehouse | `pi pi-database` |
| Stock In | Arrow Down | `pi pi-arrow-down` |
| Stock Out | Arrow Up | `pi pi-arrow-up` |
| Supplier | Truck | `pi pi-truck` |  
| Alerts | Bell | `pi pi-bell` |
| User/Account | Person | `pi pi-user` |
| Settings | Gear | `pi pi-cog` |
| Search | Magnifier | `pi pi-search` |
| Add | Plus | `pi pi-plus` |
| Edit | Pencil | `pi pi-pencil` |
| Delete | Trash | `pi pi-trash` |
| Logout | Sign Out | `pi pi-sign-out` |

---

## 6. Motion & Animation

| Property | Value | Usage |
|---|---|---|
| **Duration — Fast** | 150ms | Button press, toggle |
| **Duration — Normal** | 250ms | Card hover, focus ring |
| **Duration — Slow** | 400ms | Modal open/close, sidebar collapse |
| **Easing** | `cubic-bezier(0.25, 0.1, 0.25, 1)` | All transitions |
| **Hover lift** | `translateY(-1px)` | Interactive cards, buttons |
| **Scale on press** | `scale(0.98)` | Buttons, FAB |

```css
/* Base transition mixin */
.transition-fast   { transition: all 150ms cubic-bezier(0.25, 0.1, 0.25, 1); }
.transition-normal { transition: all 250ms cubic-bezier(0.25, 0.1, 0.25, 1); }
.transition-slow   { transition: all 400ms cubic-bezier(0.25, 0.1, 0.25, 1); }
```
