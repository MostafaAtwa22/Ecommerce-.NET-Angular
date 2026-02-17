import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AccountService } from '../account-service';
import { CommonModule } from '@angular/common';
import { AnimatedOverlayComponent } from "../animated-overlay-component/animated-overlay-component";
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-verify-email',
  imports: [ReactiveFormsModule, CommonModule, AnimatedOverlayComponent],
  templateUrl: './verify-email.component.html',
  styleUrl: './verify-email.component.scss',
})
export class VerifyEmailComponent implements OnInit {
  error?: string;
  loading = false;
  showResendSuccess = false;
  resendLoading = false;
  resendCooldown = 0;
  private resendTimer: any;

  form: FormGroup;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private accountService: AccountService,
    private router: Router,
    private toastr: ToastrService
  ) {
    this.form = this.fb.group({
      email: [{ value: '', disabled: true }],
      code: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]],
    });
  }

  ngOnInit(): void {
    const email = this.route.snapshot.queryParamMap.get('email');

    if (!email) {
      this.error = 'Invalid verification link';
      this.toastr.error(this.error, 'Error', {
        timeOut: 6000,
        positionClass: 'toast-top-center',
        closeButton: true,
      });
      return;
    }

    this.form.patchValue({ email });

    this.startResendCooldown(30);
  }

  get email() {
    return this.form.get('email');
  }

  get code() {
    return this.form.get('code');
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.toastr.error('Please enter the verification code.', 'Validation Error', {
        timeOut: 4000,
        positionClass: 'toast-top-center',
        closeButton: true,
      });
      return;
    }

    this.loading = true;
    this.error = undefined;
    this.showResendSuccess = false;

    const email = this.form.getRawValue().email!;
    const code = this.form.getRawValue().code!;

    this.accountService.verifyEmail({ email, code }).subscribe({
      next: () => {
        this.loading = false;

        this.toastr.success('Email verified successfully!', 'Success', {
          timeOut: 4000,
          positionClass: 'toast-top-right',
          progressBar: true,
          closeButton: true,
        });
        this.navigateByRole();
      },
      error: (err) => {
        this.error = 'Invalid or expired verification code';
        if (err?.error?.message) {
          this.error = err.error.message;
        }
        this.loading = false;

        this.toastr.error(this.error, 'Verification Failed', {
          timeOut: 6000,
          positionClass: 'toast-top-center',
          closeButton: true,
        });
      },
    });
  }

  private navigateByRole(): void {
    const user = this.accountService.user();

    if (!user || !user.roles) {
      this.router.navigate(['/']);
      return;
    }

    if (user.roles.includes('Admin') || user.roles.includes('SuperAdmin')) {
      this.router.navigate(['/dashboard']);
    } else if (user.roles.includes('Customer')) {
      this.router.navigate(['/']);
    } else {
      this.router.navigate(['/']);
    }
  }

  resendCode() {
    if (this.resendCooldown > 0 || this.resendLoading) {
      if (this.resendCooldown > 0) {
        this.toastr.warning(`Please wait ${this.resendCooldown} seconds before resending.`, 'Too Soon', {
          timeOut: 3000,
          positionClass: 'toast-top-center',
          closeButton: true,
        });
      }
      return;
    }

    const email = this.form.getRawValue().email!;

    if (!email) {
      this.error = 'Email not found';
      this.toastr.error(this.error, 'Error', {
        timeOut: 6000,
        positionClass: 'toast-top-center',
        closeButton: true,
      });
      return;
    }

    this.resendLoading = true;
    this.error = undefined;

    this.accountService.resendVerificationEmail(email).subscribe({
      next: () => {
        this.showResendSuccess = true;
        this.resendLoading = false;

        this.toastr.success('Verification code resent. Check your inbox.', 'Code Resent', {
          timeOut: 5000,
          positionClass: 'toast-top-right',
          progressBar: true,
          closeButton: true,
        });

        this.startResendCooldown(30);

        setTimeout(() => {
          this.showResendSuccess = false;
        }, 5000);
      },
      error: (err) => {
        this.error = 'Failed to resend verification code. Please try again.';
        if (err?.error?.message) {
          this.error = err.error.message;
        }
        this.resendLoading = false;

        this.toastr.error(this.error, 'Resend Failed', {
          timeOut: 6000,
          positionClass: 'toast-top-center',
          closeButton: true,
        });
      }
    });
  }

  private startResendCooldown(seconds: number) {
    this.resendCooldown = seconds;

    if (this.resendTimer) {
      clearInterval(this.resendTimer);
    }

    this.resendTimer = setInterval(() => {
      if (this.resendCooldown > 0) {
        this.resendCooldown--;
      } else {
        clearInterval(this.resendTimer);
      }
    }, 1000);
  }

  ngOnDestroy() {
    if (this.resendTimer) {
      clearInterval(this.resendTimer);
    }
  }
}
