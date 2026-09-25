import {
  flexRender,
  getCoreRowModel,
  getPaginationRowModel,
  useReactTable,
  type ColumnDef,
} from "@tanstack/react-table";
import { useMemo } from "react";
import { Button } from "../../../../components/ui";
import { useTranslation } from "../../../../lib/i18n";
import type { AttendanceRecord } from "../../types/AttendanceRecord";
import "./AttendanceTable.scss";

interface AttendanceTableProps {
  data: AttendanceRecord[];
  pageIndex: number;
  pageCount: number;
  pageSize: number;
  onPageChange: (pageIndex: number) => void;
}

export function AttendanceTable({
  data,
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
        <table>
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
            {table.getRowModel().rows.length > 0 ? (
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
