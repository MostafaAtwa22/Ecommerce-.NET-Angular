import { Injectable } from '@angular/core';
import { Environment } from '../environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IOrder, IOrderToCreate } from '../shared/modules/order';

@Injectable({
  providedIn: 'root'
})
export class CheckoutService {
  baseUrl = `${Environment.baseUrl}/api/orders`;

  constructor(private http: HttpClient){}

  getAllUserOrders(): Observable<IOrder[]> {
    return this.http.get<IOrder[]>(`${this.baseUrl}`);
  }

  getUserOrderById(id: number): Observable<IOrder> {
    return this.http.get<IOrder>(`${this.baseUrl}/${id}`);
  }

  createOrder(order: IOrderToCreate): Observable<IOrder> {
    return this.http.post<IOrder>(`${this.baseUrl}`, order)
  }
}
