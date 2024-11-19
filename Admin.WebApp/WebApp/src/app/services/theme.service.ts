import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private darkTheme = new BehaviorSubject<boolean>(true);
  
  isDarkTheme() {
    return this.darkTheme.value;
  }
  
  setTheme(theme: 'dark' | 'light') {
    this.darkTheme.next(theme === 'dark');
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('theme', theme);
  }
  
  initializeTheme() {
    const savedTheme = localStorage.getItem('theme') || 'dark';
    this.setTheme(savedTheme as 'dark' | 'light');
  }
}