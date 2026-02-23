import { Component, HostBinding } from '@angular/core';
import { ChatSidebarComponent } from '../../chat/chat-sidebar.component/chat-sidebar.component';
import { ChatRightSidebarComponent } from '../../chat/chat-right-sidebar.component/chat-right-sidebar.component';
import { ChatWindowComponent } from '../../chat/chat-window.component/chat-window.component';

@Component({
  selector: 'app-dashboard-chat',
  imports: [ChatSidebarComponent, ChatRightSidebarComponent, ChatWindowComponent],
  templateUrl: './dashboard-chat.component.html',
  styleUrl: './dashboard-chat.component.scss',
})
export class DashboardChatComponent {
  @HostBinding('class') hostClass = 'dashboard-chat-host';

  // Sidebar states for mobile
  isLeftSidebarActive = false;
  isRightSidebarActive = false;

  toggleLeftSidebar() {
    this.isLeftSidebarActive = !this.isLeftSidebarActive;
    // Auto-close right sidebar when opening left on mobile
    if (this.isLeftSidebarActive) {
      this.isRightSidebarActive = false;
    }
  }

  toggleRightSidebar() {
    this.isRightSidebarActive = !this.isRightSidebarActive;
    // Auto-close left sidebar when opening right on mobile
    if (this.isRightSidebarActive) {
      this.isLeftSidebarActive = false;
    }
  }

  closeSidebars() {
    this.isLeftSidebarActive = false;
    this.isRightSidebarActive = false;
  }
}
