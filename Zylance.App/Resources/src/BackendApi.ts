export class BackendApi {
  public sendMessage (message: string) {
    // @ts-expect-error provided by Photino wrapper
    // noinspection JSDeprecatedSymbols
    window.external.sendMessage(message);
  }

  public receiveMessage (callback: (message: string) => void) {
    // @ts-expect-error provided by Photino wrapper
    // noinspection JSDeprecatedSymbols
    window.external.receiveMessage(callback);
  }
}

export const backend = new BackendApi();