import { CommonModule } from "@angular/common";
import { Component } from "@angular/core";
import { MatButtonModule } from "@angular/material/button";
import { MatDialogModule } from "@angular/material/dialog";
import { MatInputModule } from "@angular/material/input";
import { RouterOutlet } from "@angular/router";


@Component({
    selector: 'app-dashboards',
    standalone: true,
    imports: [CommonModule, RouterOutlet, MatDialogModule, MatInputModule, MatButtonModule],
    template: `
        <router-outlet></router-outlet>
      `
})
export class DashboardComponent { }