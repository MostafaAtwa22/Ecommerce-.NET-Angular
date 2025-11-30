import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, inject, OnChanges, SimpleChanges } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors, AsyncValidatorFn } from '@angular/forms';
import { IProfile, IProfileUpdate } from '../../shared/modules/profile';
import { ProfileService } from '../../shared/services/profile-service';
import { AccountService } from '../../account/account-service';
import { ToastrService } from 'ngx-toastr';
import { finalize, timer, map, switchMap, catchError, of } from 'rxjs';

type EditableField = 'firstName' | 'lastName' | 'userName' | 'email' | 'phoneNumber' | 'gender';

interface FieldConfig {
  key: EditableField;
  label: string;
  icon: string;
  hint: string;
  type: 'text' | 'email' | 'tel' | 'select';
  validators?: any[];
  asyncValidators?: AsyncValidatorFn[];
  options?: { value: string; label: string }[];
}

@Component({
  selector: 'app-main-info',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './main-info.component.html',
  styleUrl: './main-info.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MainInfoComponent implements OnChanges {
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly profileService = inject(ProfileService);
  private readonly accountService = inject(AccountService);
  private readonly toastr = inject(ToastrService);

  @Input() profile: IProfile | null = null;

  editingField: EditableField | null = null;
  loadingFields = new Set<EditableField>();

  readonly fieldConfigs: FieldConfig[] = [
    {
      key: 'firstName',
      label: 'First Name',
      icon: 'fa-user',
      hint: 'Your legal first name',
      type: 'text',
      validators: [Validators.required, Validators.pattern(/^[A-Za-z]+$/), Validators.minLength(2)]
    },
    {
      key: 'lastName',
      label: 'Last Name',
      icon: 'fa-user',
      hint: 'Your legal last name',
      type: 'text',
      validators: [Validators.required, Validators.pattern(/^[A-Za-z]+$/), Validators.minLength(2)]
    },
    {
      key: 'userName',
      label: 'Username',
      icon: 'fa-at',
      hint: 'This will be your public display name',
      type: 'text',
      validators: [Validators.required, Validators.pattern(/^[a-zA-Z0-9_]+$/), Validators.minLength(3)],
      asyncValidators: [this.validateUsernameNotTaken()]
    },
    {
      key: 'email',
      label: 'Email Address',
      icon: 'fa-envelope',
      hint: "We'll never share your email with anyone else",
      type: 'email',
      validators: [Validators.required, Validators.email],
      asyncValidators: [this.validateEmailNotTaken()]
    },
    {
      key: 'phoneNumber',
      label: 'Phone Number',
      icon: 'fa-phone',
      hint: 'Include country code for international numbers',
      type: 'tel',
      validators: [Validators.pattern(/^[0-9+\-() ]+$/)]
    },
    {
      key: 'gender',
      label: 'Gender',
      icon: 'fa-venus-mars',
      hint: 'For personalization purposes only',
      type: 'select',
      validators: [],
      options: [
        { value: '', label: 'Select your gender' },
        { value: 'Male', label: 'Male' },
        { value: 'Female', label: 'Female' }
      ]
    }
  ];

  profileForm = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, Validators.pattern(/^[A-Za-z]+$/), Validators.minLength(2)]],
    lastName: ['', [Validators.required, Validators.pattern(/^[A-Za-z]+$/), Validators.minLength(2)]],
    userName: ['',
      [Validators.required, Validators.pattern(/^[a-zA-Z0-9_]+$/), Validators.minLength(3)],
      [this.validateUsernameNotTaken()]
    ],
    email: ['',
      [Validators.required, Validators.email],
      [this.validateEmailNotTaken()]
    ],
    phoneNumber: ['', [Validators.pattern(/^[0-9+\-() ]+$/)]],
    gender: [''],
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['profile'] && this.profile) {
      this.updateFormWithProfileData();
    }
  }

  private updateFormWithProfileData(): void {
    if (!this.profile) return;

    this.profileForm.patchValue({
      firstName: this.profile.firstName || '',
      lastName: this.profile.lastName || '',
      userName: this.profile.userName || '',
      email: this.profile.email || '',
      phoneNumber: this.profile.phoneNumber || '',
      gender: this.profile.gender || '',
    });

    this.disableAllFields();
    this.cdr.markForCheck();
  }

  // Username validation (exclude current user's username)
  private validateUsernameNotTaken(): AsyncValidatorFn {
    return (control: AbstractControl) => {
      if (!control.value || control.value === this.profile?.userName) {
        return of(null);
      }

      return timer(800).pipe( // Increased delay for better UX
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

  // Email validation (exclude current user's email)
  private validateEmailNotTaken(): AsyncValidatorFn {
    return (control: AbstractControl) => {
      if (!control.value || control.value === this.profile?.email) {
        return of(null);
      }

      return timer(800).pipe( // Increased delay for better UX
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

  isEditing(field: EditableField): boolean {
    return this.editingField === field;
  }

  isLoading(field: EditableField): boolean {
    return this.loadingFields.has(field);
  }

  isDisabled(field: EditableField): boolean {
    return this.profileForm.get(field)?.disabled ?? false;
  }

  isFieldPending(field: EditableField): boolean {
    return this.profileForm.get(field)?.pending ?? false;
  }

  startEdit(field: EditableField): void {
    if (this.editingField && this.editingField !== field) {
      this.cancelEdit(this.editingField);
    }

    this.editingField = field;
    this.profileForm.get(field)?.enable();
    this.cdr.markForCheck();
  }

  cancelEdit(field: EditableField): void {
    if (this.profile) {
      const originalValue = this.profile[field] || '';
      this.profileForm.get(field)?.setValue(originalValue as any);
    }

    this.profileForm.get(field)?.disable();
    this.editingField = null;
    this.cdr.markForCheck();
  }

  saveField(field: EditableField): void {
    const control = this.profileForm.get(field);

    if (!control || control.invalid || control.pending) {
      control?.markAsTouched();

      if (control?.pending) {
        this.toastr.warning('Please wait while we validate your input');
        return;
      }

      if (control?.errors) {
        const errorMessage = this.getErrorMessage(field);
        this.toastr.error(errorMessage || 'Please enter a valid value');
      }
      return;
    }

    const newValue = control.value;
    const oldValue = this.profile?.[field] || '';

    // Check if value actually changed
    if (newValue === oldValue) {
      this.cancelEdit(field);
      this.toastr.info('No changes detected');
      return;
    }

    // Prepare update payload
    const updatePayload: IProfileUpdate = {
      [field]: newValue || undefined
    };

    // Add loading state
    this.loadingFields.add(field);
    control.disable();
    this.cdr.markForCheck();

    // Call API
    this.profileService.updateProfile(updatePayload)
      .pipe(
        finalize(() => {
          this.loadingFields.delete(field);
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: (updatedProfile) => {
          // Update local profile data
          if (this.profile) {
            Object.assign(this.profile, updatedProfile);
            this.updateFormWithProfileData();
            this.updateLocalStorageUser(updatedProfile);
          }

          // Reset editing state
          this.editingField = null;

          this.toastr.success(`${this.getFieldConfig(field)?.label} updated successfully`);
          this.cdr.markForCheck();
        },
        error: (error) => {
          // Revert to original value on error
          if (this.profile) {
            this.profileForm.patchValue({ [field]: oldValue });
          }

          // Re-enable the field for editing
          control.enable();

          const errorMessage = error.message || 'Failed to update profile';
          this.toastr.error(errorMessage);

          console.error(`Failed to update ${field}:`, error);
          this.cdr.markForCheck();
        }
      });
  }

  getFieldConfig(field: EditableField): FieldConfig | undefined {
    return this.fieldConfigs.find(f => f.key === field);
  }

  getControl(field: EditableField): FormControl {
    return this.profileForm.get(field) as FormControl;
  }

  hasError(field: EditableField): boolean {
    const control = this.profileForm.get(field);
    return !!(control?.invalid && control?.touched);
  }

  getErrorMessage(field: EditableField): string {
    const control = this.profileForm.get(field);

    if (!control?.errors) return '';

    if (control.errors['required']) {
      return `${this.getFieldConfig(field)?.label} is required`;
    }

    if (control.errors['email']) {
      return 'Please enter a valid email address';
    }

    if (control.errors['pattern']) {
      if (field === 'userName') {
        return 'Only letters, numbers, and underscores allowed';
      }
      if (field === 'firstName' || field === 'lastName') {
        return 'Only letters allowed';
      }
      if (field === 'phoneNumber') {
        return 'Please enter a valid phone number';
      }
    }

    if (control.errors['minlength']) {
      const minLength = control.errors['minlength'].requiredLength;
      return `Must be at least ${minLength} characters`;
    }

    if (control.errors['emailExists']) {
      return 'This email is already taken';
    }

    if (control.errors['usernameExists']) {
      return 'This username is already taken';
    }

    return 'Invalid value';
  }

  private disableAllFields(): void {
    Object.keys(this.profileForm.controls).forEach(key => {
      this.profileForm.get(key)?.disable();
    });
  }

  private updateLocalStorageUser(updatedProfile: IProfile): void {
    try {
      const userStr = localStorage.getItem('user');
      if (!userStr) {
        console.warn('No user data in localStorage to update');
        return;
      }

      const userData = JSON.parse(userStr);

      const updatedUserData = {
        ...userData,
        firstName: updatedProfile.firstName,
        lastName: updatedProfile.lastName,
        userName: updatedProfile.userName,
        gender: updatedProfile.gender,
        email: updatedProfile.email,
        profilePicture: updatedProfile.profilePicture
      };

      localStorage.setItem('user', JSON.stringify(updatedUserData));
      this.accountService.loadCurrentUser();

      console.log('LocalStorage user data updated successfully');
    } catch (error) {
      console.error('Failed to update localStorage user data:', error);
    }
  }
}
