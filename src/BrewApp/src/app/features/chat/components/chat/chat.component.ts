import { Component, computed, effect, ElementRef, inject, OnInit, signal, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BreakpointObserver } from '@angular/cdk/layout';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatListModule } from '@angular/material/list';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ChatService } from '../../services/chat.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { BeerCatalogItem, ChatMessage, ChatRequest } from '../../models/chat.model';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatSidenavModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatListModule,
    MatChipsModule,
    MatIconModule,
    MatDividerModule,
    MatTooltipModule
  ],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.scss'
})
export class ChatComponent implements OnInit {
  private readonly chatService = inject(ChatService);
  private readonly notifications = inject(NotificationService);
  private readonly breakpointObserver = inject(BreakpointObserver);

  @ViewChild('messageList') private messageList!: ElementRef<HTMLElement>;

  readonly beerCatalog = signal<BeerCatalogItem[]>([]);
  readonly chatHistory = signal<ChatMessage[]>([]);
  readonly userInput = signal('');
  readonly conversationId = signal<string | undefined>(undefined);
  readonly isLoadingCatalog = signal(true);
  readonly isProcessing = signal(false);
  readonly sidenavOpen = signal(true);
  readonly isMobile = signal(false);

  readonly sidenavMode = computed(() => this.isMobile() ? 'over' as const : 'side' as const);

  readonly canSend = computed(
    () => this.userInput().trim().length > 0 && !this.isProcessing()
  );

  constructor() {
    effect(() => {
      // Auto-scroll to bottom when chat history changes
      if (this.chatHistory().length > 0 || this.isProcessing()) {
        setTimeout(() => {
          const el = this.messageList?.nativeElement;
          if (el) el.scrollTop = el.scrollHeight;
        }, 50);
      }
    });
  }

  ngOnInit(): void {
    this.breakpointObserver.observe('(max-width: 768px)')
      .subscribe(result => {
        this.isMobile.set(result.matches);
        this.sidenavOpen.set(!result.matches);
      });

    this.chatService.getBeerCatalog().subscribe({
      next: (catalog) => {
        this.beerCatalog.set(catalog);
        this.isLoadingCatalog.set(false);
      },
      error: () => {
        this.isLoadingCatalog.set(false);
        // 5xx errors already handled by api-error.interceptor
      }
    });
  }

  sendMessage(): void {
    if (!this.canSend()) {
      return;
    }

    const text = this.userInput().trim();

    this.chatHistory.update(history => [
      ...history,
      { role: 'user', content: text }
    ]);

    this.userInput.set('');
    this.isProcessing.set(true);

    const request: ChatRequest = {
      message: text,
      conversationId: this.conversationId()
    };

    this.chatService.askChat(request).subscribe({
      next: (response) => {
        this.chatHistory.update(history => [
          ...history,
          { role: 'assistant', content: response.answer ?? '(no response)' }
        ]);
        if (response.conversationId) {
          this.conversationId.set(response.conversationId);
        }
        this.isProcessing.set(false);
      },
      error: () => {
        this.chatHistory.update(history => [
          ...history,
          { role: 'assistant', content: 'Sorry, I could not process your request.' }
        ]);
        this.isProcessing.set(false);
      }
    });
  }

  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }
}
