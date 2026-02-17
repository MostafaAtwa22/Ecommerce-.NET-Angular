import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { AccountService } from '../account-service';
import { Subscription, interval } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { AnimatedOverlayComponent } from '../animated-overlay-component/animated-overlay-component';

@Component({
  selector: 'app-check-inbox',
  imports: [CommonModule, RouterLink, AnimatedOverlayComponent],
  templateUrl: './check-inbox.component.html',
  styleUrls: ['./check-inbox.component.scss'],
})
export class CheckInboxComponent implements OnInit, OnDestroy {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private accountService = inject(AccountService);
  private toastr = inject(ToastrService);

  email: string = '';
  username: string = '';
  verificationCode: string = '';
  isPasswordReset: boolean = true; 
  isResending: boolean = false;
  resendCooldown: number = 0;
  private cooldownSubscription?: Subscription;

  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      this.email = params['email'] || '';
      this.username = params['username'] || '';
      this.verificationCode = params['code'] || '';

      this.isPasswordReset = !this.username && !this.verificationCode;

      if (this.email) {
        if (this.isPasswordReset) {
          this.toastr.info(`Password reset link sent to ${this.email}`, 'Check Your Inbox', {
            timeOut: 5000,
            positionClass: 'toast-top-right',
            progressBar: true,
          });
        } else {
          this.toastr.info(`Verification email sent to ${this.email}`, 'Verify Your Email', {
            timeOut: 5000,
            positionClass: 'toast-top-right',
            progressBar: true,
          });
        }
      }
    });

    const navigation = this.router.getCurrentNavigation();
    if (navigation?.extras?.state) {
      const state = navigation.extras.state;
      this.email = state['email'] || this.email;
      this.username = state['username'] || this.username;
      this.verificationCode = state['code'] || this.verificationCode;

      this.isPasswordReset = !this.username && !this.verificationCode;
    }

    this.startCooldown(30);
  }

  ngOnDestroy() {
    this.stopCooldown();
  }

  // Helper to split verification code into digits for display
  getCodeDigits(): string[] {
    if (!this.verificationCode) return [];
    return this.verificationCode.split('');
  }

  // Unified resend email functionality
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
    const loadingToast = this.toastr.info(
      this.isPasswordReset
        ? 'Resending password reset email...'
        : 'Resending verification email...',
      'Processing',
      {
        disableTimeOut: true,
        positionClass: 'toast-top-center',
      }
    );

    // Call appropriate service method based on flow
    const resendObservable = this.isPasswordReset
      ? this.accountService.resendResetEmail(this.email)
      : this.accountService.resendVerificationEmail(this.email);

    resendObservable.subscribe({
      next: (response) => {
        // Clear loading toast
        this.toastr.clear(loadingToast.toastId);
        this.isResending = false;

        // Show success toast
        this.toastr.success(
          this.isPasswordReset
            ? 'Password reset email has been resent successfully!'
            : 'Verification email has been resent successfully!',
          'Email Resent',
          {
            timeOut: 4000,
            positionClass: 'toast-top-right',
            progressBar: true,
            closeButton: true,
          }
        );

        // Restart cooldown
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

        const errorMessages = this.getResendErrorMessages(err);

        if (errorMessages.length === 0) {
          this.toastr.error('Failed to resend email. Please try again.', 'Resend Failed', {
            timeOut: 6000,
            positionClass: 'toast-top-center',
            closeButton: true,
          });
          return;
        }

        for (const message of errorMessages) {
          this.toastr.error(message, 'Resend Failed', {
            timeOut: 6000,
            positionClass: 'toast-top-center',
            closeButton: true,
          });
        }
      },
    });
  }

  // Error message helper
  private getResendErrorMessages(err: any): string[] {
    if (!err) return ['An unknown error occurred. Please try again.'];
    if (err.status === 0) return ['Network error. Please check your internet connection.'];

    const errorBody = err.error;

    // Common ASP.NET Core validation shape:
    // { errors: { Email: ["..."] }, title: "..." }
    if (errorBody && typeof errorBody === 'object') {
      const errorsObj = (errorBody as any).errors;
      if (errorsObj && typeof errorsObj === 'object') {
        const messages: string[] = [];
        for (const key of Object.keys(errorsObj)) {
          const value = (errorsObj as any)[key];
          if (Array.isArray(value)) {
            for (const msg of value) {
              if (typeof msg === 'string' && msg.trim()) messages.push(msg);
            }
          } else if (typeof value === 'string' && value.trim()) {
            messages.push(value);
          }
        }
        if (messages.length > 0) return messages;
      }
    }

    // If backend returns an array of error messages
    if (Array.isArray(errorBody)) {
      const messages = errorBody
        .filter((x) => typeof x === 'string')
        .map((x) => x.trim())
        .filter(Boolean);
      if (messages.length > 0) return messages;
    }

    // If backend returns plain text
    if (typeof errorBody === 'string' && errorBody.trim()) return [errorBody.trim()];

    const fallbackFromBody = errorBody?.message || errorBody?.title;
    if (typeof fallbackFromBody === 'string' && fallbackFromBody.trim()) return [fallbackFromBody.trim()];

    // Status-based fallback
    switch (err.status) {
      case 400:
        return ['Invalid request. Please check your input.'];
      case 404:
        return [
          this.isPasswordReset
            ? 'Email address not found. Please use a different email.'
            : 'Email address not found or already verified.',
        ];
      case 429:
        return ['Too many resend attempts. Please wait a few minutes.'];
      case 500:
        return ['Server error. Please try again later.'];
      default:
        return [err.message || 'Failed to resend email. Please try again.'];
    }
  }

  // Change email (for password reset flow)
  changeEmail() {
    this.toastr.info('Redirecting to email change...', 'Changing Email', {
      timeOut: 2000,
      positionClass: 'toast-top-center',
    });
    this.router.navigate(['/forgotpassword'], {
      state: { email: this.email },
    });
  }

  // Go to verification page (for email verification flow)
  goToVerificationPage() {
    this.router.navigate(['/verify-email'], {
      queryParams: { email: this.email },
      state: { username: this.username }
    });
  }

  // Start cooldown timer
  private startCooldown(seconds: number) {
    this.resendCooldown = seconds;
    this.stopCooldown();

    this.cooldownSubscription = interval(1000).subscribe(() => {
      if (this.resendCooldown > 0) {
        this.resendCooldown--;
      } else {
        this.stopCooldown();
        // Notify user that resend is available
        this.toastr.success('You can now resend the email', 'Ready to Resend', {
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

  // Simulate email provider opening
  simulateEmailOpen(provider: string) {
    this.toastr.info(`Opening ${provider}...`, 'Redirecting', {
      timeOut: 2000,
      positionClass: 'toast-top-center',
    });
  }
}
