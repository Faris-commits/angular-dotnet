import {
  Component,
  OnInit,
  OnChanges,
  SimpleChanges,
  Input,
  Output,
  EventEmitter,
  inject
} from '@angular/core';
import {
  DecimalPipe,
  NgClass,
  NgFor,
  NgIf,
  NgStyle,
  AsyncPipe,
  CommonModule
} from '@angular/common';
import { FileUploadModule, FileUploader } from 'ng2-file-upload';
import { environment } from '../../../environments/environment';
import { Member } from '../../_models/member';
import { Photo } from '../../_models/photo';
import { MembersService } from '../../_services/members.service';
import { PhotoTagSelectorComponent } from '../../photo-tags/photo-tag-selector/photo-tag-selector.component';
import { PhotoTagDto } from '../../tags/tag.service';
import { FormsModule } from '@angular/forms';
import { AuthStoreService } from '../../_services/AuthStoreService';
import { BehaviorSubject, combineLatest, Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Component({
  selector: 'app-photo-editor',
  standalone: true,
  imports: [
    NgIf, NgFor, NgStyle, NgClass, FileUploadModule, DecimalPipe,
    PhotoTagSelectorComponent, FormsModule, AsyncPipe, CommonModule
  ],
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css'],
})
export class PhotoEditorComponent implements OnInit, OnChanges {
  private authStore = inject(AuthStoreService);
  private memberService = inject(MembersService);

  @Input({ required: true }) member!: Member;
  @Output() memberChange = new EventEmitter<Member>();

  uploader?: FileUploader;
  hasBaseDropZoneOver = false;
  baseUrl = environment.apiUrl;

  allTags: PhotoTagDto[] = [];

  assignTagIds: number[] = [];
  filterTagIds: number[] = [];

  assignTagIds$ = new BehaviorSubject<number[]>([]);
  filterTagIds$ = new BehaviorSubject<number[]>([]);
  approvalStatus$ = new BehaviorSubject<boolean | null>(null);
  private photos$ = new BehaviorSubject<Photo[]>([]);

  filteredPhotos$: Observable<Photo[]> = combineLatest([
    this.photos$,
    this.filterTagIds$,
    this.approvalStatus$
  ]).pipe(
    map(([photos, tagIds, approval]) =>
      photos.filter(photo => {
        const matchesTags = tagIds.length === 0 || (photo.tags ?? []).some(tag => tagIds.includes(tag.id));
        const matchesApproval = approval === null || photo.isApproved === approval;
        return matchesTags && matchesApproval;
      })
    )
  );

  ngOnInit(): void {
    this.loadTags();
    this.initializeUploader();
    this.photos$.next(this.member.photos ?? []);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['member'] && this.member) {
      this.photos$.next(this.member.photos ?? []);
    }
  }

  onFilterTagChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    const values = Array.from(select.selectedOptions).map(option => +option.value);
    this.filterTagIds = values;
    this.filterTagIds$.next(values);
  }

  setSelectedTagIds(ids: number[]) {
    this.assignTagIds = ids;
    this.assignTagIds$.next(ids);
  }

  setApprovalStatusFromEvent(event: Event) {
    const select = event.target as HTMLSelectElement;
    const value = select.value;
    this.setApprovalStatus(value === '' ? null : value === 'true');
  }

  setApprovalStatus(status: boolean | null) {
    this.approvalStatus$.next(status);
  }

  loadTags() {
    this.memberService.getPhotoTags().subscribe(tags => {
      this.allTags = tags;
    });
  }

  fileOverBase(e: any) {
    this.hasBaseDropZoneOver = e;
  }

  deleteTag(tag: PhotoTagDto) {
    this.memberService.deleteTag(tag.id).subscribe({
      next: () => {
        this.allTags = this.allTags.filter(t => t.id !== tag.id);
        this.assignTagIds = this.assignTagIds.filter(id => id !== tag.id);
        this.filterTagIds = this.filterTagIds.filter(id => id !== tag.id);
        this.assignTagIds$.next(this.assignTagIds);
        this.filterTagIds$.next(this.filterTagIds);
        this.member.photos.forEach(photo => {
          if (photo.tags) photo.tags = photo.tags.filter(t => t.id !== tag.id);
        });
      }
    });
  }

  deletePhoto(photo: Photo) {
    this.memberService.deletePhoto(photo).subscribe({
      next: _ => {
        const updatedMember = { ...this.member };
        updatedMember.photos = updatedMember.photos.filter(x => x.id !== photo.id);
        this.memberChange.emit(updatedMember);
      }
    });
  }

  setMainPhoto(photo: Photo) {
    this.memberService.setMainPhoto(photo).subscribe({
      next: _ => {
        const user = this.authStore.currentUserValue;
        if (user) {
          user.photoUrl = photo.url;
          this.authStore.setCurrentUser(user);
        }
        const updatedMember = { ...this.member };
        updatedMember.photoUrl = photo.url;
        updatedMember.photos.forEach(p => {
          p.isMain = p.id === photo.id;
        });
        this.memberChange.emit(updatedMember);
      }
    });
  }

  assignTagsToPhoto(photo: Photo) {
    const existingTagIds = (photo.tags ?? []).map(t => t.id);
    const duplicateTags = this.assignTagIds.filter(tagId => existingTagIds.includes(tagId));
    if (duplicateTags.length > 0) {
      alert('You cannot assign duplicate tags to the photo.');
      return;
    }

    this.memberService.setPhotoTags(photo.id, [...existingTagIds, ...this.assignTagIds]).subscribe({
      next: () => {
        const newTags = this.allTags.filter(tag =>
          this.assignTagIds.includes(tag.id) && !existingTagIds.includes(tag.id)
        );
        photo.tags = [...(photo.tags ?? []), ...newTags];
        alert('Tags assigned successfully!');
      }
    });
  }

  removeTagFromPhoto(photo: Photo, tag: PhotoTagDto) {
    const newTagIds = (photo.tags ?? []).filter(t => t.id !== tag.id).map(t => t.id);
    this.memberService.setPhotoTags(photo.id, newTagIds).subscribe(() => {
      photo.tags = (photo.tags ?? []).filter(t => t.id !== tag.id);
    });
  }

  getSelectedTagObjects() {
    return this.allTags.filter(tag => this.assignTagIds.includes(tag.id));
  }

  initializeUploader() {
    const user = this.authStore.currentUserValue;
    this.uploader = new FileUploader({
      url: this.baseUrl + 'users/add-photo',
      authToken: user ? 'Bearer ' + user.token : '',
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024,
    });

    this.uploader.onAfterAddingFile = file => {
      file.withCredentials = false;
    };

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      const photo = JSON.parse(response);
      const updatedMember = { ...this.member };
      updatedMember.photos.push(photo);
      this.memberChange.emit(updatedMember);

      if (photo.id && this.assignTagIds.length > 0) {
        this.memberService.setPhotoTags(photo.id, this.assignTagIds).subscribe({
          next: () => {
            const addedPhoto = updatedMember.photos.find(p => p.id === photo.id);
            if (addedPhoto) {
              addedPhoto.tags = this.getSelectedTagObjects();
            }
            this.memberChange.emit(updatedMember);
            this.assignTagIds = [];
            this.assignTagIds$.next([]);
          }
        });
      } else {
        this.assignTagIds = [];
        this.assignTagIds$.next([]);
      }

      if (photo.isMain) {
        const user = this.authStore.currentUserValue;
        if (user) {
          user.photoUrl = photo.url;
          this.authStore.setCurrentUser(user);
        }
        updatedMember.photoUrl = photo.url;
        updatedMember.photos.forEach(p => {
          p.isMain = p.id === photo.id;
        });
        this.memberChange.emit(updatedMember);
      }
    };
  }

  get selectedTagIdsSafe(): number[] {
    return this.assignTagIds$?.value ?? [];
  }

  get selectedTagIds(): number[] {
    return this.assignTagIds;
  }
}
