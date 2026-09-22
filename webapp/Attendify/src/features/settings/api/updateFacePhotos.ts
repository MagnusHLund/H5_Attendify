interface UpdateFacePhotosOptions {
  straightPhoto: File
  leftPhoto: File
  rightPhoto: File
}

export async function updateFacePhotos({
  straightPhoto,
  leftPhoto,
  rightPhoto,
}: UpdateFacePhotosOptions): Promise<void> {
  // TODO: Rewrite this to use fetchApi and use json instead of form data
  const formData = new FormData()

  formData.append('straightPhoto', straightPhoto)
  formData.append('leftPhoto', leftPhoto)
  formData.append('rightPhoto', rightPhoto)

  const response = await fetch('/api/settings/face-photos', {
    method: 'PUT',
    credentials: 'include',
    body: formData,
  })

  if (!response.ok) {
    throw new Error('Failed to update face pictures')
  }
}
