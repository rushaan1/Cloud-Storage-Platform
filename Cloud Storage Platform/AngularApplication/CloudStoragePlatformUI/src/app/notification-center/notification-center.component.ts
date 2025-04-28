import {
  AfterViewChecked,
  AfterViewInit, ChangeDetectorRef,
  Component,
  ElementRef,
  HostListener,
  Input,
  Renderer2,
  ViewChild
} from '@angular/core';
import {FilesStateService} from '../services/StateManagementServices/files-state.service';
import {EventService} from '../services/event-service.service';
import {File} from "../models/File";
import {forkJoin} from "rxjs";
import {FilesAndFoldersService} from "../services/ApiServices/files-and-folders.service";
import {BreadcrumbService} from "../services/StateManagementServices/breadcrumb.service";
import {Utils} from "../Utils";
import {Router} from "@angular/router";
import {FileType} from "../models/FileType";
import {NgIf} from "@angular/common";

@Component({
  selector: 'notification-center',
  templateUrl: './notification-center.component.html',
  styleUrl: './notification-center.component.css',
  imports: [
    NgIf
  ],
  standalone: true
})
export class NotificationCenterComponent implements AfterViewChecked, AfterViewInit {
  protected readonly window = window;
  @Input() fromViewer = true;
  @ViewChild("selectionInfoPanel") selectionInfoPanel!:ElementRef;
  @ViewChild("deleteConfirmNotif") deleteConfirmNotif!:ElementRef;
  @ViewChild("moveNotif") moveNotif!:ElementRef;
  @ViewChild("uploadProgressNotif") uploadProgressNotif!:ElementRef;
  @ViewChild("progressBar") progressBarElement!:ElementRef;
  @ViewChild("progressTxt") progressTxt!:ElementRef;

  orderedInfoPanels:HTMLElement[] = [];
  recentInfoPanelsInSequence:HTMLElement[] = [];

  itemsSelected = 0;
  mostRecentNonStickyNotification:HTMLElement | null = null;

  selectedItems:File[] = [];
  itemsBeingMoved:File[] = [];
  numberOfItemsBeingMoved = 0;
  deleteFunc: (() => void) | undefined;

  crumbs:string[] = [];

  /*

  The terminologies info-panel, notif and notification mean the same.

   */


  constructor(public filesState:FilesStateService, public eventService:EventService, private el: ElementRef, private renderer: Renderer2, private foldersService:FilesAndFoldersService, protected breadcrumbService:BreadcrumbService, protected router:Router, protected cd:ChangeDetectorRef){}

  ngAfterViewInit(): void {
    this.orderedInfoPanels = [this.uploadProgressNotif.nativeElement, this.selectionInfoPanel.nativeElement, this.deleteConfirmNotif.nativeElement, this.moveNotif.nativeElement];
    //any new info panel must be added in the array above based on its position in the HTML file

    this.setNotificationEventListeners();
    this.breadcrumbService.breadcrumbs$.subscribe((c)=>{
      this.crumbs = c;
    });
    if (Number(localStorage.getItem("uploadProgress"))!=-1){
      this.uploadProgressNotificationUpdate(Number(localStorage.getItem("uploadProgress")));
    }
  }

  @HostListener('window:scroll', ['$event'])
  onWindowScroll(): void {
    this.updateNotificationAlertTxt();
  }

  @HostListener('window:storage', ['$event'])
  onStorageChange(event:StorageEvent){
    if (event.key == "uploadProgress"){
      this.uploadProgressNotificationUpdate(Number(event.newValue));
    }
  }

  ngAfterViewChecked(): void {
    this.updateNotificationAlertTxt();
    this.computeStickyPanelsTop();
  }

  unselect(){
    this.filesState.deSelectAll();
  }

  // Helper to find the first scrollable parent
  private getScrollableParent(element: HTMLElement | null): HTMLElement | null {
    while (element) {
      const overflowY = window.getComputedStyle(element).overflowY;
      if (overflowY === 'auto' || overflowY === 'scroll') {
        return element;
      }
      element = element.parentElement;
    }
    return null;
  }

  updateNotificationAlertTxt(){
    const infoPanels = document.getElementsByClassName("info-panel");
    const visibleInfoPanels = Array.from(infoPanels).filter(panel => {
      return (window.getComputedStyle(panel).display !== "none") && (panel.classList.contains("sticky-notif")==false);
    });

    if (this.mostRecentNonStickyNotification!=null){
      const notificationQuantity = visibleInfoPanels.length;

      const alertTexts = document.getElementsByClassName("alertTxt");
      for (let i = 0; i < alertTexts.length; i++) {
        alertTexts[i].textContent = "";
      }
      const alertTxt:HTMLElement|null = this.mostRecentNonStickyNotification.querySelector(".alertTxt");
      let scrollTop = window.scrollY || document.documentElement.scrollTop;
      if (scrollTop === 0) {
        const scrollableParent = this.getScrollableParent(this.el.nativeElement.parentElement);
        if (scrollableParent) {
          scrollTop = scrollableParent.scrollTop;
        }
      }
      if (visibleInfoPanels.length>0 && scrollTop>90){
        alertTxt!.style.display = "inline";
        alertTxt!.innerText = `+${notificationQuantity} notifications!`;
      }
      else{
        if (this.mostRecentNonStickyNotification!=null) {
          alertTxt!.innerText = "";
          alertTxt!.style.display = "none";
        }
      }
      this.cd.detectChanges();
    }
  }

  computeStickyPanelsTop(){
    let visibleOrderedStickyInfoPanels:HTMLElement[] = this.orderedInfoPanels;
    visibleOrderedStickyInfoPanels = Array.from(visibleOrderedStickyInfoPanels).filter(panel => {
      return (window.getComputedStyle(panel).display !== "none") && (panel.classList.contains("sticky-notif")==true);
    });

    // Dynamically get the height of the panel and breadcrumbs element
    let panelHeight = 0;
    let breadcrumbsHeight = 0;
    const panelElem = document.querySelector('.top-panel');
    if (panelElem) {
      panelHeight = (panelElem as HTMLElement).getBoundingClientRect().height;
    }
    const breadcrumbsElem = document.querySelector('.breadcrumbs');
    if (breadcrumbsElem) {
      breadcrumbsHeight = (breadcrumbsElem as HTMLElement).getBoundingClientRect().height;
    }

    let offset = panelHeight + breadcrumbsHeight;
    if (!this.fromViewer) {
      offset += 43.5;
    }
    offset -= 5;


    if (visibleOrderedStickyInfoPanels.length>0){
      visibleOrderedStickyInfoPanels[0].style.top = `${offset}px`;
      let cumulativeHeights = 0;
      for (let i = 1; i<visibleOrderedStickyInfoPanels.length; i++){
        let previousInfoPanelHeight:number = parseFloat(
          window.getComputedStyle(visibleOrderedStickyInfoPanels[i - 1]).height
        );
        cumulativeHeights += previousInfoPanelHeight;
        visibleOrderedStickyInfoPanels[i].style.top = `${cumulativeHeights+offset}px`;
        // console.log(previousInfoPanelHeight);
      }
    }
  }

  dismissNotificationAlert(event:MouseEvent){
    const infoPanel = event.target as HTMLElement;
    infoPanel.parentElement!.style.display = "none";
    if (this.recentInfoPanelsInSequence.length>0){
      this.setLatestAlertNotification(this.recentInfoPanelsInSequence.pop() as HTMLElement, true);
    }
  }

  dismissTextNotif(div:any){
    if (this.recentInfoPanelsInSequence.length>0){
      this.setLatestAlertNotification(this.recentInfoPanelsInSequence.pop() as HTMLElement, true);
    }
    this.renderer.removeChild(this.el.nativeElement, div);
  }

  // Alert notifications can be dismissed and if there are multiple alert notifs only 1 of them will be sticky
  setLatestAlertNotification(infoPanel:HTMLElement, beingRemoved:boolean=false){
    if (this.mostRecentNonStickyNotification){
      this.mostRecentNonStickyNotification.classList.remove("sticky-notif");
      if (!beingRemoved){
        this.recentInfoPanelsInSequence.push(this.mostRecentNonStickyNotification);
      }
    }
    this.mostRecentNonStickyNotification = infoPanel;
    this.mostRecentNonStickyNotification.classList.add("sticky-notif");

    const infoText = this.mostRecentNonStickyNotification.querySelector(".infoText");
    const timestamp:HTMLElement|null|undefined = infoText?.querySelector(".timestamp");

    const now = new Date();
    const hours = now.getHours().toString().padStart(2, '0');
    const minutes = now.getMinutes().toString().padStart(2, '0');
    const newInnerText = `at ${hours}:${minutes}`;

    if (timestamp){
      timestamp.innerText = newInnerText;
    }
    else{
      const newTimestamp = document.createElement("span");
      newTimestamp.innerText = newInnerText;
      newTimestamp.style.paddingLeft = "8px";
      newTimestamp.style.color = "gray";
      newTimestamp.classList.add("timestamp");
      infoText?.appendChild(newTimestamp);
    }
  }


  setNotificationEventListeners(){

    this.filesState.selectedItems$.subscribe(selectedFiles => {
      this.selectedItems = selectedFiles;
      if (this.selectedItems.length > 0) {
        this.selectionInfoPanel.nativeElement.style.display = "flex";
      }
      else{
        this.selectionInfoPanel.nativeElement.style.display = "none";
      }
      this.itemsSelected = this.selectedItems.length;
    });

    this.filesState.itemsBeingMoved$.subscribe(items => {
      if (items.length > 0) {
        this.moveNotif.nativeElement.style.display = "flex";
      }
      else{
        this.moveNotif.nativeElement.style.display = "none";
      }
      this.itemsBeingMoved = items;
      this.numberOfItemsBeingMoved = items.length;
    });

    this.eventService.listen("deleteConfirmNotif", (notifMsg:string, deleteFunc: (() => void) | undefined)=>{
      this.deleteConfirmNotif.nativeElement.style.display = "flex";
      this.deleteConfirmNotif.nativeElement.querySelector(".infoText").innerText = notifMsg;
      this.deleteFunc = deleteFunc;
    });

    this.eventService.listen("addNotif", (args:string|number[])=>{
      const notifText:string = args[0] as string;
      const duplicate = document.getElementsByClassName("divToDetectDuplicateText")[0]
      if (duplicate){
        const regex = /^(.*)\sat.*$/;
        // if (duplicate.textContent!.replace(regex,"$1") == notifText){
        //   return;
        // }
      }
      const notification:HTMLElement = this.createNotificationDiv(notifText);
      this.setLatestAlertNotification(notification);
      setTimeout(()=>{
        this.dismissTextNotif(notification);
      },args[1] as number)
    });

    this.eventService.listen("uploadProgress", (progress:number)=>{
      this.uploadProgressNotificationUpdate(progress);
    });
  }

  uploadProgressNotificationUpdate(progress:number){
    this.uploadProgressNotif.nativeElement.style.display = "flex";
    if (progress==-1){
      progress = 100;
      setTimeout(()=>{
        this.uploadProgressNotif.nativeElement.style.display = "none";
        const foldersUploaded = localStorage.getItem("foldersUploadedQty");
        const filesUploaded = localStorage.getItem("filesUploadedQty");
        if (filesUploaded !==null && filesUploaded!==undefined){
          if (foldersUploaded !==null && foldersUploaded!==undefined) {
            if (Number(foldersUploaded) > 0) {
              this.eventService.emit("addNotif", ["Uploaded " + foldersUploaded + " folder(s) and " + filesUploaded + " file(s) to "+Utils.resize(decodeURIComponent(localStorage.getItem("uploadingTo")!),25), 12000]);
              return;
            }
          }
          this.eventService.emit("addNotif", ["Uploaded "+filesUploaded+" file(s) to "+Utils.resize(decodeURIComponent(localStorage.getItem("uploadingTo")!),25) , 12000]);
        }
      }, 1500);
    }
    this.progressBarElement.nativeElement.style.width = `${progress}%`;
    this.progressTxt.nativeElement.innerText = `${progress}%`;
  }


  createNotificationDiv(notificationText:string): any {
    const infoPanel = this.renderer.createElement('div');
    infoPanel.style.display = "flex";

    this.renderer.addClass(infoPanel, 'info-panel');

    const icon = this.renderer.createElement('i');
    this.renderer.addClass(icon, 'fa-solid');
    this.renderer.addClass(icon, 'fa-square-xmark');
    this.renderer.listen(icon, 'click', (event: MouseEvent) => this.dismissTextNotif(infoPanel));

    const textDiv = this.renderer.createElement('div');
    this.renderer.addClass(textDiv, 'margin-left');
    this.renderer.addClass(textDiv, 'infoText');
    this.renderer.addClass(textDiv, 'divToDetectDuplicateText');

    const textNode = this.renderer.createText(notificationText + " ");
    this.renderer.appendChild(textDiv, textNode);

    const anchor = this.renderer.createElement('a');
    this.renderer.addClass(anchor, 'alertTxt');
    this.renderer.listen(anchor, 'click', () => {
      window.scrollTo({ top: 0, behavior: 'smooth' });
    });
    this.renderer.appendChild(textDiv, anchor);
    this.renderer.appendChild(infoPanel, icon);
    this.renderer.appendChild(infoPanel, textDiv);
    this.renderer.appendChild(this.el.nativeElement, infoPanel);
    this.orderedInfoPanels.push(infoPanel);
    return infoPanel;
  }

  moveToTrash(){
    const filesBeingMoved = this.selectedItems.filter(f=>{return f.fileType!=FileType.Folder});
    const foldersBeingMoved = this.selectedItems.filter(f=>{return f.fileType==FileType.Folder});
    forkJoin({
      files: this.foldersService.batchAddOrRemoveFilesFromTrash(filesBeingMoved.map((f)=>f.fileId)),
      folders: this.foldersService.batchAddOrRemoveFoldersFromTrash(foldersBeingMoved.map((f)=>f.fileId))
    }).subscribe({
      next: ()=>{
        setTimeout(()=>{this.eventService.emit("addNotif", ["Moved "+(foldersBeingMoved.length+filesBeingMoved.length)+" item(s) to trash.", 12000]);},800);
        this.filesState.deSelectAll();
      }
    });
  }

  restore(){
    const filesBeingMoved = this.selectedItems.filter(f=>{return f.fileType!=FileType.Folder});
    const foldersBeingMoved = this.selectedItems.filter(f=>{return f.fileType==FileType.Folder});
    forkJoin({
      files: this.foldersService.batchAddOrRemoveFilesFromTrash(filesBeingMoved.map((f)=>f.fileId)),
      folders: this.foldersService.batchAddOrRemoveFoldersFromTrash(foldersBeingMoved.map((f)=>f.fileId))
    }).subscribe({
      next: ()=>{
        setTimeout(()=>{this.eventService.emit("addNotif", ["Restored "+(foldersBeingMoved.length+filesBeingMoved.length)+" item(s).", 12000]);},800);
        this.filesState.deSelectAll();
      }
    });
  }

  delete(){
    this.eventService.emit("deleteConfirmNotif", "Are you sure you want to permanently delete all selected items?", ()=>{
      const filesBeingDeleted = this.selectedItems.filter(f=>{return f.fileType!=FileType.Folder});
      const foldersBeingDeleted = this.selectedItems.filter(f=>{return f.fileType==FileType.Folder});
      forkJoin({
        files: this.foldersService.batchDeleteFiles(filesBeingDeleted.map((f)=>f.fileId)),
        folders: this.foldersService.batchDeleteFolders(foldersBeingDeleted.map((f)=>f.fileId))
      }).subscribe({
        next: ()=>{
          setTimeout(()=>{this.eventService.emit("addNotif", ["Deleted "+(foldersBeingDeleted.length+filesBeingDeleted.length)+" item(s).", 12000]);},800);
          this.filesState.deSelectAll();
        }
      });
    });
  }

  activateMoveState(){
    this.filesState.setItemsBeingMoved(this.selectedItems);
    this.filesState.deSelectAll();
    if (this.router.url.includes("filter/home")){
      this.eventService.emit("reload viewer list");
      return;
    }
    this.router.navigate(["filter","home"]);
  }

  move(){
    const crumbsLowerCase:string[] = this.breadcrumbService.getBreadcrumbs().map(c=>c.toLowerCase());
    let destinationCrumbs = this.breadcrumbService.getBreadcrumbs();
    if (crumbsLowerCase.includes("filter")){
      if (crumbsLowerCase.includes("home")){
        destinationCrumbs = ["home"];
      }
      else {
        return;
      }
    }

    const filesBeingMoved = this.filesState.getItemsBeingMoved().filter(f=>{return f.fileType!=FileType.Folder});
    const foldersBeingMoved = this.filesState.getItemsBeingMoved().filter(f=>{return f.fileType==FileType.Folder});
    forkJoin({
      files: this.foldersService.batchMoveFiles(filesBeingMoved.map((f)=>f.fileId), Utils.constructFilePathForApi(destinationCrumbs)),
      folders: this.foldersService.batchMoveFolders(foldersBeingMoved.map((f)=>f.fileId), Utils.constructFilePathForApi(destinationCrumbs))
    }).subscribe({
      next: ()=>{
        setTimeout(()=>{this.eventService.emit("addNotif", ["Moved "+(foldersBeingMoved.length+filesBeingMoved.length)+" item(s) to "+decodeURIComponent(this.breadcrumbService.getBreadcrumbs()[this.breadcrumbService.getBreadcrumbs().length-1])+".", 12000]);},800);
        this.filesState.setItemsBeingMoved([]);
      }
    });
  }

  moveAbort(){
    this.filesState.setItemsBeingMoved([]);
  }

  getConcatenatedMovingItemsNames(){
    return this.itemsBeingMoved.map(f=>f.fileName).join(", ");
  }

  download() {
    const folders = this.selectedItems.filter(item => item.fileType == FileType.Folder);
    const files = this.selectedItems.filter(item => item.fileType != FileType.Folder);

    const folderIds = folders.map(item => item.fileId).join("&folderIds=");
    const fileIds = files.map(item => item.fileId).join("&fileIds=");

    const queryParams = [];

    if (folderIds) {
      queryParams.push(`folderIds=${folderIds}`);
    }

    if (fileIds) {
      queryParams.push(`fileIds=${fileIds}`);
    }

    const nameSource = folders.length > 0 ? folders[0] : files[0];
    const name = encodeURIComponent(nameSource.fileName);

    queryParams.push(`name=${name}`);

    const url = `https://localhost:7219/api/Retrievals/download?${queryParams.join("&")}`;

    window.open(url, '_blank');
    this.unselect();
  }

}
