import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
    selector: 'app-unauthorized',
    standalone: true,
    imports: [CommonModule, RouterLink],
    template: `
    <div class="unauthorized-container text-center py-5">
      <div class="row justify-content-center">
        <div class="col-md-6">
          <i class="fas fa-shield-alt fa-5x text-danger mb-4"></i>
          <h1 class="display-4 fw-bold">403 - Access Denied</h1>
          <p class="lead text-muted mb-4">
            Oops! You don't have the required permissions to access this page.
          </p>
          <div class="d-flex justify-content-center gap-3">
            <button class="btn btn-dark px-4 py-2" routerLink="/dashboard">
              <i class="fas fa-th-large me-2"></i>Back to Dashboard
            </button>
            <button class="btn btn-outline-dark px-4 py-2" routerLink="/home">
              <i class="fas fa-home me-2"></i>Home Page
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
    styles: [`
    .unauthorized-container {
      min-height: 80vh;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    i {
      opacity: 0.8;
      animation: pulse 2s infinite;
    }
    @keyframes pulse {
      0% { transform: scale(1); opacity: 0.8; }
      50% { transform: scale(1.1); opacity: 1; }
      100% { transform: scale(1); opacity: 0.8; }
    }
  `]
})
export class UnauthorizedComponent { }
