import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { IBasket, IBasketItem } from '../shared/modules/basket';
import { BasketService } from '../shared/services/basket-service';
import { AsyncPipe, CurrencyPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { OrderTotalsComponent } from '../shared/components/order-totals-component/order-totals-component';
import { WishlistService } from '../wishlist/wishlist-service';
import { ShopService } from '../shop/shop-service';
import { ToastrService } from 'ngx-toastr';
import { IProduct } from '../shared/modules/product';

@Component({
  selector: 'app-basket-component',
  standalone: true,
  imports: [AsyncPipe, OrderTotalsComponent, CurrencyPipe, RouterLink],
  templateUrl: './basket-component.html',
  styleUrls: ['./basket-component.scss']
})
export class BasketComponent implements OnInit {
  basket$!: Observable<IBasket | null>;

  constructor(
    private basketService: BasketService,
    private wishlistService: WishlistService,
    private shopService: ShopService,
    private toastr: ToastrService
  ) {}

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

  moveItemToWishlist(item: IBasketItem) {
    if (!item.id) return;

    this.shopService.getProduct(item.id).subscribe({
      next: (product: IProduct) => {
        this.wishlistService.addItemToWishList(product).subscribe({
          next: () => {
            this.toastr.success('Moved to wishlist');
            this.removeItem(item);
          },
          error: (err) => console.error(err)
        });
      },
      error: (err) => console.error(err)
    });
  }

  clearBasket() {
    const basket = this.basketService.getCurrentBasketValue();
    if (basket) {
      this.basketService.deleteBasket(basket).subscribe();
    }
  }
}
