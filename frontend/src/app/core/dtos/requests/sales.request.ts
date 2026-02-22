export interface SalesOrderItemRequest {
  drugId: number;
  quantity: number;
}

export interface CreateSalesOrderRequest {
  customerInfo?: string;
  paymentMethod: string;
  items: SalesOrderItemRequest[];
}
