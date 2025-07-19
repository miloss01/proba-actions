import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MaterialModule } from 'app/infrastructure/material/material.module';

@Component({
  selector: 'app-edit-team-dialog',
  standalone: true,
  imports: [MaterialModule, FormsModule, CommonModule, ReactiveFormsModule],
  templateUrl: './edit-team-dialog.component.html',
  styleUrl: './edit-team-dialog.component.css'
})
export class EditTeamDialogComponent {
  editTeamForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<EditTeamDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { name: string; description: string }
  ) {
    this.editTeamForm = this.fb.group({
      name: [data.name, [Validators.required, Validators.maxLength(20)]],
      description: [data.description, Validators.maxLength(200)],
    });
  }

  onSubmit(): void {
    if (this.editTeamForm.valid) {
      this.dialogRef.close(this.editTeamForm.value);
    }
  }
}
