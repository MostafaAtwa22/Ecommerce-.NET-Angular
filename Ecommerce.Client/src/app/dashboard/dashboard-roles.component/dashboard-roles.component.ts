import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-dashboard-roles',
  standalone: true,
  imports: [
    RouterOutlet
  ],
  templateUrl: './dashboard-roles.component.html',
  styleUrl: './dashboard-roles.component.scss'
})
export class DashboardRolesComponent {
}
