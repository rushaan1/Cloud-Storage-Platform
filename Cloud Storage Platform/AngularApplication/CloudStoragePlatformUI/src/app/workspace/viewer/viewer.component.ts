import {AfterViewChecked, AfterViewInit, Component, OnInit, ViewChild} from '@angular/core';
import { ItemSelectionService } from '../../services/item-selection.service';
import { EventService } from '../../services/event-service.service';
import {ActivatedRoute} from "@angular/router";
import {Folder} from "../../models/Folder";
import {File} from "../../models/File";

@Component({
  selector: 'viewer',
  templateUrl: './viewer.component.html',
  styleUrl: './viewer.component.css'
})
export class ViewerComponent implements OnInit{
  folders: Folder[] = [];
  files: File[] = [];

  searchQuery?:string;
  predefinedTypeFilter?:string;
  sortBy?:string;
  sortingOrder?:string;

  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const searchQuery = params['q'];
      const typeFilter = params['predefinedTypeFilter'];
      const sortBy = params['sortBy'];
      const sortingOrder = params['sortingOrder'];

      this.searchQuery = searchQuery;
      this.predefinedTypeFilter = typeFilter;
      this.sortBy = sortBy;
      this.sortingOrder = sortingOrder;
      localStorage["renaming"] = false;
      // use !queryName to see if query param is valid or empty/null/undefined
    });


    this.route.url.subscribe(url => {
      if (!url[0]){
        return;
      }
      switch(url[0].path){
        case "filter":
          if (url[1]){
            switch (url[1].path){
              case "home":
                // TODO
                this.loadHomeFolder();
                break;
              case "recents":
                break;
              case "favorites":
                break;
              case "trash":
                break;
            }
          }
          break;
        case "folder":
          break;
        case "searchFilter":
          // incase if search query param is not present redirect to home predefined filter because you cannot search empty or only spaces
          break;
        default:
          break;
      }
    });
  }

}
