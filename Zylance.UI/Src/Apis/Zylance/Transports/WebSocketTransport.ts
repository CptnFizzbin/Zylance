import type { ITransport } from "@/Apis/Zylance/Transports/ITransport"

export class WebSocketTransport implements ITransport {
  private socket: WebSocket
  private receiveHandler?: (message: string) => void

  private constructor(socket: WebSocket) {
    this.socket = socket
    this.socket.onmessage = (event) => {
      if (this.receiveHandler) {
        this.receiveHandler(event.data)
      }
    }
  }

  public static async connect(url: string): Promise<WebSocketTransport> {
    const socket = new WebSocket(url)

    await new Promise<void>((resolve, reject) => {
      socket.onopen = () => resolve()
      socket.onerror = (err) =>
        reject(new Error("WebSocket connection failed", { cause: err }))
    })

    return new WebSocketTransport(socket)
  }

  public receive(handler: (message: string) => void): void {
    this.receiveHandler = handler
  }

  public send(message: string): void {
    if (this.socket.readyState === WebSocket.OPEN) {
      this.socket.send(message)
    } else {
      throw new Error("WebSocket is not open")
    }
  }
}
