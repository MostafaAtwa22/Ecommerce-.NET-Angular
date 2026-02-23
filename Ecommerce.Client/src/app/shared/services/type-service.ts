import { Injectable } from '@angular/core';
import { Environment } from '../../environment';
import { IType } from '../modules/type';
import { Observable, of, tap } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class TypeService {
  private baseUrl = `${Environment.baseUrl}/api/productTypes`;
  private types: IType[] = [];   // Local cache

  constructor(private http: HttpClient) {}

  getAllTypes(): Observable<IType[]> {
    // Return cached types if already loaded
    if (this.types.length > 0) {
      return of(this.types);
    }

    return this.http.get<IType[]>(this.baseUrl)
      .pipe(
        tap(response => this.types = response)
      );
  }

  getType(id: number): Observable<IType> {
    const type = this.types.find(t => t.id === id);

    if (type)
      return of(type);

    return this.http.get<IType>(`${this.baseUrl}/${id}`);
  }

  createType(type: IType): Observable<IType> {
    return this.http.post<IType>(this.baseUrl, type)
      .pipe(
        tap(createdType => {
          this.types.push(createdType); // update cache
        })
      );
  }

  deleteType(id: number): Observable<IType> {
    return this.http.delete<IType>(`${this.baseUrl}/${id}`)
      .pipe(
        tap(() => {
          this.types = this.types.filter(t => t.id !== id); // update cache
        })
      );
  }
}
