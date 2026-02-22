export interface SupplierResponse {
  supplierId: number;
  supplierName: string;
  contact?: string;
  address?: string;
  isActive: boolean;
  createdAt: string;
}
