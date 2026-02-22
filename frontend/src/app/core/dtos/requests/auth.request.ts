export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  empName: string;
  email: string;
  password: string;
  roleIds: number[];
}
