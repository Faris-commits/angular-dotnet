import { Component, OnInit } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-photo-approval-stats',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './photo-approval-stats.component.html',
  styleUrl: './photo-approval-stats.component.css'
})
export class PhotoApprovalStatsComponent implements OnInit {
  photoApprovalStats: any[] = [];
  usersWithoutMainPhoto: string[] = [];
  loadingStats = true;
  loadingUsers = true;

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.getPhotoApprovalStats();
    this.getUsersWithoutMainPhoto();
  }

  getPhotoApprovalStats(): void {
    this.adminService.getPhotoApprovalStats().subscribe({
      next: (stats) => {
        this.photoApprovalStats = stats;
        this.loadingStats = false;
      },
      error: (err) => {
        console.error('Error fetching photo approval stats:', err);
        this.loadingStats = false;
      }
    });
  }

  getUsersWithoutMainPhoto(): void {
    this.adminService.getUsersWithoutMainPhoto().subscribe({
      next: (users) => {
        this.usersWithoutMainPhoto = users;
        this.loadingUsers = false;
      },
      error: (err) => {
        console.error('Error fetching users without main photo:', err);
        this.loadingUsers = false;
      }
    });
  }
}
