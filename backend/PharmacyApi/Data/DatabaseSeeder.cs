using Microsoft.EntityFrameworkCore;
using PharmacyApi.Models;
using PharmacyApi.Services;

namespace PharmacyApi.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(PharmacyDbContext db, PasswordService passwordService)
    {
        // ── 1. Seed Employees ───────────────────────────────
        if (!await db.Employees.IgnoreQueryFilters().AnyAsync())
        {
            var employees = new List<Employee>
            {
                new()
                {
                    EmpName = "System Admin",
                    Email = "admin@pharmacy.com",
                    PasswordHash = passwordService.HashPassword("Admin@123"),
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    EmpName = "Dr. Somchai Pharma",
                    Email = "somchai@pharmacy.com",
                    PasswordHash = passwordService.HashPassword("Pharma@123"),
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    EmpName = "Nattapong Stock",
                    Email = "nattapong@pharmacy.com",
                    PasswordHash = passwordService.HashPassword("Stock@123"),
                    CreatedAt = DateTime.UtcNow
                }
            };

            db.Employees.AddRange(employees);
            await db.SaveChangesAsync();

            // Assign roles: Admin=1, Pharmacist=2, StockEmployee=3
            db.EmployeeRoles.AddRange(
                new EmployeeRole { EId = employees[0].EId, RoleId = 1 },
                new EmployeeRole { EId = employees[1].EId, RoleId = 2 },
                new EmployeeRole { EId = employees[2].EId, RoleId = 3 }
            );
            await db.SaveChangesAsync();

            Console.WriteLine("✅ Seeded 3 employees (Admin, Pharmacist, StockEmployee)");
        }

        // ── 2. Seed Categories ──────────────────────────────
        if (!await db.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new() { CategoryName = "Pain Relief", Description = "ยาบรรเทาอาการปวด (Analgesics & Antipyretics)" },
                new() { CategoryName = "Antibiotics", Description = "ยาปฏิชีวนะ สำหรับติดเชื้อแบคทีเรีย" },
                new() { CategoryName = "Allergy & Cold", Description = "ยาแก้แพ้ แก้หวัด ลดน้ำมูก" },
                new() { CategoryName = "Gastrointestinal", Description = "ยาระบบทางเดินอาหาร กรดไหลย้อน" },
                new() { CategoryName = "Cardiovascular", Description = "ยาระบบหัวใจและหลอดเลือด" },
                new() { CategoryName = "Vitamins & Supplements", Description = "วิตามินและอาหารเสริม" },
                new() { CategoryName = "Diabetes", Description = "ยาเบาหวาน ควบคุมน้ำตาลในเลือด" },
                new() { CategoryName = "Dermatology", Description = "ยาทาภายนอก ครีม ขี้ผึ้ง" },
            };

            db.Categories.AddRange(categories);
            await db.SaveChangesAsync();
            Console.WriteLine("✅ Seeded 8 categories");
        }

        // ── 3. Seed Suppliers ───────────────────────────────
        if (!await db.Suppliers.AnyAsync())
        {
            var suppliers = new List<Supplier>
            {
                new() { SupplierName = "Bangkok Drug Co., Ltd.", Contact = "02-123-4567", Address = "123 Sukhumvit Rd, Bangkok 10110" },
                new() { SupplierName = "Thai Pharma Distribution", Contact = "02-987-6543", Address = "456 Rama IV Rd, Bangkok 10500" },
                new() { SupplierName = "MedLine Supply", Contact = "02-555-7890", Address = "789 Phetkasem Rd, Bangkok 10160" },
                new() { SupplierName = "Global Pharma Import", Contact = "02-222-3344", Address = "321 Silom Rd, Bangkok 10500" },
            };

            db.Suppliers.AddRange(suppliers);
            await db.SaveChangesAsync();
            Console.WriteLine("✅ Seeded 4 suppliers");
        }

        // ── 4. Seed Medicines (need category IDs) ───────────
        if (!await db.Medicines.AnyAsync())
        {
            var cats = await db.Categories.ToListAsync();
            var catLookup = cats.ToDictionary(c => c.CategoryName, c => c.CategoryId);

            var medicines = new List<Medicine>
            {
                // Pain Relief
                new() { DrugName = "Paracetamol 500mg", GenericName = "Acetaminophen", Barcode = "8850001001001", Unit = "Box (100 tabs)", CategoryId = catLookup["Pain Relief"], ReorderLevel = 50 },
                new() { DrugName = "Ibuprofen 400mg", GenericName = "Ibuprofen", Barcode = "8850001001002", Unit = "Box (30 tabs)", CategoryId = catLookup["Pain Relief"], ReorderLevel = 30 },
                new() { DrugName = "Diclofenac 25mg", GenericName = "Diclofenac Sodium", Barcode = "8850001001003", Unit = "Box (10 tabs)", CategoryId = catLookup["Pain Relief"], ReorderLevel = 20 },

                // Antibiotics
                new() { DrugName = "Amoxicillin 500mg", GenericName = "Amoxicillin", Barcode = "8850001002001", Unit = "Box (30 caps)", CategoryId = catLookup["Antibiotics"], ReorderLevel = 40 },
                new() { DrugName = "Azithromycin 250mg", GenericName = "Azithromycin", Barcode = "8850001002002", Unit = "Box (6 tabs)", CategoryId = catLookup["Antibiotics"], ReorderLevel = 25 },
                new() { DrugName = "Ciprofloxacin 500mg", GenericName = "Ciprofloxacin HCl", Barcode = "8850001002003", Unit = "Box (10 tabs)", CategoryId = catLookup["Antibiotics"], ReorderLevel = 20 },

                // Allergy & Cold
                new() { DrugName = "Cetirizine 10mg", GenericName = "Cetirizine HCl", Barcode = "8850001003001", Unit = "Box (10 tabs)", CategoryId = catLookup["Allergy & Cold"], ReorderLevel = 30 },
                new() { DrugName = "Loratadine 10mg", GenericName = "Loratadine", Barcode = "8850001003002", Unit = "Box (10 tabs)", CategoryId = catLookup["Allergy & Cold"], ReorderLevel = 25 },
                new() { DrugName = "Pseudoephedrine 60mg", GenericName = "Pseudoephedrine HCl", Barcode = "8850001003003", Unit = "Box (10 tabs)", CategoryId = catLookup["Allergy & Cold"], ReorderLevel = 15 },

                // Gastrointestinal
                new() { DrugName = "Omeprazole 20mg", GenericName = "Omeprazole", Barcode = "8850001004001", Unit = "Box (14 caps)", CategoryId = catLookup["Gastrointestinal"], ReorderLevel = 30 },
                new() { DrugName = "Loperamide 2mg", GenericName = "Loperamide HCl", Barcode = "8850001004002", Unit = "Box (6 tabs)", CategoryId = catLookup["Gastrointestinal"], ReorderLevel = 20 },

                // Cardiovascular
                new() { DrugName = "Amlodipine 5mg", GenericName = "Amlodipine Besylate", Barcode = "8850001005001", Unit = "Box (30 tabs)", CategoryId = catLookup["Cardiovascular"], ReorderLevel = 25 },
                new() { DrugName = "Atorvastatin 20mg", GenericName = "Atorvastatin Calcium", Barcode = "8850001005002", Unit = "Box (30 tabs)", CategoryId = catLookup["Cardiovascular"], ReorderLevel = 20 },

                // Vitamins
                new() { DrugName = "Vitamin C 1000mg", GenericName = "Ascorbic Acid", Barcode = "8850001006001", Unit = "Bottle (60 tabs)", CategoryId = catLookup["Vitamins & Supplements"], ReorderLevel = 20 },
                new() { DrugName = "Multivitamin A-Z", GenericName = "Multivitamin", Barcode = "8850001006002", Unit = "Bottle (30 tabs)", CategoryId = catLookup["Vitamins & Supplements"], ReorderLevel = 15 },

                // Diabetes
                new() { DrugName = "Metformin 500mg", GenericName = "Metformin HCl", Barcode = "8850001007001", Unit = "Box (30 tabs)", CategoryId = catLookup["Diabetes"], ReorderLevel = 30 },

                // Dermatology
                new() { DrugName = "Hydrocortisone Cream 1%", GenericName = "Hydrocortisone", Barcode = "8850001008001", Unit = "Tube (15g)", CategoryId = catLookup["Dermatology"], ReorderLevel = 10 },
                new() { DrugName = "Clotrimazole Cream 1%", GenericName = "Clotrimazole", Barcode = "8850001008002", Unit = "Tube (15g)", CategoryId = catLookup["Dermatology"], ReorderLevel = 10 },
            };

            db.Medicines.AddRange(medicines);
            await db.SaveChangesAsync();
            Console.WriteLine("✅ Seeded 18 medicines");
        }

        // ── 5. Seed Inventory Batches (need medicine & supplier IDs) ─
        if (!await db.InventoryBatches.IgnoreQueryFilters().AnyAsync())
        {
            var meds = await db.Medicines.ToListAsync();
            var sups = await db.Suppliers.ToListAsync();
            var admin = await db.Employees.FirstAsync();
            var rng = new Random(42);

            var batches = new List<InventoryBatch>();

            foreach (var med in meds)
            {
                var supplier = sups[rng.Next(sups.Count)];

                // Batch 1: normal stock, expiring in 6-18 months
                batches.Add(new InventoryBatch
                {
                    DrugId = med.DrugId,
                    SupplierId = supplier.SupplierId,
                    BatchNumber = $"BT-{med.DrugId:D3}-A",
                    QuantityInStock = rng.Next(50, 200),
                    CostPrice = Math.Round((decimal)(rng.NextDouble() * 100 + 10), 2),
                    SellingPrice = Math.Round((decimal)(rng.NextDouble() * 150 + 20), 2),
                    MfgDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3)),
                    ExpDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(rng.Next(6, 18))),
                    CreatedAt = DateTime.UtcNow
                });

                // Batch 2: older stock, expiring sooner (for FEFO testing)
                batches.Add(new InventoryBatch
                {
                    DrugId = med.DrugId,
                    SupplierId = supplier.SupplierId,
                    BatchNumber = $"BT-{med.DrugId:D3}-B",
                    QuantityInStock = rng.Next(10, 80),
                    CostPrice = Math.Round((decimal)(rng.NextDouble() * 80 + 10), 2),
                    SellingPrice = Math.Round((decimal)(rng.NextDouble() * 120 + 20), 2),
                    MfgDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-12)),
                    ExpDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(rng.Next(5, 45))), // Some expiring soon!
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Add some low-stock batches (for alert testing)
            var lowStockMeds = meds.Take(3).ToList();
            foreach (var med in lowStockMeds)
            {
                // Override batch B to have very low stock
                var batch = batches.First(b => b.DrugId == med.DrugId && b.BatchNumber.EndsWith("-B"));
                batch.QuantityInStock = rng.Next(1, 5); // Very low
            }

            db.InventoryBatches.AddRange(batches);
            await db.SaveChangesAsync();

            // Record stock-in transactions for all batches
            foreach (var batch in batches)
            {
                db.StockTransactions.Add(new StockTransaction
                {
                    BatchId = batch.BatchId,
                    EId = admin.EId,
                    TransType = "IN",
                    ReferenceNo = $"SEED-{batch.BatchNumber}",
                    Quantity = batch.QuantityInStock,
                    Notes = "Initial seed stock",
                    CreatedAt = DateTime.UtcNow
                });
            }
            await db.SaveChangesAsync();

            Console.WriteLine($"✅ Seeded {batches.Count} inventory batches with transactions");
        }

        Console.WriteLine("🏥 Database seeding complete!");
    }
}
