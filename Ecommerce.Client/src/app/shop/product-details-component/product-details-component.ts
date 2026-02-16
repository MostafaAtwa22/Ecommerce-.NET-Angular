import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { switchMap } from 'rxjs/operators';
import { Subscription } from 'rxjs';
import { IProduct } from '../../shared/modules/product';
import { ShopService } from '../shop-service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BasketService } from '../../shared/services/basket-service';
import { WishlistService } from '../../wishlist/wishlist-service';
import { ToastrService } from 'ngx-toastr';
import { ProductReviewComponent } from "../../product-reviews/product-review.component";
import { AccountService } from '../../account/account-service';
import { HasPermissionDirective } from '../../shared/directives/has-permission.directive';

@Component({
  selector: 'app-product-details',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    ProductReviewComponent,
    HasPermissionDirective
  ],
  templateUrl: './product-details-component.html',
  styleUrls: ['./product-details-component.scss'],
})
export class ProductDetailsComponent implements OnInit, OnDestroy {
  product!: IProduct;
  loading = true;
  errorMessage: string | null = null;
  quantity: number = 1;
  private basketSubscription?: Subscription;

  constructor(
    private _shopService: ShopService,
    private _activatedRoute: ActivatedRoute,
    private _basketService: BasketService,
    private _wishlistService: WishlistService,
    private _toastr: ToastrService,
    private _accountService: AccountService
  ) {}

  ngOnInit(): void {
    this.basketSubscription = this._basketService.basket$.subscribe(() => {
      this.alignQuantityWithStock();
    });

    this._activatedRoute.paramMap
      .pipe(
        switchMap((params) => {
          const id = +params.get('id')!;
          this.loading = true;
          this.errorMessage = null;
          return this._shopService.getProduct(id);
        })
      )
      .subscribe({
        next: (product) => {
          this.product = product;
          this.loading = false;
          this.alignQuantityWithStock();
        },
        error: (err) => {
          console.error('Error loading product details:', err);
          this.errorMessage = 'Failed to load product details. Please try again later.';
          this.loading = false;
        },
      });
  }

  ngOnDestroy(): void {
    this.basketSubscription?.unsubscribe();
  }

  isInStock(): boolean {
    const quantity = this.product.quantity || 0;
    const boughtQuantity = this.product.boughtQuantity || 0;
    return quantity > boughtQuantity;
  }

  getMaxQuantity(): number {
    if (!this.product) return 0;
    return this._basketService.getRemainingStockForProduct(this.product);
  }

  canAddToBasket(): boolean {
    return this.isInStock() && this.getMaxQuantity() > 0;
  }

  increaseQuantity(): void {
    const maxQuantity = this.getMaxQuantity();
    if (!this.canAddToBasket()) return;
    if (maxQuantity === 0) return;

    if (this.quantity < maxQuantity) this.quantity++;
  }

  decreaseQuantity(): void {
    if (this.quantity > 1) this.quantity--;
  }

  addToBasket(): void {
    if (!this.canAddToBasket()) {
      this._toastr.info('Maximum quantity already in basket');
      return;
    }

    const maxQuantity = this.getMaxQuantity();
    const quantityToAdd = Math.min(Math.max(this.quantity, 1), maxQuantity);

    this._basketService.addItemToBasket(this.product, quantityToAdd).subscribe({
      next: () => {
        const wishlist = this._wishlistService.getCurrentWishListValue();
        if (wishlist && wishlist.items.some((item) => item.id === this.product.id)) {
          this._wishlistService.removeItemFromWishList({ id: this.product.id } as any);
        }

        this.alignQuantityWithStock();
      },
      error: (err) => this._toastr.error(err?.message ?? 'Unable to add to basket'),
    });
  }

  addToWishlist(): void {
    this._wishlistService.addItemToWishList(this.product).subscribe({
      next: () => {
        this._basketService.removeItemFromBasket({ id: this.product.id } as any);
      },
      error: (err) => this._toastr.error(err?.message ?? 'Unable to add to wishlist'),
    });
  }

  isInWishlist(productId: number): boolean {
    const wishlist = this._wishlistService.getCurrentWishListValue();
    if (!wishlist) return false;

    return wishlist.items.some((item) => item.id === productId);
  }

  private alignQuantityWithStock(): void {
    if (!this.product) return;

    const maxQuantity = this.getMaxQuantity();

    if (maxQuantity <= 0) {
      this.quantity = 0;
      return;
    }

    if (this.quantity <= 0) {
      this.quantity = 1;
    }

    if (this.quantity > maxQuantity) {
      this.quantity = maxQuantity;
    }
  }

  isCustomer(): boolean {
    const user = this._accountService.user();
    if (!user) return true;

    const roles = user.roles || [];
    const isAdminOrSuper = roles.some(r => {
      const role = r.toLowerCase();
      return role === 'admin' || role === 'superadmin';
    });

    return !isAdminOrSuper;
  }
}
