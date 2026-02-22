export interface MonthlySales {
  year: number;
  month: number;
  totalAmount: number;
  orderCount: number;
}

export interface DashboardSummary {
  totalMedicines: number;
  lowStockCount: number;
  expiringSoonCount: number;
  todayTransactions: number;
  monthlySales: MonthlySales[];
}
