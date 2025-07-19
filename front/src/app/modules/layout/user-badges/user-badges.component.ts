import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MaterialModule } from 'app/infrastructure/material/material.module';
import { MinifiedStandardUserDTO, NewBadgeDTO } from 'app/models/models';
import { UserService } from 'app/services/user.service';

@Component({
  selector: 'app-user-badges',
  standalone: true,
  imports: [
    MaterialModule,
    CommonModule,
    FormsModule
  ],
  templateUrl: './user-badges.component.html',
  styleUrl: './user-badges.component.css'
})
export class UserBadgesComponent implements OnInit {

  users: MinifiedStandardUserDTO[] = [];
  filteredUsers: MinifiedStandardUserDTO[] = [];
  searchTerm: string = '';

  displayedColumns: string[] = ['username', 'badge', 'actions'];
  badges: string[] = ['NoBadge', 'VerifiedPublisher', 'SponsoredOSS'];

  constructor(private userService: UserService, private snackBar: MatSnackBar) {}

  ngOnInit(): void {
    this.populateTable();
  }

  populateTable(): void {
    this.userService.getAllStandardUsers().subscribe({
      next: (res: MinifiedStandardUserDTO[]) => {
        this.users = res;
        this.filteredUsers = [...this.users];
      },
      error: (err: any) => {
        console.log(err);
      }
    })
  }

  changeBadge(userId: string, newBadge: string): void {
    const newBadgeDTO: NewBadgeDTO = {
      badge: newBadge
    };

    this.userService.changeBadge(userId, newBadgeDTO).subscribe({
      next: () => {
        this.snackBar.open('Successfully changed badge.', 'Close', { duration: 3000 });
        this.populateTable();
      },
      error: (err: any) => {
        console.log(err);
      }
    })
  }

  filterTable(): void {
    this.filteredUsers = this.users.filter((user: MinifiedStandardUserDTO) => 
      user.username.toLowerCase().includes(this.searchTerm.toLowerCase()) || 
      user.badge.toLowerCase().includes(this.searchTerm.toLowerCase())
    )
  }

}
