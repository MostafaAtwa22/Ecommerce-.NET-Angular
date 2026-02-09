import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatbotService } from '../../services/chatbot-service';
import { ChatMessage } from '../../modules/chatMessage';

@Component({
  selector: 'app-chatbot-widget',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chatbot-widget.html',
  styleUrls: ['./chatbot-widget.scss'],
})
export class ChatbotWidgetComponent {
  isOpen = signal(false);
  isLoading = signal(false);
  input = signal('');
  messages = signal<ChatMessage[]>([]);

  readonly quickQuestions: string[] = [
    'How do I register and create an account?',
    'How do I log in or use Google sign-in?',
    'How do I add products to my basket and checkout?',
    'How does the wishlist work?',
    'How can I track my orders?',
  ];

  constructor(private chatbotService: ChatbotService) {}

  toggleOpen(): void {
    this.isOpen.update((v) => !v);
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

