import { Dropdown } from '../../../../components/ui'
import './OverviewFilters.scss'

interface OverviewFiltersProps {
  pageSize: number
  onPageSizeChange: (pageSize: number) => void
}

export function OverviewFilters({
  pageSize,
  onPageSizeChange,
}: OverviewFiltersProps) {
  return (
    <div className="overview-filters">
      <Dropdown
        label="Rows per page"
        value={String(pageSize)}
        options={[
          { value: '10', label: '10' },
          { value: '25', label: '25' },
          { value: '50', label: '50' },
          { value: '100', label: '100' },
        ]}
        onChange={(event) => {
          onPageSizeChange(Number(event.target.value))
        }}
      />
    </div>
  )
}
