export type DexLaneMeter = {
  id: number
  productIdentifier: string
  price: number
  numberOfVends: number
  valueOfPaidSales: number
}

export type DexMeter = {
  id: number
  machineId: string
  dexDateTime: string
  machineSerialNumber: string
  valueOfPaidVends: number
  lanes: DexLaneMeter[]
}

export type UploadDexResponse = {
  dexMeterId: number
  machineId: string
  dexDateTime: string
  laneCount: number
  message: string
}
