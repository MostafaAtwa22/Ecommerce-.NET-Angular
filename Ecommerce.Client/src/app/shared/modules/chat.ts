export interface onlineUsers {
  id: string,
  connectionId: string,
  userName: string,
  email: string,
  firstName: string,
  lastName: string,
  profilePictureUrl?: string,
  phoneNumber?: string,
  roles?: string[],
  isOnline: boolean,
  unReadCount: number,
  isTyping: boolean
}

export interface messageResponse {
  id: number,
  createdAt: Date,
  content: string,
  senderId: string,
  reciverId: string,
  isRead: boolean,
  isEdited?: boolean,
  isDeleted?: boolean,
}

export interface messageRequest {
  createdAt: Date,
  content: string,
  senderId: string,
  isRead: boolean,
}
