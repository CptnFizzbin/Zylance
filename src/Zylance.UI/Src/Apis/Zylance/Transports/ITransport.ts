import { WebSocketTransport } from "@/Apis/Zylance/Transports/WebSocketTransport"

export interface ITransport {
  send: (message: string) => void
  receive: (handler: (message: string) => void) => void
}

declare global {
  interface Window {
    zylance?: {
      websocketUrl: string
    }
  }
}

export async function getTransport(): Promise<ITransport> {
  if (window.zylance) {
    const { websocketUrl } = window.zylance
    console.log(`Initializing Zylance WebSocket Transport: ${websocketUrl}`)
    return await WebSocketTransport.connect(window.zylance.websocketUrl)
  }

  throw new Error("No transport available")
}
