export interface Category {
  categoryId: number;
  categoryName: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
}

export interface CategoryRequest {
  categoryName: string;
  description?: string;
}
