// src/app/layout/main-layout/main-layout.component.ts
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { ErrorToastComponent } from '../../shared/components/error-toast/error-toast.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';
import { DialogComponent } from "../../shared/components/dialog/dialog.component";

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [
    CommonModule,
    SidebarComponent,
    ErrorToastComponent,
    LoadingSpinnerComponent,
    DialogComponent
  ],
  template: `
    <div class="layout-container" [attr.data-theme]="isDarkTheme ? 'dark' : 'light'">
      <app-sidebar></app-sidebar>
      <main class="main-content" role="main">
        <ng-content></ng-content>
      </main>
      <app-error-toast></app-error-toast>
      <app-loading-spinner></app-loading-spinner>
            <app-dialog></app-dialog>
    </div>
  `,
  styleUrls: ['./main-layout.component.scss']
})
export class MainLayoutComponent {
  isDarkTheme = true;
}