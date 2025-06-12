import { Component, OnInit } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { Photo } from '../../_models/photo';
import { PhotoTagDto } from '../../tags/tag.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BehaviorSubject, combineLatest, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { PhotoTagSelectorComponent } from '../../photo-tags/photo-tag-selector/photo-tag-selector.component';

@Component({
  selector: 'app-photo-management',
  standalone: true,
  imports: [CommonModule, FormsModule, PhotoTagSelectorComponent],
  templateUrl: './photo-management.component.html',
  styleUrl: './photo-management.component.css',
})
export class PhotoManagementComponent implements OnInit {
  photos: Photo[] = [];
  allTags: PhotoTagDto[] = [];
  newTagName = '';

  selectedTagIds$ = new BehaviorSubject<number[]>([]);
  approvalStatus$ = new BehaviorSubject<boolean | null>(null);
  private photos$ = new BehaviorSubject<Photo[]>([]);

  filteredPhotos$: Observable<Photo[]> = combineLatest([
    this.photos$,
    this.selectedTagIds$,
    this.approvalStatus$
  ]).pipe(
    map(([photos, tagIds, approval]) =>
      photos.filter(photo => {
        const matchesTags =
          tagIds.length === 0 ||
          (photo.tags ?? []).some(tag => tagIds.includes(tag.id));
        const matchesApproval =
          approval === null || photo.isApproved === approval;
        return matchesTags && matchesApproval;
      })
    )
  );

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.getPhotosForApproval();
    this.getAllTags();
  }

  getPhotosForApproval() {
    this.adminService.getPhotosForApproval().subscribe({
      next: photos => {
        this.photos = Array.isArray(photos) ? photos : [];
        this.photos$.next(this.photos);
      },
      error: () => {
        this.photos = [];
        this.photos$.next([]);
      }
    });
  }

  getAllTags() {
    this.adminService.getAllTags().subscribe({
      next: tags => this.allTags = tags || [],
      error: () => this.allTags = []
    });
  }

  setSelectedTagIds(tagIds: number[]) {
    this.selectedTagIds$.next(tagIds);
  }

  setApprovalStatusFromEvent(event: Event) {
    const select = event.target as HTMLSelectElement;
    const value = select.value;
    this.setApprovalStatus(value === '' ? null : value === 'true');
  }

  setApprovalStatus(status: boolean | null) {
    this.approvalStatus$.next(status);
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
        this.photos$.next(this.photos);
      }
    });
  }

  rejectPhoto(photoId: number) {
    const reason = prompt('Please enter a reason for rejection:');
    if (!reason || !reason.trim()) return;
    this.adminService.rejectPhoto(photoId, reason).subscribe({
      next: () => {
        this.photos = this.photos.filter(photo => photo.id !== photoId);
        this.photos$.next(this.photos);
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

  isTagAssigned(photo: Photo, tagId: number): boolean {
    return (photo.tags ?? []).some(t => t.id === tagId);
  }

  trackByPhotoId(index: number, photo: Photo) {
    return photo.id;
  }

  get selectedTagIdsSafe(): number[] {
    return this.selectedTagIds$.value ?? [];
  }
}