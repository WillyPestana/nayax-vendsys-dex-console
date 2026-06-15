import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  ColumnDirective,
  ColumnsDirective,
  DetailRow,
  Filter,
  GridComponent,
  Inject,
  Sort,
} from '@syncfusion/ej2-react-grids'
import { RefreshCcw, Trash2 } from 'lucide-react'
import { useAuth } from '../../app/providers/AuthContext'
import type { DexLaneMeter, DexMeter } from '../../entities/dex/types'
import { clearDexMeters, getDexMeters } from '../../shared/api/dexApi'
import { formatCurrency, formatDateTime } from '../../shared/lib/formatters'
import { Alert } from '../../shared/ui/Alert/Alert'
import { Button } from '../../shared/ui/Button/Button'
import { PageHeader } from '../../shared/ui/PageHeader/PageHeader'
import { Spinner } from '../../shared/ui/Spinner/Spinner'

export function DexMetersPage() {
  const { authorizationHeader } = useAuth()
  const queryClient = useQueryClient()
  const query = useQuery({
    queryKey: ['dex-meters'],
    queryFn: () => getDexMeters(authorizationHeader ?? ''),
    enabled: Boolean(authorizationHeader),
  })
  const clearMutation = useMutation({
    mutationFn: () => clearDexMeters(authorizationHeader ?? ''),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['dex-meters'] })
    },
  })

  function handleClear() {
    if (!window.confirm('Clear all persisted DEX meters and lanes?')) {
      return
    }

    clearMutation.mutate()
  }

  return (
    <section className="page-stack">
      <PageHeader
        title="DEX Meters"
        description="Review persisted DEX meters and expand each row to inspect related lane meters."
        actions={
          <div className="button-row">
            <Button type="button" variant="secondary" icon={RefreshCcw} onClick={() => void query.refetch()}>
              Refresh
            </Button>
            <Button
              type="button"
              variant="danger"
              icon={Trash2}
              isLoading={clearMutation.isPending}
              disabled={!query.data?.length}
              onClick={handleClear}
            >
              Reset data
            </Button>
          </div>
        }
      />

      {clearMutation.isError ? (
        <Alert tone="danger" title="Reset failed">
          The stored DEX data could not be cleared. Check the API connection and try again.
        </Alert>
      ) : null}

      {clearMutation.isSuccess ? (
        <Alert tone="success" title="Data reset">
          Persisted DEX meters and lane meters were cleared.
        </Alert>
      ) : null}

      {query.isLoading ? (
        <div className="panel">
          <div className="panel-body">
            <Spinner />
          </div>
        </div>
      ) : null}

      {query.isError ? (
        <Alert tone="danger" title="Unable to load DEX meters">
          Check the API connection and authentication state.
        </Alert>
      ) : null}

      {query.data ? (
        <div className="panel grid-panel">
          <GridComponent
            dataSource={query.data}
            allowSorting
            allowFiltering
            filterSettings={{ type: 'Menu' }}
            detailTemplate={laneDetailTemplate}
            height="auto"
          >
            <ColumnsDirective>
              <ColumnDirective field="id" headerText="ID" width="90" textAlign="Right" />
              <ColumnDirective field="machineId" headerText="Machine ID" width="150" />
              <ColumnDirective field="machineSerialNumber" headerText="Serial Number" width="160" />
              <ColumnDirective
                field="dexDateTime"
                headerText="DEX Date/Time"
                width="210"
                valueAccessor={(_, data) => formatDateTime((data as DexMeter).dexDateTime)}
              />
              <ColumnDirective
                field="valueOfPaidVends"
                headerText="Value of Paid Vends"
                width="190"
                textAlign="Right"
                valueAccessor={(_, data) => formatCurrency((data as DexMeter).valueOfPaidVends)}
              />
              <ColumnDirective
                field="lanes"
                headerText="Lanes"
                width="100"
                textAlign="Right"
                valueAccessor={(_, data) => String((data as DexMeter).lanes.length)}
              />
            </ColumnsDirective>
            <Inject services={[Sort, Filter, DetailRow]} />
          </GridComponent>
        </div>
      ) : null}
    </section>
  )
}

function laneDetailTemplate(data: DexMeter) {
  return (
    <div className="lane-detail">
      <table>
        <thead>
          <tr>
            <th>Product</th>
            <th>Price</th>
            <th>Vends</th>
            <th>Paid sales</th>
          </tr>
        </thead>
        <tbody>
          {data.lanes.map((lane: DexLaneMeter) => (
            <tr key={lane.id}>
              <td>{lane.productIdentifier}</td>
              <td>{formatCurrency(lane.price)}</td>
              <td>{lane.numberOfVends}</td>
              <td>{formatCurrency(lane.valueOfPaidSales)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
