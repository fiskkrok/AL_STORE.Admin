
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="home-container">
      <h1>Welcome to Store Admin</h1>
      <div class="card">
        <h2>Quick Actions</h2>
        <div class="actions-grid">
          <div class="action-card">
            <i class="bi bi-plus-circle"></i>
            <h3>Add Products</h3>
            <p>Add new products to your store</p>
          </div>
          <div class="action-card">
            <i class="bi bi-graph-up"></i>
            <h3>View Statistics</h3>
            <p>Check your store's performance</p>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .home-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 2rem;
    }

    h1 {
      margin-bottom: 2rem;
      color: var(--text-primary);
    }

    .card {
      background-color: var(--bg-secondary);
      border-radius: 8px;
      padding: 2rem;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
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
export class HomeComponent {}