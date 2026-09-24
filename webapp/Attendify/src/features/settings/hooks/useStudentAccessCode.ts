import { useQuery } from '@tanstack/react-query'
import { getStudentAccessCode } from '../api/getStudentAccessCode'

export function useStudentAccessCode() {
  return useQuery({
    queryKey: ['student-access-code'],
    queryFn: getStudentAccessCode,
    staleTime: 1000 * 60 * 5, // 5 minutes
  })
}
