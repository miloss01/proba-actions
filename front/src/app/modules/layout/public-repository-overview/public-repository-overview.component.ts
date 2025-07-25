import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MaterialModule } from 'app/infrastructure/material/material.module';
import { DockerRepositoryDTO } from 'app/models/models';
import { RepositoryService } from 'app/services/repository.service';
import { ImagesListComponent } from "../images-list/images-list.component";

@Component({
  selector: 'app-public-repository-overview',
  standalone: true,
  imports: [
    MaterialModule,
    ImagesListComponent
],
  templateUrl: './public-repository-overview.component.html',
  styleUrl: './public-repository-overview.component.css'
})
export class PublicRepositoryOverviewComponent implements OnInit {

  repositoryId: string | null = "";
  repository: DockerRepositoryDTO | undefined;

  constructor(private dockerRepositoryService: RepositoryService, 
              private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      this.repositoryId = params.get('id');
      this.getRepository();
    });
  }

  getRepository(): void {
    this.dockerRepositoryService.getDockerRepositoryById(this.repositoryId!).subscribe({
      next: (res: DockerRepositoryDTO) => {
        this.repository = res;
      },
      error: (err: any) => {
        console.log(err);
      }
    })
  }

  copyToClipboard(text: string): void {
    navigator.clipboard.writeText(text);
  }

}
