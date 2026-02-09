export type ChatMessageFrom = 'user' | 'bot';

export interface ChatMessage {
  from: ChatMessageFrom;
  text: string;
  at: Date;
}

export interface ChatbotResponse {
  response: string;
  timestamp: string;
}
