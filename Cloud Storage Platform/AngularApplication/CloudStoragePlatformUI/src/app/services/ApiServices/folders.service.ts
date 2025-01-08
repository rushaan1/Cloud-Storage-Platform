import { Injectable } from '@angular/core';
import {HttpClient, HttpParams} from "@angular/common/http";
import {Folder} from "../../models/Folder";
import {Observable} from "rxjs";
import {HelperMethods} from "../../HelperMethods";


const BASE_URL = "https://localhost:7219/api/Folders";

@Injectable({
  providedIn: 'root'
})
export class FoldersService {
  constructor(private httpClient:HttpClient) { }

  public getAllFoldersInHome():Observable<Folder[]>{
    return this.httpClient.get<Folder[]>(`${BASE_URL}/getAllFoldersInHome`);
  }

  public getAllSubFolders(folderId:string):Observable<Folder[]>{
    new HelperMethods().handleStringInvalidError(folderId);
    const params = new HttpParams()
      .set('parentFolderId', folderId);
    return this.httpClient.get<Folder[]>(`${BASE_URL}/getAllSubFolders`, {params:params});
  }

  public getFolderByFolderId(folderId:string):Observable<Folder>{
    new HelperMethods().handleStringInvalidError(folderId);
    const params = new HttpParams()
      .set('folderId', folderId);
    return this.httpClient.get<Folder>(`${BASE_URL}/getFolderById`, {params:params});
  }

  public getFolderByFolderPath(path:string):Observable<Folder>{
    new HelperMethods().handleStringInvalidError(path);
    const params = new HttpParams()
      .set('path', path);
    return this.httpClient.get<Folder>(`${BASE_URL}/getFolderByPath`, {params:params});
  }

  public getFilteredFolders(searchString:string):Observable<Folder[]>{
    new HelperMethods().handleStringInvalidError(searchString);
    const params = new HttpParams()
      .set('searchString', searchString);
    return this.httpClient.get<Folder[]>(`${BASE_URL}/getFilteredFolders`, {params:params});
  }







  public addFolder(folderName:string, folderPath:string):Observable<Folder>{
    new HelperMethods().handleStringInvalidError(folderName);
    new HelperMethods().handleStringInvalidError(folderPath);
    return this.httpClient.post<Folder>(`${BASE_URL}/add`, {folderName:folderName, folderPath:folderPath});
  }


  public renameFolder(folderId:string, folderNewName:string):Observable<Folder>{
    new HelperMethods().handleStringInvalidError(folderId);
    new HelperMethods().handleStringInvalidError(folderNewName);
    return this.httpClient.patch<Folder>(`${BASE_URL}/rename`, {folderId:folderId, folderNewName:folderNewName});
  }

  public moveFolder(folderId:string, newFolderPath:string):Observable<Folder>{
    new HelperMethods().handleStringInvalidError(folderId);
    new HelperMethods().handleStringInvalidError(newFolderPath);
    const params = new HttpParams()
      .set('folderId', folderId)
      .set('newFolderPath', newFolderPath);
    return this.httpClient.patch<Folder>(`${BASE_URL}/move`, {params:params});
  }

  public deleteFolder(folderId:string):Observable<Folder>{
    new HelperMethods().handleStringInvalidError(folderId);
    const params = new HttpParams()
      .set('folderId', folderId)
    return this.httpClient.delete<Folder>(`${BASE_URL}/delete`, {params:params});
  }

  public addOrRemoveFromFavorite(folderId:string):Observable<Folder>{
    new HelperMethods().handleStringInvalidError(folderId);
    const params = new HttpParams()
      .set('folderId', folderId)
    return this.httpClient.patch<Folder>(`${BASE_URL}/addOrRemoveFromFavorite`, {params:params});
  }

  public addOrRemoveFromTrash(folderId:string):Observable<Folder>{
    new HelperMethods().handleStringInvalidError(folderId);
    const params = new HttpParams()
      .set('folderId', folderId)
    return this.httpClient.patch<Folder>(`${BASE_URL}/addOrRemoveFromTrash`, {params:params});
  }

}
