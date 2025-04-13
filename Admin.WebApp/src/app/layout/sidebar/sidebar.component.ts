// src/app/layout/sidebar/sidebar.component.ts
import { Component, OnInit, OnDestroy, Input, ViewEncapsulation, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { ThemeService } from '../../core/services/theme.service';
import { AuthService } from 'src/app/core/services/auth.service';
import { User } from 'src/app/shared/models/auth.models';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Subject, takeUntil, filter } from 'rxjs';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatIconModule,
    MatTooltipModule
  ],
  templateUrl: './sidebar.component.html',
  encapsulation: ViewEncapsulation.None,
  host: {
    '[class.dark]': 'isDarkTheme',
    '[class.collapsed]': 'collapsed'
  }
})
export class SidebarComponent implements OnInit, OnDestroy {
  @Input() collapsed = false;
  private readonly themeService = inject(ThemeService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroy$ = new Subject<void>();
  currentUser = signal<User | null>(null);
  isDarkTheme = false;
  sidebarVisible = true;
  expandedGroups: Set<string> = new Set();
  newOrdersCount = 0; // Example count, replace with actual logic
  isUserMenuOpen = false;

  constructor(

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
