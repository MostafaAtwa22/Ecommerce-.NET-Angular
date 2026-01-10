import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, FormGroup, AbstractControl, ValidationErrors } from '@angular/forms';
import { IDeliveryMethod } from '../../../shared/modules/deliveryMethod';

@Component({
  selector: 'app-delivery-method-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './delivery-method-form.component.html',
  styleUrls: ['./delivery-method-form.component.scss']
})
export class DeliveryMethodFormComponent implements OnInit {
  @Input() method: IDeliveryMethod | null = null;
  @Input() isEditing = false;

  @Output() submitForm = new EventEmitter<Partial<IDeliveryMethod>>();
  @Output() cancel = new EventEmitter<void>();

  form!: FormGroup;

  // Validation constants
  readonly MAX_NAME_LENGTH = 50;
  readonly MAX_DESC_LENGTH = 500;
  readonly MIN_DELIVERY_TIME = 1;
  readonly MAX_DELIVERY_TIME = 365;
  readonly MIN_PRICE = 0;
  readonly MAX_PRICE = 1000;

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.initializeForm();
    this.patchFormValues();
  }

  private initializeForm(): void {
    this.form = this.fb.group({
      shortName: ['', [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(this.MAX_NAME_LENGTH),
        Validators.pattern('^[a-zA-Z0-9\\s\\-]+$')
      ]],
      description: ['', [
        Validators.required,
        Validators.minLength(10),
        Validators.maxLength(this.MAX_DESC_LENGTH)
      ]],
      deliveryTime: [1, [
        Validators.required,
        Validators.min(this.MIN_DELIVERY_TIME),
        Validators.max(this.MAX_DELIVERY_TIME),
        Validators.pattern('^[0-9]+$')
      ]],
      price: [0, [
        Validators.required,
        Validators.min(this.MIN_PRICE),
        Validators.max(this.MAX_PRICE),
        Validators.pattern('^\\d+(\\.\\d{1,2})?$') 
      ]]
    });
  }

  private patchFormValues(): void {
    if (this.method) {
      this.form.patchValue({
        shortName: this.method.shortName,
        description: this.method.description,
        deliveryTime: this.method.deliveryTime,
        price: this.method.price
      });
    }
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.showValidationErrors();
      return;
    }

    const formValue = this.form.value;
    const payload: Partial<IDeliveryMethod> = {
      shortName: formValue.shortName.trim(),
      description: formValue.description.trim(),
      deliveryTime: Number(formValue.deliveryTime),
      price: Number(formValue.price)
    };

    this.submitForm.emit(payload);
  }

  private showValidationErrors(): void {
    const errors: string[] = [];

    if (this.shortName?.errors) {
      if (this.shortName.errors['required']) errors.push('Method name is required');
      if (this.shortName.errors['minlength']) errors.push(`Method name must be at least 3 characters`);
      if (this.shortName.errors['maxlength']) errors.push(`Method name cannot exceed ${this.MAX_NAME_LENGTH} characters`);
      if (this.shortName.errors['pattern']) errors.push('Only letters, numbers, spaces, and hyphens are allowed');
    }

    if (this.description?.errors) {
      if (this.description.errors['required']) errors.push('Description is required');
      if (this.description.errors['minlength']) errors.push('Description must be at least 10 characters');
      if (this.description.errors['maxlength']) errors.push(`Description cannot exceed ${this.MAX_DESC_LENGTH} characters`);
    }

    if (this.deliveryTime?.errors) {
      if (this.deliveryTime.errors['required']) errors.push('Delivery time is required');
      if (this.deliveryTime.errors['min']) errors.push(`Delivery time must be at least ${this.MIN_DELIVERY_TIME} day`);
      if (this.deliveryTime.errors['max']) errors.push(`Delivery time cannot exceed ${this.MAX_DELIVERY_TIME} days`);
      if (this.deliveryTime.errors['pattern']) errors.push('Delivery time must be a whole number');
    }

    if (this.price?.errors) {
      if (this.price.errors['required']) errors.push('Price is required');
      if (this.price.errors['min']) errors.push(`Price must be at least $${this.MIN_PRICE}`);
      if (this.price.errors['max']) errors.push(`Price cannot exceed $${this.MAX_PRICE}`);
      if (this.price.errors['pattern']) errors.push('Price must be a valid number (max 2 decimal places)');
    }

    if (errors.length > 0) {
      const errorList = errors.map(err => `• ${err}`).join('\n');
      alert(`Please fix the following errors:\n\n${errorList}`);
    }
  }

  onCancel(): void {
    this.cancel.emit();
  }

  get shortName() { return this.form.get('shortName'); }
  get description() { return this.form.get('description'); }
  get deliveryTime() { return this.form.get('deliveryTime'); }
  get price() { return this.form.get('price'); }

  // Helper methods for template
  get isShortNameInvalid(): boolean {
    return !!(this.shortName?.invalid && this.shortName?.touched);
  }

  get isDescriptionInvalid(): boolean {
    return !!(this.description?.invalid && this.description?.touched);
  }

  get isDeliveryTimeInvalid(): boolean {
    return !!(this.deliveryTime?.invalid && this.deliveryTime?.touched);
  }

  get isPriceInvalid(): boolean {
    return !!(this.price?.invalid && this.price?.touched);
  }

  // Format price display
  formatPrice(value: number): string {
    if (value === 0) return 'FREE';
    return `$${value.toFixed(2)}`;
  }

  // Calculate estimated delivery date
  getEstimatedDeliveryDate(): string {
    const days = this.deliveryTime?.value || 0;
    if (days === 0) return '';

    const today = new Date();
    const deliveryDate = new Date(today);
    deliveryDate.setDate(today.getDate() + days);

    return deliveryDate.toLocaleDateString('en-US', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }
}
