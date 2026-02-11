import { Component, inject } from '@angular/core';
import { ChatService } from '../chat.service';
import { TitleCasePipe } from '@angular/common';

@Component({
  selector: 'app-chat-right-sidebar',
  imports: [TitleCasePipe],
  templateUrl: './chat-right-sidebar.component.html',
  styleUrl: './chat-right-sidebar.component.scss',
})
export class ChatRightSidebarComponent {
  _chatService = inject(ChatService);
}
