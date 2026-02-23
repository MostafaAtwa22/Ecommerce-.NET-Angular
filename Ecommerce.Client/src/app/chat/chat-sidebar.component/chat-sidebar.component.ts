import { Component, effect, inject, signal } from '@angular/core';
import { TitleCasePipe, CommonModule, NgIf, NgClass } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TypingIndicatorComponent } from '../typing-indicator.component/typing-indicator.component';
import { ChatService } from '../chat.service';
import { AccountService } from '../../account/account-service';
import { onlineUsers } from '../../shared/modules/chat';
import { getDefaultAvatarByGender } from '../../shared/utils/avatar-utils';

@Component({
  selector: 'app-chat-sidebar',
  imports: [TitleCasePipe, CommonModule, FormsModule, TypingIndicatorComponent],
  templateUrl: './chat-sidebar.component.html',
  styleUrl: './chat-sidebar.component.scss',
})
export class ChatSidebarComponent {
  private _router = inject(Router);
  public _authService = inject(AccountService);
  public _chatService = inject(ChatService);

  onlineUsers = this._chatService.onlineUsers;
  filteredUsers = signal<onlineUsers[]>([]);
  searchTerm = '';

  constructor() {
    effect(() => {
      const users = this.onlineUsers();
      const term = this.searchTerm.toLowerCase().trim();
      const filtered = users.filter((u) => u.firstName?.toLowerCase().includes(term));
      this.filteredUsers.set(filtered);
    });
  }

  ngOnInit() {
    const currentUserId = this._authService.user()?.id;
    const token = this._authService.user()?.token;
    if (token) {
      this._chatService.startConnection(token, currentUserId);
    } else {
      // No token available yet; avoid starting the connection to prevent type errors.
      console.warn('Chat startConnection skipped: no auth token available');
    }
  }

  filterUsers() {
    const term = this.searchTerm.toLowerCase().trim();
    const filtered = this.onlineUsers().filter((u) => u.firstName?.toLowerCase().includes(term));
    this.filteredUsers.set(filtered);
  }

  openChatWindow(user: onlineUsers) {
    this._chatService.currentOpenChat.set(user);
    this._chatService.chatMessages.set([]);
    this._chatService.isLoading.set(true);
    this._chatService.loadMessages(1);
  }

  setDefaultAvatar(event: Event, gender?: any) {
    const img = event.target as HTMLImageElement;
    img.src = getDefaultAvatarByGender(gender);
  }

  getAvatar(profilePicture?: string | null, gender?: any): string {
    return profilePicture || getDefaultAvatarByGender(gender);
  }
}
