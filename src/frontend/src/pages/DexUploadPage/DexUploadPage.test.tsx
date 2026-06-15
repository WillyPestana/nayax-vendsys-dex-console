import { http, HttpResponse } from 'msw'
import { setupServer } from 'msw/node'
import { beforeAll, afterAll, afterEach, describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { App } from '../../app/App'

vi.mock('../../shared/lib/signalr', () => ({
  createDexProcessingConnection: () => ({
    on: vi.fn(),
    start: vi.fn().mockRejectedValue(new Error('offline')),
    stop: vi.fn().mockResolvedValue(undefined),
  }),
}))

const server = setupServer(
  http.post('http://localhost:8080/dex', () =>
    HttpResponse.json(
      {
        dexMeterId: 42,
        machineId: '100077238',
        dexDateTime: '2023-12-10T23:10:53',
        laneCount: 38,
        message: 'DEX file processed successfully.',
      },
      { status: 201 },
    ),
  ),
)

describe('DexUploadPage', () => {
  beforeAll(() => server.listen())
  afterEach(() => {
    server.resetHandlers()
    sessionStorage.clear()
  })
  afterAll(() => server.close())

  it('uploads a selected DEX file and shows success state', async () => {
    const user = userEvent.setup()
    sessionStorage.setItem(
      'nayax-vendsys-auth',
      JSON.stringify({ username: 'vendsys', authorizationHeader: 'Basic token' }),
    )
    window.history.pushState({}, '', '/upload')

    render(<App />)

    const file = new File(
      ['DXS*STF0000000*VA*V0/6*1\nID1*100077238\nID5*20231210*2310*53\nVA1*100*1\nPA1*101*325\nPA2*1*325'],
      'machine-a.txt',
      { type: 'text/plain' },
    )
    await user.upload(screen.getByLabelText(/select a dex file/i), file)
    await user.click(screen.getByRole('button', { name: /upload file/i }))

    expect(await screen.findByText(/Machine 100077238 was saved with 38 lane meters/i)).toBeInTheDocument()
  })

  it('blocks files with unsupported extensions before upload', async () => {
    const user = userEvent.setup({ applyAccept: false })
    sessionStorage.setItem(
      'nayax-vendsys-auth',
      JSON.stringify({ username: 'vendsys', authorizationHeader: 'Basic token' }),
    )
    window.history.pushState({}, '', '/upload')

    render(<App />)

    const file = new File(['not a dex'], 'machine-a.pdf', { type: 'application/pdf' })
    await user.upload(screen.getByLabelText(/select a dex file/i), file)

    expect(await screen.findByText('Invalid file')).toBeInTheDocument()
    expect(screen.getByText('Use a .dex, .dts or .txt file.')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /upload file/i })).toBeDisabled()
  })

  it('blocks text files that do not look like DEX content', async () => {
    const user = userEvent.setup()
    sessionStorage.setItem(
      'nayax-vendsys-auth',
      JSON.stringify({ username: 'vendsys', authorizationHeader: 'Basic token' }),
    )
    window.history.pushState({}, '', '/upload')

    render(<App />)

    const file = new File(['plain text'], 'machine-a.txt', { type: 'text/plain' })
    await user.upload(screen.getByLabelText(/select a dex file/i), file)
    await user.click(screen.getByRole('button', { name: /upload file/i }))

    expect(await screen.findByText(/first segment must be DXS/i)).toBeInTheDocument()
  })
})
