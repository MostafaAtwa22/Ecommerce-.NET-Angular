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
  isTyping: boolean,
  gender?: string,
  createdAt?: Date,
  lastMessage?: string,
  lastMessageTime?: Date
}

export interface messageResponse {
  id: number,
  createdAt: Date,
  content: string,
  senderId: string,
  reciverId: string,
  isRead: boolean,
  isReceived: boolean,
  isEdited?: boolean,
  isDeleted?: boolean,
  attachmentUrl?: string,
  attachmentName?: string,
  attachmentType?: string,
}

export interface messageRequest {
  createdAt: Date,
  content: string,
  senderId: string,
  isRead: boolean,
  attachmentUrl?: string,
  attachmentName?: string,
  attachmentType?: string,
}
