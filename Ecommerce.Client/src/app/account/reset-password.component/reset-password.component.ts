import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Subscription } from 'rxjs';
import { AccountService } from '../account-service';
import { IResetPassword } from '../../shared/modules/accountUser';
import { AnimatedOverlayComponent } from '../animated-overlay-component/animated-overlay-component';

@Component({
  selector: 'app-reset-password.component',
  imports: [ReactiveFormsModule, CommonModule, RouterLink, AnimatedOverlayComponent],
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.scss'],
})
export class ResetPasswordComponent implements OnInit, OnDestroy {
  private fb = inject(FormBuilder);
  private accountService = inject(AccountService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private toastr = inject(ToastrService);

  resetForm: FormGroup;
  email: string = '';
  token: string = '';
  showPassword: boolean = false;
  isLoading: boolean = false;
  successMessage: string = '';
  errorMessage: string = '';
  showResendOption: boolean = false;
  private paramSubscription?: Subscription;

  constructor() {
    this.resetForm = this.fb.group(
      {
        newPassword: [
          '',
          [
            Validators.required,
            Validators.minLength(6),
            Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*\W).{6,}$/),
          ],
        ],
        confirmNewPassword: ['', [Validators.required]],
      },
      { validators: this.passwordMatchValidator }
    );
  }

  get newPassword() {
    return this.resetForm.get('newPassword');
  }

  get confirmNewPassword() {
    return this.resetForm.get('confirmNewPassword');
  }

  ngOnInit() {
    this.paramSubscription = this.route.queryParams.subscribe((params) => {
      this.email = params['email'] || '';
      this.token = params['token'] || '';

      console.log('Email from URL:', this.email);
      console.log('Token from URL:', this.token);
      console.log('Decoded token:', decodeURIComponent(this.token));

      if (!this.email || !this.token) {
        this.errorMessage = 'Invalid or expired reset link. Please request a new password reset.';
        this.showResendOption = true;
        this.toastr.error('Invalid reset link', 'Error');
      } else {
        this.toastr.info(`Resetting password for ${this.email}`, 'Reset Password');
      }
    });
  }

  ngOnDestroy() {
    if (this.paramSubscription) {
      this.paramSubscription.unsubscribe();
    }
  }

  // Password validation helpers
  passwordIsLength6(): boolean {
    const pass = this.newPassword?.value ?? '';
    return pass.length >= 6;
  }

  passwordContainsCapitalLetter(): boolean {
    const pass = this.newPassword?.value ?? '';
    return /[A-Z]/.test(pass);
  }

  passwordContainsSmallLetter(): boolean {
    const pass = this.newPassword?.value ?? '';
    return /[a-z]/.test(pass);
  }

  passwordContainsSpecialChar(): boolean {
    const pass = this.newPassword?.value ?? '';
    return /[\W_]/.test(pass);
  }

  passwordContainsNumber(): boolean {
    const pass = this.newPassword?.value ?? '';
    return /[0-9]/.test(pass);
  }

  togglePassword() {
    this.showPassword = !this.showPassword;
  }

  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('newPassword')?.value;
    const confirmPassword = control.get('confirmNewPassword')?.value;

    if (!password || !confirmPassword) return null;

    if (password !== confirmPassword) {
      control.get('confirmNewPassword')?.setErrors({ passwordMismatch: true });
    } else {
      const confirmControl = control.get('confirmNewPassword');
      if (confirmControl?.hasError('passwordMismatch')) {
        const errors = { ...confirmControl.errors };
        delete errors['passwordMismatch'];
        confirmControl.setErrors(Object.keys(errors).length ? errors : null);
      }
    }
    return null;
  }
  onSubmit() {
    if (this.resetForm.invalid) {
      this.resetForm.markAllAsTouched();
      this.toastr.warning('Please fill all required fields correctly', 'Validation Error');
      return;
    }

    if (!this.email || !this.token) {
      this.errorMessage = 'Invalid reset link. Please request a new password reset.';
      this.showResendOption = true;
      this.toastr.error('Invalid reset link parameters', 'Error');
      return;
    }

    this.isLoading = true;
    this.successMessage = '';
    this.errorMessage = '';
    this.showResendOption = false;

    const resetData: IResetPassword = {
      email: this.email,
      token: decodeURIComponent(this.token),
      newPassword: this.newPassword?.value,
      confirmNewPassword: this.confirmNewPassword?.value,
    };

    console.log('Sending reset data:', {
      email: resetData.email,
      tokenLength: resetData.token.length,
      newPassword: '***',
      confirmNewPassword: '***',
    });

    this.accountService.resetPassword(resetData).subscribe({
      next: (response: any) => {
        console.log('Reset successful! Response:', response);
        this.isLoading = false;
        this.successMessage =
          'Password reset successfully! You can now login with your new password.';

        this.toastr.success('Password reset successfully!', 'Success', {
          timeOut: 5000,
          positionClass: 'toast-top-right',
          progressBar: true,
        });

        this.resetForm.reset();

        // Navigate to login immediately (remove the 5-second delay)
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 2000); // Reduced to 2 seconds for better UX
      },
      error: (err) => {
        console.error('=== RESET PASSWORD ERROR ===');
        console.error('Full error:', err);
        console.error('Status:', err?.status);
        console.error('Status text:', err?.statusText);
        console.error('Error message:', err?.error?.message);
        console.error('Error errors:', err?.error?.errors);
        console.error('=== END ERROR ===');

        this.isLoading = false;

        // Only show error if it's NOT a success status
        if (err?.status !== 200) {
          this.errorMessage = this.getErrorMessage(err);
          this.showResendOption = this.shouldShowResendOption(err);

          this.toastr.error(this.errorMessage, 'Reset Failed', {
            timeOut: 6000,
            positionClass: 'toast-top-center',
            closeButton: true,
          });
        }
      },
    });
  }

  private getErrorMessage(error: any): string {
    console.log('Getting error message for:', error);

    // If status is 200, it's actually a success
    if (error.status === 200) {
      return ''; // Empty string for success
    }

    if (error.status === 400) {
      if (error.error?.message?.includes('No user')) {
        return 'No user with this email exists.';
      }
      if (error.error?.message?.includes('Invalid token')) {
        return 'Reset link has expired or is invalid. Please request a new reset link.';
      }
      if (error.error?.errors) {
        const errors = error.error.errors;
        console.log('API validation errors:', errors);

        if (errors.NewPassword) {
          return `Password error: ${errors.NewPassword[0]}`;
        }
        if (errors.ConfirmNewPassword) {
          return `Confirm password: ${errors.ConfirmNewPassword[0]}`;
        }
        if (errors.Email) {
          return `Email: ${errors.Email[0]}`;
        }
        if (errors.Token) {
          return `Token error: ${errors.Token[0]}`;
        }
        if (Array.isArray(errors) && errors.length > 0) {
          return errors[0];
        }
      }
      return error.error?.message || 'Invalid password reset request. Please check your input.';
    }

    if (error.status === 429) {
      return 'Too many attempts. Please wait a few minutes before trying again.';
    }

    if (error.status === 500) {
      return 'Server error. Please try again later.';
    }

    if (error.status === 0) {
      return 'Network error. Please check your internet connection.';
    }

    return `Failed to reset password (${error.status}). Please try again.`;
  }

  private shouldShowResendOption(error: any): boolean {
    // Don't show resend option if it's a success (200)
    if (error.status === 200) return false;

    return (
      error.status === 400 &&
      (error.error?.message?.includes('Invalid token') ||
        error.error?.message?.includes('No user') ||
        error.error?.errors?.Token)
    );
  }

  resendResetEmail() {
    if (!this.email) return;

    this.toastr.info('Resending reset email...', 'Processing');

    this.accountService.resendResetEmail(this.email).subscribe({
      next: () => {
        this.toastr.success('Reset email resent successfully! Check your inbox.', 'Email Sent', {
          timeOut: 5000,
          positionClass: 'toast-top-right',
        });

        this.errorMessage = 'A new reset link has been sent to your email.';
        this.showResendOption = false;
      },
      error: (err) => {
        console.error('Resend error:', err);
        this.toastr.error('Failed to resend email. Please try again.', 'Error');
      },
    });
  }
}
