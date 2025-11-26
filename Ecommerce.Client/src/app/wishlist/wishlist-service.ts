import { Injectable } from '@angular/core';
import { Environment } from '../environment';
import { BehaviorSubject, map } from 'rxjs';
import { IWishList, IWishListItem, WishList } from '../shared/modules/wishlist';
import { HttpClient } from '@angular/common/http';
import { IProduct } from '../shared/modules/product';

@Injectable({
  providedIn: 'root'
})
export class WishlistService {
  private baseUrl = `${Environment.baseUrl}/api/wishlists`;

  private wishListSource = new BehaviorSubject<null | IWishList>(null);

  wishList$ = this.wishListSource.asObservable();

  constructor(private http: HttpClient) {}

  // load the wishlist
  getWishList(id: string) {
    return this.http.get<IWishList>(`${this.baseUrl}/${id}`)
    .pipe(
      map(wishList => {
        this.wishListSource.next(wishList);
        return wishList;
      })
    );
  }

  // save wishlist
  setWishList(wishList: IWishList) {
    return this.http.post<IWishList>(`${this.baseUrl}`, wishList)
    .pipe(
      map(res => {
        this.wishListSource.next(res);
        return res;
      })
    );
  }

  // delete
  deleteWishList(wishList: IWishList) {
    return this.http.delete(`${this.baseUrl}/${wishList.id}`, {responseType: 'text'})
      .pipe(map(() => this.clearWisthList()))
  }

  // clear the local storage from wishlist
  clearWisthList() {
    this.wishListSource.next(null);
    localStorage.removeItem('wishlist_id');
  }

  // add product
  addItemToWishList(product: IProduct) {
    const itemToAdd = this.mapProductItemToWishListItem(product);
    var wishList = this.getCurrentWishListValue() ?? this.createWishList();
    wishList.items = this.addOrRemoveItem(wishList.items, itemToAdd);
    return this.setWishList(wishList);
  }

  // remove item
  removeItemFromWishList(item: IWishListItem) {
    const wishList = this.getCurrentWishListValue();
    if (!wishList) return;

    wishList.items = wishList.items.filter(i => !this.itemsMatch(i, item));

    if (wishList.items.length > 0)
      this.setWishList(wishList).subscribe();
    else
      this.deleteWishList(wishList).subscribe();
  }

  getCurrentWishListValue() {
    return this.wishListSource.value;
  }

  private addOrRemoveItem(items: IWishListItem[], itemToAdd: IWishListItem): IWishListItem[] {
    const index = this.findMatchingItemIndex(items, itemToAdd);
    if (index === -1)
      items.push(itemToAdd);
    else
      items.splice(index, 1);
    return items;
  }


  private createWishList(): IWishList {
    const wishList = new WishList();
    localStorage.setItem('wishlist_id', wishList.id);
    this.wishListSource.next(wishList);
    return wishList;
  }

  private mapProductItemToWishListItem(product: IProduct): IWishListItem {
    return {
      id: product.id,
      productName: product.name,
      price: product.price,
      pictureUrl: product.pictureUrl,
      brand: product.productBrandName,
      type: product.productTypeName,
      quantity: product.quantity,
    };
  }

  private findMatchingItemIndex(items: IWishListItem[], target: IWishListItem): number {
    return items.findIndex(item => this.itemsMatch(item, target));
  }

  private itemsMatch(item: IWishListItem, target: IWishListItem): unknown {
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
}
