import { Component, Input } from '@angular/core';
import { NgClass } from '@angular/common';

@Component({
    standalone: true,
    imports: [NgClass],
    selector: 'app-status-badge',
    template: `
    <span class="status-badge" [ngClass]="statusClass">
      {{ text }}
    </span>
  `,
    styles: [`
    .status-badge {
      padding: 0.25rem 0.5rem;
      border-radius: 9999px;
      font-size: 0.875rem;
      font-weight: 500;
      text-transform: capitalize;
    }

    .pending {
      background-color: rgb(250, 204, 21, 0.2);
      color: rgb(161, 98, 7);
    }

    .confirmed {
      background-color: rgb(37, 99, 235, 0.2);
      color: rgb(29, 78, 216);
    }

    .processing {
      background-color: rgb(147, 51, 234, 0.2);
      color: rgb(126, 34, 206);
    }

    .shipped {
      background-color: rgb(6, 182, 212, 0.2);
      color: rgb(14, 116, 144);
    }

    .delivered {
      background-color: rgb(34, 197, 94, 0.2);
      color: rgb(21, 128, 61);
    }

    .cancelled {
      background-color: rgb(239, 68, 68, 0.2);
      color: rgb(185, 28, 28);
    }
  `]
})
export class StatusBadgeComponent {
    @Input() status!: string;

    get statusClass(): string {
        return this.status.toLowerCase();
    }

    get text(): string {
        return this.status.charAt(0).toUpperCase() + this.status.slice(1).toLowerCase();
    }
}