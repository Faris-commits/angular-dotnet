import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { PhotoTagSelectorComponent } from "../../photo-tags/photo-tag-selector/photo-tag-selector.component";

@Component({
  selector: 'app-photo-upload',
  standalone: true,
  imports: [PhotoTagSelectorComponent],
  templateUrl: './photo-upload.component.html',
  styleUrl: './photo-upload.component.css'
})
export class PhotoUploadComponent {
  selectedFile: File | null = null;
  selectedTagIds: number[] = [];

  constructor(private http: HttpClient) {}

 onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
  }

  uploadPhoto() {
    if (!this.selectedFile) return;
    const formData = new FormData();
    formData.append('file', this.selectedFile);

    this.http.post<any>('https://localhost:5001/api/users/add-photo', formData)
      .subscribe(photo => {
        if (photo && photo.id && this.selectedTagIds.length > 0) {
          this.http.post(
            `https://localhost:5001/api/users/photos/${photo.id}/tags`,
            this.selectedTagIds
          ).subscribe();
        }
      });
  }
}
