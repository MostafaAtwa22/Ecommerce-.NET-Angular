import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProfileService } from '../../shared/services/profile-service';
import { ToastrService } from 'ngx-toastr';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-two-fa',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './two-fa.component.html',
  styleUrl: './two-fa.component.scss',
})
export class TwoFAComponent implements OnInit {
  is2FAEnabled = false;
  loading = false;

  constructor(
    private profileService: ProfileService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadStatus();
  }

  loadStatus() {
    this.profileService.get2FAStatus().subscribe({
      next: (status) => (this.is2FAEnabled = status),
      error: () => this.toastr.error('Failed to load 2FA status', 'Error'),
    });
  }

  async toggle2FA() {
    const action = this.is2FAEnabled ? 'disable' : 'enable';
    const actionTitle = this.is2FAEnabled ? 'Disable' : 'Enable';
    
    const result = await Swal.fire({
      title: `${actionTitle} Two-Factor Authentication?`,
      html: this.is2FAEnabled 
        ? '<p>Disabling 2FA will make your account less secure.</p><p>Are you sure you want to continue?</p>'
        : '<p>Enabling 2FA will add an extra layer of security to your account.</p><p>You will need a verification code each time you log in.</p>',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: this.is2FAEnabled ? '#ef4444' : '#5624d0',
      cancelButtonColor: '#6a6f73',
      confirmButtonText: `Yes, ${action} it!`,
      cancelButtonText: 'Cancel',
      reverseButtons: true,
      customClass: {
        popup: 'swal2-custom-popup',
        title: 'swal2-custom-title',
        htmlContainer: 'swal2-custom-html',
        confirmButton: 'swal2-custom-confirm',
        cancelButton: 'swal2-custom-cancel'
      }
    });

    if (!result.isConfirmed) {
      return;
    }

    this.loading = true;

    this.profileService.enable2FA(!this.is2FAEnabled).subscribe({
      next: (msg) => {
        this.is2FAEnabled = !this.is2FAEnabled;
        
        Swal.fire({
          title: 'Success!',
          text: msg || `Two-Factor Authentication has been ${this.is2FAEnabled ? 'enabled' : 'disabled'} successfully.`,
          icon: 'success',
          confirmButtonColor: '#5624d0',
          timer: 3000,
          timerProgressBar: true,
          customClass: {
            popup: 'swal2-custom-popup',
            title: 'swal2-custom-title',
            confirmButton: 'swal2-custom-confirm'
          }
        });
        
        this.toastr.success(
          `Two-Factor Authentication ${this.is2FAEnabled ? 'enabled' : 'disabled'}`,
          'Success'
        );
        this.loading = false;
      },
      error: (err) => {
        const errorMsg = err?.error?.message || 'Failed to update 2FA settings';
        
        Swal.fire({
          title: 'Error!',
          text: errorMsg,
          icon: 'error',
          confirmButtonColor: '#5624d0',
          customClass: {
            popup: 'swal2-custom-popup',
            title: 'swal2-custom-title',
            confirmButton: 'swal2-custom-confirm'
          }
        });
        
        this.toastr.error(errorMsg, 'Error');
        this.loading = false;
      },
    });
  }
}
