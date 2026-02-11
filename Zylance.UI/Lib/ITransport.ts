import { WebSocketTransport } from "@Lib/WebSocketTransport"

export interface ITransport {
  send: (message: string) => void
  receive: (handler: (message: string) => void) => void
}

export function getTransport(): ITransport {
  return new WebSocketTransport()
}
