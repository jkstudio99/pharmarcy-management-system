export interface EmployeeDto {
  eId: number;
  empName: string;
  email: string;
  isActive: boolean;
  createdAt: string;
  roles: string[];
}

export interface UpdateEmployeeRequest {
  empName: string;
  email: string;
  isActive: boolean;
  newPassword?: string;
  roleIds?: number[];
}
