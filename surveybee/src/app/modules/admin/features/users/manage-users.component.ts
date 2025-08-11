import { Component, OnInit } from '@angular/core';
import { UserService, User } from '../../../../common/services/user.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-manage-users',
  standalone: false,
  templateUrl: './manage-users.component.html',
  styleUrls: ['./manage-users.component.css']
})
export class ManageUsersComponent implements OnInit {
  users: User[] = [];
  isLoading = false;
  errorMessage = '';
  displayedColumns = ['email', 'role', 'actions'];

  constructor(private userService: UserService, private snackBar: MatSnackBar) {}

  ngOnInit() {
    this.fetchUsers();
  }

  fetchUsers() {
    this.isLoading = true;
    this.errorMessage = '';
    this.userService.getAllUsers().subscribe({
      next: (response: any) => {
        console.log('Fetched users:', response);
        this.users = response.result || [];
        this.isLoading = false;
      },
      error: (err: any) => {
        this.errorMessage = 'Failed to load users: ' + (err.error?.message || err.message);
        this.isLoading = false;
        this.snackBar.open(this.errorMessage, 'Close', { duration: 5000 });
      }
    });
  }

  makeAdmin(user: User) {
    if (user.role === 'Admin') return;
    if (!confirm(`Promote ${user.email} to admin?`)) return;
    this.userService.promoteToAdmin(user.id).subscribe({
      next: () => {
        this.snackBar.open('User promoted to admin!', 'Close', { duration: 3000 });
        this.fetchUsers();
      },
      error: (err: any) => {
        this.snackBar.open('Failed to promote user: ' + (err.error?.message || err.message), 'Close', { duration: 5000 });
      }
    });
  }

  deleteUser(user: User) {
    if (user.role === 'Admin') return;
    if (!confirm(`Delete user ${user.email}? This cannot be undone.`)) return;
    this.userService.deleteUser(user.id).subscribe({
      next: () => {
        this.snackBar.open('User deleted!', 'Close', { duration: 3000 });
        this.fetchUsers();
      },
      error: (err: any) => {
        this.snackBar.open('Failed to delete user: ' + (err.error?.message || err.message), 'Close', { duration: 5000 });
      }
    });
  }
}
