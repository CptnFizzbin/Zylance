import { type Endpoint, MessageError, ZylanceClient } from "@/Lib/ZylanceClient"
import type { GetStatusRes } from "@/Generated/zylance/api/Status.ts"
import type { EchoReq, EchoRes } from "@/Generated/zylance/api/Echo.ts"
import type {
  CreateFileReq,
  CreateFileRes,
  FileRef,
  SelectFileReq,
  SelectFileRes,
} from "@/Generated/zylance/api/File.ts"
import type { VaultOpenRes, VaultRef } from "@/Generated/zylance/api/Vault.ts"

export interface ZylanceApi {
  GetStatus: Endpoint<"Status:GetStatus", void, GetStatusRes>;
  EchoMessage: Endpoint<"Echo:EchoMessage", EchoReq, EchoRes>;

  files: {
    select: Endpoint<"File:SelectFile", SelectFileReq, SelectFileRes, FileRef>;
    create: Endpoint<"File:CreateFile", CreateFileReq, CreateFileRes, FileRef>;
  };

  vault: {
    open: Endpoint<"Vault:OpenVault", void, VaultOpenRes, VaultRef>;
  };
}

export function createZylanceApi (): ZylanceApi {
  const client = new ZylanceClient()

  return {
    GetStatus: client.createEndpoint("Status:GetStatus"),
    EchoMessage: client.createEndpoint("Echo:EchoMessage"),

    files: {
      select: client.createEndpoint("File:SelectFile", async res => {
        return res.fileRef || MessageError.throw("No fileRef in response")
      }),
      create: client.createEndpoint("File:CreateFile", async res => {
        return res.fileRef || MessageError.throw("No fileRef in response")
      }),
    },

    vault: {
      open: client.createEndpoint("Vault:OpenVault", async ({ vaultRef }) => {
        if (!vaultRef) throw new MessageError("No vaultRef in response")
        client.sendEvent("Vault/Opened", { vaultRef })
        return vaultRef
      }),
    },
  }
}
