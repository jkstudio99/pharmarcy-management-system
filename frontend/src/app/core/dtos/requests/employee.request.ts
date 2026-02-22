export interface UpdateEmployeeRequest {
  empName: string;
  email: string;
  isActive: boolean;
  newPassword?: string;
  roleIds?: number[];
}
