import { Component, OnInit } from '@angular/core';
import { AccountService } from '../account-service';
import { Router } from '@angular/router';
import { AnimatedOverlayComponent } from "../animated-overlay-component/animated-overlay-component";

@Component({
  selector: 'app-google-callback',
  imports: [AnimatedOverlayComponent],
  templateUrl: './google-callback.component.html',
  styleUrl: './google-callback.component.scss',
})
export class GoogleCallbackComponent implements OnInit {
  constructor(private accountService: AccountService, private router: Router) {
  }

  async ngOnInit() {
    const result$ = await this.accountService.processGoogleLogin();

    if (!result$) {
      this.router.navigate(['/login']);
      return;
    }

    result$.subscribe({
      next: (response) => {
        if (response.requiresTwoFactor) {
          const email = response.email || response.user?.email;

          if (email) {
            this.router.navigate(['/verify-2fa'], {
              queryParams: { email }
            });
          } else {
            this.router.navigate(['/login']);
          }
        } else {
          this.router.navigate(['/home']);
        }
      },
      error: (err) => {
        console.error('Google login failed:', err);
        this.router.navigate(['/login']);
      }
    });
  }
}
