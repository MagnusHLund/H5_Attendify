import { useEffect } from 'react'
import {
  CopyField,
  Spinner,
  useErrorModal,
} from '../../../../components/ui'
import { useStudentAccessCode } from '../../hooks/useStudentAccessCode'
import './StudentAccessCode.scss'

export function StudentAccessCode() {
  const { data, error, isLoading } = useStudentAccessCode()
  const { showError } = useErrorModal()

  useEffect(() => {
    if (error) {
      showError(error, 'Access code could not be loaded')
    }
  }, [error, showError])

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
        <div className="student-access-code__status">
          <Spinner label="Loading access code" />
          <span>Loading access code...</span>
        </div>
      )}

      {data && <CopyField label="Access code" value={data.code} />}
    </div>
  )
}
