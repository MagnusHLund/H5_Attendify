import { useState } from 'react'
import { AttendanceTable } from './components/AttendanceTable/AttendanceTable'
import type { AttendanceRecord } from './types/AttendanceRecord'
import { OverviewFilters } from './components/OverviewFilters/OverviewFilters'
import { useCurrentUser } from '../auth/hooks/useCurrentUser'
import { useAttendance } from './hooks/useAttendance'
import './OverviewPage.scss'

export function OverviewPage() {
  const { data: user } = useCurrentUser()

  const [pageIndex, setPageIndex] = useState(0)
  const [pageSize, setPageSize] = useState(10)

  const isAdministrator = user?.role === 'administrator'
  const studentId = user?.studentId

  const { data: attendanceData } = useAttendance({
    studentId,
    pageIndex,
    pageSize,
  })

  const attendanceRecords: AttendanceRecord[] = attendanceData?.items ?? []

  function handlePageSizeChange(newPageSize: number) {
    setPageSize(newPageSize)
    setPageIndex(0)
  }

  return (
    <div className="overview-page">
      {isAdministrator && studentId && (
        <p className="overview-page__student">
          Looking at student: {studentId}
        </p>
      )}

      <OverviewFilters
        pageSize={pageSize}
        onPageSizeChange={handlePageSizeChange}
      />

      <AttendanceTable
        data={attendanceRecords as AttendanceRecord[]}
        pageIndex={pageIndex}
        pageSize={pageSize}
        onPageChange={setPageIndex}
      />
    </div>
  )
}
