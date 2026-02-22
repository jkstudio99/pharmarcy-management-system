export interface InventoryBatchResponse {
  batchId: number;
  batchNumber: string;
  drugId: number;
  drugName: string;
  supplierId?: number;
  supplierName?: string;
  quantityInStock: number;
  costPrice?: number;
  sellingPrice?: number;
  mfgDate?: string;
  expDate?: string;
  isActive: boolean;
  isExpiringSoon: boolean;
  createdAt: string;
}

export interface LowStockAlert {
  drugId: number;
  drugName: string;
  totalStock: number;
  reorderLevel: number;
}

export interface ExpiryAlert {
  batchId: number;
  batchNumber: string;
  drugName: string;
  quantityInStock: number;
  expDate: string;
  daysUntilExpiry: number;
}

export interface AlertsResponse {
  lowStockAlerts: LowStockAlert[];
  expiryAlerts: ExpiryAlert[];
}
