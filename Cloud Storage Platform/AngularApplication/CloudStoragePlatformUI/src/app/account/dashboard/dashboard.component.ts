import {Component, OnDestroy, OnInit} from '@angular/core';
import {FilesStateService} from "../../services/StateManagementServices/files-state.service";

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit, OnDestroy{
  constructor(private filesState:FilesStateService) {}
  ngOnInit(): void {
    this.filesState.outsideFilesAndFoldersMode = true;
    this.filesState.showSpaceUtilizedInNavDrawer = false;
  }
  ngOnDestroy(): void {
    this.filesState.outsideFilesAndFoldersMode = false;
    this.filesState.showSpaceUtilizedInNavDrawer = true;
  }
}
