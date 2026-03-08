import * as dateFns from "date-fns"
import type { FC } from "react"
import { useZylance } from "@/Components/Application/UseZylance"

export interface TimestampProps {
  timestamp: Date
  dateOnly?: boolean
  timeOnly?: boolean
}

function convertCSharpToDateFns (
  pattern: string | undefined,
): string | undefined {
  if (!pattern) return undefined

  return pattern
    .replace(/tt/g, "a")
    .replace(/f{1,7}/g, (m) => "S".repeat(m.length))
}

export const useTimestamp = (
  timestamp: Date,
  options: Omit<TimestampProps, "timestamp"> = {},
): string => {
  const { userPreferences } = useZylance().settings
  const { datePattern: rawDatePattern, timePattern: rawTimePattern } =
  userPreferences.dateTimeFormat || {}

  const datePattern = convertCSharpToDateFns(rawDatePattern)
  const timePattern = convertCSharpToDateFns(rawTimePattern)

  const formatDate = () => {
    if (!datePattern) return timestamp.toLocaleDateString()
    return dateFns.format(timestamp, datePattern)
  }
  const formatTime = () => {
    if (!timePattern) return timestamp.toLocaleTimeString()
    return dateFns.format(timestamp, timePattern)
  }

  if (options.dateOnly) {
    return formatDate()
  }

  if (options.timeOnly) {
    return formatTime()
  }

  return `${formatDate()} ${formatTime()}`
}

export const Timestamp: FC<TimestampProps> = ({
  timestamp,
  dateOnly,
  timeOnly,
}) => {
  return useTimestamp(timestamp, { dateOnly, timeOnly })
}
