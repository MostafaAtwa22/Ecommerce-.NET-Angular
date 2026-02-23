import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Environment } from '../../environment';
import { ChatbotResponse } from '../modules/chatMessage';

@Injectable({
  providedIn: 'root',
})
export class ChatbotService {
  private readonly baseUrl = `${Environment.baseUrl}/api/chatbot`;

  constructor(private http: HttpClient) {}

  ask(message: string): Observable<ChatbotResponse> {
    const body = JSON.stringify(message);
    return this.http.post<ChatbotResponse>(`${this.baseUrl}/ask`, body, {
      headers: { 'Content-Type': 'application/json' },
      withCredentials: false,
    });
  }
}

