import { Injectable } from '@angular/core';
import { Environment } from '../../environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IBrand } from '../modules/brand';

@Injectable({
  providedIn: 'root'
})
export class BrandService {
  private baseUrl = `${Environment.baseUrl}/api/productBrands`;

  constructor(private http: HttpClient){}

  getAllBrands(): Observable<IBrand[]> {
    return this.http.get<IBrand[]>(`${this.baseUrl}`);
  }

  getBrand(id: number): Observable<IBrand> {
    return this.http.get<IBrand>(`${this.baseUrl}/${id}`);
  }

  createBrand(brand: IBrand): Observable<IBrand> {
    return this.http.post<IBrand>(`${this.baseUrl}`, brand);
  }

  deleteBrand(id: number): Observable<IBrand> {
    return this.http.delete<IBrand>(`${this.baseUrl}/${id}`);
  }
}
