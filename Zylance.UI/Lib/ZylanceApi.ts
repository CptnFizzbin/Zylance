import { MessageError, type RequestEndpoint, ZylanceClient } from "@Lib/ZylanceClient"
import type { GetStatusRes } from "@Contract/api/Status"
import type { EchoReq, EchoRes } from "@Contract/api/Echo"
import type { CreateFileReq, CreateFileRes, FileRef, SelectFileReq, SelectFileRes } from "@Contract/api/File"
import type { VaultOpenRes, VaultRef } from "@Contract/api/Vault"

export interface ZylanceApi {
  desktop: {
    exit: RequestEndpoint<"Desktop:Exit">;
  };

  status: {
    getStatus: RequestEndpoint<"Status:GetStatus", void, GetStatusRes>;
  }

  echo: {
    echoMessage: RequestEndpoint<"Echo:EchoMessage", EchoReq, EchoRes>;
  }

  files: {
    selectFile: RequestEndpoint<"File:SelectFile", SelectFileReq, SelectFileRes, FileRef>;
    createFile: RequestEndpoint<"File:CreateFile", CreateFileReq, CreateFileRes, FileRef>;
  };

  vault: {
    openVault: RequestEndpoint<"Vault:OpenVault", void, VaultOpenRes, VaultRef>;
  };
}

export function createZylanceApi (): ZylanceApi {
  const client = new ZylanceClient()

  return {
    desktop: {
      exit: client.createEventEndpoint("Desktop:Exit"),
    },

    status: {
      getStatus: client.createRequestEndpoint("Status:GetStatus"),
    },

    echo: {
      echoMessage: client.createRequestEndpoint("Echo:EchoMessage"),
    },

    files: {
      selectFile: client.createRequestEndpoint("File:SelectFile", async res => {
        return res.fileRef || MessageError.throw("No fileRef in response")
      }),
      createFile: client.createRequestEndpoint("File:CreateFile", async res => {
        return res.fileRef || MessageError.throw("No fileRef in response")
      }),
    },

    vault: {
      openVault: client.createRequestEndpoint("Vault:OpenVault", async ({ vaultRef }) => {
        if (!vaultRef) throw new MessageError("No vaultRef in response")
        client.sendEvent("Vault:Opened", { vaultRef })
        return vaultRef
      }),
    },
  }
}
