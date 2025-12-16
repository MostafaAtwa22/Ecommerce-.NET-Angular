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
    email: ['', [Validators.required, Validators.pattern(/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/)]],
    password: ['', [Validators.required]],
    rememberMe: [false]
  });

  ngOnInit(): void {
    this.activatedRoute.queryParams.subscribe(params => {
      this.returnUrl = params['returnUrl'] || '/';
    });

    // Subscribe to Google authentication state
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

  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }

  onSubmit() {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const loginData = this.loginForm.value as ILogin;

    this.accountService.login(loginData).subscribe({
      next: () => {
        this.isLoading = false;
        this.router.navigateByUrl(this.returnUrl);
      },
      error: (err) => {
        this.isLoading = false;
        console.error('Login failed:', err);
      }
    });
  }

  // Handle Google login
  private handleGoogleLogin(user: SocialUser) {
    this.isLoading = true;

    // Send the ID token to your backend
    this.accountService.googleLogin(user.idToken).subscribe({
      next: () => {
        this.isLoading = false;
        this.router.navigateByUrl(this.returnUrl);
      },
      error: (err) => {
        this.isLoading = false;
        console.error('Google login failed:', err);
      }
    });
  }

  get email() { return this.loginForm.get('email'); }
  get password() { return this.loginForm.get('password'); }
}
