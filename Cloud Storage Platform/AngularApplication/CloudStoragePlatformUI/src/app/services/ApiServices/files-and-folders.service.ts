import { Injectable } from '@angular/core';
import {HTTP_INTERCEPTORS, HttpClient, HttpParams} from "@angular/common/http";
import {File} from "../../models/File";
import {Observable} from "rxjs";
import {Utils} from "../../Utils";
import {ResponseInterceptor} from "./response.interceptor";
import {Metadata} from "../../models/Metadata";


const BASE_URL = "https://localhost:7219/api/Folders";

@Injectable({
  providedIn: 'root',
})
export class FilesAndFoldersService {
  constructor(private httpClient:HttpClient) { }

  public getAllInHome(fetchFiles:boolean):Observable<File[]>{
    let params = new HttpParams();
    const sortVal = localStorage.getItem("sort")?.toString();
    if (sortVal){
      params = params.set('sortOrder', sortVal);
    }
    params = params.set("fetchFiles", fetchFiles);
    return this.httpClient.get<File[]>(`${BASE_URL}/getAllInHome`, {params: params});
  }

  public getAllFilesAndSubFoldersByParentFolderPath(fetchFiles:boolean,folderPath:string):Observable<File[]>{
    Utils.handleStringInvalidError(folderPath);
    let params = new HttpParams()
      .set('path', folderPath);
    const sortVal = localStorage.getItem("sort")?.toString();
    if (sortVal){
      params = params.set('sortOrder', sortVal);
    }
    params = params.set("fetchFiles", fetchFiles);
    return this.httpClient.get<File[]>(`${BASE_URL}/getAllChildrenByPath`, {params:params});
  }

  public getFileOrFolderById(id:string, isFolder:boolean):Observable<File>{
    Utils.handleStringInvalidError(id);
    let params = new HttpParams();
    params = params.set("id", id)
    return this.httpClient.get<File>(`${this.getUrl(isFolder)}/getById`, {params:params});
  }

  public getFileOrFolderByPath(path:string, isFolder:boolean):Observable<File>{
    Utils.handleStringInvalidError(path);
    let params = new HttpParams()
      .set('path', path);
    return this.httpClient.get<File>(`${this.getUrl(isFolder)}/getByPath`, {params:params});
  }

  public getFilteredFolders(fetchFiles:boolean,searchString:string):Observable<File[]>{
    Utils.handleStringInvalidError(searchString);
    let params = new HttpParams()
      .set('searchString', searchString);
    const sortVal = localStorage.getItem("sort")?.toString();
    if (sortVal){
      params = params.set('sortOrder', sortVal);
    }
    params = params.set("fetchFiles", fetchFiles);
    return this.httpClient.get<File[]>(`${BASE_URL}/getAllFiltered`, {params:params});
  }

  public getAllFavoriteFolders(fetchFiles:boolean):Observable<File[]>{
    let params = new HttpParams();
    const sortVal = localStorage.getItem("sort")?.toString();
    if (sortVal){
      params = params.set('sortOrder', sortVal);
    }
    params = params.set("fetchFiles", fetchFiles);
    return this.httpClient.get<File[]>(`${BASE_URL}/getAllFavorites`, {params:params});
  }

  public getAllTrashFolders(fetchFiles:boolean):Observable<File[]>{
    let params = new HttpParams();
    const sortVal = localStorage.getItem("sort")?.toString();
    if (sortVal){
      params = params.set('sortOrder', sortVal);
    }
    params = params.set("fetchFiles", fetchFiles);
    return this.httpClient.get<File[]>(`${BASE_URL}/getAllTrashes`, {params:params});
  }

  public getMetadata(id:string, isFolder:boolean){
    Utils.handleStringInvalidError(id);
    let params = new HttpParams()
      .set('id', id);
    return this.httpClient.get<Metadata>(`${this.getUrl(isFolder)}/getMetadata`, {params:params});
  }





  public addFolder(folderName:string, folderPath:string):Observable<File>{
    Utils.handleStringInvalidError(folderName);
    Utils.handleStringInvalidError(folderPath);
    return this.httpClient.post<File>(`${BASE_URL}/add`, {folderName:folderName, folderPath:folderPath});
  }

  public batchAddFolders(paths:string[]){
    //batchFoldersAdd
    return this.httpClient.post(`${BASE_URL}/batchFoldersAdd`, paths);
  }

  public uploadFile(formData:FormData){
    return this.httpClient.post<File[]>(`${this.getUrl(false)}/upload`,formData, {reportProgress:true, observe:'events'});
  }


  public rename(folderId:string, folderNewName:string, isFolder:boolean):Observable<File>{
    Utils.handleStringInvalidError(folderId);
    Utils.handleStringInvalidError(folderNewName);
    let reqBody:any;
    if (isFolder){
      reqBody = {folderId:folderId, folderNewName:folderNewName}
    }
    else{
      reqBody = {fileId:folderId, fileNewName:folderNewName}
    }
    return this.httpClient.patch<File>(`${this.getUrl(isFolder)}/rename`, reqBody);
  }

  public move(folderId:string, newFolderPath:string, isFolder:boolean):Observable<File>{
    Utils.handleStringInvalidError(folderId);
    Utils.handleStringInvalidError(newFolderPath);
    let params = new HttpParams()
      .set('id', folderId)
      .set('newPath', newFolderPath);
    return this.httpClient.patch<File>(`${this.getUrl(isFolder)}/move`,null, {params:params});
  }

  public delete(folderId:string, isFolder:boolean):Observable<File>{
    Utils.handleStringInvalidError(folderId);
    let params = new HttpParams()
      .set('id', folderId)
    return this.httpClient.delete<File>(`${this.getUrl(isFolder)}/delete`, {params:params});
  }

  public addOrRemoveFromFavorite(folderId:string, isFolder:boolean):Observable<File>{
    Utils.handleStringInvalidError(folderId);
    let params = new HttpParams()
      .set('id', folderId)
    return this.httpClient.patch<File>(`${this.getUrl(isFolder)}/addOrRemoveFromFavorite`, null, {params:params});
  }

  public addOrRemoveFromTrash(folderId:string, isFolder:boolean):Observable<File>{
    Utils.handleStringInvalidError(folderId);
    let params = new HttpParams()
      .set('id', folderId)
    return this.httpClient.patch<File>(`${this.getUrl(isFolder)}/addOrRemoveFromTrash`, null, {params:params});
  }

  public batchAddOrRemoveFoldersFromTrash(folderIds:string[]):Observable<object>{
    for (let id of folderIds){
      Utils.handleStringInvalidError(id);
    }
    return this.httpClient.patch(`${BASE_URL}/batchAddOrRemoveFromTrash`, folderIds);
  }

  public batchDeleteFolders(folderIds:string[]):Observable<object>{
    for (let id of folderIds){
      Utils.handleStringInvalidError(id);
    }
    let params = new HttpParams();
    folderIds.forEach(id => {params = params.append('ids', id)});
    return this.httpClient.delete(`${BASE_URL}/batchDelete`, {params:params});
  }

  public batchMoveFolders(ids:string[], newPath:string):Observable<object>{
    Utils.handleStringInvalidError(newPath);
    for (let id of ids){
      Utils.handleStringInvalidError(id);
    }
    let params = new HttpParams()
      .set('newFolderPath', newPath);
    return this.httpClient.patch(`${BASE_URL}/batchMove`, ids, {params:params});
  }

  public batchAddOrRemoveFilesFromTrash(ids:string[]):Observable<object>{
    for (let id of ids){
      Utils.handleStringInvalidError(id);
    }
    return this.httpClient.patch(`${this.getUrl(false)}/batchAddOrRemoveFromTrash`, ids);
  }

  public batchDeleteFiles(ids:string[]):Observable<object>{
    for (let id of ids){
      Utils.handleStringInvalidError(id);
    }
    let params = new HttpParams();
    ids.forEach(id => {params = params.append('ids', id)});
    return this.httpClient.delete(`${this.getUrl(false)}/batchDelete`, {params:params});
  }

  public batchMoveFiles(ids:string[], newPath:string):Observable<object>{
    Utils.handleStringInvalidError(newPath);
    for (let id of ids){
      Utils.handleStringInvalidError(id);
    }
    let params = new HttpParams()
      .set('newFolderPath', newPath);
    return this.httpClient.patch(`${this.getUrl(false)}/batchMove`, ids, {params:params});
  }

  private getUrl(folder:boolean):string{
    if (folder){
      return BASE_URL;
    }
    else{
      return BASE_URL.replace("Folders","File");
    }
  }
}
