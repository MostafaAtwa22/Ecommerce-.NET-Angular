import { Injectable } from '@angular/core';
import { Environment } from '../../environment';
import { IAddress } from '../modules/address';
import { Observable, of, tap } from 'rxjs';
import { HttpClient, HttpParams } from '@angular/common/http';
import {
  IChangePassword,
  IDeleteAccount,
  IProfile,
  IProfileUpdate,
  ISetPassword,
} from '../modules/profile';
import { IPagination, Pagination } from '../modules/pagination';
import { UserParams } from '../modules/UserParams ';

@Injectable({
  providedIn: 'root',
})
export class ProfileService {
  private baseUrl = `${Environment.baseUrl}/api/profiles`;

  private cache = new Map<number, IProfile[]>();
  pagination = new Pagination<IProfile>();
  userParams = new UserParams();

  constructor(private http: HttpClient) {}

  getAllUsers(useCache: boolean = true): Observable<IPagination<IProfile>> {
    const cachedUsers = this.cache.get(1) || [];

    if (!useCache) this.cache.set(1, []);

    const pagesReceived = Math.ceil(cachedUsers.length / this.userParams.pageSize);

    if (useCache && cachedUsers.length > 0 && this.userParams.pageIndex <= pagesReceived) {
      this.pagination.data = cachedUsers.slice(
        (this.userParams.pageIndex - 1) * this.userParams.pageSize,
        this.userParams.pageIndex * this.userParams.pageSize
      );
      this.pagination.pageIndex = this.userParams.pageIndex;
      this.pagination.pageSize = this.userParams.pageSize;
      this.pagination.totalData = cachedUsers.length;

      return of(this.pagination);
    }

    let params = new HttpParams()
      .set('pageIndex', this.userParams.pageIndex)
      .set('pageSize', this.userParams.pageSize)
      .set('sort', this.userParams.sort);

    if (this.userParams.search) params = params.set('search', this.userParams.search);

    if (this.userParams.role) params = params.set('role', this.userParams.role);

    return this.http.get<IPagination<IProfile>>(`${this.baseUrl}/users`, { params }).pipe(
      tap((response) => {
        const currentCache = this.cache.get(1) || [];
        this.cache.set(1, [...currentCache, ...response.data]);
        this.pagination = response;
      })
    );
  }

  setUserParams(params: UserParams) {
    this.userParams = params;
  }

  getUserParams() {
    return this.userParams;
  }

  resetUserParams() {
    this.userParams = new UserParams();
    return this.userParams;
  }

  getUser(id: string): Observable<IProfile> {
    const cachedUsers = this.cache.get(1) || [];
    const user = cachedUsers.find((u) => u.id === id);

    return user ? of(user) : this.http.get<IProfile>(`${this.baseUrl}/users/${id}`);
  }

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
    formData.append('ProfileImageFile', file, file.name);

    return this.http.patch<IProfile>(`${this.baseUrl}/profile/image`, formData);
  }

  deleteProfileImage(): Observable<IProfile> {
    return this.http.delete<IProfile>(`${this.baseUrl}/profile/image`);
  }

  private createJsonPatchDocument(profile: IProfileUpdate): any[] {
    const patchOps: any[] = [];

    // Add operations for each defined property
    if (profile.firstName !== undefined) {
      patchOps.push({
        op: 'replace',
        path: '/firstName',
        value: profile.firstName,
      });
    }

    if (profile.lastName !== undefined) {
      patchOps.push({
        op: 'replace',
        path: '/lastName',
        value: profile.lastName,
      });
    }

    if (profile.userName !== undefined) {
      patchOps.push({
        op: 'replace',
        path: '/userName',
        value: profile.userName,
      });
    }

    if (profile.gender !== undefined) {
      patchOps.push({
        op: 'replace',
        path: '/gender',
        value: profile.gender,
      });
    }

    if (profile.phoneNumber !== undefined) {
      patchOps.push({
        op: 'replace',
        path: '/phoneNumber',
        value: profile.phoneNumber,
      });
    }

    return patchOps;
  }
}
