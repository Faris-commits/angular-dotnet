import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { User } from '../_models/user';
import { Photo } from '../_models/photo';
import { PhotoTagDto } from '../tags/tag.service';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class AdminService {
  baseUrl = environment.apiUrl;
  private http = inject(HttpClient);

  getUserWithRoles(): Observable<User[]> {
    return this.http.get<User[]>(this.baseUrl + 'admin/users-with-roles');
  }

  updateUserRoles(username: string, roles: string[]): Observable<string[]> {
    return this.http.post<string[]>(
      this.baseUrl + 'admin/edit-roles/' + username + '?roles=' + roles,
      {}
    );
  }

  getPhotosForApproval(): Observable<Photo[]> {
    return this.http.get<Photo[]>(this.baseUrl + 'admin/photos-to-moderate');
  }

  approvePhoto(photoId: number): Observable<void> {
    return this.http.post<void>(this.baseUrl + 'admin/approve-photo/' + photoId, {});
  }

  rejectPhoto(photoId: number, reason: string) {
  return this.http.post(
    this.baseUrl + 'admin/reject-photo/' + photoId,
    { reason }
  );
}

  getAllTags(): Observable<PhotoTagDto[]> {
    return this.http.get<PhotoTagDto[]>(this.baseUrl + 'admin/photo-tags');
  }

  addTagToPhoto(photoId: number, tagId: number): Observable<PhotoTagDto> {
    return this.http.post<PhotoTagDto>(this.baseUrl + `admin/photos/${photoId}/tags/${tagId}`, {});
  }

  removeTagFromPhoto(photoId: number, tagId: number): Observable<void> {
    return this.http.delete<void>(this.baseUrl + `admin/photos/${photoId}/tags/${tagId}`);
  }

  createTag(tag: { name: string }): Observable<PhotoTagDto> {
    return this.http.post<PhotoTagDto>(this.baseUrl + 'admin/photo-tags', tag);
  }

  getPhotoApprovalStats(): Observable<any[]> {
    return this.http.get<any[]>(this.baseUrl + 'admin/photo-approval-stats').pipe(
      catchError((error) => {
        console.error('Error fetching photo approval stats:', error);
        return throwError(() => error);
      })
    );
  }
  getUsersWithoutMainPhoto(): Observable<string[]> {
  return this.http.get<string[]>(this.baseUrl + 'admin/users-without-main-photo').pipe(
    catchError((error) => {
      console.error('Error fetching users without main photo:', error);
      return throwError(() => error);
    })
  );
}
}