import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { AccountService } from '../account-service';
import { Subscription, interval } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { AnimatedOverlayComponent } from '../animated-overlay-component/animated-overlay-component';

@Component({
  selector: 'app-check-inbox.component',
  imports: [CommonModule, RouterLink, AnimatedOverlayComponent],
  templateUrl: './check-inbox.component.html',
  styleUrls: ['./check-inbox.component.scss'],
})
export class CheckInboxComponent implements OnInit, OnDestroy {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private accountService = inject(AccountService);
  private toastr = inject(ToastrService); // Inject ToastrService

  email: string = '';
  isResending: boolean = false;
  resendCooldown: number = 0;
  private cooldownSubscription?: Subscription;

  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      this.email = params['email'] || '';

      // Show info toast when component loads with email
      if (this.email) {
        this.toastr.info(`Password reset link sent to ${this.email}`, 'Check Your Inbox', {
          timeOut: 5000,
          positionClass: 'toast-top-right',
          progressBar: true,
        });
      }
    });

    // Alternatively, get from state
    const navigation = this.router.getCurrentNavigation();
    if (navigation?.extras?.state?.['email']) {
      this.email = navigation.extras.state['email'];
    }

    // Start with 30-second cooldown to prevent spam
    this.startCooldown(30);
  }

  ngOnDestroy() {
    this.stopCooldown();
  }

  // Resend email functionality
  resendEmail() {
    if (this.resendCooldown > 0 || this.isResending) {
      if (this.resendCooldown > 0) {
        this.toastr.warning(
          `Please wait ${this.resendCooldown} seconds before resending`,
          'Too Soon',
          {
            timeOut: 3000,
            positionClass: 'toast-top-center',
          }
        );
      }
      return;
    }

    // Validate email
    if (!this.email || this.email.trim() === '') {
      this.toastr.error('Email address is required', 'Validation Error', {
        timeOut: 3000,
        positionClass: 'toast-top-center',
      });
      return;
    }

    this.isResending = true;

    // Show loading toast
    const loadingToast = this.toastr.info('Resending reset email...', 'Processing', {
      disableTimeOut: true,
      positionClass: 'toast-top-center',
    });

    // Debug log
    console.log('Attempting to resend email to:', this.email);

    this.accountService.resendResetEmail(this.email).subscribe({
      next: (response) => {
        // Clear loading toast
        this.toastr.clear(loadingToast.toastId);

        this.isResending = false;

        // Debug log
        console.log('Resend successful! Response:', response);

        // Show success toast
        this.toastr.success('Password reset email has been resent successfully!', 'Email Resent', {
          timeOut: 4000,
          positionClass: 'toast-top-right',
          progressBar: true,
          closeButton: true,
        });

        // User stays on the same page - just restarts cooldown
        this.startCooldown(30);

        // Show info about cooldown
        this.toastr.info('You can resend again in 30 seconds', 'Cooldown Started', {
          timeOut: 3000,
          positionClass: 'toast-bottom-right',
        });
      },
      error: (err) => {
        // Clear loading toast
        this.toastr.clear(loadingToast.toastId);

        this.isResending = false;

        // Enhanced error logging
        console.error('=== RESEND ERROR DETAILS ===');
        console.error('Error object:', err);
        console.error('Status code:', err?.status);
        console.error('Status text:', err?.statusText);
        console.error('Error message:', err?.error?.message || err?.message);
        console.error('Full error:', JSON.stringify(err, null, 2));
        console.error('=== END ERROR DETAILS ===');

        // Get detailed error message
        const errorMessage = this.getDetailedResendErrorMessage(err);

        // Show error toast
        this.toastr.error(errorMessage, 'Resend Failed', {
          timeOut: 6000,
          positionClass: 'toast-top-center',
          closeButton: true,
          enableHtml: false,
        });

        // If it's a 400 error, show more specific guidance
        if (err?.status === 400) {
          this.toastr.warning(
            'Please check if the email is correct and try again.',
            'Bad Request',
            {
              timeOut: 4000,
              positionClass: 'toast-bottom-right',
            }
          );
        }
      },
    });
  }

  // Enhanced error message helper
  private getDetailedResendErrorMessage(err: any): string {
    if (!err) {
      return 'An unknown error occurred. Please try again.';
    }

    // Check for network errors
    if (err.status === 0) {
      return 'Network error. Please check your internet connection.';
    }

    // Check for specific status codes
    switch (err.status) {
      case 400:
        if (err.error?.message) {
          return `Bad request: ${err.error.message}`;
        }
        return 'Invalid request. Please check your input and try again.';

      case 404:
        return 'Email address not found. Please use a different email.';

      case 429:
        return 'Too many resend attempts. Please wait a few minutes before trying again.';

      case 500:
        if (err.error?.message) {
          return `Server error: ${err.error.message}`;
        }
        return 'Server error. Please try again later.';

      default:
        // Try to extract message from error object
        const message = err.error?.message || err.message || err.statusText;
        return message || 'Failed to resend email. Please try again.';
    }
  }

  // Also update your existing getResendErrorMessage method if you want to keep it:
  private getResendErrorMessage(err: any): string {
    if (err.status === 404) {
      return 'Email address not found. Please use a different email.';
    } else if (err.status === 429) {
      return 'Too many resend attempts. Please wait a few minutes.';
    } else if (err.status === 500) {
      return 'Server error. Please try again later.';
    }
    return 'Failed to resend email. Please try again.';
  }

  changeEmail() {
    // Show info toast before navigating
    this.toastr.info('Redirecting to email change...', 'Changing Email', {
      timeOut: 2000,
      positionClass: 'toast-top-center',
    });

    this.router.navigate(['/forgotpassword'], {
      state: { email: this.email },
    });
  }

  private startCooldown(seconds: number) {
    this.resendCooldown = seconds;
    this.stopCooldown();

    this.cooldownSubscription = interval(1000).subscribe(() => {
      if (this.resendCooldown > 0) {
        this.resendCooldown--;

        // Optional: Show toast when cooldown reaches certain points
        if (this.resendCooldown === 10 || this.resendCooldown === 5) {
          this.toastr.info(`Resend available in ${this.resendCooldown} seconds`, 'Almost Ready', {
            timeOut: 2000,
            positionClass: 'toast-bottom-right',
          });
        }
      } else {
        this.stopCooldown();

        // Notify user that resend is available
        this.toastr.success('You can now resend the reset email', 'Ready to Resend', {
          timeOut: 3000,
          positionClass: 'toast-bottom-right',
        });
      }
    });
  }

  private stopCooldown() {
    if (this.cooldownSubscription) {
      this.cooldownSubscription.unsubscribe();
      this.cooldownSubscription = undefined;
    }
  }

  simulateEmailOpen(provider: string) {
    // Show info toast when user opens email provider
    this.toastr.info(`Opening ${provider}...`, 'Redirecting', {
      timeOut: 2000,
      positionClass: 'toast-top-center',
    });
  }
}
