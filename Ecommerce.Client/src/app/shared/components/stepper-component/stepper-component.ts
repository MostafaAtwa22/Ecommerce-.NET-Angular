// stepper-component.ts - FIXED VERSION

import { Component, Input, OnInit, Output, EventEmitter, inject } from '@angular/core';
import { CdkStepper } from '@angular/cdk/stepper';
import { CommonModule } from '@angular/common';
import { FormGroup } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { IDeliveryMethod } from '../../modules/deliveryMethod';
import { CheckoutService } from '../../../checkout/checkout-service';
import { BasketService } from '../../services/basket-service';
import { NavigationExtras, Router } from '@angular/router';

@Component({
  selector: 'app-stepper-component',
  templateUrl: './stepper-component.html',
  styleUrls: ['./stepper-component.scss'],
  standalone: true,
  imports: [CommonModule],
  providers: [{ provide: CdkStepper, useExisting: StepperComponent }],
})
export class StepperComponent extends CdkStepper implements OnInit {
  @Input() linearModeSelected = false;
  @Input() createPaymentIntent!: () => void;
  @Input() paymentIntentCreated = false;
  @Input() paymentComponent: any;

  // Validation Inputs
  @Input() addressStepValid = false;
  @Input() deliveryStepValid = false;
  @Input() paymentStepValid = false;

  @Input() addressStepAttempted = false;
  @Input() deliveryStepAttempted = false;
  @Input() paymentStepAttempted = false;

  // Inputs from parent
  @Input() addressForm!: FormGroup;
  @Input() deliveryForm!: FormGroup;
  @Input() selectedDeliveryMethod!: IDeliveryMethod;

  @Output() stepChange = new EventEmitter<void>();

  private toastr = inject(ToastrService);
  private basketService = inject(BasketService);
  private orderService = inject(CheckoutService);
  private router = inject(Router);

  ngOnInit(): void {
    this.linear = this.linearModeSelected;
  }

  onClick(index: number): void {
    this.selectedIndex = index;
    this.stepChange.emit();
  }

  override next(): void {
    // When moving from Review (step 2) to Payment (step 3), create payment intent
    if (this.selectedIndex === 2 && this.createPaymentIntent) {
      this.createPaymentIntent();
    }
    super.next();
    this.stepChange.emit();
  }

  override previous(): void {
    super.previous();
    this.stepChange.emit();
  }

  isStepSelected(index: number): boolean {
    return this.selectedIndex === index;
  }

  getStepDisplay(stepIndex: number): string {
    switch (stepIndex) {
      case 0:
        if (!this.addressStepAttempted) return '1';
        return this.addressStepValid ? '✓' : '✕';
      case 1:
        if (!this.deliveryStepAttempted) return '2';
        return this.deliveryStepValid ? '✓' : '✕';
      case 2:
        return '3';
      case 3:
        if (!this.paymentStepAttempted) return '4';
        return this.paymentStepValid ? '✓' : '✕';
      default:
        return (stepIndex + 1).toString();
    }
  }

  getStepState(stepIndex: number): string {
    switch (stepIndex) {
      case 0:
        if (!this.addressStepAttempted) return 'pending';
        return this.addressStepValid ? 'completed' : 'error';
      case 1:
        if (!this.deliveryStepAttempted) return 'pending';
        return this.deliveryStepValid ? 'completed' : 'error';
      case 2:
        if (this.selectedIndex === 2) return 'pending';
        if (this.selectedIndex > 2) return 'completed';
        return 'pending';
      case 3:
        if (!this.paymentStepAttempted) return 'pending';
        return this.paymentStepValid ? 'completed' : 'error';
      default:
        return 'pending';
    }
  }

  getPreviousStepName(): string {
    if (this.selectedIndex === 0) return '';
    const previousStep = this.steps.toArray()[this.selectedIndex - 1];
    return typeof previousStep.label === 'string' ? previousStep.label : 'Previous';
  }

  getNextStepName(): string {
    if (this.selectedIndex === this.steps.length - 1) return '';
    const nextStep = this.steps.toArray()[this.selectedIndex + 1];
    return typeof nextStep.label === 'string' ? nextStep.label : 'Next';
  }

  isLastStep(): boolean {
    return this.selectedIndex === this.steps.length - 1;
  }

  async createOrder(): Promise<void> {
    if (!this.addressForm) {
      this.toastr.error('Address form is not ready yet.');
      return;
    }

    this.addressForm.markAllAsTouched();
    if (this.addressForm.invalid) {
      this.toastr.error('Please fill all required address fields.');
      return;
    }

    // Get basket
    const basket = this.basketService.getCurrentBasketValue();
    if (!basket) {
      this.toastr.error('Your basket is empty.');
      return;
    }

    // Check delivery method
    if (!basket.deliveryMethodId) {
      this.toastr.error('Please select a delivery method.');
      return;
    }

    // Check payment intent
    if (!basket.clientSecret) {
      this.toastr.error('Payment intent not created. Please go back to Review step.');
      return;
    }

    // Validate payment form
    if (!this.paymentComponent || !this.paymentComponent.isPaymentFormValid()) {
      this.toastr.error('Please complete all payment fields correctly.');
      return;
    }

    try {
      // First, confirm the payment with Stripe
      this.toastr.info('Processing payment...');

      const paymentResult = await this.paymentComponent.confirmPayment(basket.clientSecret);

      if (paymentResult.error) {
        this.toastr.error(paymentResult.error.message || 'Payment failed');
        return;
      }

      if (paymentResult.paymentIntent?.status !== 'succeeded') {
        this.toastr.error('Payment was not successful');
        return;
      }

      // Payment successful, now create the order
      const orderToCreate = {
        basketId: basket.id,
        deliveryMethodId: basket.deliveryMethodId,
        shipToAddress: this.addressForm.getRawValue(),
      };

      this.orderService.createOrder(orderToCreate).subscribe({
        next: (order) => {
          this.toastr.success('Order created successfully!');
          this.basketService.clearBasket();
          this.basketService.deleteBasket(basket);

          const navExtra: NavigationExtras = { state: order };
          this.router.navigate(['success'], navExtra).catch(err => {
            console.error('Navigation error:', err);
            this.toastr.error('Failed to navigate to success page');
          });
        },
        error: (err) => {
          this.toastr.error('Order created but failed to save. Please contact support.');
          console.error('Order creation error:', err);
        }
      });
    } catch (error: any) {
      this.toastr.error(error.message || 'Payment processing failed');
      console.error('Payment error:', error);
    }
  }
}
