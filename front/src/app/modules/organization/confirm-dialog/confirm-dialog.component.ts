import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MaterialModule } from 'app/infrastructure/material/material.module';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [MaterialModule],
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.css'
})
export class ConfirmDialogComponent {
  constructor(public dialogRef: MatDialogRef<ConfirmDialogComponent>, @Inject(MAT_DIALOG_DATA) public data: { action: string; itemName?: string | null; userEmail?: string }) {}

  onCancel(): void {
    this.dialogRef.close(false);  
  }

  onConfirm(): void {
    this.dialogRef.close(true); 
  }
}
