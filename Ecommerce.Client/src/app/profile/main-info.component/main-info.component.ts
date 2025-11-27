import { CommonModule, NgIf } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, inject, OnChanges, SimpleChanges } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { IProfile } from '../../shared/modules/profile';

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

  @Input() profile: IProfile | null = null;

  profileForm = this.fb.nonNullable.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    userName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: [''],
    gender: [''],
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['profile'] && this.profile) {
      // Patch fields individually
      this.profileForm.patchValue({
        firstName: this.profile.firstName,
        lastName: this.profile.lastName,
        userName: this.profile.userName,
        email: this.profile.email,
        phoneNumber: this.profile.phoneNumber || '',
        gender: this.profile.gender || '',
      });
      this.cdr.markForCheck();
    }
  }

  get controls() {
    return this.profileForm.controls;
  }

  // Example: patch a single field programmatically
  updateField(field: keyof IProfile, value: string) {
    this.profileForm.patchValue({ [field]: value });
    this.cdr.markForCheck();
  }

  // Optional: submit updated profile
  submit() {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }
    // const updatedProfile: IProfile = this.profileForm.getRawValue();
    // console.log('Updated profile:', updatedProfile);
    // Call your ProfileService.updateProfile(updatedProfile) here
  }
}
