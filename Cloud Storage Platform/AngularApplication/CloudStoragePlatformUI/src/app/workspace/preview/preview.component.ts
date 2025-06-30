import {AfterViewInit, ChangeDetectorRef, Component, Input, OnDestroy, OnInit} from '@angular/core';
import {File} from "../../models/File";
import {FileType} from "../../models/FileType";
import {Router} from "@angular/router";
import {HttpClient, HttpParams} from "@angular/common/http";
import {LoadingService} from "../../services/StateManagementServices/loading.service";
import {finalize, tap} from "rxjs";
import {DomSanitizer, SafeResourceUrl} from "@angular/platform-browser";

@Component({
  selector: 'file-preview',
  templateUrl: './preview.component.html',
  styleUrl: './preview.component.css'
})
export class PreviewComponent implements AfterViewInit, OnDestroy, OnInit{
  @Input() file!: File;
  fileText: string = '';
  trustedUrl: SafeResourceUrl = '';
  protected readonly FileType = FileType;
  constructor(private cd:ChangeDetectorRef,protected router:Router, private http: HttpClient, private loader:LoadingService, protected sanitizer: DomSanitizer) {}

  ngAfterViewInit(): void {
    this.loadTxtFile();
  }

  ngOnDestroy() {
    // if (this.fileUrl && this.fileUrl.length > 0) {
    //   URL.revokeObjectURL(this.fileUrl);
    // }
  }

  loadTxtFile() {
    if (!this.file.filePath.endsWith(".txt")){return;}
    const url = `https://localhost:7219/api/Retrievals/filePreview`;
    const httpParams = new HttpParams()
      .set('filePath', this.file.filePath);
    this.loader.loadingStart();
    this.http.get(url, { responseType: 'blob', observe: 'response', params: httpParams}).pipe(finalize(()=>{this.loader.loadingEnd();})).subscribe({
      next: (res) => {
        const reader = new FileReader();
        reader.onload = () => this.fileText = reader.result as string;
        reader.readAsText(res.body!);
      }
    });
  }

  download(){
    let url = "https://localhost:7219/api/Retrievals/download";
    url = url+"?fileIds="+this.file.fileId;
    url += "&name="+this.file.fileName;
    window.open(url, "_blank");
  }

  ngOnInit(): void {
    this.trustedUrl = this.sanitizer.bypassSecurityTrustResourceUrl('https://localhost:7219/api/Retrievals/filePreview?filePath=' + this.file.filePath);
  }

  protected readonly window = window;
}
