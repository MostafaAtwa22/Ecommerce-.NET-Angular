import { Injectable } from '@angular/core';
import { Environment } from '../../environment';
import { HttpClient } from '@angular/common/http';
import { Observable, of, tap } from 'rxjs';
import { IBrand } from '../modules/brand';

@Injectable({
  providedIn: 'root'
})
export class BrandService {
  private baseUrl = `${Environment.baseUrl}/api/productBrands`;
  private brands: IBrand[] = [];   // Local cache

  constructor(private http: HttpClient) {}

  getAllBrands(): Observable<IBrand[]> {
    // If already loaded, return cached
    if (this.brands.length > 0) {
      return of(this.brands);
    }

    return this.http.get<IBrand[]>(this.baseUrl)
      .pipe(
        tap(response => this.brands = response)
      );
  }

  getBrand(id: number): Observable<IBrand> {
    const brand = this.brands.find(b => b.id === id);

    if (brand)
      return of(brand);

    return this.http.get<IBrand>(`${this.baseUrl}/${id}`);
  }

  createBrand(brand: IBrand): Observable<IBrand> {
    return this.http.post<IBrand>(this.baseUrl, brand)
      .pipe(
        tap(createdBrand => {
          this.brands.push(createdBrand); // update cache
        })
      );
  }

  deleteBrand(id: number): Observable<IBrand> {
    return this.http.delete<IBrand>(`${this.baseUrl}/${id}`)
      .pipe(
        tap(() => {
          this.brands = this.brands.filter(b => b.id !== id); // update cache
        })
      );
  }
}
