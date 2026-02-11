import { Component, ElementRef, inject, OnInit, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ChatBoxComponent } from '../chat-box.component/chat-box.component';
import { ChatService } from '../chat.service';
import { PickerComponent } from '@ctrl/ngx-emoji-mart';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-chat-window',
  standalone: true,
  imports: [FormsModule, ChatBoxComponent, PickerComponent, CommonModule],
  templateUrl: './chat-window.component.html',
  styleUrl: './chat-window.component.scss',
})
export class ChatWindowComponent implements OnInit {
  @ViewChild('chatBox') chatContainer?: ElementRef;
  isLoading: boolean = true;

  ngOnInit() {
    this.isLoading = true;
    this.loadChat();
  }

  loadChat() {
    setTimeout(() => {
      this.isLoading = false;
    }, 1500);
  }

  _chatService = inject(ChatService);

  message: string = '';
  editingMessageId: number | null = null;
  showEmojiPicker = false;

  toggleEmojiPicker() {
    this.showEmojiPicker = !this.showEmojiPicker;
  }
  addEmoji(event: any) {
    this.message += event.emoji.native;
    this.showEmojiPicker = false;
  }
  onKeyDown(event: KeyboardEvent) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    } else if (event.key === 'Escape' && this.editingMessageId) {
      this.cancelEdit();
    }
  }

  onEditMessage(message: any) {
    this.editingMessageId = message.id;
    this.message = message.content;
    const textarea = document.querySelector('.chat-textarea') as HTMLTextAreaElement;
    if (textarea) textarea.focus();
  }

  cancelEdit() {
    this.editingMessageId = null;
    this.message = '';
  }

  sendMessage() {
    const text = this.message.trim();
    if (!text) return;

    if (this.editingMessageId) {
      this._chatService.editMessage(this.editingMessageId, text);
      this.editingMessageId = null;
    } else {
      this._chatService.sendMessage(text);
    }

    this.message = '';
    this.scrollToBottom();
    const textarea = document.querySelector('.chat-textarea') as HTMLTextAreaElement;
    if (textarea) textarea.style.height = 'auto';

    this.showEmojiPicker = false;
  }

  autoResize(event: Event) {
    const textarea = event.target as HTMLTextAreaElement;
    textarea.style.height = 'auto';
    textarea.style.height = `${textarea.scrollHeight}px`;
  }

  // Close emoji picker when clicking outside
  onOutsideClick() {
    this.showEmojiPicker = false;
  }

  private scrollToBottom() {
    if (this.chatContainer)
      this.chatContainer.nativeElement.scrollToTop = this.chatContainer.nativeElement.scrollHeight;
  }

  toggleLeftSidebar() {
    const sidebar = document.querySelector('.chat-left-sidebar');
    const overlay = document.querySelector('.sidebar-overlay');

    if (sidebar && overlay) {
      sidebar.classList.toggle('show');
      overlay.classList.toggle('show');
    }
  }

  toggleRightSidebar() {
    const sidebar = document.querySelector('.chat-right-sidebar');
    const overlay = document.querySelector('.sidebar-overlay');

    if (sidebar && overlay) {
      sidebar.classList.toggle('show');
      overlay.classList.toggle('show');
    }
  }
}
