import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { MaterialModule } from 'app/infrastructure/material/material.module';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { PaginatorComponent } from '../paginator/paginator.component';
import { OrganizationService } from 'app/services/organization.service';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';
import { MatDialog } from '@angular/material/dialog';

@Component({
  selector: 'app-details',
  standalone: true,
  imports: [CommonModule, FormsModule, MaterialModule, PaginatorComponent],
  templateUrl: './details.component.html',
  styleUrl: './details.component.css'
})
export class DetailsComponent implements OnInit {
  id: string | null = null;
  name: string | null = null;
  isOwner: boolean | null = null;
  organization: any | null = null;

  users: any[] = [];
  filteredUsers: any[] = [];
  displayedAllUsers: any[] = [];
  members: any[] = [];
  displayedMembers: any[] = [];

  searchQuery = ''
  pageSize: number = 2;
  currentPage: number = 1;

  pageSizeMembers: number = 2;
  currentPageMembers: number = 1;

  constructor(private dialog: MatDialog, private route: ActivatedRoute, private location: Location, private orgService: OrganizationService) {
  }

  ngOnInit() {
    this.id = this.route.snapshot.paramMap.get('id');

    this.route.queryParams.subscribe(params => {
      this.name = params['name'];
      this.isOwner = params['isOwner'] === 'true';
    });

    if(this.id != null)
      this.fetchMembers(this.id)
  }

  fetchMembers(id: string): void {
    this.orgService.getMembersByOrganizationId(id).subscribe({
      next: (data) => {
        console.log(data)
        this.members = data.members;
        this.displayedMembers = [...this.members];
        this.updateDisplayedMembers();

        this.users = data.otherUsers;
        this.filteredUsers = [...this.users];
        this.updateDisplayedUsers();

        console.log(data)
        console.log("ok")
      },
      error: (err) => {
        this.members = [];
        console.log(err)
      }
    });
  }

  updateSearch() {
    this.filteredUsers = this.users.filter((user) => {
      const fullName = `${user.firstName} ${user.lastName}`.toLowerCase();
      const email = user.email.toLowerCase();
      const query = this.searchQuery.toLowerCase();

      return fullName.includes(query) || email.includes(query);
    });

    this.currentPage = 1;
    this.updateDisplayedUsers();
  }

  onPageChange(newPage: number) {
    this.currentPage = newPage;
    this.updateDisplayedUsers();
  }

  onPageSizeChange(newSize: number) {
    this.pageSize = newSize;
    this.currentPage = 1; 
    this.updateDisplayedUsers();
  }

  updateDisplayedUsers() {
    const startIndex = (this.currentPage - 1) * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.displayedAllUsers = this.filteredUsers.slice(startIndex, endIndex);
  }

  onPageChangeMember(newPage: number) {
    this.currentPageMembers = newPage;
    this.updateDisplayedMembers();
  }

  onPageSizeChangeMember(newSize: number) {
    this.pageSizeMembers = newSize;
    this.currentPageMembers = 1; 
    this.updateDisplayedMembers();
  }

  updateDisplayedMembers() {
    const startIndex = (this.currentPageMembers - 1) * this.pageSizeMembers;
    const endIndex = startIndex + this.pageSizeMembers;
    this.displayedMembers = this.members.slice(startIndex, endIndex);
  }

  addUser(user: any) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '300px',
      data: { action: 'add new member', userEmail: user.email }
    });
  
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.addMember(user.id);
      } else {
        console.log('User addition was canceled');
      }
    });
  }

  addMember(userId: string) {
    if (this.id) {
      this.orgService.addMemberToOrganization(this.id, userId)
        .subscribe(
          (response) => {
            console.log(response)
            console.log('User added to organization successfully.')
            if (this.id)
              this.fetchMembers(this.id);
          },
          (error) => {
            console.log('Error: ' + error.error)
            console.log(error)
          }
        );
    } else {
      console.log('Please provide valid organization and user IDs.')
    }
  }

  goBack(): void {
    this.location.back(); 
  }
}
