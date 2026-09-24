import type { AttendanceRecord } from "./AttendanceRecord";

export interface AttendanceResponse {
  items: AttendanceRecord[]
  totalCount: number
}
