import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Environment } from '../environment';
import { Observable, of, tap } from 'rxjs';
import { IPagination, Pagination } from '../shared/modules/pagination';
import { IProduct } from '../shared/modules/product';
import { ShopParams } from '../shared/modules/ShopParams';

@Injectable({
  providedIn: 'root'
})
export class ShopService {
  private baseUrl = `${Environment.baseUrl}/api`;
  private products: IProduct[] = [];   // Local cache
  pagination = new Pagination();
  shopParams = new ShopParams();

  constructor(private http: HttpClient) { }

  getAllProducts(useCache: boolean): Observable<IPagination<IProduct>> {
    if (useCache === false)
      this.products = [];

    if (this.products.length > 0 && useCache === true) {
      const pagesReceived = Math.ceil(this.products.length / this.shopParams.pageSize);

      if (this.shopParams.pageIndex <= pagesReceived) {
        this.pagination.data =
          this.products.slice((this.shopParams.pageIndex - 1) * this.shopParams.pageSize,
            this.shopParams.pageIndex * this.shopParams.pageSize);

        return of(this.pagination);
      }
    }
    let params = new HttpParams();

    if (this.shopParams.brandId != null)
      params = params.append('brandId', this.shopParams.brandId);

    if (this.shopParams.typeId != null)
      params = params.append('typeId', this.shopParams.typeId);

    if (this.shopParams.search)
      params = params.append('search', this.shopParams.search);

    params = params.append('sort', this.shopParams.sort);
    params = params.append('pageIndex', this.shopParams.pageIndex);
    params = params.append('pageSize', this.shopParams.pageSize);

    return this.http.get<IPagination<IProduct>>(`${this.baseUrl}/products`, { params })
      .pipe(
        tap(response => {
          this.products = [...this.products, ...response.data];  // Cache current page only
          this.pagination = response;      // Store pagination object
        })
      );
  }

  setShopParams(params: ShopParams) {
    this.shopParams = params;
  }

  getShopParams() {
    return this.shopParams;
  }

  getProduct(id: number): Observable<IProduct> {
    const product = this.products.find(p => p.id === id);

    if (product)
      return of(product); // return cached product

    return this.http.get<IProduct>(`${this.baseUrl}/products/${id}`);
  }

  createProduct(product: IProduct): Observable<IProduct> {
    return this.http.post<IProduct>(`${this.baseUrl}/products`, product)
      .pipe(
        tap(createdProduct => {
          this.products.push(createdProduct); // add to cache
        })
      );
  }

  updateProduct(product: IProduct): Observable<IProduct> {
    return this.http.put<IProduct>(`${this.baseUrl}/products`, product)
      .pipe(
        tap(updatedProduct => {
          const index = this.products.findIndex(p => p.id === updatedProduct.id);
          if (index !== -1) {
            this.products[index] = updatedProduct; // update existing in cache
          }
        })
      );
  }

  deleteProduct(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/products/${id}`)
      .pipe(
        tap(() => {
          this.products = this.products.filter(p => p.id !== id); // remove from cache
        })
      );
  }
}
