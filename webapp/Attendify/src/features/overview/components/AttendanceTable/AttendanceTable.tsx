import {
  flexRender,
  getCoreRowModel,
  getPaginationRowModel,
  useReactTable,
  type ColumnDef,
} from '@tanstack/react-table'
import { Button } from '../../../../components/ui'
import type { AttendanceRecord } from '../../types/AttendanceRecord'
import './AttendanceTable.scss'

interface AttendanceTableProps {
  data: AttendanceRecord[]
  pageIndex: number
  pageSize: number
  onPageChange: (pageIndex: number) => void
}

const columns: ColumnDef<AttendanceRecord>[] = [
  {
    accessorKey: 'date',
    header: 'Date',
  },
  {
    accessorKey: 'arrivedAt',
    header: 'Arrived at',
  },
  {
    accessorKey: 'departedAt',
    header: 'Departed at',
  },
  {
    accessorKey: 'classroom',
    header: 'Classroom',
  },
  {
    accessorKey: 'status',
    header: 'Status',
  },
]

export function AttendanceTable({
  data,
  pageIndex,
  pageSize,
  onPageChange,
}: AttendanceTableProps) {
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
      }

      const nextPagination =
        typeof updater === 'function' ? updater(currentPagination) : updater

      onPageChange(nextPagination.pageIndex)
    },

    getCoreRowModel: getCoreRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
  })

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
                <td colSpan={columns.length}>No attendance records found.</td>
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
          Previous
        </Button>

        <span className="attendance-table__page">
          Page {pageIndex + 1} of {Math.max(table.getPageCount(), 1)}
        </span>

        <Button
          type="button"
          variant="secondary"
          disabled={!table.getCanNextPage()}
          onClick={() => table.nextPage()}
        >
          Next
        </Button>
      </div>
    </div>
  )
}
