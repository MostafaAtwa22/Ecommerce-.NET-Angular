import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { IBasket, IBasketItem } from '../../shared/modules/basket';
import { BasketService } from '../../shared/services/basket-service';
import { AsyncPipe, CurrencyPipe, NgIf } from '@angular/common';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-checkout-review-component',
  imports: [CurrencyPipe, AsyncPipe, NgIf],
  templateUrl: './checkout-review-component.html',
  styleUrl: './checkout-review-component.scss',
})
export class CheckoutReviewComponent implements OnInit {
  basket$!: Observable<IBasket | null>;
  couponCodeInput = '';

  constructor(
    private basketService: BasketService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.basket$ = this.basketService.basket$;
  }

  onCouponInput(event: Event) {
    this.couponCodeInput = (event.target as HTMLInputElement).value;
  }

  applyCoupon() {
    const basketId = this.basketService.getCurrentBasketValue()?.id;
    if (!basketId || !this.couponCodeInput) return;

    this.basketService.applyCoupon(basketId, this.couponCodeInput).subscribe({
      next: (basket) => {
        if (basket.discount && basket.discount > 0) {
          this.toastr.success(`Coupon applied! You saved $${basket.discount}`);
        } else {
          this.toastr.error('Invalid or expired coupon code');
        }
      },
      error: () => {
        this.toastr.error('Invalid or expired coupon code');
      }
    });
  }

  removeCoupon() {
    const basketId = this.basketService.getCurrentBasketValue()?.id;
    if (!basketId) return;

    this.basketService.removeCoupon(basketId).subscribe({
      next: () => {
        this.couponCodeInput = '';
        this.toastr.info('Coupon removed');
      }
    });
  }
}
