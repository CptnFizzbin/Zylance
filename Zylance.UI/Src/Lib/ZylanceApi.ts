import { ZylanceClient } from '@/Lib/ZylanceClient';
import type { CreateFileReq, FileRef, SelectFileReq } from '@/Generated/Messages/Files';
import type { VaultOpenRes } from '@/Generated/Messages/Vault';

export interface ZylanceApi {
  GetStatus: () => Promise<{ status: string }>;
  EchoMessage: (data: { message: string }) => Promise<{ echoed: string }>;

  files: {
    select: (req?: Partial<SelectFileReq>) => Promise<FileRef>;
    create: (req?: Partial<CreateFileReq>) => Promise<FileRef>;
  };

  vault: {
    open: () => Promise<VaultOpenRes>;
  };
}

export function createZylanceApi (): ZylanceApi {
  const client = new ZylanceClient();

  return {
    GetStatus: client.createEndpoint<void, { status: string }>('Status/GetStatus'),
    EchoMessage: client.createEndpoint<{ message: string }, { echoed: string }>('Echo/Echo'),

    files: {
      select: client.createEndpoint<Partial<SelectFileReq> | undefined, FileRef>('file:selectFile'),
      create: client.createEndpoint<Partial<CreateFileReq> | undefined, FileRef>('file:createFile'),
    },

    vault: {
      open: () => {
        const endpoint = client.createEndpoint<void, VaultOpenRes>('vault:openVault');
        const vaultRef = endpoint();
        client.sendEvent('Vault/Opened', { vaultRef });
        return vaultRef;
      },
    },
  };
}
