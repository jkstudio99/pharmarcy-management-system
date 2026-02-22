export interface MedicineDto {
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

export interface BatchSummaryDto {
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

export interface MedicineDetailDto extends MedicineDto {
  batches: BatchSummaryDto[];
}

export interface MedicineRequest {
  barcode?: string;
  drugName: string;
  genericName?: string;
  unit?: string;
  categoryId?: number;
  reorderLevel?: number;
  imageUrl?: string;
}
