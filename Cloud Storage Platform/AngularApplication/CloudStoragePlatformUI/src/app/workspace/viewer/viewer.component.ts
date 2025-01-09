import {AfterViewChecked, AfterViewInit, Component, OnInit, ViewChild} from '@angular/core';
import { ItemSelectionService } from '../../services/item-selection.service';
import { EventService } from '../../services/event-service.service';
import {ActivatedRoute, Router} from "@angular/router";
import {File} from "../../models/File";
import {FoldersService} from "../../services/ApiServices/folders.service";
import {HelperMethods} from "../../HelperMethods";

@Component({
  selector: 'viewer',
  templateUrl: './viewer.component.html',
  styleUrl: './viewer.component.css'
})
export class ViewerComponent implements OnInit{
  folders: File[] = [];
  files: File[] = [];

  searchQuery?:string;
  predefinedTypeFilter?:string;
  sortBy?:string;
  sortingOrder?:string;

  constructor(private router: Router, private route: ActivatedRoute, private foldersService:FoldersService) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const searchQuery = params['q'];
      const typeFilter = params['predefinedTypeFilter'];
      const sortBy = params['sortBy'];
      const sortingOrder = params['sortingOrder'];

      this.searchQuery = searchQuery;
      this.predefinedTypeFilter = typeFilter;
      this.sortBy = sortBy;
      this.sortingOrder = sortingOrder;
      localStorage["renaming"] = false;
      // use !queryName to see if query param is valid or empty/null/undefined
    });


    this.route.url.subscribe(url => {
      if (!url[0]){
        this.router.navigate(["filter", "home"]);
        return;
      }
      switch(url[0].path){
        case "filter":
          if (url[1]){
            switch (url[1].path){
              case "home":
                // TODO
                this.loadHomeFolder();
                break;
              case "recents":
                break;
              case "favorites":
                this.folders = this.folders.filter(folder => {return folder.isFavorite});
                // TODO For files
                break;
              case "trash":
                this.folders = this.folders.filter(folder => {return folder.isTrash});
                break;
            }
          }
          break;
        case "folder":
          let constructedPathForApi = "";
          for (let i = 1; i< url.length; i++) {
            constructedPathForApi = "\\"+constructedPathForApi + url[i];
          }
          // API
          if (new HelperMethods().validString(constructedPathForApi)){
            this.foldersService.getAllSubFoldersByParentFolderPath(constructedPathForApi).subscribe({
              next: response => {
                this.folders = response;
              },
              error: err => {}, //TODO
              complete: () => {} //TODO
            });
          }
          break;
        case "searchFilter":
          // incase if search query param is not present redirect to home predefined filter because you cannot search empty or only spaces
          break;
        default:
          this.router.navigate(["filter", "home"]);
          break;
      }
    });
  }

  loadHomeFolder() {
    // API
    this.foldersService.getAllFoldersInHome().subscribe({
      next: (response) => {
        this.folders = response;
      },
      error: (error) => {
        // TODO
      },
      complete: () => {

      }
    });
  }
}
