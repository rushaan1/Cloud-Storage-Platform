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

  public getFileOrFolderById(id:string, file:boolean):Observable<File>{
    Utils.handleStringInvalidError(id);
    let params = new HttpParams();
    params.append("id", id)
    return this.httpClient.get<File>(`${this.getUrl(file)}/getById`, {params:params});
  }

  public getFileOrFolderByPath(path:string, file:boolean):Observable<File>{
    Utils.handleStringInvalidError(path);
    let params = new HttpParams()
      .set('path', path);
    return this.httpClient.get<File>(`${this.getUrl(file)}/getByPath`, {params:params});
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
    return this.httpClient.get<File[]>(`${BASE_URL}/getAllFavoriteFolders`, {params:params});
  }

  public getAllTrashFolders(fetchFiles:boolean):Observable<File[]>{
    let params = new HttpParams();
    const sortVal = localStorage.getItem("sort")?.toString();
    if (sortVal){
      params = params.set('sortOrder', sortVal);
    }
    params = params.set("fetchFiles", fetchFiles);
    return this.httpClient.get<File[]>(`${BASE_URL}/getAllTrashFolders`, {params:params});
  }

  public getMetadata(id:string, file:boolean){
    Utils.handleStringInvalidError(id);
    let params = new HttpParams()
      .set('id', id);
    return this.httpClient.get<Metadata>(`${this.getUrl(file)}/getMetadata`, {params:params});
  }





  public addFolder(folderName:string, folderPath:string):Observable<File>{
    Utils.handleStringInvalidError(folderName);
    Utils.handleStringInvalidError(folderPath);
    return this.httpClient.post<File>(`${BASE_URL}/add`, {folderName:folderName, folderPath:folderPath});
  }


  public rename(folderId:string, folderNewName:string, file:boolean):Observable<File>{
    Utils.handleStringInvalidError(folderId);
    Utils.handleStringInvalidError(folderNewName);
    return this.httpClient.patch<File>(`${this.getUrl(file)}/rename`, {folderId:folderId, folderNewName:folderNewName});
  }

  public move(folderId:string, newFolderPath:string, file:boolean):Observable<File>{
    Utils.handleStringInvalidError(folderId);
    Utils.handleStringInvalidError(newFolderPath);
    let params = new HttpParams()
      .set('id', folderId)
      .set('newFolderPath', newFolderPath);
    return this.httpClient.patch<File>(`${this.getUrl(file)}/move`,null, {params:params});
  }

  public delete(folderId:string, file:boolean):Observable<File>{
    Utils.handleStringInvalidError(folderId);
    let params = new HttpParams()
      .set('id', folderId)
    return this.httpClient.delete<File>(`${this.getUrl(file)}/delete`, {params:params});
  }

  public addOrRemoveFromFavorite(folderId:string, file:boolean):Observable<File>{
    Utils.handleStringInvalidError(folderId);
    let params = new HttpParams()
      .set('id', folderId)
    return this.httpClient.patch<File>(`${this.getUrl(file)}/addOrRemoveFromFavorite`, null, {params:params});
  }

  public addOrRemoveFromTrash(folderId:string, file:boolean):Observable<File>{
    Utils.handleStringInvalidError(folderId);
    let params = new HttpParams()
      .set('id', folderId)
    return this.httpClient.patch<File>(`${this.getUrl(file)}/addOrRemoveFromTrash`, null, {params:params});
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

  private getUrl(folder:boolean):string{
    if (folder){
      return BASE_URL;
    }
    else{
      return BASE_URL.replace("Folders","File");
    }
  }
}
