# 🏥 Pharmacy Inventory Management System

> **Enterprise-grade inventory management for pharmacies**  
> .NET 10 · Angular · PrimeNG · PostgreSQL

---

## 📚 Documentation Index

| # | Document | Description |
|---|---|---|
| 1 | [Project Plan](docs/01-PROJECT-PLAN.md) | 8-week phased timeline with Gantt chart, milestones, and risk register |
| 2 | [Requirements](docs/02-REQUIREMENTS.md) | FR / NFR / CON specification with role-permission matrix |
| 3 | [Database Architecture](docs/03-DATABASE-ARCHITECTURE.md) | ER diagram, indexing strategy, FEFO queries, backend setup |
| 4 | [Design System](docs/04-DESIGN-SYSTEM.md) | Color palette, typography scale, PrimeNG overrides, motion guidelines |
| 5 | [Responsive Design](docs/05-RESPONSIVE-DESIGN.md) | Breakpoint strategy, layout wireframes, component adaptation rules |

---

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────────────┐
│                    Angular Frontend                  │
│  PrimeNG Components · Design System · Responsive UI │
├─────────────────────────────────────────────────────┤
│                  HTTP / JWT Bearer                   │
├─────────────────────────────────────────────────────┤
│                   .NET 10 Web API                    │
│  Controllers · Services · EF Core · JWT Auth         │
├─────────────────────────────────────────────────────┤
│                    PostgreSQL                        │
│  phardb · EF Core Migrations · FEFO Queries          │
└─────────────────────────────────────────────────────┘
```

## 🎨 Design Palette

| Role | Light Mode | Dark Mode |
|---|---|---|
| Primary | `#00C781` 🟩 | `#34E89E` |
| Hover | `#00A368` | `#2BC885` |
| Error | `#FF3B30` 🟥 | `#FF453A` |
| Warning | `#FFCC00` 🟨 | `#FFD60A` |
| Background | `#F2F2F7` | `#1C1C1E` |
| Surface | `#FFFFFF` | `#2C2C2E` |

## ⚡ Quick Start

```bash
# Backend
cd backend/
dotnet ef database update
dotnet run

# Frontend
cd frontend/
npm install
ng serve
```

## 📋 Core Entities

`EMPLOYEE` → `ROLE` → `EMPLOYEE_ROLE` (RBAC)  
`CATEGORY` → `MEDICINE` → `INVENTORY_BATCH` → `STOCK_TRANSACTION`  
`SUPPLIER` → `INVENTORY_BATCH`  
`SALES_ORDER` → `SALES_ORDER_ITEM` → `INVENTORY_BATCH`  
`AUDIT_LOG` (centralized change tracking)

## 🔒 Security

- **JWT** Bearer token authentication (60 min expiry)
- **BCrypt** password hashing (12 rounds)
- **RBAC** with three roles: Admin, Pharmacist, Stock Employee
- **CORS** restricted to Angular frontend origin
- **EF Core** parameterized queries (SQL injection prevention)
# pharmarcy-management-system
