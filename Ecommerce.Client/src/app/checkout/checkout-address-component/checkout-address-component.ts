import { CommonModule } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ProfileService } from '../../shared/services/profile-service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-checkout-address-component',
  templateUrl: './checkout-address-component.html',
  styleUrls: ['./checkout-address-component.scss'],
  imports: [ReactiveFormsModule, CommonModule],
  standalone: true,
})
export class CheckoutAddressComponent {
  @Input() addressForm!: FormGroup;
  private profileService = inject(ProfileService);
  private toastr = inject(ToastrService);

  saveAddress() {
    this.addressForm.markAllAsTouched(); // show validation errors

    if (this.addressForm.invalid) {
      this.toastr.error('Please fill all required fields correctly.');
      return;
    }
    const data = this.addressForm.getRawValue();
    delete data.firstName;
    delete data.lastName;

    this.profileService.updateAddress(data).subscribe({
      next: () => {
        this.toastr.success('Address saved successfully!');
      },
      error: () => {
        this.toastr.error('Failed to save address.');
      }
    });
  }
}
