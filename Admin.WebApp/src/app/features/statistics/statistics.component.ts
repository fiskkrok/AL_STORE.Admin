// src/app/features/statistics/statistics.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { StatisticsService } from '../../core/services/statistics.service';

// Define Statistics interface
export interface Statistics {
    totalProducts: number;
    activeProductCount: number;
    lowStockCount: number;
    totalCategories: number;
    // Add other properties used in the component as needed
}

@Component({
    selector: 'app-statistics',
    standalone: true,
    imports: [
        CommonModule,
        MatCardModule,
        MatButtonModule,
        MatIconModule,
        MatSelectModule,
        MatMenuModule,
        MatTooltipModule,
        RouterModule,
        ReactiveFormsModule
    ],
    template: `
        <div class="container mx-auto p-4 md:p-6">
            <!-- Dashboard Header -->
            <div class="flex flex-col md:flex-row justify-between items-start md:items-center mb-6">
                <div>
                    <h1 class="text-2xl font-bold text-slate-900 dark:text-white">Statistics</h1>
                    <p class="text-sm text-slate-500 dark:text-slate-400">Track your store performance and insights</p>
                </div>
                
                <div class="flex flex-col sm:flex-row gap-3 mt-4 md:mt-0">
                    <div class="inline-flex rounded-md shadow-sm" role="group">
                        <button 
                            (click)="setTimeRange('day')"
                            [class.bg-primary-50]="timeRange === 'day'"
                            [class.text-primary-700]="timeRange === 'day'"
                            [class.dark:bg-slate-700]="timeRange === 'day'"
                            [class.dark:text-primary-400]="timeRange === 'day'"
                            class="px-4 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded-l-md hover:bg-slate-50 dark:bg-slate-800 dark:border-slate-600 dark:text-slate-300 dark:hover:bg-slate-700">
                            Today
                        </button>
                        <button 
                            (click)="setTimeRange('week')"
                            [class.bg-primary-50]="timeRange === 'week'"
                            [class.text-primary-700]="timeRange === 'week'"
                            [class.dark:bg-slate-700]="timeRange === 'week'"
                            [class.dark:text-primary-400]="timeRange === 'week'"
                            class="px-4 py-2 text-sm font-medium text-slate-700 bg-white border-t border-b border-slate-300 hover:bg-slate-50 dark:bg-slate-800 dark:border-slate-600 dark:text-slate-300 dark:hover:bg-slate-700">
                            Week
                        </button>
                        <button 
                            (click)="setTimeRange('month')"
                            [class.bg-primary-50]="timeRange === 'month'"
                            [class.text-primary-700]="timeRange === 'month'"
                            [class.dark:bg-slate-700]="timeRange === 'month'"
                            [class.dark:text-primary-400]="timeRange === 'month'"
                            class="px-4 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded-r-md hover:bg-slate-50 dark:bg-slate-800 dark:border-slate-600 dark:text-slate-300 dark:hover:bg-slate-700">
                            Month
                        </button>
                    </div>
                    
                    <button 
                        class="flex items-center px-4 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded-md shadow-sm hover:bg-slate-50 dark:bg-slate-800 dark:border-slate-600 dark:text-slate-300 dark:hover:bg-slate-700">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0l-4 4m4-4v12" />
                        </svg>
                        Export Data
                    </button>
                </div>
            </div>
            
            <!-- Stats Overview Cards -->
            <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
                <!-- Total Products -->
                <div class="bg-white dark:bg-slate-800 rounded-lg shadow-sm border border-slate-200 dark:border-slate-700 p-6">
                    <div class="flex justify-between">
                        <div>
                            <p class="text-sm font-medium text-slate-500 dark:text-slate-400">Total Products</p>
                            <p class="mt-2 text-3xl font-bold text-slate-900 dark:text-white">
                                {{ (stats$ | async)?.totalProducts || 0 }}
                            </p>
                            <div class="mt-2 flex items-center">
                                <p class="text-xs text-slate-500 dark:text-slate-400">From {{ categoryCount }} categories</p>
                            </div>
                        </div>
                        <div class="rounded-full bg-primary-100 dark:bg-primary-900/20 p-3 text-primary-700 dark:text-primary-400">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                                <path stroke-linecap="round" stroke-linejoin="round" d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
                            </svg>
                        </div>
                    </div>
                    <div class="mt-4">
                        <a routerLink="/products/list" class="text-sm font-medium text-primary-600 hover:text-primary-700 dark:text-primary-400 dark:hover:text-primary-300 inline-flex items-center">
                            View All Products
                            <svg xmlns="http://www.w3.org/2000/svg" class="ml-1 h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                            </svg>
                        </a>
                    </div>
                </div>
                
                <!-- Active Products -->
                <div class="bg-white dark:bg-slate-800 rounded-lg shadow-sm border border-slate-200 dark:border-slate-700 p-6">
                    <div class="flex justify-between">
                        <div>
                            <p class="text-sm font-medium text-slate-500 dark:text-slate-400">Active Products</p>
                            <p class="mt-2 text-3xl font-bold text-slate-900 dark:text-white">
                                {{ (stats$ | async)?.activeProductCount || 0 }}
                            </p>
                            <div class="mt-2 flex items-center">
                                <p class="text-xs text-slate-500 dark:text-slate-400">
                                    {{ getActivePercentage((stats$ | async)) }}% of total
                                </p>
                            </div>
                        </div>
                        <div class="rounded-full bg-emerald-100 dark:bg-emerald-900/20 p-3 text-emerald-700 dark:text-emerald-400">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                                <path stroke-linecap="round" stroke-linejoin="round" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                            </svg>
                        </div>
                    </div>
                    <div class="mt-4">
                        <a routerLink="/products/list" class="text-sm font-medium text-primary-600 hover:text-primary-700 dark:text-primary-400 dark:hover:text-primary-300 inline-flex items-center">
                            View Active Products
                            <svg xmlns="http://www.w3.org/2000/svg" class="ml-1 h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                            </svg>
                        </a>
                    </div>
                </div>
                
                <!-- Low Stock Items -->
                <div class="bg-white dark:bg-slate-800 rounded-lg shadow-sm border border-slate-200 dark:border-slate-700 p-6">
                    <div class="flex justify-between">
                        <div>
                            <p class="text-sm font-medium text-slate-500 dark:text-slate-400">Low Stock Items</p>
                            <p class="mt-2 text-3xl font-bold text-slate-900 dark:text-white">
                                {{ (stats$ | async)?.lowStockCount || 0 }}
                            </p>
                            <div class="mt-2 flex items-center">
                                <p *ngIf="(stats$ | async)?.lowStockCount" class="text-xs text-amber-600 dark:text-amber-400 flex items-center">
                                    <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                                    </svg>
                                    Needs attention
                                </p>
                                <p *ngIf="!(stats$ | async)?.lowStockCount" class="text-xs text-emerald-600 dark:text-emerald-400 flex items-center">
                                    <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                                    </svg>
                                    Stock levels good
                                </p>
                            </div>
                        </div>
                        <div class="rounded-full bg-amber-100 dark:bg-amber-900/20 p-3 text-amber-700 dark:text-amber-400">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                                <path stroke-linecap="round" stroke-linejoin="round" d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
                            </svg>
                        </div>
                    </div>
                    <div class="mt-4">
                        <a routerLink="/products/list" class="text-sm font-medium text-primary-600 hover:text-primary-700 dark:text-primary-400 dark:hover:text-primary-300 inline-flex items-center">
                            View Low Stock
                            <svg xmlns="http://www.w3.org/2000/svg" class="ml-1 h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                            </svg>
                        </a>
                    </div>
                </div>
                
                <!-- Categories -->
                <div class="bg-white dark:bg-slate-800 rounded-lg shadow-sm border border-slate-200 dark:border-slate-700 p-6">
                    <div class="flex justify-between">
                        <div>
                            <p class="text-sm font-medium text-slate-500 dark:text-slate-400">Categories</p>
                            <p class="mt-2 text-3xl font-bold text-slate-900 dark:text-white">
                                {{ (stats$ | async)?.totalCategories || 0 }}
                            </p>
                            <div class="mt-2 flex items-center">
                                <p class="text-xs text-slate-500 dark:text-slate-400">
                                    {{ getProductsPerCategory((stats$ | async)) }} prods/category
                                </p>
                            </div>
                        </div>
                        <div class="rounded-full bg-purple-100 dark:bg-purple-900/20 p-3 text-purple-700 dark:text-purple-400">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                                <path stroke-linecap="round" stroke-linejoin="round" d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" />
                            </svg>
                        </div>
                    </div>
                    <div class="mt-4">
                        <a routerLink="/categories" class="text-sm font-medium text-primary-600 hover:text-primary-700 dark:text-primary-400 dark:hover:text-primary-300 inline-flex items-center">
                            Manage Categories
                            <svg xmlns="http://www.w3.org/2000/svg" class="ml-1 h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                            </svg>
                        </a>
                    </div>
                </div>
            </div>
            
            <!-- Charts Section -->
            <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-8">
                <!-- Main Chart -->
                <div class="bg-white dark:bg-slate-800 rounded-lg shadow-sm border border-slate-200 dark:border-slate-700 lg:col-span-2">
                    <div class="flex justify-between items-center p-6 border-b border-slate-200 dark:border-slate-700">
                        <h2 class="text-lg font-medium text-slate-900 dark:text-white">Sales Overview</h2>
                        
                        <div class="flex space-x-2">
                            <button
                                [class.bg-primary-600]="chartView === 'revenue'"
                                [class.text-white]="chartView === 'revenue'"
                                [class.bg-white]="chartView !== 'revenue'"
                                [class.dark:bg-slate-700]="chartView !== 'revenue'"
                                [class.text-slate-700]="chartView !== 'revenue'"
                                [class.dark:text-white]="chartView !== 'revenue'"
                                [class.border-slate-300]="chartView !== 'revenue'"
                                [class.dark:border-slate-600]="chartView !== 'revenue'"
                                class="px-3 py-1 text-sm font-medium rounded-md border transition-colors"
                                (click)="setChartView('revenue')">
                                Revenue
                            </button>
                            <button
                                [class.bg-primary-600]="chartView === 'orders'"
                                [class.text-white]="chartView === 'orders'"
                                [class.bg-white]="chartView !== 'orders'"
                                [class.dark:bg-slate-700]="chartView !== 'orders'"
                                [class.text-slate-700]="chartView !== 'orders'"
                                [class.dark:text-white]="chartView !== 'orders'"
                                [class.border-slate-300]="chartView !== 'orders'"
                                [class.dark:border-slate-600]="chartView !== 'orders'"
                                class="px-3 py-1 text-sm font-medium rounded-md border transition-colors"
                                (click)="setChartView('orders')">
                                Orders
                            </button>
                        </div>
                    </div>
                    
                    <div class="p-6">
                        <!-- Placeholder for chart -->
                        <div class="bg-slate-50 dark:bg-slate-700 rounded-lg p-8 h-72 flex items-center justify-center">
                            <div class="text-center">
                                <svg xmlns="http://www.w3.org/2000/svg" class="h-16 w-16 mx-auto text-slate-300 dark:text-slate-600 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                                </svg>
                                <p class="text-slate-500 dark:text-slate-400">
                                    {{ chartView === 'revenue' ? 'Revenue' : 'Orders' }} data visualization will be shown here
                                </p>
                                <button class="mt-4 px-4 py-2 bg-primary-600 text-white rounded-md text-sm hover:bg-primary-700 transition-colors">
                                    Generate Chart
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
                
                <!-- Top Products -->
                <div class="bg-white dark:bg-slate-800 rounded-lg shadow-sm border border-slate-200 dark:border-slate-700">
                    <div class="flex justify-between items-center p-6 border-b border-slate-200 dark:border-slate-700">
                        <h2 class="text-lg font-medium text-slate-900 dark:text-white">Top Products</h2>
                        <select
                            class="form-select bg-white dark:bg-slate-700 text-slate-700 dark:text-white border border-slate-300 dark:border-slate-600 rounded-md text-sm transition-all focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500">
                            <option value="sales">By Sales</option>
                            <option value="revenue">By Revenue</option>
                        </select>
                    </div>
                    
                    <div class="px-6 py-4">
                        <div class="divide-y divide-slate-200 dark:divide-slate-700">
                            <!-- Top product items - this would be populated from API data -->
                            <div class="py-3 flex items-center justify-between">
                                <div class="flex items-center">
                                    <div class="h-10 w-10 bg-slate-100 dark:bg-slate-700 rounded-md flex items-center justify-center mr-3">
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 text-slate-400 dark:text-slate-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
                                        </svg>
                                    </div>
                                    <div>
                                        <p class="font-medium text-slate-900 dark:text-white">Wireless Headphones</p>
                                        <p class="text-xs text-slate-500 dark:text-slate-400">Electronics</p>
                                    </div>
                                </div>
                                <div class="text-right">
                                    <p class="font-medium text-slate-900 dark:text-white">$1,245.50</p>
                                    <p class="text-xs text-emerald-600 dark:text-emerald-400">+12.5%</p>
                                </div>
                            </div>
                            
                            <div class="py-3 flex items-center justify-between">
                                <div class="flex items-center">
                                    <div class="h-10 w-10 bg-slate-100 dark:bg-slate-700 rounded-md flex items-center justify-center mr-3">
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 text-slate-400 dark:text-slate-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
                                        </svg>
                                    </div>
                                    <div>
                                        <p class="font-medium text-slate-900 dark:text-white">Smart Watch</p>
                                        <p class="text-xs text-slate-500 dark:text-slate-400">Wearables</p>
                                    </div>
                                </div>
                                <div class="text-right">
                                    <p class="font-medium text-slate-900 dark:text-white">$945.20</p>
                                    <p class="text-xs text-emerald-600 dark:text-emerald-400">+8.3%</p>
                                </div>
                            </div>
                            
                            <div class="py-3 flex items-center justify-between">
                                <div class="flex items-center">
                                    <div class="h-10 w-10 bg-slate-100 dark:bg-slate-700 rounded-md flex items-center justify-center mr-3">
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 text-slate-400 dark:text-slate-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
                                        </svg>
                                    </div>
                                    <div>
                                        <p class="font-medium text-slate-900 dark:text-white">Smartphone Case</p>
                                        <p class="text-xs text-slate-500 dark:text-slate-400">Accessories</p>
                                    </div>
                                </div>
                                <div class="text-right">
                                    <p class="font-medium text-slate-900 dark:text-white">$625.40</p>
                                    <p class="text-xs text-rose-600 dark:text-rose-400">-2.1%</p>
                                </div>
                            </div>
                        </div>
                    </div>
                    
                    <div class="p-6 border-t border-slate-200 dark:border-slate-700">
                        <a routerLink="/products/list" class="text-sm font-medium text-primary-600 hover:text-primary-700 dark:text-primary-400 dark:hover:text-primary-300 inline-flex items-center">
                            View All Products
                            <svg xmlns="http://www.w3.org/2000/svg" class="ml-1 h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                            </svg>
                        </a>
                    </div>
                </div>
            </div>
            
            <!-- Recent Activity -->
            <div class="bg-white dark:bg-slate-800 rounded-lg shadow-sm border border-slate-200 dark:border-slate-700 mb-8">
                <div class="flex justify-between items-center p-6 border-b border-slate-200 dark:border-slate-700">
                    <h2 class="text-lg font-medium text-slate-900 dark:text-white">Recent Activity</h2>
                    <button class="text-sm font-medium text-primary-600 hover:text-primary-700 dark:text-primary-400 dark:hover:text-primary-300">
                        View All
                    </button>
                </div>
                
                <div class="p-6">
                    <div class="flow-root">
                        <ul class="-my-5 divide-y divide-slate-200 dark:divide-slate-700">
                            <li class="py-5">
                                <div class="relative flex items-start space-x-4">
                                    <div class="flex-shrink-0">
                                        <div class="bg-emerald-100 dark:bg-emerald-900/20 rounded-full p-2 text-emerald-700 dark:text-emerald-400">
                                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                                            </svg>
                                        </div>
                                    </div>
                                    <div class="min-w-0 flex-1">
                                        <div class="text-sm font-medium text-slate-900 dark:text-white">
                                            New product added: Wireless Headphones
                                        </div>
                                        <p class="mt-1 text-sm text-slate-500 dark:text-slate-400">
                                            Added by John Smith
                                        </p>
                                    </div>
                                    <div class="flex-shrink-0 self-center">
                                        <p class="text-xs text-slate-500 dark:text-slate-400">2 hours ago</p>
                                    </div>
                                </div>
                            </li>
                            
                            <li class="py-5">
                                <div class="relative flex items-start space-x-4">
                                    <div class="flex-shrink-0">
                                        <div class="bg-rose-100 dark:bg-rose-900/20 rounded-full p-2 text-rose-700 dark:text-rose-400">
                                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V7m-6 4h6m-6 4h6m-6 4h6m-6 4h6m-9-8l3.5 3.5L15 11" />
                                            </svg>
                                        </div>
                                    </div>
                                    <div class="min-w-0 flex-1">
                                        <div class="text-sm font-medium text-slate-900 dark:text-white">
                                            Order #12345 shipped
                                        </div>
                                        <p class="mt-1 text-sm text-slate-500 dark:text-slate-400">
                                            Shipped to Jane Doe
                                        </p>
                                    </div>
                                    <div class="flex-shrink-0 self-center">
                                        <p class="text-xs text-slate-500 dark:text-slate-400">5 hours ago</p>
                                    </div>
                                </div>
                            </li>

                            <li class="py-5">
                                <div class="relative flex items-start space-x-4">
                                    <div class="flex-shrink-0">
                                        <div class="bg-blue-100 dark:bg-blue-900/20 rounded-full p-2 text-blue-700 dark:text-blue-400">
                                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6 1a9 9 0 11-18 0 9 9 0 0118 0z" />
                                            </svg>
                                        </div>
                                    </div>
                                    <div class="min-w-0 flex-1">
                                        <div class="text-sm font-medium text-slate-900 dark:text-white">
                                            Product stock updated: Smart Watch
                                        </div>
                                        <p class="mt-1 text-sm text-slate-500 dark:text-slate-400">
                                            Updated by Sarah Connor
                                        </p>
                                    </div>
                                    <div class="flex-shrink-0 self-center">
                                        <p class="text-xs text-slate-500 dark:text-slate-400">1 day ago</p>
                                    </div>
                                </div>
                            </li>

                            <li class="py-5">
                                <div class="relative flex items-start space-x-4">
                                    <div class="flex-shrink-0">
                                        <div class="bg-purple-100 dark:bg-purple-900/20 rounded-full p-2 text-purple-700 dark:text-purple-400">
                                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6 1a9 9 0 11-18 0 9 9 0 0118 0z" />
                                            </svg>
                                        </div>
                                    </div>
                                    <div class="min-w-0 flex-1">
                                        <div class="text-sm font-medium text-slate-900 dark:text-white">
                                            New category added: Accessories
                                        </div>
                                        <p class="mt-1 text-sm text-slate-500 dark:text-slate-400">
                                            Added by John Smith
                                        </p>
                                    </div>
                                    <div class="flex-shrink-0 self-center">
                                        <p class="text-xs text-slate-500 dark:text-slate-400">2 days ago</p>
                                    </div>
                                </div>
                            </li>
                        </ul>
                    </div>
                </div>
            </div>
        </div>
    `,
    styles: [`
        .bg-primary-50 {
            background-color: #f0f9ff;
        }
        .text-primary-700 {
            color: #1d4ed8;
        }
        .dark\:bg-slate-700 {
            background-color: #374151;
        }
        .dark\:text-primary-400 {
            color: #60a5fa;
        }
    `]
})
export class StatisticsComponent implements OnInit {
    stats$: Observable<Statistics> | undefined;
    timeRange: string = 'day';
    chartView: string = 'revenue';
    categoryCount: number = 0;


    constructor(private statisticsService: StatisticsService) { }

    ngOnInit(): void {
        this.stats$ = this.statisticsService.getStatistics();
        this.categoryCount = this.statisticsService.getCategoryCount();
    }

    setTimeRange(range: string): void {
        this.timeRange = range;
        this.stats$ = this.statisticsService.getStatistics(range);
    }

    setChartView(view: string): void {
        this.chartView = view;
    }

    getActivePercentage(stats: any): string {
        return ((stats.activeProductCount / stats.totalProducts) * 100).toFixed(2);
    }

    getProductsPerCategory(stats: any): string {
        return (stats.totalProducts / stats.totalCategories).toFixed(2);
    }
}