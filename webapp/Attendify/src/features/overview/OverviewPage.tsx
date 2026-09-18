import { useEffect, useState } from 'react'
import { Spinner, useErrorModal } from '../../components/ui'
import { AttendanceTable } from './components/AttendanceTable/AttendanceTable'
import type { AttendanceRecord } from './types/AttendanceRecord'
import { OverviewFilters } from './components/OverviewFilters/OverviewFilters'
import { useCurrentUser } from '../auth/hooks/useCurrentUser'
import { useAttendance } from './hooks/useAttendance'
import './OverviewPage.scss'

export function OverviewPage() {
  const {
    data: user,
    error: userError,
    isLoading: isUserLoading,
  } = useCurrentUser()
  const { showError } = useErrorModal()

  const [pageIndex, setPageIndex] = useState(0)
  const [pageSize, setPageSize] = useState(10)

  const isAdministrator = user?.role === 'administrator'
  const studentId = user?.studentId

  const {
    data: attendanceData,
    error: attendanceError,
    isLoading: isAttendanceLoading,
  } = useAttendance({
    studentId,
    pageIndex,
    pageSize,
  })

  const attendanceRecords: AttendanceRecord[] = attendanceData?.items ?? []

  useEffect(() => {
    if (userError) {
      showError(userError, 'User details could not be loaded')
    }
  }, [showError, userError])

  useEffect(() => {
    if (attendanceError) {
      showError(attendanceError, 'Attendance could not be loaded')
    }
  }, [attendanceError, showError])

  function handlePageSizeChange(newPageSize: number) {
    setPageSize(newPageSize)
    setPageIndex(0)
  }

  return (
    <div className="overview-page">
      {(isUserLoading || isAttendanceLoading) && (
        <div className="overview-page__loading">
          <Spinner size="large" label="Loading attendance" />
        </div>
      )}

      {isAdministrator && studentId && (
        <p className="overview-page__student">
          Looking at student: {studentId}
        </p>
      )}

      {!isUserLoading && !isAttendanceLoading && (
        <>
          <OverviewFilters
            pageSize={pageSize}
            onPageSizeChange={handlePageSizeChange}
          />

          <AttendanceTable
            data={attendanceRecords}
            pageIndex={pageIndex}
            pageSize={pageSize}
            onPageChange={setPageIndex}
          />
        </>
      )}
    </div>
  )
}
