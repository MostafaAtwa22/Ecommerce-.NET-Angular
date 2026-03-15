import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly THEME_KEY = 'preferred-theme';
  isDark = signal(false);

  constructor() {
    const savedTheme = localStorage.getItem(this.THEME_KEY);
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    
    // Use saved preference if available, otherwise use system preference
    const isDarkMode = savedTheme ? savedTheme === 'dark' : prefersDark;
    this.setTheme(isDarkMode);
  }

  toggleTheme() {
    this.setTheme(!this.isDark());
  }

  private setTheme(isDark: boolean) {
    this.isDark.set(isDark);
    document.documentElement.setAttribute('data-theme', isDark ? 'dark' : 'light');
    localStorage.setItem(this.THEME_KEY, isDark ? 'dark' : 'light');
  }
}
