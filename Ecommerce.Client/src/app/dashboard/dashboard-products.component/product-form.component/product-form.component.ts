import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, FormGroup, AbstractControl, ValidationErrors } from '@angular/forms';
import { IProduct, IProductCreate, IProductUpdate } from '../../../shared/modules/product';
import { IBrand } from '../../../shared/modules/brand';
import { IType } from '../../../shared/modules/type';

// Custom validator for file extensions
function allowedExtensionsValidator(allowedExtensions: string[]) {
  return (control: AbstractControl): ValidationErrors | null => {
    const file = control.value as File;
    if (!file) {
      return null; // No file is valid (for updates)
    }

    const extension = file.name.split('.').pop()?.toLowerCase();
    if (!extension) {
      return { invalidExtension: true };
    }

    const isValid = allowedExtensions.includes(`.${extension}`);
    return isValid ? null : { invalidExtension: true };
  };
}

// Custom validator for file size (max size in bytes)
function maxFileSizeValidator(maxSizeInBytes: number) {
  return (control: AbstractControl): ValidationErrors | null => {
    const file = control.value as File;
    if (!file) {
      return null; // No file is valid (for updates)
    }

    return file.size <= maxSizeInBytes ? null : { fileTooLarge: true };
  };
}

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './product-form.component.html',
  styleUrls: ['./product-form.component.scss']
})
export class ProductFormComponent implements OnInit {

  @Input() isEditing = false;
  @Input() product: IProduct | null = null;
  @Input() brands: IBrand[] = [];
  @Input() types: IType[] = [];

  @Output() submitForm = new EventEmitter<IProductCreate | IProductUpdate>();
  @Output() cancel = new EventEmitter<void>();

  form!: FormGroup;
  imagePreview: string | null = null;
  selectedFile: File | null = null;

  // Constants matching server-side validation
  readonly MAX_FILE_SIZE_BYTES = 1 * 1024 * 1024; // 1MB in bytes
  readonly ALLOWED_EXTENSIONS = ['.jpg', '.jpeg', '.png'];
  readonly ALLOWED_EXTENSIONS_STRING = this.ALLOWED_EXTENSIONS.join(', ');

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.initializeForm();
    this.patchFormValues();
  }

  private initializeForm(): void {
    this.form = this.fb.group({
      name: ['', [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(50),
        Validators.pattern('^[a-zA-Z0-9\\s\\-_]+$')
      ]],
      description: ['', [
        Validators.required,
        Validators.minLength(10),
        Validators.maxLength(1500)
      ]],
      price: [0, [
        Validators.required,
        Validators.min(1),
        Validators.max(10000)
      ]],
      quantity: [1, [
        Validators.required,
        Validators.min(5),
        Validators.max(10000)
      ]],
      productBrandId: [null, Validators.required],
      productTypeId: [null, Validators.required],
      imageFile: [null, [
        // For create mode, image is required
        // For edit mode, image is optional
        (control: AbstractControl) => {
          if (!this.isEditing && !control.value) {
            return { required: true };
          }
          return null;
        },
        allowedExtensionsValidator(this.ALLOWED_EXTENSIONS),
        maxFileSizeValidator(this.MAX_FILE_SIZE_BYTES)
      ]]
    });
  }

  private patchFormValues(): void {
    if (this.product) {
      this.form.patchValue({
        name: this.product.name,
        description: this.product.description,
        price: this.product.price,
        quantity: this.product.quantity,
        productBrandId: this.product.productBrandId,
        productTypeId: this.product.productTypeId
      });

      if (this.product.pictureUrl) {
        this.imagePreview = this.product.pictureUrl;
      }
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (!file) return;

    // Client-side validation before setting the file
    const extension = file.name.split('.').pop()?.toLowerCase();
    const isValidExtension = extension && this.ALLOWED_EXTENSIONS.includes(`.${extension}`);

    if (!isValidExtension) {
      alert(`Invalid file type. Only ${this.ALLOWED_EXTENSIONS_STRING} are allowed.`);
      input.value = '';
      return;
    }

    if (file.size > this.MAX_FILE_SIZE_BYTES) {
      alert(`File is too large. Maximum size is ${this.MAX_FILE_SIZE_BYTES / (1024 * 1024)}MB.`);
      input.value = '';
      return;
    }

    this.selectedFile = file;
    this.form.patchValue({ imageFile: file });
    this.form.get('imageFile')?.updateValueAndValidity();

    const reader = new FileReader();
    reader.onload = () => {
      this.imagePreview = reader.result as string;
    };
    reader.readAsDataURL(file);
  }

  removeImage(): void {
    this.imagePreview = null;
    this.selectedFile = null;
    this.form.patchValue({ imageFile: null });
    this.form.get('imageFile')?.updateValueAndValidity();

    // Clear file input
    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement;
    if (fileInput) {
      fileInput.value = '';
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();

      // Show specific error messages
      this.showValidationErrors();
      return;
    }

    if (this.isEditing && this.product) {
      const formValue = this.form.value;
      const payload: IProductUpdate = {
        name: formValue.name,
        description: formValue.description,
        price: formValue.price,
        quantity: formValue.quantity,
        productBrandId: formValue.productBrandId,
        productTypeId: formValue.productTypeId,
        imageFile: this.selectedFile || null,
        productId: this.product.id
      };
      this.submitForm.emit(payload);
    } else {
      const formValue = this.form.value;
      const payload: IProductCreate = {
        name: formValue.name,
        description: formValue.description,
        price: formValue.price,
        quantity: formValue.quantity,
        productBrandId: formValue.productBrandId,
        productTypeId: formValue.productTypeId,
        imageFile: this.selectedFile!
      };
      this.submitForm.emit(payload);
    }
  }

  private showValidationErrors(): void {
    const errors: string[] = [];

    if (this.name?.errors) {
      if (this.name.errors['required']) errors.push('Name is required');
      if (this.name.errors['minlength']) errors.push('Name must be at least 3 characters');
      if (this.name.errors['maxlength']) errors.push('Name cannot exceed 50 characters');
      if (this.name.errors['pattern']) errors.push('Only letters, numbers, spaces, hyphens and underscores are allowed');
    }

    if (this.description?.errors) {
      if (this.description.errors['required']) errors.push('Description is required');
      if (this.description.errors['minlength']) errors.push('Description must be at least 10 characters');
      if (this.description.errors['maxlength']) errors.push('Description cannot exceed 1500 characters');
    }

    if (this.price?.errors) {
      if (this.price.errors['required']) errors.push('Price is required');
      if (this.price.errors['min']) errors.push('Price must be at least $1');
      if (this.price.errors['max']) errors.push('Price cannot exceed $10,000');
    }

    if (this.quantity?.errors) {
      if (this.quantity.errors['required']) errors.push('Quantity is required');
      if (this.quantity.errors['min']) errors.push('Quantity must be at least 5');
      if (this.quantity.errors['max']) errors.push('Quantity cannot exceed 10,000');
    }

    if (this.productBrandId?.errors?.['required']) {
      errors.push('Please select a brand');
    }

    if (this.productTypeId?.errors?.['required']) {
      errors.push('Please select a type');
    }

    if (this.imageFile?.errors) {
      if (this.imageFile.errors['required'] && !this.isEditing) {
        errors.push('Image file is required for new products');
      }
      if (this.imageFile.errors['invalidExtension']) {
        errors.push(`Only ${this.ALLOWED_EXTENSIONS_STRING} files are allowed`);
      }
      if (this.imageFile.errors['fileTooLarge']) {
        errors.push(`File is too large. Maximum size is ${this.MAX_FILE_SIZE_BYTES / (1024 * 1024)}MB`);
      }
    }

    if (errors.length > 0) {
      alert('Please fix the following errors:\n\n' + errors.join('\n'));
    }
  }

  onCancel(): void {
    this.cancel.emit();
  }

  get name() { return this.form.get('name'); }
  get description() { return this.form.get('description'); }
  get price() { return this.form.get('price'); }
  get quantity() { return this.form.get('quantity'); }
  get productBrandId() { return this.form.get('productBrandId'); }
  get productTypeId() { return this.form.get('productTypeId'); }
  get imageFile() { return this.form.get('imageFile'); }
}
