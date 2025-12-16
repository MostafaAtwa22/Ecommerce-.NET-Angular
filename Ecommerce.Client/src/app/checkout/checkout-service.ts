import { Injectable } from '@angular/core';
import { Environment } from '../environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of, tap } from 'rxjs';
import { IAllOrders, IOrder, IOrderToCreate } from '../shared/modules/order';
import { OrdersParams } from '../shared/modules/OrdersParams';
import { IPagination, Pagination } from '../shared/modules/pagination';

@Injectable({
  providedIn: 'root'
})
export class CheckoutService {
  baseUrl = `${Environment.baseUrl}/api/orders`;


  private cache = new Map<number, IAllOrders[]>();
  pagination = new Pagination<IAllOrders>();
  ordersParams = new OrdersParams();

  constructor(private http: HttpClient) { }

  getAllOrders(useCache: boolean = true): Observable<IPagination<IAllOrders>> {
    const cachedOrders = this.cache.get(1) || [];

    if (!useCache) this.cache.set(1, []);

    const pagesReceived =
      Math.ceil(cachedOrders.length / this.ordersParams.pageSize);

    if (
      useCache &&
      cachedOrders.length > 0 &&
      this.ordersParams.pageIndex <= pagesReceived
    ) {
      this.pagination.data = cachedOrders.slice(
        (this.ordersParams.pageIndex - 1) * this.ordersParams.pageSize,
        this.ordersParams.pageIndex * this.ordersParams.pageSize
      );

      this.pagination.pageIndex = this.ordersParams.pageIndex;
      this.pagination.pageSize = this.ordersParams.pageSize;
      this.pagination.totalData = cachedOrders.length;

      return of(this.pagination);
    }

    let params = new HttpParams()
      .set('pageIndex', this.ordersParams.pageIndex)
      .set('pageSize', this.ordersParams.pageSize)
      .set('sort', this.ordersParams.sort);

    return this.http
      .get<IPagination<IAllOrders>>(`${this.baseUrl}/getall`, { params })
      .pipe(
        tap(response => {
          const currentCache = this.cache.get(1) || [];
          this.cache.set(1, [...currentCache, ...response.data]);
          this.pagination = response;
        })
      );
  }

  setOrdersParams(params: OrdersParams) {
    this.ordersParams = params;
  }

  getOrdersParams() {
    return this.ordersParams;
  }

  resetOrdersParams() {
    this.ordersParams = new OrdersParams();
    return this.ordersParams;
  }
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
