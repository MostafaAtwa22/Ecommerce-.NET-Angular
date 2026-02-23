import { AsyncPipe, CommonModule, CurrencyPipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { IWishList, IWishListItem } from '../shared/modules/wishlist';
import { WishlistService } from './wishlist-service';
import { BasketService } from '../shared/services/basket-service';
import { IProduct } from '../shared/modules/product';
import { ShopService } from '../shop/shop-service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-wishlist',
  imports: [
    AsyncPipe,
    CurrencyPipe,
    RouterLink,
    CommonModule
  ],
  templateUrl: './wishlist.component.html',
  styleUrls: ['./wishlist.component.scss']
})
export class WishlistComponent implements OnInit {
  wishList$!: Observable<IWishList | null>;

  constructor(
    private _wishListService: WishlistService,
    private _basketService: BasketService,
    private _shopService: ShopService,
    private _toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.wishList$ = this._wishListService.wishList$;
  }

  trackById(index: number, item: IWishListItem) {
    return item.id;
  }

  removeItem(item: IWishListItem) {
    this._wishListService.removeItemFromWishList(item);
    this._toastr.info("Removed from wishlist");
  }

  productInstock(item: IWishListItem) {
    if (typeof item.quantity !== 'number') return true;
    return item.quantity > 0;
  }

  addItemToBasket(item: IWishListItem) {
    this._shopService.getProduct(item.id).subscribe({
      next: (product: IProduct) => {
        if (product.quantity <= 0) {
          this._toastr.error("The product is out of stock");
          return;
        }

        this._basketService.addItemToBasket(product).subscribe({
          next: () => {
            this._toastr.success("Added to basket");
            this.removeItem(item);
          },
          error: (err) => console.error(err),
        });
      },
      error: (err) => console.error(err),
    });
  }

  clearWishList() {
    const wishList = this._wishListService.getCurrentWishListValue();
    if (wishList) this._wishListService.deleteWishList(wishList).subscribe();
  }
}
