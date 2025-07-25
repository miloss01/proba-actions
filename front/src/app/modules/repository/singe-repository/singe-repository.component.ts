import { Component, inject } from '@angular/core';
import { MaterialModule } from 'app/infrastructure/material/material.module';
import { GeneralOverviewComponent } from '../general-overview/general-overview.component';
import { SettingsComponent } from '../settings/settings.component';
import { DockerRepositoryDTO } from 'app/models/models';
import { ActivatedRoute } from '@angular/router';
import { ImagesListComponent } from 'app/modules/layout/images-list/images-list.component';
import { RepositoryService } from 'app/services/repository.service';

@Component({
  selector: 'app-singe-repository',
  standalone: true,
  imports: [ MaterialModule, GeneralOverviewComponent, ImagesListComponent, SettingsComponent ],
  templateUrl: './singe-repository.component.html',
  styleUrl: './singe-repository.component.css'
})
export class SingeRepositoryComponent {
  repository: DockerRepositoryDTO = {
    images: [],
    lastPushed: '12,23,32',
    name: 'lalala',
    owner: 'selena',
    description: 'fdvdvd',
    isPublic: true,
    createdAt: '1.1.235',
    id: "0",
    starCount: 0,
    badge: ''
  }
  route = inject(ActivatedRoute);

  constructor(private readonly repositoryService: RepositoryService){}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const id = params['id'];
      this.repositoryService.getDockerRepositoryById(id).subscribe(repo => {
        this.repository = repo; // Assign fetched repository details
      })
      console.log(id)
    })
  }
}
