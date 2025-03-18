import { Injectable } from '@angular/core';
import {HTTP_INTERCEPTORS, HttpClient, HttpParams} from "@angular/common/http";
import {File} from "../../models/File";
import {Observable} from "rxjs";
import {Utils} from "../../Utils";
import {ResponseInterceptor} from "./response.interceptor";


const BASE_URL = "https://localhost:7219/api/Folders";

@Injectable({
  providedIn: 'root',
})
export class FoldersService {
  constructor(private httpClient:HttpClient) { }

  public getAllFoldersInHome():Observable<File[]>{
    let params = new HttpParams();
    const sortVal = localStorage.getItem("sort")?.toString();
    if (sortVal){
      params = params.set('sortOrder', sortVal);
    }
    return this.httpClient.get<File[]>(`${BASE_URL}/getAllFoldersInHome`, {params: params});
  }

  public getAllSubFoldersByParentFolderPath(folderPath:string):Observable<File[]>{
    Utils.handleStringInvalidError(folderPath);
    let params = new HttpParams()
      .set('path', folderPath);
    const sortVal = localStorage.getItem("sort")?.toString();
    if (sortVal){
      params = params.set('sortOrder', sortVal);
    }
    return this.httpClient.get<File[]>(`${BASE_URL}/getAllSubFoldersByPath`, {params:params});
  }

  public getFolderByFolderId(folderId:string):Observable<File>{
    Utils.handleStringInvalidError(folderId);
    let params = new HttpParams()
      .set('folderId', folderId);
    return this.httpClient.get<File>(`${BASE_URL}/getFolderById`, {params:params});
  }

  public getFolderByFolderPath(path:string):Observable<File>{
    Utils.handleStringInvalidError(path);
    let params = new HttpParams()
      .set('path', path);
    return this.httpClient.get<File>(`${BASE_URL}/getFolderByPath`, {params:params});
  }

  public getFilteredFolders(searchString:string):Observable<File[]>{
    Utils.handleStringInvalidError(searchString);
    let params = new HttpParams()
      .set('searchString', searchString);
    const sortVal = localStorage.getItem("sort")?.toString();
    if (sortVal){
      params = params.set('sortOrder', sortVal);
    }
    return this.httpClient.get<File[]>(`${BASE_URL}/getFilteredFolders`, {params:params});
  }

  public getAllFavoriteFolders():Observable<File[]>{
    let params = new HttpParams();
    const sortVal = localStorage.getItem("sort")?.toString();
    if (sortVal){
      params = params.set('sortOrder', sortVal);
    }
    return this.httpClient.get<File[]>(`${BASE_URL}/getAllFavoriteFolders`, {params:params});
  }
  public getAllTrashFolders():Observable<File[]>{
    let params = new HttpParams();
    const sortVal = localStorage.getItem("sort")?.toString();
    if (sortVal){
      params = params.set('sortOrder', sortVal);
    }
    return this.httpClient.get<File[]>(`${BASE_URL}/getAllTrashFolders`, {params:params});
  }







  public addFolder(folderName:string, folderPath:string):Observable<File>{
    Utils.handleStringInvalidError(folderName);
    Utils.handleStringInvalidError(folderPath);
    return this.httpClient.post<File>(`${BASE_URL}/add`, {folderName:folderName, folderPath:folderPath});
  }


  public renameFolder(folderId:string, folderNewName:string):Observable<File>{
    Utils.handleStringInvalidError(folderId);
    Utils.handleStringInvalidError(folderNewName);
    return this.httpClient.patch<File>(`${BASE_URL}/rename`, {folderId:folderId, folderNewName:folderNewName});
  }

  public moveFolder(folderId:string, newFolderPath:string):Observable<File>{
    Utils.handleStringInvalidError(folderId);
    Utils.handleStringInvalidError(newFolderPath);
    let params = new HttpParams()
      .set('folderId', folderId)
      .set('newFolderPath', newFolderPath);
    return this.httpClient.patch<File>(`${BASE_URL}/move`,null, {params:params});
  }

  public deleteFolder(folderId:string):Observable<File>{
    Utils.handleStringInvalidError(folderId);
    let params = new HttpParams()
      .set('folderId', folderId)
    return this.httpClient.delete<File>(`${BASE_URL}/delete`, {params:params});
  }

  public addOrRemoveFromFavorite(folderId:string):Observable<File>{
    Utils.handleStringInvalidError(folderId);
    let params = new HttpParams()
      .set('folderId', folderId)
    return this.httpClient.patch<File>(`${BASE_URL}/addOrRemoveFromFavorite`, null, {params:params});
  }

  public addOrRemoveFromTrash(folderId:string):Observable<File>{
    Utils.handleStringInvalidError(folderId);
    let params = new HttpParams()
      .set('folderId', folderId)
    return this.httpClient.patch<File>(`${BASE_URL}/addOrRemoveFromTrash`, null, {params:params});
  }

  public batchAddOrRemoveFromTrash(folderIds:string[]):Observable<object>{
    for (let id of folderIds){
      Utils.handleStringInvalidError(id);
    }
    return this.httpClient.patch(`${BASE_URL}/batchAddOrRemoveFromTrash`, folderIds);
  }

  public batchDelete(folderIds:string[]):Observable<object>{
    for (let id of folderIds){
      Utils.handleStringInvalidError(id);
    }
    let params = new HttpParams();
    folderIds.forEach(id => {params = params.append('folderIds', id)});
    return this.httpClient.delete(`${BASE_URL}/batchDelete`, {params:params});
  }

  public batchMove(folderIds:string[], newPath:string):Observable<object>{
    Utils.handleStringInvalidError(newPath);
    for (let id of folderIds){
      Utils.handleStringInvalidError(id);
    }
    let params = new HttpParams()
      .set('newFolderPath', newPath);
    return this.httpClient.patch(`${BASE_URL}/batchMove`, folderIds, {params:params});
  }
}
