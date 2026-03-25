import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { IBasketTotals } from '../../modules/basket';
import { BasketService } from '../../services/basket-service';
import { AsyncPipe, CurrencyPipe, NgIf } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { IBasket } from '../../modules/basket';
import { map } from 'rxjs/operators';

@Component({
  selector: 'app-order-totals-component',
  standalone: true,
  imports: [AsyncPipe, CurrencyPipe, RouterLink, NgIf, FormsModule],
  templateUrl: './order-totals-component.html',
  styleUrl: './order-totals-component.scss',
})
export class OrderTotalsComponent implements OnInit {
  basketTotals$!: Observable<IBasketTotals | null>;
  basket$!: Observable<IBasket | null>;
  couponCode: string = '';

  constructor(private basketService: BasketService, private toastr: ToastrService) {}

  ngOnInit(): void {
    this.basketTotals$ = this.basketService.basketTotal$;
    this.basket$ = this.basketService.basket$;
  }

  applyCoupon() {
    if (!this.couponCode) return;
    const basket = this.basketService.getCurrentBasketValue();
    if (!basket) return;

    this.basketService.applyCoupon(basket.id, this.couponCode).subscribe({
      next: () => {
        this.toastr.success('Coupon applied successfully');
        this.couponCode = '';
      },
      error: (err) => {
        this.toastr.error('Invalid coupon code');
        console.error(err);
      }
    });
  }

  removeCoupon() {
    const basket = this.basketService.getCurrentBasketValue();
    if (!basket) return;

    this.basketService.removeCoupon(basket.id).subscribe({
      next: () => {
        this.toastr.success('Coupon removed');
      },
      error: (err) => {
        this.toastr.error('Failed to remove coupon');
        console.error(err);
      }
    });
  }

  // Calculate free shipping threshold
  getFreeShippingAmount(totals: IBasketTotals): number {
    const freeShippingThreshold = 50;
    return Math.max(0, freeShippingThreshold - totals.subTotal);
  }

  hasFreeShipping(totals: IBasketTotals): boolean {
    return totals.subTotal >= 50;
  }

  getShippingProgress(totals: IBasketTotals): number {
    const freeShippingThreshold = 50;
    return Math.min(100, (totals.subTotal / freeShippingThreshold) * 100);
  }
}
