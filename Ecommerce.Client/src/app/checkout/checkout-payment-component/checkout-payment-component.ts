import { AfterViewInit, Component, ElementRef, Input, OnDestroy, ViewChild } from '@angular/core';
import { FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';

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

  private initializeStripe(): void {
    try {
      // Initialize Stripe
      this.stripe = Stripe('pk_test_51SVCaI3f1LzxHemr5eYgjrkPsmted61kv4dPMBeHGlFDEwHlKKBIPoqKFVXqmL96gESKd8SkOq5UPMBCF0bBkjSo00njbjuXY4');
      const elements = this.stripe.elements();

      // Style for Stripe elements
      const style = {
        base: {
          color: '#32325d',
          fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif',
          fontSmoothing: 'antialiased',
          fontSize: '16px',
          '::placeholder': {
            color: '#aab7c4',
          },
        },
        invalid: {
          color: '#fa755a',
          iconColor: '#fa755a',
        },
      };

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
    } else {
      this.cardErrors = null;
      // Only clear errors if all fields are complete
      if (this.cardNumberComplete && this.cardExpiryComplete && this.cardCvcComplete) {
        this.paymentForm?.setErrors(null);
      }
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
