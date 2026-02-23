import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-typing-indicator',
  templateUrl: './typing-indicator.component.html',
  styleUrl: './typing-indicator.component.scss',
})
export class TypingIndicatorComponent {
  @Input() showText: boolean = false;
  @Input() darkBg: boolean = false;
}