import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { PhotoTagSelectorComponent } from "../../photo-tags/photo-tag-selector/photo-tag-selector.component";

@Component({
  selector: 'app-photo-list',
  standalone: true,
  imports: [PhotoTagSelectorComponent],
  templateUrl: './photo-list.component.html',
  styleUrl: './photo-list.component.css'
})
export class PhotoListComponent implements OnInit {
  photos: any[] = [];
  filterTagIds: number[] = [];

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadPhotos();
  }

  loadPhotos() {
    this.http.get<any[]>('https://localhost:5001/api/users/photos').subscribe(p => this.photos = p);
  }

  getTagNames(photo: any): string {
  return photo.tags && photo.tags.length
    ? photo.tags.map((t: any) => t.name).join(', ')
    : 'None';
}

  filterPhotos() {
    if (this.filterTagIds.length === 0) {
      this.loadPhotos();
    } else {
      this.http.get<any[]>(`https://localhost:5001/api/users/photos/by-tag/${this.filterTagIds[0]}`)
        .subscribe(p => this.photos = p);
    }
  }
}
