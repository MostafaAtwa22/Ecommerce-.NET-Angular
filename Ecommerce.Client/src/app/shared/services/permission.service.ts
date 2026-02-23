import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, catchError, of } from 'rxjs';
import { Environment } from '../../environment';

@Injectable({
    providedIn: 'root'
})
export class PermissionService {
    private readonly baseUrl = `${Environment.baseUrl}/api/account`;
    private readonly CACHE_TTL = 5 * 60 * 1000;

    private permissionsSubject = new BehaviorSubject<string[]>([]);
    private lastFetchTime: number = 0;
    private isFetching = false;


    public permissions$ = this.permissionsSubject.asObservable();

    constructor(private http: HttpClient) { }

    fetchPermissions(forceRefresh: boolean = false): Observable<string[]> {
        const now = Date.now();
        const isCacheValid = (now - this.lastFetchTime) < this.CACHE_TTL;

        if (!forceRefresh && isCacheValid && this.permissionsSubject.value.length > 0) {
            return of(this.permissionsSubject.value);
        }

        if (this.isFetching) {
            return this.permissions$;
        }

        this.isFetching = true;

        return this.http.get<string[]>(`${this.baseUrl}/permissions`, { withCredentials: true })
            .pipe(
                tap(permissions => {
                    this.permissionsSubject.next(permissions);
                    this.lastFetchTime = Date.now();
                    this.isFetching = false;
                }),
                catchError(error => {
                    console.error('Failed to fetch permissions:', error);
                    this.isFetching = false;
                    return of(this.permissionsSubject.value);
                })
            );
    }

    hasPermission(permission: string): Observable<boolean> {
        return new Observable(observer => {
            this.fetchPermissions().subscribe(permissions => {
                observer.next(permissions.includes(permission));
                observer.complete();
            });
        });
    }

    hasPermissionSync(permission: string): boolean {
        return this.permissionsSubject.value.includes(permission);
    }

    hasAllPermissions(permissions: string[]): Observable<boolean> {
        return new Observable(observer => {
            this.fetchPermissions().subscribe(userPermissions => {
                const hasAll = permissions.every(p => userPermissions.includes(p));
                observer.next(hasAll);
                observer.complete();
            });
        });
    }

    hasAnyPermission(permissions: string[]): Observable<boolean> {
        return new Observable(observer => {
            this.fetchPermissions().subscribe(userPermissions => {
                const hasAny = permissions.some(p => userPermissions.includes(p));
                observer.next(hasAny);
                observer.complete();
            });
        });
    }

    refreshPermissions(): Observable<string[]> {
        return this.fetchPermissions(true);
    }

    getCurrentPermissions(): string[] {
        return this.permissionsSubject.value;
    }

    setPermissions(permissions: string[]): void {
        this.permissionsSubject.next(permissions);
        this.lastFetchTime = Date.now();
    }

    clearCache(): void {
        this.permissionsSubject.next([]);
        this.lastFetchTime = 0;
    }


    isCacheValid(): boolean {
        const now = Date.now();
        return (now - this.lastFetchTime) < this.CACHE_TTL;
    }
}
