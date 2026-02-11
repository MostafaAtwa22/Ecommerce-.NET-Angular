import { AfterViewChecked, Component, ElementRef, EventEmitter, inject, Output, ViewChild } from '@angular/core';
import { ChatService } from '../chat.service';
import { AccountService } from '../../account/account-service';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-chat-box',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './chat-box.component.html',
  styleUrl: './chat-box.component.scss',
})
export class ChatBoxComponent implements AfterViewChecked {
  @ViewChild('chatBox', { read: ElementRef }) chatBox?: ElementRef;

  _chatService = inject(ChatService);
  _authService = inject(AccountService);

  @Output() editMessage = new EventEmitter<any>();

  onEditMessage(message: any) {
    this.editMessage.emit(message);
  }

  onDeleteMessage(id: number) {
    if (confirm('Are you sure you want to delete this message?')) {
      this._chatService.deleteMessage(id);
    }
  }

  isFakeLoading = true;

  constructor() {
    // Fake loading (UI only)
    setTimeout(() => {
      this.isFakeLoading = false;
    }, 2000);
  }

  ngAfterViewChecked(): void {
    if (this._chatService.autoScrollEnable()) {
      this.scrollToBottom();
    }
  }

  scrollToBottom() {
    if (!this.chatBox) return;

    this.chatBox.nativeElement.scrollTo({
      top: this.chatBox.nativeElement.scrollHeight,
      behavior: 'smooth',
    });
  }

  onScrollTop() {
    this._chatService.autoScrollEnable.set(false);
  }
  loadMoreMessages() {
    this._chatService.loadMoreMessages();
  }
  enableAutoScroll() {
    this._chatService.autoScrollEnable.set(true);
  }

  isUserScrolling = false;

  onScroll(event: Event) {
    if (!this.chatBox) return;

    const element = this.chatBox.nativeElement;

    const atBottom = Math.abs(element.scrollHeight - element.scrollTop - element.clientHeight) < 50;

    if (atBottom) {
      this._chatService.autoScrollEnable.set(true);
      this.isUserScrolling = false;
    } else {
      this._chatService.autoScrollEnable.set(false);
      this.isUserScrolling = true;
    }

    if (element.scrollTop === 0 && this._chatService.getHasMoreMessages() && !this._chatService.isLoading()) {
      const oldHeight = element.scrollHeight;
      this._chatService.loadMoreMessages();
    }
  }
}
