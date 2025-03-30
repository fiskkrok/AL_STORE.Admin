// src/app/layout/sidebar/sidebar.component.ts
import { Component, OnInit, OnDestroy, Input, ViewEncapsulation, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { ThemeService } from '../../core/services/theme.service';
import { AuthService } from 'src/app/core/services/auth.service';
import { User } from 'src/app/shared/models/auth.models';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Subject, takeUntil, filter } from 'rxjs';

interface NavItem {
  path: string;
  icon: string;
  label: string;
  children?: NavItem[];
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatIconModule,
    MatTooltipModule
  ],
  template: `
    <aside 
      class="h-full w-[280px] bg-white dark:bg-slate-800 border-r border-slate-200 dark:border-slate-700 flex flex-col"
      [class.w-[280px]]="!collapsed"
      [class.w-[70px]]="collapsed">
      
      <!-- Sidebar Header -->
      <div class="flex items-center justify-between h-16 px-4 border-b border-slate-200 dark:border-slate-700">
        <!-- Logo -->
        <div class="flex items-center">
          <div class="flex items-center justify-center w-10 h-10 rounded-lg bg-primary-600 text-white">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
            </svg>
          </div>
          <h1 *ngIf="!collapsed" class="ml-3 text-lg font-semibold text-slate-900 dark:text-white">Admin</h1>
        </div>
        
        <!-- Collapse Button -->
        <button 
          (click)="toggleCollapse()" 
          class="p-1.5 rounded-full text-slate-500 hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors"
          [matTooltip]="collapsed ? 'Expand' : 'Collapse'">
          <svg 
            xmlns="http://www.w3.org/2000/svg" 
            class="h-5 w-5" 
            viewBox="0 0 20 20" 
            fill="currentColor" 
            [class.rotate-180]="collapsed">
            <path fill-rule="evenodd" d="M12.707 5.293a1 1 0 010 1.414L9.414 10l3.293 3.293a1 1 0 01-1.414 1.414l-4-4a1 1 0 010-1.414l4-4a1 1 0 011.414 0z" clip-rule="evenodd" />
          </svg>
        </button>
      </div>
      
      <!-- Navigation -->
      <nav class="flex-1 overflow-y-auto py-4 px-3">
        <!-- Main Navigation Section -->
        <div class="mb-6">
          <div *ngIf="!collapsed" class="px-4 mb-2">
            <h2 class="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider">
              Main
            </h2>
          </div>
          
          <!-- Dashboard Link -->
          <a 
            routerLink="/" 
            routerLinkActive="bg-primary-50 dark:bg-slate-700 text-primary-700 dark:text-primary-400" 
            [routerLinkActiveOptions]="{exact: true}"
            class="flex items-center px-3 py-2 rounded-md text-slate-700 dark:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors mb-1 cursor-pointer"
            [class.justify-center]="collapsed">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
            </svg>
            <span *ngIf="!collapsed" class="ml-3">Dashboard</span>
          </a>
          
          <!-- Products Section -->
          <div class="mb-1">
            <!-- Products Dropdown Toggle -->
            <div 
              (click)="toggleGroup('products')"
              class="flex items-center px-3 py-2 rounded-md text-slate-700 dark:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors cursor-pointer"
              [class.justify-center]="collapsed"
              [class.bg-primary-50]="expandedGroups.has('products') && !collapsed"
              [class.dark:bg-slate-700]="expandedGroups.has('products') && !collapsed"
              [class.text-primary-700]="expandedGroups.has('products') && !collapsed"
              [class.dark:text-primary-400]="expandedGroups.has('products') && !collapsed">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
              </svg>
              <span *ngIf="!collapsed" class="ml-3 flex-1">Products</span>
              <svg *ngIf="!collapsed" xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 transition-transform duration-200 ease-in-out" [class.rotate-90]="expandedGroups.has('products')" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
              </svg>
            </div>
            
            <!-- Products Submenu -->
            <div *ngIf="expandedGroups.has('products') && !collapsed" class="mt-1 ml-4 space-y-1">
              <a 
                routerLink="/products/list" 
                routerLinkActive="bg-primary-50 dark:bg-slate-700 text-primary-700 dark:text-primary-400" 
                class="flex items-center px-3 py-2 rounded-md text-slate-700 dark:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors text-sm">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 10h16M4 14h16M4 18h16" />
                </svg>
                <span class="ml-3">Product List</span>
              </a>
              <a 
                routerLink="/products/add" 
                routerLinkActive="bg-primary-50 dark:bg-slate-700 text-primary-700 dark:text-primary-400" 
                class="flex items-center px-3 py-2 rounded-md text-slate-700 dark:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors text-sm">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                </svg>
                <span class="ml-3">Add Product</span>
              </a>
              <a 
                routerLink="/categories" 
                routerLinkActive="bg-primary-50 dark:bg-slate-700 text-primary-700 dark:text-primary-400" 
                class="flex items-center px-3 py-2 rounded-md text-slate-700 dark:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors text-sm">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" />
                </svg>
                <span class="ml-3">Categories</span>
              </a>
            </div>
          </div>
          
          <!-- Stats Link -->
          <a 
            routerLink="/statistics" 
            routerLinkActive="bg-primary-50 dark:bg-slate-700 text-primary-700 dark:text-primary-400" 
            class="flex items-center px-3 py-2 rounded-md text-slate-700 dark:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors mb-1 cursor-pointer"
            [class.justify-center]="collapsed">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
            </svg>
            <span *ngIf="!collapsed" class="ml-3">Statistics</span>
          </a>
          
          <!-- Orders Link -->
          <a 
            routerLink="/orders" 
            routerLinkActive="bg-primary-50 dark:bg-slate-700 text-primary-700 dark:text-primary-400" 
            class="flex items-center px-3 py-2 rounded-md text-slate-700 dark:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors mb-1 cursor-pointer"
            [class.justify-center]="collapsed">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
            </svg>
            <span *ngIf="!collapsed" class="ml-3">Orders</span>
            <span 
              *ngIf="newOrdersCount > 0" 
              class="ml-auto inline-flex items-center justify-center px-2 py-1 text-xs font-bold leading-none text-white bg-red-500 rounded-full">
              {{ newOrdersCount }}
            </span>
          </a>
        </div>
        
        <!-- System Section -->
        <div>
          <div *ngIf="!collapsed" class="px-4 mb-2">
            <h2 class="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider">
              System
            </h2>
          </div>
          
          <!-- Settings Link -->
          <a 
            routerLink="/settings" 
            routerLinkActive="bg-primary-50 dark:bg-slate-700 text-primary-700 dark:text-primary-400" 
            class="flex items-center px-3 py-2 rounded-md text-slate-700 dark:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors mb-1 cursor-pointer"
            [class.justify-center]="collapsed">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
            <span *ngIf="!collapsed" class="ml-3">Settings</span>
          </a>
          
          <!-- Users Link -->
          <a 
            routerLink="/users" 
            routerLinkActive="bg-primary-50 dark:bg-slate-700 text-primary-700 dark:text-primary-400" 
            class="flex items-center px-3 py-2 rounded-md text-slate-700 dark:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors mb-1 cursor-pointer"
            [class.justify-center]="collapsed">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
            </svg>
            <span *ngIf="!collapsed" class="ml-3">Users</span>
          </a>
        </div>
      </nav>
      
      <!-- User Profile & Theme Toggle -->
      <div class="border-t border-slate-200 dark:border-slate-700 p-4">
        <!-- User Profile -->
        <div *ngIf="!collapsed" class="flex items-center mb-4">
          <div class="flex-shrink-0">
            <div class="w-10 h-10 rounded-full bg-primary-100 dark:bg-slate-700 flex items-center justify-center text-primary-700 dark:text-primary-400">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
              </svg>
            </div>
          </div>
          <div class="ml-3 flex-1">
            <p class="text-sm font-medium text-slate-900 dark:text-white">
              {{ currentUser()?.username || 'Guest User' }}
            </p>
            <p class="text-xs text-slate-500 dark:text-slate-400">
              {{ currentUser()?.roles || 'No role assigned' }}
            </p>
          </div>
          <button 
            (click)="toggleUserMenu()"
            class="p-1 rounded-full text-slate-400 hover:text-slate-500 dark:hover:text-slate-300">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z" clip-rule="evenodd" />
            </svg>
          </button>
        </div>
        
        <!-- User Menu Dropdown -->
        <div *ngIf="isUserMenuOpen && !collapsed" class="mt-2 mb-4 py-1 bg-white dark:bg-slate-750 rounded-md shadow-lg border border-slate-200 dark:border-slate-700">
          <a 
            routerLink="/profile" 
            class="flex items-center px-4 py-2 text-sm text-slate-700 dark:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-700">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-3" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
            My Profile
          </a>
          <a 
            (click)="logout()"
            class="flex items-center px-4 py-2 text-sm text-slate-700 dark:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-700 cursor-pointer">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-3" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
            </svg>
            Logout
          </a>
        </div>
        
        <!-- Theme Toggle -->
        <div [class.flex]="!collapsed" [class.justify-center]="collapsed" class="items-center">
          <button 
            (click)="toggleTheme()"
            class="p-2 rounded-md text-slate-500 hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors w-full flex items-center justify-center sm:justify-start">
            <svg *ngIf="isDarkTheme" xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" />
            </svg>
            <svg *ngIf="!isDarkTheme" xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
            </svg>
            <span *ngIf="!collapsed" class="ml-3">{{ isDarkTheme ? 'Light Mode' : 'Dark Mode' }}</span>
          </button>
        </div>
      </div>
    </aside>
  `,
  encapsulation: ViewEncapsulation.None,
  host: {
    '[class.dark]': 'isDarkTheme',
    '[class.collapsed]': 'collapsed'
  }
})
export class SidebarComponent implements OnInit, OnDestroy {
  @Input() collapsed = false;
  currentUser = signal<User | null>(null);
  isDarkTheme = false;
  sidebarVisible = true;
  expandedGroups: Set<string> = new Set();
  newOrdersCount = 0; // Example count, replace with actual logic
  private destroy$ = new Subject<void>();
  isUserMenuOpen = false;

  constructor(
    private themeService: ThemeService,
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.isDarkTheme = this.themeService.isDarkTheme();
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.currentUser.set(user);
      });
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.sidebarVisible = true; // Show sidebar on navigation
    });
  }

  toggleCollapse(): void {
    this.collapsed = !this.collapsed;
  }

  toggleGroup(group: string): void {
    if (this.expandedGroups.has(group)) {
      this.expandedGroups.delete(group);
    } else {
      this.expandedGroups.add(group);
    }
  }
  toggleUserMenu(): void {
    this.isUserMenuOpen = !this.isUserMenuOpen;
  }
  toggleTheme(): void {
    this.isDarkTheme = !this.isDarkTheme;
    this.themeService.toggleTheme();
  }
  logout(): void {
    this.authService.logout().then(() => { // Ensure logout is awaited
      this.router.navigate(['/login']); // Redirect to login page after logout
    });
  }
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
