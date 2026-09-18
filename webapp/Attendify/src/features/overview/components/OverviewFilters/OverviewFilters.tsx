import { Dropdown } from '../../../../components/ui'
import { useTranslation } from '../../../../lib/i18n'
import './OverviewFilters.scss'

interface OverviewFiltersProps {
  pageSize: number
  onPageSizeChange: (pageSize: number) => void
}

export function OverviewFilters({
  pageSize,
  onPageSizeChange,
}: OverviewFiltersProps) {
  const { t } = useTranslation()

  return (
    <div className="overview-filters">
      <Dropdown
        label={t('overview.rowsPerPage')}
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
