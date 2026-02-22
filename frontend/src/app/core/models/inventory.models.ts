export interface InventoryBatchDto {
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

export interface StockInRequest {
  drugId: number;
  supplierId?: number;
  batchNumber: string;
  quantity: number;
  costPrice?: number;
  sellingPrice?: number;
  mfgDate?: string;
  expDate?: string;
}

export interface StockAdjustRequest {
  newQuantity: number;
  notes?: string;
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

export interface AlertsDto {
  lowStock: LowStockAlert[];
  expiringSoon: ExpiryAlert[];
}
