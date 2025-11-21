import { Injectable } from '@angular/core';
import { Environment } from '../../environment';
import { IAddress } from '../modules/address';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class ProfileService {
  private baseUrl = `${Environment.baseUrl}/api/profiles`;

  constructor(private http: HttpClient) {}

  getAddress(): Observable<IAddress> {
    return this.http.get<IAddress>(`${this.baseUrl}/address`);
  }

  updateAddress(address: IAddress): Observable<IAddress> {
    return this.http.put<IAddress>(`${this.baseUrl}/address`, address);
  }
}
