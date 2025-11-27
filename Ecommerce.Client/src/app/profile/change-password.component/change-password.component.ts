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
  selector: 'app-change-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './change-password.component.html',
  styleUrl: './change-password.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChangePasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly profileService = inject(ProfileService);

  private readonly passwordsMatchValidator: ValidatorFn = (group): ValidationErrors | null => {
    const newPassword = group.get('newPassword')?.value;
    const confirmPassword = group.get('confirmNewPassword')?.value;
    if (!newPassword || !confirmPassword) {
      return null;
    }
    return newPassword === confirmPassword ? null : { passwordMismatch: true };
  };

  passwordForm = this.fb.nonNullable.group(
    {
      oldPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmNewPassword: ['', Validators.required],
    },
    { validators: this.passwordsMatchValidator }
  );

  // Password visibility states
  showOldPassword = false;
  showNewPassword = false;
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
    return this.passwordForm.controls;
  }

  togglePasswordVisibility(field: 'oldPassword' | 'newPassword' | 'confirmNewPassword'): void {
    switch (field) {
      case 'oldPassword':
        this.showOldPassword = !this.showOldPassword;
        break;
      case 'newPassword':
        this.showNewPassword = !this.showNewPassword;
        break;
      case 'confirmNewPassword':
        this.showConfirmPassword = !this.showConfirmPassword;
        break;
    }
  }

  checkPasswordStrength(): void {
    const password = this.controls.newPassword.value;

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
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }

    this.saving = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.profileService
      .changePassword(this.passwordForm.getRawValue())
      .pipe(
        finalize(() => {
          this.saving = false;
        })
      )
      .subscribe({
        next: () => {
          this.successMessage = 'Password updated successfully.';
          this.passwordForm.reset();
          this.resetPasswordStrength();
        },
        error: () => {
          this.errorMessage = 'Unable to update password. Double-check your current password.';
        },
      });
  }
}
