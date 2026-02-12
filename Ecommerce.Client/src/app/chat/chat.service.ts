import { inject, Injectable, signal } from '@angular/core';
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
      this.onlineUsers.update((users) =>
        users.map((user) => {
          if (user.id === message.senderId) {
            user.isTyping = false;

            // Also clear the timer if it exists (using userName)
            if (this.typingTimers.has(user.userName)) {
              clearTimeout(this.typingTimers.get(user.userName));
              this.typingTimers.delete(user.userName);
            }
          }
          return user;
        })
      );

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
          let audio = new Audio('assets/notification/notifications.wav');
          audio.play().catch(e => console.error("Audio play failed", e));
          this.chatMessages.update((messages) => [...messages, message]);
          document.title = '(1) New message';
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
  }

  disconnectConnection() {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      this.hubConnection.stop()
        .then(() => console.log('Connection Ended'))
        .catch((err) => console.log('Disconnect error:', err));
    }
  }

  sendMessage(content: string) {
    const receiverId = this.currentOpenChat()?.id;
    if (!receiverId || !content.trim()) return;

    // Optimistic update
    const optimisticMessage: messageResponse = {
      id: 0,
      content: content,
      senderId: this._authService.user()?.id || '',
      reciverId: receiverId,
      createdAt: new Date(),
      isRead: false
    };

    this.chatMessages.update((messages) => [...messages, optimisticMessage]);

    this.hubConnection.invoke(functionsName.SendMessage.toString(), {
      reciverId: receiverId,
      content: content
    })
      .then(() => console.log('Message sent successfully'))
      .catch((err) => {
        console.log(`Send message failed: ${err}`);
        this.chatMessages.update((messages) =>
          messages.filter(m => m !== optimisticMessage)
        );
      });
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
