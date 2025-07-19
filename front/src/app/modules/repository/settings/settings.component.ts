import { Component, inject, Input } from '@angular/core';
import { MaterialModule } from 'app/infrastructure/material/material.module';
import { DockerRepositoryDTO } from 'app/models/models';
import { ChangeVisibilityPopupComponent } from "../change-visibility-popup/change-visibility-popup.component";
import { MatDialog } from '@angular/material/dialog';
import { DeleteRepositoryPopupComponent } from '../delete-repository-popup/delete-repository-popup.component';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [MaterialModule],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.css'
})
export class SettingsComponent {


  @Input() repository: DockerRepositoryDTO = {
    id: "0",
    images: [],
    lastPushed: '',
    createdAt: '',
    name: '',
    owner: '',
    description: '',
    isPublic: true,
    starCount: 0,
    badge: ''
  }
  readonly dialog = inject(MatDialog);



  onVisibilityChange(): void {
    console.log(this.repository)
    const dialogRef = this.dialog.open(ChangeVisibilityPopupComponent, {
      data: {repository: this.repository},
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log('The dialog was closed');
      if (result !== undefined) {
        console.log("AAAAA")
        console.log(result)
      }
    })  
  }

  onDelete(): void {
    console.log("ssss")
    const dialogRef = this.dialog.open(DeleteRepositoryPopupComponent, {
      data: {repository: this.repository},
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log('The dialog was closed');
      if (result !== undefined) {
        console.log("AAAAA")
        console.log(result)
      }
    })
  }


}
