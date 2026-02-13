import { Component, signal, computed, inject, ViewChild, ElementRef, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatbotService } from '../../services/chatbot-service';
import { AccountService } from '../../../account/account-service';
import { ChatMessage } from '../../modules/chatMessage';

@Component({
  selector: 'app-chatbot-widget',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chatbot-widget.html',
  styleUrls: ['./chatbot-widget.scss'],
})
export class ChatbotWidgetComponent {
  private accountService = inject(AccountService);
  private chatbotService = inject(ChatbotService);

  @ViewChild('messagesContainer') private messagesContainer!: ElementRef;

  isOpen = signal(false);
  isExpanded = signal(false);
  isLoading = signal(false);
  input = signal('');
  messages = signal<ChatMessage[]>([]);

  private readonly adminQuestions: string[] = [
    'How do I add a new product to the store?',
    'How do I manage user roles and permissions?',
    'How do I view sales reports and analytics?',
    'How do I process and manage customer orders?',
    'How do I update product stock and inventory?',
  ];

  private readonly userQuestions: string[] = [
    'How do I register and create an account?',
    'How do I log in or use Google sign-in?',
    'How do I add products to my basket and checkout?',
    'How does the wishlist work?',
    'How can I track my orders?',
  ];

  readonly quickQuestions = computed(() => {
    const isAdmin = this.accountService.hasRole('Admin') || this.accountService.hasRole('SuperAdmin');
    return isAdmin ? this.adminQuestions : this.userQuestions;
  });

  constructor() {
    // Scroll to bottom when messages change
    effect(() => {
      this.messages(); // track the signal
      this.scrollToBottom();
    });

    // Scroll to bottom when opening
    effect(() => {
      if (this.isOpen()) {
        this.scrollToBottom();
      }
    });
  }

  private scrollToBottom(): void {
    setTimeout(() => {
      if (this.messagesContainer) {
        const element = this.messagesContainer.nativeElement;
        element.scrollTop = element.scrollHeight;
      }
    }, 100);
  }

  toggleOpen(): void {
    this.isOpen.update((v) => !v);
    if (!this.isOpen()) {
      this.isExpanded.set(false);
    }
  }

  toggleExpand(event: Event): void {
    event.stopPropagation();
    this.isExpanded.update((v) => !v);
    this.scrollToBottom();
  }

  sendFromQuick(question: string): void {
    this.ensureOpen();
    this.sendMessageInternal(question);
  }

  send(): void {
    const message = this.input().trim();
    if (!message || this.isLoading()) {
      return;
    }
    this.sendMessageInternal(message);
    this.input.set('');
  }

  handleKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.send();
    }
  }

  private ensureOpen(): void {
    if (!this.isOpen()) {
      this.isOpen.set(true);
    }
  }

  private sendMessageInternal(message: string): void {
    const now = new Date();
    this.messages.update((msgs) => [
      ...msgs,
      { from: 'user', text: message, at: now },
    ]);

    this.isLoading.set(true);

    this.chatbotService.ask(message).subscribe({
      next: (res) => {
        const botText = res?.response?.trim()
          ? res.response
          : "I'm sorry, I couldn't generate a response.";
        this.messages.update((msgs) => [
          ...msgs,
          { from: 'bot', text: botText, at: new Date(res.timestamp) },
        ]);
        this.isLoading.set(false);
      },
      error: () => {
        this.messages.update((msgs) => [
          ...msgs,
          {
            from: 'bot',
            text:
              'The assistant is temporarily unavailable. Please try again in a moment.',
            at: new Date(),
          },
        ]);
        this.isLoading.set(false);
      },
    });
  }
}
