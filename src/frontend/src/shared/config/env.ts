export const env = {
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8080',
  signalRHubUrl: import.meta.env.VITE_SIGNALR_HUB_URL ?? 'http://localhost:8080/hubs/dex-processing',
  syncfusionLicenseKey: import.meta.env.VITE_SYNCFUSION_LICENSE_KEY ?? '',
} as const
