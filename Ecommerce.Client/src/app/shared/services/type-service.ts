import { Injectable } from '@angular/core';
import { Environment } from '../../environment';
import { IType } from '../modules/type';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class TypeService {
  private baseUrl = `${Environment.baseUrl}/api/productTypes`;

  constructor(private http: HttpClient){}

  getAllTypes(): Observable<IType[]> {
    return this.http.get<IType[]>(`${this.baseUrl}`);
  }

  getType(id: number): Observable<IType> {
    return this.http.get<IType>(`${this.baseUrl}/${id}`);
  }

  createType(type: IType): Observable<IType> {
    return this.http.post<IType>(`${this.baseUrl}`, type);
  }

  deleteType(id: number): Observable<IType> {
    return this.http.delete<IType>(`${this.baseUrl}/${id}`);
  }
}
