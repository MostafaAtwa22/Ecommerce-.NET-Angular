import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, map, throwError } from 'rxjs';
import { Environment } from '../../environment';
import { Basket, IBasket, IBasketItem, IBasketTotals } from '../modules/basket';
import { IProduct } from '../modules/product';
import { IDeliveryMethod } from '../modules/deliveryMethod';
import { ShopService } from '../../shop/shop-service';
import { ToastrService } from 'ngx-toastr';

@Injectable({ providedIn: 'root' })
export class BasketService {
  private baseUrl = `${Environment.baseUrl}/api/baskets`;

  private basketSource = new BehaviorSubject<IBasket | null>(null);
  private basketTotalSource = new BehaviorSubject<IBasketTotals | null>(null);

  basket$ = this.basketSource.asObservable();
  basketTotal$ = this.basketTotalSource.asObservable();
  shipping = 0;

  constructor(
    private http: HttpClient,
    private shopService: ShopService,
    private toastr: ToastrService
  ) {}

  createPaymentIntent() {
    return this.http.post<IBasket>(`${Environment.baseUrl}/api/payment/${this.getCurrentBasketValue()?.id}`, {})
      .pipe(
        map((basket: IBasket) => {
          this.basketSource.next(basket);
          console.log(this.getCurrentBasketValue());
          return basket;
        })
      );
  }

  setShippingPrice(deliveryMethod: IDeliveryMethod) {
      this.shipping = deliveryMethod.price;
      var basket = this.getCurrentBasketValue();
      if (!basket) return;
      basket.deliveryMethodId = deliveryMethod.id;
      basket.shippingPrice = deliveryMethod.price;
      this.calculateTotals();
      this.setBasket(basket).subscribe();
  }

  // 🔹 Load Basket
  getBasket(id: string) {
    return this.http.get<IBasket>(`${this.baseUrl}/${id}`).pipe(
      map(basket => {
        this.basketSource.next(basket);
        this.shipping = basket.shippingPrice ?? 0;
        this.calculateTotals();
        return basket;
      })
    );
  }

  // 🔹 Save Basket
  setBasket(basket: IBasket) {
    return this.http.post<IBasket>(this.baseUrl, basket).pipe(
      map(res => {
        this.basketSource.next(res);
        this.calculateTotals();
        return res;
      })
    );
  }

  // 🔹 Delete Basket
  deleteBasket(basket: IBasket) {
    return this.http.delete(`${this.baseUrl}/${basket.id}`, { responseType: 'text' })
      .pipe(map(() => this.clearBasket()));
  }

  // 🔹 Add Product
  addItemToBasket(product: IProduct, quantity = 1) {
    if (!product.quantity || product.quantity <= 0) {
      return throwError(() => new Error('This product is currently out of stock.'));
    }

    const basket = this.getCurrentBasketValue() ?? this.createBasket();
    const tentativeItem = this.mapProductItemToBasketItem(product, quantity);
    const existingItem = this.findMatchingItem(basket.items, tentativeItem);
    const currentQuantity = existingItem?.quantity ?? 0;
    const remainingStock = product.quantity - currentQuantity;

    if (remainingStock <= 0) {
      return throwError(() => new Error('You already added the maximum available quantity.'));
    }

    const quantityToAdd = Math.min(quantity, remainingStock);

    if (quantityToAdd < quantity) {
      this.toastr.info('Only limited stock available. Added remaining quantity.');
    }

    basket.items = this.addOrUpdateItem(basket.items, tentativeItem, quantityToAdd);
    return this.setBasket(basket);
  }

  getProductQuantityInBasket(productId: number): number {
    const basket = this.getCurrentBasketValue();
    if (!basket) return 0;

    const item = basket.items.find((basketItem) => basketItem.id === productId);
    return item?.quantity ?? 0;
  }

  getRemainingStockForProduct(product: IProduct): number {
    if (!product?.quantity) return 0;

    const remaining = product.quantity - this.getProductQuantityInBasket(product.id);
    return remaining > 0 ? remaining : 0;
  }

  hasReachedMaxQuantity(product: IProduct): boolean {
    if (!product?.quantity) return true;
    return this.getProductQuantityInBasket(product.id) >= product.quantity;
  }

  // 🔹 Increase Quantity
  incrementItemQuantity(item: IBasketItem) {
    const basket = this.getCurrentBasketValue();
    if (!basket) return;

    const foundItem = this.findMatchingItem(basket.items, item);
    if (!foundItem) return;

    this.shopService.getProduct(item.id).subscribe({
      next: (product) => {
        if (foundItem.quantity >= product.quantity) {
          this.toastr.warning('No more stock available for this product.');
          return;
        }

        foundItem.quantity++;
        this.setBasket(basket).subscribe();
      },
      error: (err) => console.error(err)
    });
  }

  // 🔹 Decrease Quantity
  decrementItemQuantity(item: IBasketItem) {
    const basket = this.getCurrentBasketValue();
    if (!basket) return;

    const foundItem = this.findMatchingItem(basket.items, item);
    if (foundItem) {
      if (foundItem.quantity > 1) {
        foundItem.quantity--;
        this.setBasket(basket).subscribe();
      } else {
        this.removeItemFromBasket(item);
      }
    }
  }

  // 🔹 Remove Item
  removeItemFromBasket(item: IBasketItem) {
    const basket = this.getCurrentBasketValue();
    if (!basket) return;

    // Remove the item
    basket.items = basket.items.filter(i => !this.itemsMatch(i, item));

    if (basket.items.length > 0) {
      // Only update the basket if items remain
      this.setBasket(basket).subscribe();
    } else {
      // Clear the basket if no items remain
      this.deleteBasket(basket).subscribe();
    }
  }

  // 🔹 Get Current Basket
  getCurrentBasketValue() {
    return this.basketSource.value;
  }

  // 🔹 Calculate Totals
  private calculateTotals() {
    const basket = this.getCurrentBasketValue();
    if (!basket || !basket.items.length) {
      this.basketTotalSource.next({ shipping: 0, subTotal: 0, total: 0 });
      return;
    }
    const shipping = this.shipping;
    const subTotal = basket.items.reduce((sum, item) => sum + item.price * item.quantity, 0);
    const total = subTotal + this.shipping;

    this.basketTotalSource.next({ shipping: this.shipping, subTotal, total });
  }

  // 🔹 Helpers
  private addOrUpdateItem(items: IBasketItem[], itemToAdd: IBasketItem, quantity: number): IBasketItem[] {
    const index = this.findMatchingItemIndex(items, itemToAdd);
    if (index === -1) {
      itemToAdd.quantity = quantity;
      items.push(itemToAdd);
    } else {
      items[index].quantity += quantity;
    }
    return items;
  }

  private findMatchingItem(items: IBasketItem[], target: IBasketItem) {
    const index = this.findMatchingItemIndex(items, target);
    return index === -1 ? undefined : items[index];
  }

  private findMatchingItemIndex(items: IBasketItem[], target: IBasketItem): number {
    return items.findIndex(item => this.itemsMatch(item, target));
  }

  private itemsMatch(item: IBasketItem, target: IBasketItem): boolean {
    const itemIdValid = item.id !== null && item.id !== undefined && item.id > 0;
    const targetIdValid = target.id !== null && target.id !== undefined && target.id > 0;

    if (itemIdValid && targetIdValid) {
      return item.id === target.id;
    }

    return (
      item.productName === target.productName &&
      item.brand === target.brand &&
      item.type === target.type &&
      item.price === target.price
    );
  }

  private createBasket(): IBasket {
    const basket = new Basket();
    localStorage.setItem('basket_id', basket.id);
    this.basketSource.next(basket);
    return basket;
  }

  clearBasket() {
    this.basketSource.next(null);
    this.basketTotalSource.next(null);
    this.shipping = 0;
    localStorage.removeItem('basket_id');
  }

  private mapProductItemToBasketItem(product: IProduct, quantity: number): IBasketItem {
    return {
      id: product.id,
      productName: product.name,
      price: product.price,
      quantity,
      pictureUrl: product.pictureUrl,
      brand: product.productBrandName,
      type: product.productTypeName,
    };
  }
}
