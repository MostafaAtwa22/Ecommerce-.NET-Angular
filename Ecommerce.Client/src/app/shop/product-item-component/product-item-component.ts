import { Component, Input, OnInit } from '@angular/core';
import { IProduct } from '../../shared/modules/product';
import { CommonModule } from '@angular/common';
import { RouterLink } from "@angular/router";
import { BasketService } from '../../shared/services/basket-service';

@Component({
  selector: 'app-product-item',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './product-item-component.html',
  styleUrls: ['./product-item-component.scss']
})
export class ProductItemComponent implements OnInit {
  @Input() product!: IProduct;

  constructor(private _basketService: BasketService) {}
  ngOnInit(): void {
  }

  getStarsArray(): string[] {
    const rating = parseFloat(this.product.avrageRating) || 0;
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
    return quantity > 0;
  }

  addItemToBasket() {
    this._basketService.addItemToBasket(this.product).subscribe({
      next: () => console.log('Added to basket'),
      error: err => console.error(err)
    });
  }
}
