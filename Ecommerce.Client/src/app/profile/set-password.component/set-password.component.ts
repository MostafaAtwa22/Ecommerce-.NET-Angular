import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import {
  ReactiveFormsModule,
  FormBuilder,
  Validators,
  ValidationErrors,
  ValidatorFn,
} from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { ProfileService } from '../../shared/services/profile-service';

@Component({
  selector: 'app-set-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './set-password.component.html',
  styleUrl: './set-password.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SetPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly profileService = inject(ProfileService);

  private readonly passwordsMatchValidator: ValidatorFn = (group): ValidationErrors | null => {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    if (!password || !confirmPassword) {
      return null;
    }
    return password === confirmPassword ? null : { passwordMismatch: true };
  };

  setPasswordForm = this.fb.nonNullable.group(
    {
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', Validators.required],
    },
    { validators: this.passwordsMatchValidator }
  );

  // Password visibility states
  showPassword = false;
  showConfirmPassword = false;

  // Password strength properties
  passwordStrength: 'weak' | 'medium' | 'strong' = 'weak';
  hasMinLength = false;
  hasUpperCase = false;
  hasLowerCase = false;
  hasNumber = false;
  hasSpecialChar = false;

  saving = false;
  successMessage = '';
  errorMessage = '';

  get controls() {
    return this.setPasswordForm.controls;
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  checkPasswordStrength(): void {
    const password = this.controls.password.value;

    if (!password) {
      this.resetPasswordStrength();
      return;
    }

    // Check individual requirements
    this.hasMinLength = password.length >= 8;
    this.hasUpperCase = /[A-Z]/.test(password);
    this.hasLowerCase = /[a-z]/.test(password);
    this.hasNumber = /[0-9]/.test(password);
    this.hasSpecialChar = /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password);

    // Calculate strength score
    let strengthScore = 0;
    if (this.hasMinLength) strengthScore++;
    if (this.hasUpperCase) strengthScore++;
    if (this.hasLowerCase) strengthScore++;
    if (this.hasNumber) strengthScore++;
    if (this.hasSpecialChar) strengthScore++;

    // Determine strength level
    if (strengthScore <= 2) {
      this.passwordStrength = 'weak';
    } else if (strengthScore <= 4) {
      this.passwordStrength = 'medium';
    } else {
      this.passwordStrength = 'strong';
    }
  }

  getPasswordStrengthText(): string {
    switch (this.passwordStrength) {
      case 'weak': return 'Weak';
      case 'medium': return 'Medium';
      case 'strong': return 'Strong';
      default: return 'Weak';
    }
  }

  private resetPasswordStrength(): void {
    this.passwordStrength = 'weak';
    this.hasMinLength = false;
    this.hasUpperCase = false;
    this.hasLowerCase = false;
    this.hasNumber = false;
    this.hasSpecialChar = false;
  }

  submit(): void {
    if (this.setPasswordForm.invalid) {
      this.setPasswordForm.markAllAsTouched();
      return;
    }

    this.saving = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.profileService
      .setPassword(this.setPasswordForm.getRawValue())
      .pipe(
        finalize(() => {
          this.saving = false;
        })
      )
      .subscribe({
        next: () => {
          this.successMessage = 'Password created successfully. You can now use it to log in.';
          this.setPasswordForm.reset();
          this.resetPasswordStrength();
        },
        error: () => {
          this.errorMessage = 'Unable to create password. Please try again.';
        },
      });
  }
}
