import { useQuery } from '@tanstack/react-query'
import { getEducationalInstitutes } from '../api/getEducationalInstitutes'

export function useEducationalInstitutes(enabled = true) {
  return useQuery({
    queryKey: ['educational-institutes'],
    queryFn: getEducationalInstitutes,
    retry: false,
    enabled,
  })
}
