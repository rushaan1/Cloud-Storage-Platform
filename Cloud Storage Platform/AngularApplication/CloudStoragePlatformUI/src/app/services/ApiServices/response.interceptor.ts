import {Injectable} from '@angular/core';
import {HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpResponse} from '@angular/common/http';
import {Observable} from 'rxjs';
import {map} from 'rxjs/operators';
import {File} from '../../models/File';
import {Metadata} from "../../models/Metadata";
import {FileType} from "../../models/FileType";
import {Utils} from "../../Utils";

@Injectable()
export class ResponseInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      map((event: HttpEvent<any>) => {
        if (event instanceof HttpResponse && (req.url.toLowerCase().includes('retrievals')||req.url.toLowerCase().includes('modifications')) && event.status == 200) {
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
    if ((data.folders instanceof Array)||(data.files instanceof Array)){
      for (let i = 0; i < data.folders.length; i++) {
        if (data.folders[i].isTrash.toString() == "true" && !url.includes("/getAllTrashes")){
          continue;
        }
        array.push(Utils.processFileModel(data.folders[i]));
      }
      for (let i = 0; i < data.files.length; i++) {
        if (data.files[i].isTrash.toString() == "true" && !url.includes("/getAllTrashes")){
          continue;
        }
        array.push(Utils.processFileModel(data.files[i]));
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
      return Utils.processFileModel(data);
    }
  }
}
