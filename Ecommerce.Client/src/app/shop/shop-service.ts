import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of, tap } from 'rxjs';
import { Environment } from '../environment';
import { IPagination, Pagination } from '../shared/modules/pagination';
import { IProduct, IProductCreate, IProductUpdate } from '../shared/modules/product';
import { ShopParams } from '../shared/modules/ShopParams';

@Injectable({
  providedIn: 'root',
})
export class ShopService {
  private baseUrl = `${Environment.baseUrl}/api`;

  // FIXED: Use Map with cache key that includes filters
  private cache = new Map<string, IPagination<IProduct>>();
  pagination = new Pagination<IProduct>();
  shopParams = new ShopParams();

  constructor(private http: HttpClient) {}

  getAllProducts(useCache: boolean = true): Observable<IPagination<IProduct>> {
    // FIXED: Generate cache key based on current params
    const cacheKey = this.generateCacheKey();

    if (useCache) {
      const cachedResponse = this.cache.get(cacheKey);
      if (cachedResponse) {
        console.log('📦 Returning cached data for:', cacheKey);
        this.pagination = cachedResponse;
        return of(cachedResponse);
      }
    }

    // Build HTTP params
    let params = new HttpParams()
      .set('pageIndex', this.shopParams.pageIndex.toString())
      .set('pageSize', this.shopParams.pageSize.toString())
      .set('sort', this.shopParams.sort);

    if (this.shopParams.brandId != null) {
      params = params.set('brandId', this.shopParams.brandId.toString());
    }
    if (this.shopParams.typeId != null) {
      params = params.set('typeId', this.shopParams.typeId.toString());
    }
    if (this.shopParams.search) {
      params = params.set('search', this.shopParams.search);
    }

    console.log('🌐 Fetching from API:', cacheKey);

    return this.http
      .get<IPagination<IProduct>>(`${this.baseUrl}/products`, {
        params,
        withCredentials: true
      })
      .pipe(
        tap((response) => {
          // Cache the complete response
          this.cache.set(cacheKey, response);
          this.pagination = response;
          console.log('✅ Cached response:', {
            key: cacheKey,
            pageIndex: response.pageIndex,
            pageSize: response.pageSize,
            totalData: response.totalData,
            dataLength: response.data.length
          });
        })
      );
  }

  // FIXED: Generate unique cache key based on all params
  private generateCacheKey(): string {
    return `page_${this.shopParams.pageIndex}_size_${this.shopParams.pageSize}_sort_${this.shopParams.sort}_brand_${this.shopParams.brandId ?? 'all'}_type_${this.shopParams.typeId ?? 'all'}_search_${this.shopParams.search || 'none'}`;
  }

  setShopParams(params: ShopParams) {
    this.shopParams = params;
  }

  getShopParams() {
    return this.shopParams;
  }

  resetShopParams() {
    this.shopParams = new ShopParams();
    this.clearCache(); // FIXED: Clear cache when resetting params
    return this.shopParams;
  }

  clearCache() {
    this.cache.clear();
    console.log('🗑️ Cache cleared');
  }

  getProduct(id: number): Observable<IProduct> {
    // FIXED: Search in all cached pages
    let product: IProduct | undefined;

    for (const [key, pagination] of this.cache.entries()) {
      product = pagination.data.find((p) => p.id === id);
      if (product) {
        console.log('📦 Product found in cache:', key);
        break;
      }
    }

    if (product) {
      return of(product);
    }

    console.log('🌐 Fetching product from API:', id);
    return this.http.get<IProduct>(`${this.baseUrl}/products/${id}`, {
      withCredentials: true
    });
  }

  createProduct(product: IProductCreate): Observable<IProduct> {
    const formData = new FormData();

    formData.append('Name', product.name);
    formData.append('Description', product.description);
    formData.append('Price', product.price.toString());
    formData.append('Quantity', product.quantity.toString());
    formData.append('ProductTypeId', product.productTypeId.toString());
    formData.append('ProductBrandId', product.productBrandId.toString());

    if (product.imageFile) {
      formData.append('ImageFile', product.imageFile, product.imageFile.name);
    }

    return this.http
      .post<IProduct>(`${this.baseUrl}/products`, formData, {
        withCredentials: true
      })
      .pipe(
        tap((created) => {
          // FIXED: Clear cache to ensure fresh data on next load
          this.clearCache();
          console.log('✅ Product created, cache cleared');
        })
      );
  }

  updateProduct(product: IProductUpdate): Observable<IProduct> {
    const formData = new FormData();

    formData.append('ProductId', product.productId.toString());
    formData.append('Name', product.name);
    formData.append('Description', product.description);
    formData.append('Price', product.price.toString());
    formData.append('Quantity', product.quantity.toString());
    formData.append('ProductTypeId', product.productTypeId.toString());
    formData.append('ProductBrandId', product.productBrandId.toString());

    if (product.imageFile && product.imageFile instanceof File) {
      formData.append('ImageFile', product.imageFile, product.imageFile.name);
    }

    return this.http
      .put<IProduct>(`${this.baseUrl}/products`, formData, {
        withCredentials: true
      })
      .pipe(
        tap((updated) => {
          // FIXED: Update product in all cached pages where it exists
          for (const [key, pagination] of this.cache.entries()) {
            const index = pagination.data.findIndex(p => p.id === updated.id);
            if (index !== -1) {
              pagination.data[index] = updated;
              this.cache.set(key, { ...pagination });
              console.log('✅ Updated product in cache:', key);
            }
          }
        })
      );
  }

  deleteProduct(id: number): Observable<void> {
    return this.http
      .delete<void>(`${this.baseUrl}/products/${id}`, {
        withCredentials: true
      })
      .pipe(
        tap(() => {
          this.clearCache();
          console.log('✅ Product deleted, cache cleared');
        })
      );
  }
}
