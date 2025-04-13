# Angular Architecture Standards

This document outlines the architecture standards for our Angular application codebase. Following these standards ensures consistency, maintainability, and scalability.

## 1. File Organization and Naming

### Directory Structure

```
src/
├── app/
│   ├── core/                   # Singleton services, guards, interceptors
│   │   ├── guards/             # Route guards 
│   │   ├── interceptors/       # HTTP interceptors
│   │   ├── services/           # Core services (auth, error, etc.)
│   ├── features/               # Feature modules
│   │   ├── feature-name/       # Each feature in its own directory
│   │   │   ├── components/     # Feature-specific components
│   │   │   ├── pages/          # Page-level components
│   │   │   ├── services/       # Feature-specific services
│   │   │   ├── feature.routes.ts # Feature routing
│   ├── layout/                 # Layout components (sidebar, header)
│   ├── shared/                 # Shared components, directives, pipes
│   │   ├── components/         # Reusable components
│   │   ├── directives/         # Custom directives
│   │   ├── pipes/              # Custom pipes
│   │   ├── models/             # Shared data models
│   ├── store/                  # NgRx store (organized by feature)
├── assets/                     # Static assets
├── environments/               # Environment configurations
```

### File Naming Conventions

- Use **kebab-case** for all filenames (e.g., `product-list.component.ts`)
- Use proper suffixes:
  - Components: `.component.ts`, `.component.html`, `.component.scss`
  - Services: `.service.ts`
  - Models: `.model.ts`
  - Interfaces: `.interface.ts`
  - Enums: `.enum.ts`
  - Guards: `.guard.ts`
  - Pipes: `.pipe.ts`
  - Directives: `.directive.ts`
- Keep related files together (component, template, styles)
- For store files, use feature name + type: `product.actions.ts`, `product.reducer.ts`

## 2. Template Usage Guidelines

### External Templates (Default Approach)

- All components should use external templates unless they meet the inline exception criteria
- Place template files in the same directory as the component class
- Name template files with the `.component.html` suffix
- Use proper indentation and formatting in HTML

### Inline Template Exceptions

Use inline templates only when:
- The component has less than 15 lines of HTML
- The component is a simple wrapper or utility
- The component's template is unlikely to change
- The component's template is tightly coupled to the component logic

## 3. Dependency Injection Standard

Use the `inject()` function for dependency injection:

```typescript
import { Component, inject } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';

@Component({...})
export class MyComponent {
  private readonly authService = inject(AuthService);
  
  // Component logic...
}
```

### Guidelines

- Mark injected services as `private readonly` when appropriate
- Place all injections at the top of the class, before any other properties
- Use descriptive names that represent the service's purpose
- Avoid unused service injections

## 4. Error Handling Pattern

### Centralized Error Management

- Use the `ErrorService` as the central hub for all error management
- Categorize errors by type (network, auth, validation, server)
- Use the HTTP interceptor to consistently capture and handle HTTP errors
- Display errors through the `ErrorToastComponent`

### Service-Level Error Handling

- All services should extend `BaseCrudService` when possible
- Use the standard error handling methods provided by `BaseCrudService`
- Provide clear, user-friendly error messages
- Use consistent error object structure throughout the application

### Implementation

```typescript
// Error handling in services
protected handleError(error: HttpErrorResponse, friendlyMessage: string): Observable<never> {
  this.errorService.addError({
    code: error.error?.code || `HTTP_${error.status}`,
    message: error.error?.message || friendlyMessage,
    severity: error.status >= 500 ? 'error' : 'warning'
  });
  
  return throwError(() => new Error(friendlyMessage));
}
```

## 5. API Layer Standards

### Base CRUD Service

- Extend `BaseCrudService` for all API services when possible
- Override methods where specific mapping is needed
- Use consistent parameter and return types

### API Service Responsibilities

- Data transformation/mapping between API and client models
- Error handling (delegated to `ErrorService`)
- Caching where appropriate
- Implement domain-specific API methods

### Pagination and Filtering

- Use standard pagination interfaces (`PagedResponse<T>`)
- Use consistent query parameter structure
- Handle filters in a standard way

## 6. SignalR Integration

- Extend `BaseSignalRService` for all SignalR hubs
- Handle connection state consistently
- Dispatch actions to NgRx store from SignalR event handlers
- Provide connection status observables

## 7. Styling Architecture

### Approach Priority

1. **Tailwind Utility Classes** - Use directly in templates for most styling needs
2. **Angular Material Components** - For complex UI elements, themed with Tailwind variables
3. **Custom SCSS** - Only for cases where Tailwind utilities are insufficient

### Guidelines

- Use Tailwind utility classes for layout, spacing, colors, and typography
- Group Tailwind classes logically (layout → spacing → appearance)
- Create reusable Tailwind combinations using Tailwind's `@apply` directive for common patterns
- Ensure consistent dark mode support using Tailwind's dark mode utilities
- Component-specific styles should use namespaced classes

## 8. NgRx Store Guidelines

### Structure and Organization

- Organize store by feature
- Use the standard NgRx pattern: actions, reducers, effects, selectors
- File naming: `feature.actions.ts`, `feature.reducer.ts`, etc.

### Best Practices

- Define actions using `createActionGroup`
- Use descriptive action names
- Handle loading states in the store
- Use selectors for all data access from components
- Keep effects pure and focused
- Dispatch actions from components/services, not directly from effects

## Implementation Examples

See the following files for implementation examples:

- `BaseCrudService` - Base service for CRUD operations
- `ErrorService` - Centralized error handling
- `BaseSignalRService` - Base service for SignalR connections
- `ProductService` - Example service implementation
- `StockSignalRService` - Example SignalR service implementation