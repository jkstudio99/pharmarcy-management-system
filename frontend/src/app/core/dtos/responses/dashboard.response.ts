export interface MonthlySalesResponse {
  year: number;
  month: number;
  totalAmount: number;
  orderCount: number;
}

export interface DashboardSummaryResponse {
  totalMedicines: number;
  lowStockCount: number;
  expiringSoonCount: number;
  todayTransactions: number;
  monthlySales: MonthlySalesResponse[];
}
