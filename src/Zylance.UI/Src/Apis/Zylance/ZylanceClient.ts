import {
  type ErrorPayload,
  type EventPayload,
  GatewayEnvelope,
  type RequestPayload,
  type ResponsePayload,
} from "@Contract/lib/Envelope"
import * as RxJs from "rxjs"
import { v7 as uuidv7 } from "uuid"
import type { ITransport } from "@/Apis/Zylance/Transports/ITransport"

type PendingRequest<TResolve, TError> = {
  resolve: (data: TResolve) => void
  reject: (reason?: TError) => void
}

export type EventHandler<TData> = (data: TData) => void | Promise<void>
export type Unsubscribe = () => void

// biome-ignore lint/suspicious/noConfusingVoidType: used to represent absence of data in a request or event
export type ObjectOrVoid = object | void

export type RequestEndpoint<
  _TAction extends string,
  TReqData extends ObjectOrVoid = void,
  TResData extends ObjectOrVoid = void,
  TReturn extends ObjectOrVoid = TResData,
> = (data: TReqData) => Promise<TReturn>

export type EventEmitter<
  _TEvent extends string,
  TEvtData extends ObjectOrVoid = void,
> = (data: TEvtData) => Promise<void>

export type EventListener<
  _TEvent extends string,
  TEvtData extends ObjectOrVoid = void,
> = (handler: (data: TEvtData) => void | Promise<void>) => Unsubscribe

export class MessageError extends Error {
  constructor (public readonly details: string) {
    super(details)
    this.name = "MessageError"
  }

  static throw (details: string): never {
    throw new MessageError(details)
  }
}

export class ZylanceClient {
  private readonly transport: ITransport

  // biome-ignore lint/suspicious/noExplicitAny: Allow any type for event handlers to support various data shapes
  private readonly pendingRequests: Map<string, PendingRequest<any, any>> =
    new Map()

  // biome-ignore lint/suspicious/noExplicitAny: Allow any type for event handlers to support various data shapes
  private readonly eventHandlers: Map<string, Set<EventHandler<any>>> =
    new Map()

  constructor (transport: ITransport) {
    this.transport = transport
    this.transport.receive(this.onMessageReceived.bind(this))
  }

  public observeEvent<TData> (eventName: string) {
    return RxJs.fromEventPattern<TData>(
      (handler) => this.addEventListener<TData>(eventName, handler),
      (handler) => this.removeEventListener<TData>(eventName, handler),
    )
  }

  public createRequestEndpoint<
    TAction extends string,
    TReqData extends ObjectOrVoid = void,
    TResData extends ObjectOrVoid = void,
  > (action: TAction): RequestEndpoint<TAction, TReqData, TResData>
  public createRequestEndpoint<
    TAction extends string,
    TReqData extends ObjectOrVoid = void,
    TResData extends ObjectOrVoid = void,
    TReturn extends ObjectOrVoid = void,
  > (
    action: TAction,
    handler: (res: TResData) => Promise<TReturn>,
  ): RequestEndpoint<TAction, TReqData, TResData, TReturn>
  public createRequestEndpoint<
    TAction extends string,
    TReqData extends ObjectOrVoid = void,
    TResData extends ObjectOrVoid = void,
    TReturn extends ObjectOrVoid = void,
  > (
    action: TAction,
    handler?: (res: TResData) => Promise<TReturn>,
  ): RequestEndpoint<TAction, TReqData, TResData, TReturn> {
    return async (data: TReqData) => {
      return handler
        ? this.makeRequest<TReqData, TResData>(action, data).then((res) =>
          handler(res),
        )
        : this.makeRequest<TReqData, TReturn>(action, data)
    }
  }

  public createEventEmitter<
    TEvent extends string,
    TEvtData extends ObjectOrVoid = void,
  > (event: TEvent): EventEmitter<TEvent, TEvtData> {
    return async (data: TEvtData) => {
      this.sendEvent<TEvtData>(event, data)
    }
  }

  public createEventListener<
    TEvent extends string,
    TData extends ObjectOrVoid = void,
  > (event: TEvent): EventListener<TEvent, TData> {
    return (handler: (data: TData) => void | Promise<void>): Unsubscribe => {
      return this.addEventListener<TData>(event, handler)
    }
  }

  public addEventListener<TData> (
    event: string,
    handler: EventHandler<TData>,
  ): Unsubscribe {
    let handlers = this.eventHandlers.get(event)
    if (!handlers) {
      handlers = new Set()
      this.eventHandlers.set(event, handlers)
    }

    handlers.add(handler)

    return () => this.removeEventListener(event, handler)
  }

  public removeEventListener<TData> (
    event: string,
    handler: EventHandler<TData>,
  ): void {
    this.eventHandlers.get(event)?.delete(handler)
  }

  public sendEvent<TData = void> (eventName: string, data?: TData) {
    const eventPayload: EventPayload = { eventName }
    if (data) {
      eventPayload.dataJson = JSON.stringify(data)
    }
    this.sendMessage({ event: eventPayload })
  }

  private sendMessage (
    payload: { request: RequestPayload } | { event: EventPayload },
  ) {
    const message = GatewayEnvelope.toJSON({ messageId: uuidv7(), ...payload })
    this.transport.send(JSON.stringify(message))
  }

  private onMessageReceived (message: string) {
    console.log(`Received ${message}`)
    const envelope = GatewayEnvelope.fromJSON(JSON.parse(message))

    switch (true) {
      case !!envelope.error:
        return this.onErrorReceived(envelope.error)
      case !!envelope.response:
        return this.onResponseReceived(envelope.response)
      case !!envelope.event:
        return this.onEventReceived(envelope.event)
      default:
        console.warn("Unknown message type received:", envelope)
    }
  }

  private onResponseReceived ({ requestId, dataJson }: ResponsePayload) {
    const pending = this.pendingRequests.get(requestId)
    if (!pending) {
      console.warn(`No pending request found for requestId: ${requestId}`)
      return
    }

    this.pendingRequests.delete(requestId)
    const data = dataJson ? JSON.parse(dataJson) : undefined
    pending.resolve(data)
  }

  private onEventReceived ({ eventName, dataJson }: EventPayload) {
    const data = dataJson ? JSON.parse(dataJson) : undefined

    const handlers = this.eventHandlers.get(eventName)
    if (!handlers || handlers.size === 0) return

    for (const handler of handlers) {
      Promise.resolve()
        .then(() => handler(data))
        .catch((err) =>
          console.error(
            `Error in event handler for event "${eventName}":`,
            err,
          ),
        )
    }
  }

  private onErrorReceived ({ requestId, type, details }: ErrorPayload) {
    console.error("Error received:", { requestId, type, details })

    if (requestId) {
      const pending = this.pendingRequests.get(requestId)
      if (pending) {
        this.pendingRequests.delete(requestId)
        pending.reject(
          new Error(`Error of type ${type} received. Details: ${details}`),
        )
      }
    }
  }

  private makeRequest<TData = void, TResponse = void> (
    action: string,
    data?: TData,
  ): Promise<TResponse> {
    return new Promise((resolve, reject) => {
      const requestId = uuidv7()
      this.pendingRequests.set(requestId, { resolve, reject })

      const request: RequestPayload = { requestId, action }
      if (data) {
        request.dataJson = JSON.stringify(data)
      }
      this.sendMessage({ request })
    })
  }
}
