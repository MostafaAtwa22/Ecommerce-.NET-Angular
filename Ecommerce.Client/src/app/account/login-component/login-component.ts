import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AccountService } from '../account-service';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ILogin } from '../../shared/modules/login';
import { AnimatedOverlayComponent } from '../animated-overlay-component/animated-overlay-component';

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

  returnUrl: string = '/';
  showPassword = false;
  isLoading = false;

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
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const loginData = this.loginForm.value as ILogin;

    this.accountService.login(loginData).subscribe({
      next: (response) => {
        this.isLoading = false;

        // Check if 2FA is required
        if (response.requiresTwoFactor) {
          // Navigate to 2FA verification page with email
          this.router.navigate(['/verify-2fa'], {
            queryParams: { email: loginData.email }
          });
        } else if (response.user) {
          // Set user and navigate based on role
          this.accountService['setUser'](response.user);
          this.navigateByRole();
        }
      },
      error: (err) => {
        this.isLoading = false;
        console.error('Login failed:', err);
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
