export interface SalesOrderResponse {
  orderId: number;
  referenceNo?: string;
  employeeName: string;
  customerInfo?: string;
  totalAmount: number;
  paymentMethod: string;
  status: string;
  createdAt: string;
  itemCount: number;
}

export interface SalesItemResponse {
  orderItemId: number;
  drugName: string;
  batchNumber: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface SalesOrderDetailResponse extends SalesOrderResponse {
  items: SalesItemResponse[];
}
