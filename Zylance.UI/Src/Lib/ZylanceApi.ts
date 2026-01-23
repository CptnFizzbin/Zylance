import { ZylanceClient } from '@/Lib/ZylanceClient';
import type { CreateFileReq, FileRef, SelectFileReq } from '@/Generated/Src/Zylance';

export interface ZylanceApi {
  GetStatus: () => Promise<{ status: string }>;
  EchoMessage: (data: { message: string }) => Promise<{ echoed: string }>;

  files: {
    select: (req?: Partial<SelectFileReq>) => Promise<FileRef>;
    create: (req?: Partial<CreateFileReq>) => Promise<FileRef>;
  };
}

export function createZylanceApi (): ZylanceApi {
  const client = new ZylanceClient();

  client.on('error', ({ type, details }) => {
    console.error('Zylance API Error:', { type, details });
  });

  return {
    GetStatus: client.createEndpoint<void, { status: string }>('Status/GetStatus'),
    EchoMessage: client.createEndpoint<{ message: string }, { echoed: string }>('Echo/Echo'),

    files: {
      select: client.createEndpoint<Partial<SelectFileReq> | undefined, FileRef>('file:selectFile'),
      create: client.createEndpoint<Partial<CreateFileReq> | undefined, FileRef>('file:createFile'),
    },
  };
}
