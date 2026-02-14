import type * as FileTypes from "@Contract/api/File"
import { ZylanceActions } from "$Generated/ZylanceConstants"
import type { ZylanceClient } from "../ZylanceClient"

export function createFileApi(client: ZylanceClient) {
  return {
    selectFile: client.createRequestEndpoint<
      typeof ZylanceActions.File_SelectFile,
      FileTypes.SelectFileReq,
      FileTypes.SelectFileRes
    >(ZylanceActions.File_SelectFile),

    createFile: client.createRequestEndpoint<
      typeof ZylanceActions.File_CreateFile,
      FileTypes.CreateFileReq,
      FileTypes.CreateFileRes
    >(ZylanceActions.File_CreateFile),

    saveFile: client.createRequestEndpoint<
      typeof ZylanceActions.File_SaveFile,
      FileTypes.SaveFileReq,
      void
    >(ZylanceActions.File_SaveFile),

    getFile: client.createRequestEndpoint<
      typeof ZylanceActions.File_GetFile,
      FileTypes.GetFileReq,
      FileTypes.FileContentRes
    >(ZylanceActions.File_GetFile),
  }
}

export type FileApi = ReturnType<typeof createFileApi>
