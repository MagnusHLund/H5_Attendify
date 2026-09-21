export type UserRole = 'student' | 'school_administrator'

export interface CurrentUser {
  studentId: string
  role: UserRole
}
