import { useQuery } from '@tanstack/react-query'
import { getAttendance } from '../api/getAttendance'

interface UseAttendanceOptions {
  studentId?: string
  pageIndex: number
  pageSize: number
}

export function useAttendance({
  studentId,
  pageIndex,
  pageSize,
}: UseAttendanceOptions) {
  return useQuery({
    queryKey: ['attendance', studentId, pageIndex, pageSize],
    queryFn: () =>
      getAttendance({
        studentId: studentId!,
        pageIndex,
        pageSize,
      }),
    enabled: Boolean(studentId),
  })
}
