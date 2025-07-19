import { Component, OnInit } from '@angular/core';
import { DockerImageDTO, DockerRepositoryDTO, PageDTO } from 'app/models/models';
import { MaterialModule } from 'app/infrastructure/material/material.module';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { DockerImageService } from 'app/services/docker-image.service';
import { RouterModule } from '@angular/router';
import { RepositoryService } from 'app/services/repository.service';
import { AuthService } from 'app/services/auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';

interface BadgeHelper {
  name: string;
  selected: boolean;
}

@Component({
  selector: 'app-explore-page',
  standalone: true,
  imports: [
    MaterialModule, 
    FormsModule, 
    CommonModule, 
    RouterModule
  ],
  templateUrl: './explore-page.component.html',
  styleUrl: './explore-page.component.css'
})
export class ExplorePageComponent implements OnInit {
  dockerRepositories: DockerRepositoryDTO[] = [];
  badges: BadgeHelper[] = [
    { name: 'NoBadge', selected: false },
    { name: 'DockerOfficialImage', selected: false },
    { name: 'VerifiedPublisher', selected: false },
    { name: 'SponsoredOSS', selected: false },
  ];
  totalNumberOfElements: number = 0;
  page: number = 0;
  pageSize: number = 2;
  totalNumberOfPages: number = 0;
  searchTerm: string = '';

  oldSearchTerm: string = this.searchTerm;
  oldBadges: BadgeHelper[] = [];

  notAllowedToStarRepositories: string[] = [];
  starredRepositoriesIds: string[] = [];
  userId: string = "";
  userRole: string = "";

  constructor(private dockerRepositoryService: RepositoryService,
              private authService: AuthService,
              private snackBar: MatSnackBar) {}

  ngOnInit(): void {
    if (this.authService.userData.value) {
      this.userId = this.authService.userData.value.userId;
      this.userRole = this.authService.userData.value.userRole;
    }
    
    if (this.showStarButton()) {
      this.getNotAllowedRepositoriesToStar();
      this.getStarredRepositories();
    }

    this.applyFilters();
  }

  showStarButton(): boolean {
    return this.userId != "" && this.userRole == "StandardUser";
  }

  getNotAllowedRepositoriesToStar(): void {
    this.dockerRepositoryService.getNotAllowedToStarRepositoriesForUser(this.userId).subscribe({
      next: (res: string[]) => {
        this.notAllowedToStarRepositories = res;
        console.log(this.notAllowedToStarRepositories);
      },
      error: (err: any) => {
        console.log(err);
      }
    })
  }

  getStarredRepositories(): void {
    this.dockerRepositoryService.getStarDockerRepositoriesForUser(this.userId).subscribe({
      next: (res: DockerRepositoryDTO[]) => {
        this.starredRepositoriesIds = res.map((repository: DockerRepositoryDTO) => repository.id);
        console.log(this.starredRepositoriesIds);
      },
      error: (err: any) => {
        console.log(err);
      }
    })
  }

  starRepository(repositoryId: string): void {
    this.dockerRepositoryService.starRepository(this.userId, repositoryId).subscribe({
      next: () => {
        this.snackBar.open('Successfully starred repository.', 'Close', { duration: 3000 });
        this.getNotAllowedRepositoriesToStar();
        this.getStarredRepositories();
        this.applyFilters();
      },
      error: (err: any) => {
        console.log(err);
      }
    })
  }

  removeStarRepository(repositoryId: string): void {
    this.dockerRepositoryService.removeStarRepository(this.userId, repositoryId).subscribe({
      next: () => {
        this.snackBar.open('Successfully removed starred repository.', 'Close', { duration: 3000 });
        this.getNotAllowedRepositoriesToStar();
        this.getStarredRepositories();
        this.applyFilters();
      },
      error: (err: any) => {
        console.log(err);
      }
    })
  }

  applyFilters(): void {
    this.page = 1;
    this.oldSearchTerm = this.searchTerm;
    this.oldBadges = this.badges.map(badge => ({ ...badge }));
    this.getDockerRepositories();
  }

  onPageChange(change: number): void {
    let newPage: number = this.page + change;

    if (newPage > this.totalNumberOfPages)
      newPage = 1;
    if (newPage < 1)
      newPage = this.totalNumberOfPages;

    this.page = newPage;

    this.getDockerRepositories();
  }

  onPageSizeChange(event: Event): void {
    const newPageSize = (event.target as HTMLSelectElement).value;
    this.pageSize = Number(newPageSize);
    this.page = 1;
    this.getDockerRepositories();
  }

  getDockerRepositories(): void {
    this.dockerRepositoryService.getDockerRepositories(
      this.page, 
      this.pageSize, 
      this.oldSearchTerm, 
      this.getSelectedBadges(this.oldBadges)
    ).subscribe({
      next: (res: PageDTO<DockerRepositoryDTO>) => {
        this.dockerRepositories = res.data;
        this.totalNumberOfElements = res.totalNumberOfElements;
        this.totalNumberOfPages = Math.ceil(this.totalNumberOfElements / this.pageSize);
        this.page = this.totalNumberOfElements != 0 ? this.page : 0
      },
      error: (err: any) => {
        console.log(err);
      }
    });
  }

  getSelectedBadges(badges: BadgeHelper[]): string {
    return badges
      .filter(badge => badge.selected)
      .map(badge => badge.name)
      .join(',')
  }
}
