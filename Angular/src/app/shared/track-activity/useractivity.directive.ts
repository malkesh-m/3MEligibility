import { Directive, Input, HostListener } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

@Directive({
  selector: '[appUseractivity]',
    standalone: false
  // NO standalone property at all - this makes it non-standalone by default
})
export class UseractivityDirective {
  @Input('appUseractivity') actionName!: string;
  @Input() componentName!: string;
  @Input() actionType!: string;

  private apiUrl = environment.apiUrl;

  constructor(
    private http: HttpClient,
    private router: Router
  ) { }

  private logActivity(eventType: string) {
    if (!this.actionName || !this.componentName) {
      console.warn('⚠️ UseractivityDirective: Missing actionName or componentName');
      return;
    }

    const payload = {
      actionType: this.actionType || eventType,
      actionName: this.actionName,
      componentName: this.componentName,
      pageUrl: this.router.url,
    //  timestamp: new Date()
    };

    this.http.post(`${this.apiUrl}/log/loguseractivity`, payload).subscribe({
      next: () => console.log('User activity logged:', payload),
      error: err => console.error(' Error logging activity:', err)
    });
  }

  @HostListener('click', ['$event'])
  onClick(event: Event) {
    console.log("Click HostListener")
      this.logActivity('ButtonClick');
    
  }

  @HostListener('submit', ['$event'])
  onSubmit(event: Event) {
    console.log("Submit HostListener")

    if ((event.target as HTMLElement).tagName === 'FORM') {
      this.logActivity('FormSubmit');
    }
  }
}
