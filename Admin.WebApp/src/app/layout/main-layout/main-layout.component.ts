// src/app/layout/main-layout/main-layout.component.ts
import { Component, OnInit, inject, effect, ViewEncapsulation } from '@angular/core';
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
    <div class="h-screen flex flex-col bg-slate-50 dark:bg-slate-900 text-slate-900 dark:text-white">
      <!-- Top Navigation Bar - Mobile Only -->
      <header class="lg:hidden bg-white dark:bg-slate-800 border-b border-slate-200 dark:border-slate-700 shadow-sm z-10">
        <div class="flex justify-between items-center px-4 py-2">
          <div class="flex items-center">
            <button 
              (click)="toggleSidebar()" 
              class="p-2 rounded-full text-slate-500 hover:bg-slate-100 dark:hover:bg-slate-700">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
              </svg>
            </button>
            <span class="ml-2 text-lg font-medium">Admin Dashboard</span>
          </div>
          <div class="flex items-center space-x-2">
            <button 
              (click)="themeService.toggleTheme()" 
              class="p-2 rounded-full text-slate-500 hover:bg-slate-100 dark:hover:bg-slate-700">
              <svg *ngIf="isDarkTheme" xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" />
              </svg>
              <svg *ngIf="!isDarkTheme" xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
              </svg>
            </button>
          </div>
        </div>
      </header>

      <!-- Main Content -->
      <div class="flex flex-1 overflow-hidden">
        <!-- Sidebar - Hidden on mobile until toggled -->
        <app-sidebar [collapsed]="sidebarCollapsed" [ngClass]="{'hidden': !sidebarVisible && !isDesktop}"></app-sidebar>
        
        <!-- Main Content Area -->
        <main class="flex-1 overflow-auto">
          <div class="mx-auto p-4 md:p-6">
            <ng-content></ng-content>
          </div>
        </main>
      </div>

      <!-- Mobile Navigation Footer -->
      <footer class="lg:hidden bg-white dark:bg-slate-800 border-t border-slate-200 dark:border-slate-700 shadow-sm">
        <div class="grid grid-cols-4 h-16">
          <a routerLink="/" class="flex flex-col items-center justify-center text-primary-600 dark:text-primary-400">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
            </svg>
            <span class="text-xs mt-1">Home</span>
          </a>
          <a routerLink="/products" class="flex flex-col items-center justify-center text-slate-600 dark:text-slate-400">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
            </svg>
            <span class="text-xs mt-1">Products</span>
          </a>
          <a routerLink="/statistics" class="flex flex-col items-center justify-center text-slate-600 dark:text-slate-400">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
            </svg>
            <span class="text-xs mt-1">Stats</span>
          </a>
          <a routerLink="/settings" class="flex flex-col items-center justify-center text-slate-600 dark:text-slate-400">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
            <span class="text-xs mt-1">Settings</span>
          </a>
        </div>
      </footer>

      <!-- Toast Messages and Dialogs -->
      <app-error-toast></app-error-toast>
      <app-global-loading></app-global-loading>
      <app-dialog></app-dialog>
    </div>
  `,
  styles: [],
  encapsulation: ViewEncapsulation.None
})
export class MainLayoutComponent implements OnInit {
  readonly themeService = inject(ThemeService);

  isDarkTheme = false;
  sidebarCollapsed = false;
  sidebarVisible = false;
  isDesktop = false;

  constructor() {
    // Listen to theme changes
    effect(() => {
      this.isDarkTheme = this.themeService.isDarkTheme();
    });
  }

  ngOnInit() {
    // Check screen size
    this.checkScreenSize();
    window.addEventListener('resize', () => this.checkScreenSize());

    // Default to showing sidebar on desktop
    this.sidebarVisible = this.isDesktop;
  }

  toggleSidebar() {
    this.sidebarVisible = !this.sidebarVisible;
  }

  private checkScreenSize() {
    this.isDesktop = window.innerWidth >= 1024; // lg breakpoint in Tailwind
    if (this.isDesktop) {
      this.sidebarVisible = true;
    }
  }
}