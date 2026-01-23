import { v7 as uuidv7 } from 'uuid';
import { getTransport, type ITransport } from '@/Generated/ITransport';
import {
  type ErrorPayload,
  type EventPayload,
  GatewayEnvelope,
  type RequestPayload,
  type ResponsePayload,
} from '@/Generated/Src/Zylance';

type PendingRequest = {
  resolve: (data: any) => void,
  reject: (reason?: any) => void
};

type EventHandler = (data: any) => void;
type Unsubscribe = () => void;

export class ZylanceClient {
  private readonly transport: ITransport;
  private readonly pendingRequests: Map<string, PendingRequest> = new Map();
  private readonly eventHandlers: Map<string, Set<EventHandler>> = new Map();

  constructor () {
    this.transport = getTransport();
    this.transport.receive(this.onMessageReceived.bind(this));
  }

  public createEndpoint<TData = void, TResponse = void> (action: string) {
    return (data: TData) => this.makeRequest<TData, TResponse>(action, data);
  }

  public on (event: 'error', handler: (data: { type: string; details: string }) => void): Unsubscribe;
  public on<TData> (event: string, handler: (data: any) => TData): Unsubscribe {
    let handlers = this.eventHandlers.get(event);
    if (!handlers) {
      handlers = new Set();
      this.eventHandlers.set(event, handlers);
    }

    handlers.add(handler);

    return () => {
      const handlers = this.eventHandlers.get(event);
      handlers?.delete(handler);
    };
  }

  private emit (event: string, data: any) {
    const handlers = this.eventHandlers.get(event);
    if (!handlers || handlers.size === 0) return;

    handlers.forEach(handler => {
      try {
        handler(data);
      } catch (err) {
        console.error(err);
      }
    });
  }

  private sendMessage (payload: { request: RequestPayload } | { event: EventPayload }) {
    const message = GatewayEnvelope.toJSON({ messageId: uuidv7(), ...payload });
    this.transport.send(JSON.stringify(message));
  }

  private onMessageReceived (message: string) {
    console.log(`Received ${message}`);
    const envelope = GatewayEnvelope.fromJSON(JSON.parse(message));

    switch (true) {
      case !!envelope.error:
        return this.onErrorReceived(envelope.error);
      case !!envelope.response:
        return this.onResponseReceived(envelope.response);
      case !!envelope.event:
        return this.onEventReceived(envelope.event);
      default:
        console.warn('Unknown message type received:', envelope);
    }
  }

  private onResponseReceived ({ requestId, dataJson }: ResponsePayload) {
    const pending = this.pendingRequests.get(requestId);
    if (!pending) {
      console.warn(`No pending request found for requestId: ${requestId}`);
      return;
    }

    this.pendingRequests.delete(requestId);
    const data = dataJson ? JSON.parse(dataJson) : undefined;
    pending.resolve(data);
  }

  private onEventReceived ({ event, dataJson }: EventPayload) {
    const data = dataJson ? JSON.parse(dataJson) : undefined;
    this.emit(event, data);
  }

  private onErrorReceived ({ requestId, type, details }: ErrorPayload) {
    if (requestId) {
      const pending = this.pendingRequests.get(requestId);
      if (pending) {
        this.pendingRequests.delete(requestId);
        pending.reject(new Error(`Error of type ${type} received. Details: ${details}`));
      }
    } else {
      this.emit('error', { type, details });
    }
  }

  private makeRequest<TData = void, TResponse = void> (action: string, data?: TData): Promise<TResponse> {
    return new Promise((resolve, reject) => {
      const requestId = uuidv7();
      this.pendingRequests.set(requestId, { resolve, reject });

      const request: RequestPayload = { requestId, action };
      if (data) { request.dataJson = JSON.stringify(data);}
      this.sendMessage({ request });
    });
  }
}