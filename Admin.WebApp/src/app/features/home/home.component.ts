// src/app/features/home/home.component.ts
import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Store } from '@ngrx/store';
import { Subject, takeUntil } from 'rxjs';
import { ProductService } from '../../core/services/product.service';

interface QuickAction {
  title: string;
  icon: string;
  description: string;
  route: string;
  color: string;
}

interface DashboardMetric {
  title: string;
  value: number | string;
  icon: string;
  change?: {
    value: number;
    isPositive: boolean;
    label: string;
  };
  color: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatTooltipModule
  ],
  template: `
    <div class="container mx-auto p-4 md:p-6">
      <!-- Dashboard Header -->
      <div class="flex flex-col md:flex-row justify-between items-start md:items-center mb-6">
        <div>
          <h1 class="text-2xl font-bold text-slate-900 dark:text-white">Dashboard</h1>
          <p class="text-sm text-slate-500 dark:text-slate-400">Welcome back to your admin dashboard</p>
        </div>
        
        <div class="mt-4 md:mt-0 flex items-center space-x-2">
          <button 
            class="px-3 py-2 bg-white dark:bg-slate-800 text-slate-600 dark:text-slate-300 border border-slate-200 dark:border-slate-700 rounded-md shadow-sm flex items-center hover:bg-slate-50 dark:hover:bg-slate-750 transition-colors">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
            </svg>
            Export
          </button>
          
          <button 
            class="px-3 py-2 bg-primary-600 text-white rounded-md shadow-sm flex items-center hover:bg-primary-700 transition-colors">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
            </svg>
            New Report
          </button>
        </div>
      </div>
      
      <!-- Metrics Overview -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <div *ngFor="let metric of metrics" 
          class="bg-white dark:bg-slate-800 rounded-lg shadow-sm p-5 border border-slate-200 dark:border-slate-700">
          <div class="flex justify-between items-start">
            <div>
              <p class="text-sm font-medium text-slate-500 dark:text-slate-400">{{ metric.title }}</p>
              <p class="text-2xl font-bold text-slate-900 dark:text-white mt-1">{{ metric.value }}</p>
              
              <div *ngIf="metric.change" class="flex items-center mt-2">
                <span 
                  [class.text-emerald-600]="metric.change.isPositive"
                  [class.text-rose-600]="!metric.change.isPositive"
                  [class.dark:text-emerald-400]="metric.change.isPositive"
                  [class.dark:text-rose-400]="!metric.change.isPositive"
                  class="text-xs font-medium flex items-center">
                  
                  <svg *ngIf="metric.change.isPositive" xmlns="http://www.w3.org/2000/svg" class="h-3 w-3 mr-1" viewBox="0 0 20 20" fill="currentColor">
                    <path fill-rule="evenodd" d="M12 7a1 1 0 110-2h5a1 1 0 011 1v5a1 1 0 11-2 0V8.414l-4.293 4.293a1 1 0 01-1.414 0L8 10.414l-4.293 4.293a1 1 0 01-1.414-1.414l5-5a1 1 0 011.414 0L11 10.586 14.586 7H12z" clip-rule="evenodd" />
                  </svg>
                  
                  <svg *ngIf="!metric.change.isPositive" xmlns="http://www.w3.org/2000/svg" class="h-3 w-3 mr-1" viewBox="0 0 20 20" fill="currentColor">
                    <path fill-rule="evenodd" d="M12 13a1 1 0 100 2h5a1 1 0 001-1v-5a1 1 0 10-2 0v2.586l-4.293-4.293a1 1 0 00-1.414 0L8 9.586 3.707 5.293a1 1 0 00-1.414 1.414l5 5a1 1 0 001.414 0L11 9.414 14.586 13H12z" clip-rule="evenodd" />
                  </svg>
                  
                  {{ metric.change.value }}% {{ metric.change.label }}
                </span>
              </div>
            </div>
            
            <div [class]="'p-3 rounded-full ' + metric.color">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" [innerHTML]="getSvgPath(metric.icon)"></svg>
            </div>
          </div>
        </div>
      </div>
      
      <!-- Quick Actions -->
      <h2 class="text-lg font-semibold text-slate-900 dark:text-white mb-4">Quick Actions</h2>
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4 mb-8">
        <div *ngFor="let action of quickActions" 
            [routerLink]="action.route"
            class="bg-white dark:bg-slate-800 rounded-lg shadow-sm border border-slate-200 dark:border-slate-700 overflow-hidden hover:shadow-md transition-shadow cursor-pointer group">
          <div class="p-5">
            <div class="flex items-center mb-4">
              <div [class]="'p-3 rounded-full ' + action.color">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" [innerHTML]="getSvgPath(action.icon)"></svg>
              </div>
              <h3 class="ml-3 text-base font-medium text-slate-900 dark:text-white">{{ action.title }}</h3>
            </div>
            <p class="text-sm text-slate-500 dark:text-slate-400">{{ action.description }}</p>
          </div>
          <div class="px-5 py-3 bg-slate-50 dark:bg-slate-750 border-t border-slate-200 dark:border-slate-700">
            <span class="text-sm font-medium text-primary-600 dark:text-primary-400 flex items-center group-hover:translate-x-0.5 transition-transform">
              Get Started
              <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 ml-1" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M12.293 5.293a1 1 0 011.414 0l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414-1.414L14.586 11H3a1 1 0 110-2h11.586l-2.293-2.293a1 1 0 010-1.414z" clip-rule="evenodd" />
              </svg>
            </span>
          </div>
        </div>
      </div>
      
      <!-- Recent Activity -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Recent Orders -->
        <div class="bg-white dark:bg-slate-800 rounded-lg shadow-sm border border-slate-200 dark:border-slate-700 overflow-hidden">
          <div class="px-5 py-4 border-b border-slate-200 dark:border-slate-700 flex justify-between items-center">
            <h3 class="font-medium text-slate-900 dark:text-white">Recent Orders</h3>
            <a routerLink="/orders" class="text-sm font-medium text-primary-600 dark:text-primary-400 hover:text-primary-700 dark:hover:text-primary-300">
              View All
            </a>
          </div>
          
          <div class="p-5">
            <div *ngIf="recentOrders.length === 0" class="text-center py-6">
              <p class="text-slate-500 dark:text-slate-400">No recent orders found</p>
            </div>
            
            <div *ngFor="let order of recentOrders" class="border-b border-slate-200 dark:border-slate-700 last:border-b-0 py-3">
              <div class="flex justify-between items-center">
                <div>
                  <p class="text-sm font-medium text-slate-900 dark:text-white">#{{ order.id }}</p>
                  <p class="text-xs text-slate-500 dark:text-slate-400">{{ order.date }}</p>
                </div>
                <div>
                  <span [class]="getOrderStatusClass(order.status)">
                    {{ order.status }}
                  </span>
                </div>
                <div class="text-right">
                  <p class="text-sm font-medium text-slate-900 dark:text-white">{{ order.amount }}</p>
                  <p class="text-xs text-slate-500 dark:text-slate-400">{{ order.items }} items</p>
                </div>
              </div>
            </div>
          </div>
        </div>
        
        <!-- Low Stock Items -->
        <div class="bg-white dark:bg-slate-800 rounded-lg shadow-sm border border-slate-200 dark:border-slate-700 overflow-hidden">
          <div class="px-5 py-4 border-b border-slate-200 dark:border-slate-700 flex justify-between items-center">
            <h3 class="font-medium text-slate-900 dark:text-white">Low Stock Items</h3>
            <a routerLink="/products/list" class="text-sm font-medium text-primary-600 dark:text-primary-400 hover:text-primary-700 dark:hover:text-primary-300">
              View All
            </a>
          </div>
          
          <div class="p-5">
            <div *ngIf="lowStockItems.length === 0" class="text-center py-6">
              <p class="text-slate-500 dark:text-slate-400">No low stock items found</p>
            </div>
            
            <div *ngFor="let item of lowStockItems" class="border-b border-slate-200 dark:border-slate-700 last:border-b-0 py-3">
              <div class="flex items-center">
                <div class="h-10 w-10 bg-slate-100 dark:bg-slate-700 rounded-md flex items-center justify-center">
                  <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 text-slate-500 dark:text-slate-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
                  </svg>
                </div>
                <div class="ml-3 flex-grow">
                  <p class="text-sm font-medium text-slate-900 dark:text-white">{{ item.name }}</p>
                  <p class="text-xs text-slate-500 dark:text-slate-400">{{ item.sku }}</p>
                </div>
                <div class="text-right">
                  <p class="text-sm font-medium text-amber-600 dark:text-amber-400">{{ item.stock }} left</p>
                  <p class="text-xs text-slate-500 dark:text-slate-400">Threshold: {{ item.threshold }}</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: []
})
export class HomeComponent implements OnInit, OnDestroy {
  metrics: DashboardMetric[] = [
    {
      title: 'Total Products',
      value: 0,
      icon: 'M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4',
      color: 'bg-primary-600'
    },
    {
      title: 'Active Products',
      value: 0,
      icon: 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z',
      color: 'bg-emerald-600'
    },
    {
      title: 'Revenue',
      value: '$0',
      icon: 'M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z',
      color: 'bg-amber-600',
      change: {
        value: 0,
        isPositive: true,
        label: 'vs last month'
      }
    },
    {
      title: 'Orders',
      value: 0,
      icon: 'M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z',
      color: 'bg-purple-600',
      change: {
        value: 0,
        isPositive: false,
        label: 'vs last month'
      }
    }
  ];

  quickActions: QuickAction[] = [
    {
      title: 'Add Product',
      icon: 'M12 6v6m0 0v6m0-6h6m-6 0H6',
      description: 'Create a new product listing with details and imagery',
      route: '/products/add',
      color: 'bg-primary-600'
    },
    {
      title: 'Manage Orders',
      icon: 'M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z',
      description: 'View and manage customer orders and shipments',
      route: '/orders',
      color: 'bg-amber-600'
    },
    {
      title: 'View Statistics',
      icon: 'M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z',
      description: 'Check your store performance and key metrics',
      route: '/statistics',
      color: 'bg-emerald-600'
    },
    {
      title: 'Manage Categories',
      icon: 'M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z',
      description: 'Organize products with categories and tags',
      route: '/categories',
      color: 'bg-purple-600'
    }
  ];

  recentOrders: any[] = [];
  lowStockItems: any[] = [];

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly store: Store,
    private readonly productService: ProductService
  ) { }

  ngOnInit(): void {
    // Load dashboard data
    this.loadDashboardData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadDashboardData(): void {
    this.productService.getStats().pipe(
      takeUntil(this.destroy$)
    ).subscribe(stats => {
      // Update metrics with real data
      this.updateMetrics(stats);

      // Create sample recent orders for demo
      this.recentOrders = this.createSampleOrders();

      // Create sample low stock items for demo
      this.lowStockItems = this.createSampleLowStockItems();
    });
  }

  private updateMetrics(stats: any): void {
    this.metrics[0].value = stats.totalProducts || 0;
    this.metrics[1].value = stats.activeProductCount || 0;
    this.metrics[2].value = `$${(stats.totalRevenue || 0).toLocaleString()}`;
    this.metrics[2].change = {
      value: stats.revenueChange || 8.2,
      isPositive: (stats.revenueChange || 8.2) >= 0,
      label: 'vs last month'
    };
    this.metrics[3].value = stats.totalOrders || 0;
    this.metrics[3].change = {
      value: stats.ordersChange || -2.5,
      isPositive: (stats.ordersChange || -2.5) >= 0,
      label: 'vs last month'
    };
  }

  private createSampleOrders(): any[] {
    return [
      { id: '10042', date: '2 hours ago', status: 'Completed', amount: '$245.99', items: 3 },
      { id: '10041', date: '5 hours ago', status: 'Processing', amount: '$129.50', items: 2 },
      { id: '10040', date: 'Yesterday', status: 'Shipped', amount: '$89.99', items: 1 },
      { id: '10039', date: 'Yesterday', status: 'Completed', amount: '$432.25', items: 5 }
    ];
  }

  private createSampleLowStockItems(): any[] {
    return [
      { name: 'Wireless Headphones', sku: 'WH-12345', stock: 3, threshold: 5 },
      { name: 'Smartphone Case', sku: 'SC-54321', stock: 2, threshold: 10 },
      { name: 'Smart Watch', sku: 'SW-98765', stock: 4, threshold: 8 },
      { name: 'Bluetooth Speaker', sku: 'BS-45678', stock: 1, threshold: 5 }
    ];
  }

  getOrderStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'completed':
        return 'px-2 py-1 text-xs rounded-full bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-400';
      case 'processing':
        return 'px-2 py-1 text-xs rounded-full bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-400';
      case 'shipped':
        return 'px-2 py-1 text-xs rounded-full bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-400';
      case 'cancelled':
        return 'px-2 py-1 text-xs rounded-full bg-rose-100 dark:bg-rose-900/30 text-rose-700 dark:text-rose-400';
      default:
        return 'px-2 py-1 text-xs rounded-full bg-slate-100 dark:bg-slate-700 text-slate-700 dark:text-slate-400';
    }
  }

  getSvgPath(path: string): string {
    return `<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="${path}" />`;
  }
}