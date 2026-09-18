export function validEmail(message = 'Please enter a valid email address') {
  return ({ value }: { value: string }) =>
    !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value) ? message : undefined
}
