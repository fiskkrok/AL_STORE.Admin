import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { ErrorToastComponent } from '../../shared/components/error-toast/error-toast.component';
import { DialogComponent } from "../../shared/components/dialog/dialog.component";
import { ThemeService } from 'src/app/core/services/theme.service';
import { GlobalLoadingComponent } from 'src/app/shared/components/global-loading/global-loading.component';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [
    CommonModule,
    SidebarComponent,
    ErrorToastComponent,
    DialogComponent,
    GlobalLoadingComponent,
  ],
  template: `
    <div class="layout-container" [attr.data-theme]="isDarkTheme ? 'dark' : 'light'">
      <app-sidebar></app-sidebar>
      <div class="main-content" role="main">
        <ng-content></ng-content>
      </div>
      <app-error-toast></app-error-toast>
    <app-global-loading></app-global-loading>
    <app-dialog></app-dialog>
  
    </div>
  `,
})
export class MainLayoutComponent implements OnInit {
  private readonly themeService = inject(ThemeService);
  isDarkTheme = true;

  ngOnInit() {
    this.themeService.theme$.subscribe((isDark: boolean) => {
      this.isDarkTheme = isDark;
      // Update document theme for Material
      document.documentElement.setAttribute('data-theme', isDark ? 'dark' : 'light');
    });
  }
}
