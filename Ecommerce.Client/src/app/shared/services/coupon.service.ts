import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Environment } from '../../environment';

export interface ICoupon {
  id: number;
  code: string;
  discountAmount: number;
  expiryDate: string;
  isActive: boolean;
}

export interface ICouponCreate {
  code: string;
  discountAmount: number;
  expiryDate: string;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class CouponService {
  private readonly base = `${Environment.baseUrl}/api/coupons`;

  constructor(private http: HttpClient) {}

  getAll() {
    return this.http.get<ICoupon[]>(this.base);
  }

  create(dto: ICouponCreate) {
    return this.http.post<ICoupon>(this.base, dto);
  }

  delete(id: number) {
    return this.http.delete(`${this.base}/${id}`);
  }

  validate(code: string) {
    return this.http.post<ICoupon>(`${this.base}/validate?code=${encodeURIComponent(code)}`, {});
  }
}
