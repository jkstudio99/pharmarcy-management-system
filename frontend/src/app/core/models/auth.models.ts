export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  eId: number;
  empName: string;
  email: string;
  token: string;
  roles: string[];
}

export interface RegisterRequest {
  empName: string;
  email: string;
  password: string;
  roleIds: number[];
}
