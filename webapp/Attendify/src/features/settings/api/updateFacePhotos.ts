import { fetchApi } from '../../../lib/api/client'

interface UpdateFacePhotosOptions {
  straightPhoto: string
  leftPhoto: string
  rightPhoto: string
}

export async function updateFacePhotos({
  straightPhoto,
  leftPhoto,
  rightPhoto,
}: UpdateFacePhotosOptions): Promise<void> {
  const payload = {
    straightPhoto,
    leftPhoto,
    rightPhoto,
  }

  const response = await fetchApi('/api/settings/face-photos', {
    method: 'PATCH',
    body: JSON.stringify(payload),
    headers: {
      'Content-Type': 'application/json',
    },
  })

  if (!response.ok) {
    throw new Error('Failed to update face pictures')
  }
}
