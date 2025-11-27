import { CommonModule, NgIf, NgFor } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, DestroyRef, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize, takeUntil } from 'rxjs/operators';
import { ProfileService } from '../../shared/services/profile-service';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-address',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NgIf, NgFor],
  templateUrl: './address.component.html',
  styleUrl: './address.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddressComponent implements OnInit {

  private readonly fb = inject(FormBuilder);
  private readonly profileService = inject(ProfileService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);

  private readonly destroy$ = new Subject<void>();

  addressForm = this.fb.nonNullable.group({
    country: ['', Validators.required],
    government: ['', Validators.required],
    city: ['', Validators.required],
    street: ['', Validators.required],
    zipcode: ['', [Validators.required, Validators.minLength(3)]],
  });

  loading = true;
  saving = false;
  loadError = '';
  successMessage = '';
  errorMessage = '';

  ngOnInit(): void {
    this.loadAddress();

    // Auto-complete cleanup
    this.destroyRef.onDestroy(() => {
      this.destroy$.next();
      this.destroy$.complete();
    });
  }

  get controls() {
    return this.addressForm.controls;
  }

  refresh(): void {
    this.loadAddress();
  }

  submit(): void {
    if (this.addressForm.invalid) {
      this.addressForm.markAllAsTouched();
      return;
    }

    this.saving = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.profileService
      .updateAddress(this.addressForm.getRawValue())
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.saving = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: (updated) => {
          this.successMessage = 'Address updated successfully.';
          this.addressForm.patchValue(updated);
          this.cdr.markForCheck();
        },
        error: () => {
          this.errorMessage = 'Unable to update your address. Please try again.';
          this.cdr.markForCheck();
        },
      });
  }

  private loadAddress(): void {
    this.loading = true;
    this.loadError = '';

    this.profileService
      .getAddress()
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.loading = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: (address) => {
          if (address) {
            this.addressForm.patchValue(address);
          }
          this.cdr.markForCheck();
        },
        error: () => {
          this.loadError = 'Unable to load your saved address.';
          this.cdr.markForCheck();
        },
      });
  }
}
