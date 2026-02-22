using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacyApi.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ════════════════════════════════════════════════════
            // SP 1: sp_deduct_stock_fefo (FEFO stock deduction)
            // ════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION sp_deduct_stock_fefo(
    p_drug_id       INT,
    p_quantity      INT,
    p_employee_id   INT,
    p_reference_no  VARCHAR DEFAULT NULL
)
RETURNS JSONB
LANGUAGE plpgsql
AS $$
DECLARE
    v_remaining     INT := p_quantity;
    v_batch         RECORD;
    v_deduct_qty    INT;
    v_result        JSONB := '[]'::JSONB;
    v_total_stock   INT;
BEGIN
    IF p_quantity <= 0 THEN
        RAISE EXCEPTION 'Quantity must be greater than 0';
    END IF;

    SELECT COALESCE(SUM(""QuantityInStock""), 0)
    INTO v_total_stock
    FROM ""INVENTORY_BATCH""
    WHERE ""DrugID"" = p_drug_id
      AND ""QuantityInStock"" > 0
      AND ""IsActive"" = true
      AND ""ExpDate"" > CURRENT_DATE;

    IF v_total_stock < p_quantity THEN
        RAISE EXCEPTION 'Insufficient stock. Available: %, Requested: %', v_total_stock, p_quantity;
    END IF;

    FOR v_batch IN
        SELECT ""BatchID"", ""BatchNumber"", ""QuantityInStock"", ""ExpDate""
        FROM ""INVENTORY_BATCH""
        WHERE ""DrugID"" = p_drug_id
          AND ""QuantityInStock"" > 0
          AND ""IsActive"" = true
          AND ""ExpDate"" > CURRENT_DATE
        ORDER BY ""ExpDate"" ASC
        FOR UPDATE
    LOOP
        EXIT WHEN v_remaining <= 0;

        IF v_batch.""QuantityInStock"" >= v_remaining THEN
            v_deduct_qty := v_remaining;
        ELSE
            v_deduct_qty := v_batch.""QuantityInStock"";
        END IF;

        UPDATE ""INVENTORY_BATCH""
        SET ""QuantityInStock"" = ""QuantityInStock"" - v_deduct_qty,
            ""UpdatedAt"" = NOW()
        WHERE ""BatchID"" = v_batch.""BatchID"";

        INSERT INTO ""STOCK_TRANSACTION"" (""BatchID"", ""EID"", ""TransType"", ""ReferenceNo"", ""Quantity"", ""Notes"", ""CreatedAt"")
        VALUES (
            v_batch.""BatchID"",
            p_employee_id,
            'OUT',
            p_reference_no,
            v_deduct_qty,
            FORMAT('FEFO deduction: %s units from batch %s', v_deduct_qty, v_batch.""BatchNumber""),
            NOW()
        );

        v_result := v_result || jsonb_build_object(
            'batchId', v_batch.""BatchID"",
            'batchNumber', v_batch.""BatchNumber"",
            'deductedQty', v_deduct_qty,
            'remainingQty', v_batch.""QuantityInStock"" - v_deduct_qty,
            'expDate', v_batch.""ExpDate""
        );

        v_remaining := v_remaining - v_deduct_qty;
    END LOOP;

    RETURN v_result;
END;
$$;
");

            // ════════════════════════════════════════════════════
            // SP 2: sp_get_dashboard_summary
            // ════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION sp_get_dashboard_summary()
RETURNS JSONB
LANGUAGE plpgsql
AS $$
DECLARE
    v_total_medicines    INT;
    v_low_stock_count    INT;
    v_expiring_soon      INT;
    v_today_transactions INT;
    v_monthly_sales      JSONB;
    v_top_selling        JSONB;
BEGIN
    SELECT COUNT(*) INTO v_total_medicines
    FROM ""MEDICINE"" WHERE ""IsActive"" = true;

    SELECT COUNT(*) INTO v_low_stock_count
    FROM (
        SELECT m.""DrugID"", m.""ReorderLevel"",
               COALESCE(SUM(b.""QuantityInStock""), 0) AS total_stock
        FROM ""MEDICINE"" m
        LEFT JOIN ""INVENTORY_BATCH"" b ON b.""DrugID"" = m.""DrugID"" AND b.""IsActive"" = true
        WHERE m.""IsActive"" = true
        GROUP BY m.""DrugID"", m.""ReorderLevel""
        HAVING COALESCE(SUM(b.""QuantityInStock""), 0) <= m.""ReorderLevel""
    ) AS low_stock;

    SELECT COUNT(*) INTO v_expiring_soon
    FROM ""INVENTORY_BATCH""
    WHERE ""IsActive"" = true AND ""QuantityInStock"" > 0
      AND ""ExpDate"" IS NOT NULL
      AND ""ExpDate"" BETWEEN CURRENT_DATE AND CURRENT_DATE + INTERVAL '30 days';

    SELECT COUNT(*) INTO v_today_transactions
    FROM ""STOCK_TRANSACTION"" WHERE ""CreatedAt""::DATE = CURRENT_DATE;

    SELECT COALESCE(jsonb_agg(monthly ORDER BY monthly->>'year' DESC, monthly->>'month' DESC), '[]'::JSONB)
    INTO v_monthly_sales
    FROM (
        SELECT jsonb_build_object(
            'year', EXTRACT(YEAR FROM so.""CreatedAt""),
            'month', EXTRACT(MONTH FROM so.""CreatedAt""),
            'totalAmount', SUM(so.""TotalAmount""),
            'orderCount', COUNT(*)
        ) AS monthly
        FROM ""SALES_ORDER"" so
        WHERE so.""Status"" = 'Completed' AND so.""IsActive"" = true
          AND so.""CreatedAt"" >= CURRENT_DATE - INTERVAL '12 months'
        GROUP BY EXTRACT(YEAR FROM so.""CreatedAt""), EXTRACT(MONTH FROM so.""CreatedAt"")
    ) AS sales_data;

    SELECT COALESCE(jsonb_agg(top_med), '[]'::JSONB)
    INTO v_top_selling
    FROM (
        SELECT jsonb_build_object(
            'drugId', m.""DrugID"",
            'drugName', m.""DrugName"",
            'totalSold', SUM(soi.""Quantity""),
            'totalRevenue', SUM(soi.""Quantity"" * soi.""UnitPrice"")
        ) AS top_med
        FROM ""SALES_ORDER_ITEM"" soi
        JOIN ""INVENTORY_BATCH"" b ON b.""BatchID"" = soi.""BatchID""
        JOIN ""MEDICINE"" m ON m.""DrugID"" = b.""DrugID""
        JOIN ""SALES_ORDER"" so ON so.""OrderID"" = soi.""OrderID""
        WHERE so.""Status"" = 'Completed' AND so.""IsActive"" = true
          AND so.""CreatedAt"" >= CURRENT_DATE - INTERVAL '12 months'
        GROUP BY m.""DrugID"", m.""DrugName""
        ORDER BY SUM(soi.""Quantity"") DESC
        LIMIT 10
    ) AS top_data;

    RETURN jsonb_build_object(
        'totalMedicines', v_total_medicines,
        'lowStockCount', v_low_stock_count,
        'expiringSoonCount', v_expiring_soon,
        'todayTransactions', v_today_transactions,
        'monthlySales', v_monthly_sales,
        'topSellingMedicines', v_top_selling
    );
END;
$$;
");

            // ════════════════════════════════════════════════════
            // SP 3: fn_audit_trigger + triggers
            // ════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION fn_audit_trigger()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_old_values JSONB := NULL;
    v_new_values JSONB := NULL;
    v_action     VARCHAR;
    v_record_id  INT;
BEGIN
    IF TG_OP = 'INSERT' THEN
        v_action := 'INSERT';
        v_new_values := to_jsonb(NEW);
        IF TG_TABLE_NAME = 'EMPLOYEE' THEN v_record_id := NEW.""EID"";
        ELSIF TG_TABLE_NAME = 'SUPPLIER' THEN v_record_id := NEW.""SupplierID"";
        ELSIF TG_TABLE_NAME = 'CATEGORY' THEN v_record_id := NEW.""CategoryID"";
        ELSIF TG_TABLE_NAME = 'MEDICINE' THEN v_record_id := NEW.""DrugID"";
        ELSIF TG_TABLE_NAME = 'INVENTORY_BATCH' THEN v_record_id := NEW.""BatchID"";
        ELSIF TG_TABLE_NAME = 'SALES_ORDER' THEN v_record_id := NEW.""OrderID"";
        END IF;
    ELSIF TG_OP = 'UPDATE' THEN
        v_action := 'UPDATE';
        v_old_values := to_jsonb(OLD);
        v_new_values := to_jsonb(NEW);
        IF TG_TABLE_NAME = 'EMPLOYEE' THEN v_record_id := OLD.""EID"";
        ELSIF TG_TABLE_NAME = 'SUPPLIER' THEN v_record_id := OLD.""SupplierID"";
        ELSIF TG_TABLE_NAME = 'CATEGORY' THEN v_record_id := OLD.""CategoryID"";
        ELSIF TG_TABLE_NAME = 'MEDICINE' THEN v_record_id := OLD.""DrugID"";
        ELSIF TG_TABLE_NAME = 'INVENTORY_BATCH' THEN v_record_id := OLD.""BatchID"";
        ELSIF TG_TABLE_NAME = 'SALES_ORDER' THEN v_record_id := OLD.""OrderID"";
        END IF;
    ELSIF TG_OP = 'DELETE' THEN
        v_action := 'DELETE';
        v_old_values := to_jsonb(OLD);
        IF TG_TABLE_NAME = 'EMPLOYEE' THEN v_record_id := OLD.""EID"";
        ELSIF TG_TABLE_NAME = 'SUPPLIER' THEN v_record_id := OLD.""SupplierID"";
        ELSIF TG_TABLE_NAME = 'CATEGORY' THEN v_record_id := OLD.""CategoryID"";
        ELSIF TG_TABLE_NAME = 'MEDICINE' THEN v_record_id := OLD.""DrugID"";
        ELSIF TG_TABLE_NAME = 'INVENTORY_BATCH' THEN v_record_id := OLD.""BatchID"";
        ELSIF TG_TABLE_NAME = 'SALES_ORDER' THEN v_record_id := OLD.""OrderID"";
        END IF;
    END IF;

    INSERT INTO ""AUDIT_LOG"" (""TableName"", ""RecordID"", ""Action"", ""OldValues"", ""NewValues"", ""ActionBy"", ""ActionDate"")
    VALUES (TG_TABLE_NAME, COALESCE(v_record_id, 0), v_action, v_old_values, v_new_values, NULL, NOW());

    IF TG_OP = 'DELETE' THEN RETURN OLD; ELSE RETURN NEW; END IF;
END;
$$;

CREATE OR REPLACE TRIGGER trg_audit_employee
    AFTER INSERT OR UPDATE OR DELETE ON ""EMPLOYEE"" FOR EACH ROW EXECUTE FUNCTION fn_audit_trigger();
CREATE OR REPLACE TRIGGER trg_audit_supplier
    AFTER INSERT OR UPDATE OR DELETE ON ""SUPPLIER"" FOR EACH ROW EXECUTE FUNCTION fn_audit_trigger();
CREATE OR REPLACE TRIGGER trg_audit_category
    AFTER INSERT OR UPDATE OR DELETE ON ""CATEGORY"" FOR EACH ROW EXECUTE FUNCTION fn_audit_trigger();
CREATE OR REPLACE TRIGGER trg_audit_medicine
    AFTER INSERT OR UPDATE OR DELETE ON ""MEDICINE"" FOR EACH ROW EXECUTE FUNCTION fn_audit_trigger();
CREATE OR REPLACE TRIGGER trg_audit_inventory_batch
    AFTER INSERT OR UPDATE OR DELETE ON ""INVENTORY_BATCH"" FOR EACH ROW EXECUTE FUNCTION fn_audit_trigger();
CREATE OR REPLACE TRIGGER trg_audit_sales_order
    AFTER INSERT OR UPDATE OR DELETE ON ""SALES_ORDER"" FOR EACH ROW EXECUTE FUNCTION fn_audit_trigger();
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP TRIGGER IF EXISTS trg_audit_employee ON ""EMPLOYEE"";
DROP TRIGGER IF EXISTS trg_audit_supplier ON ""SUPPLIER"";
DROP TRIGGER IF EXISTS trg_audit_category ON ""CATEGORY"";
DROP TRIGGER IF EXISTS trg_audit_medicine ON ""MEDICINE"";
DROP TRIGGER IF EXISTS trg_audit_inventory_batch ON ""INVENTORY_BATCH"";
DROP TRIGGER IF EXISTS trg_audit_sales_order ON ""SALES_ORDER"";
DROP FUNCTION IF EXISTS fn_audit_trigger();
DROP FUNCTION IF EXISTS sp_get_dashboard_summary();
DROP FUNCTION IF EXISTS sp_deduct_stock_fefo(INT, INT, INT, VARCHAR);
");
        }
    }
}
