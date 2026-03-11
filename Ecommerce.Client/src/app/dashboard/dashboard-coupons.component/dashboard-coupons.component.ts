import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CouponService, ICoupon } from '../../shared/services/coupon.service';
import { HasPermissionDirective } from '../../shared/directives/has-permission.directive';
import { ToastrService } from 'ngx-toastr';
import { SweetAlertService } from '../../shared/services/sweet-alert.service';

@Component({
  selector: 'app-dashboard-coupons',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, HasPermissionDirective],
  templateUrl: './dashboard-coupons.component.html',
  styleUrls: ['./dashboard-coupons.component.scss'],
})
export class DashboardCouponsComponent implements OnInit {
  coupons: ICoupon[] = [];
  filteredCoupons: ICoupon[] = [];
  loading = false;
  showForm = false;
  searchTerm = '';
  form!: FormGroup;

  constructor(
    private couponService: CouponService,
    private fb: FormBuilder,
    private toastr: ToastrService,
    private sweetAlert: SweetAlertService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadCoupons();
  }

  initForm(): void {
    this.form = this.fb.group({
      code: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(30)]],
      discountAmount: [0, [Validators.required, Validators.min(0.01)]],
      expiryDate: ['', Validators.required],
      isActive: [true]
    });
  }

  loadCoupons(): void {
    this.loading = true;
    this.couponService.getAll().subscribe({
      next: (coupons) => {
        this.coupons = coupons;
        this.filteredCoupons = coupons;
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.toastr.error(err?.error?.message || 'Failed to load coupons.', 'Error', {
          positionClass: 'toast-top-center', closeButton: true, timeOut: 5000
        });
      }
    });
  }

  onSearch(): void {
    const term = this.searchTerm.toLowerCase().trim();
    this.filteredCoupons = term
      ? this.coupons.filter(c => c.code.toLowerCase().includes(term))
      : [...this.coupons];
  }

  toggleForm(): void {
    this.showForm = !this.showForm;
    if (!this.showForm) {
      this.form.reset({ isActive: true });
    }
  }

  submitCoupon(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const val = this.form.value;
    const payload = {
      code: val.code.toUpperCase().trim(),
      discountAmount: val.discountAmount,
      expiryDate: new Date(val.expiryDate).toISOString(),
      isActive: val.isActive
    };

    this.couponService.create(payload).subscribe({
      next: (coupon) => {
        this.coupons.unshift(coupon);
        this.filteredCoupons = [...this.coupons];
        this.toastr.success(`Coupon "${coupon.code}" created!`, 'Success', {
          positionClass: 'toast-top-right', timeOut: 3000
        });
        this.toggleForm();
      },
      error: (err) => {
        this.toastr.error(err?.error?.message || 'Failed to create coupon.', 'Error', {
          positionClass: 'toast-top-center', closeButton: true, timeOut: 5000
        });
      }
    });
  }

  deleteCoupon(id: number): void {
    this.sweetAlert.confirm({
      title: 'Delete Coupon',
      text: 'Are you sure you want to delete this coupon? This action cannot be undone.',
      icon: 'warning',
      confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
      if (result.isConfirmed) {
        this.couponService.delete(id).subscribe({
          next: () => {
            this.coupons = this.coupons.filter(c => c.id !== id);
            this.filteredCoupons = this.filteredCoupons.filter(c => c.id !== id);
            this.toastr.success('Coupon deleted.', 'Success', {
              positionClass: 'toast-top-right', timeOut: 3000
            });
          },
          error: (err) => {
            this.toastr.error(err?.error?.message || 'Failed to delete coupon.', 'Error', {
              positionClass: 'toast-top-center', closeButton: true, timeOut: 5000
            });
          }
        });
      }
    });
  }

  isExpired(coupon: ICoupon): boolean {
    return new Date(coupon.expiryDate) < new Date();
  }

  get activeCoupons(): number {
    return this.coupons.filter(c => c.isActive && !this.isExpired(c)).length;
  }

  get expiredCoupons(): number {
    return this.coupons.filter(c => this.isExpired(c)).length;
  }

  get avgDiscount(): number {
    if (!this.coupons.length) return 0;
    return this.coupons.reduce((sum, c) => sum + c.discountAmount, 0) / this.coupons.length;
  }
}
