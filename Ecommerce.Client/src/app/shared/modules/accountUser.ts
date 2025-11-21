export interface IAccountUser {
  firstName: string
  lastName: string
  userName: string
  email: string
  gender: 'Male' | 'Female'
  profilePicture: string
  roles: string[]
  token: string
}
