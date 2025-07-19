import { LiveAnnouncer } from '@angular/cdk/a11y';
import { AfterViewInit, Component, inject, Signal, signal, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort, Sort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { Router } from '@angular/router';
import { MaterialModule } from 'app/infrastructure/material/material.module';
import { DockerRepositoryDTO } from 'app/models/models';
import { RepositoryService } from '../services/repository.service';
import { AuthService } from 'app/services/auth.service';
import { OrganizationService } from 'app/services/organization.service';

@Component({
  selector: 'app-all-repositories',
  standalone: true,
  imports: [ MaterialModule, FormsModule ],
  templateUrl: './all-repositories.component.html',
  styleUrl: './all-repositories.component.css'
})
export class AllRepositoriesComponent  implements AfterViewInit{
  namespaces: string[] = []
  categories: string[] = ["c1", "c2", "c3"]
  searchQuery: Signal<string> = signal("");
  displayedColumns: string[] = ["name", "lastPushed", "contains", "visibility"]
  repositories: DockerRepositoryDTO[] = []
  
      
  repositorySource = new MatTableDataSource(this.repositories)

  router = inject(Router)
  private _liveAnnouncer = inject(LiveAnnouncer);

  @ViewChild(MatSort)
  sort: MatSort = new MatSort;

  @ViewChild(MatPaginator)
  paginator!: MatPaginator;

  constructor(private readonly repositoryService: RepositoryService,
              private readonly authService: AuthService,
              private readonly organizationService: OrganizationService
            ) 
  {
    this.fillNamespaces()
    const userId: string = this.authService.userData.value?.userId || ""
    this.repositoryService.GetUsersRepositories(userId).subscribe({
      next: (response: DockerRepositoryDTO[]) => {
        console.log(response)
        this.repositories = response
        this.repositorySource = new MatTableDataSource(this.repositories)

      },
      error: (error) => {
        console.error('Error creating repository:', error);
      }
    }); 

  }

  fillNamespaces() {
    const userEmail: string = this.authService.userData.value?.userEmail || ""
    this.namespaces.push(userEmail)
    this.organizationService.getOrganizations(userEmail).subscribe({
      next: (data) => {
        console.log(data)
        data.forEach(organization => {
          this.namespaces.push(organization.name)          
        });
      },
      error: (err) => {
        console.error('Error fetching organizations:', err);
      }
    });

  }

  ngAfterViewInit() {
    this.repositorySource.sort = this.sort;
    this.repositorySource.paginator = this.paginator
  }

  announceSortChange(sortState: Sort) {
    // This example uses English messages. If your application supports
    // multiple language, you would internationalize these strings.
    // Furthermore, you can customize the message to add additional
    // details about the values being sorted.
    if (sortState.direction) {
      this._liveAnnouncer.announce(`Sorted ${sortState.direction}ending`);
    } else {
      this._liveAnnouncer.announce('Sorting cleared');
    }
  }

  onCreate(): void {
    this.router.navigate(["/create-repo"])
    
  }

  openRepository(repository: DockerRepositoryDTO): void {
    console.log(repository)
    this.router.navigate(["/single-repo", repository.id])
  }
}
