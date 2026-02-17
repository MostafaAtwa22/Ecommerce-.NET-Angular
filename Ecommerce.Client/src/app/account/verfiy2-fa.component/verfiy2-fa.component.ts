import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AccountService } from '../account-service';
import { AnimatedOverlayComponent } from '../animated-overlay-component/animated-overlay-component';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-verfiy2-fa.component',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    AnimatedOverlayComponent
  ],
  templateUrl: './verfiy2-fa.component.html',
  styleUrl: './verfiy2-fa.component.scss',
})
export class Verfiy2FAComponent implements OnInit {
  private fb = inject(FormBuilder);
  private accountService = inject(AccountService);
  private router = inject(Router);
  private activatedRoute = inject(ActivatedRoute);
  private toastr = inject(ToastrService);

  email: string = '';
  isLoading = false;
  isResending = false;
  errorMessage = '';
  successMessage = '';

  verificationForm = this.fb.group({
    code: [
      '',
      [
        Validators.required,
        Validators.pattern(/^\d{6}$/), // 6-digit code
        Validators.minLength(6),
        Validators.maxLength(6)
      ]
    ]
  });

  ngOnInit(): void {
    // Get email from query parameters
    this.activatedRoute.queryParams.subscribe(params => {
      this.email = params['email'] || '';

      if (!this.email) {
        // If no email provided, redirect to login
        this.router.navigate(['/login']);
      }
    });
  }

  onSubmit(): void {
    if (this.verificationForm.invalid) {
      this.verificationForm.markAllAsTouched();
      this.toastr.error('Please enter the 6-digit verification code.', 'Validation Error', {
        timeOut: 4000,
        positionClass: 'toast-top-center',
        closeButton: true,
      });
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const code = this.verificationForm.value.code!;

    this.accountService.verify2FA({ email: this.email, code }).subscribe({
      next: () => {
        this.isLoading = false;
        this.successMessage = 'Verification successful! Redirecting...';

        this.toastr.success('Verification successful!', 'Success', {
          timeOut: 4000,
          positionClass: 'toast-top-right',
          progressBar: true,
          closeButton: true,
        });

        // Navigate based on role
        setTimeout(() => {
          this.navigateByRole();
        }, 1000);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Invalid or expired verification code. Please try again.';
        console.error('2FA verification failed:', err);

        this.toastr.error(this.errorMessage, 'Verification Failed', {
          timeOut: 6000,
          positionClass: 'toast-top-center',
          closeButton: true,
        });
      }
    });
  }

  resendCode(): void {
    if (!this.email) return;

    this.isResending = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.accountService.resend2FA(this.email).subscribe({
      next: (response) => {
        this.isResending = false;
        this.successMessage = 'Verification code resent successfully! Check your inbox.';

        this.toastr.success('Verification code resent successfully! Check your inbox.', 'Code Resent', {
          timeOut: 5000,
          positionClass: 'toast-top-right',
          progressBar: true,
          closeButton: true,
        });

        // Clear success message after 5 seconds
        setTimeout(() => {
          this.successMessage = '';
        }, 5000);
      },
      error: (err) => {
        this.isResending = false;
        this.errorMessage = 'Failed to resend code. Please try again.';
        console.error('Resend 2FA failed:', err);

        const message = err?.error?.message || this.errorMessage;
        this.toastr.error(message, 'Resend Failed', {
          timeOut: 6000,
          positionClass: 'toast-top-center',
          closeButton: true,
        });
      }
    });
  }

  // Role-based navigation
  private navigateByRole(): void {
    const user = this.accountService.user();

    if (!user || !user.roles) {
      this.router.navigateByUrl('/');
      return;
    }

    if (user.roles.includes('Admin') || user.roles.includes('SuperAdmin')) {
      this.router.navigateByUrl('/dashboard');
    } else {
      this.router.navigateByUrl('/');
    }
  }

  get code() {
    return this.verificationForm.get('code');
  }
}
