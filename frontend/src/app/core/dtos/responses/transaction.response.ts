export interface TransactionResponse {
  transactionId: number;
  batchId: number;
  batchNumber: string;
  drugName: string;
  employeeName: string;
  transType: string;
  referenceNo?: string;
  quantity: number;
  notes?: string;
  createdAt: string;
}
