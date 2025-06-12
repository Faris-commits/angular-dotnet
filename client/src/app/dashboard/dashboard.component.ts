import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PhotoFeedService } from '../_services/PhotoFeedService';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent {
  constructor(public photoFeed: PhotoFeedService) {}
}