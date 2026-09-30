export type UserType = "student" | "school_administrator";

export interface CurrentUser {
  studentId: string;
  userType: UserType;
}
