import { Component, inject, Signal } from '@angular/core';
import { MakerCheckerService } from '../../../core/services/setting/makerchecker.service';

@Component({
  selector: 'app-exception-config',
  standalone: false,

  templateUrl: './exception-config.component.html',
  styleUrl: './exception-config.component.scss'
})
export class ExceptionConfigComponent {
  private service = inject(MakerCheckerService);

  // Signal that holds the exception state
  isExceptionEnabled: Signal<boolean> = this.service.isMakerCheckerEnabled;

  constructor() { }

  ngOnInit(): void {
    this.service.getMakerCheckerConfig(); //  Fetch the latest setting when the component loads
  }

  toggleException(event: any): void {
    const isEnabled = event.checked;

    // Update the exception config via service
    this.service.updateMakerCheckerConfig(isEnabled).subscribe({
      next: () => {
        // Directly update the signal to reflect the new value
        // this.service.setMakerCheckerEnabled(isEnabled); // If the service has a method to directly update the signal

        console.log(`Exception config updated successfully. Enabled: ${isEnabled}`);
      },
      error: (error) => {
        console.error('Error updating Exception config:', error);
      }
    });
  }
}
