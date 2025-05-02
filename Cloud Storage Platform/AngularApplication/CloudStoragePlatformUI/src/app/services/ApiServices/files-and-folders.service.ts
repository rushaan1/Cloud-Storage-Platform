import { Injectable } from '@angular/core';
import { HTTP_INTERCEPTORS, HttpClient, HttpParams } from "@angular/common/http";
import { File } from "../../models/File";
import { Observable } from "rxjs";
import { Utils } from "../../Utils";
import { Metadata } from "../../models/Metadata";
import { Meta } from "@angular/platform-browser";

const RETRIEVAL_BASE_URL = "https://localhost:7219/api/Retrievals";
const MODIFICATION_BASE_URL = "https://localhost:7219/api/Modifications";

@Injectable({
  providedIn: 'root',
})
export class FilesAndFoldersService {
  constructor(private httpClient: HttpClient) {}

  // RETRIEVAL endpoints

  public getAllInHome(): Observable<File[]> {
    let params = new HttpParams();
    const sortVal = localStorage.getItem("sort")?.toString();
    if (sortVal) {
      params = params.set("sortOrder", sortVal);
    }
    return this.httpClient.get<File[]>(`${RETRIEVAL_BASE_URL}/getAllInHome`, { params });
}

public getAllFilesAndSubFoldersByParentFolderPath(folderPath: string): Observable<File[]> {
  Utils.handleStringInvalidError(folderPath);
  let params = new HttpParams().set("path", folderPath);
  const sortVal = localStorage.getItem("sort")?.toString();
  if (sortVal) {
    params = params.set("sortOrder", sortVal);
  }
  return this.httpClient.get<File[]>(`${RETRIEVAL_BASE_URL}/getAllChildrenByPath`, { params });
}

public getFileOrFolderById(id: string, isFolder: boolean): Observable<File> {
  Utils.handleStringInvalidError(id);
  let params = new HttpParams().set("id", id);
  const url = isFolder
    ? `${RETRIEVAL_BASE_URL}/getFolderById`
    : `${RETRIEVAL_BASE_URL}/getFileById`;
  return this.httpClient.get<File>(url, { params });
}

public getFileOrFolderByPath(path: string, isFolder: boolean): Observable<any> {
  Utils.handleStringInvalidError(path);
  let params = new HttpParams().set("path", path);
  const url = isFolder
    ? `${RETRIEVAL_BASE_URL}/getFolderByPath`
    : `${RETRIEVAL_BASE_URL}/getFileByPath`;
  return this.httpClient.get<any>(url, { params });
}

public getFilteredFolders(searchString: string): Observable<File[]> {
  Utils.handleStringInvalidError(searchString);
  let params = new HttpParams().set("searchString", searchString);
  const sortVal = localStorage.getItem("sort")?.toString();
  if (sortVal) {
    params = params.set("sortOrder", sortVal);
  }
  return this.httpClient.get<File[]>(`${RETRIEVAL_BASE_URL}/getAllFiltered`, { params });
}

public getAllFavoriteFolders(): Observable<File[]> {
  let params = new HttpParams();
  const sortVal = localStorage.getItem("sort")?.toString();
  if (sortVal) {
    params = params.set("sortOrder", sortVal);
  }
  return this.httpClient.get<File[]>(`${RETRIEVAL_BASE_URL}/getAllFavorites`, { params });
}

public getAllTrashFolders(): Observable<File[]> {
  let params = new HttpParams();
  const sortVal = localStorage.getItem("sort")?.toString();
  if (sortVal) {
    params = params.set("sortOrder", sortVal);
  }
  return this.httpClient.get<File[]>(`${RETRIEVAL_BASE_URL}/getAllTrashes`, { params });
}

public getAllRecents(): Observable<File[]> {
  return this.httpClient.get<File[]>(`${RETRIEVAL_BASE_URL}/getAllRecents`);
}

public getMetadata(id: string, isFolder: boolean): Observable<Metadata> {
  Utils.handleStringInvalidError(id);
  let params = new HttpParams().set("id", id).set("isFolder", isFolder);
  return this.httpClient.get<Metadata>(`${RETRIEVAL_BASE_URL}/getMetadata`, { params });
}

// MODIFICATION endpoints

public addFolder(folderName: string, folderPath: string): Observable<any> {
  Utils.handleStringInvalidError(folderName);
  Utils.handleStringInvalidError(folderPath);
  const payload = { folderName, folderPath };
  return this.httpClient.post(`${MODIFICATION_BASE_URL}/add`, payload);
}

public batchAddFolders(paths: string[]): Observable<any> {
  return this.httpClient.post(`${MODIFICATION_BASE_URL}/batchFoldersAdd`, paths);
}

public uploadFile(formData: FormData): Observable<any> {
  return this.httpClient.post(`${MODIFICATION_BASE_URL}/upload`, formData, {
    reportProgress: true,
    observe: 'events'
  });
}

public rename(folderId: string, folderNewName: string, isFolder: boolean): Observable<any> {
  Utils.handleStringInvalidError(folderId);
  Utils.handleStringInvalidError(folderNewName);
  const renameReq = { id: folderId, newName: folderNewName };
  return this.httpClient.patch(`${MODIFICATION_BASE_URL}/rename?isFolder=${isFolder}`, renameReq);
}

public delete(folderId: string, isFolder: boolean): Observable<any> {
  Utils.handleStringInvalidError(folderId);
  let params = new HttpParams().append("ids", folderId).set("isFolder", isFolder);
  return this.httpClient.delete(`${MODIFICATION_BASE_URL}/batchDelete`, { params });
}

public addOrRemoveFromFavorite(folderId: string, isFolder: boolean): Observable<any> {
  Utils.handleStringInvalidError(folderId);
  let params = new HttpParams().set("id", folderId).set("isFolder", isFolder);
  return this.httpClient.patch(`${MODIFICATION_BASE_URL}/addOrRemoveFromFavorite`, null, { params });
}

public addOrRemoveFromTrash(folderId: string, isFolder: boolean): Observable<any> {
  Utils.handleStringInvalidError(folderId);
  let params = new HttpParams().set("isFolder", isFolder);
  return this.httpClient.patch(`${MODIFICATION_BASE_URL}/batchAddOrRemoveFromTrash`, [folderId], { params });
}

public batchAddOrRemoveFoldersFromTrash(folderIds: string[]): Observable<any> {
  folderIds.forEach(id => Utils.handleStringInvalidError(id));
  let params = new HttpParams().set("isFolder", true);
  return this.httpClient.patch(`${MODIFICATION_BASE_URL}/batchAddOrRemoveFromTrash`, folderIds, { params });
}

public batchDeleteFolders(folderIds: string[]): Observable<any> {
  folderIds.forEach(id => Utils.handleStringInvalidError(id));
  let params = new HttpParams();
  folderIds.forEach(id => params = params.append("ids", id));
  params = params.set("isFolder", true);
  return this.httpClient.delete(`${MODIFICATION_BASE_URL}/batchDelete`, { params });
}

public batchMoveFolders(ids: string[], newPath: string): Observable<any> {
  Utils.handleStringInvalidError(newPath);
  ids.forEach(id => Utils.handleStringInvalidError(id));
  let params = new HttpParams().set("newFolderPath", newPath).set("isFolder", true);
  return this.httpClient.patch(`${MODIFICATION_BASE_URL}/batchMove`, ids, { params });
}

public batchAddOrRemoveFilesFromTrash(ids: string[]): Observable<any> {
  ids.forEach(id => Utils.handleStringInvalidError(id));
  let params = new HttpParams().set("isFolder", false);
  return this.httpClient.patch(`${MODIFICATION_BASE_URL}/batchAddOrRemoveFromTrash`, ids, { params });
}

public batchDeleteFiles(ids: string[]): Observable<any> {
  ids.forEach(id => Utils.handleStringInvalidError(id));
  let params = new HttpParams();
  ids.forEach(id => params = params.append("ids", id));
  params = params.set("isFolder", false);
  return this.httpClient.delete(`${MODIFICATION_BASE_URL}/batchDelete`, { params });
}

public batchMoveFiles(ids: string[], newPath: string): Observable<any> {
  Utils.handleStringInvalidError(newPath);
  ids.forEach(id => Utils.handleStringInvalidError(id));
  let params = new HttpParams().set("newFolderPath", newPath).set("isFolder", false);
  return this.httpClient.patch(`${MODIFICATION_BASE_URL}/batchMove`, ids, { params });
}
}
