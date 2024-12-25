import { AfterViewChecked, AfterViewInit, Component, ElementRef, ViewChild } from '@angular/core';
import { ItemSelectionService } from '../../services/item-selection.service';
import { EventService } from '../../services/event-service.service';

@Component({
  selector: 'info-panel-handler',
  templateUrl: './panel-handler.component.html',
  styleUrl: './panel-handler.component.css'
})
export class PanelHandlerComponent implements AfterViewChecked, AfterViewInit {
  @ViewChild("notificationAlert") notificationAlert!:ElementRef;
  @ViewChild("sampleTextInfoPanel") sampleTextInfoPanel!:ElementRef;
  @ViewChild("selectionInfoPanel") selectionInfoPanel!:ElementRef;
  orderedStickyInfoPanels:ElementRef[] = [];
  itemsSelected = 0;
  notificationQuantity = 0;

  constructor(public itemSelectionService:ItemSelectionService, public eventService:EventService){}

  ngAfterViewInit(): void {
    this.orderedStickyInfoPanels = [this.notificationAlert, this.selectionInfoPanel];
    this.sampleTextInfoPanel.nativeElement.style.display = "flex";
    this.eventService.listen("checkbox selection change",()=>{
      this.itemsSelected = this.itemSelectionService.selectedItems.length; // had to do this because change was not being detected when checkbox was being selected/unselected until mouse moved
    });
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

    this.showNotificationAlert();
    this.computeStickyPanelsTop();
  }

  anyItemsSelected():boolean{
    return (this.itemSelectionService.selectedItems.length>0);
  }

  unselect(){
    this.eventService.emit("unselector all", 0);
  }

  showNotificationAlert(){
    const infoPanels = document.getElementsByClassName("info-panel");
    const visibleInfoPanels = Array.from(infoPanels).filter(panel => {
      return (window.getComputedStyle(panel).display !== "none") && (panel.classList.contains("sticky-notif")==false);
    });
    this.notificationQuantity = visibleInfoPanels.length;
    if (visibleInfoPanels.length>0){
      this.notificationAlert.nativeElement.style.display = "flex";
    }
    else{
      this.notificationAlert.nativeElement.style.display = "none";
    }
  }

  computeStickyPanelsTop(){
    let visibleOrderedStickyInfoPanels:ElementRef[] = this.orderedStickyInfoPanels;
    visibleOrderedStickyInfoPanels = Array.from(visibleOrderedStickyInfoPanels).filter(panel => {
      return (window.getComputedStyle(panel.nativeElement).display !== "none")
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
  }
}
