# Icon Style Guide

## General Guidelines

- Use Material Icons throughout the application for consistency
- Maintain a consistent size for icons in similar contexts
- Use appropriate colors that match the application's theme
- Ensure proper spacing around icons

## Icon Sizes

- **Navigation icons**: 24px (standard Material size)
- **Button icons**: 20px
- **Form field icons**: 20px
- **Table action icons**: 20px
- **Dialog header icons**: 24px
- **Informational icons**: 20px

## Icon Colors

- **Primary actions**: primary-600 (brand color) 
- **Secondary actions**: slate-500 (neutral color)
- **Destructive actions**: rose-600 (error color)
- **Success indicators**: emerald-600 (success color)
- **Warning indicators**: amber-600 (warning color)
- **Informational indicators**: blue-600 (info color)

## Hover States

- All interactive icons should have a hover state
- Use bg-slate-100 dark:bg-slate-700 for hover backgrounds
- Use color transitions for a smooth experience

## Icon Buttons

### Standard Icon Button
```html
<button 
  mat-icon-button 
  class="text-slate-500 hover:bg-slate-100 dark:hover:bg-slate-700 rounded-full transition-colors">
  <mat-icon>icon_name</mat-icon>
</button>
```

### Primary Icon Button
```html
<button 
  mat-icon-button 
  class="text-primary-600 hover:bg-primary-50 dark:hover:bg-primary-900/20 rounded-full transition-colors">
  <mat-icon>icon_name</mat-icon>
</button>
```

### Destructive Icon Button
```html
<button 
  mat-icon-button 
  class="text-rose-600 hover:bg-rose-50 dark:hover:bg-rose-900/20 rounded-full transition-colors">
  <mat-icon>icon_name</mat-icon>
</button>
```

## Common Icons by Context

### Navigation
- Dashboard: `dashboard`
- Products: `inventory_2`
- Categories: `category`
- Orders: `shopping_bag`
- Settings: `settings`
- Reports: `bar_chart`
- Statistics: `insights`
- Users: `people`

### Actions
- Add: `add`
- Edit: `edit`
- Delete: `delete`
- Save: `save`
- Cancel: `close`
- Search: `search`
- Filter: `filter_list`
- Sort: `sort`
- View: `visibility`
- More options: `more_vert`
- Export: `file_download`
- Import: `file_upload`

### Media/Files
- Image: `image`
- Upload: `cloud_upload`
- Download: `cloud_download`
- Attachment: `attach_file`
- Gallery: `collections`

### Status Indicators
- Success: `check_circle`
- Error: `error`
- Warning: `warning`
- Information: `info`
- Processing: `autorenew` (with spin animation)
- Locked: `lock`
- Unlocked: `lock_open`

## Implementation Examples

### Table Action Row
```html
<div class="flex space-x-1">
  <button 
    mat-icon-button
    matTooltip="View Details"
    class="text-primary-600 hover:bg-primary-50 dark:hover:bg-slate-700 rounded-full p-1 transition-colors">
    <mat-icon class="text-sm h-5 w-5">visibility</mat-icon>
  </button>
  
  <button 
    mat-icon-button
    matTooltip="Edit"
    class="text-slate-500 hover:bg-slate-100 dark:hover:bg-slate-700 rounded-full p-1 transition-colors">
    <mat-icon class="text-sm h-5 w-5">edit</mat-icon>
  </button>
  
  <button 
    mat-icon-button
    matTooltip="Delete"
    class="text-rose-600 hover:bg-rose-50 dark:hover:bg-slate-700 rounded-full p-1 transition-colors">
    <mat-icon class="text-sm h-5 w-5">delete</mat-icon>
  </button>
</div>
```

### Form Field Icons
```html
<mat-form-field >
  <mat-label>Search</mat-label>
  <input matInput placeholder="Search...">
  <mat-icon matPrefix class="text-slate-400 mr-2">search</mat-icon>
</mat-form-field>
```

### Status Indicator
```html
<div class="flex items-center">
  <mat-icon 
    class="text-emerald-500 dark:text-emerald-400 mr-1">
    check_circle
  </mat-icon>
  <span>Order Completed</span>
</div>
```

### Animated Processing Icon
```html
<mat-icon class="text-primary-500 animate-spin">autorenew</mat-icon>
```