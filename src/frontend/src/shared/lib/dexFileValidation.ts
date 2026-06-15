export const DEX_ALLOWED_EXTENSIONS = ['.dex', '.dts', '.txt'] as const
export const DEX_MAX_FILE_SIZE_BYTES = 5 * 1024 * 1024
export const DEX_MAX_FILE_NAME_LENGTH = 120

const requiredDexSegments = ['DXS*', 'ID1*', 'ID5*', 'VA1*', 'PA1*', 'PA2*'] as const

export function validateDexFileMetadata(file: File): string[] {
  const errors: string[] = []
  const extension = getFileExtension(file.name)

  if (!DEX_ALLOWED_EXTENSIONS.includes(extension as (typeof DEX_ALLOWED_EXTENSIONS)[number])) {
    errors.push('Use a .dex, .dts or .txt file.')
  }

  if (file.name.length > DEX_MAX_FILE_NAME_LENGTH || hasUnsafeFileName(file.name)) {
    errors.push('Use a valid file name without path separators or control characters.')
  }

  if (file.size <= 0) {
    errors.push('The selected file is empty.')
  }

  if (file.size > DEX_MAX_FILE_SIZE_BYTES) {
    errors.push(`The selected file must be ${formatBytes(DEX_MAX_FILE_SIZE_BYTES)} or smaller.`)
  }

  return errors
}

export async function validateDexFileContent(file: File): Promise<string[]> {
  const content = await file.text()

  if (content.includes('\0')) {
    return ['The selected file appears to contain binary data.']
  }

  const lines = content
    .replace(/\r\n/g, '\n')
    .replace(/\r/g, '\n')
    .split('\n')
    .map((line) => line.trim())
    .filter(Boolean)

  if (!lines[0]?.toUpperCase().startsWith('DXS*')) {
    return ['The selected file is not a valid DEX audit file. The first segment must be DXS.']
  }

  const missingSegments = requiredDexSegments.filter(
    (segment) => !lines.some((line) => line.toUpperCase().startsWith(segment)),
  )

  if (missingSegments.length > 0) {
    return [`The selected file is missing required DEX segments: ${missingSegments.join(', ')}.`]
  }

  return []
}

export function formatBytes(bytes: number) {
  if (bytes < 1024) {
    return `${bytes} B`
  }

  if (bytes < 1024 * 1024) {
    return `${Math.ceil(bytes / 1024)} KB`
  }

  return `${Math.round((bytes / (1024 * 1024)) * 10) / 10} MB`
}

function getFileExtension(fileName: string) {
  const dotIndex = fileName.lastIndexOf('.')
  return dotIndex >= 0 ? fileName.slice(dotIndex).toLowerCase() : ''
}

function hasUnsafeFileName(fileName: string) {
  return [...fileName].some((character) => {
    const code = character.charCodeAt(0)
    return character === '/' || character === '\\' || code < 32
  })
}
