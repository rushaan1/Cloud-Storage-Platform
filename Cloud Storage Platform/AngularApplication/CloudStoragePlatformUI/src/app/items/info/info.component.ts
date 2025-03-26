import {AfterViewInit, Component, ElementRef, Input, OnInit, ViewChild} from '@angular/core';
import {File} from "../../models/File";
import {Metadata} from "../../models/Metadata";
import {FilesAndFoldersService} from "../../services/ApiServices/files-and-folders.service";
import {Utils} from "../../Utils";
import {ActivatedRoute, Router} from "@angular/router";
import {forkJoin} from "rxjs";
import {FilesStateService} from "../../services/StateManagementServices/files-state.service";

@Component({
  selector: 'info',
  templateUrl: './info.component.html',
  styleUrl: './info.component.css'
})
export class InfoComponent implements OnInit, AfterViewInit{
  @ViewChild("infoContent") infoContent!: ElementRef<HTMLDivElement>;
  @ViewChild("fav") favBtn!: ElementRef<HTMLButtonElement>;
  @ViewChild("trash") trashBtn!: ElementRef<HTMLButtonElement>;
  f!:File;
  type:string="folder";
  metadata!:Metadata;
  favTxt!:string;
  trashTxt!:string;
  favBtnTxt:string = "Favorite";
  trashBtnTxt:string = "Add to Trash";
  activeTab:number = 0;
  isDeleting = false;
  isFolder = false;

  constructor(protected foldersService:FilesAndFoldersService, protected route:ActivatedRoute, protected router:Router, protected filesState:FilesStateService) {}
  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const id = params.get("id");
      if (!id) {
        console.error("Error: Folder ID is missing.");
        return;
      }
      const appUrl = this.router.url.split("?")[0].split("/");
      this.isFolder = appUrl[appUrl.length-2]=="foldermetadata";

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
    this.trashTxt = this.f.isTrash ? "Is this folder in trash?: Yes" : "Is this folder in trash?: No";
    this.favTxt = this.f.isFavorite ? "Favorite Folder: Yes" : "Favorite Folder: No";

    this.trashBtnTxt = this.f.isTrash ? "Remove from Trash" : "Add to Trash";
    this.favBtnTxt = this.f.isFavorite ? "UnFavorite" : "Favorite";
  }


  setTranslate(val:number, tabNo:number){
    this.infoContent.nativeElement.style.transform = "translateX(" + val + "%)";
    this.activeTab = tabNo;
  }

  delete(){
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
      error: (err) => console.error("Error deleting folder", err),
    });
  }

  activateMoveState(event:MouseEvent){
    event.stopPropagation();
    this.filesState.setItemsBeingMoved([this.f]);
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
      next: (response:File) => {
        this.f.isFavorite = response.isFavorite;
        this.updateFavAndTrashTxts();
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
      next: (response:File) => {
        this.f.isTrash = response.isTrash;
        this.updateFavAndTrashTxts();
        setTimeout(()=>{
          this.trashBtn.nativeElement.disabled = false;
          this.trashBtn.nativeElement.classList.remove("disabled");
          },2000);
      },
    });
  }

  renameRedirect(){
    let path = Utils.cleanPath(this.f.filePath);
    path.pop();
    this.router.navigate(["folder",...path], {queryParams:{'renameFocus':this.f.fileId}});
  }

  protected readonly Utils = Utils;
}
