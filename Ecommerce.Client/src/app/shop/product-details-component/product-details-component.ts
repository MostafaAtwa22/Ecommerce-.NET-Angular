import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { switchMap } from 'rxjs/operators';
import { IProduct } from '../../shared/modules/product';
import { ShopService } from '../shop-service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BasketService } from '../../shared/services/basket-service';

@Component({
  selector: 'app-product-details',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './product-details-component.html',
  styleUrls: ['./product-details-component.scss']
})
export class ProductDetailsComponent implements OnInit {
  product!: IProduct;
  loading = true;
  errorMessage: string | null = null;
  quantity: number = 1;

  // Mock rating distribution data (in real app, this would come from API)
  private ratingDistribution = {
    5: 182,
    4: 34,
    3: 8,
    2: 0,
    1: 0
  };

  constructor(
    private _shopService: ShopService,
    private _activatedRoute: ActivatedRoute,
    private _basketService: BasketService
  ) {}

  ngOnInit(): void {
    this._activatedRoute.paramMap
      .pipe(
        switchMap(params => {
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
        },
        error: (err) => {
          console.error('Error loading product details:', err);
          this.errorMessage = 'Failed to load product details. Please try again later.';
          this.loading = false;
        }
      });
  }

  isInStock(): boolean {
    return this.product.quantity > 0;
  }

  getMaxQuantity(): number {
    return this.product.quantity;
  }

  increaseQuantity(): void {
    const maxQuantity = this.getMaxQuantity();
    if (this.quantity < maxQuantity) this.quantity++;
  }

  decreaseQuantity(): void {
    if (this.quantity > 1) this.quantity--;
  }

  // Rating Methods
  getAverageRating(): number {
    return parseFloat(this.product.avrageRating) || 0;
  }

  getStarsArray(): string[] {
    const rating = this.getAverageRating();
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

  getTotalRatings(): number {
    return Object.values(this.ratingDistribution).reduce((sum, count) => sum + count, 0);
  }

  getRatingDistribution(): any[] {
    const total = this.getTotalRatings();
    return [
      { stars: 5, count: this.ratingDistribution[5], percentage: (this.ratingDistribution[5] / total) * 100 },
      { stars: 4, count: this.ratingDistribution[4], percentage: (this.ratingDistribution[4] / total) * 100 },
      { stars: 3, count: this.ratingDistribution[3], percentage: (this.ratingDistribution[3] / total) * 100 },
      { stars: 2, count: this.ratingDistribution[2], percentage: (this.ratingDistribution[2] / total) * 100 },
      { stars: 1, count: this.ratingDistribution[1], percentage: (this.ratingDistribution[1] / total) * 100 }
    ];
  }

  getProgressBarClass(stars: number): string {
    return `rating-${stars}`;
  }

  addToBasket(): void {
    if (this.isInStock()) {
      this._basketService.addItemToBasket(this.product, this.quantity);
    }
  }
}
