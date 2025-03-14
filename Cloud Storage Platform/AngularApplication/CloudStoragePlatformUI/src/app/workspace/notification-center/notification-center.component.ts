import {
  AfterViewChecked,
  AfterViewInit,
  Component,
  ElementRef,
  HostListener,
  Renderer2,
  ViewChild
} from '@angular/core';
import { FilesStateService } from '../../services/StateManagementServices/files-state.service';
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
  @ViewChild("selectionInfoPanel") selectionInfoPanel!:ElementRef;
  @ViewChild("deleteConfirmNotif") deleteConfirmNotif!:ElementRef;

  orderedInfoPanels:HTMLElement[] = [];
  recentInfoPanelsInSequence:HTMLElement[] = [];

  itemsSelected = 0;
  mostRecentNonStickyNotification:HTMLElement | null = null;

  selectedItems:File[] = [];
  deleteFunc: (() => void) | undefined;

  /*

  The terminologies info-panel, notif and notification mean the same.

   */


  constructor(public itemSelectionService:FilesStateService, public eventService:EventService, private el: ElementRef, private renderer: Renderer2){}

  ngAfterViewInit(): void {
    this.orderedInfoPanels = [this.selectionInfoPanel.nativeElement, this.deleteConfirmNotif.nativeElement];
    //any new info panel must be added in the array above based on its position in the HTML file

    this.setNotificationEventListeners();
  }

  @HostListener('window:scroll', ['$event'])
  onWindowClick() {
    this.updateNotificationAlertTxt();
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

      const alertTxt:HTMLElement|null = this.mostRecentNonStickyNotification.querySelector(".alertTxt");
      if (visibleInfoPanels.length>0 && document.documentElement.scrollTop>34){
        alertTxt!.style.display = "inline";
        alertTxt!.innerText = `+${notificationQuantity} notifications!`;
      }
      else{
        if (this.mostRecentNonStickyNotification!=null) {
          alertTxt!.innerText = "";
          alertTxt!.style.display = "none";
        }
      }
    }
  }

  computeStickyPanelsTop(){
    let visibleOrderedStickyInfoPanels:HTMLElement[] = this.orderedInfoPanels;
    visibleOrderedStickyInfoPanels = Array.from(visibleOrderedStickyInfoPanels).filter(panel => {
      return (window.getComputedStyle(panel).display !== "none") && (panel.classList.contains("sticky-notif")==true);
    });

    if (visibleOrderedStickyInfoPanels.length>0){
      visibleOrderedStickyInfoPanels[0].style.top = `119.5px`;
      let cumulativeHeights = 0;
      for (let i = 1; i<visibleOrderedStickyInfoPanels.length; i++){
        let previousInfoPanelHeight:number = parseFloat(
          window.getComputedStyle(visibleOrderedStickyInfoPanels[i - 1]).height
        );
        cumulativeHeights += previousInfoPanelHeight;
        visibleOrderedStickyInfoPanels[i].style.top = `${cumulativeHeights+119.5}px`;
        console.log(previousInfoPanelHeight);
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

  dissmissTextNotif(div:any){
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

    this.eventService.listen("deleteConfirmNotif", (notifMsg:string, deleteFunc: (() => void) | undefined)=>{
      this.deleteConfirmNotif.nativeElement.style.display = "flex";
      this.deleteConfirmNotif.nativeElement.querySelector(".infoText").innerText = notifMsg;
      this.deleteFunc = deleteFunc;
    });

    this.eventService.listen("addNotif", (args:string|number[])=>{
      const notification:HTMLElement = this.createNotificationDiv(args[0] as string);
      this.setLatestAlertNotification(notification);
      setTimeout(()=>{
        this.dissmissTextNotif(notification);
      },args[1] as number)
    });
  }


  createNotificationDiv(notificationText:string): any {
    const infoPanel = this.renderer.createElement('div');
    infoPanel.style.display = "flex";

    this.renderer.addClass(infoPanel, 'info-panel');

    const icon = this.renderer.createElement('i');
    this.renderer.addClass(icon, 'fa-solid');
    this.renderer.addClass(icon, 'fa-square-xmark');
    this.renderer.listen(icon, 'click', (event: MouseEvent) => this.dissmissTextNotif(infoPanel));

    const textDiv = this.renderer.createElement('div');
    this.renderer.addClass(textDiv, 'margin-left');
    this.renderer.addClass(textDiv, 'infoText');

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

}
