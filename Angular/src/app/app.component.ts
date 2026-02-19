import { Component } from '@angular/core';
import { OidcAuthService } from './core/services/auth/oidc-auth.service';
import { AuthService } from './core/services/auth/auth.service';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { filter, map, mergeMap } from 'rxjs';
import { HeaderTitleService } from './core/services/header-title.service';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  standalone: false,
})
export class AppComponent {
  title = '3MEligibilityFrontend';
  constructor(
    private authService: AuthService,
    private oidcAuthService: OidcAuthService,
    private titleService: Title,
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private headerTitleService: HeaderTitleService,
    private translate: TranslateService
  ) {
    // Initialize translations on startup so translate.instant() works everywhere
    const savedLang = localStorage.getItem('appLanguage') || 'en';
    this.translate.setDefaultLang('en');
    this.translate.use(savedLang);
  }

  ngOnInit(): void {
    if (this.oidcAuthService.isAuthenticated()) {
      this.authService.loadUserPermissions();
    }
    this.router.events.pipe(
      filter((event: any) => event instanceof NavigationEnd),
      map(() => this.activatedRoute),
      map(route => {
        while (route.firstChild) route = route.firstChild;
        return route;
      }),
      mergeMap(route => route.data)
    ).subscribe(data => {
      const pageTitle = data['title']
        ? `${data['title']} - 3M Eligibility`
        : '3M Eligibility';

      this.titleService.setTitle(pageTitle);

      // Also update the header title
      if (data['title']) {
        this.headerTitleService.setTitle(data['title']);
      }
    });

  }
}

