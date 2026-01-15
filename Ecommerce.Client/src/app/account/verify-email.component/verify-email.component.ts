import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AccountService } from '../account-service';
import { CommonModule } from '@angular/common';
import { AnimatedOverlayComponent } from "../animated-overlay-component/animated-overlay-component";

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
    private router: Router
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
    if (this.form.invalid) return;

    this.loading = true;
    this.error = undefined;
    this.showResendSuccess = false;

    const email = this.form.getRawValue().email!;
    const code = this.form.getRawValue().code!;

    this.accountService.verifyEmail({ email, code }).subscribe({
      next: () => {
        this.loading = false;
        this.navigateByRole();
      },
      error: (err) => {
        this.error = 'Invalid or expired verification code';
        if (err?.error?.message) {
          this.error = err.error.message;
        }
        this.loading = false;
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
      return;
    }

    const email = this.form.getRawValue().email!;

    if (!email) {
      this.error = 'Email not found';
      return;
    }

    this.resendLoading = true;
    this.error = undefined;

    this.accountService.resendVerificationEmail(email).subscribe({
      next: () => {
        this.showResendSuccess = true;
        this.resendLoading = false;

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
