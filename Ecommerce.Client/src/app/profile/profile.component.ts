import { AsyncPipe, CommonModule, NgFor, NgIf } from '@angular/common';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  ElementRef,
  OnInit,
  ViewChild,
} from '@angular/core';
import { BehaviorSubject, EMPTY, Observable, catchError, finalize, shareReplay, tap } from 'rxjs';
import { ProfileService } from '../shared/services/profile-service';
import { IProfile } from '../shared/modules/profile';
// Child components
import { AddressComponent } from './address.component/address.component';
import { ChangePasswordComponent } from './change-password.component/change-password.component';
import { DeleteProfileComponent } from './delete-profile.component/delete-profile.component';
import { MainInfoComponent } from './main-info.component/main-info.component';
import { SetPasswordComponent } from './set-password.component/set-password.component';
import { ToastrService } from 'ngx-toastr';

type ProfileSection =
  | 'main-info'
  | 'address'
  | 'change-password'
  | 'set-password'
  | 'delete-profile';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    NgIf,
    NgFor,
    AsyncPipe,
    MainInfoComponent,
    AddressComponent,
    ChangePasswordComponent,
    SetPasswordComponent,
    DeleteProfileComponent,
  ],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProfileComponent implements OnInit {
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  private profileSubject = new BehaviorSubject<IProfile | null>(null);
  profile$ = this.profileSubject.asObservable();

  selected: ProfileSection = 'main-info';
  isProfileLoading = true;
  profileError = '';

  // Profile picture upload states
  showEditIcon = false;
  isUploading = false;
  uploadProgress = 0;

  readonly sections: { id: ProfileSection; label: string; caption: string }[] = [
    { id: 'main-info', label: 'Overview', caption: 'Account basics' },
    { id: 'address', label: 'Address', caption: 'Shipping details' },
    { id: 'change-password', label: 'Change Password', caption: 'Update credentials' },
    { id: 'set-password', label: 'Set Password', caption: 'Create password' },
    { id: 'delete-profile', label: 'Delete Account', caption: 'Danger zone' },
  ];

  constructor(
    private profileService: ProfileService,
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadProfile();
  }

  retry(): void {
    this.loadProfile();
  }

  select(section: ProfileSection): void {
    this.selected = section;
  }

  // Profile picture upload methods
  triggerFileInput(): void {
    this.fileInput.nativeElement.click();
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      // Validate file type and size
      if (!this.validateFile(file)) {
        return;
      }

      this.uploadProfilePicture(file);
    }

    // Reset the file input
    event.target.value = '';
  }

  validateFile(file: File): boolean {
    const validTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
    const maxSize = 5 * 1024 * 1024; // 5MB

    if (!validTypes.includes(file.type)) {
      this.toastr.error('Please select a valid image file (JPEG, PNG, GIF, or WebP)');
      return false;
    }

    if (file.size > maxSize) {
      this.toastr.error('Image size must be less than 5MB');
      return false;
    }

    return true;
  }

  // In ProfileComponent, update the uploadProfilePicture method:
  uploadProfilePicture(file: File): void {
    this.isUploading = true;
    this.uploadProgress = 0;
    this.cdr.markForCheck();

    // Simulate progress (you can remove this if your API supports upload progress)
    const progressInterval = setInterval(() => {
      if (this.uploadProgress < 90) {
        this.uploadProgress += 10;
        this.cdr.markForCheck();
      }
    }, 150);

    // Use updateProfile instead of uploadProfilePicture
    this.profileService
      .updateProfile({ profileImageFile: file })
      .pipe(
        finalize(() => {
          clearInterval(progressInterval);
          this.isUploading = false;
          this.uploadProgress = 0;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: (updatedProfile) => {
          this.uploadProgress = 100;
          this.cdr.markForCheck();

          // Update the profile subject with new data
          this.profileSubject.next(updatedProfile);

          // Show success message
          this.toastr.success('Profile picture updated successfully!');
        },
        error: (error) => {
          console.error('Upload failed:', error);
          this.toastr.error(error.message || 'Failed to upload profile picture. Please try again.');
        },
      });
  }
  private loadProfile(): void {
    this.isProfileLoading = true;
    this.profileError = '';
    this.cdr.markForCheck();

    this.profileService
      .getProfile()
      .pipe(
        tap((profile) => {
          this.isProfileLoading = false;
          this.profileSubject.next(profile);
          this.cdr.markForCheck();
        }),
        catchError((error) => {
          this.isProfileLoading = false;
          this.profileError = 'Unable to load your profile data right now.';
          console.error('Profile load error:', error);
          this.cdr.markForCheck();
          return EMPTY;
        })
      )
      .subscribe();
  }
}
