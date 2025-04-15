import {AfterViewInit, Component, Input, OnDestroy} from '@angular/core';
import {File} from "../../models/File";
import {FileType} from "../../models/FileType";
import {Router} from "@angular/router";
import {HttpClient, HttpParams} from "@angular/common/http";
import {LoadingService} from "../../services/StateManagementServices/loading.service";
import {finalize, tap} from "rxjs";

@Component({
  selector: 'file-preview',
  templateUrl: './preview.component.html',
  styleUrl: './preview.component.css'
})
export class PreviewComponent implements AfterViewInit, OnDestroy{
  @Input() file!: File;
  fileText: string = '';
  fileUrl: string = '';
  protected readonly FileType = FileType;
  constructor(protected router:Router, private http: HttpClient, private loader:LoadingService) {}

  ngAfterViewInit(): void {
    this.loadFile();
  }

  ngOnDestroy() {
    if (this.fileUrl && this.fileUrl.length > 0) {
      URL.revokeObjectURL(this.fileUrl);
    }
  }

  loadFile() {
    const url = `https://localhost:7219/api/Retrievals/filePreview`;
    const httpParams = new HttpParams()
      .set('filePath', this.file.filePath);
    this.loader.loadingStart();
    this.http.get(url, { responseType: 'blob', observe: 'response', params: httpParams}).pipe(finalize(()=>{this.loader.loadingEnd();})).subscribe({
      next: (res) => {
        const mime = res.headers.get('Content-Type') || '';
        if (mime.startsWith('text')) {
          const reader = new FileReader();
          reader.onload = () => this.fileText = reader.result as string;
          reader.readAsText(res.body!);
        } else {
          this.fileUrl = URL.createObjectURL(res.body!);
        }
      }
    });
  }

}
