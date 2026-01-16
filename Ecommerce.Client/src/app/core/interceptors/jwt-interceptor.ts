import { HttpInterceptorFn, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AccountService } from '../../account/account-service';
import { catchError, throwError } from 'rxjs';
import { isTokenExpired } from '../../shared/utils/token-utils';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('token');
  const accountService = inject(AccountService);

  // Always add credentials for refresh-token endpoint (carries refresh token cookie)
  if (req.url.includes('/refresh-token')) {
    console.log('Refresh token endpoint - sending with credentials only');
    return next(req.clone({ withCredentials: true }));
  }

  // Skip auth headers for public endpoints
  const publicEndpoints = [
    '/login',
    '/register',
    '/googlelogin',
    '/home',
    '/revoke-token',
    '/email-verification',
    '/forgetpassword',
    '/resetpassword',
    '/products',
    '/categories'
  ];

  const isPublicEndpoint = publicEndpoints.some(endpoint =>
    req.url.includes(endpoint)
  );

  if (isPublicEndpoint) {
    console.log('Public endpoint, skipping auth:', req.url);
    return next(req);
  }

  // Add token if available
  if (token) {
    try {
      const isExpired = isTokenExpired(token);

      if (!isExpired) {
        console.log('Adding valid token to request:', req.url);
        req = req.clone({
          setHeaders: { Authorization: `Bearer ${token}` },
          withCredentials: true
        });
      } else {
        // Token expired - let error interceptor handle refresh
        console.warn('Token expired for:', req.url, '- will trigger refresh via error interceptor');
        return next(req.clone({ withCredentials: true }));
      }
    } catch (error) {
      console.warn('Invalid token format:', error);
      return next(req.clone({ withCredentials: true }));
    }
  } else {
    console.warn('No token found for protected endpoint:', req.url);
    return next(req.clone({ withCredentials: true }));
  }

  return next(req);
};
