import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, MatCardModule, RouterModule],
  template: `
    <div class="home-container container-lg">
      <h1>Welcome to Store Admin</h1>
      <mat-card class="card d-flex"  >
        <h2 style="color: white;">Quick Actions</h2>
        <div class="actions-grid">
          
          <div class="action-card" style="color: white;" [routerLink]="['/products/add']">
            <i class="bi bi-plus-circle"></i>
            <h3>Add Products</h3>
            <p>Add new products to your store</p>
          </div>
          <div class="action-card" style="color: white;" [routerLink]="['/statistics']">
            <i class="bi bi-graph-up"></i>
            <h3>View Statistics</h3>
            <p>Check your store's performance</p>
          </div>
        </div>
      </mat-card>
    </div>
  `,
  styles: [`
    .home-container {
      // max-width: 1200px;
      margin: 0 auto;
      padding: 2rem;
    }

    h1 {
      margin-bottom: 2rem;
      color: var(--text-primary);
    }

    .card {
      // max-width: 800px;
      background-color: var(--bg-secondary);
      border-radius: 8px;
      padding: 2rem;
      box-shadow: 0 0 20px 20px rgb(0 0 0 / 19%);
    }

    .actions-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 2rem;
      margin-top: 2rem;
    }

    .action-card {
      background-color: var(--bg-primary);
      padding: 1.5rem;
      border-radius: 8px;
      text-align: center;
      transition: transform 0.2s;
      cursor: pointer;

      &:hover {
        transform: translateY(-5px);
      }

      i {
        font-size: 2rem;
        color: var(--primary);
        margin-bottom: 1rem;
      }

      h3 {
        color: var(--text-primary);
        margin-bottom: 0.5rem;
      }

      p {
        color: var(--text-secondary);
        margin: 0;
      }
    }
  `]
})
export class HomeComponent { }