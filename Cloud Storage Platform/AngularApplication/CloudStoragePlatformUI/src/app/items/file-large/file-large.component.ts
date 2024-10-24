import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'file-large-item',
  templateUrl: './file-large.component.html',
  styleUrl: './file-large.component.css'
})
export class FileLargeComponent implements OnInit {
  @Input() type: string = "";
  @Input() name: string = "";
  originalName="";

  ngOnInit(): void {
    this.originalName = this.name;
    if (this.name.length>=32){
      let portionToBeExcluded = this.name.slice(32, this.name.length);
      this.name = this.name.replace(portionToBeExcluded, "")+"...";
    }
  }
}
