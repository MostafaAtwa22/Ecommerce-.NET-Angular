import { Injectable } from '@angular/core';
import { Environment } from '../../environment';
import { IAddress } from '../modules/address';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import {
  IChangePassword,
  IDeleteAccount,
  IProfile,
  IProfileUpdate,
  ISetPassword,
} from '../modules/profile';

@Injectable({
  providedIn: 'root',
})
export class ProfileService {
  private baseUrl = `${Environment.baseUrl}/api/profiles`;

  constructor(private http: HttpClient) {}

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
      body: password,
    });
  }

  updateProfile(profile: IProfileUpdate): Observable<IProfile> {
    const patchDocument = this.createJsonPatchDocument(profile);
    return this.http.patch<IProfile>(`${this.baseUrl}/profile/json`, patchDocument);
  }

  updateProfileImage(file: File): Observable<IProfile> {
    const formData = new FormData();
    formData.append('profileImageFile', file);

    return this.http.patch<IProfile>(`${this.baseUrl}/profile/image`, formData);
  }

  private createJsonPatchDocument(profile: IProfileUpdate): any[] {
    const patchOps: any[] = [];

    // Add operations for each defined property
    if (profile.firstName !== undefined) {
      patchOps.push({
        op: 'replace',
        path: '/firstName',
        value: profile.firstName
      });
    }

    if (profile.lastName !== undefined) {
      patchOps.push({
        op: 'replace',
        path: '/lastName',
        value: profile.lastName
      });
    }

    if (profile.userName !== undefined) {
      patchOps.push({
        op: 'replace',
        path: '/userName',
        value: profile.userName
      });
    }

    if (profile.gender !== undefined) {
      patchOps.push({
        op: 'replace',
        path: '/gender',
        value: profile.gender
      });
    }

    if (profile.phoneNumber !== undefined) {
      patchOps.push({
        op: 'replace',
        path: '/phoneNumber',
        value: profile.phoneNumber
      });
    }

    return patchOps;
  }
}

