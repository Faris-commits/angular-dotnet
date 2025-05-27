import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { User } from '../_models/user';
import { Photo } from '../_models/photo';
import { PhotoTagDto } from '../tags/tag.service';

@Injectable({
  providedIn: 'root',
})
export class AdminService {
  baseUrl = environment.apiUrl;
  private http = inject(HttpClient);

  getUserWithRoles() {
    return this.http.get<User[]>(this.baseUrl + 'admin/users-with-roles');
  }

  updateUserRoles(username: string, roles: string[]) {
    return this.http.post<string[]>(
      this.baseUrl + 'admin/edit-roles/' + username + '?roles=' + roles,
      {}
    );
  }

  getPhotosForApproval() {
    return this.http.get<Photo[]>(this.baseUrl + 'admin/photos-to-moderate');
  }

  approvePhoto(photoId: number) {
    return this.http.post(this.baseUrl + 'admin/approve-photo/' + photoId, {});
  }

  rejectPhoto(photoId: number) {
    return this.http.post(this.baseUrl + 'admin/reject-photo/' + photoId, {});
  }
  getAllTags() {
  return this.http.get<PhotoTagDto[]>(this.baseUrl + 'admin/photo-tags');
}

addTagToPhoto(photoId: number, tagId: number) {
  return this.http.post<PhotoTagDto>(this.baseUrl + `admin/photos/${photoId}/tags/${tagId}`, {});
}

removeTagFromPhoto(photoId: number, tagId: number) {
  return this.http.delete(this.baseUrl + `admin/photos/${photoId}/tags/${tagId}`);
}

createTag(tag: { name: string }) {
  return this.http.post<PhotoTagDto>(this.baseUrl + 'admin/photo-tags', tag);
}
}
