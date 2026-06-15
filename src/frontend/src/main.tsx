import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { registerLicense } from '@syncfusion/ej2-base'
import '@syncfusion/ej2-base/styles/material.css'
import '@syncfusion/ej2-react-grids/styles/material.css'
import { App } from './app/App'
import { env } from './shared/config/env'
import './shared/styles/global.css'

if (env.syncfusionLicenseKey) {
  registerLicense(env.syncfusionLicenseKey)
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
