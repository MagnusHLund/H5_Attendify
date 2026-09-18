export type UserRole = 'student' | 'administrator'

export interface CurrentUser {
  id: string
  studentId: string
  role: UserRole
}
