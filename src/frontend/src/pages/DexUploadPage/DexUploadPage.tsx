import { useMutation, useQueryClient } from '@tanstack/react-query'
import { CheckCircle2, Database, UploadCloud } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../../app/providers/AuthContext'
import { uploadDexFile } from '../../shared/api/dexApi'
import { validateDexFileContent, validateDexFileMetadata } from '../../shared/lib/dexFileValidation'
import { createDexProcessingConnection, type DexProcessingEvent } from '../../shared/lib/signalr'
import { Alert } from '../../shared/ui/Alert/Alert'
import { Button } from '../../shared/ui/Button/Button'
import { FileDropzone } from '../../shared/ui/FileDropzone/FileDropzone'
import { PageHeader } from '../../shared/ui/PageHeader/PageHeader'

export function DexUploadPage() {
  const { authorizationHeader } = useAuth()
  const queryClient = useQueryClient()
  const [file, setFile] = useState<File | null>(null)
  const [fileErrors, setFileErrors] = useState<string[]>([])
  const [events, setEvents] = useState<DexProcessingEvent[]>([])
  const [connectionState, setConnectionState] = useState<'connected' | 'offline'>('offline')

  const mutation = useMutation({
    mutationFn: async () => {
      if (!file || !authorizationHeader) {
        throw new Error('A DEX file and authorization header are required.')
      }

      return uploadDexFile(file, authorizationHeader)
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['dex-meters'] })
    },
  })

  useEffect(() => {
    const connection = createDexProcessingConnection()
    let isMounted = true

    connection.on('dexProcessingEvent', (event: DexProcessingEvent) => {
      setEvents((current) => [event, ...current].slice(0, 8))
    })

    connection
      .start()
      .then(() => {
        if (isMounted) {
          setConnectionState('connected')
        }
      })
      .catch(() => {
        if (isMounted) {
          setConnectionState('offline')
        }
      })

    return () => {
      isMounted = false
      void connection.stop()
    }
  }, [])

  const latestResult = mutation.data
  const statusEvents = useMemo(() => {
    if (events.length > 0) {
      return events
    }

    return [
      {
        eventName: connectionState === 'connected' ? 'Ready' : 'Realtime offline',
        message:
          connectionState === 'connected'
            ? 'Waiting for the next DEX upload.'
            : 'Upload still works; realtime status is unavailable.',
        occurredAtUtc: new Date().toISOString(),
      },
    ]
  }, [connectionState, events])

  function handleFileChange(selectedFile: File | null) {
    setFile(selectedFile)
    setFileErrors(selectedFile ? validateDexFileMetadata(selectedFile) : [])
    mutation.reset()
  }

  async function handleSubmit() {
    if (!file) {
      setFileErrors(['Select a DEX file before uploading.'])
      return
    }

    const metadataErrors = validateDexFileMetadata(file)
    if (metadataErrors.length > 0) {
      setFileErrors(metadataErrors)
      return
    }

    const contentErrors = await validateDexFileContent(file)
    if (contentErrors.length > 0) {
      setFileErrors(contentErrors)
      return
    }

    setFileErrors([])
    setEvents([])
    mutation.mutate()
  }

  return (
    <section className="page-stack">
      <PageHeader
        title="Upload DEX"
        description="Submit NAMA DEX audit files. The API parses ID, VA and PA segments, then stores meter and lane data transactionally."
      />

      <div className="page-grid">
        <div className="panel">
          <div className="panel-body page-stack">
            <FileDropzone file={file} hasError={fileErrors.length > 0} onChange={handleFileChange} />

            {fileErrors.length > 0 ? (
              <Alert tone="danger" title="Invalid file">
                <ul className="validation-list">
                  {fileErrors.map((error) => (
                    <li key={error}>{error}</li>
                  ))}
                </ul>
              </Alert>
            ) : null}

            {mutation.isError ? (
              <Alert tone="danger" title="Upload failed">
                The file could not be processed. Check the DEX structure and try again.
              </Alert>
            ) : null}

            {latestResult ? (
              <Alert tone="success" title="DEX processed">
                Machine {latestResult.machineId} was saved with {latestResult.laneCount} lane meters.
              </Alert>
            ) : null}

            <div className="button-row">
              <Button
                type="button"
                icon={UploadCloud}
                isLoading={mutation.isPending}
                disabled={!file || fileErrors.length > 0}
                onClick={() => void handleSubmit()}
              >
                Upload file
              </Button>
              <Link to="/meters">
                <Button type="button" variant="secondary" icon={Database}>
                  View meters
                </Button>
              </Link>
            </div>
          </div>
        </div>

        <aside className="panel">
          <div className="panel-body page-stack">
            <div>
              <h2>Processing status</h2>
              <p className="muted-copy">Latest server-side events for DEX processing.</p>
            </div>
            <ul className="timeline">
              {statusEvents.map((event, index) => (
                <li key={`${event.eventName}-${event.occurredAtUtc}-${index}`}>
                  <strong>{event.eventName}</strong>
                  <span>{event.message}</span>
                </li>
              ))}
            </ul>
            {latestResult ? (
              <div className="metric-row">
                <div className="metric">
                  <span>Meter ID</span>
                  <strong>{latestResult.dexMeterId}</strong>
                </div>
                <div className="metric">
                  <span>Lanes</span>
                  <strong>{latestResult.laneCount}</strong>
                </div>
                <div className="metric">
                  <span>Status</span>
                  <strong>
                    <CheckCircle2 size={18} aria-hidden="true" /> Saved
                  </strong>
                </div>
              </div>
            ) : null}
          </div>
        </aside>
      </div>
    </section>
  )
}
