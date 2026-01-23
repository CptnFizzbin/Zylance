export interface ITransport {
  send: (message: string) => void;
  receive: (handler: (message: string) => void) => void;
}

interface DesktopExternal {
  sendMessage: (message: string) => void;
  receiveMessage: (handler: (message: string) => void) => void;
}

function isDesktopExternal (obj: any): obj is DesktopExternal {
  return typeof obj.sendMessage === 'function' && typeof obj.receiveMessage === 'function';
}

declare global {
  interface Window {
    // @ts-expect-error - external is injected by the desktop host environment
    external: DesktopExternal;
  }
}

export function getTransport (): ITransport {
  const external = window.external;
  if (isDesktopExternal(external)) {
    return {
      send: (message: string) => external.sendMessage(message),
      receive: (handler: (message: string) => void) => external.receiveMessage(handler),
    };
  }

  throw new Error('No suitable transport found.');
}

