import { AfterViewInit, Component, ElementRef, Input, OnDestroy, ViewChild, effect } from '@angular/core';
import { ThemeService } from '../../shared/services/theme.service';
import { FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ToastrService } from 'ngx-toastr';
import { BasketService } from '../../shared/services/basket-service';
import { CheckoutService } from '../checkout-service';
import { IOrderToCreate } from '../../shared/modules/order';
import { Router } from '@angular/router';

declare var Stripe: any;

@Component({
  selector: 'app-checkout-payment-component',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './checkout-payment-component.html',
  styleUrls: ['./checkout-payment-component.scss'],
})
export class CheckoutPaymentComponent implements AfterViewInit, OnDestroy {
  @Input() paymentForm!: FormGroup;

  constructor(
    private toastr: ToastrService,
    private basketService: BasketService,
    private checkoutService: CheckoutService,
    private router: Router,
    public themeService: ThemeService
  ) {
    // React to theme changes
    effect(() => {
      const isDark = this.themeService.isDark();
      this.updateStripeStyles(isDark);
    });
  }

  @ViewChild('cardNumber') cardNumberElement!: ElementRef;
  @ViewChild('cardExpiry') cardExpiryElement!: ElementRef;
  @ViewChild('cardCvc') cardCvcElement!: ElementRef;

  stripe: any;
  cardNumber: any;
  cardExpiry: any;
  cardCvc: any;
  cardErrors: string | null = null;
  cardHandler = this.onChange.bind(this);
  cardNumberComplete = false;
  cardExpiryComplete = false;
  cardCvcComplete = false;

  ngAfterViewInit(): void {
    // Set validators
    this.paymentForm.get('nameOnCard')?.setValidators([Validators.required]);
    this.paymentForm.get('nameOnCard')?.updateValueAndValidity();

    // Small delay to ensure DOM is ready
    setTimeout(() => {
      this.initializeStripe();
    }, 100);
  }

  private getStripeStyle(isDark: boolean) {
    return {
      base: {
        color: isDark ? '#f8fafc' : '#0f172a',
        fontFamily: 'var(--font-sans)',
        fontSmoothing: 'antialiased',
        fontSize: '16px',
        '::placeholder': {
          color: isDark ? '#64748b' : '#94a3b8',
        },
      },
      invalid: {
        color: '#ef4444',
        iconColor: '#ef4444',
      },
    };
  }

  private updateStripeStyles(isDark: boolean): void {
    const style = this.getStripeStyle(isDark);
    this.cardNumber?.update({ style });
    this.cardExpiry?.update({ style });
    this.cardCvc?.update({ style });
  }

  private initializeStripe(): void {
    try {
      const isDark = this.themeService.isDark();
      const style = this.getStripeStyle(isDark);

      // Initialize Stripe
      this.stripe = Stripe('pk_test_51SVCaI3f1LzxHemr5eYgjrkPsmted61kv4dPMBeHGlFDEwHlKKBIPoqKFVXqmL96gESKd8SkOq5UPMBCF0bBkjSo00njbjuXY4');
      const elements = this.stripe.elements();

      // Create and mount Card Number
      this.cardNumber = elements.create('cardNumber', {
        style,
        showIcon: true
      });
      this.cardNumber.mount(this.cardNumberElement.nativeElement);
      this.cardNumber.on('change', (event: any) => {
        this.cardNumberComplete = event.complete;
        this.onChange(event);
      });

      // Create and mount Card Expiry
      this.cardExpiry = elements.create('cardExpiry', { style });
      this.cardExpiry.mount(this.cardExpiryElement.nativeElement);
      this.cardExpiry.on('change', (event: any) => {
        this.cardExpiryComplete = event.complete;
        this.onChange(event);
      });

      // Create and mount Card CVC
      this.cardCvc = elements.create('cardCvc', { style });
      this.cardCvc.mount(this.cardCvcElement.nativeElement);
      this.cardCvc.on('change', (event: any) => {
        this.cardCvcComplete = event.complete;
        this.onChange(event);
      });

      console.log('Stripe elements initialized successfully');
    } catch (error) {
      console.error('Error initializing Stripe:', error);
      this.cardErrors = 'Failed to initialize payment system. Please refresh the page.';
      this.toastr.error(this.cardErrors, 'Payment Error', {
        timeOut: 6000,
        positionClass: 'toast-top-center',
        closeButton: true,
      });
    }
  }

  ngOnDestroy(): void {
    if (this.cardNumber) {
      this.cardNumber.destroy();
    }
    if (this.cardExpiry) {
      this.cardExpiry.destroy();
    }
    if (this.cardCvc) {
      this.cardCvc.destroy();
    }
  }

  onChange(event: any): void {
    if (event.error) {
      this.cardErrors = event.error.message;
      this.paymentForm?.setErrors({ cardInvalid: true });

      this.toastr.error(this.cardErrors ?? 'Invalid card details.', 'Card Error', {
        timeOut: 4000,
        positionClass: 'toast-top-center',
        closeButton: true,
      });
    } else {
      this.cardErrors = null;
      // Only clear errors if all fields are complete
      if (this.cardNumberComplete && this.cardExpiryComplete && this.cardCvcComplete) {
        this.paymentForm?.setErrors(null);
      }
    }
    switch (event.elementType) {
      case 'cardNumber':
        this.cardNumberComplete = event.complete;
      break;
      case 'cardExpiry':
        this.cardExpiryComplete = event.complete;
      break;
      case 'cardCvc':
        this.cardCvcComplete = event.complete;
      break;
    }
  }

  isPaymentFormValid(): boolean {
    const nameValid = this.paymentForm.get('nameOnCard')?.valid;
    const cardsValid = this.cardNumberComplete && this.cardExpiryComplete && this.cardCvcComplete;
    return !!(nameValid && cardsValid && !this.cardErrors);
  }

  async confirmPayment(clientSecret: string): Promise<any> {
    if (!this.stripe || !this.cardNumber) {
      throw new Error('Stripe has not been initialized');
    }

    if (!this.isPaymentFormValid()) {
      throw new Error('Please complete all payment fields correctly');
    }

    const result = await this.stripe.confirmCardPayment(clientSecret, {
      payment_method: {
        card: this.cardNumber,
        billing_details: {
          name: this.paymentForm?.get('nameOnCard')?.value
        }
      }
    });

    return result;
  }
}
