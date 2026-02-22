export interface AuditLogResponse {
  logId: number;
  tableName: string;
  recordId: number;
  action: string;
  oldValues?: string;
  newValues?: string;
  actionBy?: number;
  actionByName?: string;
  actionDate: string;
}
