import { Component, EventEmitter, Inject, inject, Input, Output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MaterialModule } from 'app/infrastructure/material/material.module';
import { DockerRepositoryDTO } from 'app/models/models';
import { RepositoryService } from '../services/repository.service';

@Component({
  selector: 'app-change-visibility-popup',
  standalone: true,
  imports: [ MaterialModule, FormsModule ],
  templateUrl: './change-visibility-popup.component.html',
  styleUrl: './change-visibility-popup.component.css'
})
export class ChangeVisibilityPopupComponent {
  repository: DockerRepositoryDTO;

  // Inject MAT_DIALOG_DATA and MatDialogRef
  constructor(@Inject(MAT_DIALOG_DATA) private data: { repository: DockerRepositoryDTO }, 
  private dialogRef: MatDialogRef<ChangeVisibilityPopupComponent>,
  private readonly repositoryService: RepositoryService) {
    // Extract the repository from the data
    this.repository = data.repository;

  }

  userInput = signal("")
  onChange() {
    if (this.userInput() === this.repository.name){
      console.log("super")
      this.repositoryService.UpdateRepositoryVisibility({
        repositoryId: this.repository.id,
        isPublic: !this.repository.isPublic
      }).subscribe({
        next: (response: DockerRepositoryDTO) => {
          this.repository.isPublic = response.isPublic
          console.log('Visibility changed:', response);
          this.dialogRef.close();

        },
        error: (error) => {
          console.error('Error creating repository:', error);
        }
      }); 
    }
    else {
      console.log("nije super")
    }
  }
  
  onCancel() {
    console.log(this.repository)
    this.dialogRef.close();
  }

}
