import { fetchApi } from '../../../lib/api/client'
import type { AttendanceResponse } from '../types/attendance'

interface GetAttendanceOptions {
  studentId: string
  pageIndex: number
  pageSize: number
}

export async function getAttendance({
  studentId,
  pageIndex,
  pageSize,
}: GetAttendanceOptions): Promise<AttendanceResponse> {
  const params = new URLSearchParams({
    studentId,
    page: String(pageIndex + 1),
    pageSize: String(pageSize),
  })

  const response = await fetchApi(`/api/attendance?${params}`, {
    credentials: 'include',
  })

  if (!response.ok) {
    throw new Error('Failed to fetch attendance')
  }

  return response.json()
}
