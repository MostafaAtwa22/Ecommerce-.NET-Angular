import {
  Component,
  ElementRef,
  inject,
  OnInit,
  ViewChild,
  AfterViewInit,
  OnDestroy,
  ChangeDetectorRef
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ChatBoxComponent } from '../chat-box.component/chat-box.component';
import { ChatService } from '../chat.service';
import { PickerComponent } from '@ctrl/ngx-emoji-mart';
import { CommonModule } from '@angular/common';
import { getDefaultAvatarByGender } from '../../shared/utils/avatar-utils';
import { Subject, takeUntil, debounceTime } from 'rxjs';
import { effect } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-chat-window',
  standalone: true,
  imports: [FormsModule, ChatBoxComponent, PickerComponent, CommonModule],
  templateUrl: './chat-window.component.html',
  styleUrl: './chat-window.component.scss',
})
export class ChatWindowComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('chatContainer') chatContainer!: ElementRef;
  @ViewChild(ChatBoxComponent) chatBoxComponent!: ChatBoxComponent;

  isLoading: boolean = true;
  message: string = '';
  editingMessageId: number | null = null;
  showEmojiPicker = false;

  _chatService = inject(ChatService);
  chatMessages$ = toObservable(this._chatService.chatMessages);
  currentOpenChat$ = toObservable(this._chatService.currentOpenChat);

  private cdr = inject(ChangeDetectorRef);
  private destroy$ = new Subject<void>();
  private autoScrollEnabled = true;

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
  // Lifecycle Hooks
  // ============================================
  ngOnInit() {
    this.loadChat();
    this.setupMessageSubscription();
  }

  ngAfterViewInit() {
    // Scroll to bottom when chat opens
    setTimeout(() => {
      this.scrollToBottom('auto');
    }, 500);
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ============================================
  // Chat Loading
  // ============================================
  loadChat() {
    this.isLoading = true;

    // Simulate loading - replace with actual chat load
    setTimeout(() => {
      this.isLoading = false;
      this.cdr.detectChanges();

      // Scroll to bottom after loading
      setTimeout(() => {
        this.scrollToBottom('auto');
      }, 100);
    }, 800);
  }

  // ============================================
  // Message Subscription - Auto Scroll on New Messages
  // ============================================
  private setupMessageSubscription() {
    // Listen for new messages
    this.chatMessages$
      .pipe(
        takeUntil(this.destroy$),
        debounceTime(50) // Prevent multiple scrolls
      )
      .subscribe(() => {
        if (this.autoScrollEnabled) {
          this.scrollToBottom('smooth');
        }
      });

    // Listen for current chat changes
    this.currentOpenChat$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.autoScrollEnabled = true;
        this.cancelEdit();

        // Scroll to bottom when switching chats
        setTimeout(() => {
          this.scrollToBottom('auto');
        }, 300);
      });
  }

  // ============================================
  // Scroll Management - ALWAYS START FROM BOTTOM
  // ============================================
  scrollToBottom(behavior: ScrollBehavior = 'smooth') {
    if (!this.chatBoxComponent) return;

    // Use ChatBoxComponent's scrollToBottom method
    this.chatBoxComponent.scrollToBottom(behavior);

    // Also scroll the container if needed
    setTimeout(() => {
      if (this.chatContainer?.nativeElement) {
        const container = this.chatContainer.nativeElement;
        const chatBox = container.querySelector('app-chat-box');

        if (chatBox) {
          const scrollElement = chatBox.querySelector('.chat-box');
          if (scrollElement) {
            scrollElement.scrollTo({
              top: scrollElement.scrollHeight,
              behavior: behavior
            });
          }
        }
      }
    }, 50);
  }

  // ============================================
  // Message Actions
  // ============================================
  sendMessage() {
    const text = this.message.trim();
    if (!text) return;

    if (this.editingMessageId) {
      this._chatService.editMessage(this.editingMessageId, text);
      this.editingMessageId = null;
    } else {
      this._chatService.sendMessage(text);
    }

    // Clear input and reset
    this.message = '';
    this.showEmojiPicker = false;
    this.autoScrollEnabled = true;

    // Force scroll to bottom
    setTimeout(() => {
      this.scrollToBottom('smooth');
    }, 50);

    // Reset textarea height
    this.resetTextareaHeight();
  }

  onEditMessage(message: any) {
    this.editingMessageId = message.id;
    this.message = message.content;
    this.autoScrollEnabled = false;

    // Focus textarea
    setTimeout(() => {
      const textarea = document.querySelector('.chat-textarea') as HTMLTextAreaElement;
      if (textarea) {
        textarea.focus();
        this.autoResize({ target: textarea } as any);
      }
    }, 50);
  }

  cancelEdit() {
    this.editingMessageId = null;
    this.message = '';
    this.autoScrollEnabled = true;
    this.resetTextareaHeight();
  }

  onDeleteMessage(id: number) {
    // Implement delete logic
  }

  // ============================================
  // Emoji Picker
  // ============================================
  toggleEmojiPicker() {
    this.showEmojiPicker = !this.showEmojiPicker;
  }

  addEmoji(event: any) {
    this.message += event.emoji.native;
    this.showEmojiPicker = false;

    // Focus textarea after adding emoji
    setTimeout(() => {
      const textarea = document.querySelector('.chat-textarea') as HTMLTextAreaElement;
      if (textarea) {
        textarea.focus();
        this.autoResize({ target: textarea } as any);
      }
    }, 50);
  }

  // ============================================
  // Input Handling
  // ============================================
  onKeyDown(event: KeyboardEvent) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    } else if (event.key === 'Escape') {
      if (this.editingMessageId) {
        this.cancelEdit();
      }
      if (this.showEmojiPicker) {
        this.showEmojiPicker = false;
      }
    }
  }

  autoResize(event: Event) {
    const textarea = event.target as HTMLTextAreaElement;
    textarea.style.height = 'auto';
    textarea.style.height = `${textarea.scrollHeight}px`;
  }

  private resetTextareaHeight() {
    setTimeout(() => {
      const textarea = document.querySelector('.chat-textarea') as HTMLTextAreaElement;
      if (textarea) {
        textarea.style.height = 'auto';
      }
    }, 10);
  }

  // ============================================
  // Typing Indicator
  // ============================================
  onTyping() {
    this._chatService.notifyTyping();
  }

  // ============================================
  // Sidebar Toggles
  // ============================================
  toggleLeftSidebar() {
    const sidebar = document.querySelector('.chat-left-sidebar');
    const overlay = document.querySelector('.modal-backdrop');

    if (sidebar) {
      sidebar.classList.toggle('active');
    }
    if (overlay) {
      overlay.classList.toggle('show');
    }
  }

  toggleRightSidebar() {
    const sidebar = document.querySelector('.chat-right-sidebar');
    const overlay = document.querySelector('.modal-backdrop');

    if (sidebar) {
      sidebar.classList.toggle('active');
    }
    if (overlay) {
      overlay.classList.toggle('show');
    }
  }

  // ============================================
  // Outside Click Handler for Emoji Picker
  // ============================================
  onOutsideClick() {
    this.showEmojiPicker = false;
  }
}
