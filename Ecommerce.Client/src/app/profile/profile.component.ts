import { AsyncPipe, CommonModule, NgFor, NgIf } from '@angular/common';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  ElementRef,
  OnInit,
  ViewChild,
} from '@angular/core';
import { BehaviorSubject, EMPTY, catchError, finalize, tap } from 'rxjs';
import { ProfileService } from '../shared/services/profile-service';
import { IProfile } from '../shared/modules/profile';
import { AddressComponent } from './address.component/address.component';
import { ChangePasswordComponent } from './change-password.component/change-password.component';
import { DeleteProfileComponent } from './delete-profile.component/delete-profile.component';
import { MainInfoComponent } from './main-info.component/main-info.component';
import { SetPasswordComponent } from './set-password.component/set-password.component';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../account/account-service';

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

  showEditOptions = false;
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
    private accountService: AccountService,
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

  triggerFileInput(): void {
    this.fileInput.nativeElement.click();
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (file && this.validateFile(file)) {
      this.uploadProfilePicture(file);
    }

    input.value = '';
  }

  validateFile(file: File): boolean {
    const validTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
    const maxSize = 5 * 1024 * 1024;

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

  uploadProfilePicture(file: File): void {
    this.isUploading = true;
    this.uploadProgress = 0;
    this.showEditOptions = false;
    this.cdr.markForCheck();

    // Slower, more realistic progress simulation
    const progressInterval = setInterval(() => {
      if (this.uploadProgress < 80) { // Stop at 80% and let the actual upload complete
        this.uploadProgress += 5; // Slower increment
        this.cdr.markForCheck();
      }
    }, 200); // Slower interval

    this.profileService
      .updateProfileImage(file)
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
          // Show 100% when complete
          this.uploadProgress = 100;
          this.profileSubject.next(updatedProfile);

          // Update local storage user
          this.accountService.updateLocalUserProfilePicture(updatedProfile.profilePicture);

          this.toastr.success('Profile picture updated successfully!');
          this.cdr.markForCheck();

          // Keep progress at 100% for a moment before hiding
          setTimeout(() => {
            this.uploadProgress = 0;
            this.cdr.markForCheck();
          }, 1000);
        },
        error: (error) => {
          console.error('Upload failed:', error);
          this.toastr.error(error.message || 'Failed to upload profile picture');
        },
      });
  }

  removeProfileImage(): void {
    const currentProfile = this.profileSubject.value;

    if (!currentProfile?.profilePicture) {
      this.toastr.info('No profile picture to remove');
      return;
    }

    this.showEditOptions = false;
    this.cdr.markForCheck();

    this.profileService
      .deleteProfileImage()
      .pipe(
        tap((updatedProfile) => {
          this.profileSubject.next(updatedProfile);

          // Update local storage user - remove image
          this.accountService.clearLocalUserProfilePicture();

          this.toastr.success('Profile picture removed successfully');
          this.cdr.markForCheck();
        }),
        catchError((error) => {
          console.error('Remove failed:', error);
          this.toastr.error(error.message || 'Failed to remove profile picture');
          return EMPTY;
        })
      )
      .subscribe();
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
