import { type Endpoint, MessageError, ZylanceClient } from "@/Lib/ZylanceClient"
import type { CreateFileReq, FileRef, SelectFileReq } from "@/Generated/Messages/Files"
import type { VaultOpenRes, VaultRef } from "@/Generated/Messages/Vault"
import type { CreateFileRes, SelectFileRes } from "@/Generated/Messages/File"
import type { GetStatusRes } from "@/Generated/Messages/Status"
import type { EchoReq, EchoRes } from "@/Generated/Messages/Echo"

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
