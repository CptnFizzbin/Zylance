import type { EchoReq, EchoRes } from "@Contract/api/Echo"
import type { CreateFileReq, CreateFileRes, SelectFileReq, SelectFileRes } from "@Contract/api/File"
import type { GetStatusRes } from "@Contract/api/Status"
import { VaultOpenRes } from "@Contract/api/Vault"
import { ZylanceClient } from "@Lib/ZylanceClient"

export function createZylanceApi () {
  const client = new ZylanceClient()

  return {
    desktop: {
      exit: client.createEventEndpoint<"Desktop:Exit">("Desktop:Exit"),
    },

    status: {
      getStatus: client.createRequestEndpoint<"Status:GetStatus", void, GetStatusRes>("Status:GetStatus"),
    },

    echo: {
      echoMessage: client.createRequestEndpoint<"Echo:EchoMessage", EchoReq, EchoRes>("Echo:EchoMessage"),
    },

    files: {
      selectFile: client.createRequestEndpoint<"File:SelectFile", SelectFileReq, SelectFileRes>("File:SelectFile"),
      createFile: client.createRequestEndpoint<"File:CreateFile", CreateFileReq, CreateFileRes>("File:CreateFile"),
    },

    vault: {
      openVault: client.createRequestEndpoint<"Vault:OpenVault", void, VaultOpenRes>("Vault:OpenVault"),
    },
  }
}
