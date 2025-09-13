import {AfterViewInit, ChangeDetectorRef, Component, Input, OnDestroy, OnInit} from '@angular/core';
import {File} from "../../models/File";
import {FileType} from "../../models/FileType";
import {Router} from "@angular/router";
import {HttpClient, HttpParams} from "@angular/common/http";
import {LoadingService} from "../../services/StateManagementServices/loading.service";
import {finalize, tap} from "rxjs";
import {DomSanitizer, SafeResourceUrl} from "@angular/platform-browser";
import {FilesStateService} from "../../services/StateManagementServices/files-state.service";

@Component({
  selector: 'file-preview',
  templateUrl: './preview.component.html',
  styleUrl: './preview.component.css'
})
export class PreviewComponent implements AfterViewInit, OnDestroy, OnInit{
  @Input() file!: File;
  @Input() publicfileurl?: string;
  @Input() publicdownloadurl?: string;
  @Input() publicfextension?: string;
  fileText: string = '';
  trustedUrl: SafeResourceUrl = '';
  PRIVATE_PREVIEW_URL = "https://localhost:7219/api/Retrievals/filePreview"
  PRIVATE_DOWNLOAD_URL = "https://localhost:7219/api/Retrievals/download"
  protected readonly FileType = FileType;
  constructor(private cd:ChangeDetectorRef,protected router:Router, private http: HttpClient, private loader:LoadingService, protected sanitizer: DomSanitizer, private filesState:FilesStateService) {}

  ngOnInit(): void {
    this.filesState.outsideFilesAndFoldersMode = true;

    if (this.publicfileurl) {
      // For public files, use the provided URL
      this.trustedUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.publicfileurl);
    } else if (this.file) {
      // For regular files, construct the preview URL
      this.trustedUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`${this.PRIVATE_PREVIEW_URL}?filePath=` + this.file.filePath);
    }
  }

  ngAfterViewInit(): void {
    this.loadTxtFile();
  }

  ngOnDestroy() {
    this.filesState.outsideFilesAndFoldersMode = false;
  }

  loadTxtFile() {
    if (this.publicfextension != "txt" && !this.file?.filePath.endsWith(".txt")){return;}
    this.loader.loadingStart();
    if (this.file){
      const url = this.PRIVATE_PREVIEW_URL;
      const httpParams = new HttpParams()
        .set('filePath', this.file.filePath);

      this.http.get(url, { responseType: 'blob', observe: 'response', params: httpParams}).pipe(finalize(()=>{this.loader.loadingEnd();})).subscribe({
        next: (res) => {
          const reader = new FileReader();
          reader.onload = () => this.fileText = reader.result as string;
          reader.readAsText(res.body!);
        }
      });
    }
    else if (this.publicfileurl){
      this.http.get(this.publicfileurl, { responseType: 'blob', observe: 'response'}).pipe(finalize(()=>{this.loader.loadingEnd();})).subscribe({
        next: (res) => {
          const reader = new FileReader();
          reader.onload = () => this.fileText = reader.result as string;
          reader.readAsText(res.body!);
        }
      });
    }
  }

  download(){
    if (this.publicfileurl) {
      // For public files, use the provided URL
      window.open(this.publicfileurl, "_blank");
    } else if (this.file) {
      let url = this.PRIVATE_DOWNLOAD_URL;
      url = url+"?fileIds="+this.file.fileId;
      url += "&name="+this.file.fileName;
      window.open(url, "_blank");
    }
  }

  protected readonly window = window;
}
