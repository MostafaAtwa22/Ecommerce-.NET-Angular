import { Injectable } from '@angular/core';
import { Environment } from '../../environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IDeliveryMethod } from '../modules/deliveryMethod';

@Injectable({
  providedIn: 'root'
})
export class DeliveryMethodService {
  private baseUrl = `${Environment.baseUrl}/api/deliveryMethods`;

  constructor(private http: HttpClient) {}

  getAllDeliveryMethods(): Observable<IDeliveryMethod[]> {
    return this.http.get<IDeliveryMethod[]>(`${this.baseUrl}`);
  }

  getDeliveryMethod(id: number): Observable<IDeliveryMethod> {
    return this.http.get<IDeliveryMethod>(`${this.baseUrl}/${id}`);
  }

  createDeliveryMethod(deliveryMethod: IDeliveryMethod): Observable<IDeliveryMethod> {
    return this.http.post<IDeliveryMethod>(`${this.baseUrl}`, deliveryMethod);
  }

  updateDeliveryMethod(id: number, deliveryMethod: IDeliveryMethod): Observable<IDeliveryMethod> {
    return this.http.put<IDeliveryMethod>(`${this.baseUrl}/${id}`, deliveryMethod);
  }

  deleteDeliveryMethod(id: number): Observable<IDeliveryMethod> {
    return this.http.delete<IDeliveryMethod>(`${this.baseUrl}/${id}`);
  }
}
