import { Component, Input, SimpleChanges } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MaterialModule } from 'app/infrastructure/material/material.module';
import { DockerRepositoryDTO } from 'app/models/models';
import { RepositoryService } from '../services/repository.service';

@Component({
  selector: 'app-general-overview',
  standalone: true,
  imports: [ MaterialModule, ReactiveFormsModule ],
  templateUrl: './general-overview.component.html',
  styleUrl: './general-overview.component.css'
})
export class GeneralOverviewComponent {
  @Input() repository: DockerRepositoryDTO = {
    images: [],
    lastPushed: '',
    name: '',
    owner: '',
    description: '',
    isPublic: true,
    createdAt: '',
    id: "0",
    starCount: 0,
    badge: ''
  }
  desctiptionEdditing: boolean = false
  descriptionFormControl = new FormControl('');


  constructor(private readonly repositoryService: RepositoryService){}


  onDescriptionEddit(): void {
    console.log('edit');
    this.descriptionFormControl.setValue(this.repository.description);
    this.desctiptionEdditing = true
  }

  onDescriptionSave(): void {
    this.desctiptionEdditing = false
    this.repository.description = this.descriptionFormControl.value || ""
    this.repositoryService.UpdateRepositoryDescription({
      repositoryId: this.repository.id,
      newDescription: this.repository.description
    }).subscribe({
      next: (response: DockerRepositoryDTO) => {
        console.log('Description changed:', response);
      },
      error: (error) => {
        console.error('Error creating repository:', error);
      }
    }); 
  }

  onDescriptionCancel(): void {
    this.desctiptionEdditing = false
  }
}
