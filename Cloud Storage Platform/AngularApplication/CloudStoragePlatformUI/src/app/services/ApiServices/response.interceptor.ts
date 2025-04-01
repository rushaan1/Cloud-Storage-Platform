import {Injectable} from '@angular/core';
import {HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpResponse} from '@angular/common/http';
import {Observable} from 'rxjs';
import {map} from 'rxjs/operators';
import {File} from '../../models/File';
import {Metadata} from "../../models/Metadata";
import {FileType} from "../../models/FileType";

@Injectable()
export class ResponseInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      map((event: HttpEvent<any>) => {
        if (event instanceof HttpResponse && req.url.toLowerCase().includes('folders') && event.status == 200) {
          return event.clone({
            body: this.transformToFileModel(event.body, req.url)
          });
        }
        return event;
      })
    );
  }

  private transformToFileModel(data: any, url:string): File|File[]|Metadata {
    let array:Array<any> = [];
    if ((data.folders instanceof Array)||(data.folders instanceof Array)){
      for (let i = 0; i < data.folders.length; i++) {
        if (data.folders[i].isTrash.toString() == "true" && !url.includes("/getAllTrashes")){
          continue;
        }
        array.push({
          fileId:data.folders[i].folderId,
          filePath:data.folders[i].folderPath,
          fileName:data.folders[i].folderName,
          isFavorite:data.folders[i].isFavorite,
          isTrash:data.folders[i].isTrash,
          fileType: FileType.Folder,
          uncreated:false
        });
      }
      for (let i = 0; i < data.files.length; i++) {
        if (data.files[i].isTrash.toString() == "true" && !url.includes("/getAllTrashes")){
          continue;
        }
        array.push({
          fileId:data.files[i].fileId,
          filePath:data.files[i].filePath,
          fileName:data.files[i].fileName,
          isFavorite:data.files[i].isFavorite,
          isTrash:data.files[i].isTrash,
          uncreated:false,
          fileType: data.files[i].fileType as FileType
        });
      }
      return array;
    }

    else if (url.includes("/getMetadata")){
      const formatter = new Intl.DateTimeFormat('en-US', { dateStyle: 'medium', timeStyle: 'short' });
      if(!data.creationDate){
        data.creationDate = "N/A";
      }
      else{
        data.creationDate = formatter.format(new Date(data.creationDate));
      }
      if(!data.previousReplacementDate){
        data.previousReplacementDate = "N/A";
      }
      else{
        data.previousReplacementDate = formatter.format(new Date(data.previousReplacementDate));
      }
      if(!data.previousRenameDate){
        data.previousRenameDate = "N/A";
      }
      else{
        data.previousRenameDate = formatter.format(new Date(data.previousRenameDate));
      }
      if(!data.previousMoveDate){
        data.previousMoveDate = "N/A";
      }
      else{
        data.previousMoveDate = formatter.format(new Date(data.previousMoveDate));
      }
      if(!data.lastOpened){
        data.lastOpened = "N/A";
      }
      else{
        data.lastOpened = formatter.format(new Date(data.lastOpened));
      }
      if (!data.previousPath){
        data.previousPath = "N/A";
      }
      return data;
    }
    else{
      if (url.toLowerCase().includes("api/file")){
        return {
          fileId:data.fileId,
          filePath:data.filePath,
          fileName:data.fileName,
          isFavorite:data.isFavorite,
          isTrash:data.isTrash,
          uncreated: false,
          fileType: data.fileType as FileType
        }
      }
      return {
        fileId:data.folderId,
        filePath:data.folderPath,
        fileName:data.folderName,
        isFavorite:data.isFavorite,
        isTrash:data.isTrash,
        uncreated: false,
        fileType: FileType.Folder
      }
    }
  }
}
