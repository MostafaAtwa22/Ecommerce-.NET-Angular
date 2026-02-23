import { CommonModule, NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { ProfileService } from '../../shared/services/profile-service';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';
import { AccountService } from '../../account/account-service';
import { BasketService } from '../../shared/services/basket-service';

@Component({
  selector: 'app-delete-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './delete-profile.component.html',
  styleUrl: './delete-profile.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DeleteProfileComponent {
  private readonly fb = inject(FormBuilder);
  private readonly profileService = inject(ProfileService);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);
  private readonly accountService = inject(AccountService);

  deleteForm = this.fb.nonNullable.group({
    password: ['', Validators.required],
    confirmation: ['', Validators.required],
  });

  saving = false;
  successMessage = '';
  errorMessage = '';
  showFinalConfirm = false;

  get controls() {
    return this.deleteForm.controls;
  }

  get isConfirmed(): boolean {
    return this.controls.confirmation.value?.toUpperCase() === 'DELETE';
  }

  submit(): void {
    if (this.deleteForm.invalid || !this.isConfirmed) {
      this.deleteForm.markAllAsTouched();

      // Add visual feedback for invalid form
      if (!this.isConfirmed) {
        this.shakeConfirmationField();
      }

      return;
    }

    // Show final confirmation modal instead of immediate deletion
    this.showFinalConfirm = true;
  }

  confirmDelete(): void {
    this.saving = true;
    this.successMessage = '';
    this.errorMessage = '';
    this.showFinalConfirm = false;

    const { password } = this.deleteForm.getRawValue();

    this.profileService
      .deleteProfile({ password })
      .pipe(
        finalize(() => {
          this.saving = false;
        })
      )
      .subscribe({
        next: () => {
          this.successMessage = 'Your account has been scheduled for deletion. You will be logged out shortly.';
          this.deleteForm.reset();
          this.toastr.success('Account deletion scheduled successfully', 'Goodbye!', {
            timeOut: 5000,
            progressBar: true,
            closeButton: true
          });

          setTimeout(() => {
            this.accountService.logout();
            this.router.navigate(['/']);
          }, 3000);
        },
        error: (error) => {
          console.error('Delete account error:', error);
          this.errorMessage = 'Unable to delete account. Please verify your password and try again.';
          this.toastr.error('Failed to delete account. Please check your password.', 'Error', {
            timeOut: 5000,
            progressBar: true,
            closeButton: true
          });
        },
      });
  }

  cancelDelete(): void {
    this.showFinalConfirm = false;
    this.toastr.info('Account deletion cancelled', 'Cancelled', {
      timeOut: 3000,
      progressBar: true
    });
  }

  private shakeConfirmationField(): void {
    const confirmationInput = document.querySelector('.confirmation-input');
    if (confirmationInput) {
      confirmationInput.classList.add('shake');
      setTimeout(() => {
        confirmationInput.classList.remove('shake');
      }, 500);
    }
  }

  // Helper method to check if form is pristine
  get isFormPristine(): boolean {
    return this.deleteForm.pristine;
  }

  // Helper method to check if form has been touched but is invalid
  get showValidationErrors(): boolean {
    return this.deleteForm.touched && this.deleteForm.invalid;
  }
}
