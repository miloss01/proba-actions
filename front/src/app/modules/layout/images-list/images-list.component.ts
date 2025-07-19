import { CommonModule } from '@angular/common';
import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MaterialModule } from 'app/infrastructure/material/material.module';
import { DockerImageDTO } from 'app/models/models';
import { DockerImageService } from 'app/services/docker-image.service';

@Component({
  selector: 'app-images-list',
  standalone: true,
  imports: [
    FormsModule,
    CommonModule,
    MaterialModule
  ],
  templateUrl: './images-list.component.html',
  styleUrl: './images-list.component.css'
})
export class ImagesListComponent implements OnChanges {

  @Input() images: DockerImageDTO[] = [];

  @Input() forDeleting: boolean = false;

  filteredImages = [...this.images];
  selectedImage: DockerImageDTO | null= null;
  sortOption = 'newest';
  filterTag = '';
  page = 0;
  pageSize = 5;
  totalNumberOfPages = 0;

  constructor(private readonly dockerImageService: DockerImageService){}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['images']) {
      this.images = changes['images'].currentValue;
      this.totalNumberOfPages = Math.ceil(this.images.length / this.pageSize);
      this.page = this.images.length == 0 ? 0 : 1;
      this.applyFilters();
    }
  }

  applyFilters(): void {
    let filtered = this.images.filter(image => 
      image.tags.join('').toLowerCase().includes(this.filterTag.toLowerCase())
    );
    
    if (this.sortOption == 'newest')
      filtered.sort((a, b) => {
        const dateA = new Date(a.lastPush).getTime();
        const dateB = new Date(b.lastPush).getTime();
        return dateB - dateA;
      });
    else if (this.sortOption == 'oldest')
      filtered.sort((a, b) => {
        const dateA = new Date(a.lastPush).getTime();
        const dateB = new Date(b.lastPush).getTime();
        return dateA - dateB;
      });
    else if (this.sortOption == 'az')
      filtered.sort((a, b) => a.tags.join('').localeCompare(b.tags.join('')));
    else if (this.sortOption == 'za')
      filtered.sort((a, b) => b.tags.join('').localeCompare(a.tags.join('')));
    
    if (filtered.length == 0) {
      this.page = 0;
      this.totalNumberOfPages = 0;
    } else {
      this.totalNumberOfPages = Math.ceil(filtered.length / this.pageSize);
    }
    
    this.filteredImages = filtered.slice((this.page - 1) * this.pageSize, this.page * this.pageSize);
  }

  onPageChange(change: number): void {
    this.page += change;

    if (this.page > this.totalNumberOfPages)
      this.page = 1;
    if (this.page < 1)
      this.page = this.totalNumberOfPages;

    this.applyFilters();
  }

  onPageSizeChange(): void {
    this.page = 1;
    this.applyFilters();
  }

  onFilter(): void {
    this.page = 1;
    this.applyFilters();
  }

  copyToClipboard(text: string): void {
    navigator.clipboard.writeText(text);
  }

  selectImage(image: any) {
    this.selectedImage = image;
  }

  DeleteTag(): void {
    if (this.selectedImage) {
      console.log('Selected Image:', this.selectedImage);
      this.dockerImageService.deleteDockerImage(this.selectedImage.imageId).subscribe({
          next: (response: void) => {
            console.log('Deleted:', response);
            this.filteredImages = this.filteredImages.filter(
              image => image.imageId !== this.selectedImage?.imageId
            );
        
            // Optionally clear the selection
            this.selectedImage = null;
          },
          error: (error) => {
            console.error('Error creating repository:', error);
          }
        });     
      }
  }
}
