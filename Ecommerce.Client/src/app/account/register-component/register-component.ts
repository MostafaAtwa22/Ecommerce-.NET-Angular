import { Component, inject } from '@angular/core';
import {
  AbstractControl,
  AsyncValidatorFn,
  FormBuilder,
  FormGroup,
  ValidationErrors,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';
import { of, timer } from 'rxjs';
import { map, switchMap, catchError } from 'rxjs/operators';
import { AccountService } from '../account-service';
import { Router, RouterLink } from '@angular/router';
import { IRegister } from '../../shared/modules/register';
import { CommonModule } from '@angular/common';
import { AnimatedOverlayComponent } from '../animated-overlay-component/animated-overlay-component';

@Component({
  selector: 'app-register-component',
  templateUrl: './register-component.html',
  styleUrls: ['./register-component.scss'],
  standalone: true,
  imports: [
    ReactiveFormsModule,
    CommonModule,
    RouterLink,
    AnimatedOverlayComponent
  ]
})
export class RegisterComponent {

  private fb = inject(FormBuilder);
  private accountService = inject(AccountService);
  private router = inject(Router);

  showPassword = false;
  isLoading = false;

  // 🔐 Used to show role dropdown only for SuperAdmin
  get isSuperAdmin(): boolean {
    const user = this.accountService.user();
    return !!user && user.roles?.includes('SuperAdmin');
  }

  registerForm: FormGroup = this.fb.group(
    {
      email: [
        '',
        [
          Validators.required,
          Validators.pattern(/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/)
        ],
        [this.validateEmailNotTaken()]
      ],
      userName: [
        '',
        [Validators.required, Validators.pattern(/^[a-zA-Z0-9_]+$/)],
        [this.validateUsernameNotTaken()]
      ],
      firstName: ['', [Validators.required, Validators.pattern(/^[A-Za-z]+$/)]],
      lastName: ['', [Validators.required, Validators.pattern(/^[A-Za-z]+$/)]],
      gender: ['', Validators.required],
      phoneNumber: [
        '',
        [Validators.required, Validators.pattern(/^[0-9+\-() ]+$/)]
      ],
      password: [
        '',
        [
          Validators.required,
          Validators.minLength(6),
          Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*\W).{6,}$/)
        ]
      ],
      confirmPassword: ['', Validators.required],
      roleName: ['Customer', Validators.required]
    },
    { validators: this.passwordMatchValidator }
  );

  // Getters
  get email() { return this.registerForm.get('email'); }
  get userName() { return this.registerForm.get('userName'); }
  get firstName() { return this.registerForm.get('firstName'); }
  get lastName() { return this.registerForm.get('lastName'); }
  get gender() { return this.registerForm.get('gender'); }
  get phoneNumber() { return this.registerForm.get('phoneNumber'); }
  get password() { return this.registerForm.get('password'); }
  get confirmPassword() { return this.registerForm.get('confirmPassword'); }
  get roleName() { return this.registerForm.get('roleName'); }

  // 🔍 Password helpers (for UI)
  passwordIsLength6() {
    return (this.password?.value ?? '').length >= 6;
  }
  passwordContainsCapitalLetter() {
    return /[A-Z]/.test(this.password?.value ?? '');
  }
  passwordContainsNumber() {
    return /[0-9]/.test(this.password?.value ?? '');
  }
  passwordContainsSmallLetter() {
    return /[a-z]/.test(this.password?.value ?? '');
  }
  passwordContainsSpecialChar() {
    return /[\W_]/.test(this.password?.value ?? '');
  }

  // 🔁 Async Validators
  validateEmailNotTaken(): AsyncValidatorFn {
    return (control: AbstractControl) => {
      if (!control.value) return of(null);
      return timer(500).pipe(
        switchMap(() =>
          this.accountService.emailExists(control.value).pipe(
            map(res => (res ? { emailExists: true } : null)),
            catchError(() => of(null))
          )
        )
      );
    };
  }

  validateUsernameNotTaken(): AsyncValidatorFn {
    return (control: AbstractControl) => {
      if (!control.value) return of(null);
      return timer(500).pipe(
        switchMap(() =>
          this.accountService.usernameExists(control.value).pipe(
            map(res => (res ? { usernameExists: true } : null)),
            catchError(() => of(null))
          )
        )
      );
    };
  }

  // 🔐 Password match validator
  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password')?.value;
    const confirm = control.get('confirmPassword');

    if (!password || !confirm) return null;

    if (password !== confirm.value) {
      confirm.setErrors({ passwordMismatch: true });
    } else if (confirm.hasError('passwordMismatch')) {
      const errors = { ...confirm.errors };
      delete errors['passwordMismatch'];
      confirm.setErrors(Object.keys(errors).length ? errors : null);
    }
    return null;
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const v = this.registerForm.value;

    const registerData: IRegister = {
      email: v.email!,
      userName: v.userName!,
      firstName: v.firstName!,
      lastName: v.lastName!,
      gender: Number(v.gender),
      phoneNumber: v.phoneNumber!,
      password: v.password!,
      confirmPassword: v.confirmPassword!,
      roleName: v.roleName!
    };

    this.accountService.register(registerData).subscribe({
      next: () => {
        this.isLoading = false;
        this.navigateByRole();
      },
      error: err => {
        this.isLoading = false;
        console.error('Registration failed:', err);
      }
    });
  }

  // 🚀 Same role-based navigation as Login
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
}
