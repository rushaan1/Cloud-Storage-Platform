import {Component, ElementRef, OnInit, OnDestroy, ViewChild, NgZone, Input} from '@angular/core';
import {FilesAndFoldersService} from "../../services/ApiServices/files-and-folders.service";
import {AccountService} from "../../services/ApiServices/account.service";

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

  constructor(
    private foldersService: FilesAndFoldersService,
    private accountService: AccountService,
    private ngZone: NgZone
  ) { }

  ngOnInit(): void {
    // Fetch current space used from server
    this.accountService.getUserSpaceUsed().subscribe({
      next: (res) => {
        if (res.sizeInMB !== undefined && res.sizeInMB !== null) {
          // Convert MB to GB
          this.usedGB = +(res.sizeInMB / 1024).toFixed(2);
          this.percentUsed = Math.min(100, (this.usedGB / this.totalGB) * 100);
          this.updateStorageBar();
          console.log("Initialized, space used response received from backend");
          // Store in localStorage
          localStorage.setItem('lastSizePercentage', this.percentUsed.toString());
          localStorage.setItem('lastSizeInGb', this.usedGB.toString());
        }
      },
      error: (err) => {
        console.error('Failed to fetch user space used:', err);
        // Fallback to localStorage if available
        this.loadFromLocalStorage();
      }
    });

    // Setup SSE for real-time updates
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
              this.updateStorageBar();
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

  private updateStorageBar(): void {
    if (this.bg?.nativeElement) {
      this.bg.nativeElement.style.width = this.percentUsed + "%";
    }
  }

  private loadFromLocalStorage(): void {
    const lastSizePercentage = localStorage.getItem('lastSizePercentage');
    const lastSizeInGb = localStorage.getItem('lastSizeInGb');
    if (lastSizePercentage && lastSizeInGb) {
      this.percentUsed = +lastSizePercentage;
      this.usedGB = +lastSizeInGb;
      this.updateStorageBar();
    }
  }
}
