import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-spinner',
  standalone: false,

  templateUrl: './spinner.component.html',
  styleUrl: './spinner.component.scss'
})
export class SpinnerComponent {
  @Input() isDownloading: boolean = false;
  @Input() isLoading: boolean = false;
  @Input() isUploading: boolean = false;
  @Input() message: string = "Loading...";
  @Input() isLocal: boolean = false;

}
