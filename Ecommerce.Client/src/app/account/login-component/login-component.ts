import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AccountService } from '../account-service';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ILogin } from '../../shared/modules/login';
import { AnimatedOverlayComponent } from '../animated-overlay-component/animated-overlay-component';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    AnimatedOverlayComponent
  ],
  templateUrl: './login-component.html',
  styleUrls: ['./login-component.scss']
})
export class LoginComponent implements OnInit {
  private fb = inject(FormBuilder);
  private accountService = inject(AccountService);
  private router = inject(Router);
  private activatedRoute = inject(ActivatedRoute);
  private toastr = inject(ToastrService);

  returnUrl: string = '/';
  showPassword = false;
  isLoading = false;
  errorMessage: string | null = null;

  loginForm = this.fb.group({
    email: [
      '',
      [
        Validators.required,
        Validators.pattern(/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/)
      ]
    ],
    password: ['', Validators.required],
    rememberMe: [false]
  });

  ngOnInit(): void {
    this.activatedRoute.queryParams.subscribe(params => {
      this.returnUrl = params['returnUrl'] || '/';
    });
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  onSubmit(): void {
    this.errorMessage = null;
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      this.toastr.error('Please enter a valid email and password.', 'Validation Error', {
        timeOut: 4000,
        positionClass: 'toast-top-center',
        closeButton: true,
      });
      return;
    }

    this.isLoading = true;
    const loginData = this.loginForm.value as ILogin;

    this.accountService.login(loginData).subscribe({
      next: (response) => {
        this.isLoading = false;

        // Check if 2FA is required
        if (response.requiresTwoFactor) {
          this.toastr.info('Two-factor authentication is required to continue.', '2FA Required', {
            timeOut: 5000,
            positionClass: 'toast-top-right',
            progressBar: true,
            closeButton: true,
          });
          // Navigate to 2FA verification page with email
          this.router.navigate(['/verify-2fa'], {
            queryParams: { email: loginData.email }
          });
        } else if (response.user) {
          this.toastr.success('Logged in successfully.', 'Welcome', {
            timeOut: 4000,
            positionClass: 'toast-top-right',
            progressBar: true,
            closeButton: true,
          });
          // Set user and navigate based on role
          this.accountService['setUser'](response.user);
          this.navigateByRole();
        }
      },
      error: (err) => {
        this.isLoading = false;
        console.error('Login failed:', err);

        const rawMessage =
          (typeof err?.error?.message === 'string' && err.error.message) ||
          (typeof err?.message === 'string' && err.message) ||
          '';
        const lowerRawMessage = rawMessage.toLowerCase();

        let message = rawMessage || 'Login failed. Please try again.';

        if (err?.status === 0) {
          message = 'Network error. Please check your internet connection.';
        } else if (err?.status === 401) {
          message = 'Invalid email or password.';
        } else if (err?.status === 423 || (err?.status === 403 && (lowerRawMessage.includes('locked') || lowerRawMessage.includes('lockout')))) {
          message = err?.error?.message || 'Your account is locked. Please try again later.';
        } else if (err?.status === 429) {
          message = 'Too many login attempts. Please wait a few minutes.';
        }

        if (lowerRawMessage.includes('locked') || lowerRawMessage.includes('lockout')) {
          this.errorMessage = err?.error?.message || 'Your account is locked. Please try again later.';
        } else {
          this.errorMessage = message;
        }

        this.toastr.error(message, 'Login Failed', {
          timeOut: 6000,
          positionClass: 'toast-top-center',
          closeButton: true,
        });
      }
    });
  }

  loginWithGoogle() {
    this.accountService.googleLogin();
  }

  // 🔥 Role-based navigation
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

  get email() {
    return this.loginForm.get('email');
  }

  get password() {
    return this.loginForm.get('password');
  }
}
