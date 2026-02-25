import { Component } from '@angular/core';
import { LoadingService } from '../../services/utility/loading.service';

@Component({
  selector: 'app-layout',
  standalone: false,

  templateUrl: './layout.component.html',
  styleUrl: './layout.component.scss'
})
export class LayoutComponent {
  constructor(public loadingService: LoadingService) { }
  showFiller = true;
}
