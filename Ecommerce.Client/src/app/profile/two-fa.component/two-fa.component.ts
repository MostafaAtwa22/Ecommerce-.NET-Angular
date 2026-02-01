import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastrService } from 'ngx-toastr';
import { ProfileService } from '../../shared/services/profile-service';

@Component({
  selector: 'app-two-fa',
  imports: [CommonModule],
  templateUrl: './two-fa.component.html',
  styleUrl: './two-fa.component.scss',
})
export class TwoFaComponent implements OnInit {
  private profileService = inject(ProfileService);
  private toastr = inject(ToastrService);

  is2FAEnabled = false;

  ngOnInit(): void {
    this.load2FAStatus();
  }

  load2FAStatus(): void {
    this.profileService.get2FAStatus().subscribe({
      next: (status) => {
        this.is2FAEnabled = status;
      },
      error: (error) => {
        this.toastr.error('Failed to load 2FA status', 'Error');
        console.error('Error loading 2FA status:', error);
      },
    });
  }

  toggle2FA(): void {
    const newStatus = !this.is2FAEnabled;
    
    // Immediately update UI
    this.is2FAEnabled = newStatus;

    // Show toast notification immediately
    if (newStatus) {
      this.toastr.success(
        'Two-factor authentication has been enabled successfully',
        'Security Enhanced',
        { timeOut: 4000 }
      );
    } else {
      this.toastr.warning(
        'Two-factor authentication has been disabled',
        'Security Warning',
        { timeOut: 4000 }
      );
    }

    // Call API in background
    this.profileService.toggle2FA(newStatus).subscribe({
      next: (message) => {
        // API call successful, state already updated
      },
      error: (error) => {
        this.is2FAEnabled = !newStatus;
        
        this.toastr.error(
          'Failed to update 2FA settings. Please try again.',
          'Error'
        );
        console.error('Error toggling 2FA:', error);
      },
    });
  }
}