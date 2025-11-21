/// <reference types="@angular/localize" />

import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

// Suppress Chrome extension runtime.lastError warnings
if (typeof window !== 'undefined') {
  const originalError = console.error;
  console.error = (...args: any[]) => {
    // Filter out runtime.lastError messages from Chrome extensions
    const errorMessage = args.join(' ');
    if (errorMessage.includes('runtime.lastError') ||
        errorMessage.includes('Could not establish connection') ||
        errorMessage.includes('Receiving end does not exist')) {
      return; // Suppress these errors
    }
    originalError.apply(console, args);
  };

  // Also handle unhandled promise rejections that might contain runtime errors
  window.addEventListener('unhandledrejection', (event) => {
    const reason = event.reason?.message || String(event.reason || '');
    if (reason.includes('runtime.lastError') ||
        reason.includes('Could not establish connection') ||
        reason.includes('Receiving end does not exist')) {
      event.preventDefault(); // Suppress these errors
    }
  });
}

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
