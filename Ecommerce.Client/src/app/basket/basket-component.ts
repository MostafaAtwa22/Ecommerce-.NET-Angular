import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { IBasket, IBasketItem } from '../shared/modules/basket';
import { BasketService } from '../shared/services/basket-service';
import { AsyncPipe, CurrencyPipe, NgForOf, NgIf } from '@angular/common';
import { RouterLink } from '@angular/router';
import { OrderTotalsComponent } from '../shared/components/order-totals-component/order-totals-component';

@Component({
  selector: 'app-basket-component',
  standalone: true,
  imports: [AsyncPipe, OrderTotalsComponent, CurrencyPipe, RouterLink],
  templateUrl: './basket-component.html',
  styleUrls: ['./basket-component.scss']
})
export class BasketComponent implements OnInit {
  basket$!: Observable<IBasket | null>;

  constructor(private basketService: BasketService) {}

  ngOnInit(): void {
    this.basket$ = this.basketService.basket$;
  }

  increaseQuantity(item: IBasketItem) {
    this.basketService.incrementItemQuantity(item);
  }

  decreaseQuantity(item: IBasketItem) {
    this.basketService.decrementItemQuantity(item);
  }

  removeItem(item: IBasketItem) {
    this.basketService.removeItemFromBasket(item);
  }

  clearBasket() {
    const basket = this.basketService.getCurrentBasketValue();
    if (basket) {
      this.basketService.deleteBasket(basket).subscribe();
    }
  }
}
