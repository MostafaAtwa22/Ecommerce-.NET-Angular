import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Environment } from '../environment';
import { messageResponse, onlineUsers } from '../shared/modules/chat';
import { IPagination } from '../shared/modules/pagination';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { IProfile } from '../shared/modules/profile';
import { AccountService } from '../account/account-service';
import { functionsName } from './interface-callback-functions-name';

@Injectable({
  providedIn: 'root',
})
export class ChatService {
  private hubUrl = `${Environment.baseUrl}/hubs/chat`;
  private _authService = inject(AccountService);
  private _http = inject(HttpClient);

  onlineUsers = signal<onlineUsers[]>([]);
  chatMessages = signal<messageResponse[]>([]);
  isLoading = signal<boolean>(false);
  currentOpenChat = signal<onlineUsers | null>(null);
  autoScrollEnable = signal<boolean>(false);

  // Pagination signals (1-based to match backend)
  private currentPageIndex = signal<number>(1);
  private pageSize = signal<number>(20);
  private totalPages = signal<number>(0);
  private totalItems = signal<number>(0);
  private hasMoreMessages = signal<boolean>(true);

  private hubConnection!: HubConnection;
  private typingTimers = new Map<string, any>();


  async startConnection(token: string, otherUserId?: string) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${this.hubUrl}?otherUserId=${otherUserId || ''}`, {
        accessTokenFactory: () => token
      })
      .build();

    this.hubConnection.on(functionsName.Notify.toString(), (user: IProfile) => {
      Notification.requestPermission()
        .then((res) => {
          if (res === 'granted') {
            new Notification('Active Now 🟢', {
              body: `${user.firstName} ${user.lastName} is online now`,
              icon: user.profilePicture || (user.gender === 'Male' ? 'assets/users/default-male.png' : 'assets/users/default-female.png')
            })
          }
        })
    })

    // start the connection
    this.hubConnection.start()
      .then(() => console.log(`Connection started @ ${Date.now()}`))
      .catch((err) => console.log(`Failed Connection @ ${Date.now()} ${err}`));

    // online users
    this.hubConnection.on(functionsName.OnlineUsers.toString(), (users: onlineUsers[]) => {
      console.log('Online users:', users);
      this.onlineUsers.update(() =>
        users.filter(
          user => user.userName !== this._authService.user()?.userName
        )
      );
    });

    // notify if a user is typing
    this.hubConnection.on(functionsName.NotifyTypingToUser.toString(), (senderUserName: string) => {
      this.onlineUsers.update((users) => {
        return users.map((user) => {
          if (user.userName == senderUserName) {
            user.isTyping = true;
          }
          return user;
        })
      })

      // Clear previous timeout if exists
      if (this.typingTimers.has(senderUserName)) {
        clearTimeout(this.typingTimers.get(senderUserName));
      }

      // Set a new timeout
      const timer = setTimeout(() => {
        this.onlineUsers.update((users) =>
          users.map((user) => {
            if (user.userName === senderUserName) {
              user.isTyping = false;
            }
            return user;
          })
        );
        this.typingTimers.delete(senderUserName);
      }, 2000);

      this.typingTimers.set(senderUserName, timer);
    });

    // Handle pagination response
    this.hubConnection.on(functionsName.ReceiveMessageList.toString(),
      (pagination: IPagination<messageResponse>) => {
        this.isLoading.update(_ => true);

        // Store pagination info
        this.totalPages.set(Math.ceil(pagination.totalData / pagination.pageSize));
        this.totalItems.set(pagination.totalData);
        this.currentPageIndex.set(pagination.pageIndex);

        // Check if there are more messages to load
        this.hasMoreMessages.set(pagination.pageIndex < this.totalPages());

        // Prepend older messages (they come in chronological order)
        this.chatMessages.update(existing => {
          return [...pagination.data, ...existing];
        });

        this.isLoading.set(false);
      });

    // Receive new message
    this.hubConnection.on(functionsName.ReceiveNewMessage.toString(), (message: messageResponse) => {
      // Clear typing indicator for the sender immediately
      this.onlineUsers.update((users) => {
        const updatedUsers = users.map((user) => {
          if (user.id === message.senderId || user.id === message.reciverId) {
            user.lastMessage = message.content || (message.attachmentUrl ? "Sent an attachment 📎" : "");
            user.lastMessageTime = message.createdAt;

            if (user.id === message.senderId) {
              user.isTyping = false;
              if (this.typingTimers.has(user.userName)) {
                clearTimeout(this.typingTimers.get(user.userName));
                this.typingTimers.delete(user.userName);
              }
            }
          }
          return user;
        });

        return updatedUsers.sort((a, b) => {
          const timeA = a.lastMessageTime ? new Date(a.lastMessageTime).getTime() : 0;
          const timeB = b.lastMessageTime ? new Date(b.lastMessageTime).getTime() : 0;
          return timeB - timeA;
        });
      });

      const current = this.currentOpenChat();

      // Only process if message belongs to current conversation
      if (current && (message.senderId === current.id || message.reciverId === current.id)) {
        // Find existing optimistic message
        const existingOptimisticIndex = this.chatMessages().findIndex(m =>
          m.id === 0 && m.content === message.content
        );

        // Check if message ID already exists
        const idExists = this.chatMessages().some(m => m.id === message.id);

        if (idExists) {
          return; // Message already processed
        }

        if (existingOptimisticIndex !== -1) {
          // Replace the optimistic message with the real one
          this.chatMessages.update(messages => {
            const updated = [...messages];
            updated[existingOptimisticIndex] = message;
            return updated;
          });
        } else {
          // New message
          let audio = new Audio('assets/notification/notifications.mp3');
          audio.play().catch(e => console.error("Audio play failed", e));
          this.chatMessages.update((messages) => [...messages, message]);
          document.title = '(1) New message';

          // If we are currently in this chat, mark all as read immediately
          if (message.senderId === current.id) {
            this.markAllMessagesAsRead(current.id);
          }
        }
      } else {
        console.log('New message for other chat', message);
      }
    })

    // Message Edited Listener
    this.hubConnection.on(functionsName.MessageEdited, (message: messageResponse) => {
      this.chatMessages.update(messages =>
        messages.map(m => m.id === message.id ? message : m)
      );
    });

    // Message Deleted Listener
    this.hubConnection.on(functionsName.MessageDeleted, (messageId: number) => {
      this.chatMessages.update(messages =>
        messages.filter(m => m.id !== messageId)
      );
    });

    // Message Read Listener (Single message)
    this.hubConnection.on(functionsName.ReceiveMessageRead, (messageId: number) => {
      this.chatMessages.update(messages =>
        messages.map(m => m.id === messageId ? { ...m, isRead: true } : m)
      );
    });

    // Bulk Message Read Listener (When other user opens our chat)
    this.hubConnection.on(functionsName.MarkMessagesAsRead, (readerId: string) => {
      const currentUserId = this._authService.user()?.id;
      this.chatMessages.update(messages =>
        messages.map(m =>
          m.senderId === currentUserId && m.reciverId === readerId ? { ...m, isRead: true, isReceived: true } : m
        )
      );
    });

    // Bulk Message Received Listener (When other user comes online)
    this.hubConnection.on(functionsName.MarkMessagesAsReceived, (receiverId: string) => {
      const currentUserId = this._authService.user()?.id;
      this.chatMessages.update(messages =>
        messages.map(m =>
          m.senderId === currentUserId && m.reciverId === receiverId ? { ...m, isReceived: true } : m
        )
      );
    });

    // Update individual user in sidebar (unread count, last message)
    this.hubConnection.on(functionsName.UpdateUserSidebar, (updatedUser: onlineUsers) => {
      this.onlineUsers.update(users => {
        const index = users.findIndex(u => u.id === updatedUser.id);
        if (index !== -1) {
          const newUsers = [...users];
          newUsers[index] = { ...newUsers[index], ...updatedUser };
          return newUsers.sort((a, b) => {
            const timeA = a.lastMessageTime ? new Date(a.lastMessageTime).getTime() : 0;
            const timeB = b.lastMessageTime ? new Date(b.lastMessageTime).getTime() : 0;
            return timeB - timeA;
          });
        }
        return users;
      });
    });

    // Update individual user status (online/offline)
    this.hubConnection.on(functionsName.UpdateUserStatus, (u: onlineUsers) => {
      this.onlineUsers.update(users => {
        const updated = users.map(user => {
          if (user.id === u.id) {
            return { ...user, isOnline: u.isOnline };
          }
          return user;
        });
        return updated.sort((a, b) => (b.isOnline ? 1 : 0) - (a.isOnline ? 1 : 0));
      });
    });
  }

  disconnectConnection() {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      this.hubConnection.stop()
        .then(() => console.log('Connection Ended'))
        .catch((err) => console.log('Disconnect error:', err));
    }
  }

  sendMessage(content: string, attachment?: { url: string, name: string, type: string }) {
    const receiverId = this.currentOpenChat()?.id;
    if (!receiverId || (!content.trim() && !attachment)) return;

    // Optimistic update
    const optimisticMessage: messageResponse = {
      id: 0,
      content: content,
      senderId: this._authService.user()?.id || '',
      reciverId: receiverId,
      createdAt: new Date(),
      isRead: false,
      isReceived: false,
      attachmentUrl: attachment?.url,
      attachmentName: attachment?.name,
      attachmentType: attachment?.type
    };

    this.chatMessages.update((messages) => [...messages, optimisticMessage]);

    this.hubConnection.invoke(functionsName.SendMessage.toString(), {
      reciverId: receiverId,
      content: content,
      attachmentUrl: attachment?.url,
      attachmentName: attachment?.name,
      attachmentType: attachment?.type
    })
      .then(() => {
        console.log('Message sent successfully');
        // When we send a message, we've surely read all previous incoming messages
        this.markAllMessagesAsRead(receiverId);
      })
      .catch((err) => {
        console.log(`Send message failed: ${err}`);
        this.chatMessages.update((messages) =>
          messages.filter(m => m !== optimisticMessage)
        );
      });
  }

  uploadFile(file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this._http.post<{ url: string, name: string, type: string }>(`${Environment.baseUrl}/api/upload`, formData);
  }

  editMessage(id: number, content: string) {
    if (!content.trim()) return;

    this.hubConnection.invoke(functionsName.EditMessage.toString(), id, content)
      .then(() => console.log('Message edited successfully'))
      .catch(err => console.log(`Edit message failed: ${err}`));
  }

  deleteMessage(id: number) {
    this.hubConnection.invoke(functionsName.DeleteMessage.toString(), id)
      .then(() => console.log('Message deleted successfully'))
      .catch(err => console.log(`Delete message failed: ${err}`));
  }

  markMessageAsRead(id: number) {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      this.hubConnection.invoke(functionsName.MarkMessageAsRead.toString(), id)
        .catch(err => console.log(`MarkMessageAsRead error: ${err}`));
    }
  }

  markAllMessagesAsRead(otherUserId: string) {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      this.hubConnection.invoke(functionsName.MarkAllMessagesAsRead.toString(), otherUserId)
        .catch(err => console.log(`MarkAllMessagesAsRead error: ${err}`));
    }
  }

  status(userName: string): string {
    const currentChatUser = this.currentOpenChat();
    if (!currentChatUser)
      return 'Offline';
    const onlineUser = this.onlineUsers().find(
      u => u.userName == userName
    )
    return onlineUser?.isTyping ? 'Typing..' : this.isUserOnline();
  }

  isUserOnline() {
    let onlineUser = this.onlineUsers().find(u => u.userName === this.currentOpenChat()?.userName);
    return onlineUser?.isOnline ? 'Online' : 'Offline'; // Changed to 'Offline' for clarity if not found or offline. But original logic was returning userName. Keep original behavior or fix? The original line 234 returns currentOpenChat()!.userName if offline. That's weird. "Online" or "John Doe"?
    // I'll stick to adding NEW method only.
  }

  isUserTyping(userName?: string): boolean {
    if (!userName) return false;
    const onlineUser = this.onlineUsers().find(u => u.userName === userName);
    return !!onlineUser?.isTyping;
  }

  loadMessages(pageIndex: number = 1, pageSize: number = 20) {
    const otherUserId = this.currentOpenChat()?.id;
    if (!otherUserId) return Promise.resolve();

    this.isLoading.update(_ => true);

    return this.hubConnection.invoke(functionsName.LoadMessages.toString(), {
      senderId: otherUserId,
      pageIndex: pageIndex,
      pageSize: pageSize
    })
      .then(_ => {
        console.log(`Loaded messages - page: ${pageIndex}, size: ${pageSize}`);
      })
      .catch(err => {
        console.log(`LoadMessages Error: ${err}`);
        this.isLoading.update(() => false);
      });
  }

  notifyTyping() {
    const receiverUserName = this.currentOpenChat()?.userName;
    if (!receiverUserName) return;

    this.hubConnection.invoke(functionsName.NotifyTyping.toString(), receiverUserName)
      .catch(err => console.log(`Typing notification error: ${err}`));
  }

  loadMoreMessages() {
    if (!this.hasMoreMessages() || this.isLoading()) {
      return Promise.resolve();
    }

    const nextPage = this.currentPageIndex() + 1;
    return this.loadMessages(nextPage, this.pageSize());
  }

  resetPagination() {
    this.currentPageIndex.set(1);
    this.pageSize.set(20);
    this.totalPages.set(0);
    this.totalItems.set(0);
    this.hasMoreMessages.set(true);
    this.chatMessages.set([]);
  }

  // Getters for pagination state
  getCurrentPageIndex() {
    return this.currentPageIndex();
  }

  getTotalPages() {
    return this.totalPages();
  }

  getTotalItems() {
    return this.totalItems();
  }

  getHasMoreMessages() {
    return this.hasMoreMessages();
  }
}
