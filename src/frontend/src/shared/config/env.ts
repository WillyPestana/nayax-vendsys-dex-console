const bundledSyncfusionLicenseKey =
  'Ngo9BigBOggjHTQxAR8/V1JHaF1cXmhPYVF3WmFZfVhgdVVMYFpbR39PIiBoS35RcEVlWHtccnRTRWlVV0F/VEFe'

export const env = {
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8080',
  signalRHubUrl: import.meta.env.VITE_SIGNALR_HUB_URL ?? 'http://localhost:8080/hubs/dex-processing',
  syncfusionLicenseKey: import.meta.env.VITE_SYNCFUSION_LICENSE_KEY || bundledSyncfusionLicenseKey,
} as const
