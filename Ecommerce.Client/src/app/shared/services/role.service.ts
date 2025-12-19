import { Injectable } from '@angular/core';
import { Observable, shareReplay, tap } from 'rxjs';
import { IRole, IRoleCreate } from '../modules/roles';
import { HttpClient } from '@angular/common/http';
import { Environment } from '../../environment';

@Injectable({
  providedIn: 'root',
})
export class RoleService {
  private baseUrl = `${Environment.baseUrl}/api/roles`;

  private rolesCache$?: Observable<IRole[]>;

  constructor(private http: HttpClient) {}

  getAllRoles(): Observable<IRole[]> {
    if (!this.rolesCache$) {
      this.rolesCache$ = this.http
        .get<IRole[]>(this.baseUrl)
        .pipe(shareReplay(1));
    }

    return this.rolesCache$;
  }

  getRoleById(id: string): Observable<IRole> {
    return this.http.get<IRole>(`${this.baseUrl}/${id}`);
  }

  createRole(role: IRoleCreate): Observable<IRole> {
    return this.http.post<IRole>(this.baseUrl, role).pipe(
      tap(() => this.clearCache())
    );
  }

  deleteRole(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(
      tap(() => this.clearCache())
    );
  }

  private clearCache(): void {
    this.rolesCache$ = undefined;
  }
}
