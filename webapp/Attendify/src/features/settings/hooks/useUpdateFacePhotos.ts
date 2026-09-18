import { useMutation } from '@tanstack/react-query'
import { updateFacePhotos } from '../api/updateFacePhotos'

export function useUpdateFacePhotos() {
  return useMutation({
    mutationFn: updateFacePhotos,
  })
}
