export interface SalesOrderDto {
  orderId: number;
  referenceNo: string;
  employeeName: string;
  customerInfo?: string;
  totalAmount: number;
  paymentMethod: string;
  status: string;
  createdAt: string;
  itemCount: number;
}

export interface SalesItemDto {
  itemId: number;
  drugName: string;
  batchNumber: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface SalesOrderDetailDto extends SalesOrderDto {
  items: SalesItemDto[];
}

export interface SalesOrderItemRequest {
  drugId: number;
  quantity: number;
}

export interface CreateSalesOrderRequest {
  customerInfo?: string;
  paymentMethod: string;
  items: SalesOrderItemRequest[];
}
