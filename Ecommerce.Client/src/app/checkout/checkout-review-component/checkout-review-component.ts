import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { IBasket, IBasketItem } from '../../shared/modules/basket';
import { BasketService } from '../../shared/services/basket-service';
import { AsyncPipe, CurrencyPipe } from '@angular/common';

@Component({
  selector: 'app-checkout-review-component',
  imports: [CurrencyPipe, AsyncPipe],
  templateUrl: './checkout-review-component.html',
  styleUrl: './checkout-review-component.scss',
})
export class CheckoutReviewComponent implements OnInit {
  basket$!: Observable<IBasket | null>;

  constructor(private basketService: BasketService) {}
  ngOnInit(): void {
    this.basket$ = this.basketService.basket$;
  }
}
