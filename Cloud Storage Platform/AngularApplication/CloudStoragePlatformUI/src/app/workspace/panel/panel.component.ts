import {AfterViewChecked, Component, ElementRef, OnInit, ViewChild} from '@angular/core';
import {FormControl, Validators} from "@angular/forms";
import {EventService} from "../../services/event-service.service";
import {invalidCharacter, invalidFileNameChars} from "../../CustomValidators";
import {ActivatedRoute, Router} from "@angular/router";
import {BreadcrumbService} from "../../services/StateManagementServices/breadcrumb.service";
import {FilesAndFoldersService} from "../../services/ApiServices/files-and-folders.service";
import {Utils} from "../../Utils";
import {HttpEvent, HttpEventType} from "@angular/common/http";
import {NetworkStatusService} from "../../services/network-status-service.service";
import {finalize} from "rxjs";
import {FilesStateService} from "../../services/StateManagementServices/files-state.service";

@Component({
  selector: 'panel',
  templateUrl: './panel.component.html',
  styleUrl: './panel.component.css'
})
export class PanelComponent implements OnInit, AfterViewChecked {
  @ViewChild('searchDiv') searchDiv!: ElementRef<HTMLInputElement>;
  @ViewChild('pfpDropdown') pfpDropdownDiv!: ElementRef<HTMLDivElement>;
  @ViewChild('panelMainFlex') panelMainFlex!: ElementRef<HTMLDivElement>;
  @ViewChild('pfp') pfp!: ElementRef<HTMLDivElement>;
  @ViewChild('pfpDropDownSpan') pfpDropDownSpan!: ElementRef<HTMLSpanElement>;
  @ViewChild('sortSelection') sortSelection!: ElementRef<HTMLSelectElement>;
  @ViewChild("sorting") sortingTxt!: ElementRef<HTMLSpanElement>;
  @ViewChild('sortParent') sortParent!: ElementRef<HTMLDivElement>;
  @ViewChild('uploadInputHidden') uploadInputHidden!: ElementRef<HTMLInputElement>;

  mouseHoveringOverPfp = false;
  onlyHiText = false;
  searchCrossVisibleDueToFocus = false;
  searchCrossVisibleDueToHover = false;
  uploadOptionsVisible = false;
  uploadProgress = 0;

  searchFormControl = new FormControl("", [Validators.required, Validators.pattern(/\S/), invalidCharacter]);
  pfpDropdownShowing = false;
  crumbs:string[] = [];
  filesInMutualPath:string[] = [];
  constructor(protected eventService:EventService, protected router: Router, private breadCrumbService:BreadcrumbService, protected route:ActivatedRoute, protected filesService:FilesAndFoldersService, private networkStatus:NetworkStatusService, private filesState:FilesStateService){}

  ngOnInit(){
    this.showStartupWelcomeMsgWithPfpDropDown();
    this.breadCrumbService.breadcrumbs$.subscribe((crumbs)=>{
      this.crumbs = crumbs;
    });
  }

  ngAfterViewChecked(){
    if (this.pfpDropdownShowing && this.pfpDropdownDiv.nativeElement){
      const rect = this.pfp.nativeElement.getBoundingClientRect();
      this.pfpDropdownDiv.nativeElement.style.right = `${rect.top}px`;
      if (this.onlyHiText){
        this.pfpDropDownSpan.nativeElement.innerText = "Hi FirstName";
      }
      else{
        this.pfpDropDownSpan.nativeElement.innerHTML = "Hi FirstName, <br> <b>Click to open account settings</b>";
      }
    }
  }

  searchDarkenBorder(){
    this.searchDiv.nativeElement.style.borderColor = "black";
    this.searchDiv.nativeElement.classList.remove("red-search-border");
  }

  searchLightenBorder(){
    this.searchDiv.nativeElement.style.borderColor = "gray";
  }

  styleChange(){
    if (localStorage["style"]!=undefined && localStorage["style"]!=null && localStorage["style"]!=""){
      if (localStorage["style"]=="large"){
        localStorage["style"] = "list";
      }
      else{
        localStorage["style"] = "large";
      }
    }
    else{
      localStorage["style"] = "list";
    }
    console.log(localStorage["style"]);
  }

  searchClick(){
    if (this.searchFormControl.invalid){
      if (this.searchFormControl.hasError("invalidCharacter")){
        this.eventService.emit("addNotif",["Invalid character in input: "+this.searchFormControl.errors?.['invalidCharactersString'], 8000]);
      }
      else{
        this.eventService.emit("addNotif",["Input cannot be empty", 8000]);
      }
      this.searchDiv.nativeElement.classList.add("red-search-border");
      return;
    }
    this.router.navigate(["searchFilter"], {queryParams:{q:this.searchFormControl.value}});
  }

  showPfpDropdown(onlyHiText:boolean){
    this.pfpDropdownShowing = true;
    this.onlyHiText = onlyHiText;
  }

  hidePfpDropdown(){
    this.pfpDropdownShowing = false;
  }

  showStartupWelcomeMsgWithPfpDropDown(){
    this.showPfpDropdown(true);

    setTimeout(() => {
      if (!this.mouseHoveringOverPfp){
        this.hidePfpDropdown();
      }
    }, 8000);
  }

  searchClear(){
    this.searchFormControl.setValue("");
    this.searchDiv.nativeElement.classList.remove("red-search-border");
    if (this.crumbs[0]=="Search Results"){
      this.router.navigate(["filter", "home"]);
    }
  }

  sortingChanged(event:Event){
    const selectElem:HTMLSelectElement = event.target as HTMLSelectElement;
    const val = selectElem.value;
    const selected = selectElem.options[selectElem.selectedIndex];
    if (selected.className=="asc"){
      this.sortingTxt.nativeElement.innerHTML = selected.text.split(" (")[0] + "  &uarr;";
    }
    else{
      this.sortingTxt.nativeElement.innerHTML = selected.text.split(" (")[0] + "  &darr;";
    }
    this.sortParent.nativeElement.style.marginTop = "12px";
    localStorage.setItem("sort",val);
    this.sortSelection.nativeElement.selectedIndex = 0;
    this.router.navigate([], {
      relativeTo:this.route,
      queryParams: {
        sort: val
      },
      queryParamsHandling:'merge'
    });
    this.eventService.emit("reload viewer list");
  }

  uploadFolder(){
    if (!this.networkStatus.statusVal()){
      this.eventService.emit("addNotif", ["Not connected. Please check your internet connection.", 12000]);
      return;
    }
    this.uploadInputHidden.nativeElement.webkitdirectory = true;
    this.uploadInputHidden.nativeElement.click();
  }

  uploadFile(){
    if (!this.networkStatus.statusVal()){
      this.eventService.emit("addNotif", ["Not connected. Please check your internet connection.", 12000]);
      return;
    }
    this.uploadInputHidden.nativeElement.webkitdirectory = false;
    this.uploadInputHidden.nativeElement.click();
  }

  // appendFoldersToBeCreated(folders:string[], currentDirCheck:string[]){
  //   let pathToCheck = Utils.constructFilePathForApi([...this.crumbs, ...currentDirCheck];
  //   if (!folders.includes(pathToCheck)){
  //     folders.push(pathToCheck);
  //   }
  //   this.appendFoldersToBeCreated(folders, [...currentDirCheck,currentDirCheck[currentDirCheck.length-1]]);
  // }



  onFileSelected(event: Event) {
    if (!this.networkStatus.statusVal()){
      this.eventService.emit("addNotif", ["Not connected. Please check your internet connection.", 12000]);
      this.uploadInputHidden.nativeElement.value = '';
      return;
    }
    this.filesInMutualPath = [];
    if (this.crumbs[0] != "home"){
      this.filesService.getAllInHome().subscribe({
        next: data => {
          this.filesInMutualPath = data.map(f=>f.fileName);
          this.processUpload(this.filesInMutualPath, event);
        }
      });
    }
    else{
      this.filesInMutualPath = this.filesState.getFilesInViewer().map(f=>f.fileName);
      this.processUpload(this.filesInMutualPath, event);
    }
  }

  processUpload(filesInMutualPath:string[], event:Event){
    const input = event.target as HTMLInputElement;
    const files = input.files;
    let uploadPath = this.crumbs;
    if (uploadPath[0] != "home"){
      uploadPath = ["home"];
    }

    if (this.uploadInputHidden.nativeElement.webkitdirectory){
      let pathOfFoldersToBeCreated:string[] = [];
      let pathOfTraversedFiles:string[] = [];
      let folderCreationPathsForApiCalls:string[] = [];

      if (files && files.length>0){
        Array.from(files).forEach(file => {
          if (file.webkitRelativePath){
            pathOfTraversedFiles.push(file.webkitRelativePath);
          }
        });
        pathOfFoldersToBeCreated = pathOfTraversedFiles.sort((a,b)=>{
          return a.split('/').length - b.split('/').length;
        });
        let originalDuplicateNameBeforeModification = "";
        let modifiedRootFolderName = "";
        Array.from(pathOfFoldersToBeCreated).forEach((path, i) => {
          let separated = pathOfFoldersToBeCreated[i].split('/');
          separated.pop();
          for (let j = 0; j<separated.length; j++) {
            if (j==0 && i==0){
              originalDuplicateNameBeforeModification = separated[j];
              modifiedRootFolderName = Utils.findUniqueName(filesInMutualPath, separated[j]);
            }
            let pathToAppend = separated.slice(0,j+1);
            if (modifiedRootFolderName!=""){
              pathToAppend[0] = pathToAppend[0].replace(originalDuplicateNameBeforeModification, modifiedRootFolderName);
            }
            const creationPath = Utils.constructFilePathForApi([...uploadPath, ...pathToAppend]);
            if (!folderCreationPathsForApiCalls.includes(creationPath)){
              folderCreationPathsForApiCalls.push(creationPath);
            }
          }
          pathOfFoldersToBeCreated[i] = separated.join('/');
        });
        this.filesService.batchAddFolders(folderCreationPathsForApiCalls).subscribe({
          next:(f)=>{
            this.handleFilesUploadToServer(files, uploadPath, (file:File)=>{return file.webkitRelativePath.replace(originalDuplicateNameBeforeModification, modifiedRootFolderName).replaceAll("/","\\")});
          }
        });
      }
      console.log(folderCreationPathsForApiCalls);
    }

    else{
      this.handleFilesUploadToServer(files, uploadPath, (file:File)=>{return Utils.findUniqueName(filesInMutualPath,file.name.replaceAll("\\","-"), true)}, true);
    }
  }

  handleFilesUploadToServer(files:FileList|null, uploadPath:string[], pathProcessor:(f:File)=>string, onlyFiles:boolean=false){
    if (files) {
      const formData = new FormData();
      Array.from(files).forEach(file => {
        let processedName:string = file.name;
        const processedPath = pathProcessor(file);
        if (onlyFiles){
          processedName = processedPath;
        }
        formData.append("fileName", processedName);
        formData.append("filePath", Utils.constructFilePathForApi([...uploadPath, processedPath]));
        if (onlyFiles){
          this.filesInMutualPath.push(processedName);
        }
      });
      Array.from(files).forEach(file => {
        formData.append("file", file);
      });
      this.reportProgress();
      this.filesService.uploadFile(formData).pipe(finalize(() => {
        this.uploadInputHidden.nativeElement.value = "";
      })).subscribe({
        next: (event) => {
          this.handleProgress(event);
        },
      });
    }
  }

  handleProgress(event:any){
    switch (event.type) {
      case HttpEventType.UploadProgress:
        if (event.total) {
          this.uploadProgress = Math.round((100 * event.loaded) / event.total);
        }
        break;
      case HttpEventType.Response:
        console.log('Upload complete!', event.body);
        this.uploadProgress = -1;
        // this.uploadInputHidden.nativeElement.value = '';
        break;
    }
  }

  reportProgress() {
    const interval = setInterval(() => {
      localStorage.setItem("uploadProgress", this.uploadProgress.toString());

      if (this.uploadProgress === -1) {
        clearInterval(interval);
        this.uploadProgress = 0;
      }
    }, 500);
  }
}
