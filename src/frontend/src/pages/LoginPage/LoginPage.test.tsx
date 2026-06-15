import { http, HttpResponse } from 'msw'
import { setupServer } from 'msw/node'
import { beforeAll, afterAll, afterEach, describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { App } from '../../app/App'

const server = setupServer(
  http.post('http://localhost:8080/authenticate', () =>
    HttpResponse.json({
      authenticated: true,
      authorizationHeader: 'Basic token',
      username: 'vendsys',
    }),
  ),
)

describe('LoginPage', () => {
  beforeAll(() => server.listen())
  afterEach(() => {
    server.resetHandlers()
    sessionStorage.clear()
  })
  afterAll(() => server.close())

  it('validates credentials and navigates to upload', async () => {
    const user = userEvent.setup()
    window.history.pushState({}, '', '/login')

    render(<App />)

    await user.type(screen.getByLabelText(/password/i), 'NFsZGmHAGWJSZ#RuvdiV')
    await user.click(screen.getByRole('button', { name: /sign in/i }))

    expect(await screen.findByRole('heading', { name: /upload dex/i })).toBeInTheDocument()
    expect(sessionStorage.getItem('nayax-vendsys-auth')).toContain('Basic token')
  })

  it('redirects protected routes to login when not authenticated', () => {
    window.history.pushState({}, '', '/meters')

    render(<App />)

    expect(screen.getByRole('heading', { name: /sign in/i })).toBeInTheDocument()
  })
})
