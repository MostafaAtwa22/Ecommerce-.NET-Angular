import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, inject, OnChanges, SimpleChanges } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { IProfile, IProfileUpdate } from '../../shared/modules/profile';
import { ProfileService } from '../../shared/services/profile-service';
import { AccountService } from '../../account/account-service';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs';

type EditableField = 'firstName' | 'lastName' | 'userName' | 'email' | 'phoneNumber' | 'gender';

interface FieldConfig {
  key: EditableField;
  label: string;
  icon: string;
  hint: string;
  type: 'text' | 'email' | 'tel' | 'select';
  validators?: any[];
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

  // Track which field is currently being edited
  editingField: EditableField | null = null;

  // Track loading state for each field
  loadingFields = new Set<EditableField>();

  // Field configurations
  readonly fieldConfigs: FieldConfig[] = [
    {
      key: 'firstName',
      label: 'First Name',
      icon: 'fa-user',
      hint: 'Your legal first name',
      type: 'text',
      validators: [Validators.required, Validators.minLength(2)]
    },
    {
      key: 'lastName',
      label: 'Last Name',
      icon: 'fa-user',
      hint: 'Your legal last name',
      type: 'text',
      validators: [Validators.required, Validators.minLength(2)]
    },
    {
      key: 'userName',
      label: 'Username',
      icon: 'fa-at',
      hint: 'This will be your public display name',
      type: 'text',
      validators: [Validators.required, Validators.minLength(3)]
    },
    {
      key: 'email',
      label: 'Email Address',
      icon: 'fa-envelope',
      hint: "We'll never share your email with anyone else",
      type: 'email',
      validators: [Validators.required, Validators.email]
    },
    {
      key: 'phoneNumber',
      label: 'Phone Number',
      icon: 'fa-phone',
      hint: 'Include country code for international numbers',
      type: 'tel',
      validators: []
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
    firstName: ['', [Validators.required, Validators.minLength(2)]],
    lastName: ['', [Validators.required, Validators.minLength(2)]],
    userName: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: [''],
    gender: [''],
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['profile'] && this.profile) {
      this.updateFormWithProfileData();
      this.profileForm.get('email')?.disable(); 
    }
  }

  /**
   * Check if a field is currently being edited
   */
  isEditing(field: EditableField): boolean {
    return this.editingField === field;
  }

  /**
   * Check if a field is currently loading
   */
  isLoading(field: EditableField): boolean {
    return this.loadingFields.has(field);
  }

  /**
   * Check if a field control is disabled
   */
  isDisabled(field: EditableField): boolean {
    return this.profileForm.get(field)?.disabled ?? false;
  }

  /**
   * Start editing a field
   */
  startEdit(field: EditableField): void {
    // Cancel any other editing field first
    if (this.editingField && this.editingField !== field) {
      this.cancelEdit(this.editingField);
    }

    this.editingField = field;
    this.profileForm.get(field)?.enable();
    this.cdr.markForCheck();
  }

  /**
   * Cancel editing and revert to original value
   */
  cancelEdit(field: EditableField): void {
    if (this.profile) {
      // Revert to original value
      this.profileForm.patchValue({
        [field]: this.profile[field] || ''
      });
    }

    this.profileForm.get(field)?.disable();
    this.editingField = null;
    this.cdr.markForCheck();
  }

  /**
   * Save a single field
   */
  saveField(field: EditableField): void {
    const control = this.profileForm.get(field);

    if (!control || control.invalid) {
      control?.markAsTouched();
      this.toastr.error('Please enter a valid value');
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
          // Update local profile data with the response from server
          if (this.profile) {
            // Update the entire profile object with server response
            Object.assign(this.profile, updatedProfile);

            // Update the form with the new profile data
            this.updateFormWithProfileData();

            // Update localStorage to persist the changes
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

    // Disable all fields initially
    this.disableAllFields();
    this.cdr.markForCheck();
  }

  /**
   * Get field configuration
   */
  getFieldConfig(field: EditableField): FieldConfig | undefined {
    return this.fieldConfigs.find(f => f.key === field);
  }

  getControl(field: EditableField): FormControl {
    return this.profileForm.get(field) as FormControl;
  }

  /**
   * Check if a field has errors
   */
  hasError(field: EditableField): boolean {
    const control = this.profileForm.get(field);
    return !!(control?.invalid && control?.touched);
  }

  /**
   * Get error message for a field
   */
  getErrorMessage(field: EditableField): string {
    const control = this.profileForm.get(field);

    if (!control?.errors) return '';

    if (control.errors['required']) {
      return `${this.getFieldConfig(field)?.label} is required`;
    }

    if (control.errors['email']) {
      return 'Please enter a valid email address';
    }

    if (control.errors['minlength']) {
      const minLength = control.errors['minlength'].requiredLength;
      return `Must be at least ${minLength} characters`;
    }

    return 'Invalid value';
  }

  /**
   * Disable all form fields
   */
  private disableAllFields(): void {
    Object.keys(this.profileForm.controls).forEach(key => {
      this.profileForm.get(key)?.disable();
    });
  }

  /**
   * Update localStorage user data to persist profile changes
   * This ensures changes persist after page refresh
   */
  private updateLocalStorageUser(updatedProfile: IProfile): void {
    try {
      const userStr = localStorage.getItem('user');
      if (!userStr) {
        console.warn('No user data in localStorage to update');
        return;
      }

      const userData = JSON.parse(userStr);

      // Update the user object with the new profile data
      const updatedUserData = {
        ...userData,
        firstName: updatedProfile.firstName,
        lastName: updatedProfile.lastName,
        userName: updatedProfile.userName,
        gender: updatedProfile.gender,
        email: updatedProfile.email,
        profilePicture: updatedProfile.profilePicture
      };

      // Save back to localStorage
      localStorage.setItem('user', JSON.stringify(updatedUserData));

      // Update the AccountService signal to reflect the changes immediately
      this.accountService.loadCurrentUser();

      console.log('LocalStorage user data updated successfully');
    } catch (error) {
      console.error('Failed to update localStorage user data:', error);
    }
  }
}
