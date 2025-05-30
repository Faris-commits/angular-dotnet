import { Component, OnInit, inject, input, output } from '@angular/core';
import { Member } from '../../_models/member';
import { DecimalPipe, NgClass, NgFor, NgIf, NgStyle } from '@angular/common';
import { FileUploadModule, FileUploader } from 'ng2-file-upload';
import { AccountService } from '../../_services/account.service';
import { environment } from '../../../environments/environment';
import { Photo } from '../../_models/photo';
import { MembersService } from '../../_services/members.service';
import { PhotoTagSelectorComponent } from '../../photo-tags/photo-tag-selector/photo-tag-selector.component';
import { PhotoTagDto } from '../../tags/tag.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-photo-editor',
  standalone: true,
  imports: [NgIf, NgFor, NgStyle, NgClass, FileUploadModule, DecimalPipe, PhotoTagSelectorComponent, FormsModule],
  templateUrl: './photo-editor.component.html',
  styleUrl: './photo-editor.component.css',
})
export class PhotoEditorComponent implements OnInit {
  private accountService = inject(AccountService);
  private memberService = inject(MembersService);
  
  member = input.required<Member>();
  uploader?: FileUploader;
  hasBaseDropZoneOver = false;
  baseUrl = environment.apiUrl;
  memberChange = output<Member>();
  selectedTagIds: number[] = [];
  allTags: PhotoTagDto[] = [];
  newTagName = '';

  ngOnInit(): void {
    this.loadTags();
    this.initializeUploader();
  }

  loadTags() {
    this.memberService.getPhotoTags().subscribe(tags => {
      this.allTags = tags;
    });
  }

  addTag() {
    const name = this.newTagName.trim();
    if (!name) return;
    this.memberService.createTag({ name }).subscribe({
      next: tag => {
        this.allTags.push(tag);
        this.selectedTagIds.push(tag.id);
        this.newTagName = '';
      }
    });
  }

  

deleteTag(tag: PhotoTagDto) {
  this.memberService.deleteTag(tag.id).subscribe({
    next: () => {
      this.allTags = this.allTags.filter(t => t.id !== tag.id);
      this.selectedTagIds = this.selectedTagIds.filter(id => id !== tag.id);
      this.member().photos.forEach(photo => {
        if (photo.tags) photo.tags = photo.tags.filter(t => t.id !== tag.id);
      });
    },
    error: err => {
     
      console.error('Failed to delete tag', err);
    }
  });
}

 onTagCheckboxChange(event: Event, tagId: number) {
  const checked = (event.target as HTMLInputElement).checked;
  if (checked) {
    if (!this.selectedTagIds.includes(tagId)) {
      this.selectedTagIds = [...this.selectedTagIds, tagId];
    }
  } else {
    this.selectedTagIds = this.selectedTagIds.filter(id => id !== tagId);
  }
}

  fileOverBase(e: any) {
    this.hasBaseDropZoneOver = e;
  }

  deletePhoto(photo: Photo) {
    this.memberService.deletePhoto(photo).subscribe({
      next: _ => {
        const updatedMember = { ...this.member() };
        updatedMember.photos = updatedMember.photos.filter(
          x => x.id !== photo.id
        );
        this.memberChange.emit(updatedMember);
      },
    });
  }

  setMainPhoto(photo: Photo) {
    this.memberService.setMainPhoto(photo).subscribe({
      next: _ => {
        const user = this.accountService.currentUser();
        if (user) {
          user.photoUrl = photo.url;
          this.accountService.setCurrentUser(user);
        }
        const updatedMember = { ...this.member() };
        updatedMember.photoUrl = photo.url;
        updatedMember.photos.forEach(p => {
          if (p.isMain) p.isMain = false;
          if (p.id === photo.id) p.isMain = true;
        });
        this.memberChange.emit(updatedMember);
      },
    });
  }

  getTagNames(photo: Photo): string {
    return photo.tags && photo.tags.length
      ? photo.tags.map(t => t.name).join(', ')
      : 'None';
  }

 assignTagsToPhoto(photo: Photo) {
  const existingTagIds = (photo.tags ?? []).map(t => t.id);

  // Check for duplicates
  const duplicateTags = this.selectedTagIds.filter(tagId => existingTagIds.includes(tagId));
  if (duplicateTags.length > 0) {
    alert('You cannot assign duplicate tags to the photo.');
    return;
  }

  this.memberService.setPhotoTags(photo.id, this.selectedTagIds).subscribe({
    next: () => {
      const newTags = this.allTags.filter(tag => this.selectedTagIds.includes(tag.id) && !existingTagIds.includes(tag.id));
      photo.tags = [...(photo.tags ?? []), ...newTags];
      alert('Tags assigned successfully!');
    },
    error: err => {
      console.error('Failed to assign tags', err);
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
    return this.allTags.filter(tag => this.selectedTagIds.includes(tag.id));
  }

  initializeUploader() {
    this.uploader = new FileUploader({
      url: this.baseUrl + 'users/add-photo',
      authToken: 'Bearer ' + this.accountService.currentUser()?.token,
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
      const updatedMember = { ...this.member() };
      updatedMember.photos.push(photo);
      this.memberChange.emit(updatedMember);

      if (photo.id && this.selectedTagIds.length > 0) {
        this.memberService.setPhotoTags(photo.id, this.selectedTagIds).subscribe({
          next: () => {
            const addedPhoto = updatedMember.photos.find(p => p.id === photo.id);
            if (addedPhoto) {
              addedPhoto.tags = this.getSelectedTagObjects();
            }
            this.memberChange.emit(updatedMember);
            this.selectedTagIds = [];
          }
        });
      } else {
        this.selectedTagIds = [];
      }

      if (photo.isMain) {
        const user = this.accountService.currentUser();
        if (user) {
          user.photoUrl = photo.url;
          this.accountService.setCurrentUser(user);
        }
        updatedMember.photoUrl = photo.url;
        updatedMember.photos.forEach(p => {
          if (p.isMain) p.isMain = false;
          if (p.id === photo.id) p.isMain = true;
        });
        this.memberChange.emit(updatedMember);
      }
    };
  }
}