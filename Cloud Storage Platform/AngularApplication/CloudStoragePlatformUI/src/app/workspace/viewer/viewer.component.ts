import { AfterViewChecked, AfterViewInit, Component, ViewChild } from '@angular/core';
import { ItemSelectionService } from '../../services/item-selection.service';
import { EventService } from '../../services/event-service.service';

@Component({
  selector: 'viewer',
  templateUrl: './viewer.component.html',
  styleUrl: './viewer.component.css'
})
export class ViewerComponent {
  files = ["folder hello", "folder 2", "folder 3","folder 4", "folder 5", "folder 6", "folder 7", "folder 8", "folder 9", "folder 10","folder 11", "folder 12", "folder 13", "folder 14", "folder 15", "folder 16"];
}
