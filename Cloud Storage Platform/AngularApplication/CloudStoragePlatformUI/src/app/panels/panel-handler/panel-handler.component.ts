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

  constructor(public itemSelectionService:ItemSelectionService, public eventService:EventService){}

  ngAfterViewInit(): void {
    this.orderedStickyInfoPanels = [this.notificationAlert, this.selectionInfoPanel];
    this.sampleTextInfoPanel.nativeElement.style.display = "none";
    this.eventService.listen("checkbox selection change",()=>{
      this.itemsSelected = this.itemSelectionService.selectedItems.length; // had to do this because change was not being detected when checkbox was being selected/unselected until mouse moved
    });
    this.computeStickyPanelsTop();
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

  }

  anyItemsSelected():boolean{
    return (this.itemSelectionService.selectedItems.length>0);
  }

  unselect(){
    this.eventService.emit("unselector all", 0);
  }

  showNotificationAlert(){
    const infoPanels = document.getElementsByClassName("sticky-notif");
    const visibleInfoPanels = Array.from(infoPanels).filter(panel => {
      return (window.getComputedStyle(panel).display !== "none") && (panel.id != "notificationAlert");
    });

    if (visibleInfoPanels.length>0){
      this.notificationAlert.nativeElement.style.display = "flex";
    }
    else{
      this.notificationAlert.nativeElement.style.display = "none";
    }
  }

  computeStickyPanelsTop(){
    this.orderedStickyInfoPanels[0].nativeElement.style.top = "62px";
    for (let i = 1; i<this.orderedStickyInfoPanels.length; i++){
      this.orderedStickyInfoPanels[i].nativeElement.style.top = `${62+(i*31)}px`;
    }
  }
}
