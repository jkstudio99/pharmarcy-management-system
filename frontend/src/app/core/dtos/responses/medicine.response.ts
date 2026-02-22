export interface MedicineResponse {
  drugId: number;
  barcode?: string;
  drugName: string;
  genericName?: string;
  unit?: string;
  categoryId?: number;
  categoryName?: string;
  reorderLevel: number;
  imageUrl?: string;
  totalStock: number;
  isLowStock: boolean;
  createdAt: string;
}

export interface BatchSummaryResponse {
  batchId: number;
  batchNumber: string;
  quantityInStock: number;
  costPrice?: number;
  sellingPrice?: number;
  mfgDate?: string;
  expDate?: string;
  supplierName?: string;
  isExpiringSoon: boolean;
}

export interface MedicineDetailResponse extends MedicineResponse {
  batches: BatchSummaryResponse[];
}
