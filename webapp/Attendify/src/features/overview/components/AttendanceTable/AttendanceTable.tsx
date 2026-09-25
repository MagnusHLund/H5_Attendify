import {
  flexRender,
  getCoreRowModel,
  useReactTable,
  type ColumnDef,
} from '@tanstack/react-table'
import Skeleton from 'react-loading-skeleton'
import 'react-loading-skeleton/dist/skeleton.css'
import { useMemo } from 'react'
import { Button } from '../../../../components/ui'
import { useTranslation } from '../../../../lib/i18n'
import type { AttendanceRecord } from '../../types/AttendanceRecord'
import './AttendanceTable.scss'

const skeletonWidths = ['65%', '50%', '50%', '60%', '55%']

interface AttendanceTableProps {
  data: AttendanceRecord[];
  isLoading: boolean;
  pageIndex: number;
  pageCount: number;
  pageSize: number;
  onPageChange: (pageIndex: number) => void;
}

export function AttendanceTable({
  data,
  isLoading,
  pageIndex,
  pageCount,
  pageSize,
  onPageChange,
}: AttendanceTableProps) {
  const { t } = useTranslation();
  const columns = useMemo<ColumnDef<AttendanceRecord>[]>(
    () => [
      { accessorKey: "attendanceDate", header: t("overview.date") },
      { accessorKey: "arrivedAt", header: t("overview.arrived") },
      { accessorKey: "departedAt", header: t("overview.departed") },
      { accessorKey: "classroom", header: t("overview.classroom") },
      { accessorKey: "status", header: t("overview.status") },
    ],
    [t],
  );

  const table = useReactTable({
    data,
    columns,

    state: {
      pagination: {
        pageIndex,
        pageSize,
      },
    },

    onPaginationChange: (updater) => {
      const currentPagination = {
        pageIndex,
        pageSize,
      };

      const nextPagination =
        typeof updater === "function" ? updater(currentPagination) : updater;

      onPageChange(nextPagination.pageIndex);
    },

    getCoreRowModel: getCoreRowModel(),
    manualPagination: true,
    pageCount,
  });

  return (
    <div className="attendance-table">
      <div className="attendance-table__container">
        {isLoading && (
          <span className="attendance-table__loading-status" role="status">
            {isLoading ? t('overview.loading') : ''}
          </span>
        )}
        <table aria-busy={isLoading}>
          <thead>
            {table.getHeaderGroups().map((headerGroup) => (
              <tr key={headerGroup.id}>
                {headerGroup.headers.map((header) => (
                  <th key={header.id}>
                    {flexRender(
                      header.column.columnDef.header,
                      header.getContext(),
                    )}
                  </th>
                ))}
              </tr>
            ))}
          </thead>

          <tbody>
            {isLoading ? (
              Array.from({ length: pageSize }, (_, rowIndex) => (
                <tr key={`skeleton-${rowIndex}`} aria-hidden="true">
                  {columns.map((_, columnIndex) => (
                    <td key={columnIndex}>
                      <Skeleton
                        className="attendance-table__skeleton"
                        height="1rem"
                        width={skeletonWidths[columnIndex]}
                      />
                    </td>
                  ))}
                </tr>
              ))
            ) : table.getRowModel().rows.length > 0 ? (
              table.getRowModel().rows.map((row) => (
                <tr key={row.id}>
                  {row.getVisibleCells().map((cell) => (
                    <td key={cell.id}>
                      {flexRender(
                        cell.column.columnDef.cell,
                        cell.getContext(),
                      )}
                    </td>
                  ))}
                </tr>
              ))
            ) : (
              <tr>
                <td colSpan={columns.length}>{t("overview.empty")}</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      <div className="attendance-table__pagination">
        <Button
          type="button"
          variant="secondary"
          disabled={!table.getCanPreviousPage()}
          onClick={() => table.previousPage()}
        >
          {t("overview.previous")}
        </Button>

        <span className="attendance-table__page">
          {t("overview.page", {
            current: pageIndex + 1,
            total: Math.max(table.getPageCount(), 1),
          })}
        </span>

        <Button
          type="button"
          variant="secondary"
          disabled={!table.getCanNextPage()}
          onClick={() => table.nextPage()}
        >
          {t("overview.next")}
        </Button>
      </div>
    </div>
  );
}
