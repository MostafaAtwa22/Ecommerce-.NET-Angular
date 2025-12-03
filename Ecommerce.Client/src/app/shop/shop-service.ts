import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of, tap } from 'rxjs';
import { Environment } from '../environment';
import { IPagination, Pagination } from '../shared/modules/pagination';
import { IProduct } from '../shared/modules/product';
import { ShopParams } from '../shared/modules/ShopParams';

@Injectable({
  providedIn: 'root'
})
export class ShopService {
  private baseUrl = `${Environment.baseUrl}/api`;

  private cache = new Map<number, IProduct[]>();
  pagination = new Pagination<IProduct>();
  shopParams = new ShopParams();

  constructor(private http: HttpClient) { }

  getAllProducts(useCache: boolean = true): Observable<IPagination<IProduct>> {
    const cachedProducts = this.cache.get(1) || [];

    if (!useCache) this.cache.set(1, []);

    const pagesReceived = Math.ceil(cachedProducts.length / this.shopParams.pageSize);

    if (useCache && cachedProducts.length > 0 && this.shopParams.pageIndex <= pagesReceived) {
      this.pagination.data = cachedProducts.slice(
        (this.shopParams.pageIndex - 1) * this.shopParams.pageSize,
        this.shopParams.pageIndex * this.shopParams.pageSize
      );
      this.pagination.pageIndex = this.shopParams.pageIndex;
      this.pagination.pageSize = this.shopParams.pageSize;
      this.pagination.totalData = cachedProducts.length;

      return of(this.pagination);
    }

    let params = new HttpParams()
      .set('pageIndex', this.shopParams.pageIndex)
      .set('pageSize', this.shopParams.pageSize)
      .set('sort', this.shopParams.sort);

    if (this.shopParams.brandId != null) params = params.set('brandId', this.shopParams.brandId);
    if (this.shopParams.typeId != null) params = params.set('typeId', this.shopParams.typeId);
    if (this.shopParams.search) params = params.set('search', this.shopParams.search);

    return this.http.get<IPagination<IProduct>>(`${this.baseUrl}/products`, { params }).pipe(
      tap(response => {
        const currentCache = this.cache.get(1) || [];
        this.cache.set(1, [...currentCache, ...response.data]);
        this.pagination = response;
      })
    );
  }

  setShopParams(params: ShopParams) {
    this.shopParams = params;
  }

  getShopParams() {
    return this.shopParams;
  }

  resetShopParams() {
    this.shopParams = new ShopParams();
    return this.shopParams;
  }

  getProduct(id: number): Observable<IProduct> {
    const cachedProducts = this.cache.get(1) || [];
    const product = cachedProducts.find(p => p.id === id);

    return product ? of(product) : this.http.get<IProduct>(`${this.baseUrl}/products/${id}`);
  }

  createProduct(product: IProduct): Observable<IProduct> {
    return this.http.post<IProduct>(`${this.baseUrl}/products`, product).pipe(
      tap(created => {
        const cached = this.cache.get(1) || [];
        cached.push(created);
        this.cache.set(1, cached);
      })
    );
  }

  updateProduct(product: IProduct): Observable<IProduct> {
    return this.http.put<IProduct>(`${this.baseUrl}/products`, product).pipe(
      tap(updated => {
        const cached = this.cache.get(1) || [];
        const index = cached.findIndex(p => p.id === updated.id);
        if (index !== -1) cached[index] = updated;
        this.cache.set(1, cached);
      })
    );
  }

  deleteProduct(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/products/${id}`).pipe(
      tap(() => {
        const cached = this.cache.get(1) || [];
        this.cache.set(1, cached.filter(p => p.id !== id));
      })
    );
  }
}
