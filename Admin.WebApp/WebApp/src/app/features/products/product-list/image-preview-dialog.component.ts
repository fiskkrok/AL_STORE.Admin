import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'app-image-preview-dialog',
  templateUrl: './image-preview-dialog.component.html',
  styleUrls: ['./image-preview-dialog.component.scss']
})
export class ImagePreviewDialogComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public data: { url: string }) {}
}
