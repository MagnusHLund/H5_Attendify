import { CopyField } from '../../../../components/ui'
import { useStudentAccessCode } from '../../hooks/useStudentAccessCode'
import './StudentAccessCode.scss'

export function StudentAccessCode() {
  const { data, isLoading, isError } = useStudentAccessCode()

  return (
    <div className="student-access-code">
      <div className="student-access-code__header">
        <h2>Student access code</h2>

        <p>
          Use this code when an administrator needs access to your attendance
          information.
        </p>
      </div>

      {isLoading && (
        <p className="student-access-code__status">Loading access code...</p>
      )}

      {isError && (
        <p className="student-access-code__error">
          Failed to load your student access code.
        </p>
      )}

      {data && <CopyField label="Access code" value={data.code} />}
    </div>
  )
}
