import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ThemeService } from '../../core/services/theme.service';
import { MatSidenavModule } from '@angular/material/sidenav';
import { AuthService } from 'src/app/core/services/auth.service';
import { User } from 'src/app/shared/models/auth.models';

interface NavItem {
  isFirstLevel?: boolean;
  path: string;
  icon: string;
  label: string;
  children?: NavItem[];
}
@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule, MatSidenavModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})

export class SidebarComponent implements OnInit {
  isCollapsed = false;
  isDarkTheme = true;
  logo = '../../../assets/dashboardadmin.png';
  openSubmenus = new Set<string>();
  currentUser: User | null = null;
  isUserMenuOpen = false;

  navItems: NavItem[] = [
    {
      path: '/products',
      icon: 'bi-box',
      label: 'Products',
      children: [
        {
          path: '/products/list',
          icon: 'bi-list-ul',
          label: 'List Products'
        },
        {
          path: '/products/add',
          icon: 'bi-plus-circle',
          label: 'Add Product'
        },
      ]
    },
    {
      path: '/categories',
      icon: 'bi-tags',
      label: 'Categories',
    },
    {
      path: '/statistics',
      icon: 'bi-graph-up',
      label: 'Statistics'
    },
    {
      path: '/dashboards',
      icon: 'bi-columns-gap',
      label: 'Dashboards',
      children: [
        {
          path: '/dashboards/stockalerts',
          icon: 'bi-bell',
          label: 'Stock Alerts'
        }
      ]
    },
    {
      path: '/orders',
      icon: 'bi-cart',
      label: 'Orders',
      children: [
        {
          path: '/orders',
          icon: 'bi-list-ul',
          label: 'All Orders'
        }
      ]
    },
  ];

  // Flattened navigation for mobile
  get mobileNavItems(): NavItem[] {
    return this.navItems.reduce((acc: NavItem[], item) => {
      if (item.children) {
        return [...acc, ...item.children];
      }
      return [...acc, item];
    }, []);
  }

  constructor(
    private readonly themeService: ThemeService,
    private readonly authService: AuthService
  ) {
    this.isDarkTheme = this.themeService.isDarkTheme();
  }

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      console.log('Received currentUser:', user); // Debug log
      this.currentUser = user;
    });

    // Ensure the current user is set if already authenticated
    this.authService.isAuthenticated$.subscribe(isAuthenticated => {
      if (isAuthenticated) {
        this.authService.currentUser$.subscribe(user => {
          this.currentUser = user;
        });
      }
    });

    this.themeService.theme$.subscribe((isDark: boolean) => {
      this.isDarkTheme = isDark;
      // Update document theme for Material
      document.documentElement.setAttribute('data-theme', isDark ? 'dark' : 'light');
    });
  }

  toggleSidebar() {
    this.isCollapsed = !this.isCollapsed;
    if (this.isCollapsed) {
      this.openSubmenus.clear();
    }
  }

  toggleTheme() {
    this.isDarkTheme = !this.isDarkTheme;
    // this.themeService.setTheme(this.isDarkTheme ? 'dark' : 'light');
    this.themeService.setTheme(this.isDarkTheme);

  }

  toggleSubmenu(item: NavItem) {
    if (this.isSubmenuOpen(item)) {
      this.openSubmenus.delete(item.path);
    } else {
      this.openSubmenus.add(item.path);
    }
  }

  isSubmenuOpen(item: NavItem): boolean {
    return this.openSubmenus.has(item.path);
  }

  isChildActive(item: NavItem): boolean {
    return item.children?.some(child =>
      window.location.pathname.startsWith(child.path)
    ) ?? false;
  }

  toggleUserMenu() {
    this.isUserMenuOpen = !this.isUserMenuOpen;
  }

  logout() {
    this.authService.logout();
  }

  trackByPath(index: number, item: NavItem): string {
    return item.path;
  }
}
