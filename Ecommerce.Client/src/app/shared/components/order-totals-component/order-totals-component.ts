import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { IBasketTotals } from '../../modules/basket';
import { BasketService } from '../../services/basket-service';
import { AsyncPipe, CurrencyPipe } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-order-totals-component',
  imports: [AsyncPipe, CurrencyPipe, RouterLink],
  templateUrl: './order-totals-component.html',
  styleUrl: './order-totals-component.scss',
})
export class OrderTotalsComponent implements OnInit {
  basketTotals$!: Observable<IBasketTotals | null>;

  constructor(private basketService: BasketService) {}

  ngOnInit(): void {
    this.basketTotals$ = this.basketService.basketTotal$;
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
