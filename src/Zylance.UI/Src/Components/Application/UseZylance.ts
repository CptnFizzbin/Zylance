import { useContext } from "react"
import { ZylanceContext } from "./ZylanceContext"

export const useZylance = () => {
  const zylance = useContext(ZylanceContext)

  if (!zylance) {
    throw new Error("useZylance must be used within a ZylanceProvider")
  }

  return zylance
}
