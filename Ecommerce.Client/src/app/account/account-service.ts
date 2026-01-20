import { Injectable, signal, computed } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';
import { Environment } from '../environment';
import { IAccountUser, IEmailVerification, IForgetPassword, IResetPassword, JwtPayload } from '../shared/modules/accountUser';
import { ILogin } from '../shared/modules/login';
import { IRegister } from '../shared/modules/register';
import { tap, finalize, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AccountService {

  private baseUrl = `${Environment.baseUrl}/api/account`;

  private userSignal = signal<IAccountUser | null>(null);
  user = computed(() => this.userSignal());
  isLoggedIn = computed(() => !!this.userSignal());
  constructor(
    private http: HttpClient,
    private router: Router
  ) {}

  // 🔐 LOGIN
  login(loginData: ILogin) {
    return this.http
      .post<IAccountUser>(
        `${this.baseUrl}/login`,
        loginData,
        { withCredentials: true }
      )
      .pipe(
        tap(user => this.setUser(user))
      );
  }

  // 🔄 REFRESH TOKEN
  refreshToken(): Observable<IAccountUser> {
    return this.http
      .get<IAccountUser>(
        `${this.baseUrl}/refresh-token`,
        { withCredentials: true }
      )
      .pipe(
        tap(user => this.setUser(user))
      );
  }

  // 🚪 LOGOUT
  logout() {
    this.http
      .post(
        `${this.baseUrl}/logout`,
        {},
        { withCredentials: true }
      )
      .pipe(finalize(() => this.clearUserData()))
      .subscribe();
  }

  // 🔴 REVOKE TOKEN (Admin / Manual)
  revokeRefreshToken(token?: string) {
    return this.http.post(
      `${this.baseUrl}/revoke-token`,
      token ? { token } : {},
      { withCredentials: true }
    );
  }

  // ==========================
  // EMAIL & PASSWORD
  // ==========================

  register(registerData: IRegister) {
    return this.http.post(
      `${this.baseUrl}/register`,
      registerData,
      {
        withCredentials: true,
        responseType: 'text',
      }
    );
  }

  verifyEmail(dto: IEmailVerification) {
    return this.http
      .post<IAccountUser>(
        `${this.baseUrl}/email-verification`,
        dto,
        { withCredentials: true }
      )
      .pipe(tap(user => this.setUser(user)));
  }

  resendVerificationEmail(email: string) {
    return this.http.post(
      `${this.baseUrl}/resend-verification`,
      { email }
    );
  }

  forgetPassword(dto: IForgetPassword) {
    return this.http.post(
      `${this.baseUrl}/forgetpassword`,
      dto
    );
  }

  resendResetEmail(email: string) {
    const formData = new FormData();
    formData.append('email', email);

    return this.http.post(
      `${this.baseUrl}/resend-resetpassword`,
      formData
    );
  }

  resetPassword(dto: IResetPassword) {
    return this.http.post(
      `${this.baseUrl}/resetpassword`,
      dto,
      { responseType: 'text' }
    );
  }

  emailExists(email: string) {
    return this.http.get<boolean>(`${this.baseUrl}/emailexists/${email}`);
  }

  usernameExists(username: string) {
    return this.http.get<boolean>(`${this.baseUrl}/usernameexists/${username}`);
  }

  updateLocalUserProfilePicture(newUrl: string | null): void {
    const userStr = localStorage.getItem('user');
    if (!userStr) return;

    try {
      const userData = JSON.parse(userStr);
      userData.profilePicture = newUrl;
      localStorage.setItem('user', JSON.stringify(userData));

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

  hasPermission(permission: string): boolean {
    const currentUser = this.userSignal();
    return currentUser?.permissions?.includes(permission) ?? false;
  }

  hasRole(role: string): boolean {
    const currentUser = this.userSignal();
    return currentUser?.roles?.includes(role) ?? false;
  }

  // HELPERS
  private setUser(user: IAccountUser) {
    let permissions: string[] = [];

    if (user.token) {
      try {
        const payload = jwtDecode<JwtPayload>(user.token);
        permissions = payload.Permission || [];
      } catch (err) {
        console.error('Failed to decode JWT', err);
      }
    }

    this.userSignal.set({ ...user, permissions });

    localStorage.setItem(
      'user',
      JSON.stringify({
        firstName: user.firstName,
        lastName: user.lastName,
        userName: user.userName,
        email: user.email,
        roles: user.roles,
        permissions, // store permissions
        profilePicture: user.profilePicture,
        refreshTokenExpiration: user.refreshTokenExpiration,
        token: user.token
      })
    );
  }

  loadCurrentUser() {
    const userStr = localStorage.getItem('user');
    if (!userStr) return;

    try {
      const user = JSON.parse(userStr);
      this.userSignal.set(user);
    } catch {
      this.clearUserData();
    }
  }

  private clearUserData() {
    localStorage.removeItem('user');
    this.userSignal.set(null);
    this.router.navigate(['/login']);
  }
}
