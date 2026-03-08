import { use } from "react"
import { createZylanceApi, type ZylanceApi } from "@/Apis/Zylance/ZylanceApi"

const apiPromise = createZylanceApi()

export const useZylanceApi = (): ZylanceApi => {
  return use(apiPromise)
}
