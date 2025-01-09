import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { File } from '../../models/File'; // Adjust the path as needed

@Injectable()
export class ResponseInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      map((event: HttpEvent<any>) => {
        if (event instanceof HttpResponse && req.url.toLowerCase().includes('folders')) {
          return event.clone({
            body: this.transformToFileModel(event.body)
          });
        }
        return event;
      })
    );
  }

  private transformToFileModel(data: any): File|File[] {
    if (data instanceof Array) {
      let array:Array<any> = [];
      for (let i = 0; i < data.length; i++) {
        array.push({
          fileId:data[i].folderId,
          filePath:data[i].folderPath,
          fileName:data[i].folderName,
          isFavorite:data[i].isFavorite,
          isTrash:data[i].isTrash
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
        isTrash:data.isTrash
      }
    }
  }
}
