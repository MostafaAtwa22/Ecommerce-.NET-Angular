export interface IAccountUser {
  firstName: string
  lastName: string
  userName: string
  email: string
  gender: 'Male' | 'Female'
  profilePicture: string | null
  roles: string[]
  token: string
}
