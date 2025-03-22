import {AfterViewInit, Component, ElementRef, Input, OnInit, ViewChild} from '@angular/core';
import {File} from "../../models/File";
import {Metadata} from "../../models/Metadata";
import {FoldersService} from "../../services/ApiServices/folders.service";
import {Utils} from "../../Utils";
import {ActivatedRoute} from "@angular/router";
import {forkJoin} from "rxjs";

@Component({
  selector: 'info',
  templateUrl: './info.component.html',
  styleUrl: './info.component.css'
})
export class InfoComponent implements OnInit{
  @ViewChild("infoContent") infoContent!: ElementRef<HTMLDivElement>;
  f!:File;
  type:string="folder";
  metadata!:Metadata;
  favTxt!:string;
  trashTxt!:string;
  creationDate!:string;

  constructor(protected foldersService:FoldersService, protected route:ActivatedRoute) {}
  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const id = params.get("id");
      if (!id) {
        console.error("Error: Folder ID is missing.");
        return;
      }

      forkJoin({
        folder: this.foldersService.getFolderByFolderId(id),
        metadata: this.foldersService.getMetadata(id),
      }).subscribe({
        next: ({ folder, metadata }) => {
          this.f = folder;
          this.metadata = metadata;
          this.trashTxt = this.f.isTrash ? "Is this folder in trash?: Yes" : "Is this folder in trash?: No";
          this.favTxt = this.f.isFavorite ? "Favorite Folder: Yes" : "Favorite Folder: No";
        },
        error: (err) => console.error("Error fetching data", err),
      });
    });
  }


  setTranslate(val:number){
    this.infoContent.nativeElement.style.transform = "translateX(" + val + "%)";
  }

  protected readonly Utils = Utils;
}
