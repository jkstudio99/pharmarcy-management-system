export interface Supplier {
  supplierId: number;
  supplierName: string;
  contact?: string;
  address?: string;
  isActive: boolean;
  createdAt: string;
}

export interface SupplierRequest {
  supplierName: string;
  contact?: string;
  address?: string;
}
