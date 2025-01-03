import {AfterViewChecked, AfterViewInit, Component, OnInit, ViewChild} from '@angular/core';
import { ItemSelectionService } from '../../services/item-selection.service';
import { EventService } from '../../services/event-service.service';
import {ActivatedRoute} from "@angular/router";

@Component({
  selector: 'viewer',
  templateUrl: './viewer.component.html',
  styleUrl: './viewer.component.css'
})
export class ViewerComponent implements OnInit{
  files = ["folder hello", "folder 2", "folder 3","folder 4", "folder 5", "folder 6", "folder 7", "folder 8", "folder 9", "folder 10","folder 11", "folder 12", "folder 13", "folder 14", "folder 15", "folder 16"];
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
        case "preDefinedFilter":
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
