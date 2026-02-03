import { useZylanceApi } from "@Lib/ZylanceContext"
import { useEffect, useState } from "react"

interface BackgroundTask {
  taskId: string
  progress?: number
  description?: string
}

export interface BackgroundTaskWithProgress extends BackgroundTask {
  progress: number
}

export function isTaskWithProgress(
  task: BackgroundTask,
): task is BackgroundTaskWithProgress {
  return task.progress !== undefined
}

export const useBackgroundTasks = () => {
  const zylanceApi = useZylanceApi()
  const [tasks, setTasks] = useState<Map<string, BackgroundTask>>(new Map())

  useEffect(() => {
    const subscriptions = [
      zylanceApi.background.onWorkStart((evt) => {
        setTasks((prev) => {
          const updated = new Map(prev)
          updated.set(evt.taskId, {
            taskId: evt.taskId,
            progress: undefined, // Start as indeterminate
            description: evt.description,
          })
          return updated
        })
      }),

      zylanceApi.background.onWorkProgress((evt) => {
        setTasks((prev) => {
          const updated = new Map(prev)
          const existing = updated.get(evt.taskId)
          if (existing) {
            updated.set(evt.taskId, {
              ...existing,
              progress: evt.progress,
              description: evt.description ?? existing.description,
            })
          }
          return updated
        })
      }),

      zylanceApi.background.onWorkFinish((evt) => {
        setTasks((prev) => {
          const updated = new Map(prev)
          updated.delete(evt.taskId)
          return updated
        })
      }),
    ]

    return () => {
      for (const unsub of subscriptions) {
        unsub()
      }
    }
  }, [zylanceApi])

  return tasks
}
