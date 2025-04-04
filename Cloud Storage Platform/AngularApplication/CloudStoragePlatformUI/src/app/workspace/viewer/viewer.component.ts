import {ChangeDetectorRef, Component, NgZone, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {FilesStateService} from '../../services/StateManagementServices/files-state.service';
import {EventService} from '../../services/event-service.service';
import {ActivatedRoute, Router} from "@angular/router";
import {File} from "../../models/File";
import {FilesAndFoldersService} from "../../services/ApiServices/files-and-folders.service";
import {Utils} from "../../Utils";
import {BreadcrumbsComponent} from "../../items/breadcrumbs/breadcrumbs.component";
import {LoadingService} from "../../services/StateManagementServices/loading.service";
import {BreadcrumbService} from "../../services/StateManagementServices/breadcrumb.service";
import {Subscription} from "rxjs";
import {FileType} from "../../models/FileType";

@Component({
  selector: 'viewer',
  templateUrl: './viewer.component.html',
  styleUrl: './viewer.component.css'
})
export class ViewerComponent implements OnInit, OnDestroy{
  @ViewChild(BreadcrumbsComponent) breadcrumbsComponent!: BreadcrumbsComponent;
  appUrl: string[] = [];
  files: File[] = [];
  subscriptions: Subscription[] = [];
  emptyFolderTxtActive = false;

  searchQuery?:string;
  sort?:string;
  anyItemRenaming = false;
  anyFolderUncreated = false;
  crumbs : string[] = [];
  emptyTxt = "No folders to show";
  private sse!:EventSource;

  constructor(
    private cdRef:ChangeDetectorRef,
    private router: Router,
    private route: ActivatedRoute,
    private foldersService:FilesAndFoldersService,
    private eventService:EventService,
    private loaderService:LoadingService,
    private breadcrumbService:BreadcrumbService,
    protected filesState:FilesStateService,
    private ngZone: NgZone) {}

  ngOnInit(): void {
    this.filesState.setUncreatedFolderExists(false);
    this.subscriptions.push(this.route.queryParams.subscribe(params => {
      const searchQuery = params['q'];
      const sort = params["sort"];

      this.searchQuery = searchQuery;
      this.sort = sort;
      this.filesState.setRenaming(false);

      if(this.crumbs[0]=="Search Results"){
        this.handleSearchOperation();
      }
      // use !queryName to see if query param is valid or empty/null/undefined
    }));


    this.subscriptions.push(this.route.url.subscribe(url => {
      let appUrl = this.router.url.split("?")[0].split("/");
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
      this.appUrl = appUrl;
      this.handleFolderLoaders();

      if (this.breadcrumbsComponent) {
        // Manually initializing breadcrumbs because once its initialized with ngOnInit the ngOnInit function won't run everytime breadcrumbs redirects to route being catched by existing parent component (viewer) already containing breadcrumbs which misses the necessary initialization needed after every navigation by breadcrumbs
        this.breadcrumbsComponent.initializeBreadcrumbs();
        console.log("manually initialized breadcrumbs");
      }
    }));

    this.subscriptions.push(this.eventService.listen("create new folder", () => {
      this.createNewFolder();
    }));

    this.subscriptions.push(this.eventService.listen("reload viewer list", () => {
      this.handleFolderLoaders();
    }));

    this.subscriptions.push(this.filesState.isRenaming$.subscribe(isRenaming => {
      this.anyItemRenaming = isRenaming;
    }));

    this.subscriptions.push(this.filesState.uncreatedFolderExists$.subscribe(uncreated => {
      this.anyFolderUncreated = uncreated;
    }));
    this.sse = new EventSource('https://localhost:7219/api/Modifications/sse');
    this.sse.onmessage = (event) =>{
      this.ngZone.run(()=>{
        const data = JSON.parse(event.data);
        console.log(data);

        const path:string = decodeURIComponent(Utils.constructFilePathForApi(this.crumbs));
        switch (data.eventType){
          case "added":
            if (this.crumbs[0]!="home"){
              return;
            }
            for (let i = 0; i<data.content.res.length; i++){
              const processedFile = Utils.processFileModel(data.content.res[i]);
              if (this.extractCleanParentPath(processedFile.filePath)==path){
                this.files.push(processedFile);
              }
            }
            break;
          case "favorite_updated" :
            let updated = false;
            this.files.forEach((f,i)=>{
              if (f.fileId==data.content.id as string){
                this.files[i].isFavorite = data.content.res.isFavorite as boolean;
                updated = true
              }
            });
            if (!updated && this.crumbs[0].toLowerCase() == "favorites" && data.content.res.isFavorite as boolean){
              const processedFile = Utils.processFileModel(data.content.res);
              this.files.push(processedFile);
              return;
            }
            break;
          case "trash_updated":
            const items:File[] = [];
            for (let i = 0; i<data.content.updatedFolders.length; i++){
              const processedFile = Utils.processFileModel(data.content.updatedFolders[i]);
              items.push(processedFile);
            }
            for (let i = 0; i<data.content.updatedFiles.length; i++){
              const processedFile = Utils.processFileModel(data.content.updatedFiles[i]);
              items.push(processedFile);
            }
            for (let i = 0; i<items.length; i++){
              if (!this.containsId(items[i].fileId)){
                if (!items[i].isTrash && this.extractCleanParentPath(items[i].filePath) == path){
                  this.files.push(items[i]);
                }
                else if (this.crumbs[0].toLowerCase() == "trash" && items[i].isTrash){
                  this.files.push(items[i]);
                }
              }
              else{
                this.files = this.files.filter((f)=>{
                  return f.fileId != items[i].fileId;
                });
              }
            }
            break;
          case "deleted":
            for (let i = 0; i<data.content.ids.length; i++){
              this.files = this.files.filter(file => {
                return file.fileId != data.content.ids[i];
              });
            }
            break;
          case "renamed":
            this.files.forEach((f,i)=>{
              if (f.fileId==data.content.id as string) {
                const file:File = {...f};
                file.fileName = data.content.val as string;
                const fullPath = file.filePath.split("\\");
                fullPath[fullPath.length-1] = data.content.val as string;
                file.filePath = fullPath.join("\\");
                this.files.splice(i, 1, file);
                this.filesState.setRenaming(false);
              }
            });
            break;
          case "moved":
            for (let i = 0; i<data.content.length; i++){
              if (this.containsId(data.content[i].id)){
                if (path != Utils.constructFilePathForApi(Utils.cleanPath(decodeURIComponent(data.content[i].movedTo as string)))){
                  this.files = this.files.filter(file => { return data.content[i].id != file.fileId });
                }
              }
              else if (path == Utils.constructFilePathForApi(Utils.cleanPath(decodeURIComponent(data.content[i].movedTo as string)))){
                this.files.push(Utils.processFileModel(data.content[i].res));
              }
            }
            this.cdRef.detectChanges();
        }
      });
    }
  }

  filterOutFoldersBeingMoved(){
    this.files = this.files.filter(f=>{return !this.filesState.getItemsBeingMoved().map(i => i.fileId).includes(f.fileId)});
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    this.sse.close();
  }

  containsId(id:string){
    return this.files.map(f=>f.fileId).includes(id);
  }

  extractCleanParentPath(path:string){
    let splitted = path.split("\\");
    splitted.pop();
    path = splitted.join("\\");
    return Utils.constructFilePathForApi(Utils.cleanPath(path));
  }

  handleFolderLoaders(){
    const appUrl = this.appUrl;
    this.crumbs = Utils.obtainBreadCrumbs(appUrl);
    this.breadcrumbService.setBreadcrumbs(this.crumbs);
    this.loaderService.loadingStart();
    switch(appUrl[0]){
      case "filter":
        if (appUrl[1]){
          switch (appUrl[1]){
            case "home":
              this.loadHomeFolder();
              break;
            case "recents":
              this.crumbs = ["Recents"];
              break;
            case "favorites":
              this.loadFavoriteFolders();
              this.crumbs = ["Favorites"];
              break;
            case "trash":
              this.loadTrashFolders();
              this.crumbs = ["Trash"];
              break;
          }
        }
        break;
      case "folder":
        const constructedPathForApi = Utils.constructFilePathForApi(appUrl);

        if (Utils.validString(constructedPathForApi)){
          this.foldersService.getAllFilesAndSubFoldersByParentFolderPath(this.filesState.getItemsBeingMoved().length==0, constructedPathForApi).subscribe({
            next: response => {
              this.files = response;
              this.cdRef.detectChanges();
              this.filterOutFoldersBeingMoved();
              if (appUrl[appUrl.length-1]=='home'){
                this.eventService.emit("home folder set active");
              }
            },
            error: err => {}, //TODO
            complete: () => {
              this.loaderService.loadingEnd();
            } //TODO
          });
        }
        break;
      case "searchFilter":
        this.breadcrumbService.setBreadcrumbs(["Search Results"]);
        this.crumbs = ["Search Results"];
        this.handleSearchOperation();
        break;
      default:
        this.router.navigate(["filter", "home"]);
        break;
    }
  }

  loadHomeFolder() {
    // API
    this.foldersService.getAllInHome(this.filesState.getItemsBeingMoved().length==0).subscribe({
      next: (response) => {
        this.files = response;
        this.filterOutFoldersBeingMoved();
      },
      error: (error) => {
        // TODO
      },
      complete: () => {
        this.loaderService.loadingEnd();
      }
    });
  }

  loadFavoriteFolders() {
    // API
    this.foldersService.getAllFavoriteFolders(this.filesState.getItemsBeingMoved().length==0).subscribe({
      next: (response) => {
        this.files = response;
        this.filterOutFoldersBeingMoved();
      },
      error: (error) => {
        // TODO
      },
      complete: () => {
        this.loaderService.loadingEnd();
        this.handleEmptyTxt();
      }
    });
  }

  loadTrashFolders() {
    // API
    this.foldersService.getAllTrashFolders(this.filesState.getItemsBeingMoved().length==0).subscribe({
      next: (response) => {
        this.files = response;
        this.filterOutFoldersBeingMoved();
      },
      error: (error) => {
        // TODO
      },
      complete: () => {
        this.loaderService.loadingEnd();
        this.handleEmptyTxt();
      }
    });
  }

  createNewFolder() {
    if (this.anyItemRenaming){
      return;
    }
    this.filesState.setUncreatedFolderExists(true);
    const folderWithNewFolderNameExists:boolean = this.files.filter(f=>f.fileName == "New Folder").length > 0;
    let uniqueNewFolderNameFound = false;
    let folderNameToBeUsed = "New Folder";
    let newFolderIndex = 1;

    while (!uniqueNewFolderNameFound) {
      if (folderWithNewFolderNameExists) {
        let nextFolderName = "New Folder (" + newFolderIndex + ")";
        if (this.files.filter(f => f.fileName == nextFolderName).length == 0) {
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
      filePath: Utils.constructFilePathForApi(this.crumbs)+"\\",
      isFavorite: false,
      isTrash: false,
      uncreated: true,
      fileType: FileType.Folder
    };
    this.files.push(folder);
  }

  handleSearchOperation(){
    if (Utils.validString(this.searchQuery)){
      this.loaderService.loadingStart();
      this.foldersService.getFilteredFolders(this.filesState.getItemsBeingMoved().length==0,this.searchQuery!).subscribe({
        next: res => {
          this.files = res;
          this.filterOutFoldersBeingMoved();
        },
        error: err => {},
        complete: () => {
          this.loaderService.loadingEnd();
          this.handleEmptyTxt("No search results match "+this.searchQuery);
        }
      });
    }
  }

  handleEmptyTxt(txt:string=this.emptyTxt){
    if (this.files.length == 0) {
      this.emptyFolderTxtActive = true;
      this.emptyTxt = txt;
    }
    else {
      this.emptyFolderTxtActive = false;
    }
  }

  protected readonly Utils = Utils;
}
