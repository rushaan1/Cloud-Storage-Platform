import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { File } from '../../models/File';

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

  private transformToFileModel(data: any, url:string): File|File[] {
    if (data instanceof Array) {
      let array:Array<any> = [];
      for (let i = 0; i < data.length; i++) {
        if (data[i].isTrash.toString() == "true" && !url.includes("getAllTrashFolders")){
          continue;
        }
        array.push({
          fileId:data[i].folderId,
          filePath:data[i].folderPath,
          fileName:data[i].folderName,
          isFavorite:data[i].isFavorite,
          isTrash:data[i].isTrash,
          uncreated:false
        });
      }
      return array;
    }
    else{
      return {
        fileId:data.folderId,
        filePath:data.folderPath,
        fileName:data.folderName,
        isFavorite:data.isFavorite,
        isTrash:data.isTrash,
        uncreated: false
      }
    }
  }
}
