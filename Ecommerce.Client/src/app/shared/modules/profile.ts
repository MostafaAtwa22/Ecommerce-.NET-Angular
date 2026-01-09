export interface IProfile {
  firstName: string;
  lastName: string;
  userName: string;
  email: string;
  gender: string;
  profilePicture: string;
  id: string;
  phoneNumber: string;
  roles: string[];
  isLocked: boolean;
}

export interface IProfileUpdate {
  firstName?: string;
  lastName?: string;
  userName?: string;
  gender?: string;
  phoneNumber?: string;
}

export interface IProfileImageUpdate {
  ProfileImageFile: File;
}

export interface IChangePassword {
  oldPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

export interface ISetPassword {
  password: string;
  confirmPassword: string;
}

export interface IDeleteAccount {
  password: string;
}
