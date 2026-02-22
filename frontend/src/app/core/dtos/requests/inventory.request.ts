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

export interface StockOutFefoRequest {
  drugId: number;
  quantity: number;
  referenceNo?: string;
}

export interface StockAdjustRequest {
  batchId: number;
  newQuantity: number;
  reason?: string;
}
