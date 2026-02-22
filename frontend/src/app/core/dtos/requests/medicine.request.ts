export interface MedicineRequest {
  barcode?: string;
  drugName: string;
  genericName?: string;
  unit?: string;
  categoryId?: number;
  reorderLevel?: number;
  imageUrl?: string;
}
