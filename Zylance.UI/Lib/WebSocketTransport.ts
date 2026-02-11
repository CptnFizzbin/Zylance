import type { ITransport } from "@Lib/ITransport"

export class WebSocketTransport implements ITransport {
  private socket: WebSocket
  private receiveHandler?: (message: string) => void

  private constructor (socket: WebSocket) {
    this.socket = socket
    this.socket.onmessage = (event) => {
      if (this.receiveHandler) {
        this.receiveHandler(event.data)
      }
    }
  }

  public static async connect (): Promise<WebSocketTransport> {
    const wsInfo = await fetch("/ws")

    if (!wsInfo.ok) throw new Error("Failed to fetch WebSocket URL")

    const { url } = await wsInfo.json()
    const socket = new WebSocket(url)

    await new Promise<void>((resolve, reject) => {
      socket.onopen = () => resolve()
      socket.onerror = (err) =>
        reject(new Error("WebSocket connection failed", { cause: err }))
    })

    return new WebSocketTransport(socket)
  }

  receive (handler: (message: string) => void): void {
    this.receiveHandler = handler
  }

  send (message: string): void {
    if (this.socket.readyState === WebSocket.OPEN) {
      this.socket.send(message)
    } else {
      throw new Error("WebSocket is not open")
    }
  }
}
