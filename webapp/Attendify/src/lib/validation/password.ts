export function minPasswordLength(
  length = 8,
  message = `Password must be at least ${length} characters`,
) {
  return ({ value }: { value: string }) =>
    value.length < length ? message : undefined
}
