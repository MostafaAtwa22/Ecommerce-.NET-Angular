import { Component, inject } from '@angular/core';
import { ChatService } from '../chat.service';
import { DatePipe, TitleCasePipe } from '@angular/common';
import { getDefaultAvatarByGender } from '../../shared/utils/avatar-utils';

@Component({
  selector: 'app-chat-right-sidebar',
  imports: [TitleCasePipe],
  templateUrl: './chat-right-sidebar.component.html',
  styleUrl: './chat-right-sidebar.component.scss',
})
export class ChatRightSidebarComponent {
  _chatService = inject(ChatService);

  setDefaultAvatar(event: Event, gender?: any) {
    const img = event.target as HTMLImageElement;
    img.src = getDefaultAvatarByGender(gender);
  }

  getAvatar(profilePicture?: string | null, gender?: any): string {
    return profilePicture || getDefaultAvatarByGender(gender);
  }

  copyToClipboard(text?: string | null): void {
    if (!text) return;

    navigator.clipboard.writeText(text).then(() => {
      // You can add a toast notification here
      console.log('Copied to clipboard:', text);
    }).catch(err => {
      console.error('Failed to copy:', err);
    });
  }

  startVideoCall(): void {
    console.log('Video call started with:', this._chatService.currentOpenChat()?.userName);
    // Implement video call logic
  }

  startAudioCall(): void {
    console.log('Audio call started with:', this._chatService.currentOpenChat()?.userName);
    // Implement audio call logic
  }

  blockUser(): void {
    const userName = this._chatService.currentOpenChat()?.userName;
    if (confirm(`Are you sure you want to block ${userName}?`)) {
      console.log('User blocked:', userName);
      // Implement block user logic
    }
  }
}
