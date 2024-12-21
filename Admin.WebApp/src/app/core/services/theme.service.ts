import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { OverlayContainer } from '@angular/cdk/overlay';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private darkTheme = new BehaviorSubject<boolean>(this.getStoredTheme());
  theme$ = this.darkTheme.asObservable();

  constructor(private overlayContainer: OverlayContainer) {
    this.initializeTheme();
  }

  private getStoredTheme(): boolean {
    const savedTheme = localStorage.getItem('theme');
    return savedTheme ? savedTheme === 'dark' : this.prefersDarkMode();
  }

  private prefersDarkMode(): boolean {
    return window.matchMedia('(prefers-color-scheme: dark)').matches;
  }

  isDarkTheme(): boolean {
    return this.darkTheme.value;
  }

  setTheme(isDark: boolean) {
    this.darkTheme.next(isDark);
    localStorage.setItem('theme', isDark ? 'dark' : 'light');
    document.documentElement.setAttribute('data-theme', isDark ? 'dark' : 'light');
    this.applyThemeToOverlay(isDark);
  }

  toggleTheme() {
    this.setTheme(!this.isDarkTheme());
  }

  initializeTheme() {
    this.setTheme(this.getStoredTheme());

    // Listen for system theme changes
    window.matchMedia('(prefers-color-scheme: dark)')
      .addEventListener('change', e => {
        if (!localStorage.getItem('theme')) {
          this.setTheme(e.matches);
        }
      });
  }

  private applyThemeToOverlay(isDark: boolean) {
    const overlayContainerClasses = this.overlayContainer.getContainerElement().classList;
    if (isDark) {
      overlayContainerClasses.add('dark-theme');
      overlayContainerClasses.remove('light-theme');
    } else {
      overlayContainerClasses.add('light-theme');
      overlayContainerClasses.remove('dark-theme');
    }
  }
}
