export function formatTime(value: string | null): string {
  if (!value) {
    return '-'
  }

  return value.split('.')[0]
}
