import { useQuery } from "@tanstack/react-query"
import { useZylanceQueries } from "@/Apis/Zylance/ZylanceQueryKeys"

export interface UseAccountCurrencyOptions {
  accountId: string | undefined
}

export const useAccountCurrency = ({ accountId }: UseAccountCurrencyOptions) => {
  const zylanceQueries = useZylanceQueries()
  const accountQuery = useQuery({
    ...zylanceQueries.accounts.get(accountId || ""),
    enabled: !!accountId,
  })

  return {
    ...accountQuery,
    currency: accountQuery.data?.currency,
  }
}
