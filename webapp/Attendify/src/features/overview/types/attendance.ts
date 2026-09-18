export interface AttendanceRecord {
  id: string
  date: string
  arrivedAt: string
  departedAt: string
  classroom: string
  status: string
}

export interface AttendanceResponse {
  items: AttendanceRecord[]
  totalCount: number
}
