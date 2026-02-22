import { createQueryKeys } from "@lukemorales/query-key-factory"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { useZylanceApi } from "@/Apis/Zylance/UseZylance"
import type * as AccountTypes from "$Contract/api/Account"

// Query Key Factory for account queries
export const accountQueryKeys = createQueryKeys("accounts", {
  list: null,
  account: (id: string) => [id],
})

// Fetch all accounts in a vault
export function useAccounts () {
  const zylanceApi = useZylanceApi()

  return useQuery({
    ...accountQueryKeys.list,
    queryFn: async () => {
      const res = await zylanceApi.account.listAccounts({})
      return res.accounts
    },
  })
}

// Fetch a single account by ID
export function useAccount (accountId: string) {
  const zylanceApi = useZylanceApi()

  return useQuery({
    ...accountQueryKeys.account(accountId),
    queryFn: () => zylanceApi.account.getAccount({ accountId }),
    enabled: !!accountId,
  })
}

// Create an account
export function useCreateAccount () {
  const zylanceApi = useZylanceApi()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (req: AccountTypes.CreateAccountReq) =>
      zylanceApi.account.createAccount(req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: accountQueryKeys._def })
    },
  })
}

// Update an account
export function useUpdateAccount () {
  const zylanceApi = useZylanceApi()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (req: AccountTypes.UpdateAccountReq) =>
      zylanceApi.account.updateAccount(req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: accountQueryKeys._def })
    },
  })
}

// Delete an account
export function useDeleteAccount () {
  const zylanceApi = useZylanceApi()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (req: AccountTypes.DeleteAccountReq) =>
      zylanceApi.account.deleteAccount(req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: accountQueryKeys._def })
    },
  })
}
