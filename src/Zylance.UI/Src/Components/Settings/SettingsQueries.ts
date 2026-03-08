import { useQuery } from "@tanstack/react-query"
import { useZylanceQueries } from "@/Apis/Zylance/ZylanceQueryKeys"

export const useDateTimeOptions = () => {
  const queries = useZylanceQueries()

  return useQuery({
    ...queries.settings.dateTimeOptions,
    staleTime: Number.POSITIVE_INFINITY,
  })
}

export const useUserPreferences = () => {
  const queries = useZylanceQueries()

  return useQuery({
    ...queries.settings.userPreferences,
  })
}
