import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Environment } from '../environment';
import { IAccountUser } from '../shared/modules/accountUser';
import { ILogin } from '../shared/modules/login';
import { IRegister } from '../shared/modules/register';
import { tap } from 'rxjs';
import { isTokenExpired } from '../shared/utils/token-utils';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private baseUrl = `${Environment.baseUrl}/api/account`;

  private userSignal = signal<IAccountUser | null>(this.getUserFromLocalStorage());

  isLoggedIn = computed(() => !!this.userSignal());

  constructor(private http: HttpClient, private router: Router) {}

  login(loginData: ILogin) {
    return this.http
      .post<IAccountUser>(`${this.baseUrl}/login`, loginData)
      .pipe(tap((user) => this.setUser(user)));
  }

  register(registerData: IRegister) {
    const token = localStorage.getItem('token');
    const headers = token ? { Authorization: `Bearer ${token}` } : undefined;

    return this.http.post<IAccountUser>(`${this.baseUrl}/register`, registerData, { headers }).pipe(
      tap((user) => {
        const existingToken = localStorage.getItem('token');
        if (!existingToken) {
          this.setUser(user);
        }
      })
    );
  }

  emailExists(email: string) {
    return this.http.get<boolean>(`${this.baseUrl}/emailexists/${email}`);
  }

  usernameExists(username: string) {
    return this.http.get<boolean>(`${this.baseUrl}/usernameexists/${username}`);
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.userSignal.set(null);
    this.router.navigate(['/']);
  }

  user() {
    return this.userSignal();
  }

  loadCurrentUser() {
    const token = localStorage.getItem('token');
    if (!token) {
      this.userSignal.set(null);
      return;
    }

    // Check if token is expired before loading user
    if (isTokenExpired(token)) {
      console.warn('Token has expired, logging out...');
      this.logout();
      return;
    }

    const storedUser = this.getUserFromLocalStorage();
    if (storedUser) {
      this.userSignal.set(storedUser);
    } else {
      // If getUserFromLocalStorage returns null, ensure user signal is cleared
      this.userSignal.set(null);
    }
  }

  private setUser(user: IAccountUser) {
    this.userSignal.set(user);
    localStorage.setItem('token', user.token);
    localStorage.setItem(
      'user',
      JSON.stringify({
        firstName: user.firstName,
        lastName: user.lastName,
        userName: user.userName,
        gender: user.gender,
        email: user.email,
        profilePicture: user.profilePicture,
        roles: user.roles,
      })
    );
  }

  private getUserFromLocalStorage(): IAccountUser | null {
    const token = localStorage.getItem('token');
    const userStr = localStorage.getItem('user');

    if (!token) {
      return null;
    }

    // Check if token is expired
    if (isTokenExpired(token)) {
      console.warn('Token has expired, clearing user data...');
      localStorage.removeItem('user');
      localStorage.removeItem('token');
      return null;
    }

    if (!userStr) {
      return null;
    }

    try {
      const userData = JSON.parse(userStr);
      if (!userData.firstName || !userData.email) {
        console.warn('Incomplete user data in localStorage, clearing...');
        localStorage.removeItem('user');
        localStorage.removeItem('token');
        return null;
      }

      return {
        ...userData,
        token,
      } as IAccountUser;
    } catch (error) {
      console.error('Failed to parse user data from localStorage:', error);
      localStorage.removeItem('user');
      localStorage.removeItem('token');
      return null;
    }
  }
  // ✔ Update only the profile picture in localStorage
  updateLocalUserProfilePicture(newUrl: string | null): void {
    const userStr = localStorage.getItem('user');
    if (!userStr) return;

    try {
      const userData = JSON.parse(userStr);
      userData.profilePicture = newUrl; // update picture
      localStorage.setItem('user', JSON.stringify(userData));

      // Update the signal
      const current = this.userSignal();
      if (current) {
        this.userSignal.set({
          ...current,
          profilePicture: newUrl,
        });
      }
    } catch (error) {
      console.error('Failed to update local user profile picture', error);
    }
  }

  clearLocalUserProfilePicture(): void {
    this.updateLocalUserProfilePicture(null);
  }
}
