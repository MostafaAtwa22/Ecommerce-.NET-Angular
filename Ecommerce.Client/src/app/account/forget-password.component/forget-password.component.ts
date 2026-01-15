import { Component, inject, OnDestroy } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
  AbstractControl,
  ValidationErrors,
  AsyncValidatorFn
} from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { Observable, of, timer, Subscription } from 'rxjs';
import { map, switchMap, catchError } from 'rxjs/operators';
import { IForgetPassword } from '../../shared/modules/accountUser';
import { AccountService } from '../account-service';
import { AnimatedOverlayComponent } from "../animated-overlay-component/animated-overlay-component";

@Component({
  selector: 'app-forget-password.component',
  imports: [ReactiveFormsModule, CommonModule, AnimatedOverlayComponent, RouterLink],
  templateUrl: './forget-password.component.html',
  styleUrls: ['./forget-password.component.scss'],
})
export class ForgetPasswordComponent implements OnDestroy {
  private fb = inject(FormBuilder);
  private accountService = inject(AccountService);
  private router = inject(Router);

  forgetForm: FormGroup;
  submitted = false;
  successMessage = '';
  errorMessage = '';
  loading = false;
  showRedirectMessage = false;
  countdown = 5;
  showAlternativeMethod = false;

  private countdownSubscription?: Subscription;
  private resetToken: string = '';

  constructor() {
    this.forgetForm = this.fb.group({
      email: [
        '',
        [Validators.required, Validators.email],
        [this.validateEmailExists()]
      ],
    });
  }

  get email() {
    return this.forgetForm.get('email');
  }

  get f() {
    return this.forgetForm.controls;
  }

  // Custom async validator to check if email exists
  validateEmailExists(): AsyncValidatorFn {
    return (control: AbstractControl): Observable<ValidationErrors | null> => {
      if (!control.value || control.errors?.['email']) {
        return of(null);
      }

      return timer(500).pipe(
        switchMap(() =>
          this.accountService.emailExists(control.value).pipe(
            map((exists: boolean) =>
              exists ? null : { emailNotFound: true }
            ),
            catchError(() => of({ emailNotFound: true }))
          )
        )
      );
    };
  }

  onSubmit() {
    this.submitted = true;
    this.successMessage = '';
    this.errorMessage = '';

    if (this.forgetForm.invalid) {
      this.forgetForm.markAllAsTouched();
      return;
    }

    this.loading = true;

    const data: IForgetPassword = {
      email: this.email?.value,
    };

    this.accountService.forgetPassword(data).subscribe({
      next: (response) => {
        this.loading = false;

        // Navigate directly to check-inbox page
        this.router.navigate(['/check-inbox'], {
          queryParams: {
            email: this.email?.value
          }
        });
      },
      error: (err) => {
        this.errorMessage = this.getErrorMessage(err);
        this.loading = false;
      },
    });
  }

  private startAutoRedirect() {
    this.showRedirectMessage = true;
    this.countdown = 5;

    if (this.countdownSubscription) {
      this.countdownSubscription.unsubscribe();
    }

    this.countdownSubscription = timer(0, 1000).subscribe({
      next: () => {
        if (this.countdown > 0) {
          this.countdown--;
        } else {
          this.navigateToResetPassword();
        }
      }
    });
  }

  navigateToResetPassword() {
    if (this.countdownSubscription) {
      this.countdownSubscription.unsubscribe();
      this.countdownSubscription = undefined;
    }

    const queryParams: any = {
      email: this.email?.value
    };

    if (this.resetToken) {
      queryParams.token = this.resetToken;
    }

    this.router.navigate(['/checkinbox'], {
      queryParams: queryParams,
      state: {
        email: this.email?.value,
        token: this.resetToken
      }
    });
  }

  navigateToSecurityQuestions() {
    this.router.navigate(['/security-questions'], {
      queryParams: { email: this.email?.value }
    });
  }

  private getErrorMessage(error: any): string {
    if (error.status === 404) {
      return 'Email address not found. Please check your email or create a new account.';
    }

    if (error.status === 429) {
      return 'Too many reset attempts. Please wait a few minutes before trying again.';
    }

    if (error.status === 500) {
      return 'Server error. Please try again later.';
    }

    return error?.error?.message || 'Failed to send reset email. Please try again.';
  }

  navigateToRegister() {
    this.router.navigate(['/register']);
  }

  ngOnDestroy() {
    // Clean up subscriptions
    if (this.countdownSubscription) {
      this.countdownSubscription.unsubscribe();
    }
  }
}
