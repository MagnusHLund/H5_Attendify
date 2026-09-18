export function required(message = 'This field is required') {
  return ({ value }: { value: string }) => (!value.trim() ? message : undefined)
}
