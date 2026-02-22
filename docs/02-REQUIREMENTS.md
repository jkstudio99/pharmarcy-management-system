# 🎯 Requirements Specification

> Pharmacy Inventory Management System  
> Version 1.0 · Last Updated: 2026-02-20

---

## 1. Functional Requirements (FR)

### FR-01 · Authentication & Authorization

| ID | Requirement | Priority |
|---|---|---|
| FR-01.1 | System shall provide login via Email + Password | **Must** |
| FR-01.2 | Passwords shall be hashed using BCrypt before storage | **Must** |
| FR-01.3 | Upon login, system shall issue a JWT access token | **Must** |
| FR-01.4 | JWT shall encode user ID, roles, and expiration | **Must** |
| FR-01.5 | System shall enforce Role-Based Access Control (RBAC) | **Must** |
| FR-01.6 | Three roles shall be supported: **Admin**, **Pharmacist**, **Stock Employee** | **Must** |
| FR-01.7 | Admin can manage all users and assign roles | **Must** |
| FR-01.8 | Pharmacist can manage medicines, view inventory, process sales | **Must** |
| FR-01.9 | Stock Employee can perform Stock In/Out and view inventory | **Must** |

### FR-02 · Medicine Master Data

| ID | Requirement | Priority |
|---|---|---|
| FR-02.1 | CRUD operations for Medicine records | **Must** |
| FR-02.2 | Each medicine shall have: Name, Generic Name, Barcode, Unit, Category, Reorder Level | **Must** |
| FR-02.3 | Medicine shall be linked to a Category | **Must** |
| FR-02.4 | Support optional packaging image upload | **Should** |
| FR-02.5 | Search and filter medicines by name, category, barcode | **Must** |
| FR-02.6 | Soft-delete via `IsActive` flag (no hard deletes) | **Must** |

### FR-03 · Category Management

| ID | Requirement | Priority |
|---|---|---|
| FR-03.1 | CRUD operations for Categories | **Must** |
| FR-03.2 | Category shall have Name and Description | **Must** |
| FR-03.3 | Category list available as dropdown in Medicine forms | **Must** |

### FR-04 · Inventory Management

| ID | Requirement | Priority |
|---|---|---|
| FR-04.1 | Track inventory at **Batch** level (not aggregate drug level) | **Must** |
| FR-04.2 | Each batch records: BatchNumber, QuantityInStock, CostPrice, SellingPrice, MfgDate, ExpDate | **Must** |
| FR-04.3 | Batch linked to Medicine and Supplier | **Must** |
| FR-04.4 | Stock In: create new batch or add to existing batch quantity | **Must** |
| FR-04.5 | Stock Out: deduct from batch using FEFO (First Expire, First Out) | **Must** |
| FR-04.6 | Adjustment: manual correction with reason/notes | **Must** |
| FR-04.7 | All stock movements recorded as `STOCK_TRANSACTION` | **Must** |
| FR-04.8 | Transaction types: `IN`, `OUT`, `ADJUST`, `EXPIRED` | **Must** |

### FR-05 · Alerts & Notifications

| ID | Requirement | Priority |
|---|---|---|
| FR-05.1 | **Low Stock Alert**: trigger when total stock ≤ `ReorderLevel` | **Must** |
| FR-05.2 | **Expiry Alert (FEFO)**: highlight batches expiring within 30/60/90 days | **Must** |
| FR-05.3 | Dashboard shall display alert count badges | **Must** |
| FR-05.4 | Alert indicators on medicine/batch list views using color-coded badges | **Must** |

### FR-06 · Supplier Management

| ID | Requirement | Priority |
|---|---|---|
| FR-06.1 | CRUD operations for Supplier records | **Must** |
| FR-06.2 | Supplier shall have: Name, Contact, Address | **Must** |
| FR-06.3 | Supplier linked to Inventory Batches | **Must** |
| FR-06.4 | Search suppliers by name or contact | **Should** |

### FR-07 · Sales & Dispensing

| ID | Requirement | Priority |
|---|---|---|
| FR-07.1 | Create sales orders with line items | **Should** |
| FR-07.2 | Each line item linked to a specific batch (FEFO auto-select) | **Should** |
| FR-07.3 | Calculate total amount from unit price × quantity | **Should** |
| FR-07.4 | Record payment method (Cash, QR PromptPay, Credit Card) | **Should** |
| FR-07.5 | Sales order creates corresponding `OUT` transactions | **Should** |

### FR-08 · Dashboard & Reporting

| ID | Requirement | Priority |
|---|---|---|
| FR-08.1 | Display key metrics: Total Medicines, Low Stock Count, Expiring Soon, Today's Transactions | **Must** |
| FR-08.2 | Stock level chart (bar or line) | **Should** |
| FR-08.3 | Transaction history with date-range filter | **Must** |
| FR-08.4 | Export data to CSV/Excel | **Could** |

### FR-09 · Audit Trail

| ID | Requirement | Priority |
|---|---|---|
| FR-09.1 | Log all data changes to `AUDIT_LOG` table | **Must** |
| FR-09.2 | Record: TableName, RecordID, Action, OldValues, NewValues, ActionBy, ActionDate | **Must** |
| FR-09.3 | Audit log viewable by Admin only | **Must** |

---

## 2. Non-Functional Requirements (NFR)

### NFR-01 · Performance

| ID | Requirement | Target |
|---|---|---|
| NFR-01.1 | API response time for list queries | **< 200ms** |
| NFR-01.2 | Dashboard initial load time | **< 2 seconds** |
| NFR-01.3 | Database queries optimized with proper indexing | All FK columns indexed |
| NFR-01.4 | Pagination required for all list endpoints | Default page size: 20 |

### NFR-02 · Security

| ID | Requirement | Target |
|---|---|---|
| NFR-02.1 | Password hashing algorithm | **BCrypt** (min 12 rounds) |
| NFR-02.2 | API authentication | **JWT Bearer Token** |
| NFR-02.3 | Token expiration | **60 minutes** (configurable) |
| NFR-02.4 | HTTPS enforcement | **Required** in production |
| NFR-02.5 | CORS policy | Restrict to Angular frontend origin |
| NFR-02.6 | SQL injection prevention | Parameterized queries via EF Core |
| NFR-02.7 | Sensitive data in logs | **Never** log passwords or tokens |

### NFR-03 · Usability

| ID | Requirement | Target |
|---|---|---|
| NFR-03.1 | Responsive design breakpoints | 375px / 768px / 1440px |
| NFR-03.2 | Dark mode support | Toggle with system preference detection |
| NFR-03.3 | Accessibility | WCAG 2.1 Level AA |
| NFR-03.4 | Form validation feedback | Inline error messages within 100ms |
| NFR-03.5 | Loading states | Skeleton screens for all data views |

### NFR-04 · Reliability

| ID | Requirement | Target |
|---|---|---|
| NFR-04.1 | System uptime | **99.5%** |
| NFR-04.2 | Database backup strategy | Daily automated backups |
| NFR-04.3 | Error logging | Structured logging (Serilog) |
| NFR-04.4 | Graceful error handling | User-friendly error pages/toasts |

### NFR-05 · Maintainability

| ID | Requirement | Target |
|---|---|---|
| NFR-05.1 | Code architecture | Clean Architecture (layered) |
| NFR-05.2 | API documentation | Auto-generated Swagger/OpenAPI |
| NFR-05.3 | Code coverage | **≥ 70%** for business logic |
| NFR-05.4 | Naming conventions | .NET / Angular standard conventions |

---

## 3. Constraints (CON)

| ID | Constraint | Details |
|---|---|---|
| CON-01 | **Backend Framework** | .NET 10 (C#) — no alternative frameworks |
| CON-02 | **Frontend Framework** | Angular (latest stable) |
| CON-03 | **UI Library** | PrimeNG — all data tables, forms, dialogs |
| CON-04 | **Database** | PostgreSQL (no MySQL, no SQL Server) |
| CON-05 | **ORM** | Entity Framework Core |
| CON-06 | **Timeline** | 8-week delivery window |
| CON-07 | **Authentication** | JWT-based (no OAuth/SAML for v1) |
| CON-08 | **Deployment** | Single-server deployment for v1 |
| CON-09 | **Language** | UI text in English (Thai localization in v2) |

---

## 4. Role–Permission Matrix

| Feature | Admin | Pharmacist | Stock Employee |
|---|:---:|:---:|:---:|
| Manage Users & Roles | ✅ | ❌ | ❌ |
| View Dashboard | ✅ | ✅ | ✅ |
| Manage Medicine Master | ✅ | ✅ | ❌ |
| Manage Categories | ✅ | ✅ | ❌ |
| Manage Suppliers | ✅ | ❌ | ❌ |
| Stock In | ✅ | ✅ | ✅ |
| Stock Out (Dispense) | ✅ | ✅ | ✅ |
| Stock Adjustment | ✅ | ❌ | ❌ |
| View Inventory | ✅ | ✅ | ✅ |
| Process Sales Orders | ✅ | ✅ | ❌ |
| View Audit Log | ✅ | ❌ | ❌ |
| View Alerts | ✅ | ✅ | ✅ |
