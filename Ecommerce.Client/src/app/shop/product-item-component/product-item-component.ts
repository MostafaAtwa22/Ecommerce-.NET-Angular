import { Component, Input, OnInit } from '@angular/core';
import { IProduct } from '../../shared/modules/product';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { BasketService } from '../../shared/services/basket-service';
import { WishlistService } from '../../wishlist/wishlist-service';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../../account/account-service';

@Component({
  selector: 'app-product-item',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './product-item-component.html',
  styleUrls: ['./product-item-component.scss'],
})
export class ProductItemComponent implements OnInit {
  @Input() product!: IProduct;

  constructor(
    private _basketService: BasketService,
    private _wishListService: WishlistService,
    private _toastr: ToastrService,
    private _accountService: AccountService
  ) {}
  ngOnInit(): void {}

  isCustomer(): boolean {
    const user = this._accountService.user();
    if (!user) return true;

    // Check if user has roles that should be excluded (Admin, SuperAdmin)
    const roles = user.roles || [];
    const isAdminOrSuper = roles.some(r => {
      const role = r.toLowerCase();
      return role === 'admin' || role === 'superadmin';
    });

    return !isAdminOrSuper;
  }

  getStarsArray(): string[] {
    const rating = (this.product.averageRating) || 0;
    const stars = [];

    for (let i = 1; i <= 5; i++) {
      if (rating >= i) {
        stars.push('fas fa-star');
      } else if (rating >= i - 0.5) {
        stars.push('fas fa-star-half-alt');
      } else {
        stars.push('far fa-star');
      }
    }

    return stars;
  }

  isInStock(): boolean {
    const quantity = this.product.quantity || 0;
    const boughtQuantity = this.product.boughtQuantity || 0;
    return quantity > boughtQuantity;
  }

  canAddToBasket(): boolean {
    return this.isInStock() && !this._basketService.hasReachedMaxQuantity(this.product);
  }

  addItemToBasket() {
    if (!this.canAddToBasket()) {
      this._toastr.info('Maximum quantity already in basket');
      return;
    }

    this._basketService.addItemToBasket(this.product).subscribe({
      next: () => {
        this._toastr.success('Added to basket');

        // ✅ Remove from wishlist if present
        const wishlist = this._wishListService.getCurrentWishListValue();
        if (wishlist && wishlist.items.some((item) => item.id === this.product.id)) {
          this._wishListService.removeItemFromWishList({ id: this.product.id } as any);
        }
      },
      error: (err) => this._toastr.error(err?.message ?? 'Unable to add to basket'),
    });
  }

  addItemToWishList() {
    this._wishListService.addItemToWishList(this.product).subscribe({
      next: () => {
        this._toastr.success('Updated wishlist');

        // ✅ Remove from basket if present
        this._basketService.removeItemFromBasket({ id: this.product.id } as any);
      },
      error: (err) => this._toastr.error(err?.message ?? 'Unable to update wishlist'),
    });
  }

  isInWishlist(productId: number): boolean {
    const wishlist = this._wishListService.getCurrentWishListValue();
    if (!wishlist) return false;

    return wishlist.items.some((item) => item.id === productId);
  }

}
