import { Component, OnInit } from '@angular/core';
import { AccountService } from '../account-service';
import { Router } from '@angular/router';
import { AnimatedOverlayComponent } from "../animated-overlay-component/animated-overlay-component";
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-google-callback',
  imports: [AnimatedOverlayComponent],
  templateUrl: './google-callback.component.html',
  styleUrl: './google-callback.component.scss',
})
export class GoogleCallbackComponent implements OnInit {
  constructor(
    private accountService: AccountService,
    private router: Router,
    private toastr: ToastrService
  ) {
  }

  async ngOnInit() {
    const result$ = await this.accountService.processGoogleLogin();

    if (!result$) {
      this.toastr.error('Google login could not be processed. Please try again.', 'Login Failed', {
        timeOut: 6000,
        positionClass: 'toast-top-center',
        closeButton: true,
      });
      this.router.navigate(['/login']);
      return;
    }

    result$.subscribe({
      next: (response) => {
        if (response.requiresTwoFactor) {
          const email = response.email || response.user?.email;

          if (email) {
            this.toastr.info('Two-factor authentication is required to continue.', '2FA Required', {
              timeOut: 5000,
              positionClass: 'toast-top-right',
              progressBar: true,
              closeButton: true,
            });
            this.router.navigate(['/verify-2fa'], {
              queryParams: { email }
            });
          } else {
            this.toastr.error('Unable to continue login. Please try again.', 'Login Failed', {
              timeOut: 6000,
              positionClass: 'toast-top-center',
              closeButton: true,
            });
            this.router.navigate(['/login']);
          }
        } else {
          this.toastr.success('Signed in successfully with Google.', 'Welcome', {
            timeOut: 4000,
            positionClass: 'toast-top-right',
            progressBar: true,
            closeButton: true,
          });
          this.router.navigate(['/home']);
        }
      },
      error: (err) => {
        console.error('Google login failed:', err);
        this.toastr.error('Google login failed. Please try again.', 'Login Failed', {
          timeOut: 6000,
          positionClass: 'toast-top-center',
          closeButton: true,
        });
        this.router.navigate(['/login']);
      }
    });
  }
}
