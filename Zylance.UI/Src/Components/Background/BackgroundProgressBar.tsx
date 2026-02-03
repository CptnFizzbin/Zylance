import { LinearProgress } from "@mui/material"
import type { FC } from "react"
import { isTaskWithProgress, useBackgroundTasks } from "./useBackgroundTasks"

export const BackgroundProgressBar: FC = () => {
  const tasks = useBackgroundTasks()

  const hasActiveTasks = tasks.size > 0

  if (!hasActiveTasks) {
    return null
  }

  const tasksWithProgress = Array.from(tasks.values()).filter(
    isTaskWithProgress,
  )

  const hasProgress = tasksWithProgress.length > 0

  const aggregateProgress = hasProgress
    ? tasksWithProgress.reduce((sum, task) => sum + task.progress, 0) /
      tasksWithProgress.length
    : 0

  return (
    <LinearProgress
      variant={hasProgress ? "determinate" : "indeterminate"}
      value={hasProgress ? aggregateProgress * 100 : undefined}
      sx={{
        position: "absolute",
        top: 0,
        left: 0,
        right: 0,
        zIndex: (theme) => theme.zIndex.appBar + 1,
        height: 3,
        backgroundColor: "transparent",
      }}
    />
  )
}
