import { FileUp } from 'lucide-react'
import type { ChangeEvent } from 'react'
import { DEX_ALLOWED_EXTENSIONS, DEX_MAX_FILE_SIZE_BYTES, formatBytes } from '../../lib/dexFileValidation'

type FileDropzoneProps = {
  file: File | null
  accept?: string
  hasError?: boolean
  onChange: (file: File | null) => void
}

export function FileDropzone({
  file,
  accept = DEX_ALLOWED_EXTENSIONS.join(','),
  hasError = false,
  onChange,
}: FileDropzoneProps) {
  function handleChange(event: ChangeEvent<HTMLInputElement>) {
    onChange(event.target.files?.[0] ?? null)
    event.target.value = ''
  }

  return (
    <label className={`file-dropzone ${hasError ? 'file-dropzone-error' : ''}`.trim()}>
      <input type="file" accept={accept} onChange={handleChange} />
      <FileUp size={28} aria-hidden="true" />
      <span>{file ? file.name : 'Select a DEX file'}</span>
      <small>
        {file
          ? formatBytes(file.size)
          : `Supported: ${DEX_ALLOWED_EXTENSIONS.join(', ')} up to ${formatBytes(DEX_MAX_FILE_SIZE_BYTES)}`}
      </small>
    </label>
  )
}
