import { http, HttpResponse } from 'msw'
import { setupServer } from 'msw/node'
import type { ReactNode } from 'react'
import userEvent from '@testing-library/user-event'
import { beforeAll, afterAll, afterEach, describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { App } from '../../app/App'

vi.mock('@syncfusion/ej2-react-grids', () => ({
  GridComponent: ({ dataSource, children }: { dataSource: Array<{ machineId: string }>; children: ReactNode }) => (
    <div data-testid="dex-grid">
      {children}
      {dataSource.map((row) => (
        <span key={row.machineId}>{row.machineId}</span>
      ))}
    </div>
  ),
  ColumnsDirective: ({ children }: { children: ReactNode }) => <>{children}</>,
  ColumnDirective: () => null,
  Inject: () => null,
  Sort: function Sort() {},
  Filter: function Filter() {},
  DetailRow: function DetailRow() {},
}))

const server = setupServer(
  http.get('http://localhost:8080/dex', () =>
    HttpResponse.json([
      {
        id: 1,
        machineId: '100077238',
        dexDateTime: '2023-12-10T23:10:53',
        machineSerialNumber: '100077238',
        valueOfPaidVends: 344.5,
        lanes: [{ id: 1, productIdentifier: '101', price: 3.25, numberOfVends: 4, valueOfPaidSales: 13 }],
      },
    ]),
  ),
  http.delete('http://localhost:8080/dex', () => new HttpResponse(null, { status: 204 })),
)

describe('DexMetersPage', () => {
  beforeAll(() => server.listen())
  afterEach(() => {
    server.resetHandlers()
    sessionStorage.clear()
    vi.restoreAllMocks()
  })
  afterAll(() => server.close())

  it('renders the grid with API data', async () => {
    sessionStorage.setItem(
      'nayax-vendsys-auth',
      JSON.stringify({ username: 'vendsys', authorizationHeader: 'Basic token' }),
    )
    window.history.pushState({}, '', '/meters')

    render(<App />)

    expect(await screen.findByTestId('dex-grid')).toBeInTheDocument()
    expect(screen.getByText('100077238')).toBeInTheDocument()
  })

  it('clears persisted DEX data after confirmation', async () => {
    const user = userEvent.setup()
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true)
    sessionStorage.setItem(
      'nayax-vendsys-auth',
      JSON.stringify({ username: 'vendsys', authorizationHeader: 'Basic token' }),
    )
    window.history.pushState({}, '', '/meters')

    render(<App />)

    await screen.findByTestId('dex-grid')
    await user.click(screen.getByRole('button', { name: /reset data/i }))

    expect(confirmSpy).toHaveBeenCalled()
    expect(await screen.findByText('Data reset')).toBeInTheDocument()
  })
})
