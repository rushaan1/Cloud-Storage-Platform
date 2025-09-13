import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from "@angular/common/http";
import { Observable } from "rxjs";
import { map } from "rxjs/operators";
import { Utils } from "../../Utils";
import { File } from "../../models/File";

const PUBLIC_BASE_URL = "https://localhost:7219/api/Shares";

export interface CreateShareRequest {
  fileOrFolderId: string;
  expiryDate?: Date;
  isFile: boolean;
}


export enum SortOrderOptions {
  ALPHABETICAL_ASCENDING = 0,
  ALPHABETICAL_DESCENDING = 1,
  DATEADDED_ASCENDING = 2,
  DATEADDED_DESCENDING = 3,
  LASTOPENED_ASCENDING = 4,
  LASTOPENED_DESCENDING = 5,
  SIZE_ASCENDING = 6,
  SIZE_DESCENDING = 7
}

@Injectable({
  providedIn: 'root',
})
export class PublicService {
  constructor(private httpClient: HttpClient) {}

  /**
   * Creates a new share for a file or folder
   * @param request - The share creation request
   * @returns Observable with sharing ID
   */
  public createShare(request: CreateShareRequest): Observable<{ sharingId: string; message: string }> {
    Utils.handleStringInvalidError(request.fileOrFolderId);
    return this.httpClient.post<{ sharingId: string; message: string }>(`${PUBLIC_BASE_URL}/CreateShare`, request);
  }

  /**
   * Removes a share for a file or folder
   * @param fileOrFolderId - The ID of the file or folder
   * @param isFile - Whether it's a file (true) or folder (false)
   * @returns Observable with success message
   */
  public removeShare(fileOrFolderId: string, isFile: boolean): Observable<{ message: string }> {
    Utils.handleStringInvalidError(fileOrFolderId);
    let params = new HttpParams()
      .set("fileOrFolderId", fileOrFolderId)
      .set("isFile", isFile.toString());
    return this.httpClient.delete<{ message: string }>(`${PUBLIC_BASE_URL}/RemoveShare`, { params });
  }

  /**
   * Fetches shared content (file preview or folder children)
   * @param sharingId - The sharing ID
   * @param fileFolderSubjectId - The ID of the file/folder being accessed
   * @param sort - Sort order for folder contents
   * @returns Observable with folder contents or No Content for files
   */
  public fetchSharedContent(sharingId: string, fileFolderSubjectId: string, sort: SortOrderOptions = SortOrderOptions.DATEADDED_ASCENDING): Observable<{files: File[], relativePath: string, file?:File}> {
    Utils.handleStringInvalidError(sharingId);
    Utils.handleStringInvalidError(fileFolderSubjectId);
    let params = new HttpParams()
      .set("sharingId", sharingId)
      .set("fileFolderSubjectId", fileFolderSubjectId)
      .set("previewSignal", "true") // Always set preview signal to true
      .set("sort", sort.toString());

    return this.httpClient.get(`${PUBLIC_BASE_URL}/FetchSharedContent`, {
      params,
      observe: 'response'
    }).pipe(
      map((response: any) => {
        const relativePath = this.getRelativePathFromHeaders(response);

        // Manually transform the response since the interceptor won't process Shares endpoints
        if (response.body && response.body.folders && response.body.files) {
          // Combine folders and files into a single File[] array
          const folders = response.body.folders.map((folder: any) => Utils.processFileModel(folder));
          const files = response.body.files.map((file: any) => Utils.processFileModel(file));
          return { files: [...folders, ...files], relativePath };
        }
        else {
          return { files: [], relativePath: "" };
        }
      })
    );
  }


  /**
   * Opens a public folder by path and returns its contents
   * @param sharingId - The sharing ID
   * @param relativePath - The relative path within the shared folder
   * @param sort - Sort order for folder contents
   * @returns Observable with folder contents
   */
  public openPublicFolderByPath(sharingId: string, relativePath: string, sort: SortOrderOptions = SortOrderOptions.DATEADDED_ASCENDING): Observable<File[]> {
    Utils.handleStringInvalidError(sharingId);
    let params = new HttpParams()
      .set("sharingId", sharingId)
      .set("relativePath", relativePath)
      .set("sort", sort.toString());

    return this.httpClient.get(`${PUBLIC_BASE_URL}/OpenPublicFolderByPath`, { params }).pipe(
      map((response: any) => {
        // Manually transform the response since the interceptor won't process Shares endpoints
        if (response.folders && response.files) {
          // Combine folders and files into a single File[] array
          const folders = response.folders.map((folder: any) => Utils.processFileModel(folder));
          const files = response.files.map((file: any) => Utils.processFileModel(file));
          return [...folders, ...files];
        }
        return response;
      })
    );
  }

  /**
   * Gets the relative path from response headers
   * @param response - The HTTP response
   * @returns The decoded relative path
   */
  public getRelativePathFromHeaders(response: any): string {
    const relativePathHeader = response.headers.get('RelativePath');
    if (relativePathHeader) {
      return atob(relativePathHeader); // Decode base64
    }
    return '';
  }

  /**
   * Creates a download link for shared content
   * @param sharingId - The sharing ID
   * @param fileFolderSubjectId - The ID of the file/folder being downloaded
   * @returns The download URL
   */
  public createDownloadUrl(sharingId: string, fileFolderSubjectId: string): string {
    Utils.handleStringInvalidError(sharingId);
    Utils.handleStringInvalidError(fileFolderSubjectId);
    let params = new HttpParams()
      .set("sharingId", sharingId)
      .set("fileFolderSubjectId", fileFolderSubjectId);

    return `${PUBLIC_BASE_URL}/DownloadSharedContent?${params.toString()}`;
  }

  /**
   * Creates a preview URL for shared content
   * @param sharingId - The sharing ID
   * @param fileFolderSubjectId - The ID of the file being previewed
   * @returns The preview URL
   */
  public createPreviewUrl(sharingId: string, fileFolderSubjectId: string): string {
    Utils.handleStringInvalidError(sharingId);
    Utils.handleStringInvalidError(fileFolderSubjectId);
    let params = new HttpParams()
      .set("sharingId", sharingId)
      .set("fileFolderSubjectId", fileFolderSubjectId);

    return `${PUBLIC_BASE_URL}/FetchSharedContent?${params.toString()}`;
  }
}
