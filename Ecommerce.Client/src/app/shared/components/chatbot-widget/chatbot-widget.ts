import { Component, signal, computed, inject, ViewChild, ElementRef, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatbotService } from '../../services/chatbot-service';
import { AccountService } from '../../../account/account-service';
import { Router } from '@angular/router';
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
  private router = inject(Router);

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
    'How can I apply a coupon'
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

  private readonly staticResponses: Record<string, { html: string, route?: string }> = {
    // Admin Questions
    'How do I add a new product to the store?': {
      html: 'To add a new product, navigate to the <strong>Dashboard</strong> > <strong>Products</strong> page. Click the <em>Create New Product</em> button to fill out the product details including name, price, images, and brand.<br><br>Would you like to go to the Dashboard Products page now?',
      route: '/dashboard/products'
    },
    'How do I manage user roles and permissions?': {
      html: 'User roles are managed in the <strong>Admin Dashboard</strong> under the <em>Users & Roles</em> section. You can assign, create, or revoke permissions from there.<br><br>Would you like to go to the Users Management page now?',
      route: '/dashboard/users'
    },
    'How do I view sales reports and analytics?': {
      html: 'Analytics are available on your main <strong>Dashboard</strong> page, where you can view total sales, recent orders, and revenue breakdowns.<br><br>Would you like to go to your Dashboard now?',
      route: '/dashboard'
    },
    'How do I process and manage customer orders?': {
      html: 'Go to the <strong>Dashboard</strong> > <strong>Orders</strong> page to view, update statuses, or refund customer orders.<br><br>Would you like to go to the Dashboard Orders page now?',
      route: '/dashboard/orders'
    },
    'How do I update product stock and inventory?': {
      html: 'Navigate to <strong>Dashboard</strong> > <strong>Products</strong>, edit the specific product, and update the <em>Quantity in Stock</em> field.<br><br>Would you like to go to the Dashboard Products page now?',
      route: '/dashboard/products'
    },
    
    // User Questions
    'How do I register and create an account?': {
      html: 'You can create a new account by clicking the <strong>Register</strong> button in the top navigation menu, or by filling out the form on our Registration page.<br><br>Would you like to go to the Register page now?',
      route: '/register'
    },
    'How do I log in or use Google sign-in?': {
      html: 'Click the <strong>Login</strong> button at the top header. You can log in using your email and password, or click the <em>Sign in with Google</em> button.<br><br>Would you like to go to the Login page now?',
      route: '/login'
    },
    'How do I add products to my basket and checkout?': {
      html: 'Browse the shop, click <strong>Add to Cart</strong> on your desired products, then click the shopping bag icon in the header to proceed to Checkout.<br><br>Would you like to view your basket now?',
      route: '/basket'
    },
    'How does the wishlist work?': {
      html: 'The wishlist lets you save items for later! Just click the ❤️ heart icon on any product. Access your saved items from the Wishlist menu.'
    },
    'How can I track my orders?': {
      html: 'You can track your orders by visiting your Account Menu > <strong>Orders</strong>. You\'ll see a list of your order history and their current processing status.<br><br>Would you like to view your orders now?',
      route: '/orders'
    },
    'How can I apply a coupon': {
      html: 'During checkout, enter your coupon code in the <em>Apply Coupon</em> box on the right side of the screen. Your discount will be calculated automatically!'
    }
  };

  private ensureOpen(): void {
    if (!this.isOpen()) {
      this.isOpen.set(true);
    }
  }

  // --- HTML Action Handling ---
  navigateAction(route: string): void {
    this.router.navigateByUrl(route);
    this.isOpen.set(false); // Optionally close chatbot when navigating
  }

  dismissAction(msg: ChatMessage): void {
    // Hide the action by removing the route prop from this specific message
    msg.actionRoute = undefined;
  }

  private sendMessageInternal(message: string): void {
    const rawInput = message.trim();
    const now = new Date();
    this.messages.update((msgs) => [
      ...msgs,
      { from: 'user', text: rawInput, at: now },
    ]);

    // Check static dictionary first
    const exactMatch = this.staticResponses[rawInput];
    if (exactMatch) {
      this.isLoading.set(true);
      setTimeout(() => {
        this.messages.update((msgs) => [
          ...msgs,
          { 
            from: 'bot', 
            text: exactMatch.html, 
            at: new Date(), 
            isHtml: true, 
            actionRoute: exactMatch.route 
          },
        ]);
        this.isLoading.set(false);
        this.scrollToBottom();
      }, 1500); // Slight delay for natural chat feel
      return;
    }

    // Otherwise, hit backend LLM
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
