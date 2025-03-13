import { AfterViewChecked, AfterViewInit, Component, ElementRef, ViewChild } from '@angular/core';
import { ItemSelectionService } from '../../services/item-selection.service';
import { EventService } from '../../services/event-service.service';
import {File} from "../../models/File";
import {timestamp} from "rxjs";

@Component({
  selector: 'notification-center',
  templateUrl: './notification-center.component.html',
  styleUrl: './notification-center.component.css'
})
export class NotificationCenterComponent implements AfterViewChecked, AfterViewInit {
  protected readonly window = window;
  @ViewChild("emptyInputNotif") emptyInputNotif!:ElementRef;
  @ViewChild("invalidCharacterNotif") invalidCharacterNotif!:ElementRef;
  @ViewChild("alreadyRenamingNotif") alreadyRenamingNotif!:ElementRef;
  @ViewChild("selectionInfoPanel") selectionInfoPanel!:ElementRef;
  @ViewChild("renameSuccessNotif") renameSuccessNotif!:ElementRef;
  @ViewChild("deleteConfirmNotif") deleteConfirmNotif!:ElementRef;
  @ViewChild("deleteSuccessNotif") deleteSuccessNotif!:ElementRef;
  @ViewChild("restoreSuccessNotif") restoreSuccessNotif!:ElementRef;
  @ViewChild("movedToTrashNotif") movedToTrashNotif!:ElementRef;

  orderedInfoPanels:ElementRef[] = [];
  recentInfoPanelsInSequence:ElementRef[] = [];

  itemsSelected = 0;
  mostRecentNonStickyNotification:ElementRef | null = null;

  invalidCharacter:string="";
  successRenamedToName:string="";
  selectedItems:File[] = [];
  folderToBeDeletedName:string="";
  folderSuccessfullyDeletedName:string="";
  folderSuccessfullyRestoredName:string="";
  deleteFunc: (() => void) | undefined;
  folderMovedToTrashName: string = "";

  /*

  The terminologies info-panel, notif and notification mean the same.

   */


  constructor(public itemSelectionService:ItemSelectionService, public eventService:EventService){}

  ngAfterViewInit(): void {
    this.orderedInfoPanels = [this.selectionInfoPanel, this.emptyInputNotif, this.invalidCharacterNotif, this.alreadyRenamingNotif, this.renameSuccessNotif, this.deleteSuccessNotif, this.deleteConfirmNotif, this.restoreSuccessNotif, this.movedToTrashNotif];
    //any new info panel must be added in the array above based on its position in the HTML file

    window.addEventListener("scroll", () => {
      this.updateNotificationAlertTxt();
    });

    this.setNotificationEventListeners();
  }

  ngAfterViewChecked(): void {
    this.updateNotificationAlertTxt();
    this.computeStickyPanelsTop();
  }

  unselect(){
    this.itemSelectionService.deSelectAll();
  }

  updateNotificationAlertTxt(){
    const infoPanels = document.getElementsByClassName("info-panel");
    const visibleInfoPanels = Array.from(infoPanels).filter(panel => {
      return (window.getComputedStyle(panel).display !== "none") && (panel.classList.contains("sticky-notif")==false);
    });

    if (this.mostRecentNonStickyNotification!=null){
      const notificationQuantity = visibleInfoPanels.length;

      const alertTxt:HTMLElement = this.mostRecentNonStickyNotification.nativeElement.querySelector(".alertTxt");
      if (visibleInfoPanels.length>0 && document.documentElement.scrollTop>34){
        alertTxt.style.display = "inline";
        alertTxt.innerText = `+${notificationQuantity} notifications!`;
      }
      else{
        if (this.mostRecentNonStickyNotification!=null) {
          alertTxt.innerText = "";
          alertTxt.style.display = "none";
        }
      }
    }
  }

  computeStickyPanelsTop(){
    let visibleOrderedStickyInfoPanels:ElementRef[] = this.orderedInfoPanels;
    visibleOrderedStickyInfoPanels = Array.from(visibleOrderedStickyInfoPanels).filter(panel => {
      return (window.getComputedStyle(panel.nativeElement).display !== "none") && (panel.nativeElement.classList.contains("sticky-notif")==true);
    });

    if (visibleOrderedStickyInfoPanels.length>0){
      visibleOrderedStickyInfoPanels[0].nativeElement.style.top = `120px`;
      let cumulativeHeights = 0;
      for (let i = 1; i<visibleOrderedStickyInfoPanels.length; i++){
        let previousInfoPanelHeight:number = parseFloat(
          window.getComputedStyle(visibleOrderedStickyInfoPanels[i - 1].nativeElement).height
        );
        cumulativeHeights += previousInfoPanelHeight;
        visibleOrderedStickyInfoPanels[i].nativeElement.style.top = `${cumulativeHeights+120}px`;
        console.log(previousInfoPanelHeight);
      }
    }
  }

  dismissNotificationAlert(event:MouseEvent){
    const infoPanel = event.target as HTMLElement;
    infoPanel.parentElement!.style.display = "none";
    if (this.recentInfoPanelsInSequence.length>0){
      this.setLatestAlertNotification(this.recentInfoPanelsInSequence.pop() as ElementRef, true);
    }


    if (infoPanel.parentElement==this.invalidCharacterNotif.nativeElement){
      this.invalidCharacter = "";
    }
  }

  // Alert notifications can be dismissed and if there are multiple alert notifs only 1 of them will be sticky
  setLatestAlertNotification(infoPanel:ElementRef, beingRemoved:boolean=false){
    if (this.mostRecentNonStickyNotification){
      this.mostRecentNonStickyNotification.nativeElement.classList.remove("sticky-notif");
      if (!beingRemoved){
        this.recentInfoPanelsInSequence.push(this.mostRecentNonStickyNotification);
      }
    }
    this.mostRecentNonStickyNotification = infoPanel;
    this.mostRecentNonStickyNotification.nativeElement.classList.add("sticky-notif");

    const infoText = this.mostRecentNonStickyNotification.nativeElement.querySelector(".infoText");
    const timestamp = infoText.querySelector(".timestamp");

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
      infoText.appendChild(newTimestamp);
    }
  }


  setNotificationEventListeners(){

    this.itemSelectionService.selectedItems$.subscribe(selectionInfoPanels => {
      this.selectedItems = selectionInfoPanels;
      if (this.selectedItems.length > 0) {
        this.selectionInfoPanel.nativeElement.style.display = "flex";
      }
      else{
        this.selectionInfoPanel.nativeElement.style.display = "none";
      }
      this.itemsSelected = this.selectedItems.length;
    });

    this.eventService.listen("emptyInputNotif", ()=>{
      this.emptyInputNotif.nativeElement.style.display = "flex";
      this.setLatestAlertNotification(this.emptyInputNotif);
    });

    this.eventService.listen("invalidCharacterNotif", (character:string)=>{
      this.invalidCharacterNotif.nativeElement.style.display = "flex";
      this.setLatestAlertNotification(this.invalidCharacterNotif);
      this.invalidCharacter = character;
    });

    this.eventService.listen("alreadyRenamingNotif", ()=>{
      this.alreadyRenamingNotif.nativeElement.style.display = "flex";
      this.setLatestAlertNotification(this.alreadyRenamingNotif);
    });

    this.eventService.listen("renameSuccessNotif", (renamedTo:string)=>{
      this.renameSuccessNotif.nativeElement.style.display = "flex";
      this.setLatestAlertNotification(this.renameSuccessNotif);
      this.successRenamedToName = renamedTo;
    });

    this.eventService.listen("deleteConfirmNotif", (folderToBeDeletedName:string, deleteFunc: (() => void) | undefined)=>{
      this.deleteConfirmNotif.nativeElement.style.display = "flex";
      this.folderToBeDeletedName = folderToBeDeletedName;
      this.deleteFunc = deleteFunc;
    });

    this.eventService.listen("folderSuccessfullyDeletedNotif", (folderSuccessfullyDeletedName:string)=>{
      this.deleteSuccessNotif.nativeElement.style.display = "flex";
      this.folderSuccessfullyDeletedName = folderSuccessfullyDeletedName;
      this.setLatestAlertNotification(this.deleteSuccessNotif);
    });

    this.eventService.listen("folderSuccessfullyRestoredNotif", (folderSuccessfullyRestoredName:string)=>{
      this.restoreSuccessNotif.nativeElement.style.display = "flex";
      this.folderSuccessfullyRestoredName = folderSuccessfullyRestoredName;
      this.setLatestAlertNotification(this.restoreSuccessNotif);
    });

    this.eventService.listen("folderSuccessfullyMovedToTrashNotif", (folderMovedToTrashName:string)=>{
      this.movedToTrashNotif.nativeElement.style.display = "flex";
      this.folderMovedToTrashName = folderMovedToTrashName;
      this.setLatestAlertNotification(this.movedToTrashNotif);
    });
  }
}
