import type { DexMeter, UploadDexResponse } from '../../entities/dex/types'
import { httpClient } from './httpClient'

export async function uploadDexFile(file: File, authorizationHeader: string) {
  const data = new FormData()
  data.append('file', file)

  const response = await httpClient.post<UploadDexResponse>('/dex', data, {
    headers: {
      Authorization: authorizationHeader,
    },
  })

  return response.data
}

export async function getDexMeters(authorizationHeader: string) {
  const response = await httpClient.get<DexMeter[]>('/dex', {
    headers: {
      Authorization: authorizationHeader,
    },
  })

  return response.data
}

export async function clearDexMeters(authorizationHeader: string) {
  await httpClient.delete('/dex', {
    headers: {
      Authorization: authorizationHeader,
    },
  })
}
