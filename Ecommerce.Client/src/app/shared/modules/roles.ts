export interface IRole {
  id: string;
  name: string;
  userCount: number;
}

export interface IRoleCreate {
  name: string;
}

export interface IPermissionCheckbox {
  permissionName: string;
  module: string;
  action: string;
  isSelected: boolean;
}

export interface IRolePermissions {
  roleId: string;
  roleName: string;
  permissions: IPermissionCheckbox[];
}

export interface ICheckBoxRoleManage {
  roleId: string;
  roleName: string;
  isSelected: boolean;
}

export interface IUserRoles {
  userId: string;
  userName: string;
  email: string;
  roles: ICheckBoxRoleManage[];
}
