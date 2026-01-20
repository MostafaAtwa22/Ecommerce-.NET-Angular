export interface IAccountUser {
  firstName: string
  lastName: string
  userName: string
  email: string
  gender: 'Male' | 'Female'
  profilePicture: string | null
  roles: string[]
  token: string
  permissions?: string[];
  refreshTokenExpiration: string
}

export interface IEmailVerification {
  email: string;
  code: string;
}

export interface IForgetPassword {
  email: string;
}

export interface IResetPassword {
  email: string;
  token: string;
  newPassword: string;
  confirmNewPassword: string;
}

export interface JwtPayload {
  email: string;
  nameid: string;
  roles: string[];
  Permission: string[];
}
