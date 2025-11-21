import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Environment } from '../environment';
import { Observable } from 'rxjs';
import { IPagination } from '../shared/modules/pagination';
import { IProduct } from '../shared/modules/product';
import { ShopParams } from '../shared/modules/ShopParams';

@Injectable({
  providedIn: 'root'
})
export class ShopService {
  private baseUrl = `${Environment.baseUrl}/api`;

  constructor(private http: HttpClient) { }

  getAllProducts(shopParams: ShopParams): Observable<IPagination<IProduct>> {
    let params = new HttpParams();

    if (shopParams.brandId != null)
      params = params.append('brandId', shopParams.brandId.toString());

    if (shopParams.typeId != null)
      params = params.append('typeId', shopParams.typeId.toString());

    if (shopParams.search)
      params = params.append('search', shopParams.search);

    params = params.append('sort', shopParams.sort);
    params = params.append('pageIndex', shopParams.pageIndex.toString());
    params = params.append('pageSize', shopParams.pageSize.toString());

    return this.http.get<IPagination<IProduct>>(`${this.baseUrl}/products`, { params });
  }

  getProduct(id: number): Observable<IProduct> {
    return this.http.get<IProduct>(`${this.baseUrl}/products/${id}`);
  }

  createProduct(product: IProduct): Observable<IProduct> {
    return this.http.post<IProduct>(`${this.baseUrl}/products`, product);
  }

  updateProduct(product: IProduct): Observable<IProduct> {
    return this.http.put<IProduct>(`${this.baseUrl}/products`, product);
  }

  deleteProduct(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/products/${id}`);
  }
}
