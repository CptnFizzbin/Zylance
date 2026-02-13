import { useContext } from "react"
import type { ZylanceApi } from "@/Apis/Zylance/ZylanceApi"
import { ZylanceContext } from "@/Contexts/ZylanceContext"

export const useZylance = () => {
  const zylance = useContext(ZylanceContext)

  if (!zylance) {
    throw new Error("useZylance must be used within a ZylanceProvider")
  }

  return zylance
}
export const useZylanceApi = (): ZylanceApi => {
  return useZylance().zylanceApi
}
