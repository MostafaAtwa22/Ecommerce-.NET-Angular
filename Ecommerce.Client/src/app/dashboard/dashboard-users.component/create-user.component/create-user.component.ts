import { Component, inject, Output, EventEmitter } from '@angular/core';
import {
    AbstractControl,
    AsyncValidatorFn,
    FormBuilder,
    FormGroup,
    ValidationErrors,
    Validators,
    ReactiveFormsModule,
} from '@angular/forms';
import { of, timer } from 'rxjs';
import { map, switchMap, catchError } from 'rxjs/operators';
import { AccountService } from '../../../account/account-service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'app-create-user',
    templateUrl: './create-user.component.html',
    styleUrls: ['./create-user.component.scss'],
    standalone: true,
    imports: [ReactiveFormsModule, CommonModule],
})
export class CreateUserComponent {
    private fb = inject(FormBuilder);
    private accountService = inject(AccountService);
    private router = inject(Router);
    private toastr = inject(ToastrService);

    showPassword = false;
    isLoading = false;

    @Output() cancel = new EventEmitter<void>();
    @Output() userCreated = new EventEmitter<void>();

    createUserForm: FormGroup = this.fb.group(
        {
            email: [
                '',
                [
                    Validators.required,
                    Validators.pattern(/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/),
                ],
                [this.validateEmailNotTaken()],
            ],
            userName: [
                '',
                [Validators.required, Validators.pattern(/^[a-zA-Z0-9_]+$/)],
                [this.validateUsernameNotTaken()],
            ],
            firstName: ['', [Validators.required, Validators.pattern(/^[A-Za-z]+$/)]],
            lastName: ['', [Validators.required, Validators.pattern(/^[A-Za-z]+$/)]],
            gender: ['', Validators.required],
            phoneNumber: ['', [Validators.required, Validators.pattern(/^[0-9+\-() ]+$/)]],
            password: [
                '',
                [
                    Validators.required,
                    Validators.minLength(6),
                    Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*\W).{6,}$/),
                ],
            ],
            confirmPassword: ['', Validators.required],
            roleName: ['Admin', Validators.required],
        },
        { validators: this.passwordMatchValidator }
    );

    get email() { return this.createUserForm.get('email'); }
    get userName() { return this.createUserForm.get('userName'); }
    get firstName() { return this.createUserForm.get('firstName'); }
    get lastName() { return this.createUserForm.get('lastName'); }
    get gender() { return this.createUserForm.get('gender'); }
    get phoneNumber() { return this.createUserForm.get('phoneNumber'); }
    get password() { return this.createUserForm.get('password'); }
    get confirmPassword() { return this.createUserForm.get('confirmPassword'); }
    get roleName() { return this.createUserForm.get('roleName'); }

    passwordIsLength6() { return (this.password?.value ?? '').length >= 6; }
    passwordContainsCapitalLetter() { return /[A-Z]/.test(this.password?.value ?? ''); }
    passwordContainsNumber() { return /[0-9]/.test(this.password?.value ?? ''); }
    passwordContainsSmallLetter() { return /[a-z]/.test(this.password?.value ?? ''); }
    passwordContainsSpecialChar() { return /[\W_]/.test(this.password?.value ?? ''); }

    validateEmailNotTaken(): AsyncValidatorFn {
        return (control: AbstractControl) => {
            if (!control.value) return of(null);
            return timer(500).pipe(
                switchMap(() =>
                    this.accountService.emailExists(control.value).pipe(
                        map((res) => (res ? { emailExists: true } : null)),
                        catchError(() => of(null))
                    )
                )
            );
        };
    }

    validateUsernameNotTaken(): AsyncValidatorFn {
        return (control: AbstractControl) => {
            if (!control.value) return of(null);
            return timer(500).pipe(
                switchMap(() =>
                    this.accountService.usernameExists(control.value).pipe(
                        map((res) => (res ? { usernameExists: true } : null)),
                        catchError(() => of(null))
                    )
                )
            );
        };
    }

    onCancel(): void {
        this.cancel.emit();
    }

    passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
        const password = control.get('password')?.value;
        const confirm = control.get('confirmPassword');
        if (!password || !confirm) return null;
        if (password !== confirm.value) {
            confirm.setErrors({ passwordMismatch: true });
        } else if (confirm.hasError('passwordMismatch')) {
            const errors = { ...confirm.errors as any };
            delete errors['passwordMismatch'];
            confirm.setErrors(Object.keys(errors).length ? errors : null);
        }
        return null;
    }

    onSubmit(): void {
        if (this.createUserForm.invalid) {
            this.createUserForm.markAllAsTouched();
            this.toastr.error('Please fix the errors.');
            return;
        }

        this.isLoading = true;
        const v = this.createUserForm.value;

        const adminData = {
            email: v.email!,
            userName: v.userName!,
            firstName: v.firstName!,
            lastName: v.lastName!,
            gender: Number(v.gender),
            phoneNumber: v.phoneNumber!,
            password: v.password!,
            confirmPassword: v.confirmPassword!,
            roleName: v.roleName!,
            emailConfirmed: true
        };

        this.accountService.createAdmin(adminData).subscribe({
            next: () => {
                this.isLoading = false;
                this.toastr.success('User created successfully');
                this.userCreated.emit();
                this.onCancel();
            },
            error: (err) => {
                this.isLoading = false;
                this.toastr.error('Failed to create user');
                console.error(err);
            },
        });
    }
}
