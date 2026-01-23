import { ZylanceClient } from '@/Lib/ZylanceClient';

export interface ZylanceApi {
  GetStatus: () => Promise<{ status: string }>;
  EchoMessage: (data: { message: string }) => Promise<{ echoed: string }>;
}

export function createZylanceApi (): ZylanceApi {
  const client = new ZylanceClient();

  client.on('error', ({ type, details }) => {
    console.error('Zylance API Error:', { type, details });
  });

  return {
    GetStatus: client.createEndpoint<void, { status: string }>('Status/GetStatus'),
    EchoMessage: client.createEndpoint<{ message: string }, { echoed: string }>('Echo/Echo'),
  };
}
