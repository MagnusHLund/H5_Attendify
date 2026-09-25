import { useEffect, useRef, useState } from "react";
import { useErrorModal } from "../../components/ui";
import { AttendanceTable } from "./components/AttendanceTable/AttendanceTable";
import type { AttendanceRecord } from "./types/AttendanceRecord";
import { OverviewFilters } from "./components/OverviewFilters/OverviewFilters";
import { useCurrentUser } from "../auth/hooks/useCurrentUser";
import { useAttendance } from "./hooks/useAttendance";
import { useTranslation } from "../../lib/i18n";
import "./OverviewPage.scss";

export function OverviewPage() {
  const {
    data: user,
    error: userError,
    isLoading: isUserLoading,
  } = useCurrentUser();
  const { showError } = useErrorModal();
  const { t } = useTranslation();

  const [pageIndex, setPageIndex] = useState(0);
  const [pageSize, setPageSize] = useState(10);

  const handledUserErrorRef = useRef<Error | null>(null);
  const handledAttendanceErrorRef = useRef<Error | null>(null);

  const isAdministrator = user?.role === "school_administrator";
  const studentId = user?.studentId;

  const {
    data: attendanceData,
    error: attendanceError,
    isLoading: isAttendanceLoading,
  } = useAttendance({
    studentId,
    pageIndex,
    pageSize,
  });

  const attendanceRecords: AttendanceRecord[] = attendanceData?.items ?? [];

  const pageCount = Math.ceil((attendanceData?.totalCount ?? 0) / pageSize);

  useEffect(() => {
    if (!userError || handledUserErrorRef.current === userError) {
      return;
    }

    handledUserErrorRef.current = userError;

    showError(new Error(t("error.userMessage")), t("error.userTitle"));
  }, [showError, t, userError]);

  useEffect(() => {
    if (
      !attendanceError ||
      handledAttendanceErrorRef.current === attendanceError
    ) {
      return;
    }

    handledAttendanceErrorRef.current = attendanceError;

    showError(
      new Error(t("error.attendanceMessage")),
      t("error.attendanceTitle"),
    );
  }, [attendanceError, showError, t]);

  function handlePageSizeChange(newPageSize: number) {
    setPageSize(newPageSize);
    setPageIndex(0);
  }

  return (
    <div className="overview-page">
      {isAdministrator && studentId && (
        <p className="overview-page__student">
          {t("overview.student", { studentId })}
        </p>
      )}

      <>
        <OverviewFilters
          pageSize={pageSize}
          onPageSizeChange={handlePageSizeChange}
        />

        <AttendanceTable
          data={attendanceRecords}
          isLoading={isUserLoading || isAttendanceLoading}
          pageIndex={pageIndex}
          pageCount={pageCount}
          pageSize={pageSize}
          onPageChange={setPageIndex}
        />
      </>
    </div>
  );
}
