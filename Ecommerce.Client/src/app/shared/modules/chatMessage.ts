export type ChatMessageFrom = 'user' | 'bot';

export interface ChatMessage {
  from: ChatMessageFrom;
  text: string;
  at: Date;
  isHtml?: boolean;
  actionRoute?: string;
  actionText?: string;
}

export interface ChatbotResponse {
  response: string;
  timestamp: string;
}
