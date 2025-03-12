import { Injectable } from '@angular/core';
import {HTTP_INTERCEPTORS, HttpClient, HttpParams} from "@angular/common/http";
import {File} from "../../models/File";
import {Observable} from "rxjs";
import {HelperMethods} from "../../HelperMethods";
import {ResponseInterceptor} from "./response.interceptor";


const BASE_URL = "https://localhost:7219/api/Folders";

@Injectable({
  providedIn: 'root',
})
export class FoldersService {
  constructor(private httpClient:HttpClient) { }

  public getAllFoldersInHome():Observable<File[]>{
    return this.httpClient.get<File[]>(`${BASE_URL}/getAllFoldersInHome`);
  }

  public getAllSubFoldersByParentFolderPath(folderPath:string):Observable<File[]>{
    new HelperMethods().handleStringInvalidError(folderPath);
    const params = new HttpParams()
      .set('path', folderPath);
    return this.httpClient.get<File[]>(`${BASE_URL}/getAllSubFoldersByPath`, {params:params});
  }

  public getFolderByFolderId(folderId:string):Observable<File>{
    new HelperMethods().handleStringInvalidError(folderId);
    const params = new HttpParams()
      .set('folderId', folderId);
    return this.httpClient.get<File>(`${BASE_URL}/getFolderById`, {params:params});
  }

  public getFolderByFolderPath(path:string):Observable<File>{
    new HelperMethods().handleStringInvalidError(path);
    const params = new HttpParams()
      .set('path', path);
    return this.httpClient.get<File>(`${BASE_URL}/getFolderByPath`, {params:params});
  }

  public getFilteredFolders(searchString:string):Observable<File[]>{
    new HelperMethods().handleStringInvalidError(searchString);
    const params = new HttpParams()
      .set('searchString', searchString);
    return this.httpClient.get<File[]>(`${BASE_URL}/getFilteredFolders`, {params:params});
  }

  public getAllFavoriteFolders():Observable<File[]>{
    return this.httpClient.get<File[]>(`${BASE_URL}/getAllFavoriteFolders`);
  }
  public getAllTrashFolders():Observable<File[]>{
    return this.httpClient.get<File[]>(`${BASE_URL}/getAllTrashFolders`);
  }







  public addFolder(folderName:string, folderPath:string):Observable<File>{
    new HelperMethods().handleStringInvalidError(folderName);
    new HelperMethods().handleStringInvalidError(folderPath);
    return this.httpClient.post<File>(`${BASE_URL}/add`, {folderName:folderName, folderPath:folderPath});
  }


  public renameFolder(folderId:string, folderNewName:string):Observable<File>{
    new HelperMethods().handleStringInvalidError(folderId);
    new HelperMethods().handleStringInvalidError(folderNewName);
    return this.httpClient.patch<File>(`${BASE_URL}/rename`, {folderId:folderId, folderNewName:folderNewName});
  }

  public moveFolder(folderId:string, newFolderPath:string):Observable<File>{
    new HelperMethods().handleStringInvalidError(folderId);
    new HelperMethods().handleStringInvalidError(newFolderPath);
    const params = new HttpParams()
      .set('folderId', folderId)
      .set('newFolderPath', newFolderPath);
    return this.httpClient.patch<File>(`${BASE_URL}/move`,null, {params:params});
  }

  public deleteFolder(folderId:string):Observable<File>{
    new HelperMethods().handleStringInvalidError(folderId);
    const params = new HttpParams()
      .set('folderId', folderId)
    return this.httpClient.delete<File>(`${BASE_URL}/delete`, {params:params});
  }

  public addOrRemoveFromFavorite(folderId:string):Observable<File>{
    new HelperMethods().handleStringInvalidError(folderId);
    const params = new HttpParams()
      .set('folderId', folderId)
    return this.httpClient.patch<File>(`${BASE_URL}/addOrRemoveFromFavorite`, null, {params:params});
  }

  public addOrRemoveFromTrash(folderId:string):Observable<File>{
    new HelperMethods().handleStringInvalidError(folderId);
    const params = new HttpParams()
      .set('folderId', folderId)
    return this.httpClient.patch<File>(`${BASE_URL}/addOrRemoveFromTrash`, null, {params:params});
  }
}
