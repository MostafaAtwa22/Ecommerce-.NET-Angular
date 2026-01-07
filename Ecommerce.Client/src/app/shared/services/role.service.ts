import { Injectable } from '@angular/core';
import { Observable, shareReplay, tap } from 'rxjs';
import { IRole, IRoleCreate, IUserRoles, IRolePermissions, IPermissionCheckbox } from '../modules/roles';
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
      this.rolesCache$ = this.http.get<IRole[]>(this.baseUrl).pipe(shareReplay(1));
    }
    return this.rolesCache$;
  }

  getRoleById(id: string): Observable<IRole> {
    return this.http.get<IRole>(`${this.baseUrl}/${id}`);
  }

  createRole(role: IRoleCreate): Observable<IRole> {
    return this.http.post<IRole>(this.baseUrl, role).pipe(tap(() => this.clearCache()));
  }

  deleteRole(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(tap(() => this.clearCache()));
  }


  getManageUserRoles(userId: string): Observable<IUserRoles> {
    return this.http.get<IUserRoles>(`${this.baseUrl}/manage-user-roles/${userId}`);
  }

  updateUserRoles(userRolesDto: IUserRoles): Observable<IUserRoles> {
    return this.http.put<IUserRoles>(`${this.baseUrl}/update-role`, userRolesDto);
  }

  getManagePermissions(roleId: string): Observable<IRolePermissions> {
    return this.http.get<IRolePermissions>(`${this.baseUrl}/manage-permissions/${roleId}`);
  }

  updateRolePermissions(roleId: string, permissions: IPermissionCheckbox[]): Observable<IRolePermissions> {
    return this.http.put<IRolePermissions>(`${this.baseUrl}/update-permissions/${roleId}`, permissions);
  }

  private clearCache(): void {
    this.rolesCache$ = undefined;
  }
}
