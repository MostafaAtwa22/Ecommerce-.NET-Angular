import { Component, HostBinding } from '@angular/core';
import { ChatSidebarComponent } from '../../chat/chat-sidebar.component/chat-sidebar.component';
import { ChatRightSidebarComponent } from '../../chat/chat-right-sidebar.component/chat-right-sidebar.component';
import { ChatWindowComponent } from '../../chat/chat-window.component/chat-window.component';
import { ChatService } from '../../chat/chat.service';

@Component({
  selector: 'app-dashboard-chat',
  imports: [ChatSidebarComponent, ChatRightSidebarComponent, ChatWindowComponent],
  templateUrl: './dashboard-chat.component.html',
  styleUrl: './dashboard-chat.component.scss',
})
export class DashboardChatComponent {
  @HostBinding('class') hostClass = 'dashboard-chat-host';

  constructor(public _chatService: ChatService) { }

  toggleLeftSidebar() {
    this._chatService.toggleLeftSidebar();
  }

  toggleRightSidebar() {
    this._chatService.toggleRightSidebar();
  }

  closeSidebars() {
    this._chatService.closeSidebars();
  }
}
