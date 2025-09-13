import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PhotoFeedService } from '../_services/PhotoFeedService';
import { Observable } from 'rxjs';
import { Photo } from '../_models/photo';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardComponent implements OnInit {
  photos$!: Observable<Photo[]>;

  constructor(private photoFeedService: PhotoFeedService) {}

  ngOnInit(): void {
    this.photos$ = this.photoFeedService.getApprovedPhotos();
  }
}
