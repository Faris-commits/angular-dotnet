import { Component, OnInit } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { Photo } from '../../_models/photo';
import { PhotoTagDto } from '../../tags/tag.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-photo-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './photo-management.component.html',
  styleUrl: './photo-management.component.css',
})
export class PhotoManagementComponent implements OnInit {
  photos: Photo[] = [];
  allTags: PhotoTagDto[] = [];
  selectedTagId: number | null = null;
  newTagName = '';

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.getPhotosForApproval();
    this.getAllTags();
  }

getPhotosForApproval() {
  this.adminService.getPhotosForApproval().subscribe({
    next: photos => {
      this.photos = Array.isArray(photos) ? photos : [];
      console.log('Loaded photos:', this.photos); 
    },
    error: () => this.photos = []
  });
}

  getAllTags() {
    this.adminService.getAllTags().subscribe({
      next: tags => this.allTags = tags || [],
      error: () => this.allTags = []
    });
  }

  createTag() {
    const name = this.newTagName.trim();
    if (!name) return;
    this.adminService.createTag({ name }).subscribe({
      next: tag => {
        this.allTags.push(tag);
        this.newTagName = '';
      }
    });
  }

  approvePhoto(photoId: number) {
    this.adminService.approvePhoto(photoId).subscribe({
      next: () => {
        this.photos = this.photos.filter(photo => photo.id !== photoId);
      }
    });
  }

  rejectPhoto(photoId: number) {
    this.adminService.rejectPhoto(photoId).subscribe({
      next: () => {
        this.photos = this.photos.filter(photo => photo.id !== photoId);
      }
    });
  }

  addTagToPhoto(photo: Photo, tagId: string) {
    const tagIdNum = +tagId;
    if (!(photo.tags ?? []).some(t => t.id === tagIdNum)) {
      this.adminService.addTagToPhoto(photo.id, tagIdNum).subscribe({
        next: tag => {
          if (tag) {
            photo.tags = [...(photo.tags ?? []), tag];
          }
        }
      });
    }
  }

  removeTagFromPhoto(photo: Photo, tagId: number) {
    this.adminService.removeTagFromPhoto(photo.id, tagId).subscribe({
      next: () => {
        photo.tags = (photo.tags ?? []).filter(t => t.id !== tagId);
      }
    });
  }

setFilterTag(tagId: string | null) {

  this.selectedTagId = tagId && tagId !== 'null' ? +tagId : null;
}

get filteredPhotos(): Photo[] {
  if (!this.selectedTagId) return this.photos;
  return this.photos.filter(photo =>
    (photo.tags ?? []).some(t => t.id === this.selectedTagId)
  );
}

  isTagAssigned(photo: Photo, tagId: number): boolean {
    return (photo.tags ?? []).some(t => t.id === tagId);
  }

  trackByPhotoId(index: number, photo: Photo) {
    return photo.id;
  }
}