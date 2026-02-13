import {
  Component,
  ElementRef,
  inject,
  OnInit,
  ViewChild,
  AfterViewInit,
  OnDestroy,
  ChangeDetectorRef,
  HostListener
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
  isUploading = false;
  selectedFile: File | null = null;
  messageSearchTerm: string = '';
  showMessageSearch = false;

  // Recording states
  isRecording = false;
  recordingDuration = 0;
  private mediaRecorder: MediaRecorder | null = null;
  private audioChunks: Blob[] = [];
  private recordingInterval: any;

  _chatService = inject(ChatService);
  chatMessages$ = toObservable(this._chatService.chatMessages);
  currentOpenChat$ = toObservable(this._chatService.currentOpenChat);

  private cdr = inject(ChangeDetectorRef);
  private destroy$ = new Subject<void>();
  private autoScrollEnabled = true;

  setDefaultAvatar(event: Event, gender?: any) {
    const img = event.target as HTMLImageElement;
    img.src = getDefaultAvatarByGender(gender);
  }

  getAvatar(profilePicture?: string | null, gender?: any): string {
    return profilePicture || getDefaultAvatarByGender(gender);
  }

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
        this.showMessageSearch = false;
        this.messageSearchTerm = '';
        this.onMessageSearch();

        // Scroll to bottom when switching chats
        setTimeout(() => {
          this.scrollToBottom('auto');
        }, 300);
      });
  }


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

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (!file) return;

    this.isUploading = true;
    this._chatService.uploadFile(file).subscribe({
      next: (res) => {
        this.isUploading = false;
        this._chatService.sendMessage('', res);
        this.scrollToBottom('smooth');
      },
      error: (err) => {
        this.isUploading = false;
        console.error('Upload failed', err);
      }
    });

    event.target.value = '';
  }

  // Voice Recording Methods
  async startRecording() {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
      this.mediaRecorder = new MediaRecorder(stream);
      this.audioChunks = [];

      this.mediaRecorder.ondataavailable = (event) => {
        if (event.data.size > 0) {
          this.audioChunks.push(event.data);
        }
      };

      this.mediaRecorder.onstop = async () => {
        const audioBlob = new Blob(this.audioChunks, { type: 'audio/webm' });
        const fileName = `voice_message_${Date.now()}.webm`;
        const file = new File([audioBlob], fileName, { type: 'audio/webm' });

        this.isUploading = true;
        this._chatService.uploadFile(file).subscribe({
          next: (res) => {
            this.isUploading = false;
            this._chatService.sendMessage('', res);
            this.scrollToBottom('smooth');
          },
          error: (err) => {
            this.isUploading = false;
            console.error('Upload failed', err);
          }
        });

        // Stop all tracks to release the microphone
        stream.getTracks().forEach(track => track.stop());
      };

      this.mediaRecorder.start();
      this.isRecording = true;
      this.recordingDuration = 0;
      this.recordingInterval = setInterval(() => {
        this.recordingDuration++;
      }, 1000);

    } catch (err) {
      console.error('Could not start recording', err);
      alert('Could not access microphone. Please ensure you have given permission.');
    }
  }

  stopRecording() {
    if (this.mediaRecorder && this.isRecording) {
      this.mediaRecorder.stop();
      this.isRecording = false;
      clearInterval(this.recordingInterval);
    }
  }

  cancelRecording() {
    if (this.mediaRecorder && this.isRecording) {
      this.mediaRecorder.onstop = null; // Prevent sending
      this.mediaRecorder.stop();
      this.isRecording = false;
      clearInterval(this.recordingInterval);
      this.mediaRecorder.stream.getTracks().forEach(track => track.stop());
    }
  }

  formatDuration(seconds: number): string {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  }

  triggerFileInput() {
    const fileInput = document.getElementById('chat-file-input') as HTMLInputElement;
    if (fileInput) {
      fileInput.click();
    }
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


  onTyping() {
    this._chatService.notifyTyping();
  }


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

  toggleMessageSearch() {
    this.showMessageSearch = !this.showMessageSearch;
    if (!this.showMessageSearch) {
      this.messageSearchTerm = '';
      this.onMessageSearch();
    }
  }

  onMessageSearch() {
    if (this.chatBoxComponent) {
      this.chatBoxComponent.searchTerm.set(this.messageSearchTerm);
    }
  }

  @HostListener('document:click')
  onOutsideClick() {
    this.showEmojiPicker = false;
  }
}
