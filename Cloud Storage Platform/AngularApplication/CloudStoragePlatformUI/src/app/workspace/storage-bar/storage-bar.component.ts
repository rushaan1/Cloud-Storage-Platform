import {Component, ElementRef, OnInit, ViewChild} from '@angular/core';
import {FilesStateService} from "../../services/StateManagementServices/files-state.service";

@Component({
  selector: 'storage-bar',
  templateUrl: './storage-bar.component.html',
  styleUrl: './storage-bar.component.css'
})
export class StorageBarComponent implements OnInit {
  @ViewChild('bg') bg!: ElementRef<HTMLDivElement>;
  constructor(protected filesState:FilesStateService) { }

  ngOnInit(): void {
    setTimeout(()=>{
      this.bg.nativeElement.style.width = "50%";
    },100);
  }
}
