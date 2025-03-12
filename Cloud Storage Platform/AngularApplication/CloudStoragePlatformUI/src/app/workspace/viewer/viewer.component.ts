import {AfterViewChecked, AfterViewInit, Component, ElementRef, OnInit, ViewChild} from '@angular/core';
import { ItemSelectionService } from '../../services/item-selection.service';
import { EventService } from '../../services/event-service.service';
import {ActivatedRoute, Router, UrlSegment} from "@angular/router";
import {File} from "../../models/File";
import {FoldersService} from "../../services/ApiServices/folders.service";
import {HelperMethods} from "../../HelperMethods";
import {BreadcrumbsComponent} from "../breadcrumbs/breadcrumbs.component";

@Component({
  selector: 'viewer',
  templateUrl: './viewer.component.html',
  styleUrl: './viewer.component.css'
})
export class ViewerComponent implements OnInit{
  @ViewChild(BreadcrumbsComponent) breadcrumbsComponent!: BreadcrumbsComponent;
  folders: File[] = [];
  files: File[] = [];

  searchQuery?:string;
  predefinedTypeFilter?:string;
  sortBy?:string;
  sortingOrder?:string;

  crumbs : string[] = [];

  constructor(private router: Router, private route: ActivatedRoute, private foldersService:FoldersService, private eventService:EventService) {}

  ngOnInit(): void {
    localStorage["uncreatedFolderExists"] = false;
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
      const appUrl = this.router.url.split("/");
      // subscribing to this.route to handle routing and this.router.url is used instead of url here to ensure it's not relative but global url is accessed to ensure usability of program structure
      if (appUrl[0]==""){
        appUrl.shift();
      }
      if (appUrl[appUrl.length-1]==""){
        appUrl.pop();
      }

      if (!appUrl[0]){
        this.router.navigate(["filter", "home"]);
        return;
      }
      this.crumbs = new HelperMethods().obtainBreadCrumbs(appUrl);
      switch(appUrl[0]){
        case "filter":
          if (appUrl[1]){
            switch (appUrl[1]){
              case "home":
                // TODO
                this.loadHomeFolder();
                break;
              case "recents":
                this.crumbs = ["Recents"];
                break;
              case "favorites":
                this.loadFavoriteFolders();
                this.crumbs = ["Favorites"];
                // TODO For files
                break;
              case "trash":
                this.loadTrashFolders();
                this.crumbs = ["Trash"];
                break;
            }
          }
          break;
        case "folder":
          const constructedPathForApi = new HelperMethods().constructFilePathForApi(appUrl);
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

      if (this.breadcrumbsComponent) {
        // Manually initializing breadcrumbs because once its initialized with ngOnInit the ngOnInit function won't run everytime breadcrumbs redirects to route being catched by existing parent component (viewer) already containing breadcrumbs which misses the necessary initialization needed after every navigation by breadcrumbs
        this.breadcrumbsComponent.initializeBreadcrumbs();
        console.log("manually initialized breadcrumbs");
      }
    });

    this.eventService.listen("create new folder", () => {
      this.createNewFolder();
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

  loadFavoriteFolders() {
    // API
    this.foldersService.getAllFavoriteFolders().subscribe({
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

  loadTrashFolders() {
    // API
    this.foldersService.getAllTrashFolders().subscribe({
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

  createNewFolder() {
    if (localStorage["renaming"] == "true"){
      return;
    }
    const folderWithNewFolderNameExists:boolean = this.folders.filter(f=>f.fileName == "New Folder").length > 0;
    let uniqueNewFolderNameFound = false;
    let folderNameToBeUsed = "New Folder";
    let newFolderIndex = 1;

    while (!uniqueNewFolderNameFound) {
      if (folderWithNewFolderNameExists) {
        let nextFolderName = "New Folder (" + newFolderIndex + ")";
        if (this.folders.filter(f => f.fileName == nextFolderName).length == 0) {
          uniqueNewFolderNameFound = true;
          folderNameToBeUsed = nextFolderName;
        } else {
          newFolderIndex++;
        }
      } else {
        uniqueNewFolderNameFound = true;
      }
    }

    let folder:File = {
      fileId: "",
      fileName: folderNameToBeUsed,
      filePath: new HelperMethods().constructFilePathForApi(this.crumbs)+"\\",
      isFavorite: false,
      isTrash: false,
      uncreated: true
    };
    this.folders.push(folder);
  }

  protected readonly localStorage = localStorage;
}
