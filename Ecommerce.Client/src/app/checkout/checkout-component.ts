import { Component, OnInit, ViewChild } from '@angular/core';
import { OrderTotalsComponent } from "../shared/components/order-totals-component/order-totals-component";
import { StepperComponent } from "../shared/components/stepper-component/stepper-component";
import { CdkStep } from "@angular/cdk/stepper";
import { CheckoutAddressComponent } from "./checkout-address-component/checkout-address-component";
import { CheckoutDelieveryComponent } from "./checkout-delievery-component/checkout-delievery-component";
import { CheckoutReviewComponent } from "./checkout-review-component/checkout-review-component";
import { CheckoutPaymentComponent } from "./checkout-payment-component/checkout-payment-component";
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { IBasketTotals } from '../shared/modules/basket';
import { BasketService } from '../shared/services/basket-service';
import { ProfileService } from '../shared/services/profile-service';
import { IAddress } from '../shared/modules/address';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-checkout-component',
  imports: [
    OrderTotalsComponent,
    StepperComponent,
    CdkStep,
    CheckoutAddressComponent,
    CheckoutDelieveryComponent,
    CheckoutReviewComponent,
    CheckoutPaymentComponent,
    ReactiveFormsModule
  ],
  templateUrl: './checkout-component.html',
  styleUrl: './checkout-component.scss',
})
export class CheckoutComponent implements OnInit {
  @ViewChild(StepperComponent) stepper!: StepperComponent;
  @ViewChild(CheckoutPaymentComponent) paymentComponent!: CheckoutPaymentComponent;

  checkoutForm!: FormGroup;
  baketTotals$!: Observable<IBasketTotals>;
  paymentIntentCreated = false;
  
  // Track step validation states
  addressStepValid = false;
  deliveryStepValid = false;
  paymentStepValid = false;

  // Track if steps have been attempted (for showing errors)
  addressStepAttempted = false;
  deliveryStepAttempted = false;
  paymentStepAttempted = false;

  constructor(
    private fb: FormBuilder,
    private profileService: ProfileService,
    private basketService: BasketService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.createCheckoutFrom();
    this.getAddressFormValues();
    this.setupFormValidationTracking();
    this.getDeliveryMethodValue();
  }

  createCheckoutFrom() {
    this.checkoutForm = this.fb.group({
      addressForm: this.fb.group({
        firstName: [{ value: '', disabled: true }, Validators.required],
        lastName: [{ value: '', disabled: true }, Validators.required],
        country: ['', Validators.required],
        government: ['', Validators.required],
        city: ['', Validators.required],
        street: ['', Validators.required],
        zipcode: ['', [Validators.required, Validators.pattern(/^\d+$/)]]
      }),
      deliveryForm: this.fb.group({
        deliveryMethod: ['', Validators.required]
      }),
      paymentForm: this.fb.group({
        nameOnCard: ['', Validators.required]
      })
    });
  }

  setupFormValidationTracking() {
    // Track address form validity
    this.checkoutForm.get('addressForm')?.statusChanges.subscribe(status => {
      if (this.addressStepAttempted) {
        this.addressStepValid = status === 'VALID';
      }
    });

    // Track delivery form validity
    this.checkoutForm.get('deliveryForm')?.statusChanges.subscribe(status => {
      if (this.deliveryStepAttempted) {
        this.deliveryStepValid = status === 'VALID';
      }
    });

    // Track payment form validity - includes Stripe validation
    this.checkoutForm.get('paymentForm')?.statusChanges.subscribe(status => {
      if (this.paymentStepAttempted) {
        // Check both Angular form and Stripe elements
        const angularFormValid = status === 'VALID';
        const stripeValid = this.paymentComponent?.isPaymentFormValid() || false;
        this.paymentStepValid = angularFormValid && stripeValid;
      }
    });

    // Initial validation check
    setTimeout(() => {
      this.addressStepValid = this.checkoutForm.get('addressForm')?.valid || false;
      this.deliveryStepValid = this.checkoutForm.get('deliveryForm')?.valid || false;
      this.paymentStepValid = this.checkoutForm.get('paymentForm')?.valid || false;
    });
  }

  getAddressFormValues() {
    const user = JSON.parse(localStorage.getItem('user') || '{}');

    if (user) {
      this.checkoutForm.get('addressForm')?.patchValue({
        firstName: user.firstName,
        lastName: user.lastName,
      });
    }

    this.profileService.getAddress().subscribe({
      next: (address: IAddress) => {
        if (address) {
          this.checkoutForm.get('addressForm')?.patchValue({
            country: address.country,
            government: address.government,
            city: address.city,
            street: address.street,
            zipcode: address.zipcode
          });
        }
      },
      error: err => {
        console.log('Failed to load address', err);
      }
    });
  }

  onStepChange() {
    if (!this.stepper || !this.checkoutForm) return;

    const currentStep = this.stepper.selectedIndex;

    // Address step
    if (currentStep >= 1 && !this.addressStepAttempted) {
      this.addressStepAttempted = true;
      const addressForm = this.checkoutForm.get('addressForm') as FormGroup;
      if (addressForm) {
        this.addressStepValid = addressForm.valid;
      }
    }

    // Delivery step
    if (currentStep >= 2 && !this.deliveryStepAttempted) {
      this.deliveryStepAttempted = true;
      const deliveryForm = this.checkoutForm.get('deliveryForm') as FormGroup;
      if (deliveryForm) {
        this.deliveryStepValid = deliveryForm.valid;
      }
    }

    // Payment step
    if (currentStep >= 3 && !this.paymentStepAttempted) {
      this.paymentStepAttempted = true;
      const paymentForm = this.checkoutForm.get('paymentForm') as FormGroup;
      if (paymentForm) {
        this.paymentStepValid = paymentForm.valid;
      }
    }
  }

  createPaymentIntent() {
    this.basketService.createPaymentIntent().subscribe({
      next: (basket) => {
        this.paymentIntentCreated = true;
        this.toastr.success('Payment intent created successfully!');
        console.log('Payment Intent Created:', basket);
      },
      error: (err) => {
        this.toastr.error(err.message || 'Failed to create payment intent');
        console.error('Payment Intent Error:', err);
      }
    });
  }

  getDeliveryMethodValue() {
    const basket = this.basketService.getCurrentBasketValue();
    if (basket && basket.deliveryMethodId != null) {
      this.checkoutForm.get('deliveryForm')
        ?.get('deliveryMethod')
        ?.patchValue(basket.deliveryMethodId);
    }
  }

  get addressForm() {
    return this.checkoutForm.get('addressForm') as FormGroup;
  }

  get deliveryForm() {
    return this.checkoutForm.get('deliveryForm') as FormGroup;
  }

  get paymentForm() {
    return this.checkoutForm.get('paymentForm') as FormGroup;
  }
}
