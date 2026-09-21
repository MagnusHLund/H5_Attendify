import { useQuery } from '@tanstack/react-query'
import { getEducationalInstitutes } from '../api/getEducationalInstitutes'

export function useEducationalInstitutes(enabled = true) {
  return useQuery({
    queryKey: ['educational-institutes'],
    queryFn: getEducationalInstitutes,
    staleTime: 60 * 60 * 1000, // 60 minutes
    retry: false,
    enabled,
  })
}
