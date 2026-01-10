import { Component, Input } from '@angular/core';

export interface Item {
  id: number;
  name: string;
  route: string;
}

@Component({
  selector: 'app-tabination',
  standalone: false,

  templateUrl: './tabination.component.html',
  styleUrl: './tabination.component.scss'
})
export class TabinationComponent {
  @Input() listOfTabs: Item[] = [];

}
