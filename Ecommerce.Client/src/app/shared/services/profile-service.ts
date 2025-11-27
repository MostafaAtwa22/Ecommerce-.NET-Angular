import { Injectable } from '@angular/core';
import { Environment } from '../../environment';
import { IAddress } from '../modules/address';
import { catchError, Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { IChangePassword, IDeleteAccount, IProfile, ISetPassword } from '../modules/profile';

@Injectable({
  providedIn: 'root'
})
export class ProfileService {
  private baseUrl = `${Environment.baseUrl}/api/profiles`;

  constructor(private http: HttpClient) { }

  getProfile(): Observable<IProfile> {
    return this.http.get<IProfile>(`${this.baseUrl}/profile`);
  }

  getAddress(): Observable<IAddress> {
    return this.http.get<IAddress>(`${this.baseUrl}/address`);
  }

  updateAddress(address: IAddress): Observable<IAddress> {
    return this.http.put<IAddress>(`${this.baseUrl}/address`, address);
  }

  changePassword(password: IChangePassword): Observable<boolean> {
    return this.http.post<boolean>(`${this.baseUrl}/changePassword`, password);
  }

  setPassword(password: ISetPassword): Observable<boolean> {
    return this.http.post<boolean>(`${this.baseUrl}/setPassword`, password);
  }

  deleteProfile(password: IDeleteAccount): Observable<boolean> {
    return this.http.delete<boolean>(`${this.baseUrl}/deleteProfile`, {
      body: password
    });
  }

  // In your profile-service.ts
  uploadProfilePicture(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('profilePicture', file);

    return this.http.post('/api/profile/picture', formData, {
      reportProgress: true,
      observe: 'events'
    }).pipe(
      // Handle upload progress if needed
      catchError((error: any) => {
        console.error('Upload error:', error);
        throw error;
      })
    );
  }
}
