import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AccountService } from '../account-service';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ILogin } from '../../shared/modules/login';
import { AnimatedOverlayComponent } from '../animated-overlay-component/animated-overlay-component';
import {
  SocialLoginModule,
  GoogleSigninButtonDirective,
  SocialAuthService,
  SocialUser
} from '@abuelwiss/angularx-social-login';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    SocialLoginModule,
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    AnimatedOverlayComponent,
    GoogleSigninButtonDirective
  ],
  templateUrl: './login-component.html',
  styleUrls: ['./login-component.scss']
})
export class LoginComponent implements OnInit {

  private fb = inject(FormBuilder);
  private accountService = inject(AccountService);
  private router = inject(Router);
  private activatedRoute = inject(ActivatedRoute);
  private authService = inject(SocialAuthService);

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

    // Google auth listener
    this.authService.authState.subscribe({
      next: (user: SocialUser) => {
        if (user) {
          this.handleGoogleLogin(user);
        }
      },
      error: (err) => {
        console.error('Google auth error:', err);
      }
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
      next: () => {
        this.isLoading = false;
        this.navigateByRole();
      },
      error: (err) => {
        this.isLoading = false;
        console.error('Login failed:', err);
      }
    });
  }

  // Google login handler
  private handleGoogleLogin(user: SocialUser): void {
    this.isLoading = true;

    this.accountService.googleLogin(user.idToken).subscribe({
      next: () => {
        this.isLoading = false;
        this.navigateByRole();
      },
      error: (err) => {
        this.isLoading = false;
        console.error('Google login failed:', err);
      }
    });
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
