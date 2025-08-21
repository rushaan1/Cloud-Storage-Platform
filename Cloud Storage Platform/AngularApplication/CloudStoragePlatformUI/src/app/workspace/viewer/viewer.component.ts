import {
  AfterViewInit,
  ChangeDetectorRef,
  Component,
  ElementRef, HostListener,
  NgZone,
  OnDestroy,
  OnInit,
  ViewChild
} from '@angular/core';
import {FilesStateService} from '../../services/StateManagementServices/files-state.service';
import {EventService} from '../../services/event-service.service';
import {ActivatedRoute, Router} from "@angular/router";
import {File} from "../../models/File";
import {FilesAndFoldersService} from "../../services/ApiServices/files-and-folders.service";
import {Utils} from "../../Utils";
import {BreadcrumbsComponent} from "../../items/breadcrumbs/breadcrumbs.component";
import {LoadingService} from "../../services/StateManagementServices/loading.service";
import {BreadcrumbService} from "../../services/StateManagementServices/breadcrumb.service";
import {BehaviorSubject, fromEvent, mapTo, skip, Subscription} from "rxjs";
import {FileType} from "../../models/FileType";
import {map} from "rxjs/operators";
import {NetworkStatusService} from "../../services/network-status-service.service";

@Component({
  selector: 'viewer',
  templateUrl: './viewer.component.html',
  styleUrl: './viewer.component.css'
})
export class ViewerComponent implements OnInit, OnDestroy{
  @ViewChild(BreadcrumbsComponent) breadcrumbsComponent!: BreadcrumbsComponent;
  appUrl: string[] = [];
  visibleFiles: File[] = [];
  subscriptions: Subscription[] = [];
  emptyFolderTxtActive = false;

  searchQuery?:string;
  sort?:string;
  anyItemRenaming = false;
  anyFolderUncreated = false;
  crumbs : string[] = [];
  emptyTxt = "No folders to show";
  renameFocus = false;
  private sse!:EventSource;
  guidsHiddenDueToFileFilter:string[] = [];
  fileBeingPreviewd?:File|null = null;

  constructor(
    private cdRef:ChangeDetectorRef,
    private router: Router,
    private route: ActivatedRoute,
    private foldersService:FilesAndFoldersService,
    private eventService:EventService,
    protected loaderService:LoadingService,
    private breadcrumbService:BreadcrumbService,
    protected filesState:FilesStateService,
    private ngZone: NgZone,
    private networkStatusService:NetworkStatusService) {}

  ngOnInit(): void {
    this.filesState.setUncreatedFolderExists(false);
    // this.foldersService.test();
    this.subscriptions.push(this.networkStatusService.getStatus().pipe(skip(1)).subscribe(status => {
      if (status) {
        this.eventService.emit("reload viewer list");
        console.log("Should reload");
      }
    }));
    this.subscriptions.push(this.filesState.filesInViewer$.pipe(skip(1)).subscribe(files => {

      if (this.crumbs[0]!="Trash"){
        this.visibleFiles = files.filter((f)=>{return !f.isTrash});
      }
      else{
        this.visibleFiles = files;
      }
      this.filterOutFoldersBeingMoved();
    }));
    this.subscriptions.push(this.route.queryParams.subscribe(params => {
      const searchQuery = params['q'];
      const sort = params["sort"];
      const renameFocus = params["renameFocus"];
      if (renameFocus) {
        this.renameFocus = true;
      }

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
      this.loaderService.loadingStart();
      this.handleFolderLoaders();
    }));

    this.subscriptions.push(this.filesState.isRenaming$.subscribe(isRenaming => {
      this.anyItemRenaming = isRenaming;
    }));

    this.subscriptions.push(this.filesState.uncreatedFolderExists$.subscribe(uncreated => {
      this.anyFolderUncreated = uncreated;
    }));

    // Listen for list style changes
    this.subscriptions.push(this.eventService.listen("list-style-changed", (val) => {
      this.handleEmptyTxt();
      if (this.crumbs[0].toLowerCase()=="home" && val!='Y'){
        this.emptyFolderTxtActive = false;
      }
    }));

    // send req to sever, get the token to access sse, establish sse connection & listening

    // TODO Do not establish SSE in case of shared viewing
    this.foldersService.ssetoken().subscribe({
      next: (res) => {
        this.sse = new EventSource('https://localhost:7219/api/Modifications/sse?token=' + res.sseToken);
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
                  // get the file or folder in its unified File type
                  if (this.extractCleanParentPath(processedFile.filePath)==path){
                    this.filesState.setFilesInViewer([...this.filesState.getFilesInViewer(), processedFile]);
                  }
                }
                break;
              case "size_updated":
                // Update file size when it changes
                const fileId = data.content.id;
                const newSize = data.content.size;

                // Update files in the viewer
                let anyFileUpdated = false;
                let filesInState = this.filesState.getFilesInViewer();

                filesInState.forEach((file, index) => {
                  if (file.fileId === fileId) {
                    filesInState[index].size = newSize;
                    anyFileUpdated = true;
                  }
                });
                // if any file was updated, set the new files in viewer
                if (anyFileUpdated) {
                  // Update state with the modified files
                  this.filesState.setFilesInViewer([...filesInState]);

                  // Also update visible files
                  this.visibleFiles.forEach((file, index) => {
                    if (file.fileId === fileId) {
                      this.visibleFiles[index].size = newSize;
                    }
                  });
                }
                break;
              case "favorite_updated" :
                let updated = false;
                let allFiles = this.filesState.getFilesInViewer();
                // if file exists in viewer update its fav status
                allFiles.forEach((f,i)=>{
                  if (f.fileId==data.content.id as string){
                    allFiles[i].isFavorite = data.content.res.isFavorite as boolean;
                    this.filesState.setFilesInViewer(allFiles);
                    const j = this.visibleFiles.findIndex((f2)=>{return f.fileId==f2.fileId});
                    this.visibleFiles[j].isFavorite = this.visibleFiles[i].isFavorite;
                    updated = true
                  }
                });
                // else if user is in favorite page & file not there but favorited then add to viewer
                if (!updated && this.crumbs[0].toLowerCase() == "favorites" && data.content.res.isFavorite as boolean){
                  const processedFile = Utils.processFileModel(data.content.res);
                  this.filesState.setFilesInViewer([...this.filesState.getFilesInViewer(),processedFile]);
                  return;
                }
                break;
              case "trash_updated":
                const items:File[] = [];
                let allFiles2 = this.filesState.getFilesInViewer();

                for (let i = 0; i<data.content.updatedFolders.length; i++){
                  // all thats happening here is that all files in viewer are being searched to find if any matches with processed file if yes update trash status
                  const processedFile = Utils.processFileModel(data.content.updatedFolders[i]);
                  items.push(processedFile);
                  const ind = allFiles2.findIndex((f)=>{return f.fileId==processedFile.fileId});
                  if (ind != -1){
                    allFiles2[ind].isTrash = processedFile.isTrash;
                  }
                }

                for (let i = 0; i<data.content.updatedFiles.length; i++){
                  // same as above
                  const processedFile = Utils.processFileModel(data.content.updatedFiles[i]);
                  items.push(processedFile);
                  const ind = allFiles2.findIndex((f,i)=>{return f.fileId==processedFile.fileId});
                  if (ind != -1){
                    allFiles2[ind].isTrash = processedFile.isTrash;
                  }
                }

                for (let i = 0; i<items.length; i++){
                  if (!this.containsId(items[i].fileId)){
                    if (this.crumbs[0].toLowerCase() == "trash" && items[i].isTrash){
                      allFiles2.push(items[i]);
                      // if user is in trash page & latest status of item is trash true but item is not in visible files then add the item to all files this is because in trash page visible files returns all files
                    }
                  }
                  else{
                    if (this.crumbs[0] == "Trash"){
                      allFiles2 = allFiles2.filter(f=>{
                        return f.fileId != items[i].fileId;
                      });
                    }
                  }
                  this.filesState.setFilesInViewer(allFiles2);
                }
                break;
              case "deleted":
                for (let i = 0; i<data.content.ids.length; i++){
                  this.filesState.setFilesInViewer(this.filesState.getFilesInViewer().filter(file => {
                    return file.fileId != data.content.ids[i];
                  }));
                }
                break;
              case "renamed":
                this.filesState.getFilesInViewer().forEach((f,i)=>{
                  // dont forget its inside a loop
                  if (this.renameFocus){
                    const queryParams = { ...this.route.snapshot.queryParams };
                    delete queryParams["renameFocus"];
                    this.router.navigate([], {
                      relativeTo: this.route,
                      queryParams: queryParams,
                      replaceUrl: true,
                    }).then(()=>{
                      if (f.fileId==data.content.id as string) {
                        this.renameUpdation(f, data, i);
                      }
                    }); // IF rename focus is already focused and rename happens remove the rename focus query param immedtly
                    // update rename in either case cuz this sse confirms a successful rename completion
                  }
                  else{
                    this.renameUpdation(f, data, i);
                  }
                });
                break;
              case "moved":
                for (let i = 0; i<data.content.length; i++){
                  // looping through updated data content
                  if (this.containsId(data.content[i].id)){
                    if (path != Utils.constructFilePathForApi(Utils.cleanPath(decodeURIComponent(data.content[i].movedTo as string)))){
                      // THIS MEANS ITS IN VISIBLE FILES BUT PATH DONT MATCH, MEANS ITS MOVED OUT OF HERE
                      this.filesState.setFilesInViewer(this.filesState.getFilesInViewer().filter(file => { return data.content[i].id != file.fileId }));
                    }
                  }
                  else if (path == Utils.constructFilePathForApi(Utils.cleanPath(decodeURIComponent(data.content[i].movedTo as string)))){
                    // THIS MEANS ITS NOT IN VISIBLE FILES BUT MOVED TO HERE
                    this.filesState.setFilesInViewer([...this.filesState.getFilesInViewer(),Utils.processFileModel(data.content[i].res)]);
                  }
                  console.log("visible ones:");
                  console.log(this.visibleFiles);
                  console.log("in state:");
                  console.log(this.filesState.getFilesInViewer());
                }
            }
            this.cdRef.detectChanges();

            if (this.crumbs[0].toLowerCase()!="home"){
              this.handleEmptyTxt();
            }

            else if (localStorage.getItem("list")=="Y"){
              this.handleEmptyTxt();
            }
          });
        }
      }
    });
  }

  filterOutFoldersBeingMoved(){
    this.visibleFiles = this.visibleFiles.filter(f=>{return !this.filesState.getItemsBeingMoved().map(i => i.fileId).includes(f.fileId)});
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    if (this.sse){
      this.sse.close();
    }
  }

  containsId(id:string){
    return this.visibleFiles.map(f=>f.fileId).includes(id);
  }

  extractCleanParentPath(path:string){
    let splitted = path.split("\\");
    splitted.pop();
    path = splitted.join("\\");
    return Utils.constructFilePathForApi(Utils.cleanPath(path));
  }

  renameUpdation(f:File, data:any, i:number){
    // THIS FN DOES NOT GURANTEE RENAME, ONLY IF f & data ids match

    // f is stale file, data contains updated file, i is the index of file in filesInViewer
    if (f.fileId==data.content.id as string) {
      const file:File = {...f}; // copy stale file in its full form
      file.fileName = data.content.val as string; //update file name property

      const fullPath = file.filePath.split("\\");
      fullPath[fullPath.length-1] = data.content.val as string;
      file.filePath = fullPath.join("\\"); // update file path property w new name

      // apply the changes
      let files = this.filesState.getFilesInViewer();
      files.splice(i, 1, file);
      this.filesState.setFilesInViewer(files);
      this.filesState.setRenaming(false);
    }
  }

  handleFolderLoaders(){
    const appUrl = this.appUrl;
    this.crumbs = Utils.obtainBreadCrumbs(appUrl);
    this.breadcrumbService.setBreadcrumbs(this.crumbs);
    this.loaderService.loadingStart();
    this.filesState.fileOpened = false;
    switch(appUrl[0]){
      case "filter":
        if (appUrl[1]){
          switch (appUrl[1]){
            case "home":
              this.loadHomeFolder();
              break;
            case "gallery":
              this.loadMediaFiles();
              this.crumbs = ["Gallery"];
              break;
            case "recents":
              this.loadRecentsFolders();
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
          this.foldersService.getAllFilesAndSubFoldersByParentFolderPath( constructedPathForApi).subscribe({
            next: response => {
              this.filesState.setFilesInViewer(response);
              this.cdRef.detectChanges();
              this.filterOutFoldersBeingMoved();
              if (appUrl[appUrl.length-1]=='home'){
                this.eventService.emit("home folder set active");
              }
            },
            error: err => {
              this.loaderService.loadingEnd();
            },
            complete: () => {
              this.loaderService.loadingEnd();
              if (localStorage.getItem("list")=="Y"){
                this.handleEmptyTxt();
              }
            }
          });
        }
        break;
      case "preview":
        const constructedPathForApiFile = Utils.constructFilePathForApi(appUrl);
        this.filesState.fileOpened = true;
        if (Utils.validString(constructedPathForApiFile)){
          this.foldersService.getFileOrFolderByPath(constructedPathForApiFile, false).subscribe({
            next: response => {
              this.fileBeingPreviewd = response;
            },
            error: err => {
              this.loaderService.loadingEnd();
            },
            complete: () => {
              this.loaderService.loadingEnd();
            }
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
    if (appUrl[0]!="preview"){
      this.fileBeingPreviewd = null;
    }
  }

  loadHomeFolder() {
    // API
    this.foldersService.getAllInHome().subscribe({
      next: (response) => {
        this.filesState.setFilesInViewer(response);
        this.filterOutFoldersBeingMoved();
      },
      error: (error) => {
        this.loaderService.loadingEnd();
      },
      complete: () => {
        this.loaderService.loadingEnd();
        if (localStorage.getItem("list")=="Y"){
          this.handleEmptyTxt();
        }
      }
    });
  }

  loadFavoriteFolders() {
    // API
    this.foldersService.getAllFavoriteFolders().subscribe({
      next: (response) => {
        this.filesState.setFilesInViewer(response);
        this.filterOutFoldersBeingMoved();
      },
      error: (error) => {
        this.loaderService.loadingEnd();
      },
      complete: () => {
        this.loaderService.loadingEnd();
        this.handleEmptyTxt();
      }
    });
  }

  loadTrashFolders() {
    // API
    this.foldersService.getAllTrashFolders().subscribe({
      next: (response) => {
        this.filesState.setFilesInViewer(response);
        this.filterOutFoldersBeingMoved();
      },
      error: (error) => {
        this.loaderService.loadingEnd();
      },
      complete: () => {
        this.loaderService.loadingEnd();
        this.handleEmptyTxt();
      }
    });
  }

  loadRecentsFolders() {
    // API
    this.foldersService.getAllRecents().subscribe({
      next: (response) => {
        this.filesState.setFilesInViewer(response);
        this.filterOutFoldersBeingMoved();
      },
      error: (error) => {
        this.loaderService.loadingEnd();
      },
      complete: () => {
        this.loaderService.loadingEnd();
        this.handleEmptyTxt("No recent files or folders to show");
      }
    });
  }

  loadMediaFiles() {
    // API
    this.foldersService.getAllMediaFiles().subscribe({
      next: (response) => {
        this.filesState.setFilesInViewer(response);
        this.filterOutFoldersBeingMoved();
      },
      error: (error) => {
        this.loaderService.loadingEnd();
      },
      complete: () => {
        this.loaderService.loadingEnd();
        this.handleEmptyTxt("No media files to show");
      }
    });
  }

  createNewFolder() {
    if (this.anyItemRenaming){
      return;
    }
    if (!this.networkStatusService.statusVal()){
      this.eventService.emit("addNotif", ["Not connected. Please check your internet connection.", 12000]);
      return;
    }
    this.filesState.setUncreatedFolderExists(true);

    let folder:File = {
      fileId: "",
      fileName: Utils.findUniqueName(this.filesState.getFilesInViewer().map(f=>f.fileName), "New Folder"),
      filePath: Utils.constructFilePathForApi(this.crumbs)+"\\",
      isFavorite: false,
      isTrash: false,
      uncreated: true,
      fileType: FileType.Folder,
      size: 0,
      thumbnail: null
    };
    this.filesState.setFilesInViewer([...this.filesState.getFilesInViewer(),folder]);
  }

  handleSearchOperation(){
    if (Utils.validString(this.searchQuery)){
      this.loaderService.loadingStart();
      this.foldersService.getFilteredFolders(this.searchQuery!).subscribe({
        next: res => {
          this.filesState.setFilesInViewer(res);
          this.filterOutFoldersBeingMoved();
        },
        error: err => {
          this.loaderService.loadingEnd();
        },
        complete: () => {
          this.loaderService.loadingEnd();
          this.handleEmptyTxt("No search results match "+this.searchQuery);
        }
      });
    }
  }

  handleEmptyTxt(txt:string=this.emptyTxt){
    if (this.visibleFiles.length == 0) {
      this.emptyFolderTxtActive = true;
      this.emptyTxt = txt;
    }
    else {
      this.emptyFolderTxtActive = false;
    }
  }

  fileFilterUpdates(f:File, event:boolean){
    event ? this.guidsHiddenDueToFileFilter.push(f.fileId) : this.guidsHiddenDueToFileFilter = this.guidsHiddenDueToFileFilter.filter((id) => id != f.fileId)
  }

  removeFile(i:number){
    this.visibleFiles.splice(i,1);
    this.filesState.setFilesInViewer(this.filesState.getFilesInViewer().filter(f=>!f.uncreated));
  }

  protected readonly Utils = Utils;
  protected readonly document = document;
  protected readonly localStorage = localStorage;
}
