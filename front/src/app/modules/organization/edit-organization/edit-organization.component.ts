import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterialModule } from 'app/infrastructure/material/material.module';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ImageService } from 'app/services/image.service';
import { OrganizationService } from 'app/services/organization.service';
import { AuthService } from 'app/services/auth.service';

@Component({
  selector: 'app-edit-organization',
  standalone: true,
  imports: [ CommonModule, MaterialModule, FormsModule ],
  templateUrl: './edit-organization.component.html',
  styleUrl: './edit-organization.component.css'
})
export class EditOrganizationComponent {
  id: string;
  name: string;
  oldDescription: string;
  organizationDescription: string;
  imagePreview: string | null = null;
  isUploading = false;
  fileName: string;
  oldFileName: string;
  imageFile: File | null = null;

  constructor(
    private organizationService: OrganizationService,
    private imageService: ImageService,
    private authService: AuthService,
    public dialogRef: MatDialogRef<EditOrganizationComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { orgId: string, name:string, desc: string; imageName: string, imageUrl: string }
  ) {
    this.id = data.orgId;
    this.name = data.name;
    this.organizationDescription = data.desc;
    this.oldDescription = data.desc;
    this.fileName = data.imageName
    this.oldFileName = data.imageName;
    this.imagePreview = data.imageUrl || null;
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input?.files && input.files[0]) {
      const file = input.files[0];
      this.imageFile = file;
      this.fileName = file.name; 
      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreview = reader.result as string;
      };
      reader.readAsDataURL(file);
    }
  }  

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    this.isUploading = true;

    const updateDto = {
      id: this.id,
      description: this.organizationDescription,
      imageLocation: this.fileName,
    };

    if(this.oldDescription != this.organizationDescription || this.oldFileName != this.fileName) {
      this.organizationService.updateOrganization(updateDto).subscribe({
        next: (response) => {
          console.log('Organization updated successfully:', response);

          if (this.oldFileName != this.fileName && this.imageFile != null) {
            const updateImageDto = {
              oldFileName: this.authService.userData?.value?.userEmail+"/"+this.id+"/"+this.oldFileName,  
              newFileName: this.authService.userData?.value?.userEmail+"/"+this.id+"/"+this.fileName  
            };

            this.imageService.updateImage(updateImageDto, this.imageFile).subscribe({
              next: (imageResponse) => {
                console.log('Image updated successfully:', imageResponse);
              },
              error: (imageError) => {
                console.error('Error updating image:', imageError);
              }
            });
          }
        },
        error: (err) => {
          console.error('Error updating organization:', err);
        }
      });
    }

    setTimeout(() => {
      this.dialogRef.close({
        description: this.organizationDescription,
        imageUrl: this.imagePreview,
        fileName: this.fileName
      });
    }, 2000)

    // this.dialogRef.close({
    //   description: this.organizationDescription,
    //   imageUrl: this.imagePreview,
    //   fileName: this.fileName
    // });
  }
}
