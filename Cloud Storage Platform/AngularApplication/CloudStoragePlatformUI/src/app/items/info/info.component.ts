import {AfterViewInit, Component, ElementRef, Input, NgZone, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {File} from "../../models/File";
import {Metadata} from "../../models/Metadata";
import {FilesAndFoldersService} from "../../services/ApiServices/files-and-folders.service";
import {Utils} from "../../Utils";
import {ActivatedRoute, Router} from "@angular/router";
import {forkJoin} from "rxjs";
import {FilesStateService} from "../../services/StateManagementServices/files-state.service";
import {FileType} from "../../models/FileType";
import {EventService} from "../../services/event-service.service";
import {NetworkStatusService} from "../../services/network-status-service.service";
import {BreadcrumbService} from "../../services/StateManagementServices/breadcrumb.service";

@Component({
  selector: 'info',
  templateUrl: './info.component.html',
  styleUrl: './info.component.css'
})
export class InfoComponent implements OnInit, AfterViewInit, OnDestroy{
  @ViewChild("infoContent") infoContent!: ElementRef<HTMLDivElement>;
  @ViewChild("fav") favBtn!: ElementRef<HTMLButtonElement>;
  @ViewChild("trash") trashBtn!: ElementRef<HTMLButtonElement>;
  f!:File;
  metadata!:Metadata;
  favTxt!:string;
  trashTxt!:string;
  favBtnTxt:string = "Favorite";
  trashBtnTxt:string = "Add to Trash";
  activeTab:number = 0;
  isDeleting = false;
  isFolder = false;
  folderOrFileTxt = "";
  eventSource!:EventSource;

  constructor(protected foldersService:FilesAndFoldersService, protected route:ActivatedRoute, protected router:Router, protected filesState:FilesStateService, protected  eventService:EventService, private ngZone:NgZone, private networkStatus:NetworkStatusService, protected bcService:BreadcrumbService) {}
  ngOnInit(): void {
    this.filesState.outsideFilesAndFoldersMode = true;
    this.route.paramMap.subscribe((params) => {
      const id = params.get("id");
      if (!id) {
        console.error("Error: ID is missing.");
        return;
      }
      const appUrl = this.router.url.split("?")[0].split("/");
      this.isFolder = appUrl[appUrl.length-2]=="foldermetadata";
      this.folderOrFileTxt = this.isFolder ? "Folder" : "File";
      this.bcService.setBreadcrumbs(['']); // if its loaded from viewer, bc was home which flipped the condition for create new folder button, this is fine for now atleast
      forkJoin({
        folder: this.foldersService.getFileOrFolderById(id, this.isFolder),
        metadata: this.foldersService.getMetadata(id, this.isFolder),
      }).subscribe({
        next: ({ folder, metadata }) => {
          this.f = folder;
          this.metadata = metadata;

          this.updateFavAndTrashTxts();
        },
        error: (err) => console.error("Error fetching data", err),
      });
    });

    this.foldersService.ssetoken().subscribe({
      next: (res) => {
        this.eventSource = new EventSource("https://localhost:7219/api/Modifications/sse?token="+res.sseToken);
        this.eventSource.onmessage = (event) => {
          this.ngZone.run(()=>{
            const data = JSON.parse(event.data);
            console.log(data);
            switch (data.eventType) {
              case "favorite_updated" :
                if (this.f.fileId==data.content.id as string){
                  this.f.isFavorite = data.content.res.isFavorite as boolean;
                  this.updateFavAndTrashTxts();
                }
                break;
              case "trash_updated":
                for (let i = 0; i<data.content.updatedFolders.length; i++){
                  if (this.f.fileId==data.content.updatedFolders[i].folderId as string){
                    this.f.isTrash = data.content.updatedFolders[i].isTrash as boolean;
                    this.updateFavAndTrashTxts();
                  }
                }
                for (let i = 0; i<data.content.updatedFiles.length; i++){
                  if (this.f.fileId==data.content.updatedFiles[i].fileId as string){
                    this.f.isTrash = data.content.updatedFiles[i].isTrash as boolean;
                    this.updateFavAndTrashTxts();
                  }
                }
                break;
            }
          });
        };
      }
    });

  }

  ngAfterViewInit() {
    this.route.queryParams.subscribe(params => {
      if (params["tab"]){
        this.activeTab = params["tab"];
        this.setTranslate(this.activeTab * -100, this.activeTab);
      }
    });
  }

  updateFavAndTrashTxts(){
    this.trashTxt = this.f.isTrash ? "Is this "+this.folderOrFileTxt+" in trash?: Yes" : "Is this "+this.folderOrFileTxt+" in trash?: No";
    this.favTxt = this.f.isFavorite ? "Favorite "+this.folderOrFileTxt+": Yes" : "Favorite "+this.folderOrFileTxt+": No";

    this.trashBtnTxt = this.f.isTrash ? "Remove from Trash" : "Add to Trash";
    this.favBtnTxt = this.f.isFavorite ? "UnFavorite" : "Favorite";
  }


  setTranslate(val:number, tabNo:number){
    this.infoContent.nativeElement.style.transform = "translateX(" + val + "%)";
    this.activeTab = tabNo;
  }

  delete(){
    if (!this.networkStatus.statusVal()){
      this.eventService.emit("addNotif", ["Not connected. Please check your internet connection.", 12000]);
      return;
    }
    this.isDeleting = true;
  }
  cancelDelete(){
    this.isDeleting = false;
  }

  confirmDelete(){
    this.foldersService.delete(this.f.fileId, this.isFolder).subscribe({
      next: () => {
        this.router.navigate(["/"]);
      },
      error: (err) => console.error("Error deleting "+this.folderOrFileTxt, err),
    });
  }

  activateMoveState(event:MouseEvent){
    event.stopPropagation();
    this.filesState.setItemsBeingMoved([this.f]);
    if (this.router.url.includes("filter/home")){
      this.eventService.emit("reload viewer list");
      return;
    }
    this.router.navigate(["filter","home"]);
  }

  toggleFavorite(event:MouseEvent){
    event.stopPropagation();
    if (this.favBtn.nativeElement.disabled){
      return;
    }
    this.favBtn.nativeElement.disabled = true;
    this.favBtn.nativeElement.classList.add("disabled");
    this.foldersService.addOrRemoveFromFavorite(this.f.fileId, this.isFolder).subscribe({
      next: () => {
        setTimeout(()=>{
          this.favBtn.nativeElement.disabled = false;
          this.favBtn.nativeElement.classList.remove("disabled");
          },2000);
      },
    });
  }

  toggleTrash(event:MouseEvent){
    event.stopPropagation();
    if (this.trashBtn.nativeElement.disabled){
      return
    }
    this.trashBtn.nativeElement.disabled = true;
    this.trashBtn.nativeElement.classList.add("disabled");
    this.foldersService.addOrRemoveFromTrash(this.f.fileId, this.isFolder).subscribe({
      next: () => {
        setTimeout(()=>{
          this.trashBtn.nativeElement.disabled = false;
          this.trashBtn.nativeElement.classList.remove("disabled");
          },2000);
      },
    });
  }

  renameRedirect(){
    if (!this.networkStatus.statusVal()){
      this.eventService.emit("addNotif", ["Not connected. Please check your internet connection.", 12000]);
      return;
    }
    let path = Utils.cleanPath(this.f.filePath);
    path.pop();
    this.router.navigate(["folder",...path], {queryParams:{'renameFocus':this.f.fileId}});
  }

  protected readonly Utils = Utils;
  protected readonly FileType = FileType;

  ngOnDestroy(): void {
    if (this.eventSource){
      this.eventSource.close();
    }
    this.filesState.outsideFilesAndFoldersMode = false;
  }
}
