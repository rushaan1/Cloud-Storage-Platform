import { AfterViewChecked, AfterViewInit, Component, ElementRef, ViewChild } from '@angular/core';
import { ItemSelectionService } from '../../services/item-selection.service';
import { EventService } from '../../services/event-service.service';
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
  orderedInfoPanels:ElementRef[] = [];
  recentInfoPanelsInSequence:ElementRef[] = [];

  itemsSelected = 0;
  mostRecentNonStickyNotification:ElementRef | null = null;

  invalidCharacter:string="";

  /*

  The terminologies info-panel, notif and notification mean the same.

   */

  constructor(public itemSelectionService:ItemSelectionService, public eventService:EventService){}

  ngAfterViewInit(): void {
    this.orderedInfoPanels = [this.selectionInfoPanel, this.emptyInputNotif, this.invalidCharacterNotif, this.alreadyRenamingNotif];
    //any new info panel must be added in the array above based on its position in the HTML file
    this.eventService.listen("checkbox selection change",()=>{
      this.itemsSelected = this.itemSelectionService.selectedItems.length; // had to do this because change was not being detected when checkbox was being selected/unselected until mouse moved
    });
    window.addEventListener("scroll", () => {
      this.updateNotificationAlerts();
    });

    this.setNotificationEventListeners();
  }

  ngAfterViewChecked(): void {
    let isSelected = this.anyItemsSelected();
    if (isSelected){
      this.selectionInfoPanel.nativeElement.style.display = "flex";
      this.itemsSelected = this.itemSelectionService.selectedItems.length;
    }
    else{
      this.selectionInfoPanel.nativeElement.style.display = "none";
    }

    this.updateNotificationAlerts();
    this.computeStickyPanelsTop();
  }

  anyItemsSelected():boolean{
    return (this.itemSelectionService.selectedItems.length>0);
  }

  unselect(){
    this.eventService.emit("unselector all", 0);
  }

  updateNotificationAlerts(){
    const infoPanels = document.getElementsByClassName("info-panel");
    const visibleInfoPanels = Array.from(infoPanels).filter(panel => {
      return (window.getComputedStyle(panel).display !== "none") && (panel.classList.contains("sticky-notif")==false);
    });

    if (this.mostRecentNonStickyNotification!=null){
      const notificationQuantity = visibleInfoPanels.length;
      const latestNotif:HTMLElement = this.mostRecentNonStickyNotification.nativeElement.querySelector(".alertTxt");
      if (visibleInfoPanels.length>0 && document.documentElement.scrollTop>34){
        latestNotif.style.display = "inline";
        latestNotif.innerText = `+${notificationQuantity} alerts!`;
      }
      else{
        if (this.mostRecentNonStickyNotification!=null) {
          latestNotif.innerText = "";
          latestNotif.style.display = "none";
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
      visibleOrderedStickyInfoPanels[0].nativeElement.style.top = `65px`;
      let cumulativeHeights = 0;
      for (let i = 1; i<visibleOrderedStickyInfoPanels.length; i++){
        let previousInfoPanelHeight:number = parseFloat(
          window.getComputedStyle(visibleOrderedStickyInfoPanels[i - 1].nativeElement).height
        );
        cumulativeHeights += previousInfoPanelHeight;
        visibleOrderedStickyInfoPanels[i].nativeElement.style.top = `${cumulativeHeights+65}px`;
        console.log(previousInfoPanelHeight);
      }
    }
  }

  dismissNotificationAlert(event:MouseEvent){
    const infoPanel = event.target as HTMLElement;
    infoPanel.parentElement!.style.display = "none";
    if (this.recentInfoPanelsInSequence.length>0){
      this.setLatestNotification(this.recentInfoPanelsInSequence.pop() as ElementRef, true);
    }


    if (infoPanel.parentElement==this.invalidCharacterNotif.nativeElement){
      this.invalidCharacter = "";
    }
  }

  setLatestNotification(infoPanel:ElementRef, beingRemoved:boolean=false){
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
    this.eventService.listen("emptyInputNotif", ()=>{
      this.emptyInputNotif.nativeElement.style.display = "flex";
      this.setLatestNotification(this.emptyInputNotif);
    });

    this.eventService.listen("invalidCharacterNotif", (character:string)=>{
      this.invalidCharacterNotif.nativeElement.style.display = "flex";
      this.setLatestNotification(this.invalidCharacterNotif);
      this.invalidCharacter = character;
    });

    this.eventService.listen("alreadyRenamingNotif", ()=>{
      this.alreadyRenamingNotif.nativeElement.style.display = "flex";
      this.setLatestNotification(this.alreadyRenamingNotif);
    });
  }
}
