import { HubConnectionBuilder, LogLevel, type HubConnection } from '@microsoft/signalr'
import { env } from '../config/env'

export type DexProcessingEvent = {
  eventName: string
  message: string
  occurredAtUtc: string
}

export function createDexProcessingConnection(): HubConnection {
  return new HubConnectionBuilder()
    .withUrl(env.signalRHubUrl)
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build()
}
