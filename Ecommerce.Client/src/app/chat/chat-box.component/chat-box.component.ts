import {
  AfterViewInit,
  Component,
  ElementRef,
  EventEmitter,
  inject,
  Output,
  ViewChild,
  HostListener,
  OnDestroy,
} from '@angular/core';
import { ChatService } from '../chat.service';
import { AccountService } from '../../account/account-service';
import { DatePipe } from '@angular/common';
import { getDefaultAvatarByGender } from '../../shared/utils/avatar-utils';
import { TypingIndicatorComponent } from '../typing-indicator.component/typing-indicator.component';

@Component({
  selector: 'app-chat-box',
  standalone: true,
  imports: [DatePipe, TypingIndicatorComponent],
  templateUrl: './chat-box.component.html',
  styleUrl: './chat-box.component.scss',
})
export class ChatBoxComponent implements AfterViewInit, OnDestroy {
  @ViewChild('chatBox', { read: ElementRef }) chatBox?: ElementRef;
  @Output() editMessage = new EventEmitter<any>();

  _chatService = inject(ChatService);
  _authService = inject(AccountService);

  isUserScrolling = false;
  private scrollThreshold = 50;
  private observer: MutationObserver | null = null;

  constructor() { }

  ngAfterViewInit(): void {
    this.setupScrollObserver();
    this.scrollToBottom();
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
  }

  // ============================================
  // Avatar Helpers
  // ============================================
  setDefaultAvatar(event: Event, gender?: any) {
    const img = event.target as HTMLImageElement;
    img.src = getDefaultAvatarByGender(gender);
  }

  getAvatar(profilePicture?: string | null, gender?: any): string {
    return profilePicture || getDefaultAvatarByGender(gender);
  }

  // ============================================
  // Message Actions
  // ============================================
  onEditMessage(message: any) {
    this.editMessage.emit(message);
  }

  onDeleteMessage(id: number) {
    this._chatService.deleteMessage(id);
  }

  // ============================================
  // Scroll Management
  // ============================================
  scrollToBottom(behavior: ScrollBehavior = 'smooth') {
    if (!this.chatBox || this.isUserScrolling) return;

    setTimeout(() => {
      const element = this.chatBox?.nativeElement;
      if (element && this._chatService.autoScrollEnable()) {
        element.scrollTo({
          top: element.scrollHeight,
          behavior: behavior,
        });
      }
    }, 50);
  }

  onScroll(event: Event) {
    if (!this.chatBox) return;

    const element = this.chatBox.nativeElement;
    const scrollTop = element.scrollTop;
    const scrollHeight = element.scrollHeight;
    const clientHeight = element.clientHeight;

    // Check if scrolled to bottom
    const atBottom = Math.abs(scrollHeight - scrollTop - clientHeight) < this.scrollThreshold;

    if (atBottom) {
      this._chatService.autoScrollEnable.set(true);
      this.isUserScrolling = false;
    } else {
      this._chatService.autoScrollEnable.set(false);
      this.isUserScrolling = true;
    }

    // Load more messages when scrolled to top
    if (scrollTop < 50 && this._chatService.getHasMoreMessages() && !this._chatService.isLoading()) {
      const oldScrollHeight = scrollHeight;

      this._chatService.loadMoreMessages()
        .then(() => {
          // Maintain scroll position after loading
          setTimeout(() => {
            if (this.chatBox) {
              const newScrollHeight = this.chatBox.nativeElement.scrollHeight;
              const scrollDiff = newScrollHeight - oldScrollHeight;

              this.chatBox.nativeElement.scrollTop = scrollDiff + 50;
            }
          }, 50);
        });
    }
  }

  // ============================================
  // Load More Messages
  // ============================================
  loadMoreMessages() {
    if (!this.chatBox || this._chatService.isLoading()) return;

    const element = this.chatBox.nativeElement;
    const oldScrollHeight = element.scrollHeight;
    const oldScrollTop = element.scrollTop;

    this._chatService.loadMoreMessages().then(() => {
      // Maintain scroll position after loading
      setTimeout(() => {
        if (this.chatBox) {
          const newScrollHeight = this.chatBox.nativeElement.scrollHeight;
          const scrollDiff = newScrollHeight - oldScrollHeight;

          this.chatBox.nativeElement.scrollTop = oldScrollTop + scrollDiff;
        }
      }, 50);
    });
  }

  // ============================================
  // Auto-scroll on new messages
  // ============================================
  private setupScrollObserver() {
    if (!this.chatBox) return;

    this.observer = new MutationObserver(() => {
      if (this._chatService.autoScrollEnable() && !this.isUserScrolling) {
        this.scrollToBottom();
      }
    });

    this.observer.observe(this.chatBox.nativeElement, {
      childList: true,
      subtree: true,
      characterData: true,
    });
  }


  @HostListener('window:resize')
  onWindowResize() {
    // Maintain scroll position on resize
    if (this._chatService.autoScrollEnable() && !this.isUserScrolling) {
      this.scrollToBottom('auto');
    }
  }
}