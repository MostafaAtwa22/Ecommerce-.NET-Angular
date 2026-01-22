import { AuthConfig } from 'angular-oauth2-oidc';
import { Environment } from '../environment';

export const googleAuthConfig: AuthConfig = {
  // Google issuer
  issuer: Environment.issuer,

  // Redirect URI after login
  redirectUri: window.location.origin + '/signin-google',

  // Your Google client ID
  clientId: Environment.googleClientId,

  responseType: 'token id_token',
  
  // Scopes
  scope: 'openid profile email',

  // Optional
  strictDiscoveryDocumentValidation: false,
  showDebugInformation: true,

  // Skip issuer check to prevent CORS issues
  skipIssuerCheck: true,

  customQueryParams: {
    prompt: 'select_account'
  }
};
