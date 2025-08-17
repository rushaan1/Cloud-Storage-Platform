import {Component, ElementRef, OnInit, OnDestroy, ViewChild, NgZone, Input} from '@angular/core';
import {FilesAndFoldersService} from "../../services/ApiServices/files-and-folders.service";

@Component({
  selector: 'storage-bar',
  templateUrl: './storage-bar.component.html',
  styleUrl: './storage-bar.component.css'
})
export class StorageBarComponent implements OnInit, OnDestroy {
  @ViewChild('bg') bg!: ElementRef<HTMLDivElement>;
  @Input() isDashboard: boolean = false;
  usedGB: number = 0;
  percentUsed: number = 0;
  private sse?: EventSource;
  readonly totalGB = 10;

  constructor(private foldersService: FilesAndFoldersService, private ngZone: NgZone) { }

  ngOnInit(): void {
    // Load from localStorage if available
    const lastSizePercentage = localStorage.getItem('lastSizePercentage');
    const lastSizeInGb = localStorage.getItem('lastSizeInGb');
    if (lastSizePercentage && lastSizeInGb) {
      this.percentUsed = +lastSizePercentage;
      this.usedGB = +lastSizeInGb;
      setTimeout(() => {
        this.bg.nativeElement.style.width = this.percentUsed + "%";
      }, 0);
    }
    this.foldersService.ssetoken().subscribe({
      next: (res) => {
        this.sse = new EventSource('https://localhost:7219/api/Modifications/sse?token=' + res.sseToken);
        this.sse.onmessage = (event) => {
          this.ngZone.run(() => {
            const data = JSON.parse(event.data);
            if (data.eventType === "size_updated" && data.content && data.content.home === true && data.content.size !== undefined) {
              // Assuming size is in MB, convert to GB
              this.usedGB = +(data.content.size / 1024).toFixed(2);
              this.percentUsed = Math.min(100, (this.usedGB / this.totalGB) * 100);
              this.bg.nativeElement.style.width = this.percentUsed + "%";
              // Store in localStorage
              localStorage.setItem('lastSizePercentage', this.percentUsed.toString());
              localStorage.setItem('lastSizeInGb', this.usedGB.toString());
            }
          });
        };
      }
    });
  }

  ngOnDestroy(): void {
    if (this.sse) {
      this.sse.close();
    }
  }
}
