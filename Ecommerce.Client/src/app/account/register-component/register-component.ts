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
import { CommonModule } from '@angular/common'; // Add this import
import { AnimatedOverlayComponent } from '../animated-overlay-component/animated-overlay-component';

@Component({
  selector: 'app-register-component',
  templateUrl: './register-component.html',
  styleUrls: ['./register-component.scss'],
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, RouterLink, AnimatedOverlayComponent] // Add these imports
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private accountService = inject(AccountService);
  private router = inject(Router);

  showPassword = false;
  isLoading = false;

  get isSuperAdmin(): boolean {
    const user = this.accountService.user();
    return !!user && user.roles?.includes('SuperAdmin');
  }

  registerForm: FormGroup = this.fb.group({
    email: [
      '',
      [Validators.required, Validators.pattern(/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/)],
      [this.validateEmailNotTaken()]
    ],
    userName: [
      '',
      [Validators.required, Validators.pattern(/^[a-zA-Z0-9_]+$/)],
      [this.validateUsernameNotTaken()]
    ],
    firstName: [
      '',
      [Validators.required, Validators.pattern(/^[A-Za-z]+$/)]
    ],
    lastName: [
      '',
      [Validators.required, Validators.pattern(/^[A-Za-z]+$/)]
    ],
    gender: ['', [Validators.required]],
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
    confirmPassword: ['', [Validators.required]],
    roleName: ['Customer', [Validators.required]]
  },
  { validators: this.passwordMatchValidator });

  get email() { return this.registerForm.get('email'); }
  get userName() { return this.registerForm.get('userName'); }
  get firstName() { return this.registerForm.get('firstName'); }
  get lastName() { return this.registerForm.get('lastName'); }
  get gender() { return this.registerForm.get('gender'); }
  get phoneNumber() { return this.registerForm.get('phoneNumber'); }
  get password() { return this.registerForm.get('password'); }
  get confirmPassword() { return this.registerForm.get('confirmPassword'); }
  get roleName() { return this.registerForm.get('roleName'); }

  passwordIsLength6(): boolean {
    const pass = this.password?.value ?? '';
    return pass.length >= 6;
  }

  passwordContainsCapitalLetter(): boolean {
    const pass = this.password?.value ?? '';
    return /[A-Z]/.test(pass);
  }

  passwordContainsNumber(): boolean {
    const pass = this.password?.value ?? '';
    return /[0-9]/.test(pass);
  }

  passwordContainsSmallLetter(): boolean {
    const pass = this.password?.value ?? '';
    return /[a-z]/.test(pass);
  }

  passwordContainsSpecialChar(): boolean {
    const pass = this.password?.value ?? '';
    return /[\W_]/.test(pass);
  }

  validateEmailNotTaken(): AsyncValidatorFn {
    return (control: AbstractControl) => {
      if (!control.value) return of(null);
      return timer(500).pipe(
        switchMap(() =>
          this.accountService.emailExists(control.value).pipe(
            map((res: boolean): ValidationErrors | null =>
              res ? { emailExists: true } : null
            ),
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
            map((res: boolean): ValidationErrors | null =>
              res ? { usernameExists: true } : null
            ),
            catchError(() => of(null))
          )
        )
      );
    };
}


  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password')?.value;
    const confirmPasswordControl = control.get('confirmPassword');

    if (!password || !confirmPasswordControl) return null;

    if (password !== confirmPasswordControl.value) {
      confirmPasswordControl.setErrors({ passwordMismatch: true });
    } else {
      // Remove only passwordMismatch error, keep other errors
      if (confirmPasswordControl.hasError('passwordMismatch')) {
        const errors = { ...confirmPasswordControl.errors };
        delete errors['passwordMismatch'];
        confirmPasswordControl.setErrors(Object.keys(errors).length ? errors : null);
      }
    }
    return null;
  }

  onSubmit() {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    if (this.password?.value !== this.confirmPassword?.value) {
      this.confirmPassword?.setErrors({ mismatch: true });
      return;
    }

    this.isLoading = true;
    const formValue = this.registerForm.value;

    const registerData: IRegister = {
      email: formValue.email!,
      userName: formValue.userName!,
      firstName: formValue.firstName!,
      lastName: formValue.lastName!,
      gender: Number(formValue.gender),
      phoneNumber: formValue.phoneNumber!,
      password: formValue.password!,
      confirmPassword: formValue.confirmPassword!,
      roleName: formValue.roleName!
    };

    this.accountService.register(registerData).subscribe({
      next: () => {
        this.isLoading = false;
        this.router.navigate(['/']);
      },
      error: err => {
        this.isLoading = false;
        console.error('Registration failed:', err);
      }
    });
  }
}
