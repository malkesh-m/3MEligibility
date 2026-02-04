import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-spinner',
  standalone: false,

  templateUrl: './spinner.component.html',
  styleUrl: './spinner.component.scss'
})
export class SpinnerComponent {
  @Input() isDownloading: boolean = false;
  @Input() isLoading: boolean = true; // Show loader on page load
  @Input() isUploading: boolean = false;
  @Input() message: string = "Loading data, please wait...";
  @Input() isLocal: boolean = false;

}
