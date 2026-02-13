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
  signal,
  computed,
  Input
} from '@angular/core';
import { ChatService } from '../chat.service';
import { AccountService } from '../../account/account-service';
import { DatePipe } from '@angular/common';
import { Environment } from '../../environment';
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
  baseUrl = Environment.baseUrl;

  searchTerm = signal(''); // Signal or input
  displayedMessages = computed(() => {
    const term = this.searchTerm().toLowerCase();
    const messages = this._chatService.chatMessages();
    if (!term) return messages;
    return messages.filter(m => m.content?.toLowerCase().includes(term));
  });

  isUserScrolling = false;
  private scrollThreshold = 50;
  private observer: MutationObserver | null = null;

  // Custom Audio Player State
  private currentAudio: HTMLAudioElement | null = null;
  playingAudioId = signal<number | null>(null);
  audioProgress = signal<{ [key: number]: number }>({});
  audioDuration = signal<{ [key: number]: number }>({});

  constructor() { }

  ngAfterViewInit(): void {
    this.setupScrollObserver();
    this.scrollToBottom();
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
  }

  setDefaultAvatar(event: Event, gender?: any) {
    const img = event.target as HTMLImageElement;
    img.src = getDefaultAvatarByGender(gender);
  }

  getAvatar(profilePicture?: string | null, gender?: any): string {
    return profilePicture || getDefaultAvatarByGender(gender);
  }

  isImage(type?: string): boolean {
    return !!type?.startsWith('image/');
  }

  isAudio(type?: string, url?: string): boolean {
    return !!type?.startsWith('audio/') || !!url?.endsWith('.webm') || !!url?.endsWith('.wav') || !!url?.endsWith('.mp3');
  }

  onEditMessage(message: any) {
    this.editMessage.emit(message);
  }

  onDeleteMessage(id: number) {
    this._chatService.deleteMessage(id);
  }

  // Voice Message Playback logic
  togglePlay(message: any) {
    const audioUrl = this.baseUrl + message.attachmentUrl;
    const msgId = message.id;

    if (this.playingAudioId() === msgId) {
      this.pauseAudio();
    } else {
      this.playAudio(msgId, audioUrl);
    }
  }

  private playAudio(msgId: number, url: string) {
    if (this.currentAudio) {
      this.currentAudio.pause();
    }

    this.currentAudio = new Audio(url);
    this.playingAudioId.set(msgId);

    this.currentAudio.addEventListener('timeupdate', () => {
      if (this.currentAudio) {
        const progress = (this.currentAudio.currentTime / this.currentAudio.duration) * 100;
        this.audioProgress.update(prev => ({ ...prev, [msgId]: progress || 0 }));
      }
    });

    this.currentAudio.addEventListener('loadedmetadata', () => {
      if (this.currentAudio) {
        this.audioDuration.update(prev => ({ ...prev, [msgId]: this.currentAudio!.duration }));
      }
    });

    this.currentAudio.addEventListener('ended', () => {
      this.playingAudioId.set(null);
      this.audioProgress.update(prev => ({ ...prev, [msgId]: 0 }));
    });

    this.currentAudio.play();
  }

  private pauseAudio() {
    if (this.currentAudio) {
      this.currentAudio.pause();
      this.playingAudioId.set(null);
    }
  }

  seekAudio(messageId: number, event: any) {
    const value = event.target.value;
    if (this.currentAudio && this.playingAudioId() === messageId) {
      this.currentAudio.currentTime = (value / 100) * this.currentAudio.duration;
    }
  }

  formatAudioTime(seconds: number): string {
    if (!seconds || isNaN(seconds)) return '0:00';
    const mins = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  }

  getAudioCurrentTime(msgId: number): string {
    if (!this.currentAudio || this.playingAudioId() !== msgId) return '0:00';
    return this.formatAudioTime(this.currentAudio.currentTime);
  }

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
